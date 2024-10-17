using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("TermType", "")]
    public class TermTypeController : BaseController
    {
        public TermTypeController(ApplicationDbContext db,
                                  IFlashMessage flashMessage,
                                  ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(int page = 1)
        {
            var models = _db.TermTypes.IgnoreQueryFilters()
                                      .GetPaged(page);   
            return View(models);
        }

        [PermissionAuthorize("TermType", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new TermType());
        }

        [PermissionAuthorize("TermType", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TermType model)
        {
            if (ModelState.IsValid)
            {
                _db.TermTypes.Add(model);
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
            TermType model = Find(id);
            return View(model);
        }

        [PermissionAuthorize("TermType", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<TermType>(model))
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

        [PermissionAuthorize("TermType", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            TermType model = Find(id);
            try
            {
                _db.TermTypes.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete); 
            }
            
            return RedirectToAction(nameof(Index));
        }

        private TermType Find(long? id) 
        {
            var model = _db.TermTypes.IgnoreQueryFilters()
                                     .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}