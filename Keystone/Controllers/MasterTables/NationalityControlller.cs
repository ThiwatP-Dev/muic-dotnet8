using Microsoft.AspNetCore.Mvc;
using KeystoneLibrary.Models;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using Keystone.Permission;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("Nationality", "")]
    public class NationalityController : BaseController
    {
        public NationalityController(ApplicationDbContext db,
                                     IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index()
        {
            var models = _db.Nationalities.IgnoreQueryFilters()
                                          .ToList();
            return View(models);
        }

        [PermissionAuthorize("Nationality", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new Nationality());
        }

        [PermissionAuthorize("Nationality", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Nationality model)
        {
            if (ModelState.IsValid)
            {
                _db.Nationalities.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            Nationality model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("Nationality", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<Nationality>(model))
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

        [PermissionAuthorize("Nationality", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Nationality model = Find(id);
            try
            {
                _db.Nationalities.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private Nationality Find(long? id) 
        {
            var model = _db.Nationalities.IgnoreQueryFilters()
                                         .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}