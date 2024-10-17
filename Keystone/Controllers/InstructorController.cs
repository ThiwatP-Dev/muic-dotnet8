using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using Newtonsoft.Json;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("Instructor", "")]
    public class InstructorController : BaseController
    {
        protected readonly IInstructorProvider _instructorProvider;

        public InstructorController(ApplicationDbContext db,
                                    IFlashMessage flashMessage,
                                    ISelectListProvider selectListProvider,
                                    IInstructorProvider instructorProvider,
                                    IMapper mapper) : base(db, flashMessage, mapper, selectListProvider)
        {
            _instructorProvider = instructorProvider;
        }

        public ActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.FacultyId);
            var instructors = _db.Instructors.Include(x => x.InstructorWorkStatus)
                                                .ThenInclude(x => x.Faculty)
                                             .Include(x => x.InstructorWorkStatus)
                                                .ThenInclude(x => x.Department)
                                             .Include(x => x.InstructorWorkStatus)
                                                .ThenInclude(x => x.InstructorType)
                                             .Include(x => x.Title)
                                             .IgnoreQueryFilters()
                                             .Where(x => ((string.IsNullOrEmpty(criteria.Code) || x.Code.Contains(criteria.Code, StringComparison.OrdinalIgnoreCase))
                                                           && (string.IsNullOrEmpty(criteria.FirstName)
                                                               || (!string.IsNullOrEmpty(x.FirstNameEn)
                                                                   && (x.FirstNameEn.Contains(criteria.FirstName, StringComparison.OrdinalIgnoreCase)))
                                                               || (!string.IsNullOrEmpty(x.FirstNameTh) && x.FirstNameTh.Contains(criteria.FirstName, StringComparison.OrdinalIgnoreCase))
                                                              )
                                                           && (string.IsNullOrEmpty(criteria.LastName)
                                                               || (!string.IsNullOrEmpty(x.LastNameEn)
                                                                   && (x.LastNameEn.Contains(criteria.LastName, StringComparison.OrdinalIgnoreCase)))
                                                               || (!string.IsNullOrEmpty(x.LastNameTh) && x.LastNameTh.Contains(criteria.LastName, StringComparison.OrdinalIgnoreCase))
                                                              )
                                                           && (criteria.FacultyId == 0
                                                               || x.InstructorWorkStatus.Faculty.Id == criteria.FacultyId)
                                                           && (criteria.DepartmentId == 0
                                                               || x.InstructorWorkStatus.Department.Id == criteria.DepartmentId)
                                                           && (criteria.Status == "All"
                                                               || string.IsNullOrEmpty(criteria.Status)
                                                               || x.IsActive == Convert.ToBoolean(criteria.Status))
                                                           && (criteria.InstructorTypeId == 0
                                                               || x.InstructorWorkStatus.InstructorTypeId == criteria.InstructorTypeId)))
                                             .Select(x => _mapper.Map<Instructor, InstructorViewModel>(x))
                                             .GetPaged(criteria, page);
            if (instructors.Results != null && instructors.Results.Any())
            {
                var emailInstructors = _instructorProvider.GetInstructors(criteria).Select(x => x.Email).ToList();
                instructors.Criteria.InstructorEmails = string.Join(";", emailInstructors);
            }


            return View(instructors);
        }

        public ActionResult DetailsSearch(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                _flashMessage.Warning(Message.RequiredData);
                return RedirectToAction(nameof(Details));
            }

            if (!_instructorProvider.IsExistInstructor(code))
            {
                _flashMessage.Danger(Message.DataNotFound);
                return RedirectToAction(nameof(Details));
            }

            var instructor = _instructorProvider.GetInstructorByCode(code);
            return RedirectToAction(nameof(Details), new { id = (instructor == null ? 0 : instructor.Id) });
        }

        public ActionResult Details(long id, string returnUrl)
        {
            ViewBag.DetailsPage = true;
            ViewBag.ReturnUrl = returnUrl;
            var instructor = _instructorProvider.GetInstructorProfile(id);
            if (instructor != null)
            {
                return View(instructor);
            }

            return View();
        }

        [PermissionAuthorize("Instructor", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new InstructorInfoViewModel());
        }

        [PermissionAuthorize("Instructor", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(InstructorInfoViewModel model, string returnUrl)
        {
            if (string.IsNullOrEmpty(model.Code) || model.TitleId == 0 || string.IsNullOrEmpty(model.FirstNameEn)
                || string.IsNullOrEmpty(model.LastNameEn) || model.Gender == 0 && model.NationalityId == 0
                || model.CountryId == 0 || model.TypeId == 0 || model.RankId == 0)
            {
                CreateSelectList(model.FacultyId ?? 0);
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }
            model.Code = model.Code.TrimEnd();
            if (_instructorProvider.IsExistInstructor(model.Code))
            {
                CreateSelectList(model.FacultyId ?? 0);
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var instructor = new Instructor();
                    var mappedInstructor = _mapper.Map<InstructorInfoViewModel, Instructor>(model, instructor);
                    _db.Instructors.Add(instructor);

                    var instructorWorkStatus = new InstructorWorkStatus();
                    var mappedInstructorWorkStatus = _mapper.Map<InstructorInfoViewModel, InstructorWorkStatus>(model, instructorWorkStatus);
                    mappedInstructorWorkStatus.InstructorId = mappedInstructor.Id;
                    _db.InstructorWorkStatuses.Add(mappedInstructorWorkStatus);

                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { Code = model.Code });
                }
                catch
                {
                    CreateSelectList(model.FacultyId ?? 0);
                    ViewBag.ReturnUrl = returnUrl;
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var instructor = _instructorProvider.GetInstructor(id);
            var model = _mapper.Map<Instructor, InstructorInfoViewModel>(instructor);
            CreateSelectList(model.FacultyId ?? 0);
            return View(model);
        }

        [PermissionAuthorize("Instructor", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(InstructorInfoViewModel model, string returnUrl)
        {
            if (string.IsNullOrEmpty(model.Code) || model.TitleId == 0 || string.IsNullOrEmpty(model.FirstNameEn)
                || string.IsNullOrEmpty(model.LastNameEn) || model.Gender == 0 && model.NationalityId == 0
                || model.CountryId == 0 || model.TypeId == 0 || model.RankId == 0)
            {
                CreateSelectList(model.FacultyId ?? 0);
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            var modelToUpdate = _instructorProvider.GetInstructor(model.Id);
            model.Code = model.Code.TrimEnd();
            if (modelToUpdate.InstructorWorkStatus == null)
            {
                modelToUpdate.InstructorWorkStatus = new InstructorWorkStatus();
            }

            if (model.Code != modelToUpdate.Code && _instructorProvider.IsExistInstructor(model.Code))
            {
                CreateSelectList(model.FacultyId ?? 0);
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _mapper.Map<InstructorInfoViewModel, Instructor>(model, modelToUpdate);
                    _mapper.Map<InstructorInfoViewModel, InstructorWorkStatus>(model, modelToUpdate.InstructorWorkStatus);

                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { Code = model.Code });
                }
                catch
                {
                    CreateSelectList(model.FacultyId ?? 0);
                    ViewBag.ReturnUrl = returnUrl;
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(model);
                }
            }
        }

        [PermissionAuthorize("Instructor", PolicyGenerator.Write)]
        public IActionResult Delete(long id)
        {
            var model = _instructorProvider.GetInstructor(id);
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.Instructors.Remove(model);
                    _db.InstructorWorkStatuses.Remove(model.InstructorWorkStatus);
                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToDelete);
                    return RedirectToAction(nameof(Index));
                }
            }
        }
        [HttpPost]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult ExportExcel(Criteria criteria, string returnUrl)
        {
            var results = _instructorProvider.GetInstructors(criteria);
            if (results != null && results.Any())
            {
                using (var wb = GenerateWorkBook(results))
                {
                    return wb.Deliver($"Instructor Report.xlsx");
                }
            }

            return Redirect(returnUrl);
        }

        private XLWorkbook GenerateWorkBook(List<InstructorInfoViewModel> results)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "INSTRUCTOR ID";

            ws.Cell(row, column++).Value = "NAME EN";
            ws.Cell(row, column++).Value = "DIVISION";
            ws.Cell(row, column++).Value = "MAJOR";
            ws.Cell(row, column++).Value = "EMAIL";
            ws.Cell(row, column++).Value = "TYPE";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "STATUS";

            foreach (var item in results)
            {
                column = 1;
                row++;
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).SetValue<string>(item.Code);

                ws.Cell(row, column++).Value = item.FullNameEn;
                ws.Cell(row, column++).Value = item.Faculty;
                ws.Cell(row, column++).Value = item.Department;
                ws.Cell(row, column++).Value = item.Email;

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.Type;

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.IsActive ? "Active" : "Inactive";
            }

            ws.Columns().AdjustToContents();
            ws.Rows().AdjustToContents();
            return wb;
        }
        public ActionResult Setting()
        {
            ViewBag.Roles = _selectListProvider.GetRoles();
            InstructorSettingViewModel model = new InstructorSettingViewModel();

            // University Level
            var advisorRoleUniversityLevel = _configurationProvider.Get<string>("AdvisorRoleUniversityLevel");
            if (!string.IsNullOrEmpty(advisorRoleUniversityLevel))
            {
                var roles = JsonConvert.DeserializeObject<List<RoleViewModel>>(advisorRoleUniversityLevel);
                foreach (var role in roles)
                {
                    if (_db.Roles.Any(x => x.Id == role.Id))
                    {
                        role.Name = _db.Roles.Find(role.Id)?.Name ?? "";
                    }
                }

                model.UniversityRoles.AddRange(roles);
            }

            // Major Level
            var advisorRoleMajorLevel = _configurationProvider.Get<string>("AdvisorRoleMajorLevel");
            if (!string.IsNullOrEmpty(advisorRoleMajorLevel))
            {
                var roles = JsonConvert.DeserializeObject<List<RoleViewModel>>(advisorRoleMajorLevel);
                foreach (var role in roles)
                {
                    if (_db.Roles.Any(x => x.Id == role.Id))
                    {
                        role.Name = _db.Roles.Find(role.Id)?.Name ?? "";
                    }
                }

                model.MajorRoles.AddRange(roles);
            }
            return View(model);
        }

        [PermissionAuthorize("Instructor", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Setting(InstructorSettingViewModel model)
        {
            ViewBag.Roles = _selectListProvider.GetRoles();
            var rolesJson = "";

            // University Level
            if (model.UniversityRoles != null && model.UniversityRoles.Any(x => x.Id != null))
            {
                model.UniversityRoles = model.UniversityRoles.GroupBy(x => x.Id)
                                                             .Select(y => y.First()).Distinct().ToList();

                rolesJson = JsonConvert.SerializeObject(model.UniversityRoles);
            }

            var updateUniversityRoles = _configurationProvider.Update(rolesJson, "AdvisorRoleUniversityLevel");

            // Major Level
            if (model.MajorRoles != null && model.MajorRoles.Any(x => x.Id != null))
            {
                model.MajorRoles = model.MajorRoles.GroupBy(x => x.Id)
                                                   .Select(y => y.First()).Distinct().ToList();

                rolesJson = JsonConvert.SerializeObject(model.MajorRoles);
            }

            var updateMajorRoles = _configurationProvider.Update(rolesJson, "AdvisorRoleMajorLevel");

            if (updateUniversityRoles && updateMajorRoles)
            {
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            else
            {
                _flashMessage.Danger(Message.UnableToSave);
            }

            return View(model);
        }

        private void CreateSelectList(long facultyId = 0)
        {
            ViewBag.Titles = _selectListProvider.GetTitlesEn();
            ViewBag.Nationalities = _selectListProvider.GetNationalities();
            ViewBag.Races = _selectListProvider.GetRaces();
            ViewBag.Religions = _selectListProvider.GetReligions();
            ViewBag.Countries = _selectListProvider.GetCountries();
            ViewBag.Provinces = _selectListProvider.GetProvinces();
            ViewBag.States = _selectListProvider.GetStates();
            ViewBag.Cities = _selectListProvider.GetCities();
            ViewBag.Districts = _selectListProvider.GetDistricts();
            ViewBag.Subdistricts = _selectListProvider.GetSubdistricts();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Statuses = _selectListProvider.GetActiveStatuses();
            ViewBag.Faculties = _selectListProvider.GetFaculties();
            ViewBag.InstructorTypes = _selectListProvider.GetInstructorTypes();
            ViewBag.InstructorRankings = _selectListProvider.GetInstructorRankings();
            if (facultyId != 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartments(facultyId);
            }
        }

    }
}