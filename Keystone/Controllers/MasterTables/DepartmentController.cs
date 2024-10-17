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
    [PermissionAuthorize("Department", "")]
    public class DepartmentController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;

        public DepartmentController(ApplicationDbContext db,
                                    IFlashMessage flashMessage, 
                                    ISelectListProvider selectListProvider,
                                    IExceptionManager exceptionManager) : base(db, flashMessage, selectListProvider) 
        { 
            _exceptionManager = exceptionManager;
        }

        public IActionResult Index(string code)
        {
            var models = _db.Departments.Include(x => x.Faculty)
                                        .IgnoreQueryFilters()
                                        .OrderBy(x => x.Code)
                                        .ToList();
            return View(models);
        }

        [PermissionAuthorize("Department", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View();
        }

        [PermissionAuthorize("Department", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Department model)
        {
            if (ModelState.IsValid)
            {
                _db.Departments.Add(model);
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
            Department model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("Department", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid && await TryUpdateModelAsync<Department>(model))
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

        [PermissionAuthorize("Department", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Department model = Find(id);
            try
            {
                _db.Departments.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete); 
            }

            return RedirectToAction(nameof(Index));
        }

        private Department Find(long? id) 
        {
            var model = _db.Departments.IgnoreQueryFilters()
                                       .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList() 
        {
            ViewBag.Faculties = _selectListProvider.GetFaculties();
        }
    }
}