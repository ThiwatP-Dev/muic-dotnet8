using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using KeystoneLibrary.Models.DataModels.Prerequisites;

namespace Keystone.Controllers.MasterTables
{
    public class PredefinedCourseController : BaseController
    {
        private readonly IPrerequisiteProvider _prerequisiteProvider;

        public PredefinedCourseController(ApplicationDbContext db,
                                          IFlashMessage flashMessage,
                                          ISelectListProvider selectListProvider,
                                          IPrerequisiteProvider prerequisiteProvider) : base(db, flashMessage, selectListProvider)
        {
            _prerequisiteProvider = prerequisiteProvider;
        }

        public ActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.CurriculumId);
            var course = _db.PredefinedCourses.Include(x => x.CurriculumVersion)
                                                  .ThenInclude(x => x.Curriculum)
                                              .Include(x => x.FirstCourse)
                                              .Include(x => x.SecondCourse)
                                              .Include(x => x.Grade)
                                              .Where(x => (criteria.AcademicLevelId == 0
                                                           || x.CurriculumVersion.Curriculum.AcademicLevelId == criteria.AcademicLevelId)
                                                           && (criteria.CurriculumId == 0
                                                           || x.CurriculumVersion.CurriculumId == criteria.CurriculumId)
                                                           && (criteria.CurriculumVersionId == 0
                                                           || x.CurriculumVersionId == criteria.CurriculumVersionId)
                                                           && (criteria.CourseId == 0
                                                               || x.FirstCourseId == criteria.CourseId
                                                               || x.SecondCourseId == criteria.CourseId)
                                                           && (string.IsNullOrEmpty(criteria.Type)
                                                               || x.RequiredType == criteria.Type))
                                              .IgnoreQueryFilters()
                                              .GetPaged(criteria ,page);
            return View(course);
        }

        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            return View(new PredefinedCourse());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PredefinedCourse model, string returnUrl)
        {
            if (model.FirstCourseId == model.SecondCourseId)
            {
                _flashMessage.Danger(Message.DuplicateCourse);
                CreateSelectList();
                return View(model);
            }

            if (_prerequisiteProvider.IsExistedPredefinedCourse(model))
            {
                CreateSelectList();
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.PredefinedCourses.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new Criteria
                                                           {
                                                               AcademicLevelId = model.AcademicLevelId,
                                                               CurriculumId = model.CurriculumId,
                                                               CurriculumVersionId = model.CurriculumVersionId
                                                           });
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
            ViewBag.ReturnUrl = returnUrl;
            var model = _prerequisiteProvider.GetPredefinedCourse(id);
            CreateSelectList(model.AcademicLevelId, model.CurriculumId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(PredefinedCourse course, string returnUrl)
        {
            var model = _prerequisiteProvider.GetPredefinedCourse(course.Id);
            if (course.FirstCourseId == course.SecondCourseId)
            {
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.DuplicateCourse);
                CreateSelectList(model.AcademicLevelId, model.CurriculumId);
                return View(model);
            }

            if (_prerequisiteProvider.IsExistedPredefinedCourse(course))
            {
                CreateSelectList();
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.DataAlreadyExist);
                CreateSelectList(model.AcademicLevelId, model.CurriculumId);
                return View(model);
            }

            if (ModelState.IsValid && await TryUpdateModelAsync<PredefinedCourse>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new Criteria
                                                           {
                                                               AcademicLevelId = model.AcademicLevelId,
                                                               CurriculumId = model.CurriculumId,
                                                               CurriculumVersionId = model.CurriculumVersionId
                                                           });
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToEdit);
                    CreateSelectList(model.AcademicLevelId, model.CurriculumId);
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            CreateSelectList(model.AcademicLevelId, model.CurriculumId);
            return View(model);
        }

        public ActionResult Delete(long id)
        {
            var model = _prerequisiteProvider.GetPredefinedCourse(id);
            try
            {
                _db.PredefinedCourses.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private void CreateSelectList(long academicLevelId = 0, long curriculumId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.RequiredTypes = _selectListProvider.GetRequiredTypes();
            ViewBag.Grades = _selectListProvider.GetGrades();
            if (academicLevelId > 0)
            {
                ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);
                ViewBag.Courses = _selectListProvider.GetCoursesByAcademicLevelId(academicLevelId);
            }

            if (curriculumId > 0)
            {
                ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
            }
        }
    }
}