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
    [PermissionAuthorize("TeachingType", "")]
    public class TeachingTypeController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;

        public TeachingTypeController(ApplicationDbContext db,
                                      IFlashMessage flashMessage,
                                      ISelectListProvider selectListProvider,
                                      IExceptionManager exceptionManager) : base(db, flashMessage, selectListProvider) 
        { 
            _exceptionManager = exceptionManager;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList();
            var models = _db.TeachingTypes.Where(x => (string.IsNullOrEmpty(criteria.CodeAndName)
                                                       || x.NameEn.Contains(criteria.CodeAndName))
                                                       && (string.IsNullOrEmpty(criteria.CalculateType)
                                                           || x.CalculateType == criteria.CalculateType)
                                                       && !x.IsExamination)
                                          .IgnoreQueryFilters()
                                          .GetPaged(criteria, page);
            return View(models);
        }

        [PermissionAuthorize("TeachingType", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new TeachingType());
        }

        [PermissionAuthorize("TeachingType", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TeachingType model)
        {
            if (ModelState.IsValid)
            {
                _db.TeachingTypes.Add(model);
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
            TeachingType model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("TeachingType", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            TeachingType model = Find(id);

            if (ModelState.IsValid && await TryUpdateModelAsync<TeachingType>(model))
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

        [PermissionAuthorize("TeachingType", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            TeachingType model = Find(id);
            try
            {
                _db.TeachingTypes.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private TeachingType Find(long? id) 
        {
            var model = _db.TeachingTypes.IgnoreQueryFilters()
                                         .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList()
        {
            ViewBag.CalculateTypes = _selectListProvider.GetCalculateTypes();
        }
    }
}