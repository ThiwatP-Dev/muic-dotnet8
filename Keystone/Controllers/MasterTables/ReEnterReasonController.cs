using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("ReenterReason", "")]
    public class ReEnterReasonController : BaseController
    {
        public ReEnterReasonController(ApplicationDbContext db,
                                       IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index()
        {
            var models = _db.ReEnterReasons.IgnoreQueryFilters()
                                           .ToList();
            return View(models);
        }

        [PermissionAuthorize("ReenterReason", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View();
        }

        [PermissionAuthorize("ReenterReason", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ReEnterReason model)
        {
            if (ModelState.IsValid)
            {
                _db.ReEnterReasons.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            ReEnterReason model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("ReenterReason", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<ReEnterReason>(model))
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
                    }
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("ReenterReason", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            ReEnterReason model = Find(id);
            try
            {
                _db.ReEnterReasons.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private ReEnterReason Find(long? id) 
        {
            var model = _db.ReEnterReasons.IgnoreQueryFilters()
                                          .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}