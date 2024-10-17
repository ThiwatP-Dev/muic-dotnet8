using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Curriculums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("Curriculum", "")]

    public class CurriculumController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;
        protected readonly ICurriculumProvider _curriculumProvider;
        
        public CurriculumController(ApplicationDbContext db,
                                    IFlashMessage flashMessage,
                                    ISelectListProvider selectListProvider,
                                    IExceptionManager exceptionManager,
                                    ICurriculumProvider curriculumProvider) : base(db, flashMessage, selectListProvider) 
        { 
            _exceptionManager = exceptionManager;
            _curriculumProvider = curriculumProvider;
        }
                                    
        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            var curriculums = _db.Curriculums.Include(x => x.Faculty)
                                             .Include(x => x.Department)
                                             .Include(x => x.AcademicLevel)
                                             .Include(x => x.CurriculumVersions)
                                                 .ThenInclude(x => x.CurriculumInstructors)
                                             .IgnoreQueryFilters()
                                             .Where(x => (string.IsNullOrEmpty(criteria.Status)
                                                          || x.IsActive == Convert.ToBoolean(criteria.Status))
                                                          && (string.IsNullOrEmpty(criteria.Code) 
                                                              || x.ReferenceCode.StartsWith(criteria.Code))
                                                          && (string.IsNullOrEmpty(criteria.FirstName)
                                                              || x.NameEn.StartsWith(criteria.FirstName)
                                                              || x.NameTh.StartsWith(criteria.FirstName))
                                                          && (criteria.AcademicLevelId == 0
                                                              || x.AcademicLevelId == criteria.AcademicLevelId)
                                                          && (criteria.FacultyId == 0
                                                              || x.FacultyId == criteria.FacultyId)
                                                          && (criteria.DepartmentId == 0
                                                              || x.DepartmentId == criteria.DepartmentId)
                                                          && (criteria.InstructorId == 0
                                                              || x.CurriculumVersions.Any(y => y.CurriculumInstructors.Any(z => z.InstructorId == criteria.InstructorId)))
                                                          && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                              || x.CurriculumVersions.Any(y => y.DegreeNameEn.StartsWith(criteria.CodeAndName))
                                                              || x.CurriculumVersions.Any(y => y.DegreeNameTh.StartsWith(criteria.CodeAndName))))
                                             .Select(x => x)
                                             .GetPaged(criteria, page);
            return View(curriculums);
        }

        public IActionResult Details(long id, string returnUrl)
        {
            Curriculum model = _curriculumProvider.GetCurriculum(id);
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        public ActionResult CreateCopyCurriculum(long academicLevelId, long masterCurriculumId, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            if (academicLevelId == 0 && masterCurriculumId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            CopyCurriculumViewModel model = new CopyCurriculumViewModel
                                            {
                                                AcademicLevelId = academicLevelId,
                                                MasterCurriculumId = masterCurriculumId,
                                                ReturnUrl = returnUrl
                                            };

            model.Curriculum = _curriculumProvider.GetCurriculum(masterCurriculumId);
            CreateSelectList(model.AcademicLevelId, model.Curriculum.FacultyId, masterCurriculumId);
            return View(model);
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCopyCurriculum(CopyCurriculumViewModel model, string returnUrl)
        {
            try
            {
                _db.Curriculums.Add(model.Curriculum);
                _db.SaveChanges();
                ViewBag.ReturnUrl = returnUrl;
                return RedirectToAction("CreateCopyVersion", "CurriculumVersion", new { academicLevelId = model.Curriculum.AcademicLevelId, masterCurriculumId = model.MasterCurriculumId, curriculumId = model.Curriculum.Id, returnUrl = returnUrl });
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

                CreateSelectList(model.AcademicLevelId, model.Curriculum.FacultyId);
                return View(model);
            }
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            CreateSelectList();
            CurriculumViewModel curriculum = new CurriculumViewModel();
            curriculum.Curriculum = new Curriculum();
            curriculum.Version = new CurriculumVersion()
                                 { 
                                     ApprovedDate = DateTime.Now 
                                 };
                                 
            curriculum.InstructorIds = new List<long>();
            ViewBag.ReturnUrl = returnUrl;
            return View(curriculum);
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CurriculumViewModel model)
        {
            if (ModelState.IsValid)
            {
                Curriculum curriculum = model.Curriculum;
                curriculum.CurriculumVersions = new List<CurriculumVersion>();
                curriculum.CurriculumVersions.Add(model.Version);
                
                model.Version.CurriculumInstructors = _curriculumProvider.SetCurriculumInstructor(model.Version.CurriculumInstructorIds, "c", model.Version.Id);
                model.Version.CurriculumInstructors = _curriculumProvider.SetCurriculumInstructor(model.Version.ThesisInstructorIds, "t", model.Version.Id);
                model.Version.CurriculumInstructors = _curriculumProvider.SetCurriculumInstructor(model.Version.InstructorIds, "i", model.Version.Id);
                _db.Curriculums.Add(model.Curriculum);
                
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

                    CreateSelectList();
                    return View(model);
                }
            }

            CreateSelectList();
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            CurriculumViewModel model = new CurriculumViewModel()
                                        {
                                            Curriculum = _curriculumProvider.GetCurriculum(id)
                                        };
            CreateSelectList(model.Curriculum.AcademicLevelId, model.Curriculum.FacultyId);

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var modelToUpdate = new CurriculumViewModel()
                                {
                                    Curriculum = _curriculumProvider.GetCurriculum(id ?? 0)
                                }; 
                                
            if (ModelState.IsValid && await TryUpdateModelAsync<CurriculumViewModel>(modelToUpdate, "", x => x.Curriculum))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch(Exception e)
                {
                    if (_exceptionManager.IsDuplicatedEntityCode(e))
                    {
                        _flashMessage.Danger(Message.CodeUniqueConstraintError);
                    }
                    else
                    {
                        _flashMessage.Danger(Message.UnableToEdit);
                    }
                    
                    CreateSelectList(modelToUpdate.Curriculum.AcademicLevelId, modelToUpdate.Curriculum.FacultyId);
                    return View(modelToUpdate);
                }
            }
            
            CreateSelectList(modelToUpdate.Curriculum.AcademicLevelId, modelToUpdate.Curriculum.FacultyId);
            _flashMessage.Danger(Message.UnableToEdit);
            return View(modelToUpdate);
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Curriculum model = Find(id);
            var curriculumVersions = _db.CurriculumVersions.Where(x => x.CurriculumId == id).ToList();
            var curriculumInstructors = _db.CurriculumInstructor.Where(x => x.CurriculumVersion.Curriculum.Id == id).ToList();
            var courseGroups = _db.CourseGroups.Where(x => x.CurriculumVersion.Curriculum.Id == id).ToList();
            var curriculumCourses = _db.CurriculumCourses.Where(x => x.CourseGroup.CurriculumVersion.Curriculum.Id == id).ToList();
            var studyPlans = _db.StudyPlans.Where(x => x.CurriculumVersion.Curriculum.Id == id).ToList();
            var studyCourses = _db.StudyCourses.Where(x => x.StudyPlan.CurriculumVersion.Curriculum.Id == id).ToList();
            if (model == null)
            {
                _flashMessage.Danger(Message.UnableToDelete);
                return RedirectToAction(nameof(Index));
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.CurriculumInstructor.RemoveRange(curriculumInstructors);
                    _db.CurriculumCourses.RemoveRange(curriculumCourses);
                    _db.CourseGroups.RemoveRange(courseGroups);
                    _db.StudyCourses.RemoveRange(studyCourses);
                    _db.StudyPlans.RemoveRange(studyPlans);
                    _db.CurriculumVersions.RemoveRange(curriculumVersions);
                    _db.Curriculums.Remove(model);
                    _db.SaveChanges();

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToDelete);
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        private Curriculum Find(long? id) 
        {
            var curriculum = _db.Curriculums.IgnoreQueryFilters()
                                            .SingleOrDefault(x => x.Id == id);
            return curriculum;
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        public ActionResult CreateCourse(long id)
        {
            CreateSelectList();
            return View("~/Views/Curriculum/CourseGroup/CreateCourse.cshtml");
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long curriculumId = 0) 
        {
            ViewBag.Statuses = _selectListProvider.GetActiveStatuses();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Faculties = _selectListProvider.GetFaculties();

            if (academicLevelId != 0)
            {
                ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                if (curriculumId != 0)
                {
                    ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
                }
                
                ViewBag.AcademicPrograms = _selectListProvider.GetAcademicProgramsByAcademicLevelId(academicLevelId);
                ViewBag.DegreeNames = _selectListProvider.GetDegreeNamesByAcademicLevelId(academicLevelId);
            }
            
            if (facultyId != 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartments(facultyId);
            }

            ViewBag.CurriculumTermTypes = _selectListProvider.GetCurriculumTermTypes();
            ViewBag.Instructors = _selectListProvider.GetInstructors();
            ViewBag.Grades = _selectListProvider.GetGrades();
        }
    }
}