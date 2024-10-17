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
    [PermissionAuthorize("TermCondition", "")]
    public class TermConditionController : BaseController
    {
        protected readonly IPrerequisiteProvider _prerequisiteProvider;

        public TermConditionController(ApplicationDbContext db,
                                       IFlashMessage flashMessage,
                                       ISelectListProvider selectListProvider,
                                       IPrerequisiteProvider prerequisiteProvider) : base(db, flashMessage, selectListProvider)
        {
            _prerequisiteProvider = prerequisiteProvider;
        }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList();
            var models = _db.TermConditions.AsNoTracking()
                                           .Where(x => (criteria.Term == 0
                                                        || x.Term == criteria.Term)
                                                       && (string.IsNullOrEmpty(criteria.Status) 
                                                           || x.IsActive == Convert.ToBoolean(criteria.Status)))
                                           .IgnoreQueryFilters()
                                           .GetPaged(criteria, page, true);
            return View(models);
        }

        [PermissionAuthorize("TermCondition", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new TermCondition());
        }

        [PermissionAuthorize("TermCondition", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TermCondition model, string returnUrl)
        {
            if (_prerequisiteProvider.IsExistTermCondition(model))
            {
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    model.Description = _prerequisiteProvider.GetTermConditionDescription(model);
                    _db.TermConditions.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { Term = model.Term });
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            var model = _prerequisiteProvider.GetTermCondition(id);
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [PermissionAuthorize("TermCondition", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = _prerequisiteProvider.GetTermCondition(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<TermCondition>(model))
            {
                if (_prerequisiteProvider.IsExistTermCondition(model))
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }
                else
                {
                    try
                    {
                        model.Description = _prerequisiteProvider.GetTermConditionDescription(model);
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index), new { Term = model.Term });
                    }
                    catch
                    {
                        ViewBag.ReturnUrl = returnUrl;
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("TermCondition", PolicyGenerator.Write)]
        public ActionResult Delete(long id, string returnUrl)
        {
            var model = _prerequisiteProvider.GetTermCondition(id);
            var check = _prerequisiteProvider.IsConditionAlreadyExits(model.Id, "term");
            if(check.IsAlreadyExist)
            {
                _flashMessage.Danger(check.Message);
                return Redirect(returnUrl);
            }
            try
            {
                _db.TermConditions.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private void CreateSelectList()
        {
            ViewBag.ActiveStatuses = _selectListProvider.GetActiveStatuses();
        }
    }
}