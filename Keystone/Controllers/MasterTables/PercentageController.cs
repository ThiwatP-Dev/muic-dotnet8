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
    [PermissionAuthorize("Percentage", "")]
    public class PercentageController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;

        public PercentageController(ApplicationDbContext db,
                                    IFlashMessage flashMessage,
                                    ISelectListProvider selectListProvide,
                                    IExceptionManager exceptionManager) : base(db, flashMessage, selectListProvide) 
        {
            _exceptionManager = exceptionManager;
        }

        public IActionResult Index(int page)
        {
            var models = _db.Percentages.IgnoreQueryFilters()
                                        .OrderBy(x => x.Value)
                                        .GetPaged(page);
            return View(models);
        }

        [PermissionAuthorize("Percentage", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new Percentage());
        }

        [PermissionAuthorize("Percentage", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Percentage model)
        {
            if (ModelState.IsValid)
            {
                _db.Percentages.Add(model);
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
            Percentage model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("Percentage", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            Percentage model = Find(id);

            if (ModelState.IsValid && await TryUpdateModelAsync<Percentage>(model))
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

        [PermissionAuthorize("Percentage", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Percentage model = Find(id);
            try
            {
                _db.Percentages.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private Percentage Find(long? id) 
        {
            var model = _db.Percentages.IgnoreQueryFilters()
                                       .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}