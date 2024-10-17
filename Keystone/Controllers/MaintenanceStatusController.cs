using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Enumeration;
using KeystoneLibrary.Helpers;

namespace Keystone.Controllers
{

    public class MaintenanceStatusController : BaseController 
    {
        protected readonly IStudentProvider _studentProvider;
        public MaintenanceStatusController(ApplicationDbContext db,
                                           IFlashMessage flashMessage,
                                           ISelectListProvider selectListProvider,
                                           IStudentProvider studentProvider) : base(db, flashMessage, selectListProvider) 
        {
            _studentProvider = studentProvider;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MaintenanceStatus model)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var student = _db.Students.IgnoreQueryFilters()
                                              .SingleOrDefault(x => x.Id == model.StudentId);
                    if (student != null)
                    {
                        student.StudentStatus = "m";
                        student.IsActive = KeystoneLibrary.Providers.StudentProvider.IsActiveFromStudentStatus(student.StudentStatus);
                    }

                    var success = _studentProvider.SaveStudentStatusLog(model.StudentId
                                                                        , model.TermId
                                                                        , SaveStatusSouces.MAINTAIN.GetDisplayName()
                                                                        , "Maintain"
                                                                        , "m");
                    if (!success)
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.UnableToCreate);
                    }
                                            
                    _db.MaintenanceStatuses.Add(model);
                    _db.SaveChanges();

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);
                } 
            }
            
            
            return RedirectToAction("Details", nameof(Student), new { id = model.StudentId, tabIndex = "8" });
        }

        public ActionResult Edit(long id)
        {
            var model = Find(id);
            CreateSelectList(model);
            return PartialView("~/Views/Student/Maintenance/_Form.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            try
            {
                if (ModelState.IsValid && await TryUpdateModelAsync<MaintenanceStatus>(model))
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
            }

            return RedirectToAction("Details", nameof(Student), new { id = model.StudentId, tabIndex = "8" });
        }

        private MaintenanceStatus Find(long? id) 
        {
            var maintenanceStatus = _db.MaintenanceStatuses.Include(x => x.Student)
                                                               .ThenInclude(x => x.AcademicInformation)
                                                           .FirstOrDefault(x => x.Id == id);
            return maintenanceStatus;
        }

        private void CreateSelectList(MaintenanceStatus model) 
        {
            ViewBag.Signatories = _selectListProvider.GetSignatories();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(model.Student?.AcademicInformation?.AcademicLevelId ?? 0);
            ViewBag.MaintenanceFees = _selectListProvider.GetMaintenanceFees(model.Student?.AcademicInformation?.FacultyId,
                                                                             model.Student?.AcademicInformation?.DepartmentId, 
                                                                             model.Student?.AcademicInformation?.AcademicLevelId,
                                                                             model.Student?.AcademicInformation?.StudentGroupId);
        }
    }
}