using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    public class ReenterController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IAdmissionProvider _admissionProvider;
        protected readonly IGradeProvider _gradeProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IMasterProvider _masterProvider;
        
        public ReenterController(ApplicationDbContext db,
                                 IFlashMessage flashMessage,
                                 ISelectListProvider selectListProvider,
                                 IStudentProvider studentProvider,
                                 ICacheProvider cacheProvider,
                                 IAdmissionProvider admissionProvider,
                                 IGradeProvider gradeProvider,
                                 IRegistrationProvider registrationProvider,
                                 IMasterProvider masterProvider) : base(db, flashMessage, selectListProvider) 
        {
            _studentProvider = studentProvider;
            _cacheProvider = cacheProvider;
            _admissionProvider = admissionProvider;
            _gradeProvider = gradeProvider;
            _registrationProvider = registrationProvider;
            _masterProvider = masterProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);

            var students = _db.Students.Where(x => (criteria.AcademicLevelId == 0
                                                    || criteria.AcademicLevelId == x.AcademicInformation.AcademicLevelId)
                                                    && (criteria.FacultyId == 0
                                                        || criteria.FacultyId == x.AcademicInformation.FacultyId)
                                                    && (criteria.DepartmentId == 0
                                                        || criteria.DepartmentId == x.AcademicInformation.DepartmentId)
                                                    && (criteria.StartStudentBatch == null
                                                        || x.AcademicInformation.Batch >= criteria.StartStudentBatch)
                                                    && (criteria.EndStudentBatch == null
                                                        || x.AcademicInformation.Batch <= criteria.EndStudentBatch)
                                                    && (criteria.PreviousCode == null
                                                        || criteria.PreviousCode == x.Code)
                                                    && ((string.IsNullOrEmpty(criteria.Type)
                                                         && (x.StudentStatus == "re" || x.StudentStatus == "ra"))
                                                         || criteria.Type == x.StudentStatus))
                                       .IgnoreQueryFilters()
                                       .Join(_db.AcademicInformations, 
                                             x => x.Code, 
                                             y => y.OldStudentCode,
                                             (x, y) => new StudentViewModel
                                                       {
                                                           StudentCode = x.Code,
                                                           StudentName = x.FullNameEn,
                                                           Faculty = x.AcademicInformation.Faculty.NameEn,
                                                           Department = x.AcademicInformation.Department.NameEn,
                                                           Type = x.StudentStatusText,
                                                           CurrentStudentCode = y.Student.Code
                                                       } )
                                       .OrderBy(x => x.StudentCode)
                                       .ToList();

            var model = new ReenterViewModel
                        {
                            Criteria = criteria,
                            StudentDetails = students
                        };

            return View(model);
        }

        public ActionResult Create(string code,  string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (string.IsNullOrEmpty(code))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            if (_studentProvider.IsExistStudentExceptAdmission(code))
            {
                var student = _studentProvider.GetStudentInformationByCode(code);
                CreateSelectList(student);
                if (student.GraduationInformation != null && student.GraduationInformation?.GraduatedAt != null)
                {
                    _flashMessage.Danger(Message.AlreadyGraduated);
                    return View();
                }

                if (student.StudentStatus == "a")
                {
                    _flashMessage.Danger(Message.AdmissionStudent);
                    return View();
                }

                if (student.StudentStatus == "re" || student.StudentStatus == "ra")
                {
                    _flashMessage.Danger(Message.AlreadyReenter);
                    return View();
                }

                return View(student);
            }
            else
            {
                _flashMessage.Danger(Message.StudentNotFound);
                return View();
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Student model, string reenterType, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var student = _studentProvider.GetStudentInformationByCode(model.Code);
            var currentTerm = _cacheProvider.GetCurrentTerm(model.AcademicInformation.AcademicLevelId);

            if (string.IsNullOrEmpty(model.StudentCode))
            {
                CreateSelectList(model);
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            var isExist = _admissionProvider.IsExistStudentCode(model.StudentCode);
            if (isExist)
            {
                CreateSelectList(model);
                _flashMessage.Danger(Message.ExistedStudentCode);
                return View(model);
            }

            var errorMessage = "";
            var reenterReason = _masterProvider.GetReEnterReason(model.ReenterReasonId)?.DescriptionEn;
            var reenterStudent = _studentProvider.SaveReenterStudent(student, model, currentTerm.Id, model.StudentCode, reenterType, reenterReason, out errorMessage);
            if (reenterStudent == null)
            {
                CreateSelectList(model);
                _flashMessage.Danger(errorMessage);
                return View(model);
            }

            _flashMessage.Confirmation(Message.SaveSucceed);
            return RedirectToAction(nameof(ReenterCourse), new { code = reenterStudent.Code });
        }

        public ActionResult ReenterCourse(string code, Criteria criteria)
        {
            var student = _studentProvider.GetStudentInformationByCode(code);
            var previousStudent = _studentProvider.GetStudentInformationByCode(student.AcademicInformation.OldStudentCode);
            var reenterCourse = new ReenterCourseViewModel
                                {
                                    Criteria = criteria,
                                    TransferedTermId = criteria.TermId,
                                    TransferedGrade = criteria.TransferedGrade,
                                    StudentCode = student.Code,
                                    StudentName = student.FullNameEn,
                                    CurriculumVersion = student?.AcademicInformation?.CurriculumVersion?.NameEn,
                                    TransferedCredit = previousStudent?.AcademicInformation?.CurriculumVersion?.TotalCredit / 2 ?? 0
                                };

            CreateSelectList(student);
            
            var previousRegistrationCourse = _registrationProvider.GetRegistrationCoursesByStudentCode(student.AcademicInformation.OldStudentCode);
            reenterCourse.TransferCourses = previousRegistrationCourse.Select(x => new TransferCourseViewModel
                                                                                   {
                                                                                       Code = x.Course.Code,
                                                                                       Name = x.Course.NameEn,
                                                                                       Term = x.Term.TermText,
                                                                                       AcademicYear = x.Term.AcademicYear,
                                                                                       AcademicTerm = x.Term.AcademicTerm,
                                                                                       Credit = x.Course.Credit,
                                                                                       Grade = x.Grade?.Name,
                                                                                       RegistrationCourseId = x.Id
                                                                                   })
                                                                      .OrderBy(x => x.AcademicYear)
                                                                          .ThenBy(x => x.AcademicTerm)
                                                                          .ThenBy(x => x.Code)
                                                                      .ToList();
            return View(reenterCourse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReenterCourse(ReenterCourseViewModel model, List<long> registrationCourseIds)
        {
            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);
            var registrationCourses = _registrationProvider.GetRegistrationCoursesByIds(registrationCourseIds);
            try
            {
                foreach (var item in registrationCourses)
                {
                    var grades = _db.Grades.Where(x => string.Equals(x.Name, "T", StringComparison.CurrentCultureIgnoreCase)).ToList();
                    _db.RegistrationCourses.Add(new RegistrationCourse
                                                {
                                                    StudentId = student.Id,
                                                    TermId = model.TransferedTermId,
                                                    CourseId = item.CourseId,
                                                    SectionId = item.SectionId,
                                                    IsPaid = false,
                                                    IsLock = false,
                                                    GradeName = model.TransferedGrade == "t" ? "t"
                                                                                             : model.TransferedGrade == "T"
                                                                                             ? "T"
                                                                                             : item.Grade?.Name,
                                                    IsTransferCourse = true,
                                                    IsSurveyed = true,
                                                    Status = "a",
                                                    GradePublishedAt = null,
                                                    IsGradePublished = false,
                                                    GradeId = model.TransferedGrade == "t" ? grades.SingleOrDefault(x => x.Name == "t").Id
                                                                                           : model.TransferedGrade == "T"
                                                                                           ? grades.SingleOrDefault(x => x.Name == "T").Id
                                                                                           : item.GradeId,
                                                    TransferCourseCode = item.Course?.Code,
                                                    TransferCourseName = item.Course?.NameEn,
                                                    TransferGradeName = item.Grade?.Name
                                                });
                }

                _db.SaveChanges();
                foreach (var registrationCourse in registrationCourses)
                {
                    _studentProvider.UpdateTermGrade(student.Id, model.TransferedTermId);
                    _studentProvider.UpdateCGPA(student.Id);
                }

                return RedirectToAction("Details", nameof(Student), new { code = model.StudentCode, tabIndex = "0" });
            }
            catch
            {
                CreateSelectList(student);
                _flashMessage.Danger(Message.UnableToCreate);
                return RedirectToAction(nameof(ReenterCourse), new { code = model.StudentCode });
            }
        }

        private void CreateSelectList(Student model)
        {
            var academicLevelId = model.AcademicInformation?.AcademicLevelId ?? 0;
            var admissionRoundId = model.AdmissionInformation?.AdmissionRoundId ?? 0;
            var admissionTermId = model.AdmissionInformation?.AdmissionTermId ?? 0;
            var facultyId = model.AcademicInformation?.FacultyId ?? 0;
            var departmentId = model.AcademicInformation?.DepartmentId ?? 0;
            var curriculumId = model.AcademicInformation?.CurriculumVersion?.CurriculumId ?? 0;
            var curriculumVersionId = model.AcademicInformation?.CurriculumVersionId ?? 0;
            var nationalityId = model?.NationalityId ?? 0;
            var batch = model.AcademicInformation?.Batch ?? 0;
            var studentGroupId = model.AcademicInformation?.StudentGroupId ?? 0;
            var studentFeeTypeId = model.StudentFeeGroup.StudentFeeTypeId ?? 0;

            ViewBag.AdmissionPlaces = _selectListProvider.GetAdmissionPlaces();
            ViewBag.ReenterTypes = _selectListProvider.GetReenterType();
            ViewBag.ReenterReasons = _selectListProvider.GetReenterReasons();
            ViewBag.TitlesEn = _selectListProvider.GetTitlesEn();
            ViewBag.TitlesTh = _selectListProvider.GetTitlesTh();
            ViewBag.AdmissionTypes = _selectListProvider.GetAdmissionTypes();
            ViewBag.StudentGroups = _selectListProvider.GetStudentGroups();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.AdmissionRounds = _selectListProvider.GetAdmissionRounds(academicLevelId, admissionTermId);
            ViewBag.AcademicPrograms = _selectListProvider.GetAcademicProgramsByAcademicLevelId(academicLevelId);
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersionForAdmissionCurriculum(admissionTermId, admissionRoundId, facultyId, departmentId);
            ViewBag.StudentFeeGroups = _selectListProvider.GetStudentFeeGroups(academicLevelId, facultyId, departmentId, curriculumId, curriculumVersionId, 
                                                                               nationalityId, batch, studentGroupId, studentFeeTypeId);
            ViewBag.TransferedGrades = _selectListProvider.GetTransferedGrades();
            ViewBag.WeightGrades = _selectListProvider.GetWeightGrades();
            ViewBag.NonWeightGrades = _selectListProvider.GetNonWeightGrades();
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.ReenterTypes = _selectListProvider.GetReenterType();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            if (facultyId != 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            }
        }
    }
}