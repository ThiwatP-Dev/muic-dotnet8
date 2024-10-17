using Microsoft.AspNetCore.Mvc;
using KeystoneLibrary.Models;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using Keystone.Permission;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("Race", "")]
    public class RaceController : BaseController
    {
        public RaceController(ApplicationDbContext db,
                              IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index()
        {
            var models = _db.Races.IgnoreQueryFilters()
                                  .ToList();
            return View(models);
        }

        [PermissionAuthorize("Race", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new Race());
        }

        [PermissionAuthorize("Race", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Race model)
        {
            if (ModelState.IsValid)
            {
                _db.Races.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            Race model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("Race", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<Race>(model))
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

        [PermissionAuthorize("Race", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Race model = Find(id);
            try
            {
                _db.Races.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private Race Find(long? id) 
        {
            var model = _db.Races.IgnoreQueryFilters()
                                 .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}