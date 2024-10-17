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
    [PermissionAuthorize("FilterCourseGroup", "")]
    public class FilterCourseGroupController : BaseController
    {
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;

        public FilterCourseGroupController(ApplicationDbContext db, 
                                           IFlashMessage flashMessage,
                                           ISelectListProvider selectListProvider,
                                           IDateTimeProvider dateTimeProvider,
                                           IRegistrationProvider registrationProvider) : base(db, flashMessage, selectListProvider)
        {
            _registrationProvider = registrationProvider;
            _dateTimeProvider = dateTimeProvider;
        }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            // if (string.IsNullOrEmpty(criteria.Name))
            // {
            //     _flashMessage.Warning(Message.RequiredData);
            //     return View();
            // }

            var model = _db.FilterCourseGroups.AsNoTracking()
                                              .Where(x => (string.IsNullOrEmpty(criteria.Name) 
                                                           || x.Name.Contains(criteria.Name)))
                                              .Select(x => new FilterCourseGroupViewModel
                                                           {
                                                               Id = x.Id,
                                                               Name = x.Name,
                                                               Description = x.Description
                                                           })
                                              .GetPaged(criteria, page);

            var filterCourseGroupIds = model.Results.Select(x => x.Id).ToList();
            var details = _db.FilterCourseGroupDetails.Where(x => filterCourseGroupIds.Contains(x.FilterCourseGroupId))
                                                      .ToList();

            foreach (var item in model.Results)
            {
                item.CourseCount = details.Where(x => x.FilterCourseGroupId == item.Id)
                                          .Count();
            }

            return View(model);
        }

        [PermissionAuthorize("FilterCourseGroup", PolicyGenerator.Write)]
        [HttpGet]
        public IActionResult Create(Criteria criteria, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if(criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var courses = _db.Courses.Where(x => x.AcademicLevelId == criteria.AcademicLevelId
                                                 && (criteria.FacultyId == 0 
                                                     || x.FacultyId == criteria.FacultyId)
                                                 && (criteria.DepartmentId == 0 
                                                     || x.DepartmentId == criteria.DepartmentId))
                                     .Select(x => new FilterCourseGroupDetailViewModel
                                                  {
                                                      CourseId = x.Id,
                                                      Code = x.Code,
                                                      NameEn = x.NameEn,
                                                      Lecture = x.Lecture,
                                                      Lab = x.Lab,
                                                      Other = x.Other,
                                                      Credit = x.Credit,
                                                      CourseRateId = x.CourseRateId,
                                                      Faculty = x.Faculty.NameEn,
                                                      Department = x.Department.NameEn
                                                  })
                                     .OrderBy(x => x.Faculty)
                                        .ThenBy(x => x.Department)
                                        .ThenBy(x => x.Code)
                                     .ToList();
            
            var result = new FilterCourseGroupViewModel
                         {
                             Criteria = criteria,
                             Courses = courses
                         };
            return View(result);
        }

        [PermissionAuthorize("FilterCourseGroup", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = 100000)]
        public IActionResult Create(FilterCourseGroupViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.Criteria.AcademicLevelId, model.Criteria.FacultyId);
            if(string.IsNullOrEmpty(model.Name))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            if(IsFilterCourseGroupExist(model.Name))
            {
                _flashMessage.Warning(Message.DuplicateFilterCourseGroup);
                return View(model);
            }
            model.Courses = model.Courses.Where(x => x.IsChecked == "on").ToList();
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var filterCourseGroup = new FilterCourseGroup
                                            {
                                                Name = model.Name,
                                                Description = model.Description
                                            };

                    _db.FilterCourseGroups.Add(filterCourseGroup);
                    _db.SaveChanges();
                    
                    var filterCourseGroupDetails = new List<FilterCourseGroupDetail>();
                    foreach (var item in model.Courses)
                    {
                        var filterCourseGroupDetail = new FilterCourseGroupDetail
                                                      {
                                                          FilterCourseGroupId = filterCourseGroup.Id,
                                                          CourseId = item.CourseId
                                                      };

                        filterCourseGroupDetails.Add(filterCourseGroupDetail);
                    }

                    _db.FilterCourseGroupDetails.AddRange(filterCourseGroupDetails);
                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return Redirect(returnUrl);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View();
                }
            }
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult GetCourses([FromForm]Criteria criteria, long filterCourseGroupId)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId != 0)
            {
                var model = Search(criteria);
                model.Id = filterCourseGroupId;
                return PartialView("~/Views/FilterCourseGroup/_FormCourseSelect.cshtml", model);
            }
            else
            {
                return null;
            }
        }

        public IActionResult Edit(long id, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            var model = Find(id);

            return View(model);
        }

        [PermissionAuthorize("FilterCourseGroup", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(FilterCourseGroupViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if(string.IsNullOrEmpty(model.Name))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            if(IsFilterCourseGroupExist(model.Name, model.Id))
            {
                _flashMessage.Warning(Message.DuplicateFilterCourseGroup);
                return View(model);
            }
            try
            {
                var filterCourseGroup = _db.FilterCourseGroups.SingleOrDefault(x => x.Id == model.Id);
                filterCourseGroup.Name = model.Name;
                filterCourseGroup.Description = model.Description;
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Edit), new { id = model.Id , returnUrl = returnUrl } );
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
                return View(model);
            }
        }

        [PermissionAuthorize("FilterCourseGroup", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = 100000)]
        public IActionResult AddCourses(FilterCourseGroupViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.Criteria?.AcademicLevelId ?? 0,model.Criteria?.FacultyId ?? 0);

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var courses = model.Courses.Where(x => x.IsChecked == "on")
                                               .ToList();

                    var courseIds = courses.Select(x => x.CourseId).ToList();
                    var oldFilterCourseGroupDetails = _db.FilterCourseGroupDetails.Where(x => x.FilterCourseGroupId == model.Id
                                                                                              && courseIds.Contains(x.CourseId) 
                                                                                              && x.IsActive)
                                                                                  .ToList();
                    if(oldFilterCourseGroupDetails.Any())
                    {
                        var filterOldCourseIds = oldFilterCourseGroupDetails.Select(x => x.CourseId).ToList();
                        courses = courses.Where(x => !filterOldCourseIds.Contains( x.CourseId)).ToList();
                    }
                    var filterCourseGroupDetails = new List<FilterCourseGroupDetail>();
                    foreach(var course in courses)
                    {
                        filterCourseGroupDetails.Add(new FilterCourseGroupDetail
                                                     {
                                                         FilterCourseGroupId = model.Id,
                                                         CourseId = course.CourseId
                                                     });
                    }
                    _db.FilterCourseGroupDetails.AddRange(filterCourseGroupDetails);
                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Edit), new { id = model.Id , returnUrl = returnUrl } );
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return RedirectToAction(nameof(Edit), new { id = model.Id , returnUrl = returnUrl } );
                }
            }
        }

        [PermissionAuthorize("FilterCourseGroup", PolicyGenerator.Write)]
        public IActionResult Delete(long id, string returnUrl)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var details = _db.FilterCourseGroupDetails.Where(x => x.FilterCourseGroupId == id)
                                                            .ToList();
                    _db.FilterCourseGroupDetails.RemoveRange(details);
                    _db.SaveChanges();

                    var model = _db.FilterCourseGroups.SingleOrDefault(x => x.Id == id);
                    _db.FilterCourseGroups.Remove(model);
                    _db.SaveChanges();

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCancel);
                }
            }
            
            return Redirect(returnUrl);
        }

        [PermissionAuthorize("FilterCourseGroup", PolicyGenerator.Write)]
        public IActionResult DeleteCourse(long id, string returnUrl)
        {
            var model = _db.FilterCourseGroupDetails.SingleOrDefault(x => x.Id == id);
            try
            {
                _db.FilterCourseGroupDetails.Remove(model);
                _db.SaveChanges();

                _flashMessage.Confirmation(Message.SaveSucceed);
                return Redirect(returnUrl);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCancel);
            }

            return Redirect(returnUrl);
        }

        private FilterCourseGroupViewModel Search(Criteria criteria)
        {
            var model = new FilterCourseGroupViewModel();
            var courses = _db.Courses.AsNoTracking()
                                     .Where(x => x.AcademicLevelId == criteria.AcademicLevelId
                                                 && (criteria.FacultyId == 0 
                                                     || x.FacultyId == criteria.FacultyId)
                                                 && (criteria.DepartmentId == 0 
                                                     || x.DepartmentId == criteria.DepartmentId))
                                     .Select(x => new FilterCourseGroupDetailViewModel
                                                  {
                                                      CourseId = x.Id,
                                                      Code = x.Code,
                                                      NameEn = x.NameEn,
                                                      Lecture = x.Lecture,
                                                      Lab = x.Lab,
                                                      Other = x.Other,
                                                      Credit = x.Credit,
                                                      CourseRateId = x.CourseRateId,
                                                      Faculty = x.Faculty.NameEn,
                                                      Department = x.Department.NameEn
                                                  })
                                     .ToList();

            model.Courses = courses;
            model.Criteria = criteria;
            return model;
        }

        private FilterCourseGroupViewModel Find(long id)
        {
            var filterCourseGroupDetails = _db.FilterCourseGroupDetails.Where(x => x.FilterCourseGroupId == id)
                                                                       .Select(x => new FilterCourseGroupDetailViewModel
                                                                                    {
                                                                                        FilterCourseGroupDetailId = x.Id,
                                                                                        FilterCourseGroupId = x.FilterCourseGroupId,
                                                                                        Code = x.Course.Code,
                                                                                        NameEn = x.Course.NameEn,
                                                                                        Lecture = x.Course.Lecture,
                                                                                        Lab = x.Course.Lab,
                                                                                        Other = x.Course.Other,
                                                                                        Credit = x.Course.Credit,
                                                                                        CourseRateId = x.Course.CourseRateId,
                                                                                        Faculty = x.Course.Faculty.NameEn,
                                                                                        Department = x.Course.Department.NameEn
                                                                                    })
                                                                       .ToList();

            var filterCourseGroup = _db.FilterCourseGroups.Where(x => x.Id == id)
                                                          .Select(x => new FilterCourseGroupViewModel
                                                                           {
                                                                               Id = x.Id,
                                                                               Name = x.Name,
                                                                               Description = x.Description,
                                                                               Courses = filterCourseGroupDetails
                                                                           })
                                                          .SingleOrDefault();
            return filterCourseGroup;
        }

       private bool IsFilterCourseGroupExist(string name, long id = 0)
       {
           var nameLower = name.ToLower();
           return _db.FilterCourseGroups.Any(x => x.Name.ToLower() == nameLower && x.Id != id);
       }

       private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if(academicLevelId != 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }
            if (facultyId != 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartments(facultyId);
            }
        }
    }
}