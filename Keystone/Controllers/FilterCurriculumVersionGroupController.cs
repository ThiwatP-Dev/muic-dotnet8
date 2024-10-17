using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class FilterCurriculumVersionGroupController : BaseController
    {
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;

        public FilterCurriculumVersionGroupController(ApplicationDbContext db, 
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

            var model = _db.FilterCurriculumVersionGroups.AsNoTracking()
                                                         .Where(x => (string.IsNullOrEmpty(criteria.Name) 
                                                                      || x.Name.Contains(criteria.Name)))
                                                         .Select(x => new FilterCurriculumVersionGroupViewModel
                                                                      {
                                                                          Id = x.Id,
                                                                          Name = x.Name,
                                                                          Description = x.Description
                                                                      })
                                                         .GetPaged(criteria, page);

            var filterCurriculumVersionGroupIds = model.Results.Select(x => x.Id).ToList();
            var details = _db.FilterCurriculumVersionGroupDetails.Where(x => filterCurriculumVersionGroupIds.Contains(x.FilterCurriculumVersionGroupId))
                                                                 .ToList();

            foreach (var item in model.Results)
            {
                item.CurriculumVersionCount = details.Where(x => x.FilterCurriculumVersionGroupId == item.Id)
                                                     .Count();
            }

            return View(model);
        }

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
            
            var curriculumVersions = _db.CurriculumVersions.Where(x => x.Curriculum.AcademicLevelId == criteria.AcademicLevelId
                                                                       && (criteria.FacultyId == 0 
                                                                           || x.Curriculum.FacultyId == criteria.FacultyId)
                                                                       && (criteria.CurriculumId == 0 
                                                                           || x.CurriculumId == criteria.CurriculumId)
                                                                       && (criteria.DepartmentId == 0 
                                                                           || x.Curriculum.DepartmentId == criteria.DepartmentId))
                                                           .Select(x => new FilterCurriculumVersionGroupDetailViewModel
                                                                        {
                                                                            CurriculumVersionId = x.Id,
                                                                            Code = x.Code,
                                                                            NameEn = x.NameEn,
                                                                            Faculty = x.Curriculum.Faculty.NameEn,
                                                                            Department = x.Curriculum.Department.Code,
                                                                            Curriculum = x.Curriculum.NameEn
                                                                        })
                                                           .OrderBy(x => x.Faculty)
                                                              .ThenBy(x => x.Department)
                                                              .ThenBy(x => x.Code)
                                                              .ThenBy(x => x.NameEn)
                                                           .ToList();
            
            var result = new FilterCurriculumVersionGroupViewModel
                         {
                             Criteria = criteria,
                             CurriculumVersions = curriculumVersions
                         };
            return View(result);
        }

        [HttpPost]
        [RequestFormLimits(ValueCountLimit = 100000)]
        public IActionResult Create(FilterCurriculumVersionGroupViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.Criteria.AcademicLevelId, model.Criteria.FacultyId);

            if(string.IsNullOrEmpty(model.Name))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }
            
            if(IsFilterCurriculumVersionGroupExist(model.Name))
            {
                _flashMessage.Warning(Message.DuplicateFilterCurriculumVersionGroup);
                return View(model);
            }

            model.CurriculumVersions = model.CurriculumVersions.Where(x => x.IsChecked == "on").ToList();
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var filterCurriculumVersionGroup = new FilterCurriculumVersionGroup
                                                       {
                                                           Name = model.Name,
                                                           Description = model.Description
                                                       };

                    _db.FilterCurriculumVersionGroups.Add(filterCurriculumVersionGroup);
                    _db.SaveChanges();
                    
                    var filterCurriculumVersionGroupDetails = new List<FilterCurriculumVersionGroupDetail>();
                    foreach (var item in model.CurriculumVersions)
                    {
                        var filterCurriculumVersionGroupDetail = new FilterCurriculumVersionGroupDetail
                                                                 {
                                                                     FilterCurriculumVersionGroupId = filterCurriculumVersionGroup.Id,
                                                                     CurriculumVersionId = item.CurriculumVersionId
                                                                 };

                        filterCurriculumVersionGroupDetails.Add(filterCurriculumVersionGroupDetail);
                    }

                    _db.FilterCurriculumVersionGroupDetails.AddRange(filterCurriculumVersionGroupDetails);
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
        public IActionResult GetCurriculumVersions([FromForm]Criteria criteria, long filterCurriculumVersionGroupId)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId != 0)
            {
                var model = Search(criteria);
                model.Id = filterCurriculumVersionGroupId;
                return PartialView("~/Views/FilterCurriculumVersionGroup/_FormCurriculumVersionSelect.cshtml", model);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(FilterCurriculumVersionGroupViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if(string.IsNullOrEmpty(model.Name))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }
            
            if(IsFilterCurriculumVersionGroupExist(model.Name, model.Id))
            {
                _flashMessage.Warning(Message.DuplicateFilterCurriculumVersionGroup);
                return RedirectToAction(nameof(Edit), new { id = model.Id , returnUrl = returnUrl } );
            }

            try
            {
                var filterCurriculumVersionGroup = _db.FilterCurriculumVersionGroups.SingleOrDefault(x => x.Id == model.Id);
                filterCurriculumVersionGroup.Name = model.Name;
                filterCurriculumVersionGroup.Description = model.Description;
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Edit), new { id = model.Id , returnUrl = returnUrl } );
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
                return RedirectToAction(nameof(Edit), new { id = model.Id , returnUrl = returnUrl } );
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = 100000)]
        public IActionResult AddCurriculumVersions(FilterCurriculumVersionGroupViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.Criteria?.AcademicLevelId ?? 0,model.Criteria?.FacultyId ?? 0);
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var curriculumVersions = model.CurriculumVersions.Where(x => x.IsChecked == "on")
                                                                     .ToList();

                    var curriculumVersionIds = curriculumVersions.Select(x => x.CurriculumVersionId).ToList();
                    var oldFilterCurriculumVersionGroupDetails = _db.FilterCurriculumVersionGroupDetails.Where(x => x.FilterCurriculumVersionGroupId == model.Id
                                                                                                                    && curriculumVersionIds.Contains(x.CurriculumVersionId) 
                                                                                                                    && x.IsActive)
                                                                                                        .ToList();
                    if (oldFilterCurriculumVersionGroupDetails.Any())
                    {
                        var oldFilterCurriculumVersionIds = oldFilterCurriculumVersionGroupDetails.Select(x => x.CurriculumVersionId).ToList();
                        curriculumVersions = curriculumVersions.Where(x => !oldFilterCurriculumVersionIds.Contains(x.CurriculumVersionId)).ToList();
                    }

                    var filterCurriculumVersionGroupDetails = new List<FilterCurriculumVersionGroupDetail>();
                    foreach (var curriculumVersion in curriculumVersions)
                    {
                        filterCurriculumVersionGroupDetails.Add(new FilterCurriculumVersionGroupDetail
                                                                {
                                                                    FilterCurriculumVersionGroupId = model.Id,
                                                                    CurriculumVersionId = curriculumVersion.CurriculumVersionId
                                                                });
                    }

                    _db.FilterCurriculumVersionGroupDetails.AddRange(filterCurriculumVersionGroupDetails);
                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Edit), new { id = model.Id , returnUrl = returnUrl });
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return RedirectToAction(nameof(Edit), new { id = model.Id , returnUrl = returnUrl } );
                }
            }
        }

        public IActionResult Delete(long id, string returnUrl)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var details = _db.FilterCurriculumVersionGroupDetails.Where(x => x.FilterCurriculumVersionGroupId == id)
                                                                         .ToList();
                    _db.FilterCurriculumVersionGroupDetails.RemoveRange(details);
                    _db.SaveChanges();

                    var model = _db.FilterCurriculumVersionGroups.SingleOrDefault(x => x.Id == id);
                    _db.FilterCurriculumVersionGroups.Remove(model);
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

        public IActionResult DeleteCurriculumVersion(long id, string returnUrl)
        {
            var model = _db.FilterCurriculumVersionGroupDetails.SingleOrDefault(x => x.Id == id);
            try
            {
                _db.FilterCurriculumVersionGroupDetails.Remove(model);
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

        private FilterCurriculumVersionGroupViewModel Search(Criteria criteria)
        {
            var model = new FilterCurriculumVersionGroupViewModel();
            var curriculumVersions = _db.CurriculumVersions.AsNoTracking()
                                                           .Where(x => x.Curriculum.AcademicLevelId == criteria.AcademicLevelId
                                                                       && (criteria.FacultyId == 0 
                                                                           || x.Curriculum.FacultyId == criteria.FacultyId)
                                                                       && (criteria.CurriculumId == 0 
                                                                           || x.CurriculumId == criteria.CurriculumId)
                                                                       && (criteria.DepartmentId == 0 
                                                                           || x.Curriculum.DepartmentId == criteria.DepartmentId))
                                                           .Select(x => new FilterCurriculumVersionGroupDetailViewModel
                                                                        {
                                                                            CurriculumVersionId = x.Id,
                                                                            Code = x.Code,
                                                                            NameEn = x.NameEn,
                                                                            Faculty = x.Curriculum.Faculty.NameEn,
                                                                            Department = x.Curriculum.Department.NameEn,
                                                                            Curriculum = x.Curriculum.NameEn
                                                                        })
                                                           .ToList();

            model.CurriculumVersions = curriculumVersions;
            model.Criteria = criteria;
            return model;
        }

        private FilterCurriculumVersionGroupViewModel Find(long id)
        {
            var filterCurriculumVersionDetails = _db.FilterCurriculumVersionGroupDetails.Where(x => x.FilterCurriculumVersionGroupId == id)
                                                                                        .Select(x => new FilterCurriculumVersionGroupDetailViewModel
                                                                                                     {
                                                                                                         FilterCurriculumVersionGroupDetailId = x.Id,
                                                                                                         FilterCurriculumVersionGroupId = x.FilterCurriculumVersionGroupId,
                                                                                                         Code = x.CurriculumVersion.Code,
                                                                                                         NameEn = x.CurriculumVersion.NameEn,
                                                                                                         Faculty = x.CurriculumVersion.Curriculum.Faculty.NameEn,
                                                                                                         Department = x.CurriculumVersion.Curriculum.Department.NameEn,
                                                                                                         Curriculum = x.CurriculumVersion.Curriculum.NameEn                                                                                                         
                                                                                                     })
                                                                                        .ToList();

            var filterCurriculumVersionGroup = _db.FilterCurriculumVersionGroups.Where(x => x.Id == id)
                                                                                .Select(x => new FilterCurriculumVersionGroupViewModel
                                                                                                 {
                                                                                                     Id = x.Id,
                                                                                                     Name = x.Name,
                                                                                                     Description = x.Description,
                                                                                                     CurriculumVersions = filterCurriculumVersionDetails
                                                                                                 })
                                                                                .SingleOrDefault();
            return filterCurriculumVersionGroup;
        }

       private bool IsFilterCurriculumVersionGroupExist(string name, long id = 0)
       {
           var nameLower = name.ToLower();
           return _db.FilterCurriculumVersionGroups.Any(x => x.Name.ToLower() == nameLower && x.Id != id);
       }
       private void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long departmentId = 0)
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
            if (departmentId != 0)
            {
                ViewBag.Curriculums = _selectListProvider.GetCurriculumByDepartment(academicLevelId, facultyId, departmentId);
            }
        }
    }
}