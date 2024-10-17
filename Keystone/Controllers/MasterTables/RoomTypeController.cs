using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("RoomType", "")]
    public class RoomTypeController : BaseController
    {
        public RoomTypeController(ApplicationDbContext db,
                                  IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index()
        {
            var models = _db.RoomTypes.IgnoreQueryFilters()
                                      .ToList();
            return View(models);
        }

        [PermissionAuthorize("RoomType", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new RoomType());
        }

        [PermissionAuthorize("RoomType", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RoomType model)
        {
            if (ModelState.IsValid)
            {
                _db.RoomTypes.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            RoomType model = Find(id);
            return View(model);
        }

        [PermissionAuthorize("RoomType", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<RoomType>(model))
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
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("RoomType", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            RoomType model = Find(id);
            try
            {
                _db.RoomTypes.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private RoomType Find(long? id) 
        {
            var model = _db.RoomTypes.IgnoreQueryFilters()
                                     .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}