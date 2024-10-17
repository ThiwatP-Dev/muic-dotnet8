using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    public class AdmissionTypeController : BaseController
    {
        public AdmissionTypeController(ApplicationDbContext db,
                                       IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            var models = _db.AdmissionTypes.Where(x => string.IsNullOrEmpty(criteria.CodeAndName)
                                                       || x.NameEn.StartsWith(criteria.CodeAndName)
                                                       || x.NameTh.StartsWith(criteria.CodeAndName))
                                           .IgnoreQueryFilters()
                                           .GetPaged(criteria, page);
            return View(models);
        }

        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new AdmissionType());
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdmissionType model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                _db.AdmissionTypes.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index), new
                                                       {
                                                           CodeAndName = model.NameEn
                                                       });
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            AdmissionType model = Find(id);	
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = Find(id);

            if (ModelState.IsValid && await TryUpdateModelAsync<AdmissionType>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new
                                                           {
                                                               CodeAndName = model.NameEn
                                                           });
                }
                catch 
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        public ActionResult Delete(long id)
        {
            AdmissionType model = Find(id);
            try
            {
                _db.AdmissionTypes.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private AdmissionType Find(long? id) 
        {
            var model = _db.AdmissionTypes.IgnoreQueryFilters()
                                          .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}