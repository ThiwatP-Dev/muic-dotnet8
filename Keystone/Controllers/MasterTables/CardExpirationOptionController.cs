using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("CardExpirationOption", "")]
    public class CardExpirationOptionController : BaseController
    {
        public CardExpirationOptionController(ApplicationDbContext db,
                                              IFlashMessage flashMessage,
                                              ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider){}

        public IActionResult Index(int page = 1)
        {
            var models = _db.CardExpirationOptions.Include(x => x.Faculty)
                                                  .Include(x => x.Department)
                                                  .Include(x => x.AcademicLevel)
                                                  .IgnoreQueryFilters()
                                                  .OrderBy(x => x.AcademicLevel.NameEn)
                                                  .ThenBy(x => x.Faculty.NameEn)
                                                  .ThenBy(x => x.Department.NameEn)
                                                  .GetPaged(page);
            return View(models);
        }

        [PermissionAuthorize("CardExpirationOption", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View(new CardExpirationOption());
        }

        [PermissionAuthorize("CardExpirationOption", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CardExpirationOption model)
        {
            if (ModelState.IsValid)
            {
                if (IsExist(model))
                {
                    CreateSelectList(model.AcademicLevelId, model.FacultyId ?? 0);
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }
                else 
                {
                    _db.CardExpirationOptions.Add(model);
                    try
                    {
                        _db.SaveChanges();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index));
                    }
                    catch
                    {
                        CreateSelectList(model.AcademicLevelId, model.FacultyId ?? 0);
                        _flashMessage.Danger(Message.UnableToCreate);
                        return View(model);
                    }
                }
            }

            CreateSelectList(model.AcademicLevelId, model.FacultyId ?? 0);
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            CardExpirationOption model = Find(id);
            CreateSelectList(model.AcademicLevelId, model.FacultyId ?? 0);
            return View(model);
        }

        [PermissionAuthorize("CardExpirationOption", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CardExpirationOption model)
        {
            if (IsExist(model))
            {
                CreateSelectList(model.AcademicLevelId, model.FacultyId ?? 0);
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }
            else
            {
                var beforeUpdated = Find(model.Id);
                if (ModelState.IsValid && await TryUpdateModelAsync<CardExpirationOption>(beforeUpdated))
                {
                    try
                    {
                        await _db.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch 
                    {
                        CreateSelectList(model.AcademicLevelId, model.FacultyId ?? 0);
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }

                CreateSelectList(model.AcademicLevelId, model.FacultyId ?? 0);
                _flashMessage.Danger(Message.UnableToEdit);
                return View(model);
            }
        }

        [PermissionAuthorize("CardExpirationOption", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            CardExpirationOption model = Find(id);
            try
            {
                _db.CardExpirationOptions.Remove(model);
                _flashMessage.Confirmation(Message.SaveSucceed);
                _db.SaveChanges();
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private CardExpirationOption Find(long? id) 
        {
            var model = _db.CardExpirationOptions.IgnoreQueryFilters()
                                                 .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private bool IsExist(CardExpirationOption updatedModel)
        {
            var model = _db.CardExpirationOptions.SingleOrDefault(x => x.AcademicLevelId == updatedModel.AcademicLevelId 
                                                                       && x.FacultyId == updatedModel.FacultyId 
                                                                       && x.DepartmentId == updatedModel.DepartmentId);
            if (model != null && model.Id == updatedModel.Id)
            {
                return model.ValidityYear == updatedModel.ValidityYear;
            }

            return model != null;
        }

        public ActionResult GetCardExpirationOption(long academicLevelId, long? facultyId, long? departmentId)
        {
            var byAcademicLevels = _db.CardExpirationOptions.Where(x => x.AcademicLevelId == academicLevelId
                                                                        && (x.DepartmentId == null || x.DepartmentId == departmentId)
                                                                        && (x.FacultyId == null || x.FacultyId == facultyId));
            
            if (byAcademicLevels.Any())
            {
                var year = byAcademicLevels.OrderByDescending(x => x.DepartmentId)
                                           .ThenByDescending(x => x.FacultyId)
                                           .FirstOrDefault()
                                           .ValidityYear;
                return Json(year);
            }
            
            return Json(0);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId > 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }
            
            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            }
        }
    }
}