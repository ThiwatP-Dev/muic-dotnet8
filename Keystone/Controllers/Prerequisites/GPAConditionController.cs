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
    [PermissionAuthorize("GPACondition", "")]
    public class GPAConditionController : BaseController
    {
        protected readonly IPrerequisiteProvider _prerequisiteProvider;

        public GPAConditionController(ApplicationDbContext db,
                                      IFlashMessage flashMessage,
                                      ISelectListProvider selectListProvider,
                                      IPrerequisiteProvider prerequisiteProvider) : base(db, flashMessage, selectListProvider)
        {
            _prerequisiteProvider = prerequisiteProvider;
        }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList();
            var models = _db.GPAConditions.AsNoTracking()
                                          .IgnoreQueryFilters()
                                          .Where(x => (criteria.GPAFrom == null
                                                       || x.GPA == criteria.GPAFrom)
                                                      && (string.IsNullOrEmpty(criteria.Status) 
                                                          || x.IsActive == Convert.ToBoolean(criteria.Status)))
                                          .OrderBy(x => x.GPA)
                                          .GetPaged(criteria, page, true);
            return View(models);
        }

        [PermissionAuthorize("GPACondition", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new GPACondition());
        }

        [PermissionAuthorize("GPACondition", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GPACondition model, string returnUrl)
        {
            if (_prerequisiteProvider.IsExistGPACondition(model))
            {
                CreateSelectList();
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    model.Description = _prerequisiteProvider.GetGPAConditionDescription(model);
                    _db.GPAConditions.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { GPAFrom = model.GPA });
                }
                catch
                {
                    CreateSelectList();
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            GPACondition model = _prerequisiteProvider.GetGPACondition(id);
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            return View(model);
        }

        [PermissionAuthorize("GPACondition", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = _prerequisiteProvider.GetGPACondition(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<GPACondition>(model))
            {
                if (_prerequisiteProvider.IsExistGPACondition(model))
                {
                    CreateSelectList();
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }
                else
                {
                    try
                    {
                        model.Description = _prerequisiteProvider.GetGPAConditionDescription(model);
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index), new { GPAFrom = model.GPA });
                    }
                    catch
                    {
                        CreateSelectList();
                        ViewBag.ReturnUrl = returnUrl;
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
            }

            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("GPACondition", PolicyGenerator.Write)]
        public ActionResult Delete(long id, string returnUrl)
        {
            GPACondition model = _prerequisiteProvider.GetGPACondition(id);
            var check = _prerequisiteProvider.IsConditionAlreadyExits(model.Id, "gpa");
            if(check.IsAlreadyExist)
            {
                _flashMessage.Danger(check.Message);
                return Redirect(returnUrl);
            }
            try
            {
                _db.GPAConditions.Remove(model);
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
            ViewBag.ActiveStatuses = _selectListProvider.GetActiveStatuses();
        }
    }
}