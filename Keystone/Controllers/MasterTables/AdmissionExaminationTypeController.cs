using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTable
{
    public class AdmissionExaminationTypeController : BaseController
    {
        public AdmissionExaminationTypeController(ApplicationDbContext db,
                                                  IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index(int page = 1)
        {
            var models = _db.AdmissionExaminationTypes.IgnoreQueryFilters()
                                                      .OrderBy(x => x.NameEn)
                                                      .GetPaged(page);
            return View(models);
        }

        public ActionResult Create()
        {
            return View(new AdmissionExaminationType());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdmissionExaminationType model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.AdmissionExaminationTypes.Add(model);
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
            AdmissionExaminationType model = Find(id);	
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<AdmissionExaminationType>(model))
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

        public ActionResult Delete(long id)
        {
            AdmissionExaminationType model = Find(id);
            try
            {
                _db.AdmissionExaminationTypes.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private AdmissionExaminationType Find(long? id) 
        {
            var model = _db.AdmissionExaminationTypes.IgnoreQueryFilters()
                                                     .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}