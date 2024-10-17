using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTable
{
    public class AdmissionPlaceController : BaseController
    {
        protected readonly IMasterProvider _masterProvider;
        
        public AdmissionPlaceController(ApplicationDbContext db,
                                        IFlashMessage flashMessage,
                                        IMasterProvider masterProvider) : base(db, flashMessage)
        {
            _masterProvider = masterProvider;
        }

        public IActionResult Index()
        {
            var models = _db.AdmissionPlaces.IgnoreQueryFilters()
                                            .ToList();
            return View(models);
        }

        public ActionResult Create()
        {
            return View(new AdmissionPlace());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdmissionPlace model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.AdmissionPlaces.Add(model);
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
            AdmissionPlace model = _masterProvider.GetAdmissionPlace(id);	
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            AdmissionPlace model = _masterProvider.GetAdmissionPlace(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<AdmissionPlace>(model))
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

        public ActionResult Delete(long id)
        {
            AdmissionPlace model = _masterProvider.GetAdmissionPlace(id);
            try
            {
                _db.AdmissionPlaces.Remove(model);
                _flashMessage.Confirmation(Message.SaveSucceed);
                _db.SaveChanges();
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }                  
    }
}