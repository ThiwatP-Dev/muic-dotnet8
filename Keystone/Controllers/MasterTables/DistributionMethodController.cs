using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("DistributionMethod", "")]
    public class DistributionMethodController : BaseController
    {
        protected readonly IMasterProvider _masterProvider;

        public DistributionMethodController(ApplicationDbContext db,
                                            IFlashMessage flashMessage,
                                            ISelectListProvider selectListProvider,
                                            IMasterProvider masterProvider) : base(db, flashMessage, selectListProvider)
        {
            _masterProvider = masterProvider;
        }
        
        public IActionResult Index(int page, Criteria criteria)
        {
            var model = _db.DistributionMethods.IgnoreQueryFilters()
                                               .OrderBy(x => x.NameEn)
                                               .GetPaged(criteria, page);

            return View(model);
        }

        [PermissionAuthorize("DistributionMethod", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new DistributionMethod());
        }

        [PermissionAuthorize("DistributionMethod", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DistributionMethod model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.DistributionMethods.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            var model = _masterProvider.GetDistributionMethod(id);
            return View(model);
        }

        [PermissionAuthorize("DistributionMethod", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = _masterProvider.GetDistributionMethod(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<DistributionMethod>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(model);
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("DistributionMethod", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = _masterProvider.GetDistributionMethod(id);
            try
            {
                _db.DistributionMethods.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}