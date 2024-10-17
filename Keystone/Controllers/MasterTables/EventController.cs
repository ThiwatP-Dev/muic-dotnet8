using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("Event", "")]
    public class EventController : BaseController
    {
        public EventController(ApplicationDbContext db,
                               IFlashMessage flashMessage,
                               ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }
                               
        public ActionResult Index()
        {
            var models = _db.Events.Include(x => x.EventCategory)
                                   .IgnoreQueryFilters()
                                   .ToList();
            return View(models);
        }

        [PermissionAuthorize("Event", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View();
        }

        [PermissionAuthorize("Event", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Event model)
        {
            if (ModelState.IsValid)
            { 
                _db.Events.Add(model);
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
            CreateSelectList();
            Event model = Find(id);
            return View(model);
        }

        [PermissionAuthorize("Event", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<Event>(model))
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

        [PermissionAuthorize("Event", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Event model = Find(id);
            try
            {
                _db.Events.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private void CreateSelectList() 
        {
            ViewBag.EventCategories = _selectListProvider.GetEventCategories();
        }

        private Event Find(long? id) 
        {
            var model = _db.Events.IgnoreQueryFilters()
                                  .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}
