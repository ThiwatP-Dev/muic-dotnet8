using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTable
{
    [PermissionAuthorize("Document", "")]
    public class DocumentController : BaseController
    {
        public DocumentController(ApplicationDbContext db,
                                  IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index(int page = 1)
        {
            var models = _db.Documents.OrderBy(x => x.NameEn)
                                      .GetPaged(page);
            return View(models);
        }

        [PermissionAuthorize("Document", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new Document());
        }

        [PermissionAuthorize("Document", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Document model)
        {
            if (ModelState.IsValid)
            {
                _db.Documents.Add(model);
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
            Document model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("Document", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<Document>(model))
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

        [PermissionAuthorize("Document", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Document model = Find(id);
            try
            {
                _db.Documents.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private Document Find(long? id) 
        {
            var model = _db.Documents.IgnoreQueryFilters()
                                     .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}