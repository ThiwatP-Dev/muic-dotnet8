using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Prerequisites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class CreditConditionController : BaseController
    {
        protected readonly IPrerequisiteProvider _prerequisiteProvider;

        public CreditConditionController(ApplicationDbContext db,
                                         IFlashMessage flashMessage,
                                         ISelectListProvider selectListProvider,
                                         IPrerequisiteProvider prerequisiteProvider) : base(db, flashMessage, selectListProvider)
        {
            _prerequisiteProvider = prerequisiteProvider;
        }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList();
            var models = _db.CreditConditions.AsNoTracking()
                                             .Include(x => x.CurriculumVersion)
                                             .Include(x => x.CourseGroup)
                                             .Include(x => x.TeachingType)
                                             .IgnoreQueryFilters()
                                             .Where(x => (criteria.Credit == null
                                                          || x.Credit == criteria.Credit)
                                                         && (criteria.TeachingTypeId == 0
                                                             || x.TeachingTypeId == criteria.TeachingTypeId)
                                                         && (criteria.CreditType == "all"
                                                             || x.CreditType == criteria.CreditType)
                                                         && (criteria.CurriculumVersionId == 0
                                                             || x.CurriculumVersionId == criteria.CurriculumVersionId)
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
            return View(new CreditCondition());
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreditCondition model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (_prerequisiteProvider.IsExistCreditCondition(model))
                {
                    CreateSelectList(model.CurriculumVersion?.Curriculum?.AcademicLevelId ?? 0, model.CurriculumVersion?.CurriculumId ?? 0, model.CurriculumVersionId ?? 0);
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }
                else
                {
                    try
                    {
                        model.Description = _prerequisiteProvider.GetCreditConditionDescription(model);
                        _db.CreditConditions.Add(model);
                        _db.SaveChanges();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return Redirect(returnUrl);
                    }
                    catch
                    {
                        CreateSelectList();
                        ViewBag.ReturnUrl = returnUrl;
                        _flashMessage.Danger(Message.UnableToCreate);
                        return View(model);
                    }
                }
            }

            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            var model = _prerequisiteProvider.GetCreditCondition(id);
            CreateSelectList(model.CurriculumVersion?.Curriculum?.AcademicLevelId ?? 0, model.CurriculumVersion?.CurriculumId ?? 0, model.CurriculumVersionId ?? 0);
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = _prerequisiteProvider.GetCreditCondition(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<CreditCondition>(model))
            {
                if (_prerequisiteProvider.IsExistCreditCondition(model))
                {
                    CreateSelectList(model.CurriculumVersion?.Curriculum?.AcademicLevelId ?? 0, model.CurriculumVersion?.CurriculumId ?? 0, model.CurriculumVersionId ?? 0);
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }
                else
                {
                    try
                    {
                        model.Description = _prerequisiteProvider.GetCreditConditionDescription(model);
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return Redirect(returnUrl);
                    }
                    catch
                    {
                        CreateSelectList(model.CurriculumVersion?.Curriculum?.AcademicLevelId ?? 0, model.CurriculumVersion?.CurriculumId ?? 0, model.CurriculumVersionId ?? 0);
                        ViewBag.ReturnUrl = returnUrl;
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
            }

            CreateSelectList(model.CurriculumVersion?.Curriculum?.AcademicLevelId ?? 0, model.CurriculumVersion?.CurriculumId ?? 0, model.CurriculumVersionId ?? 0);
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        public ActionResult Delete(long id, string returnUrl)
        {
            var model = _prerequisiteProvider.GetCreditCondition(id);
            var check = _prerequisiteProvider.IsConditionAlreadyExits(model.Id, "credit");
            if(check.IsAlreadyExist)
            {
                _flashMessage.Danger(check.Message);
                return Redirect(returnUrl);
            }
            try
            {
                _db.CreditConditions.Remove(model);
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
            ViewBag.TeachingTypes = _selectListProvider.GetTeachingTypes();
            ViewBag.CreditTypes = _selectListProvider.GetCreditTypes();
            ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);
            ViewBag.CurriculumVersions = curriculumId == 0 ? _selectListProvider.GetCurriculumVersions()
                                                           : _selectListProvider.GetCurriculumVersion(curriculumId);
            ViewBag.CourseGroups = _selectListProvider.GetCurriculumCourseGroups(curriculumVersionId);
            ViewBag.ActiveStatuses = _selectListProvider.GetActiveStatuses();
        }
    }
}