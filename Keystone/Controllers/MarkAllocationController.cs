using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    public class MarkAllocationController : BaseController
    {
        public MarkAllocationController(ApplicationDbContext db,
                                        IFlashMessage flashMessage,
                                        ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public ActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var instructorId = GetInstructorId();
            var markAllocations = _db.MarkAllocations.Where(x => x.TermId == criteria.TermId);
            var model = _db.Sections.Include(x => x.Course)
                                    .Where(x => x.TermId == criteria.TermId
                                                && x.MainInstructorId == instructorId
                                                && x.ParentSectionId == null)
                                    .Select(x => new MarkAllocationViewModel
                                                 {
                                                     CourseId = x.CourseId,
                                                     Course = x.Course.CourseAndCredit
                                                 })
                                    .GroupBy(x => x.CourseId)
                                    .Select(x => new MarkAllocationViewModel
                                                 {
                                                     CourseId = x.Key,
                                                     Course = x.FirstOrDefault().Course
                                                 })
                                    .GetPaged(criteria, page);

            foreach (var item in model.Results)
            {
                item.IsMarkAllocation = markAllocations.Any(x => x.CourseId == item.CourseId);
            }
            
            return View(model);
        }

        public ActionResult Details(long courseId, long termId, string returnUrl, string tabIndex)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            var model = new MarkAllocationViewModel();
            var course = _db.Courses.SingleOrDefault(x => x.Id == courseId);
            var details = _db.MarkAllocations.Where(x => x.TermId == termId
                                                         && x.CourseId == courseId)
                                             .Select(x => new MarkAllocationDetail
                                                          {
                                                              Id = x.Id,
                                                              Name = x.Name,
                                                              Abbreviation = x.Abbreviation,
                                                              Score = x.Score,
                                                              Sequence = x.Sequence
                                                          })
                                             .AsNoTracking()
                                             .ToList();
            
            var letterStandardGradingGroup = _db.StandardGradingGroups.Include(x => x.StandardGradingScores)
                                                                            .ThenInclude(x => x.Grade)
                                                                      .AsNoTracking()
                                                                      .FirstOrDefault(x => x.Name.ToLower()
                                                                      .Contains("letter"));
            
            var passFailStandardGradingGroup = _db.StandardGradingGroups.Include(x => x.StandardGradingScores)
                                                                            .ThenInclude(x => x.Grade)
                                                                        .AsNoTracking()
                                                                        .FirstOrDefault(x => x.Name.ToLower()
                                                                        .Contains("pass") || x.Name.ToLower().Contains("fail"));
            
            var letterGradingCurves = _db.GradingCurves.Include(x => x.Grade)
                                                       .Where(x => x.CourseId == courseId 
                                                                   && x.TermId == termId
                                                                   && x.GradeTemplateId == letterStandardGradingGroup.GradeTemplateId)
                                                       .AsNoTracking()
                                                       .OrderByDescending(x => x.Grade.Weight);

            var passFailGradingCurve = _db.GradingCurves.Include(x => x.Grade)
                                                        .Where(x => x.CourseId == courseId 
                                                                    && x.TermId == termId
                                                                    && x.GradeTemplateId == passFailStandardGradingGroup.GradeTemplateId)
                                                        .AsNoTracking()
                                                        .OrderByDescending(x => x.Grade.Weight);
            
            var letterCurves = new List<MarkAllocationScoreCurve>();
            var passFailCurves = new List<MarkAllocationScoreCurve>();
            if (letterGradingCurves != null && letterGradingCurves.Any())
            {
                foreach (var item in letterGradingCurves)
                {
                    var curve = new MarkAllocationScoreCurve
                                {
                                    GradingCurveId = item.Id,
                                    GradeId = item.GradeId,
                                    Grade = item.Grade.Name,
                                    GradeTemplateId = item.GradeTemplateId,
                                    MinScore = item.Minimum,
                                    MaxScore = item.Maximum
                                };
                    
                    letterCurves.Add(curve);
                }
            }
            else
            {
                foreach (var item in letterStandardGradingGroup.StandardGradingScores.OrderByDescending(x => x.Grade.Weight))
                {
                    var curve = new MarkAllocationScoreCurve
                                {
                                    GradeId = item.Id,
                                    Grade = item.Grade.Name,
                                    GradeTemplateId = letterStandardGradingGroup.GradeTemplateId,
                                    MinScore = item.Minimum,
                                    MaxScore = item.Maximum
                                };
                    
                    letterCurves.Add(curve);
                }
            }

            if (passFailGradingCurve != null && passFailGradingCurve.Any())
            {
                foreach (var item in passFailGradingCurve)
                {
                    var curve = new MarkAllocationScoreCurve
                                {
                                    GradingCurveId = item.Id,
                                    GradeId = item.GradeId,
                                    Grade = item.Grade.Name,
                                    GradeTemplateId = item.GradeTemplateId,
                                    MinScore = item.Minimum,
                                    MaxScore = item.Maximum
                                };
                    
                    passFailCurves.Add(curve);
                }
            }
            else
            {
                foreach (var item in passFailStandardGradingGroup.StandardGradingScores.OrderByDescending(x => x.Grade.Weight))
                {
                    var curve = new MarkAllocationScoreCurve
                                {
                                    GradeId = item.Id,
                                    Grade = item.Grade.Name,
                                    GradeTemplateId = passFailStandardGradingGroup.GradeTemplateId,
                                    MinScore = item.Minimum,
                                    MaxScore = item.Maximum
                                };
                    
                    passFailCurves.Add(curve);
                }
            }

            model.CourseId = courseId;
            model.Course = course.CourseAndCredit;
            model.TermId = termId;
            model.Details = details;
            model.LetterCurves = letterCurves;
            model.PassFailCurves = passFailCurves;
            model.IsScoreChanged = false;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditScoreCurve(MarkAllocationViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var gradingCurves = new List<GradingCurve>();
            if (model.LetterCurves.Any(x => x.MinScore == null || x.MaxScore == null)
                || model.PassFailCurves.Any(x => x.MinScore == null || x.MaxScore == null))
            {
                _flashMessage.Danger(Message.RequiredMinMaxScore);
                return RedirectToAction(nameof(Details), new { courseId = model.CourseId, termId = model.TermId, returnUrl = returnUrl, tabIndex = "1" });
            }

            var oldCurves = _db.GradingCurves.Where(x => model.CourseId == x.CourseId
                                                         && model.TermId == x.TermId)
                                             .ToList();

            try
            {
                foreach (var item in model.LetterCurves)
                {
                    if (oldCurves.Any(x => x.Id == item.GradingCurveId))
                    {
                        oldCurves.Where(x => x.Id == item.GradingCurveId)
                                 .Select(x => {
                                                   x.CourseId = model.CourseId;
                                                   x.TermId = model.TermId;
                                                   x.GradeTemplateId = item.GradeTemplateId;
                                                   x.GradeId = item.GradeId;
                                                   x.Minimum = item.MinScore ?? 0;
                                                   x.Maximum = item.MaxScore ?? 0;
                                                   return x;
                                              }).ToList();
                    }
                    else
                    {
                        var gradingCurve = new GradingCurve
                                           {
                                               CourseId = model.CourseId,
                                               TermId = model.TermId,
                                               GradeTemplateId = item.GradeTemplateId,
                                               GradeId = item.GradeId,
                                               Minimum = item.MinScore ?? 0,
                                               Maximum = item.MaxScore ?? 0
                                           };

                        gradingCurves.Add(gradingCurve);
                    }
                }

                foreach (var item in model.PassFailCurves)
                {
                    if (oldCurves.Any(x => x.Id == item.GradingCurveId))
                    {
                        oldCurves.Where(x => x.Id == item.GradingCurveId)
                                 .Select(x => {
                                                   x.CourseId = model.CourseId;
                                                   x.TermId = model.TermId;
                                                   x.GradeTemplateId = item.GradeTemplateId;
                                                   x.GradeId = item.GradeId;
                                                   x.Minimum = item.MinScore ?? 0;
                                                   x.Maximum = item.MaxScore ?? 0;
                                                   return x;
                                              }).ToList();
                    }
                    else
                    {
                        var gradingCurve = new GradingCurve
                                           {
                                               CourseId = model.CourseId,
                                               TermId = model.TermId,
                                               GradeTemplateId = item.GradeTemplateId,
                                               GradeId = item.GradeId,
                                               Minimum = item.MinScore ?? 0,
                                               Maximum = item.MaxScore ?? 0
                                           };

                        gradingCurves.Add(gradingCurve);
                    }
                }

                _db.GradingCurves.AddRange(gradingCurves);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
            }

            return RedirectToAction(nameof(Details), new { courseId = model.CourseId, termId = model.TermId, returnUrl = returnUrl, tabIndex = "1" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMarkAllocation(MarkAllocationViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if(model.Details.GroupBy(x => x.Abbreviation).Any(x => x.Count() > 1) || model.Details.GroupBy(x => x.Name).Any(x => x.Count() > 1))
            {
                _flashMessage.Danger(Message.DuplicateMarkAllocation);
                return RedirectToAction(nameof(Details), new { courseId = model.CourseId, termId = model.TermId, returnUrl = returnUrl, tabIndex = "0" });
            }
            try
            {
                var markallocationIds = model.Details.Select(x => x.Id).ToList();
                var removeMarkAllocations = _db.MarkAllocations.Where(x => !markallocationIds.Contains(x.Id)
                                                                           && x.CourseId == model.CourseId
                                                                           && x.TermId == model.TermId)
                                                               .ToList();

                var oldMarkAllocations = _db.MarkAllocations.Where(x => model.CourseId == x.CourseId
                                                                        && model.TermId == x.TermId)
                                                            .ToList();
                _db.MarkAllocations.RemoveRange(removeMarkAllocations);
                if (model.Details != null)
                {
                    model.Details = model.Details.Where(x => !string.IsNullOrEmpty(x.Name)
                                                             && !string.IsNullOrEmpty(x.Abbreviation))
                                                 .ToList();

                    var markAllocations = new List<MarkAllocation>();
                    for (var i = 0; i < model.Details.Count(); i++)
                    {
                        if (oldMarkAllocations.Any(x => x.Id == model.Details[i].Id))
                        {
                            oldMarkAllocations.Where(x => x.Id == model.Details[i].Id)
                                              .Select(x => {
                                                                x.TermId = model.TermId;
                                                                x.CourseId = model.CourseId;
                                                                x.Sequence = i+1;
                                                                x.Name = model.Details[i].Name;
                                                                x.Abbreviation = model.Details[i].Abbreviation;
                                                                x.Score = model.Details[i].Score;
                                                                return x;
                                                           }).ToList();
                        }
                        else
                        {
                            var markAllocation = new MarkAllocation
                                                {
                                                    TermId = model.TermId,
                                                    CourseId = model.CourseId,
                                                    Sequence = i+1,
                                                    Name = model.Details[i].Name,
                                                    Abbreviation = model.Details[i].Abbreviation,
                                                    Score = model.Details[i].Score
                                                };

                            markAllocations.Add(markAllocation);
                        }

                        _db.MarkAllocations.AddRange(markAllocations);
                    }
                }

                if (model.IsScoreChanged)
                {
                    var oldCurves = _db.GradingCurves.Where(x => model.CourseId == x.CourseId
                                                                 && model.TermId == x.TermId)
                                                     .ToList();

                    _db.GradingCurves.RemoveRange(oldCurves);
                }
                
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
                return RedirectToAction(nameof(Details), new { courseId = model.CourseId, termId = model.TermId, returnUrl = returnUrl, tabIndex = "0" });
            }

            return RedirectToAction(nameof(Details), new { courseId = model.CourseId, termId = model.TermId, returnUrl = returnUrl, tabIndex = "0" });
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
        }
    }
}