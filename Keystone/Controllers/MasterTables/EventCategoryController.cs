using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("EventCategory", "")]
    public class EventCategoryController : BaseController
    {
        public EventCategoryController(ApplicationDbContext db,
                                       IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index()
        {
            var models = _db.EventCategories.IgnoreQueryFilters()
                                            .ToList();
            return View(models);
        }

        [PermissionAuthorize("EventCategory", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new EventCategory());
        }

        [PermissionAuthorize("EventCategory", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EventCategory model)
        {
            if (ModelState.IsValid)
            {
                _db.EventCategories.Add(model);
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
            EventCategory model = Find(id);
            return View(model);
        }

        [PermissionAuthorize("EventCategory", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid && await TryUpdateModelAsync<EventCategory>(model))
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

        [PermissionAuthorize("EventCategory", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            EventCategory model = Find(id);
            try
            {
                _db.EventCategories.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private EventCategory Find(long? id) 
        {
            var model = _db.EventCategories.IgnoreQueryFilters()
                                           .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}