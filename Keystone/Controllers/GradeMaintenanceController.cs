using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("GradeMaintenance", PolicyGenerator.Write)]
    public class GradeMaintenanceController : BaseController
    {
        protected readonly IGradeProvider _gradeProvider;
        protected readonly IStudentProvider _studentProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IFileProvider _fileProvider;
        protected readonly IUserProvider _userProvider;
        private UserManager<ApplicationUser> _userManager { get; }

        public GradeMaintenanceController(ApplicationDbContext db,
                                          IFlashMessage flashMessage,
                                          ISelectListProvider selectListProvider,
                                          IMapper mapper,
                                          IGradeProvider gradeProvider,
                                          IStudentProvider studentProvider,
                                          IFileProvider fileProvider,
                                          UserManager<ApplicationUser> user,
                                          IUserProvider userProvider,
                                          IRegistrationProvider registrationProvider) : base(db, flashMessage, mapper, selectListProvider) 
        {
            _gradeProvider = gradeProvider;
            _studentProvider = studentProvider;
            _registrationProvider = registrationProvider;
            _fileProvider = fileProvider;
            _userManager = user;
            _userProvider = userProvider;
        }

        public IActionResult Index(int page, Criteria criteria, string returnUrl) 
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList("", 0, criteria.AcademicLevelId);
            var model = new GradeMaintenanceViewModel();
            model.Criteria = criteria;

            if (!String.IsNullOrEmpty(criteria.Code) && criteria.TermId > 0)
            {
                var student = _studentProvider.GetStudentInformationByCode(criteria.Code);
                if (student == null)
                {
                    _flashMessage.Warning(Message.StudentNotFound);
                }
                else
                {
                    if (!student.IsActive)
                    {
                        _flashMessage.Warning(Message.InactiveStudent);
                    }
                    model.GradingInfo = (from regCourse in _db.RegistrationCourses
                                         join course in _db.Courses on regCourse.CourseId equals course.Id
                                         join section in _db.Sections on regCourse.SectionId equals section.Id into g1
                                         from section in g1.DefaultIfEmpty()
                                         join mainInstructor in _db.Instructors on section.MainInstructorId equals mainInstructor.Id into mainInstructors
                                         from mainInstructor in mainInstructors.DefaultIfEmpty()
                                         join title in _db.Titles on mainInstructor.TitleId equals title.Id into titles
                                         from title in titles.DefaultIfEmpty()
                                         join grade in _db.Grades on regCourse.GradeId equals grade.Id into grades
                                         from grade in grades.DefaultIfEmpty()
                                         where regCourse.StudentId == student.Id 
                                               && regCourse.TermId == criteria.TermId
                                               && regCourse.Status != "d"
                                         select new GradingInformation
                                                {
                                                    Course = course,
                                                    SectionNumber = section != null ? section.Number : "n/a",
                                                    Section = section,
                                                    PreviousGrade = grade.Name,
                                                    IsGradePublished = regCourse.IsGradePublished,
                                                    RegistrationCourseId = regCourse.Id,
                                                    MainInstructor = mainInstructor
                                                }).ToList();

                    model.StudentFullName = student.FullNameEn;
                    model.CurriculumName = student.AcademicInformation.CurriculumVersion?.Curriculum.NameEn;
                    model.Criteria.AcademicLevelId = model.Criteria.AcademicLevelId == 0 ? student.AcademicInformation.AcademicLevelId : model.Criteria.AcademicLevelId;

                    return View(model);
                }
            }

            _flashMessage.Warning(Message.RequiredData);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Details(long registrationCourseId)
        {
            var gradingInfo = (from regCourse in _db.RegistrationCourses
                               join student in _db.Students.IgnoreQueryFilters().Include(x => x.AcademicInformation) on regCourse.StudentId equals student.Id
                               join course in _db.Courses on regCourse.CourseId equals course.Id
                               join gradeTemplate in _db.GradeTemplates on course.GradeTemplateId equals gradeTemplate.Id
                               join section in _db.Sections on regCourse.SectionId equals section.Id into sections
                               from section in sections.DefaultIfEmpty()
                               join grade in _db.Grades on regCourse.GradeId equals grade.Id into grades
                               from grade in grades.DefaultIfEmpty()
                               join studentRawScore in _db.StudentRawScores on regCourse.Id equals studentRawScore.RegistrationCourseId into studentRawScores
                               from studentRawScore in studentRawScores.DefaultIfEmpty()
                               where regCourse.Id == registrationCourseId
                                     && regCourse.Status != "d"
                               select new GradingInformation
                               {
                                   GradesInTemplate = gradeTemplate.GradeIds,
                                   StudentRawScoreId = studentRawScore == null ? 0 : studentRawScore.Id,
                                   Course = course,
                                   PreviousGrade = grade == null ? "" : grade.Name,
                                   StudentCode = student.Code,
                                   TermId = regCourse.TermId,
                                   SectionNumber = section == null ? "" : section.Number,
                                   IsGradePublished = regCourse.IsGradePublished,
                                   RegistrationCourseId = regCourse.Id,
                                   AcademicLevelId = student.AcademicInformation.AcademicLevelId
                               }).SingleOrDefault();
            
            gradingInfo.GradingLogs = _db.GradingLogs.Where(x => x.RegistrationCourseId == gradingInfo.RegistrationCourseId)
                                                     .ToList();

            if(gradingInfo.GradingLogs.Any())
            {
                var userId = gradingInfo.GradingLogs.Select(x => x.UpdatedBy).ToList();
                var users = _userProvider.GetCreatedFullNameByIds(userId);
                foreach(var item in gradingInfo.GradingLogs)
                {
                    item.UpdatedByFullName = users.Where(x => x.CreatedBy == item.UpdatedBy).FirstOrDefault()?.CreatedByFullNameEn ?? item.UpdatedBy;
                }
            }
            
            CreateSelectList(gradingInfo.GradesInTemplate, gradingInfo.CurrentGradeId ?? 0, gradingInfo.AcademicLevelId);
            return PartialView("~/Views/GradeMaintenance/_GradeEditor.cshtml", gradingInfo);
        }

        public IActionResult EditGrade(GradingInformation model)
        {
            var user = GetUser();
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var currentGrade = model.CurrentGradeId.HasValue ? _gradeProvider.GetGradeById(model.CurrentGradeId.Value).Name : null;
                    var studentRawScore = _db.StudentRawScores.SingleOrDefault(x => x.Id == model.StudentRawScoreId);
                    if (studentRawScore != null)
                    {
                        studentRawScore.GradeId = model.CurrentGradeId;
                    }

                    var registrationCourse = _db.RegistrationCourses.SingleOrDefault(x => x.Id == model.RegistrationCourseId);
                    registrationCourse.GradeName = currentGrade;
                    registrationCourse.GradeId = model.CurrentGradeId;
                    registrationCourse.IsGradePublished = true;
                    
                    _studentProvider.UpdateTermGrade(registrationCourse.StudentId, registrationCourse.TermId);
                    _studentProvider.UpdateCGPA(registrationCourse.StudentId);
                    
                    var previousGrade = model.PreviousGrade == null ? "" : model.PreviousGrade;
                    var gradeLog = _gradeProvider.SetGradingLog(model.StudentRawScoreId, model.RegistrationCourseId, previousGrade, currentGrade, user.Id, model.Remark, "m");
                    if (model.UploadFile != null)
                    {
                        var formFile = model.UploadFile;
                        if (formFile.Length > 0)
                        {
                            gradeLog.DocumentUrl = _fileProvider.UploadFile(UploadSubDirectory.GRADE_MAINTENANCE, formFile, model.StudentCode + "_" + DateTime.UtcNow.ToString("yyMMddHHmmss"));
                        }
                    }
                    _db.GradingLogs.Add(gradeLog);

                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToEdit);
                }
            }

            return RedirectToAction(nameof(Index), new { TermId = model.TermId, Code = model.StudentCode, AcademicLevelId = model.AcademicLevelId });
        }

        public IActionResult ChangePublicationStatus(long registrationCourseId, bool IsGradePublished, long termId, string studentCode)
        {
            var academicLevelId = _db.Students.Include(x => x.AcademicInformation)
                                              .SingleOrDefault(x => x.Code == studentCode)?.AcademicInformation?.AcademicLevelId ?? 0;
            try
            {
                var model = _db.RegistrationCourses.SingleOrDefault(x => x.Id == registrationCourseId);
                model.IsGradePublished = !IsGradePublished;

                if (model.IsGradePublished)
                {
                    model.GradePublishedAt = DateTime.UtcNow;
                }
                _db.SaveChanges();
                    
                _studentProvider.UpdateTermGrade(model.StudentId, model.TermId);
                _studentProvider.UpdateCGPA(model.StudentId);
                
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
            }
            
            return RedirectToAction(nameof(Index), new { TermId = termId, Code = studentCode, AcademicLevelId = academicLevelId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditGradingLogRemark(long id, string remark) 
        {
            try
            {
                var gradingLog = _db.GradingLogs.Find(id);
                gradingLog.Remark = remark;

                _db.Entry(gradingLog).State = EntityState.Modified;
                _db.SaveChanges();
                return Ok(gradingLog);
            } 
            catch
            {
                return StatusCode((int)HttpStatusCode.Forbidden, Message.UnableToEdit);
            }
        }

        private void CreateSelectList(string gradeIds = "", long gradeId = 0, long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }

            //var gradesInTemplate = string.IsNullOrEmpty(gradeIds) ? new List<long>() : _registrationProvider.Deserialize(gradeIds)
            //                                                                                                .ToList();
            //ViewBag.Grades = _selectListProvider.GetGradesExcept(gradeId, gradesInTemplate);
            ViewBag.Grades = _selectListProvider.GetGrades();
        }    
    }
}