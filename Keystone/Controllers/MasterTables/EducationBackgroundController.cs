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
    [PermissionAuthorize("EducationBackground", "")]
    public class EducationBackgroundController : BaseController
    {
        public EducationBackgroundController(ApplicationDbContext db,
                                             IFlashMessage flashMessage,
                                             ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(int page)
        { 
            var models = _db.EducationBackgrounds.Include(x => x.Country)
                                                 .IgnoreQueryFilters()
                                                 .GetPaged(page);
            return View(models);
        }

        [PermissionAuthorize("EducationBackground", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View(new EducationBackground());
        }

        [PermissionAuthorize("EducationBackground", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EducationBackground model)
        {
            if (ModelState.IsValid)
            {
                _db.EducationBackgrounds.Add(model);
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
            EducationBackground model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("EducationBackground", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<EducationBackground>(model))
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

        [PermissionAuthorize("EducationBackground", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            EducationBackground model = Find(id);
            try
            {
                _db.EducationBackgrounds.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete); 
            }

            return RedirectToAction(nameof(Index));
        }

        private EducationBackground Find(long? id) 
        {
            var model = _db.EducationBackgrounds.IgnoreQueryFilters()
                                                .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList()
        {
            ViewBag.Countries = _selectListProvider.GetCountries();
        }
    }
}