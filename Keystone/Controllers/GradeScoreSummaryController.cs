using AutoMapper;
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
    public class GradeScoreSummaryController : BaseController
    {
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IGradeProvider _gradeProvider;
        protected readonly IInstructorProvider _instructorProvider;
        private UserManager<ApplicationUser> _userManager { get; }
        protected readonly ICacheProvider _cacheProvider;

        public GradeScoreSummaryController(ApplicationDbContext db,
                                           IFlashMessage flashMessage,
                                           IMapper mapper,
                                           ISelectListProvider selectListProvider,
                                           IAcademicProvider academicProvider,
                                           IRegistrationProvider registrationProvider,
                                           IGradeProvider gradeProvider,
                                           IInstructorProvider instructorProvider,
                                           ICacheProvider cacheProvider,
                                           UserManager<ApplicationUser> user) : base(db, flashMessage, mapper, selectListProvider)
        {
            _registrationProvider = registrationProvider;
            _academicProvider = academicProvider;
            _gradeProvider = gradeProvider;
            _userManager = user;
            _instructorProvider = instructorProvider;
            _cacheProvider = cacheProvider;
        }

        public IActionResult Index(GradingScoreSummaryViewModel model)
        {
            if (model.TermId == 0)
            {
                model.AcademicLevelId = _db.AcademicLevels.SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
                model.TermId = _cacheProvider.GetCurrentTerm(model.AcademicLevelId).Id;
            }

            CreateSelectList(model.AcademicLevelId);
            ViewBag.TermId = model.TermId;
            CreateSelectList(model.AcademicLevelId);
            var instructorId = GetInstructorId();
            var user = GetUser();
            var isGradeMember = _db.GradeMembers.Any(x => x.UserId == user.Id);
            var result = _db.Sections.AsNoTracking()
                                     .Include(x => x.Course)
                                     .Where(x => x.TermId == model.TermId
                                                && x.ParentSectionId == null
                                                && (x.MainInstructorId == instructorId || isGradeMember))
                                     .Select(x => new
                                                  {
                                                      CourseId = x.CourseId,
                                                      CourseAndCredit = x.Course.CourseAndCredit,
                                                      SectionId = x.Id,
                                                      SectionNumber = x.Number,
                                                      MainInstructorId = x.MainInstructorId
                                                  })
                                     .GroupBy(x => x.CourseId)
                                     .Select(x => new 
                                           {
                                               CourseId = x.FirstOrDefault().CourseId,
                                               CourseAndCredit = x.FirstOrDefault().CourseAndCredit,
                                               MainInstructorId = x.FirstOrDefault().MainInstructorId,
                                               Sections = x.OrderBy(y => y.SectionNumber).ToList()
                                           })
                                     .ToList();

            var sectionCourseIds = result.Select(x => x.CourseId).ToList();
            var sectionIds = _db.Sections.AsNoTracking()
                                         .Where(x => x.TermId == model.TermId
                                                     && x.ParentSectionId == null
                                                     && x.Status == "a"
                                                     && sectionCourseIds.Contains(x.CourseId)
                                                     && (x.MainInstructorId == instructorId || isGradeMember))
                                         .Select(x => x.Id)
                                         .ToList();

            var sectionIdsNullable = sectionIds.Select(x => (long?)x).ToList();
            var barcodes = _db.Barcodes.Where(x => sectionIds.Contains(x.SectionId)).ToList();

            var jointSections = _db.Sections.AsNoTracking()
                                           .Include(x => x.Course)
                                           .Where(x => sectionIdsNullable.Contains(x.ParentSectionId))
                                           .Select(x => new
                                                        {
                                                            SectionId = x.Id,
                                                            ParentSectionId = x.ParentSectionId,
                                                            CourseCode = x.Course.Code,
                                                            SectionNumber = x.Number
                                                        })
                                           .ToList();
            var sectionIdsMasterJoint = sectionIds;
            sectionIdsMasterJoint.AddRange(jointSections.Select(x => x.SectionId).ToList());

            var markAllocations = _db.MarkAllocations.Where(x => x.TermId == model.TermId
                                                                 && sectionCourseIds.Contains(x.CourseId))
                                                     .ToList();
            var studentRawScores = _db.StudentRawScores.Include(x => x.Grade)
                                                       .Include(x => x.RegistrationCourse)
                                                          .ThenInclude(x => x.Grade)
                                                       .Where(x => sectionIdsMasterJoint.Contains(x.SectionId)).ToList();
            var gradeSkip = _db.Grades.Where(x => x.Name.ToUpper() == "I" 
                                                  || x.Name.ToUpper() == "AU" )
                                      .Select(x => (long?)x.Id)
                                      .ToList();

            foreach (var item in result)
            {
                var sectionIdsByCourse = item.Sections.Select(x => x.SectionId).ToList();
                var sectionIdsMasterJointByCourse = sectionIdsByCourse;
                sectionIdsMasterJointByCourse.AddRange(jointSections.Where(x => sectionIdsByCourse.Contains(x.ParentSectionId??0)).Select(x => x.SectionId).ToList());
                model.GradingStatuses.Add(new GradingScoreSummaryDetailViewModel
                                          {
                                               TermId = model.TermId,
                                               CourseId = item.CourseId,
                                               CourseNames = item.CourseAndCredit,
                                               InstructorId = item.MainInstructorId ?? 0,
                                               IsScored = studentRawScores.Any(x => sectionIdsMasterJointByCourse.Contains(x.SectionId)) 
                                                                                     && studentRawScores.Any(x => sectionIdsMasterJointByCourse.Contains(x.SectionId) 
                                                                                     && !(x.IsSkipGrading || gradeSkip.Contains(x.GradeId))),
                                               SectionNumbers = string.Join(", ", item.Sections.Select(x => x.SectionNumber).ToList()),
                                               IsAllocated = markAllocations.Any(x => item.CourseId == x.CourseId),
                                               IsBarcodeGenereated = barcodes?.Any(x => sectionIdsByCourse.Contains(x.SectionId)) ?? false,
                                               SkipGrading = studentRawScores.Count(x => sectionIdsMasterJointByCourse.Contains(x.SectionId) 
                                                                                         && (x.IsSkipGrading || gradeSkip.Contains(x.GradeId) || x.TotalScore == null)
                                                                                         && !(x.RegistrationCourse.Grade?.Name == "W" || x.RegistrationCourse.Grade?.Name == "w")),
                                               TotalStudent = studentRawScores.Count(x => sectionIdsMasterJointByCourse.Contains(x.SectionId)),
                                               TotalStudentScoring = studentRawScores.Count(x => !(x.IsSkipGrading || gradeSkip.Contains(x.GradeId) || x.TotalScore == null) 
                                                                                                  && !x.RegistrationCourse.IsGradePublished 
                                                                                                  && !(x.RegistrationCourse.Grade?.Name == "W" || x.RegistrationCourse.Grade?.Name == "w")
                                                                                                  && sectionIdsMasterJointByCourse.Contains(x.SectionId)),
                                               Withdrawal = studentRawScores.Count(x => sectionIdsMasterJointByCourse.Contains(x.SectionId) 
                                                                                        && !x.RegistrationCourse.IsGradePublished
                                                                                        && (x.RegistrationCourse.Grade?.Name == "W" || x.RegistrationCourse.Grade?.Name == "w")),
                                               Published = studentRawScores.Count(x => sectionIdsMasterJointByCourse.Contains(x.SectionId) && x.RegistrationCourse.IsGradePublished),
                                               Barcode = barcodes?.Where(x => sectionIdsByCourse.Contains(x.SectionId)).OrderByDescending(x => x.CreatedAt).Select(x => x.BarcodeNumber).FirstOrDefault() ?? "-",
                                               IsPublished = barcodes?.Any(x => sectionIdsByCourse.Contains(x.SectionId) && x.IsPublished) ?? false,
                                               JointSections = string.Join(", ",jointSections.Where(x => sectionIdsByCourse.Contains(x.ParentSectionId??0))
                                                                                             .OrderBy(x => x.SectionNumber)
                                                                                                .ThenBy(x => x.CourseCode)
                                                                                             .Select(x => x.CourseCode + "(" + x.SectionNumber + ")" )
                                                                                             .ToList())
                                          });
            }

            return View(model);
        }

        public IActionResult Edit(long courseId, long termId, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var instructorId = GetInstructorId();
            var term = _db.Terms.SingleOrDefault(x => x.Id == termId);
            var user = GetUser();
            var isGradeMember = _db.GradeMembers.Any(x => x.UserId == user.Id);
            var model = _gradeProvider.GetGradeScoreSummaryViewModelByCourseId(courseId, term.Id, instructorId, isGradeMember);
            return View(model);
        }

        public IActionResult EditGrade(long courseId, long termId, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var instructorId = GetInstructorId();
            var term = _db.Terms.SingleOrDefault(x => x.Id == termId);
            var user = GetUser();
            var isGradeMember = _db.GradeMembers.Any(x => x.UserId == user.Id);
            var model = _gradeProvider.GetGradeScoreSummaryViewModelByCourseId(courseId, term.Id, instructorId, isGradeMember);
            if(model.HaveBarcode)
            {
                ViewBag.DisplayQuestion = "Are you sure to submit this form, because barcode already in database.";
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(GradingReportViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var instructorId = GetInstructorId();
            var user = GetUser();
            var isGradeMember = _db.GradeMembers.Any(x => x.UserId == user.Id);
            var term = _db.Terms.SingleOrDefault(x => x.Id == model.TermId);
            if (model.GradingCurves.Any(x => x.Maximum == 0) || model.GradingCurvesNotCalc.Any(x => x.Maximum == 0))
            {
                _flashMessage.Warning(Message.RequiredData);
                return Edit(model.CourseId, model.TermId, returnUrl);
            }

            try
            {   
                var result = new GradingReportViewModel();
                var oldCurves = _db.GradingCurves.Include(x => x.Grade)
                                                 .Where(x => model.CourseId == x.CourseId
                                                             && model.TermId == x.TermId)
                                                 .ToList();
                foreach (var item in model.GradingCurves)
                {
                    if (oldCurves.Any(x => x.Id == item.Id))
                    {
                        oldCurves.Where(x => x.Id == item.Id)
                                 .Select(x => {
                                                   x.CourseId = model.CourseId;
                                                   x.TermId = model.TermId;
                                                   x.GradeTemplateId = item.GradeTemplateId;
                                                   x.GradeId = item.GradeId;
                                                   x.Minimum = item.Minimum;
                                                   x.Maximum = item.Maximum;
                                                   return x;
                                              }).ToList();
                    }
                }
                foreach (var item in model.GradingCurvesNotCalc)
                {
                    if (oldCurves.Any(x => x.Id == item.Id))
                    {
                        oldCurves.Where(x => x.Id == item.Id)
                                 .Select(x => {
                                                   x.CourseId = model.CourseId;
                                                   x.TermId = model.TermId;
                                                   x.GradeTemplateId = item.GradeTemplateId;
                                                   x.GradeId = item.GradeId;
                                                   x.Minimum = item.Minimum;
                                                   x.Maximum = item.Maximum;
                                                   return x;
                                              }).ToList();
                    }
                }
                if (model.IsSave)
                {
                    _db.SaveChanges();
                }
                else
                {
                    result = _gradeProvider.GetGradeScoreSummaryViewModelByCourseId(model.CourseId, term.Id, instructorId, isGradeMember);
                }
                var sections = _db.Sections.AsNoTracking()
                                         .Include(x => x.Course)
                                         .Where(x => x.TermId == model.TermId
                                                    && x.ParentSectionId == null
                                                    && x.MainInstructorId == instructorId)
                                         .Select(x => new
                                                      {
                                                          CourseId = x.CourseId,
                                                          CourseAndCredit = x.Course.CourseAndCredit,
                                                          SectionId = x.Id,
                                                          SectionNumber = x.Number
                                                      })
                                         .GroupBy(x => x.CourseId)
                                         .Select(x => new 
                                               {
                                                   CourseId = x.FirstOrDefault().CourseId,
                                                   CourseAndCredit = x.FirstOrDefault().CourseAndCredit,
                                                   Sections = x.OrderBy(y => y.SectionNumber).ToList()
                                               })
                                         .ToList();

                var sectionCourseIds = sections.Select(x => x.CourseId).ToList();
                var sectionIds = _db.Sections.AsNoTracking()
                                             .Where(x => x.TermId == model.TermId
                                                         && x.ParentSectionId == null
                                                         && x.Status == "a"
                                                         && sectionCourseIds.Contains(x.CourseId)
                                                         && (x.MainInstructorId == instructorId || isGradeMember))
                                             .Select(x => x.Id)
                                             .ToList();

                var sectionIdsNullable = sectionIds.Select(x => (long?)x).ToList();
                var barcodes = _db.Barcodes.Where(x => sectionIds.Contains(x.SectionId)).ToList();

                var jointSections = _db.Sections.AsNoTracking()
                                               .Include(x => x.Course)
                                               .Where(x => sectionIdsNullable.Contains(x.ParentSectionId))
                                               .Select(x => new
                                                            {
                                                                SectionId = x.Id,
                                                                ParentSectionId = x.ParentSectionId,
                                                                CourseCode = x.Course.Code,
                                                                SectionNumber = x.Number
                                                            })
                                               .ToList();
                var sectionIdsMasterJoint = sectionIds;
                sectionIdsMasterJoint.AddRange(jointSections.Select(x => x.SectionId).ToList());

                var markAllocations = _db.MarkAllocations.Where(x => x.TermId == model.TermId
                                                                     && sectionCourseIds.Contains(x.CourseId))
                                                         .ToList();
                var studentRawScores = _db.StudentRawScores.Include(x => x.RegistrationCourse)
                                                           .Where(x => sectionIdsMasterJoint.Contains(x.SectionId) && !x.IsSkipGrading)
                                                           .ToList();

                var gradeCurves = oldCurves;


                if(model.IsSave)
                {
                    var studentRawScoreIds = studentRawScores.Select(x => (long?)x.Id).ToList();
                    var gradeLogs = _db.GradingLogs.Where(x => studentRawScoreIds.Contains(x.StudentRawScoreId)).ToList();

                    var gradeNotEditScore = _db.Grades.Where(x => x.Name.ToUpper() == "I" 
                                                                || x.Name.ToUpper() == "AU" )
                                                    .Select(x => x.Id)
                                                    .ToList();

                    foreach(var studentRawScore in studentRawScores.Where(x => !x.RegistrationCourse.IsGradePublished).ToList())
                    {
                        if(!gradeNotEditScore.Any(x => x == studentRawScore.GradeId))
                        {
                            studentRawScore.GradeId = gradeCurves.Where(x => x.Minimum <= studentRawScore.TotalScore 
                                                                            && x.Maximum + 1 > studentRawScore.TotalScore
                                                                            && x.GradeTemplateId == studentRawScore.GradeTemplateId)
                                                                .FirstOrDefault()?.GradeId ?? null;
                        }
                    }

                    _db.GradingLogs.RemoveRange(gradeLogs);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(EditGrade),new { courseId = model.CourseId, termId = model.TermId, returnUrl = returnUrl});
                }
                else
                {
                    foreach(var studentRawScore in result.StudentScoreAllocations.Where(x => !x.IsGradePublish).ToList())
                    { 
                        studentRawScore.Grade = gradeCurves.Where(x => x.Minimum <= studentRawScore.TotalScore 
                                                                        && x.Maximum + 1 > studentRawScore.TotalScore
                                                                        && x.GradeTemplateId == studentRawScore.GradeTemplateId)
                                                          .FirstOrDefault()?.Grade.Name ?? string.Empty;

                    }
                    var classStatistics = _gradeProvider.GetClassStatisticsGradeScoreSummary(result.StudentScoreAllocations);
                    result.ClassStatistics = classStatistics;
                    result.GradingCurves = gradeCurves.Where(x => x.Grade.IsCalculateGPA).OrderByDescending(x => x.Maximum).ToList();
                    result.GradingCurvesNotCalc = gradeCurves.Where(x => !x.Grade.IsCalculateGPA).OrderByDescending(x => x.Maximum).ToList();
                    result.GradeNormalCurves = _gradeProvider.GetGradeNormalCurves(classStatistics);
                    result.GradingRanges = _gradeProvider.GetSummaryGradingCurves(model.CourseId, model.TermId, result.StudentScoreAllocations);
                    result.GradingFrequencies = _gradeProvider.GetGradingFrequencies(result.StudentScoreAllocations);
                    result.StudentRawScores = new List<StudentRawScoreViewModel>();
                    return View(result);
                }
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
                return Edit(model.CourseId, model.TermId, returnUrl);
            }
        }

        public GradingStudentRawScoreViewModel GetGradingByStudentRawScoreId(long studentRawScoreId, long courseId, long termId)
        {
            var studentRawScore = _db.StudentRawScores.Include(x => x.Grade)
                                                      .Include(x => x.Student)
                                                         .ThenInclude(x => x.Title)
                                                      .Include(x => x.Section)
                                                      .AsNoTracking()
                                                      .SingleOrDefault(x => x.Id == studentRawScoreId);

            var course = _db.Courses.AsNoTracking()
                                    .SingleOrDefault(x => x.Id == studentRawScore.CourseId);
            var curriculumVersionId = _db.AcademicInformations.AsNoTracking()
                                                              .SingleOrDefault(x => x.StudentId == studentRawScore.StudentId)?.CurriculumVersionId ?? 0;
            var courseGroupIds = _db.CourseGroups.Where(x => x.CurriculumVersionId == curriculumVersionId)
                                                 .AsNoTracking()
                                                 .Select(x => x.Id)
                                                 .ToList();
            var gradeTemplateId = _db.CurriculumCourses.Where(x => courseGroupIds.Contains(x.CourseGroupId) && x.CourseId == course.Id)
                                                       .AsNoTracking()
                                                       .FirstOrDefault()?.GradeTemplateId;
            if (gradeTemplateId == 0 || gradeTemplateId == null)
            {
                gradeTemplateId = course.GradeTemplateId ?? 0;
            }

            var gradeCurves = _db.GradingCurves.Where(x =>  x.CourseId == courseId 
                                                           && x.TermId == termId )
                                               .Select(x => new 
                                                           {
                                                               Id = x.GradeId,
                                                               Name = x.Grade.Name,
                                                               GradeTemplateId = x.GradeTemplateId
                                                           })
                                               .AsNoTracking()
                                               .ToList();

            var grade = _db.Grades.Where(x => !gradeCurves.Any(y => y.Id == x.Id))
                                  .AsNoTracking()
                                  .Select(x => new GradeSelectViewModal
                                               {
                                                   Id = x.Id,
                                                   Name = x.Name
                                               }).ToList();

            var gradeCurve = gradeCurves.Where(x => x.GradeTemplateId == gradeTemplateId)
                                        .Select(x => new GradeSelectViewModal
                                                     {
                                                        Id = x.Id,
                                                        Name = x.Name
                                                     })
                                        .ToList();
            gradeCurve.AddRange(grade);
            var userId = GetUser();
            var isGradeMember = _db.GradeMembers.Any(x => x.UserId == userId.Id);

            var result = new GradingStudentRawScoreViewModel
                         {
                             CurrentGrade = studentRawScore.Grade.Name,
                             CourseId = courseId,
                             TermId = termId,
                             IsGradeMember = isGradeMember,
                             CurrentGradeId = studentRawScore.GradeId,
                             Student = studentRawScore.Student.CodeAndName,
                             Course = $"{course.Code} ({ studentRawScore.Section.Number }) [{ studentRawScore.Section.SectionTypes }]",
                             Grades = gradeCurve
                         };
            return result;
        }

        public ActionResult UpdateGrade(long studentRawScoreId, long gradeId, string remark)
        {
            var studentRawScore = _db.StudentRawScores.Include(x => x.Grade).SingleOrDefault(x => x.Id == studentRawScoreId);
            var grade = _db.Grades.SingleOrDefault(x => x.Id == gradeId);

            if(studentRawScore == null || grade == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Ok(new {
                                IsInvalid = true
                              });
            }

            try
            {

                var gradeLog = new GradingLog
                               {
                                   RegistrationCourseId = studentRawScore.RegistrationCourseId,
                                   PreviousGrade = studentRawScore.Grade.Name,
                                   CurrentGrade = grade.Name,
                                   StudentRawScoreId = studentRawScore.Id,
                                   Type = "b",
                                   Remark = remark
                               };
                studentRawScore.GradeId = grade.Id;
                _db.SaveChanges();

                _db.GradingLogs.Add(gradeLog);
                _db.SaveChanges();
                return Ok(new {
                                IsInvalid = false,
                                Grade = grade.Name
                              });
            }
            catch
            {
                return StatusCode((int)HttpStatusCode.Forbidden, Message.UnableToEdit);
            }
            
        }

        public ActionResult GetGradeLogsByStudentRawScoreId(long studentRawScoreId)
        {
            var gradelogs = _db.GradingLogs.Where(x => x.StudentRawScoreId == studentRawScoreId).ToList();
            var studentRawScore = _db.StudentRawScores.Include(x => x.Grade)
                                                      .Include(x => x.Student)
                                                         .ThenInclude(x => x.Title)
                                                      .Include(x => x.Section)
                                                      .SingleOrDefault(x => x.Id == studentRawScoreId);

            var course = _db.Courses.SingleOrDefault(x => x.Id == studentRawScore.CourseId);
            var model = new GradeLogsViewModel
                        {
                            Student = studentRawScore.Student.CodeAndName,
                            Course = $"{course.Code} ({ studentRawScore.Section.Number }) [{ studentRawScore.Section.SectionTypes }]",
                            GradingLogs = gradelogs.Select(x => new GradingLogsViewModel
                                                                {
                                                                    PreviousGrade = x.PreviousGrade,
                                                                    CurrentGrade = x.CurrentGrade,
                                                                    CreatedAtText = x.CreatedAtText,
                                                                    CreatedAt = x.CreatedAt,
                                                                    Type = x.TypeText,
                                                                    Remark = x.Remark
                                                                })
                                                   .OrderBy(x => x.CreatedAt)
                                                   .ToList()
                        };
            return PartialView("~/Views/GradeScoreSummary/_GradeLogContent.cshtml", model);
        }

        public ActionResult Save(long courseId, long termId , string returnUrl)
        {
            var instructorId = GetInstructorId();
            var user = GetUser();
            var isGradeMember = _db.GradeMembers.Any(x => x.UserId == user.Id);
            var term = _db.Terms.SingleOrDefault(x => x.Id == termId);
            var course = _db.Courses.SingleOrDefault(x => x.Id == courseId);
            var sectionIds = _db.Sections.AsNoTracking()
                                         .Where(x => x.TermId == termId
                                                     && x.ParentSectionId == null
                                                     && x.Status == "a"
                                                     && x.CourseId == courseId
                                                     && (x.MainInstructorId == instructorId || isGradeMember))
                                         .Select(x => x.Id)
                                         .ToList();

            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var barcodeOld = _db.Barcodes.Where(x => x.CourseId == courseId && sectionIds.Contains(x.SectionId) && !x.IsPublished).ToList();
                    foreach(var item in barcodeOld)
                    {
                        item.IsActive = false;
                    }
                    _db.SaveChanges();

                    var barcodes = _gradeProvider.GenerateBarcode(termId, sectionIds, course);  
                    _db.Barcodes.AddRange(barcodes);
                    _db.SaveChanges();

                    foreach(var barcode in barcodes)
                    {
                        var jointSectionIds = _db.Sections.Where(x => x.ParentSectionId == barcode.SectionId).Select(x => x.Id).ToList();
                        jointSectionIds.Add(barcode.SectionId);
                        var studentRawScores = _db.StudentRawScores.Where(x => jointSectionIds.Contains(x.SectionId) 
                                                                               && !x.IsSkipGrading 
                                                                               && !x.RegistrationCourse.IsGradePublished)
                                                                   .ToList();
                        
                        foreach(var studentRawScore in studentRawScores)
                        {
                            studentRawScore.BarcodeId = barcode.Id;
                        }
                        _db.SaveChanges();
                    }
                    
                    
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Report),new { courseId = courseId, termId = termId, returnUrl = returnUrl});
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToSave);
                    transaction.Rollback();
                    return RedirectToAction(nameof(EditGrade),new { courseId = courseId, termId = termId, returnUrl = returnUrl});
                }
            }
        }
        public ActionResult Report(long courseId, long termId, string type = "c")
        {
            var instructorId = GetInstructorId();
            var user = GetUser();
            var isGradeMember = _db.GradeMembers.Any(x => x.UserId == user.Id);
            var term = _db.Terms.SingleOrDefault(x => x.Id == termId);

            var report = new ReportViewModel();
            var body = _gradeProvider.GetGradeScoreSummaryViewModelByCourseIdForReport(courseId, term.Id, instructorId, isGradeMember);
            if (type == "n")
            {
                body.StudentScoreAllocations = body.StudentScoreAllocations.OrderBy(x => x.StudentFirstName).ToList();
                foreach (var item in body.SectionMarkAllocations)
                {
                    item.StudentScoreAllocations = item.StudentScoreAllocations.OrderBy(x => x.StudentFirstName).ToList();
                }
            }
            else
            {
                body.StudentScoreAllocations = body.StudentScoreAllocations.OrderBy(x => x.StudentCode).ToList();
                foreach (var item in body.SectionMarkAllocations)
                {
                    item.StudentScoreAllocations = item.StudentScoreAllocations.OrderBy(x => x.StudentCode).ToList();
                }
            }
 
            report = new ReportViewModel
            {
                TermId = term.Id,
                AcademicLevelId = term.AcademicLevelId,
                Title = "Report",
                Subject = "Grading Result Report",
                Creator = "Keystone V.xxxx",
                Author = user.UserName,
                Body = body
            };

            return View(report);
        }

        private void CreateGradingSelectList(long termId, long instructorId)
        {
            ViewBag.ExaminationTypes = _gradeProvider.GetExaminationTypes();
            ViewBag.Courses = instructorId == 0 ? _selectListProvider.GetCoursesByTerm(termId)
                                                : _selectListProvider.GetTeachingCourseByInstructorId(instructorId, termId);
            ViewBag.Groups = _selectListProvider.GetStandardGradingGroups();
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}