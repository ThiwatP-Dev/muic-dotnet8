using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("Grade", "")]
    public class GradeController : BaseController
    {
        public GradeController(ApplicationDbContext db,
                               IFlashMessage flashMessage,
                               ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(int page)
        {
            var models = _db.Grades.Include(x => x.AcademicLevel)
                                   .Include(x => x.Faculty)
                                   .Include(x => x.Department)
                                   .IgnoreQueryFilters()
                                   .OrderBy(x => x.AcademicLevel.NameEn)
                                       .ThenBy(x => x.Faculty.ShortNameEn)
                                       .ThenBy(x => x.Department.ShortNameEn)
                                       .ThenByDescending(x => x.Weight)
                                   .GetPaged(page);
            return View(models);
        }

        [PermissionAuthorize("Grade", PolicyGenerator.Write)]
        public IActionResult Create()
        {
            CreateSelectList();
            return View(new Grade());
        }

        [PermissionAuthorize("Grade", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Grade model)
        {
            if (ModelState.IsValid)
            {
                if (IsExisted(model))
                {
                    CreateSelectList();
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }
                else
                {
                    try
                    {
                        _db.Grades.Add(model);
                        _db.SaveChanges();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index));
                    }
                    catch
                    {
                        CreateSelectList();
                        _flashMessage.Danger(Message.UnableToCreate);
                        return View(model);
                    }
                }
            }

            CreateSelectList();
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public IActionResult Edit(long id)
        {
            CreateSelectList();
            Grade model = Find(id);
            return View(model);
        }

        [PermissionAuthorize("Grade", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Grade model)
        {
            if (IsExisted(model))
            {
                CreateSelectList();
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }
            else
            {
                var beforeUpdate = Find(model.Id);
                if (ModelState.IsValid && await TryUpdateModelAsync<Grade>(beforeUpdate))
                {
                    try
                    {
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index));
                    }
                    catch
                    {
                        CreateSelectList();
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
            }

            CreateSelectList();
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("Grade", PolicyGenerator.Write)]
        public IActionResult Delete(long id)
        {
            Grade model = Find(id);
            try
            {
                _db.Grades.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private Grade Find(long? id) 
        {
            var item = _db.Grades.IgnoreQueryFilters()
                                 .SingleOrDefault(x => x.Id == id);
            return item;
        }

        public bool IsExisted(Grade model)
        {
            var grade = _db.Grades.Any(x => x.Name == model.Name
                                            && x.AcademicLevelId == model.AcademicLevelId
                                            && x.FacultyId == model.FacultyId
                                            && x.DepartmentId == model.DepartmentId
                                            && x.Id != model.Id);
            return grade;
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
        }
    }
}