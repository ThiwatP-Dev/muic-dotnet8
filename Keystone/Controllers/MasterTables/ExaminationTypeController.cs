using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("ExaminationType", "")]
    public class ExaminationTypeController : BaseController
    {
        public ExaminationTypeController(ApplicationDbContext db,
                                         IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index()
        {
            var models = _db.ExaminationTypes.IgnoreQueryFilters()
                                             .ToList();
            return View(models);
        }

        [PermissionAuthorize("ExaminationType", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View();
        }

        [PermissionAuthorize("ExaminationType", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ExaminationType model)
        {
            if (ModelState.IsValid)
            {
                _db.ExaminationTypes.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            ExaminationType model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("ExaminationType", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<ExaminationType>(model))
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

        [PermissionAuthorize("ExaminationType", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            ExaminationType model = Find(id);
            try
            {
                _db.ExaminationTypes.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private ExaminationType Find(long? id) 
        {
            var model = _db.ExaminationTypes.IgnoreQueryFilters()
                                            .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}