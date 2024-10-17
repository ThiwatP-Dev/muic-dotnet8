using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("Sponsor", "")]
    public class SponsorController : BaseController
    {
        public SponsorController(ApplicationDbContext db,
                                 IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index(int page)
        {
            var models = _db.Sponsors.IgnoreQueryFilters()
                                     .GetPaged(page);
            return View(models);
        }

        [PermissionAuthorize("Sponsor", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new Sponsor());
        }

        [PermissionAuthorize("Sponsor", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Sponsor model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.Sponsors.Add(model);
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
            Sponsor model = Find(id);
            return View(model);
        }

        [PermissionAuthorize("Sponsor", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            Sponsor model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<Sponsor>(model))
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

        [PermissionAuthorize("Sponsor", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Sponsor model = Find(id);
            try
            {
                _db.Sponsors.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private Sponsor Find(long? id)
        {
            var model = _db.Sponsors.IgnoreQueryFilters()
                                    .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}