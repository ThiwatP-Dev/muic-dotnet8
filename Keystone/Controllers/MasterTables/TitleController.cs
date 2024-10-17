using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("Title", "")]
    public class TitleController : BaseController
    {
        public TitleController(ApplicationDbContext db,
                               IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index(int page)
        {
            var models = _db.Titles.IgnoreQueryFilters()
                                   .OrderBy(x => x.Order)
                                   .GetPaged(page);
            return View(models);
        }

        [PermissionAuthorize("Title", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new Title());
        }

        [PermissionAuthorize("Title", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Title model)
        {
            if (ModelState.IsValid)
            {
                _db.Titles.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            Title model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("Title", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<Title>(model))
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

        [PermissionAuthorize("Title", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Title model = Find(id);
            try
            {
                _db.Titles.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private Title Find(long? id) 
        {
            var model = _db.Titles.IgnoreQueryFilters()
                                  .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}