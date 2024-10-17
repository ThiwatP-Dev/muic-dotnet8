using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("ResignStudent", "")]
    public class ResignStudentController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IScholarshipProvider _scholarshipProvider;
        protected readonly IRegistrationProvider _registrationProvider;

        public ResignStudentController(ApplicationDbContext db,
                                       IFlashMessage flashMessage,
                                       ICacheProvider cacheProvider,
                                       IStudentProvider studentProvider,
                                       ISelectListProvider selectListProvider,
                                       IMapper mapper,
                                       IScholarshipProvider scholarshipProvider,
                                       IRegistrationProvider registrationProvider) : base(db, flashMessage, mapper, selectListProvider) 
        { 
            _cacheProvider = cacheProvider;
            _studentProvider = studentProvider;
            _scholarshipProvider = scholarshipProvider;
            _registrationProvider = registrationProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);

            if (criteria.AcademicLevelId == 0)
            {
                 _flashMessage.Warning(Message.RequiredData);
                 return View();
            }

            var models = _db.ResignStudents.Where(x =>  criteria.AcademicLevelId == x.Student.AcademicInformation.AcademicLevelId
                                                        && (criteria.TermId == 0
                                                            || criteria.TermId == x.TermId)
                                                        && (string.IsNullOrEmpty(criteria.Code)
                                                            ||criteria.Code == x.Student.Code)
                                                        && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                            || x.Student.FirstNameEn.Contains(criteria.CodeAndName))
                                                        && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                            || x.Student.LastNameEn.Contains(criteria.CodeAndName))
                                                        && (criteria.FacultyId == 0
                                                            || criteria.FacultyId == x.Student.AcademicInformation.FacultyId)
                                                        && (criteria.DepartmentId == 0
                                                            || criteria.DepartmentId == x.Student.AcademicInformation.DepartmentId)
                                                        && (criteria.ResignReasonId == 0 
                                                            || criteria.ResignReasonId == x.ResignReasonId)
                                                        && (criteria.StartStudentBatch == null
                                                            || criteria.StartStudentBatch == 0
                                                            || criteria.StartStudentBatch <= x.Student.AcademicInformation.Batch)
                                                        && (criteria.EndStudentBatch == null
                                                            || criteria.EndStudentBatch == 0
                                                            || criteria.EndStudentBatch >= x.Student.AcademicInformation.Batch))
                                           .Select(x => new ResignStudentViewModel
                                                        {
                                                            StudentCode =  x.Student.Code,
                                                            TitleEn = x.Student.Title.NameEn,
                                                            FirstNameEn = x.Student.FirstNameEn,
                                                            MidNameEn = x.Student.MidNameEn,
                                                            LastNameEn = x.Student.LastNameEn,
                                                            FacultyCode = x.Student.AcademicInformation.Department.Faculty.Code,
                                                            FacultyNameEn = x.Student.AcademicInformation.Department.Faculty.NameEn,
                                                            DepartmentCode = x.Student.AcademicInformation.Department.Code,
                                                            DepartmentNameEn = x.Student.AcademicInformation.Department.NameEn,
                                                            ResignReason = x.ResignReason.DescriptionEn,
                                                            Remark = x.Remark,
                                                            EffectiveTerm = x.EffectiveTerm == null ? "" : x.EffectiveTerm.AcademicTerm + "/" + x.EffectiveTerm.AcademicYear,
                                                            ApprovedAtText = x.ApprovedAtText
                                                        })
                                           .IgnoreQueryFilters()
                                           .GetPaged(criteria, page);
            return View(models);
        }

        [PermissionAuthorize("ResignStudent", PolicyGenerator.Write)]
        public IActionResult Create(ResignStudentViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var resignStudent = new ResignStudentViewModel();
            if (_studentProvider.IsExistStudent(model.StudentCode, "rs"))
            {
                _flashMessage.Danger(Message.DataAlreadyExist);
            }
            else
            {
                var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);
                if (student != null)
                {
                    CreateSelectList(student.AcademicInformation?.AcademicLevelId ?? 0);
                    var currentTerm = _cacheProvider.GetCurrentTerm(student.AcademicInformation.AcademicLevelId);
                    resignStudent = _mapper.Map<Student, ResignStudentViewModel>(student);
                    resignStudent.ScholarshipStudents = _scholarshipProvider.GetScholarshipStudents(student.Id);
                    resignStudent.RegistrationCourses = _registrationProvider.GetRegistrationCourses(student.Id, currentTerm.Id);
                    resignStudent.TermId = currentTerm.Id;
                }
                else
                {
                    _flashMessage.Danger(Message.StudentNotFound);
                }
            }

            return View(resignStudent);
        }

        [PermissionAuthorize("ResignStudent", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ResignStudentViewModel model)
        {
            var resignReason = _db.ResignReasons.SingleOrDefault(x => model.ResignReasonId == x.Id)?.DescriptionEn ?? "N/A";
            var studentToUpdate = _db.Students.Include(x => x.AcademicInformation)
                                              .IgnoreQueryFilters()
                                              .SingleOrDefault(x => x.Id == model.StudentId);

            var resignStudent = new ResignStudent
                                {
                                    StudentId = model.StudentId,
                                    TermId = model.TermId,
                                    EffectiveTermId = model.EffectiveTermId,
                                    ResignReasonId = model.ResignReasonId,
                                    Remark = model.Remark,
                                    ApprovedAt = model.ApprovedAt
                                };

            var studentStatus = new StudentStatusLog
                                {
                                    StudentId = model.StudentId,
                                    TermId = model.TermId,
                                    Source = "Resign",
                                    StudentStatus = "rs",
                                    EffectiveAt = model.ApprovedAt,
                                    Remark = $"{ resignReason } { model.Remark }"
                                };
            try
            {
                _db.ResignStudents.Add(resignStudent);
                _db.StudentStatusLogs.Add(studentStatus);
                
                studentToUpdate.StudentStatus = "rs";
                studentToUpdate.IsActive = KeystoneLibrary.Providers.StudentProvider.IsActiveFromStudentStatus(studentToUpdate.StudentStatus);
                _db.SaveChanges();

                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index), new
                                                       {
                                                           AcademicLevelId = studentToUpdate.AcademicInformation.AcademicLevelId,
                                                           Code = studentToUpdate.Code
                                                       });
            }
            catch
            {
                CreateSelectList();
                _flashMessage.Danger(Message.UnableToCreate);
                return View(model);
            }
        }
        [HttpPost]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult ExportExcel(Criteria criteria, string returnUrl)
        {
            var results = _studentProvider.GetResignStudents(criteria);
            if (results != null && results.Any())
            {
                using (var wb = GenerateWorkBook(results))
                {
                    return wb.Deliver($"Resign Student Report.xlsx");
                }
            }

            return Redirect(returnUrl);
        }

        private XLWorkbook GenerateWorkBook(List<ResignStudentViewModel> results)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "STUDENT CODE";

            ws.Cell(row, column++).Value = "NAME EN";
            ws.Cell(row, column++).Value = "DIVISION";
            ws.Cell(row, column++).Value = "MAJOR";
            ws.Cell(row, column++).Value = "CURRICULUM";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "CREDIT EARN";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "CREDIT REGISTRATION";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "GPA";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "STUDENT STATUS";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "EFFECTIVE TERM";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "APPROVALED DATE";

            ws.Cell(row, column++).Value = "RESIGN REASON";

            ws.Cell(row, column++).Value = "REMARK";

            foreach (var item in results)
            {
                column = 1;
                row++;
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).SetValue<string>(item.StudentCode);

                ws.Cell(row, column++).Value = item.FullNameEn;
                ws.Cell(row, column++).Value = item.FacultyCodeAndName;
                ws.Cell(row, column++).Value = item.DepartmentCodeAndName;
                ws.Cell(row, column++).Value = item.CurriculumVersionCodeAndName;

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.CreditComp;

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.CreditEarn;

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.GPAString;


                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.StudentStatusText;
                
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).SetValue<string>(item.EffectiveTerm);

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.ApprovedAtText;

                ws.Cell(row, column++).Value = item.ResignReason;
                ws.Cell(row, column++).Value = item.Remark;
            }

            ws.Columns().AdjustToContents();
            ws.Rows().AdjustToContents();
            return wb;
        }
        
        public void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.ResignReasons = _selectListProvider.GetResignReasons();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId (academicLevelId);

                if (facultyId > 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }
        }
    }
}