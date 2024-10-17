using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("DeleteStudent", "")]
    public class DeleteStudentController : BaseController
    {
        public DeleteStudentController(ApplicationDbContext db,
                                       IFlashMessage flashMessage,
                                       ISelectListProvider selectListProvider,
                                       IMapper mapper) : base(db, flashMessage, mapper, selectListProvider) { }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var models = _db.Students.AsNoTracking()
                                     .Include(x => x.AcademicInformation)
                                         .ThenInclude(x => x.AcademicLevel)
                                     .Include(x => x.AdmissionInformation)
                                         .ThenInclude(x => x.AdmissionRound)
                                         .ThenInclude(x => x.AdmissionTerm)
                                     .Include(x => x.AcademicInformation)
                                         .ThenInclude(x => x.Faculty)
                                     .Include(x => x.AcademicInformation)
                                         .ThenInclude(x => x.Department)
                                     .Include(x => x.Nationality)
                                     .Where(x => (criteria.AcademicLevelId == 0
                                                  || x.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId)
                                                  && (criteria.AdmissionRoundId == 0
                                                      || x.AdmissionInformation.AdmissionRoundId == criteria.AdmissionRoundId)
                                                  && (criteria.FacultyId == 0
                                                      || x.AcademicInformation.FacultyId == criteria.FacultyId)
                                                  && (criteria.DepartmentId == 0
                                                      || x.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                  && (criteria.StudentCodeFrom == null
                                                      || (criteria.StudentCodeTo == null ? x.CodeInt == criteria.StudentCodeFrom : x.CodeInt >= criteria.StudentCodeFrom))
                                                  && (criteria.StudentCodeTo == null
                                                      || (criteria.StudentCodeFrom == null ? x.CodeInt == criteria.StudentCodeTo : x.CodeInt <= criteria.StudentCodeTo))
                                                  && (string.IsNullOrEmpty(criteria.FirstName)
                                                      || x.FirstNameEn.ToLower().StartsWith(criteria.FirstName.ToLower())
                                                      || x.FirstNameTh.StartsWith(criteria.FirstName))
                                                  && (string.IsNullOrEmpty(criteria.LastName)
                                                      || x.LastNameEn.ToLower().StartsWith(criteria.LastName.ToLower())
                                                      || x.LastNameTh.StartsWith(criteria.LastName))
                                                  && (criteria.NationalityId == 0
                                                      || x.NationalityId == criteria.NationalityId)
                                                  && x.StudentStatus == "d")
                                     .IgnoreQueryFilters()
                                     .GetPaged(criteria, page, true);

            return View(models);
        }

        [PermissionAuthorize("DeleteStudent", PolicyGenerator.Write)]
        public ActionResult Refresh(Guid id)
        {
            Student model = Find(id);
            try
            {
                model.StudentStatus = "s";
                model.IsActive = StudentProvider.IsActiveFromStudentStatus(model.StudentStatus);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToSave);
            }

            return RedirectToAction(nameof(Index), new Criteria
                                                   {
                                                       AcademicLevelId = model.AcademicInformation.AcademicLevelId,
                                                       StudentCodeFrom = model.CodeInt,
                                                       StudentCodeTo = model.CodeInt
                                                   });
        }

        public ActionResult Delete(Criteria criteria, string returnUrl)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            ViewBag.ReturnUrl = returnUrl;
            if (criteria.AcademicLevelId == 0 || criteria.StudentCodeFrom == null || criteria.StudentCodeTo == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var students = _db.Students.Where(x => x.StudentStatus != "d"
                                                   && x.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                   && x.CodeInt >= criteria.StudentCodeFrom
                                                   && x.CodeInt <= criteria.StudentCodeTo
                                                   && (criteria.AdmissionRoundId == 0
                                                       || x.AdmissionInformation.AdmissionRoundId == criteria.AdmissionRoundId)
                                                   && (criteria.FacultyId == 0
                                                       || x.AcademicInformation.FacultyId == criteria.FacultyId)
                                                   && (criteria.DepartmentId == 0
                                                       || x.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                   && (string.IsNullOrEmpty(criteria.Status) 
                                                       || x.StudentStatus == criteria.Status)
                                                   && (criteria.NationalityId == 0 
                                                       || x.NationalityId == criteria.NationalityId)
                                                   && (string.IsNullOrEmpty(criteria.FirstName) 
                                                       || x.FirstNameEn.ToLower().Contains(criteria.FirstName.ToLower())
                                                       || (x.FirstNameTh ?? string.Empty).Contains(criteria.FirstName))
                                                   && (string.IsNullOrEmpty(criteria.LastName) 
                                                       || x.LastNameEn.ToLower().Contains(criteria.LastName.ToLower())
                                                       || (x.LastNameTh ?? string.Empty).Contains(criteria.LastName))
                                                   && (criteria.NonRegistration ? (x.RegistrationCourses == null && !x.RegistrationCourses.Any())
                                                                                : (x.RegistrationCourses != null && x.RegistrationCourses.Any())))
                                       .Select(x => new SearchDeleteStudentViewModel
                                                    {
                                                        AcademicLevelId = x.AcademicInformation.AcademicLevelId,
                                                        FacultyId = x.AcademicInformation.FacultyId,
                                                        DepartmentId = x.AcademicInformation.DepartmentId ?? 0,
                                                        Faculty = x.AcademicInformation.Faculty.NameEn,
                                                        Department = x.AcademicInformation.Department.NameEn,
                                                        StudentId = x.Id,
                                                        Code = x.Code,
                                                        Title = x.Title.NameEn,
                                                        FirstName = x.FirstNameEn,
                                                        MidName = x.MidNameEn,
                                                        LastName = x.LastNameEn,
                                                        StudentStatus = x.StudentStatus,
                                                        IsChecked = "on"
                                                    })
                                       .OrderBy(x => x.Code)
                                       .ToList();

            var model = new DeleteStudentViewModel
                        {
                            Criteria = criteria,
                            SearchDeleteStudents = students
                        };

            return View(model);
        }

        [PermissionAuthorize("DeleteStudent", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(DeleteStudentViewModel model, string returnUrl)
        {
            var selectedStudents = model.SearchDeleteStudents.Where(x => x.IsChecked == "on")
                                                             .Select(x => x.StudentId)
                                                             .ToList();

            var studentToUpdate = _db.Students.Where(x => selectedStudents.Contains(x.Id))
                                              .ToList();

            try
            {
                studentToUpdate.Select(x => {
                                                x.StudentStatus = "d";
                                                x.IsActive = StudentProvider.IsActiveFromStudentStatus("d");
                                                return x;
                                            })
                               .ToList();

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return Redirect(returnUrl);
            }
            catch
            {
                ViewBag.ReturnUrl = returnUrl;
                CreateSelectList(model.Criteria.AcademicLevelId, model.Criteria.FacultyId);
                _flashMessage.Danger(Message.UnableToSave);
                return View(model);
            }
        }

        private Student Find(Guid id) 
        {
            var model = _db.Students.Include(x => x.AcademicInformation)
                                    .IgnoreQueryFilters()
                                    .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.StudentStatus = _selectListProvider.GetStudentStatuses();
            ViewBag.Nationalities = _selectListProvider.GetNationalities();
            if (academicLevelId > 0)
            {
                ViewBag.AdmissionRounds = _selectListProvider.GetAdmissionRoundByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }

            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            }
        }
    }
}