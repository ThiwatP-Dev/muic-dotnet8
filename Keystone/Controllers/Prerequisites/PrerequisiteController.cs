using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Prerequisites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("Prerequisite", "")]
    public class PrerequisiteController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;
        protected readonly IPrerequisiteProvider _prerequisiteProvider;

        public PrerequisiteController(ApplicationDbContext db,
                                      IFlashMessage flashMessage,
                                      ISelectListProvider selectListProvider,
                                      IPrerequisiteProvider prerequisiteProvider,
                                      IExceptionManager exceptionManager) : base(db, flashMessage, selectListProvider)
        {
            _exceptionManager = exceptionManager;
            _prerequisiteProvider = prerequisiteProvider;
        }

        public ActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.Type);
            if (criteria.CourseId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var models = _db.Prerequisites.Include(x => x.Course)
                                          .Include(x => x.CurriculumVersion)
                                          .Where(x => x.CourseId == criteria.CourseId
                                                      && (criteria.CurriculumVersionId == 0
                                                          || x.CurriculumVersionId == criteria.CurriculumVersionId)
                                                      && (string.IsNullOrEmpty(criteria.Type)
                                                          || criteria.Type == x.ConditionType)
                                                      && (criteria.ConditionId == 0
                                                          || criteria.ConditionId == x.ConditionId))
                                          .IgnoreQueryFilters()
                                          .AsNoTracking()
                                          .ToList();

            _prerequisiteProvider.GetPrerequisiteNames(ref models);
            var modelPageResult = models.Select(x => new PrerequisiteFormViewModel
                                                     {
                                                         Id = x.Id,
                                                         CourseId = x.CourseId,
                                                         CurriculumVersionId = x.CurriculumVersionId ?? 0,
                                                         CurriculumVersionName = x.CurriculumVersion?.NameEn,
                                                         CourseCode = x.Course.Code,
                                                         CourseName = x.Course.NameEnAndCredit,
                                                         Condition = x.PrerequisiteName,
                                                         IsActive = x.IsActive
                                                     })
                                        .AsQueryable()
                                        .GetPaged(criteria, page);
                                        
            return View(modelPageResult);
        }

        [PermissionAuthorize("Prerequisite", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View(new PrerequisiteFormViewModel());
        }

        [PermissionAuthorize("Prerequisite", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PrerequisiteFormViewModel model)
        {
            if (model.CourseId == 0)
            {
                CreateSelectList();
                _flashMessage.Danger(Message.RequiredData);
                return View(model);
            }
            
            try
            {
                AddPrerequisites(model);

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index), new { CourseId = model.CourseId, CurriculumVersionId = model.CurriculumVersionId });
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCreate);
                CreateSelectList();
                return View(model);
            }
        }

        public ActionResult Edit(long id, long curriculumVersionId)
        {
            CreateSelectList();
            var model = _prerequisiteProvider.GetPrerequisiteFormViewModel(id, curriculumVersionId);
            return View(model);
        }

        [PermissionAuthorize("Prerequisite", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PrerequisiteFormViewModel model)
        {
            try
            {
                var prerequisites = _db.Prerequisites.Where(x => x.CourseId == model.CourseId
                                                                 && x.CurriculumVersionId == model.CurriculumVersionId);
                _db.Prerequisites.RemoveRange(prerequisites);

                AddPrerequisites(model);

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index), new { CourseId = model.CourseId, CurriculumVersionId = model.CurriculumVersionId });
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
                CreateSelectList();
                return View(model);
            }
        }

        [PermissionAuthorize("Prerequisite", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            long courseId = 0;
            long curriculumVersionId = 0;
            try
            {
                var prerequisite = _prerequisiteProvider.GetPrerequisite(id);
                courseId = prerequisite.CourseId;
                curriculumVersionId = prerequisite.CurriculumVersionId ?? 0;
                _db.Prerequisites.Remove(prerequisite);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index), new { CourseId = courseId, CurriculumVersionId = curriculumVersionId });
        }

        private void CreateSelectList(string type = null)
        {
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.Conditions = _selectListProvider.GetConditionsByType(type);
            ViewBag.CurrirulumVersions = _selectListProvider.GetCurriculumVersions();
            ViewBag.Types = _selectListProvider.GetConditionTypes();
            ViewBag.AndConditions = _selectListProvider.GetAndConditions();
            ViewBag.OrConditions = _selectListProvider.GetOrConditions();
            ViewBag.GradeConditions = _selectListProvider.GetGradeConditions();
            ViewBag.CourseGroupConditions = _selectListProvider.GetCourseGroupConditions();
            ViewBag.TermConditions = _selectListProvider.GetTermConditions();
            ViewBag.GPAConditions = _selectListProvider.GetGPAConditions();
            ViewBag.CreditConditions = _selectListProvider.GetCreditConditions();
            ViewBag.TotalCourseGroupConditions = _selectListProvider.GetTotalCourseGroupConditions();
            ViewBag.BatchConditions = _selectListProvider.GetBatchConditions();
            ViewBag.AbilityConditions = _selectListProvider.GetAbilityConditions();
        }

        private void AddPrerequisites(PrerequisiteFormViewModel model)
        {
            //and
            if (model.AndConditionIds != null && model.AndConditionIds.Any())
            {
                _db.Prerequisites.AddRange(model.AndConditionIds
                                 .Select(x => new Prerequisite
                                              {
                                                  ConditionType = "and",
                                                  CourseId = model.CourseId,
                                                  ConditionId = x,
                                                  CurriculumVersionId = model.CurriculumVersionId,
                                                  Description = _db.AndConditions.SingleOrDefault(y => y.Id == x).Description
                                              }).ToList());
            }
            
            //or
            if (model.OrConditionIds != null && model.OrConditionIds.Any())
            {
                _db.Prerequisites.AddRange(model.OrConditionIds
                                 .Select(x => new Prerequisite
                                              {
                                                  ConditionType = "or",
                                                  CourseId = model.CourseId,
                                                  ConditionId = x,
                                                  CurriculumVersionId = model.CurriculumVersionId,
                                                  Description = _db.OrConditions.SingleOrDefault(y => y.Id == x).Description
                                              }).ToList());
            }
            
            //coursegroup
            if (model.CourseGroupConditionIds != null && model.CourseGroupConditionIds.Any())
            {
                _db.Prerequisites.AddRange(model.CourseGroupConditionIds
                                 .Select(x => new Prerequisite
                                              {
                                                  ConditionType = "coursegroup",
                                                  CourseId = model.CourseId,
                                                  ConditionId = x,
                                                  CurriculumVersionId = model.CurriculumVersionId,
                                                  Description = _db.CourseGroupConditions.SingleOrDefault(y => y.Id == x).Description
                                              }).ToList());
            }
            
            //credit
            if (model.CreditConditionIds != null && model.CreditConditionIds.Any())
            {
                _db.Prerequisites.AddRange(model.CreditConditionIds
                                 .Select(x => new Prerequisite
                                              {
                                                  ConditionType = "credit",
                                                  CourseId = model.CourseId,
                                                  ConditionId = x,
                                                  CurriculumVersionId = model.CurriculumVersionId,
                                                  Description = _db.CreditConditions.SingleOrDefault(y => y.Id == x).Description
                                              }).ToList());
            }
            
            //gpa
            if (model.GPAConditionIds != null && model.GPAConditionIds.Any())
            {
                _db.Prerequisites.AddRange(model.GPAConditionIds
                                 .Select(x => new Prerequisite
                                              {
                                                  ConditionType = "gpa",
                                                  CourseId = model.CourseId,
                                                  ConditionId = x,
                                                  CurriculumVersionId = model.CurriculumVersionId,
                                                  Description = _db.GPAConditions.SingleOrDefault(y => y.Id == x).Description
                                              }).ToList());
            }
            
            //grade
            if (model.GradeConditionIds != null && model.GradeConditionIds.Any())
            {
                _db.Prerequisites.AddRange(model.GradeConditionIds
                                 .Select(x => new Prerequisite
                                              {
                                                  ConditionType = "grade",
                                                  CourseId = model.CourseId,
                                                  ConditionId = x,
                                                  CurriculumVersionId = model.CurriculumVersionId,
                                                  Description = _db.GradeConditions.SingleOrDefault(y => y.Id == x).Description
                                              }).ToList());
            }
            
            //term
            if (model.TermConditionIds != null && model.TermConditionIds.Any())
            {
                _db.Prerequisites.AddRange(model.TermConditionIds
                                 .Select(x => new Prerequisite
                                              {
                                                  ConditionType = "term",
                                                  CourseId = model.CourseId,
                                                  ConditionId = x,
                                                  CurriculumVersionId = model.CurriculumVersionId,
                                                  Description = _db.TermConditions.SingleOrDefault(y => y.Id == x).Description
                                              }).ToList());
            }
            
            //totalcoursegroup
            if (model.TotalCourseGroupConditionIds != null && model.TotalCourseGroupConditionIds.Any())
            {
                _db.Prerequisites.AddRange(model.TotalCourseGroupConditionIds
                                 .Select(x => new Prerequisite
                                              {
                                                  ConditionType = "totalcoursegroup",
                                                  CourseId = model.CourseId,
                                                  ConditionId = x,
                                                  CurriculumVersionId = model.CurriculumVersionId,
                                                  Description = _db.TotalCourseGroupConditions.SingleOrDefault(y => y.Id == x).Description
                                              }).ToList());
            }

            //batch
            if (model.BatchConditionIds != null && model.BatchConditionIds.Any())
            {
                _db.Prerequisites.AddRange(model.BatchConditionIds
                                 .Select(x => new Prerequisite
                                              {
                                                  ConditionType = "batch",
                                                  CourseId = model.CourseId,
                                                  ConditionId = x,
                                                  CurriculumVersionId = model.CurriculumVersionId,
                                                  Description = _db.BatchConditions.SingleOrDefault(y => y.Id == x).Description
                                              }).ToList());
            }

            //ability
            if (model.AbilityConditionIds != null && model.AbilityConditionIds.Any())
            {
                _db.Prerequisites.AddRange(model.AbilityConditionIds
                                 .Select(x => new Prerequisite
                                              {
                                                  ConditionType = "ability",
                                                  CourseId = model.CourseId,
                                                  ConditionId = x,
                                                  CurriculumVersionId = model.CurriculumVersionId,
                                                  Description = _db.SpecializationGroups.SingleOrDefault(y => y.Id == _db.AbilityConditions.SingleOrDefault(z => z.Id == x).AbilityId).NameEn
                                              }).ToList());
            }
        }
    }
}