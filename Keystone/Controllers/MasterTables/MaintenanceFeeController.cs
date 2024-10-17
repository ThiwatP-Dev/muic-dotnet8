using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    public class MaintenanceFeeController : BaseController
    {
        public MaintenanceFeeController(ApplicationDbContext db,
                                        IFlashMessage flashMessage,
                                        ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index()
        {
            var models = _db.MaintenanceFees.Include(x => x.Faculty)
                                            .Include(x => x.Department)
                                            .Include(x => x.Term)
                                            .Include(x => x.StudentGroup)
                                            .Include(x => x.AcademicLevel)
                                            .IgnoreQueryFilters()
                                            .ToList();
            return View(models);
        }

        public ActionResult Create()
        {
            CreateSelectList();
            return View();
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MaintenanceFee model)
        {
            if (ModelState.IsValid)
            {
                _db.MaintenanceFees.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            MaintenanceFee model = Find(id);
            CreateSelectList(model.AcademicLevelId, model.FacultyId ?? 0);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<MaintenanceFee>(model))
                {
                    try
                    {
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index));
                    }
                    catch 
                    { 
                        _flashMessage.Danger(Message.UnableToEdit);
                    }
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        public ActionResult Delete(long id)
        {
            MaintenanceFee model = Find(id);
            try
            {
                _db.MaintenanceFees.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private MaintenanceFee Find(long? id) 
        {
            var model = _db.MaintenanceFees.IgnoreQueryFilters()
                                           .SingleOrDefault(x => x.Id == id);
            return model; 
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            ViewBag.StudentGroups = _selectListProvider.GetStudentGroups();
        }
    }
}