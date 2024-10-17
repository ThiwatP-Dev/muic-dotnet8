using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    public class AdmissionChannelController : BaseController
    {
        public AdmissionChannelController(ApplicationDbContext db,
                                          IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            var models = _db.AdmissionChannels.Where(x => string.IsNullOrEmpty(criteria.CodeAndName)
                                                          || x.NameEn.StartsWith(criteria.CodeAndName)
                                                          || x.NameTh.StartsWith(criteria.CodeAndName))
                                              .OrderBy(x => x.NameEn)
                                              .IgnoreQueryFilters()
                                              .GetPaged(criteria, page);
            return View(models);
        }

        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new AdmissionChannel());
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdmissionChannel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                _db.AdmissionChannels.Add(model);
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
            AdmissionChannel model = Find(id);	
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = Find(id);

            if (ModelState.IsValid && await TryUpdateModelAsync<AdmissionChannel>(model))
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
            AdmissionChannel model = Find(id);
            try
            {
                _db.AdmissionChannels.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private AdmissionChannel Find(long? id) 
        {
            var model = _db.AdmissionChannels.IgnoreQueryFilters()
                                             .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}