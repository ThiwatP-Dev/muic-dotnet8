using Microsoft.AspNetCore.Mvc;
using KeystoneLibrary.Models;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Vereyon.Web;
using Microsoft.EntityFrameworkCore;
using Keystone.Permission;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("Channel", "")]
    public class ChannelController : BaseController
    {
        public ChannelController(ApplicationDbContext db,
                                 IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index()
        {
            var models = _db.Channels.IgnoreQueryFilters()
                                     .ToList();
            return View(models);
        }

        [PermissionAuthorize("Channel", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new Channel());
        }

        [PermissionAuthorize("Channel", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Channel model)
        {
            if (ModelState.IsValid)
            {
                _db.Channels.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            Channel model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("Channel", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<Channel>(model))
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

        [PermissionAuthorize("Channel", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Channel model = Find(id);
            try
            {
                _db.Channels.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete); 
            }

            return RedirectToAction(nameof(Index));
        }

        private Channel Find(long? id) 
        {
            var model = _db.Channels.IgnoreQueryFilters()
                                    .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}