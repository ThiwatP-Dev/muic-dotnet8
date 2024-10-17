using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    public class PreviousSchoolController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;
        protected readonly IAdmissionProvider _admissionProvider;

        public PreviousSchoolController(ApplicationDbContext db,
                                        IFlashMessage flashMessage,
                                        ISelectListProvider selectListProvider,
                                        IExceptionManager exceptionManager,
                                        IAdmissionProvider admissionProvider) : base(db, flashMessage, selectListProvider) 
        { 
            _exceptionManager = exceptionManager;
            _admissionProvider = admissionProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.CountryId);
            if (criteria.CountryId == 0 && criteria.ProvinceId == 0 && criteria.SchoolTypeId == 0 && criteria.SchoolGroupId == 0
                && criteria.SchoolTerritoryId == 0 && criteria.StateId == 0 && string.IsNullOrEmpty(criteria.CodeAndName))
            {
                return View();
            }

            var models = _db.PreviousSchools.Include(x => x.Country)
                                            .Include(x => x.Province)
                                            .Include(x => x.State)
                                            .Include(x => x.SchoolTerritory)
                                            .Include(x => x.SchoolType)
                                            .Include(x => x.SchoolGroup)
                                            .Where(x => (String.IsNullOrEmpty(criteria.CodeAndName)
                                                         || x.NameEn.StartsWith(criteria.CodeAndName)
                                                         || x.NameTh.StartsWith(criteria.CodeAndName))
                                                         && (string.IsNullOrEmpty(criteria.Code)
                                                             || x.Code.StartsWith(criteria.Code))
                                                         && (criteria.CountryId == 0
                                                             || criteria.CountryId == x.CountryId)
                                                         && (criteria.ProvinceId == 0
                                                             || criteria.ProvinceId == x.ProvinceId)
                                                         && (criteria.StateId == 0
                                                             || criteria.StateId == x.StateId)
                                                         && (criteria.SchoolTypeId == 0
                                                             || criteria.SchoolTypeId == x.SchoolTypeId)
                                                         && (criteria.SchoolTerritoryId == 0
                                                             || criteria.SchoolTerritoryId == x.SchoolTerritoryId)
                                                         && (criteria.SchoolGroupId == 0
                                                             || criteria.SchoolGroupId == x.SchoolGroupId))
                                            .IgnoreQueryFilters()
                                            .GetPaged(criteria, page);
            return View(models);
        }

        public IActionResult Details(long id)
        {
            var school = _admissionProvider.GetPreviousSchool(id);
            return PartialView("_DetailsInfo", school);
        }

        public ActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new PreviousSchool());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PreviousSchool model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                _db.PreviousSchools.Add(model);
                try
                {
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new
                                                           {
                                                               CodeAndName = model.NameEn
                                                           });
                }
                catch (Exception e)
                {
                    if (_exceptionManager.IsDuplicatedEntityCode(e))
                    {
                        _flashMessage.Danger(Message.CodeUniqueConstraintError);
                    }
                    else
                    {
                        _flashMessage.Danger(Message.UnableToCreate);
                    }

                    ViewBag.ReturnUrl = returnUrl;
                    CreateSelectList(model.CountryId);
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.CountryId);
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            PreviousSchool model = Find(id);
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.CountryId);	
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = Find(id);

            if (ModelState.IsValid && await TryUpdateModelAsync<PreviousSchool>(model))
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
                catch (Exception e)
                { 
                    if (_exceptionManager.IsDuplicatedEntityCode(e))
                    {
                        _flashMessage.Danger(Message.CodeUniqueConstraintError);
                    }
                    else
                    {
                        _flashMessage.Danger(Message.UnableToEdit);
                    }
                    
                    ViewBag.ReturnUrl = returnUrl;
                    CreateSelectList(model.CountryId);
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.CountryId);
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        public ActionResult Delete(long id)
        {
            PreviousSchool model = Find(id);
            try
            {
                _db.PreviousSchools.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private PreviousSchool Find(long? id) 
        {
            var model = _db.PreviousSchools.IgnoreQueryFilters()
                                           .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList(long countryId = 0)
        {
            ViewBag.Countries = _selectListProvider.GetCountries();
            ViewBag.SchoolGroups = _selectListProvider.GetSchoolGroup();
            if (countryId != 0)
            {
                ViewBag.Provinces = _selectListProvider.GetProvinces(countryId);
                ViewBag.States = _selectListProvider.GetStates(countryId);
            }

            ViewBag.SchoolTypes = _selectListProvider.GetSchoolTypes();
            ViewBag.SchoolTerritories = _selectListProvider.GetSchoolTerritories();
        } 
    }
}