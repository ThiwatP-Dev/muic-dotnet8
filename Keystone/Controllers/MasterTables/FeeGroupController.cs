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
    [PermissionAuthorize("FeeGroup", "")]
    public class FeeGroupController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;

        public FeeGroupController(ApplicationDbContext db,
                                  IFlashMessage flashMessage,
                                  ISelectListProvider selectListProvider,
                                  IExceptionManager exceptionManager) : base(db, flashMessage, selectListProvider) 
        { 
            _exceptionManager = exceptionManager;
        }

        public IActionResult Index(int page)
        {
            var models = _db.FeeGroups.IgnoreQueryFilters()
                                      .GetPaged(page, true);
            return View(models);
        }

        [PermissionAuthorize("FeeGroup", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new FeeGroup());
        }

        [PermissionAuthorize("FeeGroup", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FeeGroup model)
        {
            if (ModelState.IsValid)
            {
                _db.FeeGroups.Add(model);
                try
                {
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
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

                    return View(model);
                }
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            FeeGroup model = Find(id);
            return View(model);
        }

        [PermissionAuthorize("FeeGroup", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            FeeGroup model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<FeeGroup>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
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

                    return View(model);
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("FeeGroup", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            FeeGroup model = Find(id);
            try
            {
                _db.FeeGroups.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private FeeGroup Find(long? id)
        {
            var item = _db.FeeGroups.IgnoreQueryFilters()
                                    .SingleOrDefault(x => x.Id == id);
            return item;
        }
    }
}