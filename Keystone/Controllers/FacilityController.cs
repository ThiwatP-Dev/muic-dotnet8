using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("Facility", "")]
    public class FacilityController : BaseController
    {
        public FacilityController(ApplicationDbContext db,
                                  IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index(int page)
        {
            var models = _db.Facilities.IgnoreQueryFilters()
                                       .GetPaged(page);
            return View(models);
        }

        [PermissionAuthorize("Facility", PolicyGenerator.Write)]
        public IActionResult Create()
        {
            return View(new Facility());
        }

        [PermissionAuthorize("Facility", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Facility model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.Facilities.Add(model);
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

        public IActionResult Edit(long id)
        {
            var model = Find(id);
            return View(model);
        }

        [PermissionAuthorize("Facility", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<Facility>(model))
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

        [PermissionAuthorize("Facility", PolicyGenerator.Write)]
        public IActionResult Delete(long id)
        {
            var model = Find(id);
            try
            {
                _db.Facilities.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private Facility Find(long? id) 
        {
            var facility = _db.Facilities.IgnoreQueryFilters()
                                         .SingleOrDefault(x => x.Id == id);
            return facility;
        }
    }
}