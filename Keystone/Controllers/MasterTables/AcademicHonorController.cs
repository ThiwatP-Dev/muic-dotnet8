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
    [PermissionAuthorize("AcademicHonor", "")]
    public class AcademicHonorController : BaseController
    {
        public AcademicHonorController(ApplicationDbContext db,
                                       IFlashMessage flashMessage,
                                       ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index()
        {
            var models = _db.AcademicHonors.Include(x => x.AcademicLevel)
                                           .IgnoreQueryFilters()
                                           .ToList();
            return View(models);
        }

        [PermissionAuthorize("AcademicHonor", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View(new AcademicHonor());
        }

        [PermissionAuthorize("AcademicHonor", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AcademicHonor model)
        {
            CreateSelectList();
            if (ModelState.IsValid)
            {
                _db.AcademicHonors.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            CreateSelectList();
            AcademicHonor model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("AcademicHonor", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            CreateSelectList();
            var model = Find(id);
            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<AcademicHonor>(model))
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

        [PermissionAuthorize("AcademicHonor", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            AcademicHonor model = Find(id);
            try
            {
                _db.AcademicHonors.Remove(model);
                _flashMessage.Confirmation(Message.SaveSucceed);
                _db.SaveChanges();
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private AcademicHonor Find(long? id) 
        {
            var model = _db.AcademicHonors.IgnoreQueryFilters()
                                          .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList()
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
        }
    }
}