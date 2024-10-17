using KeystoneLibrary.Data;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using Microsoft.EntityFrameworkCore;
using KeystoneLibrary.Interfaces;

namespace Keystone.Controllers.MasterTables
{
    public class ResidentTypeController : BaseController
    {
        protected readonly IMasterProvider _masterProvider;

        public ResidentTypeController(ApplicationDbContext db,
                                      IFlashMessage flashMessage,
                                      IMasterProvider masterProvider) : base(db, flashMessage)
        {
            _masterProvider = masterProvider;
        }

        public ActionResult Index(Criteria criteria, int page)
        {
            var types = _db.ResidentTypes.Where(x => string.IsNullOrEmpty(criteria.CodeAndName)
                                                     || x.NameEn.StartsWith(criteria.CodeAndName)
                                                     || x.NameTh.StartsWith(criteria.CodeAndName))
                                         .IgnoreQueryFilters()
                                         .GetPaged(criteria ,page);
            return View(types);
        }

        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new ResidentType());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ResidentType model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.ResidentTypes.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new
                                                           {
                                                               CodeAndName = model.NameEn
                                                           });
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = _masterProvider.FindResidentType(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = _masterProvider.FindResidentType(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<ResidentType>(model))
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
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        } 

        public ActionResult Delete(long id)
        {
            var model = _masterProvider.FindResidentType(id);
            try
            {
                _db.ResidentTypes.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}