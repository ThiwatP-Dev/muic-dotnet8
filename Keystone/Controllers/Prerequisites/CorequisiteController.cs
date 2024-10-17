using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.DataModels.Prerequisites;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("Corequisite", "")]
    public class CorequisiteController : BaseController
    {
        protected readonly IPrerequisiteProvider _prerequisiteProvider;

        public CorequisiteController(ApplicationDbContext db,
                                     IFlashMessage flashMessage,
                                     ISelectListProvider selectListProvider,
                                     IPrerequisiteProvider prerequisiteProvider) : base(db, flashMessage, selectListProvider)
        {
            _prerequisiteProvider = prerequisiteProvider;
        }
        
        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList();
            var models = _db.Corequisites.Include(x => x.FirstCourse)
                                         .Include(x => x.SecondCourse)
                                         .Where(x => (criteria.CourseId == 0
                                                      || x.FirstCourseId == criteria.CourseId
                                                      || x.SecondCourseId == criteria.CourseId)
                                                     && (string.IsNullOrEmpty(criteria.Status) 
                                                         || x.IsActive == Convert.ToBoolean(criteria.Status)))
                                         .IgnoreQueryFilters()
                                         .GetPaged(criteria, page);
            return View(models);
        }

        [PermissionAuthorize("Corequisite", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new Corequisite());
        }

        [PermissionAuthorize("Corequisite", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Corequisite model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (_prerequisiteProvider.IsExistCourseCorequisite(model))
                {
                    CreateSelectList();
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }
                else
                {
                    try
                    {
                        model.Description = _prerequisiteProvider.GetCorequisiteDescription(model);
                        _db.Corequisites.Add(model);
                        _db.SaveChanges();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index), new { CourseId = model.FirstCourseId });
                    }
                    catch
                    {
                        CreateSelectList();
                        ViewBag.ReturnUrl = returnUrl;
                        _flashMessage.Danger(Message.UnableToCreate);
                        return View(model);
                    }   
                }
            }
            
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            var model = _prerequisiteProvider.GetCorequisite(id);
            return View(model);
        }

        [PermissionAuthorize("Corequisite", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = _prerequisiteProvider.GetCorequisite(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<Corequisite>(model))
            {
                if (_prerequisiteProvider.IsExistCourseCorequisite(model))
                {
                    CreateSelectList();
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }
                else
                {
                    try
                    {
                        model.Description = _prerequisiteProvider.GetCorequisiteDescription(model);
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index), new { CourseId = model.FirstCourseId });
                    }
                    catch
                    {
                        CreateSelectList();
                        ViewBag.ReturnUrl = returnUrl;
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
            }
            
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("Corequisite", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = _prerequisiteProvider.GetCorequisite(id);
            try
            {
                _db.Corequisites.Remove(model);
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
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.ActiveStatuses = _selectListProvider.GetActiveStatuses();
            ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersions();
        }    
    }
}