using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Fee;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("FeeItem", "")]
    public class FeeItemController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;
        protected readonly IMasterProvider _masterProvider;

        public FeeItemController(ApplicationDbContext db,
                                 IFlashMessage flashMessage,
                                 IExceptionManager exceptionManager,
                                 IMasterProvider masterProvider,
                                 ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider)
        {
            _exceptionManager = exceptionManager;
            _masterProvider = masterProvider;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList();
            var models = _db.FeeItems.Include(x => x.FeeGroup)
                                     .Where(x => (criteria.FeeGroupId == 0
                                                  || x.FeeGroupId == criteria.FeeGroupId)
                                                  && (string.IsNullOrEmpty(criteria.Code)
                                                      || x.Code.StartsWith(criteria.Code))
                                                  && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                      || x.NameEn.StartsWith(criteria.CodeAndName))
                                                  && (string.IsNullOrEmpty(criteria.AccountCode)
                                                      || x.AccountCode.StartsWith(criteria.AccountCode)))
                                     .IgnoreQueryFilters()
                                     .OrderBy(x => x.NameEn)
                                     .GetPaged(criteria, page, true);
            return View(models);
        }

        [PermissionAuthorize("FeeItem", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View(new FeeItem());
        }

        [PermissionAuthorize("FeeItem", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FeeItem model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.FeeItems.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { CodeAndName = model.NameEn });
                }
                catch (Exception e)
                {
                    if (_exceptionManager.IsDuplicatedEntityCode(e))
                    {
                        _flashMessage.Danger(Message.CodeUniqueConstraintError);
                    }
                    else
                    {
                        _flashMessage.Danger(Message.UnableToCreate);
                    }

                    CreateSelectList();
                    return View(model);
                }
            }

            CreateSelectList();
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            FeeItem model = _masterProvider.GetFeeItem(id);
            CreateSelectList();
            return View(model);
        }

        [PermissionAuthorize("FeeItem", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            FeeItem model = _masterProvider.GetFeeItem(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<FeeItem>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { CodeAndName = model.NameEn });
                }
                catch (Exception e)
                {
                    if (_exceptionManager.IsDuplicatedEntityCode(e))
                    {
                        _flashMessage.Danger(Message.CodeUniqueConstraintError);
                    }
                    else
                    {
                        _flashMessage.Danger(Message.UnableToEdit);
                    }

                    CreateSelectList();
                    return View(model);
                }
            }

            CreateSelectList();
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("FeeItem", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            FeeItem model = _masterProvider.GetFeeItem(id);
            try
            {
                _db.FeeItems.Remove(model);
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
            ViewBag.FeeGroups = _selectListProvider.GetFeeGroups();
        }
    }
}