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
    [PermissionAuthorize("Campus", "")]
    public class CampusController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;

        public CampusController(ApplicationDbContext db,
                                IFlashMessage flashMessage,
                                ISelectListProvider selectListProvider,
                                IExceptionManager exceptionManager) : base(db, flashMessage, selectListProvider)
        { 
            _exceptionManager = exceptionManager;
        }
        
        public IActionResult Index()
        {
            var models = _db.Campuses.Include(x => x.Country)
                                     .Include(x => x.Province)
                                     .Include(x => x.District)
                                     .Include(x => x.Subdistrict)
                                     .IgnoreQueryFilters()
                                     .ToList();
            return View(models);
        }

        [PermissionAuthorize("Campus", PolicyGenerator.Write)]
        public IActionResult Create()
        {
            CreateSelectList();
            return View(new Campus());
        }

        [PermissionAuthorize("Campus", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Campus model)
        {
            if (ModelState.IsValid)
            {
                _db.Campuses.Add(model);
                try
                {
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch(Exception e)
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
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            CreateSelectList();
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            Campus model = Find(id);
            CreateSelectList();
            return View(model);
        }

        [PermissionAuthorize("Campus", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<Campus>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch(Exception e)
                {
                    if (_exceptionManager.IsDuplicatedEntityCode(e))
                    {
                        _flashMessage.Danger(Message.CodeUniqueConstraintError);
                    }
                    else
                    {
                        _flashMessage.Danger(Message.UnableToEdit);
                    } 

                    CreateSelectList();
                    return View(model);
                }
            }

            CreateSelectList();  
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("Campus", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Campus model = Find(id);
            try
            {
                _db.Campuses.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private Campus Find(long? id) 
        {
            var model = _db.Campuses.IgnoreQueryFilters()
                                    .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList()
        {
            ViewBag.Countries = _selectListProvider.GetCountries();
            ViewBag.Provinces = _selectListProvider.GetProvinces();
            ViewBag.Districts = _selectListProvider.GetDistricts();
            ViewBag.Subdistricts = _selectListProvider.GetSubdistricts();
        }
    }
}