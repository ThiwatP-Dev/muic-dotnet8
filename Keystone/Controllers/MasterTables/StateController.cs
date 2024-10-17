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
    [PermissionAuthorize("State", "")]
    public class StateController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;

        public StateController(ApplicationDbContext db,
                               IFlashMessage flashMessage,
                               ISelectListProvider selectListProvider,
                               IExceptionManager exceptionManager) : base(db, flashMessage, selectListProvider)
        {
            _exceptionManager = exceptionManager;
        }

        public IActionResult Index()
        {
            var models = _db.States.Include(x => x.Country)
                                   .IgnoreQueryFilters()
                                   .ToList();
            return View(models);
        }

        [PermissionAuthorize("State", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View(new State());
        }

        [PermissionAuthorize("State", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(State model)
        {
            if (ModelState.IsValid)
            {
                _db.States.Add(model);
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
            State model = Find(id);
            CreateSelectList(model.CountryId);
            return View(model);
        }

        [PermissionAuthorize("State", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<State>(model))
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

        [PermissionAuthorize("State", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            State model = Find(id);
            try
            {
                _db.States.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private State Find(long? id) 
        {
            var model = _db.States.IgnoreQueryFilters()
                                  .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList(long countryId = 0)
        {
            ViewBag.Countries = _selectListProvider.GetCountries();
            ViewBag.Cities = countryId == 0 ? _selectListProvider.GetCities() : _selectListProvider.GetCities(countryId);
        }    
    }
}