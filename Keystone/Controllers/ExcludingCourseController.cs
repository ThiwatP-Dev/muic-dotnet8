using KeystoneLibrary.Data;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Keystone.Controllers.MasterTables
{
    public class ExcludingCourseController : BaseController
    {
        protected readonly IMasterProvider _masterProvider;
        protected readonly ICurriculumProvider _curriculumProvider;

        public ExcludingCourseController(ApplicationDbContext db,
                                         IFlashMessage flashMessage,
                                         IMasterProvider masterProvider,
                                         ICurriculumProvider curriculumProvider,
                                         ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider)
        {
            _masterProvider = masterProvider;
            _curriculumProvider = curriculumProvider;
        }

        public ActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.CurriculumId, criteria.CurriculumVersionId);
            if (criteria.AcademicLevelId == 0)
            {
                 _flashMessage.Warning(Message.RequiredData);
                 return View();
            }

            var today = DateTime.Today.Date;
            var courseExclusions = _db.CourseExclusions.Include(x => x.Course)
                                                       .Include(x => x.ExcludingCourse)
                                                       .Include(x => x.CurriculumVersion)
                                                           .ThenInclude(x => x.Curriculum)
                                                           .ThenInclude(x => x.AcademicLevel)
                                                       .Where(x => criteria.AcademicLevelId == x.Course.AcademicLevelId
                                                                   && (criteria.CurriculumId == 0 
                                                                       || criteria.CurriculumId == x.CurriculumVersion.CurriculumId)
                                                                   && (criteria.CurriculumVersionId == 0
                                                                       || criteria.CurriculumVersionId == x.CurriculumVersionId)
                                                                   && (criteria.CourseId == 0
                                                                       || criteria.CourseId == x.CourseId)
                                                                   && (string.IsNullOrEmpty(criteria.EffectivedStatus)
                                                                       || (Convert.ToBoolean(criteria.EffectivedStatus) 
                                                                            ? (x.EffectivedAt >= today && (x.EndedAt == null 
                                                                                                           || x.EndedAt.Value.Date <= today))
                                                                            : (x.EndedAt != null && x.EndedAt.Value.Date < today))))
                                                       .OrderBy(x => x.Course.Code)
                                                       .IgnoreQueryFilters()
                                                       .GetPaged(criteria ,page);
            return View(courseExclusions);
        }

        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            return View(new CourseExclusion());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CourseExclusion model, string returnUrl)
        {
            List<CourseExclusion> courses = new List<CourseExclusion>();
            if (ModelState.IsValid)
            {
                foreach(var item in model.ExcludingCourseIds)
                {
                    var course = new CourseExclusion
                                 {
                                     CourseId = model.CourseId,
                                     ExcludingCourseId = item,
                                     CurriculumVersionId = model.CurriculumVersionId,
                                     Remark = model.Remark,
                                     EffectivedAt = model.EffectivedAt,
                                     EndedAt = model.EndedAt,
                                 };

                    if (!_curriculumProvider.IsExistCourseExclusion(course))
                    {
                        courses.Add(course);
                    }
                }

                try
                {
                    _db.CourseExclusions.AddRange(courses);
                    _db.SaveChanges();   
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new 
                                                           {
                                                               AcademicLevelId = model.AcademicLevelId,
                                                               CourseId = model.CourseId
                                                           });
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToCreate);
                    CreateSelectList(model.AcademicLevelId, model.CurriculumId, model.CurriculumVersionId);
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            CreateSelectList(model.AcademicLevelId, model.CurriculumId, model.CurriculumVersionId);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            CourseExclusion model = _masterProvider.FindCourseExclusion(id);
            CreateSelectList(model.AcademicLevelId, model.CurriculumId, model.CurriculumVersionId);
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CourseExclusion model, string returnUrl)
        {
            if (_curriculumProvider.IsExistCourseExclusion(model))
            {
                ViewBag.ReturnUrl = returnUrl;
                CreateSelectList(model.AcademicLevelId, model.CurriculumId, model.CurriculumVersionId);
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }

            var modelToUpdate = _masterProvider.FindCourseExclusion(model.Id);
            if (ModelState.IsValid && await TryUpdateModelAsync<CourseExclusion>(modelToUpdate))
            {
                try
                {
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new 
                                                           {
                                                               AcademicLevelId = model.AcademicLevelId,
                                                               CourseId = model.CourseId
                                                           });
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToEdit);
                    CreateSelectList(model.AcademicLevelId, model.CurriculumId, model.CurriculumVersionId);
                    return View(model);
                }
            }
            
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            CreateSelectList(model.AcademicLevelId, model.CurriculumId, model.CurriculumVersionId);
            return View(model);
        }
    

        public ActionResult Delete(long id, string returnUrl)
        {
            var model = _masterProvider.FindCourseExclusion(id);
            try
            {
                _db.CourseExclusions.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private void CreateSelectList(long academicLevelId = 0, long curriculumId = 0, long curriculumVersionId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.EffectivedStatuses = _selectListProvider.GetEffectivedStatuses();
            if (academicLevelId != 0)
            {
                ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);
                ViewBag.ExcludingCourses = _selectListProvider.GetCoursesByAcademicLevelId(academicLevelId);
                if (curriculumId != 0)
                {
                    ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
                    if (curriculumVersionId != 0)
                    {
                        ViewBag.CurriculumCourse = _selectListProvider.GetCurriculumCourse(curriculumVersionId);
                    }
                }  
            }
        }
    }
}