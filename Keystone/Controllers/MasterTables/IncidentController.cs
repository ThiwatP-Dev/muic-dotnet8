using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("Incident", "")]
    public class IncidentController : BaseController
    {
        public IncidentController(ApplicationDbContext db,
                                  IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index()
        {
            var models = _db.Incidents.IgnoreQueryFilters()
                                      .OrderBy(x => x.NameEn)
                                      .ToList();
            return View(models);
        }

        [PermissionAuthorize("Incident", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new Incident());
        }

        [PermissionAuthorize("Incident", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Incident model)
        {
            if (ModelState.IsValid)
            {
                _db.Incidents.Add(model);
                try
                {
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
            Incident model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("Incident", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            Incident model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<Incident>(model))
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

        [PermissionAuthorize("Incident", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Incident model = Find(id);
            try
            {
                _db.Incidents.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private Incident Find(long? id) 
        {
            var model = _db.Incidents.IgnoreQueryFilters()
                                     .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}