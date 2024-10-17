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
    [PermissionAuthorize("StudentGroup", "")]
    public class StudentGroupController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;

        public StudentGroupController(ApplicationDbContext db,
                                      IFlashMessage flashMessage,
                                      ISelectListProvider selectListProvider,
                                      IExceptionManager exceptionManager) : base(db, flashMessage, selectListProvider) 
        {
            _exceptionManager = exceptionManager;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList();
            var models = _db.StudentGroups.Include(x => x.AcademicLevel)
                                          .Where(x => (string.IsNullOrEmpty(criteria.CodeAndName)
                                                       || x.Name.Contains(criteria.CodeAndName))
                                                       && (criteria.AcademicLevelId == 0
                                                           || x.AcademicLevelId == criteria.AcademicLevelId))
                                          .IgnoreQueryFilters()
                                          .GetPaged(page);
            return View(models);
        }

        [PermissionAuthorize("StudentGroup", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View();
        }

        [PermissionAuthorize("StudentGroup", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentGroup model)
        {
            if (ModelState.IsValid)
            { 
                _db.StudentGroups.Add(model);
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
            StudentGroup model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("StudentGroup", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            StudentGroup model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<StudentGroup>(model))
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

        [PermissionAuthorize("StudentGroup", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            StudentGroup model = Find(id);
            try
            {
                _db.StudentGroups.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private StudentGroup Find(long? id) 
        {
            var model = _db.StudentGroups.IgnoreQueryFilters()
                                         .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList()
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
        }
    }
}