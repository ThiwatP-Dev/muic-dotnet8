using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Prerequisites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
namespace Keystone.Controllers
{
    [PermissionAuthorize("AbilityCondition", "")]
    public class AbilityConditionController : BaseController
    {
        protected readonly IPrerequisiteProvider _prerequisiteProvider;
        public AbilityConditionController(ApplicationDbContext db,
                                          IFlashMessage flashMessage,
                                          ISelectListProvider selectListProvider,
                                          IPrerequisiteProvider prerequisiteProvider) : base(db, flashMessage, selectListProvider)
        {
            _prerequisiteProvider = prerequisiteProvider;
        }
        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList();
            var models = _db.AbilityConditions.Include(x => x.Ability)
                                              .Where(x => criteria.AbilityId == 0
                                                          || x.AbilityId == criteria.AbilityId)
                                              .IgnoreQueryFilters()
                                              .GetPaged(criteria, page, true);
            return View(models);
        }

        [PermissionAuthorize("AbilityCondition", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            return View(new AbilityCondition());
        }

        [PermissionAuthorize("AbilityCondition", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AbilityCondition model, string returnUrl)
        {
            CreateSelectList();
            if (_prerequisiteProvider.IsExistAbilityCondition(model))
            {
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    _db.AbilityConditions.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { AbilityId = model.AbilityId });
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToCreate);
                    CreateSelectList();
                    return View(model);
                }
            }
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            CreateSelectList();
            return View(model);
        }
        public ActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            AbilityCondition model = _prerequisiteProvider.GetAbilityConditionById(id);
            return View(model);
        }

        [PermissionAuthorize("AbilityCondition", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = _prerequisiteProvider.GetAbilityConditionById(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<AbilityCondition>(model))
            {
                if (_prerequisiteProvider.IsExistAbilityCondition(model))
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }
                else
                {
                    try
                    {
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return Redirect(returnUrl);
                    }
                    catch
                    {
                        ViewBag.ReturnUrl = returnUrl;
                        _flashMessage.Danger(Message.UnableToEdit);
                        CreateSelectList();
                        return View(model);
                    }
                }
            }
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("AbilityCondition", PolicyGenerator.Write)]
        public ActionResult Delete(long id, string returnUrl)
        {
            AbilityCondition model = _prerequisiteProvider.GetAbilityConditionById(id);
            var check = _prerequisiteProvider.IsConditionAlreadyExits(model.Id, "ability");
            if(check.IsAlreadyExist)
            {
                _flashMessage.Danger(check.Message);
                return Redirect(returnUrl);
            }
            try
            {
                _db.AbilityConditions.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            return Redirect(returnUrl);
        }
        private void CreateSelectList()
        {
            ViewBag.Abilities = _selectListProvider.GetAbilities();
        }
    }
}