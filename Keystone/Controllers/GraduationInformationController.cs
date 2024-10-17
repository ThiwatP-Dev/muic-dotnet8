using KeystoneLibrary.Data;
using KeystoneLibrary.Enumeration;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Helpers;

namespace Keystone.Controllers
{
    public class GraduationInformationController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly ICacheProvider _cacheProvider;
        
        public GraduationInformationController(ApplicationDbContext db,
                                               IFlashMessage flashMessage,
                                               IStudentProvider studentProvider,
                                               ICacheProvider cacheProvider,
                                               ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) 
        { 
            _studentProvider = studentProvider;
            _cacheProvider = cacheProvider;
        }

        public IActionResult Edit(long id)
        {
            var model = _studentProvider.GetStudentGraduationInformation(id);
            ViewBag.Curriculums = _selectListProvider.GetCurriculumByCurriculumInformation(model.StudentId);
            ViewBag.AcademicHonors = _selectListProvider.GetAcademicHonors();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(model.Student.AcademicInformation.AcademicLevelId);
            return View("~/Views/Student/GraduationInformation/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GraduationInformation model)
        {
            var isActive = _studentProvider.IsActiveStudentGraduationInformation(model);
            if (model.IsActive && isActive)
            {
               _flashMessage.Danger(Message.UnableToCreate);
               return RedirectToAction("Details", "Student", new { id = model.StudentId, tabIndex = "4" });
            }

            try
            {
                if(model.IsActive == false)
                {
                    var student = _studentProvider.GetStudentById(model.StudentId);
                    var currentTermId = _cacheProvider.GetCurrentTerm(student.AcademicInformation.AcademicLevelId).Id;
                    var studentLog = _db.StudentStatusLogs.Where(x => x.StudentId == model.StudentId  && !x.StudentStatus.Contains("g")).OrderBy(x => x.EffectiveAt).LastOrDefault();
                    if(studentLog == null)
                    {
                        student.StudentStatus = "s";
                        student.IsActive = KeystoneLibrary.Providers.StudentProvider.IsActiveFromStudentStatus(student.StudentStatus);
                        var success = _studentProvider.SaveStudentStatusLog(model.StudentId
                                                                            , currentTermId
                                                                            , SaveStatusSouces.CHANGESTUDENTSTATUS.GetDisplayName()
                                                                            , "Update Graduation Information inactive"
                                                                            , "s"
                                                                            , DateTime.UtcNow);
                    }
                    else
                    {
                        student.StudentStatus = studentLog.StudentStatus;
                        student.IsActive = KeystoneLibrary.Providers.StudentProvider.IsActiveFromStudentStatus(student.StudentStatus);
                        var success = _studentProvider.SaveStudentStatusLog(model.StudentId
                                                                            , currentTermId
                                                                            , SaveStatusSouces.CHANGESTUDENTSTATUS.GetDisplayName()
                                                                            , "Update Graduation Information inactive"
                                                                            , studentLog.StudentStatus
                                                                            , DateTime.UtcNow);
                    }
                }
                _db.Entry(model).State = EntityState.Modified;
                _db.Entry(model).Property(x => x.CreatedBy).IsModified = false;
                _db.Entry(model).Property(x => x.CreatedAt).IsModified = false;
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
            }

            return RedirectToAction("Details", "Student", new { id = model.StudentId, tabIndex = "4" });
        }

        public IActionResult Delete(long id)
        {
            var model = _studentProvider.GetStudentGraduationInformation(id);
            try
            {
                _db.GraduationInformations.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction("Details", "Student", new { id = model.StudentId, tabIndex = "4" });
        }
    }
}