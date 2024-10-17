using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Api;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Vereyon.Web;

namespace Keystone.Controllers
{
    //[PermissionAuthorize("ScoreByInstructor", "")]
    public class ScoreByInstructorController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;
        protected readonly IGradeProvider _gradeProvider;
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IAcademicProvider _academicProvider;

        public ScoreByInstructorController(ApplicationDbContext db,
                                           IGradeProvider gradeProvider,
                                           IMapper mapper,
                                           IFlashMessage flashMessage,
                                           ICacheProvider cacheProvider,
                                           IAcademicProvider academicProvider,
                                           ISelectListProvider selectListProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _gradeProvider = gradeProvider;
            _cacheProvider = cacheProvider;
            _academicProvider = academicProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            if (criteria.TermId == 0)
            {
                criteria.AcademicLevelId = _db.AcademicLevels.SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
                criteria.TermId = _academicProvider.GetCurrentTermByAcademicLevelId(criteria.AcademicLevelId).Id;
            }

            CreateSelectList(criteria.AcademicLevelId);
            var instructorId = GetInstructorId();
            var user = GetUser();
            var isGradeMember = _db.GradeMembers.Any(x => x.UserId == user.Id);
            ViewBag.IsGradeMember = isGradeMember;

            var parentSectionIds = _db.Sections.Where(x => x.TermId == criteria.TermId 
                                                           && x.ParentSectionId != null 
                                                           && (x.MainInstructorId == instructorId || isGradeMember)
                                                           && (string.IsNullOrEmpty(criteria.CourseCode) 
                                                               || x.Course.Code.Contains(criteria.CourseCode))
                                                           && (criteria.CourseId == 0 || x.CourseId == criteria.CourseId))
                                               .Select(x => (long)x.ParentSectionId)
                                               .ToList();

            var model = _db.Sections.Include(x => x.Course)
                                    .Where(x => x.TermId == criteria.TermId
                                               && (string.IsNullOrEmpty(criteria.CourseCode) 
                                                   || x.Course.Code.Contains(criteria.CourseCode)
                                                   || parentSectionIds.Contains(x.Id))
                                               && x.ParentSectionId == null
                                               && (x.MainInstructorId == instructorId || isGradeMember)
                                               && (criteria.CourseId == 0 || x.CourseId == criteria.CourseId)
                                               )
                                    .AsNoTracking()
                                    .Select(x => new ScoringViewModel
                                                 {
                                                     SectionId = x.Id,
                                                     CourseId = x.CourseId,
                                                     CourseAndCredit = x.Course.CourseAndCredit,
                                                     SectionNumber = x.Number
                                                 })
                                    .GroupBy(x => x.CourseId)
                                    .Select(x => new ScoringViewModel
                                                 {
                                                     CourseId = x.FirstOrDefault().CourseId,
                                                     CourseAndCredit = x.FirstOrDefault().CourseAndCredit,
                                                     SectionNumber = x.FirstOrDefault().SectionNumber
                                                 })
                                    .OrderBy(x => x.CourseAndCredit)
                                    .GetPaged(criteria, 0, true);
            
            var courseIds = model.Results.Select(x => x.CourseId).ToList();
            var sections = _db.Sections.Where(x => courseIds.Contains(x.CourseId)
                                                   && x.TermId == criteria.TermId
                                                   && (x.MainInstructorId == instructorId || isGradeMember))
                                       .ToList();

            var sectionIds = sections.Select(x => x.Id).ToList();
            var jointSections = _db.Sections.AsNoTracking()
                                            .Include(x => x.Course)
                                            .Where(x => sectionIds.Contains(x.ParentSectionId ?? 0)
                                                        && x.TermId == criteria.TermId
                                                        && (x.MainInstructorId == instructorId || isGradeMember))
                                            .ToList();

            var jointSectionIds = jointSections.Select(x => x.Id).ToList();
            var markAllocations = _db.MarkAllocations.Where(x => x.TermId == criteria.TermId
                                                                 && courseIds.Contains(x.CourseId))
                                                     .ToList();

            var barcodes = _db.Barcodes.Where(x => courseIds.Contains(x.CourseId)
                                                   && sectionIds.Contains(x.SectionId))
                                       .ToList();

            var studentRawScores = _db.StudentRawScores.Where(x => sectionIds.Contains(x.SectionId)
                                                                   || jointSectionIds.Contains(x.SectionId))
                                                       .Select(x => new
                                                                    {
                                                                        Id = x.Id,
                                                                        GradeName = x.Grade.Name,
                                                                        RegisGradeName = x.RegistrationCourse.Grade.Name,
                                                                        TotalScore = x.TotalScore,
                                                                        SectionId = x.SectionId,
                                                                        CreatedAt = x.CreatedAt
                                                                    })
                                                       .ToList();

            var registrationCourses = _db.RegistrationCourses.Where(x => (sectionIds.Contains(x.SectionId ?? 0)
                                                                          || jointSectionIds.Contains(x.SectionId ?? 0))
                                                                          && x.TermId == criteria.TermId
                                                                          && x.IsPaid
                                                                          && x.Status != "d")
                                                             .ToList();

            var newMarkAllocations = new List<MarkAllocation>();
            foreach (var item in model.Results)
            {
                var allSectionIds = new List<long>();
                var resultSectionIds = sections.Where(x => x.CourseId == item.CourseId).Select(x => x.Id).ToList();
                item.IsMarkAllocation = markAllocations.Any(x => x.CourseId == item.CourseId);

                var sectionId = sections.Where(x => x.CourseId == item.CourseId && x.ParentSectionId == null).Select(x => (long?)x.Id).ToList();
                var courseAndCreditJoint = jointSections.Where(x => sectionId.Contains(x.ParentSectionId)).GroupBy(x => x.CourseId).Select(x => x.FirstOrDefault().Course.CodeAndCredit).ToList();
                item.CourseAndCreditJoint = courseAndCreditJoint.Any() ? string.Join(", ", courseAndCreditJoint) : string.Empty;

                if (!item.IsMarkAllocation)
                {
                    var markAllocation = new MarkAllocation
                                         {
                                             CourseId = item.CourseId,
                                             TermId = criteria.TermId,
                                             Abbreviation = "FN",
                                             Name = "Final",
                                             Sequence = 1,
                                             Score = 100
                                         };
                    
                    if (!newMarkAllocations.Any(x => x.CourseId == item.CourseId))
                    {
                        newMarkAllocations.Add(markAllocation);
                    }
                    item.IsMarkAllocation = true;
                }

                item.TermId = criteria.TermId;
                var barcode = barcodes.Where(x => x.CourseId == item.CourseId
                                                      && resultSectionIds.Contains(x.SectionId)
                                                      && x.IsActive)
                                      .OrderByDescending(x => x.CreatedAt)
                                      .FirstOrDefault();                
                if (barcode != null)
                {
                    item.IsBarcode = true;
                    item.SubmitDate = barcode.CreatedAt.AddHours(7);
                }

                var resultJointSectionIds = jointSections.Where(x => resultSectionIds.Contains(x.ParentSectionId ?? 0))
                                                         .Select(x => x.Id)
                                                         .ToList();

                allSectionIds.AddRange(resultSectionIds);
                allSectionIds.AddRange(resultJointSectionIds);
                var studentRawScoresBySectionIds = studentRawScores.Where(x => allSectionIds.Contains(x.SectionId)).ToList();
                var registraitonCourseBySectionIds = registrationCourses.Where(x => allSectionIds.Contains(x.SectionId ?? 0)).ToList();

                var studentScore = studentRawScoresBySectionIds.Distinct()
                                                                .Where(x => x.TotalScore != null
                                                                            && x.GradeName?.ToLower() != "w"
                                                                            && x.GradeName?.ToLower() != "i"
                                                                            && x.GradeName?.ToLower() != "au"
                                                                            && x.RegisGradeName?.ToLower() != "i"
                                                                            && x.RegisGradeName?.ToLower() != "au"
                                                                            && x.RegisGradeName?.ToLower() != "w")
                                                                .ToList();

                item.ScoreStudent = studentScore.Count() ;

                item.GradeAuOrIStudent = studentRawScoresBySectionIds.Distinct()
                                                                     .Count(x => x.GradeName?.ToLower() == "i"
                                                                                 || x.GradeName?.ToLower() == "au");

                // item.Skip = studentRawScores.Distinct().Count(x => allSectionIds.Contains(x.SectionId) 
                //                                                    && x.TotalScore == null
                //                                                    && x.GradeName?.ToLower() != "w"
                //                                                    && x.GradeName?.ToLower() != "i"
                //                                                    && x.GradeName?.ToLower() != "au"
                //                                                    && x.RegisGradeName?.ToLower() != "w"
                //                                                    && x.RegisGradeName?.ToLower() != "i"
                //                                                    && x.RegisGradeName?.ToLower() != "au");

                // item.PublishedStudent = studentRawScores.Where(x => allSectionIds.Contains(x.SectionId)
                //                                                     && x.RegistrationCourse.IsGradePublished
                //                                                     && x.GradeName?.ToLower() != "i"
                //                                                     && x.GradeName?.ToLower() != "au"
                //                                                     && x.RegisGradeName?.ToLower() != "w")
                //                                         .Select(x => x.StudentId)
                //                                         .Distinct()
                //                                         .Count();

                item.WithdrawnStudent = registraitonCourseBySectionIds.Distinct()
                                                                      .Count(x => allSectionIds.Contains(x.SectionId ?? 0)
                                                                                  && x.GradeName?.ToLower() == "w");

                item.TotalStudent = registraitonCourseBySectionIds.Distinct().Count();

                item.LastGradeDate = studentScore.Any() ? studentScore.Max(x => x.CreatedAt) : null;
                item.LastGradeDate = item.LastGradeDate.HasValue ? item.LastGradeDate.Value.AddHours(7) : null;
            }
            if(newMarkAllocations.Any())
            {
                _db.MarkAllocations.AddRange(newMarkAllocations);
                _db.SaveChanges();
            }
            return View(model);
        }

        public IActionResult Edit(long courseId, long termId, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            var result = GetStudentScoreListByCourseAndTerm(courseId, termId);
            return View(result);
        }

        //[PermissionAuthorize("ScoreByInstructor", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = int.MaxValue)]
        public ActionResult Edit(EditScoringViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (model.StudentRawScores.Any())
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        var studentRawScores = new List<StudentRawScore>();
                        var studentIds = model.StudentRawScores.Where(x => !x.IsSkipGrading 
                                                                           //&& x.Id == 0 
                                                                           && (x.GradeTemplateId == null || x.GradeTemplateId == 0))
                                                              .Select(x => x.StudentId).ToList();
                        var courseIds = model.StudentRawScores.Select(x => x.CourseId).Distinct().ToList();
                        var academicInformations = _db.AcademicInformations.AsNoTracking()
                                                                           .Where(x => studentIds.Contains(x.StudentId))
                                                                           .ToList();

                        var curriculumVersionIds = academicInformations.Select(x => x.CurriculumVersionId)
                                                                       .ToList();
                        var courseGroups = _db.CourseGroups.AsNoTracking()
                                                           .Where(x => curriculumVersionIds.Contains(x.CurriculumVersionId))
                                                           .Select(x => new
                                                                        {
                                                                            Id = x.Id,
                                                                            CurriculumVersionId = x.CurriculumVersionId
                                                                        })
                                                           .ToList();
                        var gradeCurves = _gradeProvider.GetGradingCurveByCourseIdAndTermId(model.CourseId, model.TermId); 

                        var courseTempleteDefault = _db.Courses.AsNoTracking()
                                                               .Where(x => courseIds.Contains(x.Id))
                                                               .Select(x => new
                                                                            {
                                                                                Id = x.Id,
                                                                                GradeTemplateId = x.GradeTemplateId
                                                                            })
                                                               .ToList();

                        var gradeNotEditScore = _db.Grades.Where(x => x.Name.ToUpper() == "I" 
                                                                    || x.Name.ToUpper() == "AU" )
                                                        .Select(x => x.Id)
                                                        .ToList();

                        foreach (var item in model.StudentRawScores)
                        {
                            if (!item.IsGradePublish)
                            {
                                if (item.Id != 0)
                                {
                                    var studentRawScore = _db.StudentRawScores.SingleOrDefault(x => x.Id == item.Id);
                                    long? gradeTemplateId = studentRawScore.GradeTemplateId;
                                    studentRawScore.TotalScore = item.IsSkipGrading || !item.StudentRawScoreDetails.Any(x => x.Score != null) ? null : item.StudentRawScoreDetails.Sum(x => x.Score);
                                    studentRawScore.IsSkipGrading = item.IsSkipGrading;
                                    studentRawScore.GradeId = item.GradeId != 0 ? item.GradeId : null;
                                    if (!item.IsSkipGrading && !gradeNotEditScore.Any(x => x == studentRawScore.GradeId))
                                    {
                                        if (gradeTemplateId == null || gradeTemplateId == 0)
                                        {
                                            var academicInformation = academicInformations.SingleOrDefault(x => x.StudentId == studentRawScore.StudentId);
                                            var courseGroupIds = courseGroups.Where(x => x.CurriculumVersionId == academicInformation.CurriculumVersionId).Select(x => x.Id).ToList();
                                            gradeTemplateId = _db.CurriculumCourses.AsNoTracking()
                                                                                   .Where(x => courseGroupIds.Contains(x.CourseGroupId)
                                                                                               && x.CourseId == studentRawScore.CourseId)
                                                                                   .FirstOrDefault()?.GradeTemplateId ?? 0;
                                            if (gradeTemplateId == 0)
                                            {
                                                gradeTemplateId = courseTempleteDefault.SingleOrDefault(x => x.Id == studentRawScore.CourseId).GradeTemplateId ?? 0;
                                            }
                                            studentRawScore.GradeTemplateId = gradeTemplateId;
                                        }
                                        var gradeCurve = gradeCurves.Where(x => x.GradeTemplateId == gradeTemplateId)
                                                                    .ToList();

                                        studentRawScore.GradeId = gradeCurve.Where(x => x.Minimum <= studentRawScore.TotalScore 
                                                                                        && x.Maximum + 1 > studentRawScore.TotalScore)
                                                                            .FirstOrDefault()?.GradeId ?? null;
                                    }
                                    else if (item.GradeId == 0)
                                    {
                                        studentRawScore.GradeId = null;
                                    }
                                    _db.SaveChanges();
                                    var studentRawScoreDetail = _db.StudentRawScoreDetails.Where(x => x.StudentRawScoreId == item.Id)
                                                                                          .ToList();
                                    _db.StudentRawScoreDetails.RemoveRange(studentRawScoreDetail);
                                    studentRawScoreDetail = item.StudentRawScoreDetails.Select(x => new StudentRawScoreDetail
                                                                                                    {
                                                                                                        MarkAllocationId = x.MarkAllocationId,
                                                                                                        Score = item.IsSkipGrading ? 0 : x.Score,
                                                                                                        StudentRawScoreId = item.Id
                                                                                                    }).ToList();

                                    _db.StudentRawScoreDetails.AddRange(studentRawScoreDetail);
                                    _db.SaveChanges();
                                }
                                else
                                {
                                    var studentRawScore = new StudentRawScore
                                                          {
                                                              RegistrationCourseId = item.RegistrationCourseId,
                                                              CourseId = item.CourseId,
                                                              SectionId = item.SectionId,
                                                              TotalScore = item.IsSkipGrading ? 0 : item.TotalScore,
                                                              IsSkipGrading = item.IsSkipGrading,
                                                              StudentId = item.StudentId,
                                                              GradeId = item.GradeId != 0 ? item.GradeId : null
                                                          };

                                    if(!item.IsSkipGrading && !gradeNotEditScore.Any(x => x == studentRawScore.GradeId))
                                    {
                                        var academicInformation = academicInformations.SingleOrDefault(x => x.StudentId == studentRawScore.StudentId);
                                        var courseGroupIds = courseGroups.Where(x => x.CurriculumVersionId == academicInformation.CurriculumVersionId).Select(x => x.Id).ToList();
                                        var gradeTemplateId = _db.CurriculumCourses.AsNoTracking()
                                                                                   .Where(x => courseGroupIds.Contains(x.CourseGroupId)
                                                                                               && x.CourseId == studentRawScore.CourseId)
                                                                                   .FirstOrDefault()?.GradeTemplateId ?? 0;
                                        if(gradeTemplateId == 0)
                                        {
                                            gradeTemplateId = courseTempleteDefault.SingleOrDefault(x => x.Id == studentRawScore.CourseId).GradeTemplateId ?? 0;
                                        }
                                        studentRawScore.GradeTemplateId = gradeTemplateId;

                                        if (studentRawScore.TotalScore == null)
                                        {
                                            studentRawScore.TotalScore = item.IsSkipGrading || !item.StudentRawScoreDetails.Any(x => x.Score != null) ? null : item.StudentRawScoreDetails.Sum(x => x.Score);
                                        }

                                        var gradeCurve = gradeCurves.Where(x => x.GradeTemplateId == gradeTemplateId)
                                                                    .ToList();

                                        studentRawScore.GradeId = gradeCurve.Where(x => x.Minimum <= studentRawScore.TotalScore 
                                                                                        && x.Maximum + 1 > studentRawScore.TotalScore)
                                                                            .FirstOrDefault()?.GradeId ?? null;
                                    }
                                    else if(item.GradeId == 0)
                                    {
                                        studentRawScore.GradeId = null;
                                    }
                                    _db.StudentRawScores.Add(studentRawScore);
                                    var studentRawScoreDetail = item.StudentRawScoreDetails.Select(x => new StudentRawScoreDetail
                                                                                                        {
                                                                                                            MarkAllocationId = x.MarkAllocationId,
                                                                                                            Score = item.IsSkipGrading ? 0 : x.Score,
                                                                                                            StudentRawScoreId = studentRawScore.Id
                                                                                                        }).ToList();
                                    _db.StudentRawScoreDetails.AddRange(studentRawScoreDetail);
                                    _db.SaveChanges();
                                }
                            }
                        }
                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        if(model.IsNext)
                        {
                            return RedirectToAction("EditGrade", "GradeScoreSummary", new {
                                                                                         CourseId = model.CourseId,
                                                                                         TermId = model.TermId,
                                                                                         ReturnUrl = returnUrl
                                                                                     });
                        }
                        else
                        {
                            return RedirectToAction(nameof(Edit), new {
                                                                        CourseId = model.CourseId,
                                                                        TermId = model.TermId,
                                                                        ReturnUrl = returnUrl
                                                                      });
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.UnableToCreate);
                        return RedirectToAction(nameof(Edit), new {
                                                                    CourseId = model.CourseId,
                                                                    TermId = model.TermId,
                                                                    ReturnUrl = returnUrl
                                                                  });
                    }
                }
            }

            return RedirectToAction(nameof(Edit), new {
                                                          CourseId = model.CourseId,
                                                          TermId = model.TermId,
                                                          ReturnUrl = returnUrl
                                                      });
        }

        //[PermissionAuthorize("ScoreByInstructor", PolicyGenerator.Write)]
        [RequestFormLimits(ValueCountLimit = 100000)]
        public IActionResult SaveImport(ImportScoringViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (model.ImportSuccess.Any())
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        var studentRawScores = new List<StudentRawScore>();
                        var studentIds = model.ImportSuccess.Where(x => x.GradeTemplateId == null || x.GradeTemplateId == 0).Select(x => x.StudentId).ToList();
                        var courseIds = model.ImportSuccess.Select(x => x.CourseId).Distinct().ToList();
                        var academicInformations = _db.AcademicInformations.AsNoTracking()
                                                                           .Where(x => studentIds.Contains(x.StudentId))
                                                                           .ToList();

                        var curriculumVersionIds = academicInformations.Select(x => x.CurriculumVersionId)
                                                                       .ToList();
                        var courseGroups = _db.CourseGroups.AsNoTracking()
                                                           .Where(x => curriculumVersionIds.Contains(x.CurriculumVersionId))
                                                           .Select(x => new
                                                                        {
                                                                            Id = x.Id,
                                                                            CurriculumVersionId = x.CurriculumVersionId
                                                                        })
                                                           .ToList();
                        var gradeCurves = _db.GradingCurves.AsNoTracking()
                                                           .Where(x => x.TermId == model.TermId
                                                                       && model.CourseId == x.CourseId)
                                                           .ToList();

                        var courseTempleteDefault = _db.Courses.AsNoTracking()
                                                               .Where(x => courseIds.Contains(x.Id))
                                                               .Select(x => new
                                                                            {
                                                                                Id = x.Id,
                                                                                GradeTemplateId = x.GradeTemplateId
                                                                            })
                                                               .ToList();

                        foreach (var item in model.ImportSuccess)
                        {
                            if (!item.IsGradePublish)
                            {
                                if (item.Id != 0)
                                {
                                    var studentRawScore = _db.StudentRawScores.SingleOrDefault(x => x.Id == item.Id);
                                    long? gradeTemplateId = studentRawScore.GradeTemplateId;
                                    studentRawScore.TotalScore = item.IsSkipGrading ? null : item.TotalScore;
                                    studentRawScore.IsSkipGrading = item.IsSkipGrading;
                                    if(!item.IsSkipGrading && item.TotalScore != null)
                                    {
                                        if (gradeTemplateId == null || gradeTemplateId == 0)
                                        {
                                            var academicInformation = academicInformations.SingleOrDefault(x => x.StudentId == studentRawScore.StudentId);
                                            var courseGroupIds = courseGroups.Where(x => x.CurriculumVersionId == academicInformation.CurriculumVersionId).Select(x => x.Id).ToList();
                                            gradeTemplateId = _db.CurriculumCourses.AsNoTracking()
                                                                                .Where(x => courseGroupIds.Contains(x.CourseGroupId)
                                                                                            && x.CourseId == studentRawScore.CourseId)
                                                                                .FirstOrDefault()?.GradeTemplateId ?? 0;
                                            if (gradeTemplateId == 0)
                                            {
                                                gradeTemplateId = courseTempleteDefault.SingleOrDefault(x => x.Id == studentRawScore.CourseId).GradeTemplateId ?? 0;
                                            }
                                            studentRawScore.GradeTemplateId = gradeTemplateId;
                                        }
                                        var gradeCurve = gradeCurves.Where(x => x.GradeTemplateId == gradeTemplateId)
                                                                    .ToList();

                                        studentRawScore.GradeId = gradeCurve.Where(x => x.Minimum <= studentRawScore.TotalScore 
                                                                                        && x.Maximum + 1 > studentRawScore.TotalScore)
                                                                            .FirstOrDefault()?.GradeId ?? null;
                                    }
                                    else
                                    {
                                        studentRawScore.GradeId = null;
                                    }
                                    _db.SaveChanges();
                                    var studentRawScoreDetail = _db.StudentRawScoreDetails.Where(x => x.StudentRawScoreId == item.Id)
                                                                                          .ToList();
                                    _db.StudentRawScoreDetails.RemoveRange(studentRawScoreDetail);
                                    studentRawScoreDetail = item.StudentRawScoreDetails.Select(x => new StudentRawScoreDetail
                                                                                                    {
                                                                                                        MarkAllocationId = x.MarkAllocationId,
                                                                                                        Score = item.IsSkipGrading ? 0 : x.Score,
                                                                                                        StudentRawScoreId = item.Id
                                                                                                    }).ToList();

                                    _db.StudentRawScoreDetails.AddRange(studentRawScoreDetail);
                                    _db.SaveChanges();
                                }
                                else
                                {
                                    var studentRawScore = new StudentRawScore
                                                          {
                                                              RegistrationCourseId = item.RegistrationCourseId,
                                                              CourseId = item.CourseId,
                                                              SectionId = item.SectionId,
                                                              TotalScore = item.IsSkipGrading ? 0 : item.TotalScore,
                                                              IsSkipGrading = item.IsSkipGrading,
                                                              StudentId = item.StudentId
                                                          };

                                    if(!item.IsSkipGrading && item.TotalScore != null)
                                    {
                                        var academicInformation = academicInformations.SingleOrDefault(x => x.StudentId == studentRawScore.StudentId);
                                        var courseGroupIds = courseGroups.Where(x => x.CurriculumVersionId == academicInformation.CurriculumVersionId).Select(x => x.Id).ToList();
                                        var gradeTemplateId = _db.CurriculumCourses.AsNoTracking()
                                                                                   .Where(x => courseGroupIds.Contains(x.CourseGroupId)
                                                                                               && x.CourseId == studentRawScore.CourseId)
                                                                                   .FirstOrDefault()?.GradeTemplateId ?? 0;
                                        if (gradeTemplateId == 0)
                                        {
                                            gradeTemplateId = courseTempleteDefault.SingleOrDefault(x => x.Id == studentRawScore.CourseId).GradeTemplateId ?? 0;
                                        }
                                        studentRawScore.GradeTemplateId = gradeTemplateId;

                                        var gradeCurve = gradeCurves.Where(x => x.GradeTemplateId == gradeTemplateId)
                                                                    .ToList();

                                        studentRawScore.GradeId = gradeCurve.Where(x => x.Minimum <= studentRawScore.TotalScore 
                                                                                        && x.Maximum + 1 > studentRawScore.TotalScore)
                                                                            .FirstOrDefault()?.GradeId ?? null;
                                    }
                                    else
                                    {
                                        studentRawScore.GradeId = null;
                                    }
                                    _db.StudentRawScores.Add(studentRawScore);
                                    var studentRawScoreDetail = item.StudentRawScoreDetails.Select(x => new StudentRawScoreDetail
                                                                                                        {
                                                                                                            MarkAllocationId = x.MarkAllocationId,
                                                                                                            Score = item.IsSkipGrading ? null : x.Score,
                                                                                                            StudentRawScoreId = studentRawScore.Id
                                                                                                        }).ToList();
                                    _db.StudentRawScoreDetails.AddRange(studentRawScoreDetail);
                                    _db.SaveChanges();
                                }
                            }
                        }
                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Edit), new {
                                                                    CourseId = model.CourseId,
                                                                    TermId = model.TermId,
                                                                    ReturnUrl = returnUrl
                                                                  });
                    }
                    catch
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.UnableToCreate);
                        return RedirectToAction(nameof(Edit), new {
                                                                    CourseId = model.CourseId,
                                                                    TermId = model.TermId,
                                                                    ReturnUrl = returnUrl
                                                                  });
                    }
                }
            }

            return RedirectToAction(nameof(Edit), new {
                                                          CourseId = model.CourseId,
                                                          TermId = model.TermId,
                                                          ReturnUrl = returnUrl
                                                      });
        }
        public IActionResult GetStudentsBySections(List<long> sectionIds, long courseId, long termId)
        {
            CreateSelectList();
            var sectionIdsNullable = sectionIds.Select(x => (long?)x).ToList();
            var course = _db.Courses.SingleOrDefault(x => x.Id == courseId);
            var markAllocations = _db.MarkAllocations.Where(x => x.TermId == termId
                                                                && x.CourseId == courseId)
                                                     .OrderBy(x => x.Sequence)
                                                     .ToList();
            var studentRawScoreDetails = markAllocations.Select(x => new StudentRawScoreDetail
                                                                     {  
                                                                         MarkAllocationId = x.Id
                                                                     })
                                                        .ToList(); 
            var sections = _db.Sections.Where(x => sectionIds.Contains(x.Id))
                                       .Select(x => new
                                                    {
                                                        SectionId = x.Id,
                                                        SeatUsed = x.SeatUsed,
                                                        SectionNumber = x.Number,
                                                        CourseName = course.NameEn,
                                                        CourseCode = course.CodeAndSpecialChar
                                                    })
                                       .ToList();

            var jointSections = _db.Sections.Where(x => sectionIdsNullable.Contains(x.ParentSectionId))
                                            .Select(x => new 
                                                         {
                                                             Id = x.Id,
                                                             ParentSectionId = x.ParentSectionId,
                                                             Number = x.Number,
                                                             SeatUsed = x.SeatUsed
                                                         })
                                            .ToList();

            sectionIdsNullable.AddRange(jointSections.Select(x => (long?)x.Id).ToList());

            var studentResultRawScore = _gradeProvider.GetStudentRawScoresBySections(sectionIdsNullable, studentRawScoreDetails);
            var sectionSearch = new List<SectionSearchViewModel>();
            foreach (var item in sections)
            {
                sectionSearch.Add(new SectionSearchViewModel
                                  {
                                      SectionId = item.SectionId,
                                      SectionNumber = item.SectionNumber,
                                      CourseCode = item.CourseCode,
                                      CourseName = item.CourseName,
                                      IsSelected = true,
                                      TotalStudent = item.SeatUsed + jointSections.Where(x => x.ParentSectionId == item.SectionId).Sum(x => x.SeatUsed),
                                      TotalWithdrawn = studentResultRawScore.Count(x => x.SectionId == item.SectionId 
                                                                                       && x.IsWithdrawal)
                                  });
            }

            var result = new EditScoringViewModel
                         {
                             CourseId = courseId,
                             TermId = termId,
                             StudentRawScores = studentResultRawScore.OrderByDescending(x => x.SectionType)
                                                                        .ThenBy(x => x.CourseCode)
                                                                        .ThenBy(x => x.SectionNumber)
                                                                        .ThenBy(x => x.StudentCode)
                                                                     .ToList(),
                             Allocations = markAllocations,
                             Sections = sectionSearch
                         };
            return PartialView("~/Views/ScoreByInstructor/_ScoringTable.cshtml", result);
        }

        [HttpPost]
        //[Consumes("application/x-www-form-urlencoded")]
        [RequestFormLimits(ValueCountLimit = 100000)]
        public IActionResult Export([FromForm] EditScoringViewModel Model)
        {
            string handle = string.Empty;
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    IXLWorksheet worksheet = workbook.Worksheets.Add("Scores");
                    worksheet.Cell(1, 1).Value = "Course Code";
                    worksheet.Cell(1, 2).Value = "Section";
                    worksheet.Cell(1, 3).Value = "Section Type";
                    worksheet.Cell(1, 4).Value = "Student Code";
                    worksheet.Cell(1, 5).Value = "Title";
                    worksheet.Cell(1, 6).Value = "First Name";
                    worksheet.Cell(1, 7).Value = "Middle Name";
                    worksheet.Cell(1, 8).Value = "Last Name";
                    worksheet.Cell(1, 9).Value = "Withdrawal";
                    worksheet.Cell(1, 10).Value = "GradePublish";
                    int column = 11;
                    foreach (var item in Model.Allocations)
                    {
                        worksheet.Cell(1, column++).Value = item.Abbreviation;
                    }

                    int row = 2;
                    foreach (var item in Model.StudentRawScores)
                    {
                        worksheet.Cell(row, 1).Value = item.CourseCode;
                        worksheet.Cell(row, 2).Value = item.SectionNumber;
                        worksheet.Cell(row, 3).Value = item.SectionType;
                        worksheet.Cell(row, 4).Value = item.StudentCode;
                        worksheet.Cell(row, 5).Value = item.StudentTitle;
                        worksheet.Cell(row, 6).Value = item.StudentFirstNameEn;
                        worksheet.Cell(row, 7).Value = item.StudentMidNameEn;
                        worksheet.Cell(row, 8).Value = item.StudentLastNameEn;
                        worksheet.Cell(row, 9).Value = item.IsWithdrawal;
                        worksheet.Cell(row, 10).Value = item.IsGradePublish;
                        column = 11;
                        foreach (var score in item.StudentRawScoreDetails)
                        {
                            worksheet.Cell(row, column++).Value = score.Score;
                        }
                        row++;
                    }

                    //using (var stream = new MemoryStream())
                    //{
                    //    workbook.SaveAs(stream);
                    //    handle = Guid.NewGuid().ToString();
                    //    stream.Position = 0;
                    //    TempData[handle] = stream.ToArray();
                    //}
                    return workbook.Deliver("Student scoring.xlsx");
                }
            }
            catch { }

            var response = new ApiResponse<string>
            {
                StatusCode = string.IsNullOrEmpty(handle) ? HttpStatusCode.Forbidden : HttpStatusCode.OK,
                Result = handle
            };

            return Ok(response);
        }

        [HttpGet]
        public virtual ActionResult Download(string fileGuid)
        {
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = "Student scoring.xlsx";
            if (TempData[fileGuid] != null)
            {
                byte[] content = TempData[fileGuid] as byte[];
                return File(content, contentType, fileName);
            }
            else
            {
                return new EmptyResult();
            }
        }

        //[PermissionAuthorize("ScoreByInstructor", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = 100000)]
        public IActionResult Import(EditScoringViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ModelState.Clear();
            var extensions = new List<string> { ".xlsx", ".xls", ".csv" };
            var studentImportSuccess = new List<StudentRawScoreViewModel>();
            var studentImportWarning = new List<ImportScoreFailViewModel>();
            var studentImportFail = new List<ImportScoreFailViewModel>();
            var gradeNotEditScore = _db.Grades.Where(x => x.Name.ToUpper() == "I" 
                                                          || x.Name.ToUpper() == "AU" )
                                              .Select(x => x.Id)
                                              .ToList();
            if (string.IsNullOrEmpty(model.UploadFile?.FileName)
                || !extensions.Contains(Path.GetExtension(model.UploadFile.FileName)))
            {
                _flashMessage.Danger("Invalid file.");
                return RedirectToAction(nameof(Edit), new {
                                                            CourseId = model.CourseId,
                                                            TermId = model.TermId,
                                                            ReturnUrl = returnUrl
                                                        });
            }
            else
            {
                try
                {
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    using (var stream = model.UploadFile.OpenReadStream())
                    {
                        using (var reader = GetExcelDataReader(Path.GetExtension(model.UploadFile.FileName), stream))
                        {
                            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                                {
                                    UseHeaderRow = false
                                }
                            });
                            model = GetStudentScoreListByCourseAndTerm(model.CourseId, model.TermId);
                            for (int i = 0; i < result.Tables.Count; i++)
                            {
                                int studentCodeIndex = -1;
                                int courseCodeIndex = -1;
                                int sectionIndex = -1;
                                for(int j = 0 ; j < result.Tables[i].Columns.Count; j++)
                                {
                                    if(result.Tables[i].Rows[0][j]?.ToString() == "Student Code")
                                    {
                                        studentCodeIndex = j;
                                    }
                                    else if(result.Tables[i].Rows[0][j]?.ToString() == "Course Code")
                                    {
                                        courseCodeIndex = j;
                                    }
                                    else if(result.Tables[i].Rows[0][j]?.ToString() == "Section")                                           
                                    {
                                        sectionIndex = j;
                                    }
                                }
                                if(studentCodeIndex != -1 && courseCodeIndex != -1 && sectionIndex != -1)
                                {
                                    for (int j = 1; j < result.Tables[i].Rows.Count; j++)
                                    {
                                        var message = string.Empty;
                                        string courseCode = result.Tables[i].Rows[j][courseCodeIndex]?.ToString();
                                        string sectionNumber = result.Tables[i].Rows[j][sectionIndex]?.ToString();
                                        string studentCode = result.Tables[i].Rows[j][studentCodeIndex]?.ToString();
                                        var studentRawScore = model.StudentRawScores.FirstOrDefault(x => x.CourseCode == courseCode
                                                                                                         && x.SectionNumber == sectionNumber
                                                                                                         && x.StudentCode == studentCode);

                                        if (studentCode != null && studentRawScore != null)
                                        {
                                            if(!studentRawScore.IsGradePublish && !studentRawScore.IsSkipGrading && !gradeNotEditScore.Any(x => x == studentRawScore.GradeId))
                                            {
                                                for (int k = 0; k < result.Tables[i].Columns.Count; k++)
                                                {
                                                    var header = result.Tables[i].Rows[0][k]?.ToString();
                                                    long markAllowcationId = model.Allocations.FirstOrDefault(x => x.Abbreviation == header)?.Id ?? 0;
                                                    var maxScore = model.Allocations.FirstOrDefault(x => x.Abbreviation == header)?.Score ?? 0;
                                                    if (studentRawScore.StudentRawScoreDetails.Any(x => x.MarkAllocationId == markAllowcationId))
                                                    {
                                                        decimal? scoreCheck = null;
                                                        if(!string.IsNullOrEmpty(result.Tables[i].Rows[j][k]?.ToString()))
                                                        {
                                                            scoreCheck = Math.Round(Convert.ToDecimal(result.Tables[i].Rows[j][k]), 2);
                                                            if(Convert.ToDecimal(result.Tables[i].Rows[j][k]) > maxScore)
                                                            {
                                                                message = $"index excel = { j }, { header } = {Convert.ToDecimal(result.Tables[i].Rows[j][k])} over max score { maxScore }";
                                                            }
                                                            else
                                                            {
                                                                studentRawScore.StudentRawScoreDetails.FirstOrDefault(x => x.MarkAllocationId == markAllowcationId).Score = scoreCheck;
                                                            }
                                                        }
                                                    }
                                                }
                                                studentRawScore.TotalScore = studentRawScore.StudentRawScoreDetails == null ? null : studentRawScore.StudentRawScoreDetails.Any(x => x.Score != null) 
                                                                                                                                   ? studentRawScore.StudentRawScoreDetails.Where(x => x.Score != null).Sum(x => x.Score) : null;
                                            }
                                            else
                                            {
                                                message = $"index excel = { j }, grade publish or grade = I, AU ,W";
                                            }
                                        }
                                        else
                                        {
                                            message = "index excel = " + j + ", student or course or section not found or invalid";
                                        }

                                        if (string.IsNullOrEmpty(message))
                                        {
                                            studentImportSuccess.Add(studentRawScore);
                                        }
                                        else if (studentRawScore.IsGradePublish 
                                                 || studentRawScore.IsSkipGrading 
                                                 || gradeNotEditScore.Any(x => x == studentRawScore.GradeId) 
                                                 || studentRawScore.IsWithdrawal)
                                        {
                                            studentImportWarning.Add(new ImportScoreFailViewModel
                                                                  {
                                                                      StudentCode = studentCode,
                                                                      CourseCode = courseCode,
                                                                      SectionNumber = sectionNumber,
                                                                      Message = message
                                                                  });
                                        }
                                        else 
                                        {
                                            studentImportFail.Add(new ImportScoreFailViewModel
                                                                  {
                                                                      StudentCode = studentCode,
                                                                      CourseCode = courseCode,
                                                                      SectionNumber = sectionNumber,
                                                                      Message = message
                                                                  });
                                        }
                                    }
                                }
                                else
                                {
                                    _flashMessage.Danger(Message.InvalidFormatExcel);
                                    return RedirectToAction(nameof(Edit), new {
                                                                                CourseId = model.CourseId,
                                                                                TermId = model.TermId,
                                                                                ReturnUrl = returnUrl
                                                                            });
                                }
                            }
                            var importScoreModel = new ImportScoringViewModel
                                                   {
                                                       TermId = model.TermId,
                                                       TermText = model.TermText,
                                                       CourseAndCredit = model.CourseAndCredit,
                                                       CourseId = model.CourseId,
                                                       MainInstructorFullNameEn = model.MainInstructorFullNameEn,
                                                       Allocations = model.Allocations,
                                                       ImportSuccess = studentImportSuccess,
                                                       ImportWarning = studentImportWarning,
                                                       ImportFail = studentImportFail
                                                   };

                            if(studentImportFail.Any())
                            {
                                _flashMessage.Danger(Message.UnableToSave);
                            }
                                                        
                            return View(importScoreModel);
                        }
                    }
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                    return RedirectToAction(nameof(Edit), new {
                                                                CourseId = model.CourseId,
                                                                TermId = model.TermId,
                                                                ReturnUrl = returnUrl
                                                              });
                }
            }
        }

        public IActionResult GetGrade(long id)
        {
            var grade = _gradeProvider.GetGradeById(id);
            return Ok(grade);
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Grades = _selectListProvider.GetGradingByPageScoreInstructor();
            ViewBag.Courses = _selectListProvider.GetCourses();
            if(academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
            ViewBag.Courses = _selectListProvider.GetCourses();
        }

        private EditScoringViewModel GetStudentScoreListByCourseAndTerm(long courseId, long termId)
        {
            var course = _db.Courses.SingleOrDefault(x => x.Id == courseId);
            var term = _db.Terms.SingleOrDefault(x => x.Id == termId);
            var instructorId = GetInstructorId();
            var instructor = _db.Instructors.Include(x => x.Title).SingleOrDefault(x => x.Id == instructorId);
            var user = GetUser();
            var isGradeMember = _db.GradeMembers.Any(x => x.UserId == user.Id);
            var markAllocations = _db.MarkAllocations.Where(x => x.TermId == termId
                                                                && x.CourseId == courseId)
                                                     .OrderBy(x => x.Sequence)
                                                     .ToList();
            var studentRawScoreDetails = markAllocations.Select(x => new StudentRawScoreDetail
                                                                     {  
                                                                         MarkAllocationId = x.Id
                                                                     })
                                                        .ToList(); 
            var sections = _db.Sections.AsNoTracking()
                                       .Where(x => x.TermId == termId
                                                   && x.ParentSectionId == null
                                                   && x.Status == "a"
                                                   && x.CourseId == courseId
                                                   && (x.MainInstructorId == instructorId || isGradeMember))
                                        .Select(x => new
                                                     {
                                                         SectionId = x.Id,
                                                         SeatUsed = x.SeatUsed,
                                                         SectionNumber = x.Number,
                                                         CourseName = course.NameEn,
                                                         CourseCode = course.CodeAndSpecialChar,
                                                         MainInstructorId = x.MainInstructorId
                                                     })
                                        .ToList();

            var sectionIdsNullable = sections.Select(x => (long?)x.SectionId).ToList();

            var jointSections = _db.Sections.AsNoTracking()
                                            .Where(x => sectionIdsNullable.Contains(x.ParentSectionId))
                                            .Select(x => new 
                                                         {
                                                             Id = x.Id,
                                                             ParentSectionId = x.ParentSectionId,
                                                             Number = x.Number,
                                                             SeatUsed = x.SeatUsed
                                                         })
                                            .ToList();

            sectionIdsNullable.AddRange(jointSections.Select(x => (long?)x.Id).ToList());
            var studentResultRawScore = _gradeProvider.GetStudentRawScoresBySections(sectionIdsNullable, studentRawScoreDetails);
            var sectionSearch = new List<SectionSearchViewModel>();
            foreach (var item in sections)
            {
                var jointSectionIds = jointSections.Where(x => x.ParentSectionId == item.SectionId).Select(x => x.Id).ToList();
                sectionSearch.Add(new SectionSearchViewModel
                                  {
                                      SectionId = item.SectionId,
                                      SectionNumber = item.SectionNumber,
                                      CourseCode = item.CourseCode,
                                      CourseName = item.CourseName,
                                      IsSelected = true,
                                      TotalStudent = item.SeatUsed + jointSections.Where(x => x.ParentSectionId == item.SectionId).Sum(x => x.SeatUsed),
                                      TotalWithdrawn = studentResultRawScore.Count(x => (x.SectionId == item.SectionId 
                                                                                        || jointSectionIds.Contains(x.SectionId))
                                                                                        && x.IsWithdrawal)
                                  });
            }

            var mainInstructorIds = sections.Select(x => x.MainInstructorId ?? 0).Distinct().ToList();

            var mainInstructor = _db.Instructors.Where(x => mainInstructorIds.Contains(x.Id))
                                                .Select(x => x.Title.NameEn + " " + x.FirstNameEn + " " + x.LastNameEn)
                                                .ToList();

            var result = new EditScoringViewModel
                         {
                             CourseId = courseId,
                             TermId = termId,
                             TermText = term.TermText,
                             CourseAndCredit = course.CourseAndCredit,
                             MainInstructorFullNameEn = string.Join(", ", mainInstructor),
                             StudentRawScores = studentResultRawScore,
                             Allocations = markAllocations,
                             Sections = sectionSearch
                         };

            return result;
        }

        private IExcelDataReader GetExcelDataReader(string extensions, Stream stream)
        {
            if (extensions.Contains("csv"))
            {
                return ExcelReaderFactory.CreateCsvReader(stream);
            }
            
            return ExcelReaderFactory.CreateReader(stream);
        }
    }
}