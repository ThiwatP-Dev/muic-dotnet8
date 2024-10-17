using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{

    public class StudentIncidentController : BaseController
    {
        protected readonly IReportProvider _reportProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IStudentProvider _studentProvider;

        public StudentIncidentController(ApplicationDbContext db,
                                         IFlashMessage flashMessage,
                                         IMapper mapper,
                                         ISelectListProvider selectListProvider,
                                         IRegistrationProvider registrationProvider,
                                         IStudentProvider studentProvider,
                                         IReportProvider reportProvider) : base(db, flashMessage, mapper, selectListProvider) 
        {
            _reportProvider = reportProvider;
            _registrationProvider = registrationProvider;
            _studentProvider = studentProvider;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentIncident model)
        {
            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var modelToUpdate = new StudentIncident();
                    var incident = _db.Incidents.SingleOrDefault(x => x.Id == model.IncidentId);
                    var signatoryName = _reportProvider.GetSignatoryNameById(model.SignatoryId);
                    modelToUpdate = _mapper.Map<Incident, StudentIncident>(incident);
                    modelToUpdate.StudentId = model.StudentId;
                    modelToUpdate.Remark = model.Remark;
                    modelToUpdate.TermId = model.TermId;
                    modelToUpdate.ApprovedBy = signatoryName;
                    modelToUpdate.ApprovedAt = model.ApprovedAt;
                    _db.StudentIncidents.Add(modelToUpdate);
                    _db.SaveChanges();

                    var student = _db.Students.IgnoreQueryFilters().SingleOrDefault(x => x.Id == model.StudentId);
                    var studentIncidents = _studentProvider.GetStudentIncidentsByStudentId(model.StudentId);
                    var registrationLock = true;
                    var paymentLock =  true;
                    var signInLock = true;
                    if(studentIncidents.Any())
                    {
                        registrationLock = studentIncidents.Any(x => x.LockedRegistration);
                        paymentLock =  studentIncidents.Any(x => x.LockedPayment);
                        signInLock = studentIncidents.Any(x => x.LockedSignIn);
                    }
                        
                    if(await _registrationProvider.UpdateLockedStudentUspark(student.Code, registrationLock, paymentLock, signInLock))
                    {
                        transaction.Rollback();
                    }
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);
                }
            }

            return RedirectToAction("Details", nameof(Student), new { id = model.StudentId, tabIndex = "5" });
        }

        public ActionResult Edit(long id)
        {
            var model = Find(id);
            return PartialView("~/Views/Student/Incident/_Form.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid && await TryUpdateModelAsync<StudentIncident>(model))
                    {
                        await _db.SaveChangesAsync();
                        var student = _db.Students.IgnoreQueryFilters().SingleOrDefault(x => x.Id == model.StudentId);
                        var studentIncidents = _studentProvider.GetStudentIncidentsByStudentId(model.StudentId);
                        var registrationLock = false;
                        var paymentLock =  false;
                        var signInLock = false;
                        if(studentIncidents.Any())
                        {
                            registrationLock = studentIncidents.Any(x => x.LockedRegistration);
                            paymentLock =  studentIncidents.Any(x => x.LockedPayment);
                            signInLock = studentIncidents.Any(x => x.LockedSignIn);
                        }
                            
                        if(await _registrationProvider.UpdateLockedStudentUspark(student.Code, registrationLock, paymentLock, signInLock))
                        {
                            transaction.Rollback();
                        }
                        else
                        {
                            transaction.Commit();
                            _flashMessage.Confirmation(Message.SaveSucceed);
                        }
                    }
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToEdit);
                }
            }

            return RedirectToAction("Details", nameof(Student), new { id = model.StudentId, tabIndex = "5" });
        }

        public async Task<ActionResult> Delete(long id, string returnUrl)
        {
            StudentIncident model = Find(id);
            if (model == null)
            {
                _flashMessage.Danger(Message.DataNotFound);
                return Redirect(returnUrl);
            }
            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    model.IsActive = false;
                    _db.SaveChanges();

                    var student = _db.Students.IgnoreQueryFilters().SingleOrDefault(x => x.Id == model.StudentId);
                    var studentIncidents = _studentProvider.GetStudentIncidentsByStudentId(model.StudentId);
                    var registrationLock = false;
                    var paymentLock =  false;
                    var signInLock = false;
                    if(studentIncidents.Any())
                    {
                        registrationLock = studentIncidents.Any(x => x.LockedRegistration);
                        paymentLock =  studentIncidents.Any(x => x.LockedPayment);
                        signInLock = studentIncidents.Any(x => x.LockedSignIn);
                    }

                    if(await _registrationProvider.UpdateLockedStudentUspark(student.Code, registrationLock, paymentLock, signInLock))
                    {
                        transaction.Rollback();
                    }
                    else
                    {
                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                    }
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToDelete);
                }
            }
            return RedirectToAction("Details", nameof(Student), new { id = model.StudentId, tabIndex = "5" });
        }

        public async Task<ActionResult> Unlock(long id, string returnUrl)
        {
            StudentIncident model = Find(id);
            if (model == null)
            {
                _flashMessage.Danger(Message.DataNotFound);
                return Redirect(returnUrl);
            }
            var user = GetUser();
            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    StudentIncidentLog log = new StudentIncidentLog
                                            {
                                                StudentId = model.StudentId,
                                                IncidentId = model.IncidentId,
                                                TermId = model.TermId,
                                                LockedDocument = model.LockedDocument,
                                                LockedRegistration = model.LockedRegistration,
                                                LockedPayment = model.LockedPayment,
                                                LockedVisa = model.LockedVisa,
                                                LockedGraduation = model.LockedGraduation,
                                                LockedChangeFaculty = model.LockedChangeFaculty,
                                                LockedSignIn = model.LockedSignIn,
                                                Remark = model.Remark,
                                                LockedBy = model.CreatedBy,
                                                LockedByAt = model.CreatedAt,
                                                UnlockedBy = user.Id,
                                                UnlockedAt = DateTime.UtcNow,
                                                ApprovedAt = model.ApprovedAt,
                                                ApprovedBy = model.ApprovedBy
                                            };

                    _db.StudentIncidentLogs.Add(log);
                    _db.SaveChanges();
                    _db.StudentIncidents.Remove(model);
                    _db.SaveChanges();

                    var student = _db.Students.IgnoreQueryFilters().SingleOrDefault(x => x.Id == model.StudentId);
                    var studentIncidents = _studentProvider.GetStudentIncidentsByStudentId(model.StudentId);
                    var registrationLock = false;
                    var paymentLock =  false;
                    var signInLock = false;
                    if(studentIncidents.Any())
                    {
                        registrationLock = studentIncidents.Any(x => x.LockedRegistration);
                        paymentLock =  studentIncidents.Any(x => x.LockedPayment);
                        signInLock = studentIncidents.Any(x => x.LockedSignIn);
                    }

                    if(await _registrationProvider.UpdateLockedStudentUspark(student.Code, registrationLock, paymentLock, signInLock))
                    {
                        transaction.Rollback();
                    }
                    else
                    {
                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                    }
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToDelete);
                }
            }
            return RedirectToAction("Details", nameof(Student), new { id = model.StudentId, tabIndex = "5" });
        }

        private StudentIncident Find(long? id)
        {
            var studentIncidents = _db.StudentIncidents.Include(x => x.Student)
                                                       .Include(x => x.Incident)
                                                       .SingleOrDefault(x => x.Id == id);
            return studentIncidents;
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.Incidents = _selectListProvider.GetIncidents();
            ViewBag.Signatories = _selectListProvider.GetSignatories();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
        }
    }
}