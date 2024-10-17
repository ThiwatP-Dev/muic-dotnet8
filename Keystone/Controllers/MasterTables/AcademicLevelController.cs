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
    [PermissionAuthorize("AcademicLevel", "")]
    public class AcademicLevelController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;

        public AcademicLevelController(ApplicationDbContext db,
                                       IExceptionManager exceptionManager,
                                       IFlashMessage flashMessage) : base(db, flashMessage) 
        { 
            _exceptionManager = exceptionManager;
        }

        public IActionResult Index()
        {
            var models = _db.AcademicLevels.IgnoreQueryFilters()
                                           .ToList();
            return View(models);
        }

        [PermissionAuthorize("AcademicLevel", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new AcademicLevel());
        }

        [PermissionAuthorize("AcademicLevel", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AcademicLevel model)
        {
            if (ModelState.IsValid)
            {
                _db.AcademicLevels.Add(model);
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
            AcademicLevel model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("AcademicLevel", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid && await TryUpdateModelAsync<AcademicLevel>(model))
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

        [PermissionAuthorize("AcademicLevel", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            AcademicLevel model = Find(id);
            try
            {
                _db.AcademicLevels.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch 
            {
                _flashMessage.Danger(Message.UnableToDelete); 
            }
            
            return RedirectToAction(nameof(Index));
        }

        private AcademicLevel Find(long? id) 
        {
            var model = _db.AcademicLevels.IgnoreQueryFilters()
                                          .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}