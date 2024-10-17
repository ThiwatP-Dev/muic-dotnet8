using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Data;
using Vereyon.Web;
using Microsoft.AspNetCore.Mvc;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Profile;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("TransferStudent", PolicyGenerator.Write)]
    public class TransferStudentController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly ICurriculumProvider _curriculumProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IEquivalenceProvider _equivalenceProvider;
        
        public TransferStudentController(ApplicationDbContext db, 
                                         IFlashMessage flashMessage,
                                         ISelectListProvider selectListProvider,
                                         ICurriculumProvider curriculumProvider,
                                         IRegistrationProvider registrationProvider,
                                         IStudentProvider studentProvider,
                                         IEquivalenceProvider equivalenceProvider) : base(db, flashMessage, selectListProvider)
        {
            _studentProvider = studentProvider;
            _curriculumProvider = curriculumProvider;
            _registrationProvider = registrationProvider;
            _equivalenceProvider = equivalenceProvider;
        }

        public IActionResult Index(string studentCode, long transferUniversityId, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            var model = new StudentTransferViewModel(0);
            if (string.IsNullOrEmpty(studentCode) || transferUniversityId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            var isExist = _studentProvider.IsExistStudent(studentCode);
            if (isExist) 
            {
                var student = _studentProvider.GetStudentInformationByCode(studentCode);
                model.TransferUniversityId = transferUniversityId;
                model.StudentId = student.Id;
                model.StudentCode = student.Code;
                model.StudentFirstName = student.FirstNameEn;
                model.StudentLastName = student.LastNameEn;
                model.AcademicLevel = student.AcademicInformation.AcademicLevel.NameEn;
                model.FacultyName = student.AcademicInformation.Faculty.NameEn;
                model.DepartmentName = student.AcademicInformation.Department?.NameEn ?? "N/A";
                model.Credit = student.AcademicInformation.CreditComp;
                model.GPA = student.AcademicInformation.GPA;
                model.CurriculumName = student.AcademicInformation.CurriculumVersion?.Curriculum?.NameEn ?? "N/A";
                model.CurriculumVersionName = student.AcademicInformation.CurriculumVersion?.NameEn ?? "N/A";
                model.CurriculumId = student.AcademicInformation.CurriculumVersion?.CurriculumId ?? 0;
                model.CurriculumVersionId = student.AcademicInformation.CurriculumVersionId ?? 0;

                CreateSelectList(student.AcademicInformation.AcademicLevelId, transferUniversityId);
                return View(model);
            }
            else
            {
                return View(model);
            }
        }

        public IActionResult MatchCourses(StudentTransferViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (model.CurriculumVersionId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return RedirectToAction(nameof(Index), new { studentCode = model.StudentCode, transferUniversityId = model.TransferUniversityId });
            }

            if (model.CurriculumId != 0) 
            {
                var curriculum = _curriculumProvider.GetCurriculum(model.CurriculumId);
                model.CurriculumName = curriculum.NameEn;
            }

            if (model.CurriculumVersionId != 0) 
            {
                var curriculumVersion = _curriculumProvider.GetCurriculumVersion(model.CurriculumVersionId);
                model.CurriculumVersionName = curriculumVersion.NameEn;
            }
            
            model.StudentCourseEquivalents = _equivalenceProvider.GetExternalEquivalentCourses(model.StudentCourses, model.TransferUniversityId ?? 0);
            
            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);
            CreateSelectList(model.StudentCode, student.AcademicInformation.CurriculumVersion.Curriculum.Id, 
                             student.AcademicInformation.AcademicLevelId, model.CurriculumVersionId);

            return View(model);
        }

        public IActionResult Summary(StudentTransferViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            model.StudentCourseEquivalents = model.StudentCourseEquivalents.Select(x => {
                                                                                            x.CurrentCourseName = _db.Courses.SingleOrDefault(y => y.Id == x.CurrentCourseId).CourseAndCredit;
                                                                                            x.CurrentCourseGrade = _db.Grades.SingleOrDefault(y => y.Id == x.GradeId).Name;
                                                                                            x.NewCourseName = _db.Courses.SingleOrDefault(y => y.Id == x.NewCourseId).CourseAndCredit;
                                                                                            x.GradeName = _db.Grades.SingleOrDefault(y => y.Id == x.NewGradeId).Name;
                                                                                            return x;
                                                                                        })
                                                                           .ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveTransferStudent(StudentTransferViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    if (model.StudentCourseEquivalents.Any())
                    {
                        var transferCourseToUSpark = new KeystoneLibrary.Models.USpark.USparkTransferCourse
                        {
                            StudentCode = model.StudentCode,
                            OldCourses = new List<KeystoneLibrary.Models.USpark.USparkTransferCourse.TransferCourseDetail>(),
                            NewCourses = new List<KeystoneLibrary.Models.USpark.USparkTransferCourse.TransferCourseDetail>()
                        };

                        foreach (var item in model.StudentCourseEquivalents)
                        {
                            var studentTransferLog = _db.StudentTransferLogs.Where(x => x.StudentId == model.StudentId
                                                                                        && x.TermId == item.TermId
                                                                                        && x.TransferUniversityId == model.TransferUniversityId)
                                                                            .SingleOrDefault();

                            var currentCourse = _registrationProvider.GetCourse(item.CurrentCourseId ?? 0);
                            var registrationCourse = new RegistrationCourse
                                                    {
                                                        StudentId = model.StudentId,
                                                        TermId = item.TermId,
                                                        CourseId = item.NewCourseId,
                                                        SectionId = null,
                                                        IsPaid = true,
                                                        IsLock = false,
                                                        GradeName = item.GradeName,
                                                        IsSurveyed = false,
                                                        Status = "r",
                                                        IsActive = true,
                                                        GradePublishedAt = DateTime.Now,
                                                        IsGradePublished = true,
                                                        EstimatedGradeId = null,
                                                        GradeId = item.NewGradeId,
                                                        IsInstallment = false,
                                                        IsOnCredit = false,
                                                        IsTransferCourse = true,
                                                        IsStarCourse = false,
                                                        TransferCourseCode = currentCourse.Code,
                                                        TransferCourseName = currentCourse.CourseAndCredit,
                                                        TransferGradeName = item.CurrentCourseGrade,
                                                        TransferUniversityId = model.TransferUniversityId
                                                    };

                            _db.RegistrationCourses.Add(registrationCourse);
                            _db.SaveChanges();

                            _studentProvider.UpdateTermGrade(registrationCourse.StudentId, registrationCourse.TermId);
                            _studentProvider.UpdateCGPA(registrationCourse.StudentId);

                            if (studentTransferLog == null)
                            {
                                studentTransferLog = new StudentTransferLog
                                                     {
                                                         StudentId = model.StudentId,
                                                         TermId = item.TermId,
                                                         TransferUniversityId = model.TransferUniversityId,
                                                         Source = "Transfer"
                                                     };

                                _db.StudentTransferLogs.Add(studentTransferLog);
                                _db.SaveChanges();
                            }

                            var studentTransferLogDetail = new StudentTransferLogDetail
                                                        {
                                                            StudentTransferLogId = studentTransferLog.Id,
                                                            RegistrationCourseId = registrationCourse.Id,
                                                            CourseId = item.NewCourseId,
                                                            PreviousGrade = item.CurrentCourseGrade,
                                                            ExternalCourseId = item.CurrentCourseId,
                                                            GradeId = item.NewGradeId
                                                        };

                            transferCourseToUSpark.NewCourses.Add(new KeystoneLibrary.Models.USpark.USparkTransferCourse.TransferCourseDetail
                            {
                                KsCourseId = item.NewCourseId,
                                KsTermId = item.TermId,
                                Grade = item.GradeName
                            });

                            _db.StudentTransferLogDetails.Add(studentTransferLogDetail);
                            _db.SaveChanges();
                        }
                        _registrationProvider.TransferCourseToUspark(transferCourseToUSpark);

                        _flashMessage.Confirmation(Message.SaveSucceed);
                        transaction.Commit();
                        CreateSelectList();
                        return RedirectToAction(nameof(Index));
                    }

                    ViewBag.ReturnUrl = returnUrl;
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View("~/Views/TransferStudent/Summary.cshtml", model);
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View("~/Views/TransferStudent/Summary.cshtml", model);
                }
            }
        }

        private void CreateSelectList(long academicLevelId = 0, long transferUniversityId = 0)
        {
            ViewBag.TransferUniversities = _selectListProvider.GetTransferUniversities();
            ViewBag.Grades = _selectListProvider.GetGrades();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                if (transferUniversityId > 0)
                {
                    ViewBag.Courses = _selectListProvider.GetCoursesByAcademicLevelAndTransferUniversity(academicLevelId, transferUniversityId);
                }
            }
        }

        private void CreateSelectList(string studentCode, long curriculumId, long academicLevelId, long newCurriculumVersionId)
        {
            ViewBag.Curriculums = _selectListProvider.GetSelectableCurriculumList(studentCode);
            ViewBag.CurriculumVersions = _selectListProvider.GetSelectableCurriculumVersionList(newCurriculumVersionId, studentCode);
            ViewBag.Grades = _selectListProvider.GetGrades();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Courses = _selectListProvider.GetCourses();
            }
            
            if (newCurriculumVersionId > 0) 
            {
                ViewBag.CourseGroups = _selectListProvider.GetCurriculumCourseGroupsForChangeCurriculum(newCurriculumVersionId);
            }
        }
    }
}