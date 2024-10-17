using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("ResignReason", "")]
    public class ResignReasonController : BaseController
    {
        public ResignReasonController(ApplicationDbContext db,
                                      IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index()
        {
            var models = _db.ResignReasons.IgnoreQueryFilters()
                                          .ToList();
            return View(models);
        }

        [PermissionAuthorize("ResignReason", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View();
        }

        [PermissionAuthorize("ResignReason", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ResignReason model)
        {
            if (ModelState.IsValid)
            {
                _db.ResignReasons.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            ResignReason model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("ResignReason", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<ResignReason>(model))
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

        [PermissionAuthorize("ResignReason", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            ResignReason model = Find(id);
            try
            {
                _db.ResignReasons.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete); 
            }
            
            return RedirectToAction(nameof(Index));
        }

        private ResignReason Find(long? id) 
        {
            var model = _db.ResignReasons.IgnoreQueryFilters()
                                         .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}