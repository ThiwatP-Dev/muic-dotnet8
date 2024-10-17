using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("Curriculum", "")]
    public class CourseGroupController : BaseController
    {
        protected readonly ICurriculumProvider _curriculumProvider;
        protected readonly IMasterProvider _masterProvider;

        public CourseGroupController(ApplicationDbContext db,
                                     IFlashMessage flashMessage,
                                     ISelectListProvider selectListProvider,
                                     ICurriculumProvider curriculumProvider,
                                     IMasterProvider masterProvider) : base(db, flashMessage, selectListProvider)
        {
            _curriculumProvider = curriculumProvider;
            _masterProvider = masterProvider;
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        public ActionResult CreateMainGroup(long? versionId, long? minorId, long? concentrationId, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            var specializationGroup = _masterProvider.FindSpecializationGroup(minorId ?? concentrationId ?? 0);
            return View("~/Views/Curriculum/CourseGroup/CreateGroup.cshtml", new CourseGroup()
                                                                             {
                                                                                 CurriculumVersionId = versionId,
                                                                                 SpecializationGroupId = minorId ?? concentrationId,
                                                                                 SpecializationGroupType = specializationGroup?.Type
                                                                             });
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        public ActionResult CreateGroup(long groupId, string returnUrl, bool isSpecialGroup = false, long? specialGroupId = null)
        {
            var version = new CurriculumVersion();
            var courseGroup = new CourseGroup();
            long defaultGrade = 0;
            long? defaultSpecializationGroup = null;
            string defaultType = null;
            string type = null;

            if (groupId != 0)
            {
                version =  _curriculumProvider.GetVersionIdByCourseGroup(groupId);
                courseGroup = _curriculumProvider.FindCourseGroup(groupId);
            }
            
            if (courseGroup != null)
            {
                defaultGrade = courseGroup.RequiredGradeId ?? 0;
                defaultType = courseGroup.Type;
                defaultSpecializationGroup = version?.Id == null ? courseGroup.SpecializationGroupId : null;
            }
            
            if (defaultSpecializationGroup != null)
            {
                type = _masterProvider.FindSpecializationGroup(defaultSpecializationGroup ?? 0)?.Type;
            }

            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.IsSpecialGroup = isSpecialGroup;
            return View("~/Views/Curriculum/CourseGroup/CreateGroup.cshtml", new CourseGroup
                                                                             {
                                                                                 CurriculumVersionId = version?.Id,
                                                                                 CourseGroupId = groupId,
                                                                                 RequiredGradeId = defaultGrade,
                                                                                 SpecializationGroupId = specialGroupId,
                                                                                 SpecializationGroupType = type,
                                                                                 Type = defaultType
                                                                             });
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateGroup(CourseGroup model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if(string.IsNullOrEmpty(model.NameEn) || string.IsNullOrEmpty(model.NameTh))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View("~/Views/Curriculum/CourseGroup/EditGroup.cshtml",new { model, returnUrl = returnUrl });
            }

            var curriculum = _curriculumProvider.GetCurriculumByVersionId(model.CurriculumVersionId ?? 0);
            if (ModelState.IsValid)
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        if (model.CurriculumVersionId != null || model.CourseGroupId != null)
                        {
                            model.CurriculumVersionId = model.CurriculumVersionId == 0 ? null : model.CurriculumVersionId;
                            model.CourseGroupId = model.CourseGroupId == 0 ? null : model.CourseGroupId;
                            if (model.SpecializationGroupId != null)
                            {
                                _db.CourseGroups.Add(model);
                                _db.SaveChanges();
                                if (model.CurriculumVersionId != null)
                                {
                                    _curriculumProvider.CopyCourseGroupFromSpecilizationGroup(model);
                                }
                            }
                            else
                            {
                                model.SpecializationGroupId = model.MinorId ?? model.ConcentrationId;
                                _db.CourseGroups.Add(model);
                            }
                        }
                        else
                        {
                            _db.CourseGroups.Add(model);
                        }
                        
                        _db.SaveChanges();
                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return Redirect(returnUrl);
                    }
                    catch
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.UnableToCreate);
                        CreateSelectList();
                        return View("~/Views/Curriculum/CourseGroup/CreateGroup.cshtml", model); 
                    }
                }
            }
            
            _flashMessage.Danger(Message.UnableToCreate);
            CreateSelectList();
            return View("~/Views/Curriculum/CourseGroup/CreateGroup.cshtml", model);
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        public ActionResult CreateCourse(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateCurriculumCourseSelectList();
            var version =  _curriculumProvider.GetVersionIdByCourseGroup(id);
            var courseGroup = _curriculumProvider.FindCourseGroup(id);
            var type = _masterProvider.FindSpecializationGroup(courseGroup.SpecializationGroupId ?? 0)?.Type;
            var defaultType = courseGroup.Type;
            var defaultGrade = courseGroup.RequiredGradeId ?? 0;
            var course = new CurriculumCourse()
                         {
                             CourseGroupId = id,
                             CurriculumVersionId = version?.Id,
                             RequiredGradeId = defaultGrade,
                             IsRequired = defaultType == "r",
                             SpecializationGroupId = courseGroup.SpecializationGroupId,
                             SpecializationGroupType = type
                         };

            var model = new CourseGroup()
                        {
                            Id = id,
                            RequiredGradeId = defaultGrade,
                            Type = defaultType,
                            CurriculumCourses = new List<CurriculumCourse>() { course }
                        };
                                            
            return View("~/Views/Curriculum/CourseGroup/CreateCourse.cshtml", model);
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCourse(List<CurriculumCourse> models, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                models.Select(x => { 
                                      x.IsRequired = x.IsRequiredText == "1";
                                      return x;
                                   }).ToList();

                try
                {
                    _db.CurriculumCourses.AddRange(models);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return Redirect(returnUrl);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToCreate);
                    CreateCurriculumCourseSelectList();
                    return View("~/Views/Curriculum/CourseGroup/CreateCourse.cshtml", new CourseGroup { CurriculumCourses = models });
                }
            }

            _flashMessage.Danger(Message.UnableToCreate);
            CreateCurriculumCourseSelectList();
            return View("~/Views/Curriculum/CourseGroup/CreateCourse.cshtml", new CourseGroup { CurriculumCourses = models });
        }

        public ActionResult EditGroup(long id, string returnUrl, bool isSpecialGroup = false)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.IsSpecialGroup = isSpecialGroup;
            var model = _curriculumProvider.FindCourseGroup(id);
            var type = _masterProvider.FindSpecializationGroup(model.SpecializationGroupId ?? 0)?.Type;
            model.SpecializationGroupType = type;
            
            CreateSelectList();
            return View("~/Views/Curriculum/CourseGroup/EditGroup.cshtml", model);
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditGroup(CourseGroup model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if(string.IsNullOrEmpty(model.NameEn) || string.IsNullOrEmpty(model.NameTh))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View("~/Views/Curriculum/CourseGroup/EditGroup.cshtml",  model);
            }
            var curriculum = _curriculumProvider.GetCurriculumByVersionId(model.CurriculumVersionId ?? 0);
            if (ModelState.IsValid)
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        var courseGroup = _db.CourseGroups.SingleOrDefault(x => x.Id == model.Id);
                        var originalSpecializationGroupId = courseGroup.SpecializationGroupId;
                        
                        courseGroup.CurriculumVersionId = model.CurriculumVersionId;
                        courseGroup.CourseGroupId = model.CourseGroupId;
                        courseGroup.NameEn = model.NameEn;
                        courseGroup.NameTh = model.NameTh;
                        courseGroup.DescriptionEn = model.DescriptionEn;
                        courseGroup.DescriptionTh = model.DescriptionTh;
                        courseGroup.Remark = model.Remark;
                        courseGroup.Credit = model.Credit;
                        courseGroup.Sequence = model.Sequence;
                        courseGroup.Type = model.Type;
                        courseGroup.RequiredGradeId = model.RequiredGradeId;
                        courseGroup.IsAutoAssignGraduationCourse = model.IsAutoAssignGraduationCourse;
                        
                        if (model.CurriculumVersionId != null)
                        {
                            if (originalSpecializationGroupId != courseGroup.SpecializationGroupId)
                            {
                                if (originalSpecializationGroupId != null)
                                {
                                    var courseGroups = _curriculumProvider.GetCourseGroupChilds(courseGroup.Id);
                                    var curriculumCourses = _db.CurriculumCourses.Where(x => courseGroups.Select(y => y.Id)
                                                                                                        .Contains(x.CourseGroupId))
                                                                                .ToList();
                                    
                                    _db.CurriculumCourses.RemoveRange(curriculumCourses);
                                    _db.CourseGroups.RemoveRange(courseGroups);
                                    _db.SaveChanges();
                                }

                                if (courseGroup.SpecializationGroupId != null)
                                {
                                    _curriculumProvider.CopyCourseGroupFromSpecilizationGroup(courseGroup);
                                }
                            }
                        }

                        _db.SaveChanges();
                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return Redirect(returnUrl);
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.UnableToEdit);
                        CreateSelectList();
                        return View("~/Views/Curriculum/CourseGroup/EditGroup.cshtml",  model);
                    }
                }
            }
            
            _flashMessage.Danger(Message.UnableToEdit);
            CreateSelectList();
            return View("~/Views/Curriculum/CourseGroup/EditGroup.cshtml",  model);
        }

        public ActionResult EditCourse(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateCurriculumCourseSelectList();
            var version =  _curriculumProvider.GetVersionIdByCourseGroup(id);
            var courses = _curriculumProvider.FindCurriculumCourses(id, version?.Id);
            var courseGroup = _curriculumProvider.FindCourseGroup(id);
            var type = _masterProvider.FindSpecializationGroup(courseGroup.SpecializationGroupId ?? 0)?.Type;
            var defaultGrade = courseGroup.RequiredGradeId ?? 0;
            var defaultType = courseGroup.Type;
            
            if (courses.Any())
            {
                courses.ForEach(x => 
                                     { 
                                         x.SpecializationGroupId = courseGroup.SpecializationGroupId; 
                                         x.SpecializationGroupType = type; 
                                     });
            }
            else
            {
                courses.Add(new CurriculumCourse
                            {
                                CourseGroupId = id,
                                CurriculumVersionId = version?.Id,
                                RequiredGradeId = defaultGrade,
                                IsRequired = defaultType == "r",
                                SpecializationGroupId = courseGroup.SpecializationGroupId,
                                SpecializationGroupType = type
                            }); 
            }

            var model = new CourseGroup
                        {
                            Id = id,
                            RequiredGradeId = defaultGrade,
                            CurriculumCourses = courses,
                            Type = defaultType
                        };

            return View("~/Views/Curriculum/CourseGroup/EditCourse.cshtml", model);
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCourse(List<CurriculumCourse> model, long courseGroupId, string returnUrl)
        {
            var courseGroup = _curriculumProvider.FindCourseGroup(courseGroupId);
            var requiredGradeId = courseGroup?.RequiredGradeId ?? 0;
            model = model.Where(x => x.CourseId != 0).ToList();
            courseGroup.CurriculumCourses = model;
            if(courseGroup.CurriculumCourses != null && courseGroup.CurriculumCourses.Any())
            {
                if(courseGroup.CurriculumCourses.GroupBy(x => x.CourseId).Any(x => x.Count() > 1))
                {
                    _flashMessage.Danger(Message.DuplicateCourses);
                    CreateCurriculumCourseSelectList();
                    return View("~/Views/Curriculum/CourseGroup/EditCourse.cshtml", courseGroup);
                }
            }

            try
            {
                var updatedCourseIds = model.Where(x => x.Id != 0)
                                            .Select(x => x.Id)
                                            .ToList();
                                            
                var removeCourses = _db.CurriculumCourses.Where(x => x.CourseGroupId == courseGroupId
                                                                     && !updatedCourseIds.Contains(x.Id))
                                                         .ToList();
                var newCourses = model.Where(x => x.Id == 0)
                                      .ToList();
                foreach(var item in model.Where(x => updatedCourseIds.Contains(x.Id)))
                {
                    _db.Entry(item).State = EntityState.Modified;
                    _db.Entry(item).Property(x => x.CreatedAt).IsModified = false;
                    _db.Entry(item).Property(x => x.CreatedBy).IsModified = false;
                }
                _db.CurriculumCourses.RemoveRange(removeCourses);
                _db.CurriculumCourses.AddRange(newCourses);
                _db.SaveChanges();
                
                _flashMessage.Confirmation(Message.SaveSucceed);
                return Redirect(returnUrl);
            }
            catch (Exception)
            {
                _flashMessage.Danger(Message.UnableToEdit);
                CreateCurriculumCourseSelectList();
                return View("~/Views/Curriculum/CourseGroup/EditCourse.cshtml", courseGroup);
            }
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        public ActionResult DeleteCourseGroup(long id, string returnUrl)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                var parentGroup = _db.CourseGroups.SingleOrDefault(x => x.Id == id);
                parentGroup.SpecializationGroupType = _masterProvider.FindSpecializationGroup(parentGroup.SpecializationGroupId ?? 0)?.Type;

                var courseGroups = _curriculumProvider.GetCourseGroupChilds(id);
                courseGroups.Add(_db.CourseGroups.SingleOrDefault(x => x.Id == id)); 
                
                var curriculumCourses = _db.CurriculumCourses.Where(x => courseGroups.Select(y => y.Id).Contains(x.CourseGroupId))
                                                             .ToList();

                try
                {
                    _db.CurriculumCourses.RemoveRange(curriculumCourses);
                    _db.CourseGroups.RemoveRange(courseGroups);
                    _db.SaveChanges();

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToDelete);
                }
                
                if (parentGroup.CurriculumVersionId == null)
                {
                    return RedirectToAction("Details", "SpecializationGroup",
                                            new 
                                            {
                                                minorId = parentGroup.SpecializationGroupType == SpecializationGroup.TYPE_MINOR_CODE ? parentGroup.SpecializationGroupId : null,
                                                concentrationId = parentGroup.SpecializationGroupType == SpecializationGroup.TYPE_CONCENTRATION_CODE ? parentGroup.SpecializationGroupId : null,
                                                moduleId = parentGroup.SpecializationGroupType == SpecializationGroup.TYPE_MODULE_CODE ? parentGroup.SpecializationGroupId : null,
                                                returnUrl = returnUrl
                                            });
                }
                
                return RedirectToAction("Details", "CurriculumVersion", new { id = courseGroups.FirstOrDefault().CurriculumVersionId, returnUrl = returnUrl });
            }
        }

        private void CreateSelectList() 
        {
            ViewBag.CourseGroupMinors = _selectListProvider.GetMinors();
            ViewBag.CourseGroupConcentrations = _selectListProvider.GetConcentrations();
            ViewBag.GroupTypes = _selectListProvider.GetCourseGroupTypes();
            ViewBag.Grades = _selectListProvider.GetGrades();
            ViewBag.CurriculumCourseGroups = _curriculumProvider.GetCurriculumCourseGroups();
        }

        private void CreateCurriculumCourseSelectList() 
        {
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            ViewBag.Grades = _selectListProvider.GetGrades();
            ViewBag.GradeTemplates = _selectListProvider.GetGradeTemplates();
        }
    }
}