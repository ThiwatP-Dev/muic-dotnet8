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
    [PermissionAuthorize("ScholarshipType", "")]
    public class ScholarshipTypeController : BaseController
    {
        public ScholarshipTypeController(ApplicationDbContext db,
                                         IExceptionManager exceptionManager,
                                         IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index(int page)
        {
            var models = _db.ScholarshipTypes.IgnoreQueryFilters()
                                             .GetPaged(page);
            return View(models);
        }

        [PermissionAuthorize("ScholarshipType", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new ScholarshipType());
        }

        [PermissionAuthorize("ScholarshipType", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ScholarshipType model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.ScholarshipTypes.Add(model);
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
            ScholarshipType model = Find(id);
            return View(model);
        }

        [PermissionAuthorize("ScholarshipType", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            ScholarshipType model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<ScholarshipType>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(model);
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("ScholarshipType", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            ScholarshipType model = Find(id);
            try
            {
                _db.ScholarshipTypes.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch (Exception)
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private ScholarshipType Find(long? id)
        {
            var model = _db.ScholarshipTypes.IgnoreQueryFilters()
                                            .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}