using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using Microsoft.EntityFrameworkCore;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.DataModels;
using Newtonsoft.Json;
using Keystone.Permission;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("CustomCourseGroup", "")]
    public class CustomCourseGroupController : BaseController
    {
        protected readonly IMasterProvider _masterProvider;
        protected readonly IRegistrationProvider _registrationProvider;

        public CustomCourseGroupController(ApplicationDbContext db,
                                           IFlashMessage flashMessage,
                                           ISelectListProvider selectListProvider,
                                           IMasterProvider masterProvider,
                                           IRegistrationProvider registrationProvider) : base(db, flashMessage, selectListProvider)
        {
            _masterProvider = masterProvider;
            _registrationProvider = registrationProvider;
        }

        public ActionResult Index(Criteria criteria, int page)
        {
            var groups = _db.CustomCourseGroups.IgnoreQueryFilters()
                                               .ToList();
            groups = groups.Select(x => {
                                            x.Courses = x.CourseIds == null ? new List<long>()
                                                                            : JsonConvert.DeserializeObject<List<long>>(x.CourseIds);
                                            x.CourseNames = x.CourseIds == null ? "" 
                                                                                : string.Join(", ", _registrationProvider.GetCourseByIds(x.Courses).Select(y => y.CourseAndCredit));
                                            return x;
                                        })
                           .ToList();

            var modelPageResults = groups.AsQueryable()
                                         .GetPaged(criteria ,page, true);
            return View(modelPageResults);
        }

        [PermissionAuthorize("CustomCourseGroup", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new CustomCourseGroup());
        }

        [PermissionAuthorize("CustomCourseGroup", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CustomCourseGroup model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.CourseIds = model.Courses != null ? JsonConvert.SerializeObject(model.Courses) : null;
                    _db.CustomCourseGroups.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToCreate);
                    CreateSelectList();
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            CreateSelectList();
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            var model = _masterProvider.GetCustomCourseGroup(id);
            model.Courses = string.IsNullOrEmpty(model.CourseIds) ? new List<long>()
                                                                  : JsonConvert.DeserializeObject<List<long>>(model.CourseIds);
            return View(model);
        }

        [PermissionAuthorize("CustomCourseGroup", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CustomCourseGroup model, string returnUrl)
        {
            var courseGroup = _masterProvider.GetCustomCourseGroup(model.Id);
            courseGroup.CourseIds = model.Courses != null ? JsonConvert.SerializeObject(model.Courses) : null;
            
            if (ModelState.IsValid && await TryUpdateModelAsync<CustomCourseGroup>(courseGroup))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToEdit);
                    CreateSelectList();
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            CreateSelectList();
            return View(model);
        }

        [PermissionAuthorize("CustomCourseGroup", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = _masterProvider.GetCustomCourseGroup(id);
            try
            {
                _db.CustomCourseGroups.Remove(model);
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
            ViewBag.Courses = _selectListProvider.GetCourses();
        }
    }
}