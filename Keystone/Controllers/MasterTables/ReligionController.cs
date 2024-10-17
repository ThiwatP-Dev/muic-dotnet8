using Microsoft.AspNetCore.Mvc;
using KeystoneLibrary.Models;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using Keystone.Permission;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("Religion", "")]
    public class ReligionController : BaseController
    {
        public ReligionController(ApplicationDbContext db,
                                  IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index()
        {
            var models = _db.Religions.IgnoreQueryFilters()
                                      .ToList();
            return View(models);
        }

        [PermissionAuthorize("Religion", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new Religion());
        }

        [PermissionAuthorize("Religion", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Religion model)
        {
            if (ModelState.IsValid)
            {
                _db.Religions.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            Religion model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("Religion", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<Religion>(model))
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

        [PermissionAuthorize("Religion", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Religion model = Find(id);
            try
            {
                _db.Religions.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private Religion Find(long? id) 
        {
            var model = _db.Religions.IgnoreQueryFilters()
                                     .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}