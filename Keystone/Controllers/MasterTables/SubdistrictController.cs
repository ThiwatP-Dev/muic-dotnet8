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
    [PermissionAuthorize("Subdistrict", "")]
    public class SubdistrictController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;

        public SubdistrictController(ApplicationDbContext db,
                                     IFlashMessage flashMessage,
                                     ISelectListProvider selectListProvider,
                                     IExceptionManager exceptionManager) : base(db, flashMessage, selectListProvider) 
        { 
            _exceptionManager = exceptionManager;
        }

        public IActionResult Index()
        {
            var models = _db.Subdistricts.Include(x => x.District)
                                         .Include(x => x.Province)
                                         .Include(x => x.Country)
                                         .IgnoreQueryFilters()
                                         .ToList();
            return View(models);
        }

        [PermissionAuthorize("Subdistrict", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View(new Subdistrict());
        }

        [PermissionAuthorize("Subdistrict", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Subdistrict model)
        {
            if (ModelState.IsValid)
            {
                _db.Subdistricts.Add(model);
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
            CreateSelectList();
            Subdistrict model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("Subdistrict", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid && await TryUpdateModelAsync<Subdistrict>(model))
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
                    
                    CreateSelectList();
                    return View(model);
                }
            }
            
            CreateSelectList();
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("Subdistrict", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Subdistrict model = Find(id);
            try
            {
                _db.Subdistricts.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private Subdistrict Find(long? id) 
        {
            var model = _db.Subdistricts.IgnoreQueryFilters()
                                        .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList() 
        {
            ViewBag.Countries = _selectListProvider.GetCountries();
            ViewBag.Provinces = _selectListProvider.GetProvinces();
            ViewBag.Districts = _selectListProvider.GetDistricts();
        }
    }
}