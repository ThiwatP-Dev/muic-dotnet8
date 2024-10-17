using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTable
{
    [PermissionAuthorize("CurriculumCourseGroup", "")]
    public class CurriculumCourseGroupController : BaseController
    {
        public CurriculumCourseGroupController(ApplicationDbContext db,
                                               IFlashMessage flashMessage) : base(db, flashMessage) { }
        
        public IActionResult Index(Criteria criteria, int page)
        {
            var models = _db.CurriculumCourseGroups.Where(x => string.IsNullOrEmpty(criteria.CodeAndName)
                                                               || x.NameEn.StartsWith(criteria.CodeAndName)
                                                               || x.NameTh.StartsWith(criteria.CodeAndName))
                                                   .IgnoreQueryFilters()
                                                   .GetPaged(criteria, page);
            return View(models);
        }

        [PermissionAuthorize("CurriculumCourseGroup", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new CurriculumCourseGroup());
        }

        [PermissionAuthorize("CurriculumCourseGroup", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CurriculumCourseGroup model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                _db.CurriculumCourseGroups.Add(model);
                try
                {
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { CodeAndName = model.NameEn });
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }   
            }
            
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            CurriculumCourseGroup model = Find(id);
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [PermissionAuthorize("CurriculumCourseGroup", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<CurriculumCourseGroup>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { CodeAndName = model.NameEn });
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToEdit); 
                    return View(model);
                }
            }
            
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("CurriculumCourseGroup", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            CurriculumCourseGroup model = Find(id);
            try
            {
                _db.CurriculumCourseGroups.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete); 
            }
            
            return RedirectToAction(nameof(Index));
        }

        private CurriculumCourseGroup Find(long? id) 
        {
            var model = _db.CurriculumCourseGroups.IgnoreQueryFilters()
                                                  .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}