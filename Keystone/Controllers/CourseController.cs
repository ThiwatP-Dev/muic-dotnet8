using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using AutoMapper;
using Newtonsoft.Json;
using KeystoneLibrary.Models.DataModels.Fee;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("Course", "")]
    public class CourseController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IFeeProvider _feeProvider;
        protected readonly IMasterProvider _masterProvider;

        public CourseController(ApplicationDbContext db,
                                IFlashMessage flashMessage,
                                IMapper mapper,
                                ISelectListProvider selectListProvider,
                                IExceptionManager exceptionManager,
                                IAcademicProvider academicProvider,
                                IFeeProvider feeProvider,
                                IMasterProvider masterProvider) : base(db, flashMessage, selectListProvider)
        {
            _exceptionManager = exceptionManager;
            _academicProvider = academicProvider;
            _feeProvider = feeProvider;
            _masterProvider = masterProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0 && criteria.FacultyId == 0 && criteria.DepartmentId == 0
                && criteria.CourseRateId == 0 && criteria.TeachingTypeId == 0 && string.IsNullOrEmpty(criteria.Code)
                && string.IsNullOrEmpty(criteria.CodeAndName))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var courses = _db.Courses.Include(x => x.Faculty)
                                     .Include(x => x.Department)
                                     .Where(x => (criteria.AcademicLevelId == 0
                                                      || criteria.AcademicLevelId == x.AcademicLevelId)
                                                  && (criteria.FacultyId == 0
                                                      || criteria.FacultyId == x.FacultyId)
                                                  && (criteria.DepartmentId == 0
                                                      || criteria.DepartmentId == x.DepartmentId)
                                                  && (criteria.CourseRateId == 0
                                                      || criteria.CourseRateId == x.CourseRateId)
                                                  && (criteria.TeachingTypeId == 0
                                                      || criteria.TeachingTypeId == x.TeachingTypeId)
                                                  && (String.IsNullOrEmpty(criteria.Code)
                                                      || x.Code.StartsWith(criteria.Code))
                                                  && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                      || x.NameEn.StartsWith(criteria.CodeAndName)
                                                      || x.NameTh.StartsWith(criteria.CodeAndName))
                                                  && (String.IsNullOrEmpty(criteria.IsSetTuitionFee)
                                                      || _db.TuitionFees.Any(y => y.CourseId == x.Id) == Convert.ToBoolean(criteria.IsSetTuitionFee))
                                                  && x.TransferUniversityId == null)
                                     .IgnoreQueryFilters()
                                     .OrderBy(x => x.Code)
                                     .GetPaged(criteria, page);

            return View(courses);
        }

        public IActionResult Details(long id, string returnUrl)
        {
            var course = _academicProvider.GetCourseDetail(id);
            ViewBag.ReturnUrl = returnUrl;
            return View(course);
        }

        [PermissionAuthorize("Course", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            return View(new Course());
        }

        [PermissionAuthorize("Course", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Course model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.Courses.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new
                    {
                        Code = model.Code,
                        AcademicLevelId = model.AcademicLevelId,
                        FacultyId = model.FacultyId ?? 0,
                        CourseRateId = model.CourseRateId ?? 0,
                        TeachingTypeId = model.TeachingTypeId ?? 0
                    });
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToCreate);
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            Course model = Find(id);
            model.ExcludingNativeLanguageIds = string.IsNullOrEmpty(model.ExcludingNativeLanguageId) ? new List<long>() : JsonConvert.DeserializeObject<List<long>>(model.ExcludingNativeLanguageId ?? "");
            CreateSelectList(model.AcademicLevelId, model.FacultyId ?? 0);
            return View(model);
        }

        [PermissionAuthorize("Course", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<Course>(model))
            {
                try
                {
                    model.ExcludingNativeLanguageId = JsonConvert.SerializeObject(model.ExcludingNativeLanguageIds) ?? "[]";
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new 
                    {
                        Code = model.Code,
                        AcademicLevelId = model.AcademicLevelId,
                        FacultyId = model.FacultyId ?? 0,
                        CourseRateId = model.CourseRateId ?? 0,
                        TeachingTypeId = model.TeachingTypeId ?? 0
                    });
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("Course", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Course model = Find(id);
            if (model == null)
            {
                _flashMessage.Danger(Message.UnableToDelete);
                return RedirectToAction(nameof(Index));
            }

            _db.Courses.Remove(model);
            _db.SaveChanges();
            _flashMessage.Confirmation(Message.SaveSucceed);
            return RedirectToAction(nameof(Index));
        }

        private Course Find(long? id)
        {
            var courses = _db.Courses.Include(x => x.Faculty)
                                     .Include(x => x.Department)
                                     .Include(x => x.CourseRate)
                                     .IgnoreQueryFilters()
                                     .SingleOrDefault(x => x.Id == id);
            return courses;
        }

        public ActionResult TuitionFeeDetails(long academicLevelId, long courseId, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateTuitionFeeSelectList(academicLevelId);
            var courseTuitionFee = _feeProvider.GetCourseTuitionFee(academicLevelId, courseId);
            return View(courseTuitionFee);
        }

        public ActionResult GetTuitionFee(long academicLevelId, long courseId, long tuitionFeeId)
        {
            var tuitionFee = _feeProvider.GetTuitionFee(academicLevelId, courseId, tuitionFeeId);
            if (tuitionFee != null)
            {
                CreateTuitionFeeSelectList(academicLevelId, tuitionFee.FacultyId ?? 0, tuitionFee.DepartmentId ?? 0, tuitionFee.CurriculumId ?? 0);
                return PartialView("~/Views/Course/_TuitionFee.cshtml", tuitionFee);
            }
            else
            {
                CreateTuitionFeeSelectList(academicLevelId);
                return PartialView("~/Views/Course/_TuitionFee.cshtml", new TuitionFee());
            }
        }

        [PermissionAuthorize("Course", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTuitionFee(TuitionFee model, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.IsActive = model.Active;
                    _db.Entry(model).State = EntityState.Added;
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                else
                {
                    _flashMessage.Danger(Message.UnableToCreate);
                }
                return RedirectToAction("TuitionFeeDetails", "Course", new { academicLevelId = model.AcademicLevelId, courseId = model.CourseId, returnUrl = returnUrl });
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

        [PermissionAuthorize("Course", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTuitionFee(TuitionFee model, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.IsActive = model.Active;
                    _db.Entry(model).State = EntityState.Modified;
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                else
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                }
                return RedirectToAction("TuitionFeeDetails", "Course", new { academicLevelId = model.AcademicLevelId, courseId = model.CourseId, returnUrl = returnUrl });
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

        [PermissionAuthorize("Course", PolicyGenerator.Write)]
        public ActionResult DeleteTuitionFee(long id, string returnUrl)
        {
            var model = _db.TuitionFees.Find(id);
            try
            {
                _db.TuitionFees.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction("TuitionFeeDetails", "Course", new { courseId = model.CourseId, returnUrl = returnUrl });
        }

        private void CreateTuitionFeeSelectList(long academicLevelId = 0,
                                                long facultyId = 0,
                                                long departmentId = 0,
                                                long curriculumId = 0)
        {
            ViewBag.FeeItems = _selectListProvider.GetFeeItems();
            ViewBag.Batches = _selectListProvider.GetBatches();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            }

            if (departmentId > 0)
            {
                ViewBag.Curriculums = _selectListProvider.GetCurriculumByDepartment(academicLevelId, facultyId, departmentId);
            }

            if (curriculumId > 0)
            {
                ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
            }

            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();
            ViewBag.TuitionFeeFormulas = _selectListProvider.GetTuitionFeeFormulas();
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.TeachingTypes = _selectListProvider.GetTeachingTypes();
            ViewBag.GradeTemplates = _selectListProvider.GetGradeTemplates();
            ViewBag.Languages = _selectListProvider.GetNativeLanguages();
            ViewBag.AllYesNoAnswer = _selectListProvider.GetAllYesNoAnswer();
            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            }
        }
    }
}