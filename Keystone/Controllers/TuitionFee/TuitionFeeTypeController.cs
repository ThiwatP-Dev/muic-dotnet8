using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Fee;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("TuitionFeeType", "")]
    public class TuitionFeeTypeController : BaseController
    {
        private readonly IFeeProvider _iFeeProvider;

        public TuitionFeeTypeController(ApplicationDbContext db,
                                        IFlashMessage flashMessage,
                                        IFeeProvider iFeeProvider) : base(db, flashMessage)
        {
            _iFeeProvider = iFeeProvider;
        }

        public IActionResult Index(int page)
        {
            var models = _db.TuitionFeeTypes.IgnoreQueryFilters()
                                            .GetPaged(page, true);

            return View(models);
        }

        [PermissionAuthorize("TuitionFeeType", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new TuitionFeeType());
        }

        [PermissionAuthorize("TuitionFeeType", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TuitionFeeType model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.TuitionFeeTypes.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
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
            var model = _iFeeProvider.GetTuitionFeeType(id);
            return View(model);
        }

        [PermissionAuthorize("TuitionFeeType", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = _iFeeProvider.GetTuitionFeeType(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<TuitionFeeType>(model))
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

        [PermissionAuthorize("TuitionFeeType", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = _iFeeProvider.GetTuitionFeeType(id);
            try
            {
                _db.TuitionFeeTypes.Remove(model);
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