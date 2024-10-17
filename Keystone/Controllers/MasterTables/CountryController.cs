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
    [PermissionAuthorize("Country", "")]
    public class CountryController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;

        public CountryController(ApplicationDbContext db,
                                 IFlashMessage flashMessage,
                                 IExceptionManager exceptionManager) : base(db, flashMessage) 
        {
            _exceptionManager = exceptionManager;
        }

        public IActionResult Index()
        {
            var models = _db.Countries.IgnoreQueryFilters()
                                      .ToList();
            return View(models);
        }

        [PermissionAuthorize("Country", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new Country());
        }

        [PermissionAuthorize("Country", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Country model)
        {
            if (ModelState.IsValid)
            {
                _db.Countries.Add(model);
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

                    return View(model);
                }
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            Country model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("Country", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid && await TryUpdateModelAsync<Country>(model))
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
                    
                    return View(model);
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("Country", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Country model = Find(id);
            try
            {
                _db.Countries.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private Country Find(long? id) 
        {
            var model = _db.Countries.IgnoreQueryFilters()
                                     .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}