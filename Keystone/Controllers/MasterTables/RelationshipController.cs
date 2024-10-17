using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("Relationship", "")]
    public class RelationshipController : BaseController
    {
        public RelationshipController(ApplicationDbContext db,
                                      IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index()
        {
            var models = _db.Relationships.IgnoreQueryFilters()
                                          .ToList();
            return View(models);
        }

        [PermissionAuthorize("Relationship", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new Relationship());
        }

        [PermissionAuthorize("Relationship", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Relationship model)
        {
            if (ModelState.IsValid)
            {
                _db.Relationships.Add(model);
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
            Relationship model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("Relationship", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<Relationship>(model))
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

        [PermissionAuthorize("Relationship", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Relationship model = Find(id);
            try
            {
                _db.Relationships.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private Relationship Find(long? id) 
        {
            var model = _db.Relationships.IgnoreQueryFilters()
                                         .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}