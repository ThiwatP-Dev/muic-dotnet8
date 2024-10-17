using Microsoft.AspNetCore.Mvc;
using KeystoneLibrary.Models;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using Keystone.Permission;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("RegistrationStatus", "")]
    public class RegistrationStatusController : BaseController
    {
        public RegistrationStatusController(ApplicationDbContext db,
                                            IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index()
        {
            var models = _db.RegistrationStatuses.IgnoreQueryFilters()
                                                 .ToList();
            return View(models);
        }

        [PermissionAuthorize("RegistrationStatus", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new RegistrationStatus());
        }

        [PermissionAuthorize("RegistrationStatus", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RegistrationStatus model)
        {
            if (ModelState.IsValid)
            {
                _db.RegistrationStatuses.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            RegistrationStatus model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("RegistrationStatus", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            RegistrationStatus model = Find(id);

            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<RegistrationStatus>(model))
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

        [PermissionAuthorize("RegistrationStatus", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            RegistrationStatus model = Find(id);
            try
            {
                _db.RegistrationStatuses.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private RegistrationStatus Find(long? id) 
        {
            var model = _db.RegistrationStatuses.IgnoreQueryFilters()
                                                .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}