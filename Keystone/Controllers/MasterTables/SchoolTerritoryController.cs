using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    public class SchoolTerritoryController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;

        public SchoolTerritoryController(ApplicationDbContext db,
                                         IFlashMessage flashMessage,
                                         ISelectListProvider selectListProvider,
                                         IExceptionManager exceptionManager) : base(db, flashMessage, selectListProvider)
        {
            _exceptionManager = exceptionManager;
        }
        
        public IActionResult Index(int page, Criteria criteria)
        {
            var models = _db.SchoolTerritories.IgnoreQueryFilters()
                                              .GetPaged(criteria, page);

            return View(models);
        }

        public ActionResult Create()
        {
            return View(new SchoolTerritory());
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SchoolTerritory model)
        {
            if (ModelState.IsValid)
            {
                _db.SchoolTerritories.Add(model);

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
                    
                    return View(model);
                }   
            }
            
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            SchoolTerritory model = Find(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid && await TryUpdateModelAsync<SchoolTerritory>(model))
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
                    
                    return View(model);
                }
            }
            
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        public ActionResult Delete(long id)
        {
            SchoolTerritory model = Find(id);

            try
            {
                _db.SchoolTerritories.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete); 
            }
            
            return RedirectToAction(nameof(Index));
        }

        private SchoolTerritory Find(long? id) 
        {
            var model = _db.SchoolTerritories.IgnoreQueryFilters()
                                             .SingleOrDefault(x => x.Id == id);

            return model;
        } 
    }
}