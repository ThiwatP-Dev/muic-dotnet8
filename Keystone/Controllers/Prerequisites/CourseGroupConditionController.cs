using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Prerequisites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class CourseGroupConditionController : BaseController
    {
        protected readonly IPrerequisiteProvider _prerequisiteProvider;

        public CourseGroupConditionController(ApplicationDbContext db,
                                              IFlashMessage flashMessage,
                                              ISelectListProvider selectListProvider,
                                              IPrerequisiteProvider prerequisiteProvider) : base(db, flashMessage, selectListProvider)
        {
            _prerequisiteProvider = prerequisiteProvider;
        }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.CurriculumId, criteria.CurriculumVersionId);
            var models = _db.CourseGroupConditions.Include(x => x.CourseGroup)
                                                      .ThenInclude(x => x.CurriculumVersion)
                                                      .ThenInclude(x => x.Curriculum)
                                                      .ThenInclude(x => x.AcademicLevel)
                                                  .IgnoreQueryFilters()
                                                  .Where(x => (criteria.CurriculumVersionId == 0
                                                               || x.CourseGroup.CurriculumVersionId == criteria.CurriculumVersionId)
                                                              && (criteria.CurriculumId == 0
                                                                  || x.CourseGroup.CurriculumVersion.CurriculumId == criteria.CurriculumId)
                                                              && (criteria.AcademicLevelId == 0
                                                                  || x.CourseGroup.CurriculumVersion.Curriculum.AcademicLevelId == criteria.AcademicLevelId)
                                                              && (criteria.CourseGroupId == 0
                                                                  || x.CourseGroupId == criteria.CourseGroupId)
                                                              && (string.IsNullOrEmpty(criteria.Status) 
                                                                  || x.IsActive == Convert.ToBoolean(criteria.Status)))
                                                  .OrderBy(x => x.CourseGroup.NameEn)
                                                      .ThenBy(x => x.Credit)    
                                                  .GetPaged(criteria, page, true);
            return View(models);
        }

        public ActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new CourseGroupCondition());
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CourseGroupCondition model, string returnUrl)
        {
            if (_prerequisiteProvider.IsExistCourseGroupCondition(model))
            {
                CreateSelectList(model.AcademicLevelId, model.CurriculumId, model.CurriculumVersionId);
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    model.Description = _prerequisiteProvider.GetCourseGroupConditionDescription(model);
                    _db.CourseGroupConditions.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return Redirect(returnUrl);
                }
                catch
                {
                    CreateSelectList(model.AcademicLevelId, model.CurriculumId, model.CurriculumVersionId);
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            CreateSelectList(model.AcademicLevelId, model.CurriculumId, model.CurriculumVersionId);
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            CourseGroupCondition model = _prerequisiteProvider.GetCourseGroupCondition(id);
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.AcademicLevelId, model.CurriculumId, model.CurriculumVersionId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = _prerequisiteProvider.GetCourseGroupCondition(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<CourseGroupCondition>(model))
            {
                if (_prerequisiteProvider.IsExistCourseGroupCondition(model))
                {
                    CreateSelectList(model.AcademicLevelId, model.CurriculumId, model.CurriculumVersionId);
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }
                else
                {
                    try
                    {
                        model.Description = _prerequisiteProvider.GetCourseGroupConditionDescription(model);
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return Redirect(returnUrl);
                    }
                    catch
                    {
                        CreateSelectList(model.AcademicLevelId, model.CurriculumId, model.CurriculumVersionId);
                        ViewBag.ReturnUrl = returnUrl;
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
            }

            CreateSelectList(model.AcademicLevelId, model.CurriculumId, model.CurriculumVersionId);
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        public ActionResult Delete(long id , string returnUrl)
        {
            CourseGroupCondition model = _prerequisiteProvider.GetCourseGroupCondition(id);
            var check = _prerequisiteProvider.IsConditionAlreadyExits(model.Id, "coursegroup");
            if(check.IsAlreadyExist)
            {
                _flashMessage.Danger(check.Message);
                return Redirect(returnUrl);
            }
            try
            {
                _db.CourseGroupConditions.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return Redirect(returnUrl);
        }

        private void CreateSelectList(long academicLevelId = 0, long curriculumId = 0, long curriculumVersionId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);
            ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
            ViewBag.CourseGroups = _selectListProvider.GetCurriculumCourseGroups(curriculumVersionId);
            ViewBag.ActiveStatuses = _selectListProvider.GetActiveStatuses();
        }
    }
}