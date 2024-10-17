using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTable
{
    [PermissionAuthorize("City", "")]
    public class CityController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;

        public CityController(ApplicationDbContext db,
                              IFlashMessage flashMessage,
                              ISelectListProvider selectListProvider,
                              IExceptionManager exceptionManager) : base(db, flashMessage, selectListProvider)
        {
            _exceptionManager = exceptionManager;
        }
        
        public IActionResult Index()
        {
            var models = _db.Cities.Include(x => x.Country)
                                   .Include(x => x.State)
                                   .IgnoreQueryFilters()
                                   .ToList();
            return View(models);
        }

        [PermissionAuthorize("City", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View(new City());
        }

        [PermissionAuthorize("City", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(City model)
        {
            if (ModelState.IsValid)
            {
                _db.Cities.Add(model);
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
            City model = Find(id);
            return View(model);
        }

        [PermissionAuthorize("City", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<City>(model))
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

        [PermissionAuthorize("City", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            City model = Find(id);
            try
            {
                _db.Cities.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete); 
            }
            
            return RedirectToAction(nameof(Index));
        }

        private City Find(long? id) 
        {
            var model = _db.Cities.IgnoreQueryFilters()
                                  .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList()
        {
            ViewBag.Countries = _selectListProvider.GetCountries();
            ViewBag.States = _selectListProvider.GetStates();
        }    
    }
}