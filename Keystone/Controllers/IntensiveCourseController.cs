using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Admission;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("IntensiveCourse", "")]
    public class IntensiveCourseController : BaseController
    {
        protected readonly IAdmissionProvider _admissionProvider;

        public IntensiveCourseController(ApplicationDbContext db,
                                         ISelectListProvider selectLsitProvider,
                                         IFlashMessage flashMessage,
                                         IAdmissionProvider admissionProvider) : base(db, flashMessage, selectLsitProvider)
        {
            _admissionProvider = admissionProvider;
        }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            var models = _db.IntensiveCourses.Include(x => x.AcademicLevel)
                                             .Include(x => x.Faculty)
                                             .Include(x => x.Department)
                                             .Where(x => (criteria.AcademicLevelId == 0
                                                          || x.AcademicLevelId == criteria.AcademicLevelId)
                                                          && (criteria.FacultyId == 0
                                                              || x.FacultyId == criteria.FacultyId)
                                                          && (criteria.DepartmentId == 0
                                                              || x.DepartmentId == criteria.DepartmentId))
                                             .GetPaged(criteria, page);

            return View(models);
        }

        [PermissionAuthorize("IntensiveCourse", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new IntensiveCourse());
        }

        [PermissionAuthorize("IntensiveCourse", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IntensiveCourse model, string returnUrl)
        {
            if (_admissionProvider.IsExistIntensiveCourse(model.CourseId, model.FacultyId ?? 0, model.DepartmentId ?? 0))
            {
                ViewBag.ReturnUrl = returnUrl;
                CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0);
                _flashMessage.Danger(Message.ExistedCourse);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.IntensiveCourses.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new Criteria
                                                           {
                                                               AcademicLevelId = model.AcademicLevelId ?? 0,
                                                               FacultyId = model.FacultyId ?? 0,
                                                               DepartmentId = model.DepartmentId ?? 0
                                                           });
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0);
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0);
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            IntensiveCourse model = Find(id);
            CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0);
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [PermissionAuthorize("IntensiveCourse", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(IntensiveCourse intensiveCourse, string returnUrl)
        {
            var model = Find(intensiveCourse.Id);
            if (_admissionProvider.IsExistIntensiveCourse(model.CourseId, model.FacultyId ?? 0, model.DepartmentId ?? 0))
            {
                ViewBag.ReturnUrl = returnUrl;
                CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0);
                _flashMessage.Danger(Message.ExistedCourse);
                return View(model);
            }

            if (ModelState.IsValid && await TryUpdateModelAsync<IntensiveCourse>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new Criteria
                                                           {
                                                               AcademicLevelId = model.AcademicLevelId ?? 0,
                                                               FacultyId = model.FacultyId ?? 0,
                                                               DepartmentId = model.DepartmentId ?? 0
                                                           });
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0);
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(model);
                }
            }
            
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0);
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("IntensiveCourse", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            IntensiveCourse model = Find(id);
            try
            {
                _db.IntensiveCourses.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private IntensiveCourse Find(long? id) 
        {
            var model = _db.IntensiveCourses.IgnoreQueryFilters()
                                            .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();

            if (academicLevelId > 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }
            
            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartments(facultyId);
            }
        }
    }
}