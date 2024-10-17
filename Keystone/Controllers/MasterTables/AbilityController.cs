using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.DataModels.Curriculums;
using Keystone.Permission;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("Ability", "")]
    public class AbilityController : BaseController
    {
        protected readonly ICurriculumProvider _curriculumProvider;
        protected readonly IExceptionManager _exceptionManager;

        public AbilityController(ApplicationDbContext db,
                                 IFlashMessage flashMessage,
                                 ISelectListProvider selectListProvider,
                                 ICurriculumProvider curriculumProvider) : base(db, flashMessage, selectListProvider)
        {
            _exceptionManager = new ExceptionManager();
            _curriculumProvider = curriculumProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            var specializationGroups = (from specializationGroup in _db.SpecializationGroups
                                        join courseGroup in _db.CourseGroups on specializationGroup.Id equals courseGroup.SpecializationGroupId
                                        where specializationGroup.Type == SpecializationGroup.TYPE_ABILITY_CODE
                                              && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                  || specializationGroup.Code.Contains(criteria.CodeAndName)
                                                  || specializationGroup.NameEn.Contains(criteria.CodeAndName)
                                                  || specializationGroup.NameTh.Contains(criteria.CodeAndName))
                                        select new SpecializationGroup
                                               {
                                                   Id = specializationGroup.Id,
                                                   Code = specializationGroup.Code,
                                                   NameEn = specializationGroup.NameEn,
                                                   NameTh = specializationGroup.NameTh,
                                                   ShortNameEn = specializationGroup.ShortNameEn,
                                                   ShortNameTh = specializationGroup.ShortNameTh,
                                                   IsForceTrack = courseGroup.IsForceTrack
                                               })
                                        .IgnoreQueryFilters()
                                        .GetPaged(criteria, page);
            return View(specializationGroups);
        }

        [PermissionAuthorize("Ability", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = new SpecializationGroup
                        {
                            IsForceTrack = true
                        };
            return View(model);
        }

        [PermissionAuthorize("Ability", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SpecializationGroup model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                try
                {
                    model.Type = SpecializationGroup.TYPE_ABILITY_CODE;
                    _db.SpecializationGroups.Add(model);
                    _db.SaveChanges();

                    // Course Group
                    CourseGroup courseGroup = new CourseGroup();
                    courseGroup.SpecializationGroupId = model.Id;
                    courseGroup.NameEn = model.NameEn;
                    courseGroup.NameTh = model.NameTh;
                    courseGroup.Type = "e";
                    courseGroup.Credit = 0;
                    courseGroup.IsForceTrack = model.IsForceTrack;
                    _db.CourseGroups.Add(courseGroup);
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

        public ActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            SpecializationGroup model = Find(id);
            var courseGroup = _db.CourseGroups.FirstOrDefault(x => x.SpecializationGroupId == id);
            model.IsForceTrack = courseGroup.IsForceTrack;
            return View(model);
        }

        [PermissionAuthorize("Ability", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SpecializationGroup model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                try
                {
                    var specializationGroup = _db.SpecializationGroups.Find(model.Id);
                    specializationGroup.Code = model.Code;
                    specializationGroup.NameEn = model.NameEn;
                    specializationGroup.NameTh = model.NameTh;
                    specializationGroup.ShortNameEn = model.ShortNameEn;
                    specializationGroup.ShortNameTh = model.ShortNameTh;

                    var courseGroup = _db.CourseGroups.FirstOrDefault(x => x.SpecializationGroupId == model.Id);
                    courseGroup.NameEn = model.NameEn;
                    courseGroup.NameTh = model.NameTh;
                    courseGroup.IsForceTrack = model.IsForceTrack;

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

        public ActionResult Details(long id, string returnUrl)
        {
            var specializationGroup = Find(id);
            var courseGroup = _db.CourseGroups.SingleOrDefault(x => x.SpecializationGroupId == id);

            AbilityViewModel abilityViewModel = new AbilityViewModel();
            abilityViewModel.Code = specializationGroup.Code;
            abilityViewModel.NameEn = specializationGroup.NameEn;
            abilityViewModel.NameTh = specializationGroup.NameTh;
            abilityViewModel.IsForceTrack = courseGroup.IsForceTrack;

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Courses = _selectListProvider.GetCourses();
            abilityViewModel.CurriculumCourses.AddRange(_curriculumProvider.GetAbilityCourses(courseGroup.Id));

            ViewBag.Departments = _selectListProvider.GetDepartments();
            abilityViewModel.SpecializationGroupBlackLists.AddRange(_curriculumProvider.GetAbilityBlacklistDepartments(id));
            return View(abilityViewModel);
        }

        public ActionResult GetCourse(long curriculumCourseId, int sequence, long specializationGroupId)
        {
            ViewBag.Courses = _selectListProvider.GetCourses();
            var curriculumCourse = _db.CurriculumCourses.Find(curriculumCourseId);
            if (curriculumCourse != null)
            {
                AbilityCourseViewModel abilityCourseViewModel = new AbilityCourseViewModel()
                                                                {
                                                                    Id = specializationGroupId,
                                                                    CurriculumCourseId = curriculumCourseId,
                                                                    CourseId = curriculumCourse.CourseId,
                                                                    Sequence = sequence,
                                                                    IsMustTake = curriculumCourse.IsMustTake
                                                                };

                return PartialView("~/Views/Ability/_Course.cshtml", abilityCourseViewModel);
            }
            else
            {
                return PartialView("~/Views/Ability/_Course.cshtml", new AbilityCourseViewModel());
            }
        }

        [PermissionAuthorize("Ability", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCourse(AbilityCourseViewModel model, string returnUrl)
        {
            try
            {
                var specializationGroup = Find(model.Id);
                var courseGroup = _db.CourseGroups.SingleOrDefault(x => x.SpecializationGroupId == model.Id);
                var course = _db.Courses.Find(model.CourseId);
                if (model.CourseId == 0)
                {
                    ModelState.AddModelError("CourseId", "Required");
                }

                if (ModelState.IsValid)
                {
                    if (_db.CurriculumCourses.Any(x => x.CourseGroupId == courseGroup.Id
                                                       && x.CourseId == model.CourseId))
                    {
                        _flashMessage.Danger(Message.UnableToCreate);
                    }
                    else
                    {
                        CurriculumCourse curriculumCourse = new CurriculumCourse()
                                                            {
                                                                CourseGroupId = courseGroup.Id,
                                                                CourseId = model.CourseId,
                                                                IsMustTake = model.IsMustTake,
                                                                GradeTemplateId = course.GradeTemplateId.Value,
                                                                Sequence = model.Sequence
                                                            };

                        _db.Entry(curriculumCourse).State = EntityState.Added;
                        _db.SaveChanges();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                    }
                }
                else
                {
                    _flashMessage.Danger(Message.UnableToCreate);
                }
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
            }
            return RedirectToAction("Details", "Ability", new { id = model.Id, returnUrl = returnUrl, tabIndex = "0" });
        }

        [PermissionAuthorize("Ability", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCourse(AbilityCourseViewModel model, string returnUrl)
        {
            try
            {
                var courseGroup = _db.CourseGroups.SingleOrDefault(x => x.SpecializationGroupId == model.Id);
                var course = _db.Courses.Find(model.CourseId);
                if (model.CourseId == 0)
                {
                    ModelState.AddModelError("CourseId", "Required");
                }

                if (ModelState.IsValid)
                {
                    if (_db.CurriculumCourses.Any(x => x.Id == model.CurriculumCourseId))
                    {
                        var curriculumCourse = _db.CurriculumCourses.Find(model.CurriculumCourseId);
                        curriculumCourse.CourseId = model.CourseId;
                        curriculumCourse.Sequence = model.Sequence;
                        curriculumCourse.IsMustTake = model.IsMustTake;

                        _db.Entry(curriculumCourse).State = EntityState.Modified;
                        _db.SaveChanges();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                    }
                    else
                    {
                        _flashMessage.Danger(Message.UnableToEdit);
                    }
                }
                else
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                }
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
            }

            return RedirectToAction("Details", "Ability", new { id = model.Id, returnUrl = returnUrl, tabIndex = "0" });
        }

        [PermissionAuthorize("Ability", PolicyGenerator.Write)]
        public ActionResult DeleteCourse(long id, string returnUrl)
        {
            var model = _db.CurriculumCourses.Find(id);
            var courseGroup = _db.CourseGroups.Find(model.CourseGroupId);
            try
            {
                _db.CurriculumCourses.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction("Details", "Ability", new { id = courseGroup.SpecializationGroupId, returnUrl = returnUrl, tabIndex = "0" });
        }

        public ActionResult GetBlacklistDepartment(long specializationGroupBlackListId)
        {
            ViewBag.Departments = _selectListProvider.GetDepartments();
            var specializationGroupBlackList = _db.SpecializationGroupBlackLists.Find(specializationGroupBlackListId);
            if (specializationGroupBlackList != null)
            {
                AbilityBlacklistDepartmentViewModel abilityBlacklistDepartmentViewModel = new AbilityBlacklistDepartmentViewModel()
                                                                                          {
                                                                                              Id = specializationGroupBlackList.SpecializationGroupId,
                                                                                              SpecializationGroupBlackListId = specializationGroupBlackList.Id,
                                                                                              DepartmentId = specializationGroupBlackList.DepartmentId,
                                                                                          };
                                                                                          
                return PartialView("~/Views/Ability/_BlacklistDepartment.cshtml", abilityBlacklistDepartmentViewModel);
            }
            else
            {
                return PartialView("~/Views/Ability/_BlacklistDepartment.cshtml", new AbilityBlacklistDepartmentViewModel());
            }
        }

        [PermissionAuthorize("Ability", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateBlacklistDepartment(AbilityBlacklistDepartmentViewModel model, string returnUrl)
        {
            try
            {
                if (model.DepartmentId == 0)
                {
                    ModelState.AddModelError("DepartmentId", "Required");
                }

                if (ModelState.IsValid)
                {
                    if (_db.SpecializationGroupBlackLists.Any(x => x.SpecializationGroupId == model.Id
                                                                   && x.DepartmentId == model.DepartmentId))
                    {
                        _flashMessage.Danger(Message.UnableToCreate);
                    }
                    else
                    {
                        SpecializationGroupBlackList specializationGroupBlack = new SpecializationGroupBlackList()
                        {
                            SpecializationGroupId = model.Id,
                            DepartmentId = model.DepartmentId
                        };
                        _db.SpecializationGroupBlackLists.Add(specializationGroupBlack);
                        _db.SaveChanges();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                    }
                }
                else
                {
                    _flashMessage.Danger(Message.UnableToCreate);
                }
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
            }
            return RedirectToAction("Details", "Ability", new { id = model.Id, returnUrl = returnUrl, tabIndex = "1" });
        }

        [PermissionAuthorize("Ability", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBlacklistDepartment(AbilityBlacklistDepartmentViewModel model, string returnUrl)
        {
            try
            {
                if (model.DepartmentId == 0)
                {
                    ModelState.AddModelError("DepartmentId", "Required");
                }

                if (ModelState.IsValid)
                {
                    if (_db.SpecializationGroupBlackLists.Any(x => x.Id == model.SpecializationGroupBlackListId))
                    {
                        var specializationGroupBlackList = _db.SpecializationGroupBlackLists.Find(model.SpecializationGroupBlackListId);
                        specializationGroupBlackList.DepartmentId = model.DepartmentId;
                        _db.Entry(specializationGroupBlackList).State = EntityState.Modified;
                        _db.SaveChanges();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                    }
                    else
                    {
                        _flashMessage.Danger(Message.UnableToEdit);
                    }
                }
                else
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                }
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
            }
            return RedirectToAction("Details", "Ability", new { id = model.Id, returnUrl = returnUrl, tabIndex = "1" });
        }

        [PermissionAuthorize("Ability", PolicyGenerator.Write)]
        public ActionResult DeleteBlacklistDepartment(long id, string returnUrl)
        {
            var model = _db.SpecializationGroupBlackLists.Find(id);
            try
            {
                _db.SpecializationGroupBlackLists.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            return RedirectToAction("Details", "Ability", new { id = model.SpecializationGroupId, returnUrl = returnUrl, tabIndex = "1" });
        }

        [PermissionAuthorize("Ability", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            SpecializationGroup model = Find(id);
            try
            {
                var courseGroup = _db.CourseGroups.SingleOrDefault(x => x.SpecializationGroupId == model.Id);
                _db.CourseGroups.Remove(courseGroup);

                _db.SpecializationGroups.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private SpecializationGroup Find(long? id)
        {
            var model = _db.SpecializationGroups.IgnoreQueryFilters()
                                                .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}