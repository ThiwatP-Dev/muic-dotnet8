using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Scholarship;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class ScholarshipBudgetController : BaseController 
    {
        protected readonly IScholarshipProvider _scholarshipProvider;

        public ScholarshipBudgetController(ApplicationDbContext db,
                                           IFlashMessage flashMessage,
                                           IMapper mapper,
                                           ISelectListProvider selectListProvider,
                                           IScholarshipProvider scholarshipProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _scholarshipProvider = scholarshipProvider;
        }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.DepartmentId, criteria.ScholarshipTypeId);
            if (criteria.AcademicLevelId == 0 && criteria.FacultyId == 0 && criteria.DepartmentId == 0 && criteria.ScholarshipId == 0
                && criteria.ScholarshipTypeId == 0 && criteria.CurriculumId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var model = _db.ScholarshipBudgets.Include(x => x.Scholarship)
                                              .Include(x => x.Faculty)
                                              .Include(x => x.Department)
                                              .Include(x => x.AcademicLevel)
                                              .Include(x => x.Curriculum)
                                              .IgnoreQueryFilters()
                                              .Where(x => (criteria.ScholarshipTypeId == 0
                                                           || criteria.ScholarshipTypeId == x.Scholarship.ScholarshipTypeId)
                                                          && (criteria.ScholarshipId == 0
                                                              || criteria.ScholarshipId == x.ScholarshipId)
                                                          && (criteria.AcademicLevelId == 0
                                                              || criteria.AcademicLevelId == x.AcademicLevelId)
                                                          && (criteria.FacultyId == 0
                                                              || criteria.FacultyId == x.FacultyId)
                                                          && (criteria.DepartmentId == 0
                                                              || criteria.DepartmentId == x.DepartmentId)
                                                          && (criteria.CurriculumId == 0
                                                              || criteria.CurriculumId == x.CurriculumId))
                                              .GetPaged(criteria, page);
            return View(model);
        }

        public IActionResult Create()
        {
            CreateSelectList();
            return View(new ScholarshipBudget());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ScholarshipBudget model)
        {
            var scholarshipTypeId = _scholarshipProvider.GetScholarshipById(model.ScholarshipId)?.ScholarshipTypeId ?? 0; 
            if (ModelState.IsValid)
            {
                if (IsExisted(model))
                {
                    CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0, model.DepartmentId ?? 0, scholarshipTypeId);
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }

                try
                {
                    _db.ScholarshipBudgets.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new Criteria
                                                           {
                                                               ScholarshipId = model.ScholarshipId
                                                           });
                }
                catch
                {
                    CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0, model.DepartmentId ?? 0, scholarshipTypeId);
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }   
            }

            CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0, model.DepartmentId ?? 0, scholarshipTypeId);
            _flashMessage.Danger(Message.UnableToCreate);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(long id)
        {
            var model = Find(id);
            CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0, model.DepartmentId ?? 0, model.Scholarship.ScholarshipTypeId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid)
            {
                if (IsExisted(model))
                {
                    CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0, model.DepartmentId ?? 0, model.Scholarship.ScholarshipTypeId);
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }

                if (await TryUpdateModelAsync<ScholarshipBudget>(model))
                {
                    try
                    {
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index), new Criteria
                                                               {
                                                                   ScholarshipId = model.ScholarshipId
                                                               });
                    }
                    catch
                    {
                        CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0, model.DepartmentId ?? 0, model.Scholarship.ScholarshipTypeId);
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
            }

            CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0, model.DepartmentId ?? 0, model.Scholarship.ScholarshipTypeId);
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        public ActionResult Delete(long id)
        {
            var model = Find(id);
            try
            {
                _db.ScholarshipBudgets.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0, model.DepartmentId ?? 0, model.Scholarship.ScholarshipTypeId);
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetScholarshipBudgetTable(long id)
        {
            var budgets = _scholarshipProvider.GetScholarshipBudgetByScholarshipId(id);
            return PartialView("~/Views/ScholarshipBudget/_BudgetDetails.cshtml", budgets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CloneScholarshipBudget(long fromScholarshipId, long toScholarshipId)
        {
            var budgets = _scholarshipProvider.GetScholarshipBudgetByScholarshipId(fromScholarshipId);
            if (fromScholarshipId == toScholarshipId || toScholarshipId == 0)
            {
                CreateSelectList();
                _flashMessage.Danger(Message.UnableToClone);
            }

            List<ScholarshipBudget> clonedBudget = new List<ScholarshipBudget>();
            foreach (var item in budgets)
            {
                var budget = new ScholarshipBudget
                             {
                                 ScholarshipId = toScholarshipId
                             };

                clonedBudget.Add(_mapper.Map<ScholarshipBudget, ScholarshipBudget>(item, budget));
            }

            try
            {
                _db.ScholarshipBudgets.AddRange(clonedBudget);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                CreateSelectList();
                _flashMessage.Danger(Message.UnableToSave);
            }
            
            return RedirectToAction(nameof(Index), new { ScholarshipId = toScholarshipId });
        }

        public ScholarshipBudget Find(long? id)
        {
            var model = _db.ScholarshipBudgets.Include(x => x.Scholarship)
                                              .IgnoreQueryFilters()
                                              .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public bool IsExisted(ScholarshipBudget model)
        {
            var budget = _db.ScholarshipBudgets.Any(x => x.ScholarshipId == model.ScholarshipId
                                                         && x.FacultyId == model.FacultyId
                                                         && x.DepartmentId == model.DepartmentId
                                                         && x.CurriculumId == model.CurriculumId
                                                         && x.Id != model.Id);
            return budget;
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long departmentId = 0, long scholarshipTypeId = 0)
        {
            ViewBag.ScholarshipTypes = _selectListProvider.GetScholarshipTypes();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Scholarships = scholarshipTypeId == 0 ? _selectListProvider.GetScholarships()
                                                          : _selectListProvider.GetScholarshipsByScholarshipTypeId(scholarshipTypeId);
            
            if (academicLevelId != 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);

                if (facultyId != 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                    if (departmentId != 0)
                    {
                        ViewBag.Curriculums = _selectListProvider.GetCurriculumByDepartment(academicLevelId, facultyId, departmentId);
                    }
                }
            }
        }
    }
}