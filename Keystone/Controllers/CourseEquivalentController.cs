using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("CourseEquivalent", "")]
    public class CourseEquivalentController : BaseController
    {
        public CourseEquivalentController(ApplicationDbContext db,
                                          IFlashMessage flashMessage,
                                          ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }
        
        public IActionResult Index(int page, Criteria criteria)
        {
            var today = DateTime.Now;
            CreateSelectList();
            var model = _db.CourseEquivalents.Include(x => x.Course)
                                             .Include(x => x.EquilaventCourse)
                                             .Where(x => (criteria.CourseId == 0
                                                          || x.CourseId == criteria.CourseId
                                                          || x.EquilaventCourseId == criteria.CourseId)
                                                         && (string.IsNullOrEmpty(criteria.EffectivedStatus)
                                                             || (Convert.ToBoolean(criteria.EffectivedStatus) 
                                                                 ? (x.EffectivedAt >= today && (x.EndedAt == null 
                                                                                                || x.EndedAt.Value.Date <= today))
                                                                 : (x.EndedAt != null && x.EndedAt.Value.Date < today)))
                                                         && (string.IsNullOrEmpty(criteria.Status)
                                                             || x.IsActive == Convert.ToBoolean(criteria.Status)))
                                             .IgnoreQueryFilters()
                                             .OrderBy(x => x.Course.Code)
                                                .ThenBy(x => x.EquilaventCourse.Code)
                                                .ThenBy(x => x.EffectivedAt)
                                             .GetPaged(criteria, page);

            return View(model);
        }

        [PermissionAuthorize("CourseEquivalent", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View(new CourseEquivalent());
        }

        [PermissionAuthorize("CourseEquivalent", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CourseEquivalent model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.CourseEquivalents.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { CourseId = model.CourseId });
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
            CourseEquivalent model = Find(id);
            CreateSelectList();
            return View(model);
        }

        [PermissionAuthorize("CourseEquivalent", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<CourseEquivalent>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
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

        [PermissionAuthorize("CourseEquivalent", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = Find(id);
            try
            {
                _db.CourseEquivalents.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private CourseEquivalent Find(long? id) 
        {
            return _db.CourseEquivalents.IgnoreQueryFilters()
                                        .SingleOrDefault(x => x.Id == id);
        }

        private void CreateSelectList()
        {
            ViewBag.Courses = _selectListProvider.GetCourseAndTransferCourse();
            ViewBag.Statuses = _selectListProvider.GetActiveStatuses();
            ViewBag.EffectivedStatuses = _selectListProvider.GetEffectivedStatuses();
        }
    }
}