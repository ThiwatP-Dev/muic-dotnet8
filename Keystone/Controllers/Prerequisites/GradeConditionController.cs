using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Prerequisites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("GradeCondition", "")]
    public class GradeConditionController : BaseController
    {
        protected readonly IPrerequisiteProvider _prerequisiteProvider;

        public GradeConditionController(ApplicationDbContext db,
                                        IFlashMessage flashMessage,
                                        ISelectListProvider selectListProvider,
                                        IPrerequisiteProvider prerequisiteProvider) : base(db, flashMessage, selectListProvider)
        {
            _prerequisiteProvider = prerequisiteProvider;
        }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList();
            var models = _db.GradeConditions.AsNoTracking()
                                            .IgnoreQueryFilters()
                                            .Where(x => (criteria.CourseId == 0
                                                         || x.CourseId == criteria.CourseId)
                                                        && (criteria.GradeId == 0
                                                            || x.GradeId == criteria.GradeId)
                                                        && (string.IsNullOrEmpty(criteria.Status) 
                                                            || x.IsActive == Convert.ToBoolean(criteria.Status)))
                                            .Select(x => new GradeConditionViewModel
                                                         {
                                                            Id = x.Id,
                                                            CourseCode = x.Course.Code,
                                                            CourseName = x.Course.NameEn,
                                                            Credit = x.Course.Credit,
                                                            Lab = x.Course.Lab,
                                                            Other = x.Course.Other,
                                                            CourseRateId = x.Course.CourseRateId,
                                                            Lecture = x.Course.Lecture,
                                                            GradeName = x.Grade.Name,
                                                            IsActive = x.IsActive,
                                                            ExpiredAt = x.ExpiredAt
                                                         })
                                            .OrderBy(x => x.CourseCode)
                                                .ThenBy(x => x.GradeName)
                                            .GetPaged(criteria, page, true);
            return View(models);
        }

        [PermissionAuthorize("GradeCondition", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new GradeCondition());
        }

        [PermissionAuthorize("GradeCondition", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GradeCondition model, string returnUrl)
        {
            if (_prerequisiteProvider.IsExistGradeCondition(model))
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
                    model.Description = _prerequisiteProvider.GetGradeConditionDescription(model, false);
                    _db.GradeConditions.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { CourseId = model.CourseId });
                }
                catch
                {
                    CreateSelectList();
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            GradeCondition model = _prerequisiteProvider.GetGradeCondition(id);
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            return View(model);
        }

        [PermissionAuthorize("GradeCondition", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = _prerequisiteProvider.GetGradeCondition(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<GradeCondition>(model))
            {
                // if (_prerequisiteProvider.IsExistGradeCondition(model))
                // {
                //     CreateSelectList();
                //     ViewBag.ReturnUrl = returnUrl;
                //     _flashMessage.Danger(Message.DataAlreadyExist);
                //     return View(model);
                // }
                // else
                // {
                    try
                    {
                        model.Description = _prerequisiteProvider.GetGradeConditionDescription(model, false);
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return Redirect(returnUrl);
                    }
                    catch
                    {
                        CreateSelectList();
                        ViewBag.ReturnUrl = returnUrl;
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                // }
            }

            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("GradeCondition", PolicyGenerator.Write)]
        public ActionResult Delete(long id, string returnUrl)
        {
            GradeCondition model = _prerequisiteProvider.GetGradeCondition(id);
            CreateSelectList();
            var check = _prerequisiteProvider.IsConditionAlreadyExits(model.Id, "grade");
            if(check.IsAlreadyExist)
            {
                _flashMessage.Danger(check.Message);
                return Redirect(returnUrl);
            }
            try
            {
                _db.GradeConditions.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return Redirect(returnUrl);
        }

        private void CreateSelectList()
        {
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.Grades = _selectListProvider.GetGrades();
            ViewBag.ActiveStatuses = _selectListProvider.GetActiveStatuses();
        }
    }
}