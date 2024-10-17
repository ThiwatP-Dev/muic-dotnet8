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
    [PermissionAuthorize("ExtendedYear", "")]
    public class ExtendedYearController : BaseController
    {
        protected readonly IMasterProvider _masterProvider;

        public ExtendedYearController(ApplicationDbContext db,
                                      IFlashMessage flashMessage, 
                                      ISelectListProvider selectListProvider,
                                      IMasterProvider masterProvider) : base(db, flashMessage, selectListProvider)
        {
            _masterProvider = masterProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList();
            var model = _db.ExtendedYears.Include(x => x.AcademicLevel)
                                         .Where(x => criteria.AcademicLevelId == 0
                                                     || x.AcademicLevelId == criteria.AcademicLevelId)
                                         .IgnoreQueryFilters()
                                         .GetPaged(criteria, page);

            return View(model);
        }

        [PermissionAuthorize("ExtendedYear", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View(new ExtendedYear());
        }

        [PermissionAuthorize("ExtendedYear", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ExtendedYear model)
        {
            if (ModelState.IsValid)
            {
                if (_masterProvider.IsExistExtendedYear(model))
                {
                    CreateSelectList();
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }

                try
                {
                    _db.ExtendedYears.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { AcademicLevelId = model.AcademicLevelId });
                }
                catch
                {
                    CreateSelectList();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            CreateSelectList();
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            var model = _masterProvider.FindExtendedYear(id);
            CreateSelectList();
            return View(model);
        }

        [PermissionAuthorize("ExtendedYear", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = _masterProvider.FindExtendedYear(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<ExtendedYear>(model))
            {
                if (_masterProvider.IsExistExtendedYear(model))
                {
                    CreateSelectList();
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }
                
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { AcademicLevelId = model.AcademicLevelId });
                }
                catch
                { 
                    CreateSelectList();
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(model);
                }
            }

            CreateSelectList();
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("ExtendedYear", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = _masterProvider.FindExtendedYear(id);
            try
            {
                _db.ExtendedYears.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private void CreateSelectList()
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
        }
    }
}