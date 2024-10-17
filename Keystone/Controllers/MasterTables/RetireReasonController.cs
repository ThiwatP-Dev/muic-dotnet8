using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("RetireReason", "")]
    public class RetireReasonController : BaseController
    {
        public RetireReasonController(ApplicationDbContext db,
                                      IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index()
        {
            var models = _db.RetireReasons.IgnoreQueryFilters()
                                          .ToList();
            return View(models);
        }

        [PermissionAuthorize("RetireReason", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View();
        }

        [PermissionAuthorize("RetireReason", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RetireReason model)
        {
            if (ModelState.IsValid)
            {
                _db.RetireReasons.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            RetireReason model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("RetireReason", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<RetireReason>(model))
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
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("RetireReason", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            RetireReason model = Find(id);
            try
            {
                _db.RetireReasons.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private RetireReason Find(long? id) 
        {
            var model = _db.RetireReasons.IgnoreQueryFilters()
                                         .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}