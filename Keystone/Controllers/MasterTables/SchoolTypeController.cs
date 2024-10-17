using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    public class SchoolTypeController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;

        public SchoolTypeController(ApplicationDbContext db,
                                    IFlashMessage flashMessage,
                                    ISelectListProvider selectListProvider,
                                    IExceptionManager exceptionManager) : base(db, flashMessage, selectListProvider)
        {
            _exceptionManager = exceptionManager;
        }
        
        public IActionResult Index(int page, Criteria criteria)
        {
            var models = _db.SchoolTypes.IgnoreQueryFilters()
                                        .GetPaged(criteria, page);

            return View(models);
        }

        public ActionResult Create()
        {
            return View(new SchoolType());
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SchoolType model)
        {
            if (ModelState.IsValid)
            {
                _db.SchoolTypes.Add(model);

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
            SchoolType model = Find(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid && await TryUpdateModelAsync<SchoolType>(model))
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
            SchoolType model = Find(id);

            try
            {
                _db.SchoolTypes.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete); 
            }
            
            return RedirectToAction(nameof(Index));
        }

        private SchoolType Find(long? id) 
        {
            var model = _db.SchoolTypes.IgnoreQueryFilters()
                                       .SingleOrDefault(x => x.Id == id);

            return model;
        } 
    }
}