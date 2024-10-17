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
    [PermissionAuthorize("Minor", "")]

    public class MinorController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;
        protected readonly IMasterProvider _masterProvider;
        protected readonly ICurriculumProvider _curriculumProvider;

        public MinorController(ApplicationDbContext db,
                               IFlashMessage flashMessage,
                               IExceptionManager exceptionManager,
                               IMasterProvider masterProvider,
                               ICurriculumProvider curriculumProvider) : base(db, flashMessage) 
        { 
            _exceptionManager = exceptionManager;
            _masterProvider = masterProvider;
            _curriculumProvider = curriculumProvider;
        }

        public IActionResult Index()
        {
            var models = _db.SpecializationGroups.IgnoreQueryFilters()
                                                 .Where(x => x.Type == SpecializationGroup.TYPE_MINOR_CODE)
                                                 .ToList();
            return View(models);
        }

        [PermissionAuthorize("Minor", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new SpecializationGroup());
        }

        [PermissionAuthorize("Minor", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SpecializationGroup model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.Type = SpecializationGroup.TYPE_MINOR_CODE;
                    _db.SpecializationGroups.Add(model);
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
            SpecializationGroup model = _masterProvider.FindSpecializationGroup(id);	
            return View(model);
        }

        [PermissionAuthorize("Minor", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = _masterProvider.FindSpecializationGroup(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<SpecializationGroup>(model))
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

        public IActionResult Details(long minorId, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.IsSpecialGroup = true;
            var model = _db.SpecializationGroups.IgnoreQueryFilters()
                                                .Where(x => x.Id == minorId
                                                            && x.Type == SpecializationGroup.TYPE_MINOR_CODE)
                                                .Select(x => new SpecializationGroupViewModel
                                                             {
                                                                 Id = x.Id,
                                                                 Code = x.Code,
                                                                 NameEn = x.NameEn,
                                                                 NameTh = x.NameTh,
                                                                 ShortNameEn = x.ShortNameEn,
                                                                 ShortNameTh = x.ShortNameTh,
                                                                 IsActive = x.IsActive
                                                             })
                                                .SingleOrDefault();

            model.CourseGroups = _curriculumProvider.GetCourseGroupBySpecialGroup(minorId);
            return View(model);
        }

        [PermissionAuthorize("Minor", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            SpecializationGroup model = _masterProvider.FindSpecializationGroup(id);
            try
            {
                _db.SpecializationGroups.Remove(model);
                _flashMessage.Confirmation(Message.SaveSucceed);
                _db.SaveChanges();
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}