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
    [PermissionAuthorize("District", "")]
    public class DistrictController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;

        public DistrictController(ApplicationDbContext db,
                                  IFlashMessage flashMessage,
                                  ISelectListProvider selectListProvider,
                                  IExceptionManager exceptionManager) : base(db, flashMessage, selectListProvider) 
        {
            _exceptionManager = exceptionManager;
        }

        public IActionResult Index()
        {
            var models = _db.Districts.Include(x => x.Province)
                                      .Include(x => x.Country)
                                      .IgnoreQueryFilters()
                                      .ToList();
            return View(models);
        }

        [PermissionAuthorize("District", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View(new District());
        }

        [PermissionAuthorize("District", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(District model)
        {
            if (ModelState.IsValid)
            {
                _db.Districts.Add(model);
                try
                {
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
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

                    CreateSelectList();
                    return View(model);
                }
            }

            CreateSelectList();
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            District model = Find(id);
            CreateSelectList(model.CountryId);
            return View(model);
        }

        [PermissionAuthorize("District", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid && await TryUpdateModelAsync<District>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
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
                    
                    CreateSelectList(model.CountryId);
                    return View(model);
                }
            }

            CreateSelectList(model.CountryId);
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("District", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            District model = Find(id);
            try
            {
                _db.Districts.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private District Find(long? id) 
        {
            var model = _db.Districts.IgnoreQueryFilters()
                                     .SingleOrDefault(x => x.Id == id);
            return model;
        }
        
        private void CreateSelectList(long countryId = 0)
        {
            ViewBag.Countries = _selectListProvider.GetCountries();
            ViewBag.Provinces = countryId == 0 ? _selectListProvider.GetProvinces() : _selectListProvider.GetProvinces(countryId);
        }
    }
}