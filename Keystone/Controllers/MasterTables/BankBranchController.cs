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
    [PermissionAuthorize("BankBranch", "")]
    public class BankBranchController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;

        public BankBranchController(ApplicationDbContext db,
                                    IExceptionManager exceptionManager,
                                    IFlashMessage flashMessage) : base(db, flashMessage) 
        { 
            _exceptionManager = exceptionManager;
        }

        public IActionResult Index()
        {
            var models = _db.BankBranches.IgnoreQueryFilters()
                                         .ToList();
            return View(models);
        }

        [PermissionAuthorize("BankBranch", PolicyGenerator.Write)]
        public IActionResult Create()
        {
            return View(new BankBranch());
        }

        [PermissionAuthorize("BankBranch", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BankBranch model)
        {
            if (ModelState.IsValid)
            {
                _db.BankBranches.Add(model);
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
            BankBranch model = Find(id);
            return View(model);
        }

        [PermissionAuthorize("BankBranch", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<BankBranch>(model))
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

        [PermissionAuthorize("BankBranch", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            BankBranch model = Find(id);
            try
            {
                _db.BankBranches.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete); 
            }
            
            return RedirectToAction(nameof(Index));
        }

        private BankBranch Find(long? id) 
        {
            var model = _db.BankBranches.IgnoreQueryFilters()
                                        .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
} 