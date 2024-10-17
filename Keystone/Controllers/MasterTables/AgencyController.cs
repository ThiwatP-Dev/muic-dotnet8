using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Admission;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("Agency", "")]
    public class AgencyController : BaseController
    {
        public AgencyController(ApplicationDbContext db,
                                IFlashMessage flashMessage,
                                ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public ActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList();
            var agencies = _db.Agencies.Include(x => x.Country)
                                       .Where(x => (string.IsNullOrEmpty(criteria.CodeAndName)
                                                    || x.Name.StartsWith(criteria.CodeAndName))
                                                    && (criteria.CountryId == 0
                                                        || x.CountryId == criteria.CountryId))
                                       .OrderBy(x => x.Name)
                                       .IgnoreQueryFilters()
                                       .GetPaged(criteria ,page);
            return View(agencies);
        }

        [PermissionAuthorize("Agency", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            return View(new Agency());
        }

        [PermissionAuthorize("Agency", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Agency model, string returnUrl)
        {
            if (model.AgencyContracts.Count == 0)
            {
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.RequiredData);
                return View(model);
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    _db.Agencies.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new
                                                           {
                                                               CodeAndName = model.Name
                                                           });
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToCreate);
                    CreateSelectList();
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            CreateSelectList();
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = Find(id);
            CreateSelectList();
            return View(model);
        }

        [PermissionAuthorize("Agency", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = Find(id);
            if (model.AgencyContracts == null)
            {
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.RequiredData);
                return View(model);
            }

            if (ModelState.IsValid && await TryUpdateModelAsync<Agency>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new
                                                           {
                                                               CodeAndName = model.Name
                                                           });
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToEdit);
                    CreateSelectList();
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            CreateSelectList();
            return View(model);
        }
        
        public ActionResult Details(long id)
        {    
            var agency = Find(id);
            return PartialView("_DetailsInfo", agency);  
        }

        [PermissionAuthorize("Agency", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = Find(id);

            try
            {
                _db.Agencies.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private Agency Find(long? id)
        {
            var model = _db.Agencies.Include(x => x.AgencyContracts)
                                    .IgnoreQueryFilters()
                                    .SingleOrDefault(x => x.Id == id);

            return model;
        }

        private void CreateSelectList()
        {
            ViewBag.Countries = _selectListProvider.GetCountries();
        }
    }
}