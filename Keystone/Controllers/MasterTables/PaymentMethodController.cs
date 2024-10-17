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
    [PermissionAuthorize("PaymentMethod", "")]
    public class PaymentMethodController : BaseController
    {
        protected readonly IMasterProvider _masterProvider;

        public PaymentMethodController(ApplicationDbContext db,
                                       IFlashMessage flashMessage,
                                       ISelectListProvider selectListProvider,
                                       IMasterProvider masterProvider) : base(db, flashMessage, selectListProvider)
        {
            _masterProvider = masterProvider;
        }
        
        public IActionResult Index(int page, Criteria criteria)
        {
            var model = _db.PaymentMethods.IgnoreQueryFilters()
                                          .OrderBy(x => x.NameEn)
                                          .GetPaged(criteria, page, true);

            return View(model);
        }

        [PermissionAuthorize("PaymentMethod", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new PaymentMethod());
        }

        [PermissionAuthorize("PaymentMethod", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PaymentMethod model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.PaymentMethods.Add(model);
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
            var model = _masterProvider.GetPaymentMethod(id);
            return View(model);
        }

        [PermissionAuthorize("PaymentMethod", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = _masterProvider.GetPaymentMethod(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<PaymentMethod>(model))
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

        [PermissionAuthorize("PaymentMethod", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = _masterProvider.GetPaymentMethod(id);
            try
            {
                _db.PaymentMethods.Remove(model);
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