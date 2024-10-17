using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Prerequisites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class TotalCourseGroupConditionController : BaseController
    {
        protected readonly IPrerequisiteProvider _prerequisiteProvider;

        public TotalCourseGroupConditionController(ApplicationDbContext db,
                                                   IFlashMessage flashMessage,
                                                   ISelectListProvider selectListProvider,
                                                   IPrerequisiteProvider prerequisiteProvider) : base(db, flashMessage, selectListProvider)
        {
            _prerequisiteProvider = prerequisiteProvider;
        }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList();
            var models = _db.TotalCourseGroupConditions.Include(x => x.CurriculumVersion)
                                                           .ThenInclude(x => x.Curriculum)
                                                       .IgnoreQueryFilters()
                                                       .Where(x => (criteria.CurriculumVersionId == 0
                                                                    || x.CurriculumVersionId == criteria.CurriculumVersionId)
                                                                   && (criteria.CurriculumId == 0
                                                                       || x.CurriculumVersion.CurriculumId == criteria.CurriculumId)
                                                                   && (string.IsNullOrEmpty(criteria.Status) 
                                                                       || x.IsActive == Convert.ToBoolean(criteria.Status)))
                                                       .GetPaged(criteria, page, true);
            return View(models);
        }

        public ActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new TotalCourseGroupCondition());
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TotalCourseGroupCondition model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (_prerequisiteProvider.IsExistTotalCourseGroupCondition(model))
                {
                    CreateSelectList(model.AcademicLevelId, model.CurriculumId);
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }
                else
                {
                    try
                    {
                        model.Description = _prerequisiteProvider.GetTotalCourseGroupConditionDescription(model);
                        _db.TotalCourseGroupConditions.Add(model);
                        _db.SaveChanges();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index),
                                                new
                                                {
                                                    AcademicLevelId = model.CurriculumVersion?.Curriculum?.AcademicLevelId ?? 0,
                                                    CurriculumId = model.CurriculumVersion?.CurriculumId ?? 0,
                                                    CurriculumVersionId = model.CurriculumVersionId
                                                });
                    }
                    catch
                    {
                        CreateSelectList(model.AcademicLevelId, model.CurriculumId);
                        ViewBag.ReturnUrl = returnUrl;
                        _flashMessage.Danger(Message.UnableToCreate);
                        return View(model);
                    }
                }
            }

            CreateSelectList(model.AcademicLevelId, model.CurriculumId);
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            var model = _prerequisiteProvider.GetTotalCourseGroupCondition(id);
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.CurriculumVersion?.Curriculum?.AcademicLevelId ?? 0, model.CurriculumVersion?.CurriculumId ?? 0);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = _prerequisiteProvider.GetTotalCourseGroupCondition(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<TotalCourseGroupCondition>(model))
            {
                if (_prerequisiteProvider.IsExistTotalCourseGroupCondition(model))
                {
                    CreateSelectList(model.CurriculumVersion?.Curriculum?.AcademicLevelId ?? 0, model.CurriculumVersion?.CurriculumId ?? 0);
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }
                else
                {
                    try
                    {
                        model.Description = _prerequisiteProvider.GetTotalCourseGroupConditionDescription(model);
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index),
                                                new
                                                {
                                                        AcademicLevelId = model.CurriculumVersion?.Curriculum?.AcademicLevelId ?? 0,
                                                        CurriculumId = model.CurriculumVersion?.CurriculumId ?? 0,
                                                        CurriculumVersionId = model.CurriculumVersionId
                                                });
                    }
                    catch
                    {
                        CreateSelectList(model.CurriculumVersion?.Curriculum?.AcademicLevelId ?? 0, model.CurriculumVersion?.CurriculumId ?? 0);
                        ViewBag.ReturnUrl = returnUrl;
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
            }

            CreateSelectList(model.CurriculumVersion?.Curriculum?.AcademicLevelId ?? 0, model.CurriculumVersion?.CurriculumId ?? 0);
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        public ActionResult Delete(long id, string returnUrl)
        {
            var model = _prerequisiteProvider.GetTotalCourseGroupCondition(id);
            var check = _prerequisiteProvider.IsConditionAlreadyExits(model.Id, "totalcoursegroup");
            if(check.IsAlreadyExist)
            {
                _flashMessage.Danger(check.Message);
                return Redirect(returnUrl);
            }
            try
            {
                _db.TotalCourseGroupConditions.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return Redirect(returnUrl);
        }

        private void CreateSelectList(long academicLevelId = 0, long curriculumId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);
            ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
            ViewBag.ActiveStatuses = _selectListProvider.GetActiveStatuses();
        }
    }
}