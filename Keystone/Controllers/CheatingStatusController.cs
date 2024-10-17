using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class CheatingStatusController : BaseController
    {
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IStudentProvider _studentProvider;
        protected readonly IAcademicProvider _academicProvider;
        private UserManager<ApplicationUser> _userManager { get; }

        public CheatingStatusController(ApplicationDbContext db,
                                        IFlashMessage flashMessage,
                                        ISelectListProvider selectListProvider,
                                        ICacheProvider cacheProvider,
                                        IStudentProvider studentProvider,
                                        IRegistrationProvider registrationProvider,
                                        IAcademicProvider academicProvider,
                                        UserManager<ApplicationUser> user) : base(db, flashMessage, selectListProvider)
        {
            _cacheProvider = cacheProvider;
            _registrationProvider = registrationProvider;
            _studentProvider = studentProvider;
            _academicProvider = academicProvider;
            _userManager = user;
        }

        public ActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(null, criteria.AcademicLevelId, criteria.TermId);

            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var model = from cheating in _db.CheatingStatuses
                        join student in _db.Students on cheating.StudentId equals student.Id
                        join academicInfo in _db.AcademicInformations on student.Id equals academicInfo.StudentId
                        join examinationType in _db.ExaminationTypes on cheating.ExaminationTypeId equals examinationType.Id
                        join incident in _db.Incidents on cheating.IncidentId equals incident.Id into incidents
                        from incident in incidents.DefaultIfEmpty()
                        join registration in _db.RegistrationCourses on cheating.RegistrationCourseId equals registration.Id
                        join course in _db.Courses on registration.CourseId equals course.Id
                        where academicInfo.AcademicLevelId == criteria.AcademicLevelId
                              && cheating.TermId == criteria.TermId
                              && (criteria.ExaminationTypeId == 0 || cheating.ExaminationTypeId == criteria.ExaminationTypeId)
                              && (criteria.IncidentId == 0 || cheating.IncidentId == criteria.IncidentId)
                              && (string.IsNullOrEmpty(criteria.Code) || cheating.Student.Code.Contains(criteria.Code))
                              && (string.IsNullOrEmpty(criteria.CodeAndName) || course.Code.StartsWith(criteria.CodeAndName))
                        select new CheatingStatusViewModel
                               {
                                   Id = cheating.Id,
                                   AcademicLevelId = academicInfo.AcademicLevelId,
                                   TermId = cheating.TermId,
                                   StudentCode = student.Code,
                                   StudentFullName = student.FullNameEn,
                                   ExaminationType = examinationType.NameEn,
                                   PunishType = cheating.IncidentId == null ? string.Empty : incident.NameEn
                               };
        
            return View(model.GetPaged(criteria, page));
        }

        public ActionResult Details(long id, string page, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = _studentProvider.GetCheatingStatus(id);
            if (page == "cheating")
            {
                return View(model);
            }

            return PartialView("~/Views/Student/Cheating/_Details.cshtml", model);
        }

        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(null);
            return View(new CheatingStatus());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CheatingStatus model, string page, string returnUrl)
        {
            model.StudentId = page == "cheating" ? _studentProvider.GetStudentByCode(model.StudentCode)?.Id ?? Guid.Empty : model.StudentId;
            ViewBag.ReturnUrl = returnUrl;
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.CheatingStatuses.Add(model);
                    _db.SaveChanges();

                    if (model.IncidentId != null && model.IncidentId != 0)
                    {
                        _studentProvider.SaveStudentIncident(model);
                    }

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    if (page == "cheating")
                    {
                        var academicLevelId = _academicProvider.GetTerm(model.TermId)?.AcademicLevelId ?? 0;
                        return RedirectToAction(nameof(Index), new { AcademicLevelId = academicLevelId, TermId = model.TermId, Code = model.StudentCode });
                    }
                }
                catch 
                {
                    transaction.Rollback();
                    CreateSelectList(model.Student.Id, model.Student.AcademicInformation.AcademicLevelId, model.TermId);
                    _flashMessage.Danger(Message.UnableToCreate);
                    if (page == "cheating")
                    {
                        return View(model);
                    }
                }
            }

            return RedirectToAction(nameof(Details), "Student", new { id = model.StudentId, tabIndex = "10" });
        }

        public ActionResult Edit(long id, string page, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = _studentProvider.GetCheatingStatus(id);
            CreateSelectList(model.Student.Id, model.Student.AcademicInformation.AcademicLevelId, model.TermId);
            if (page == "cheating")
            {
                model.StudentCode = model.Student.Code;
                return View(model);
            }

            return PartialView("~/Views/Student/Cheating/_Form.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CheatingStatus model, string page, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        var userId = _userManager.GetUserId(User);
                        var cheatingStatus = _db.CheatingStatuses.SingleOrDefault(x => x.Id == model.Id);
                        _studentProvider.UpdateStudentIncident(cheatingStatus, model);
                        
                        cheatingStatus.RegistrationCourseId = model.RegistrationCourseId;
                        cheatingStatus.TermId = model.TermId;
                        cheatingStatus.ExaminationTypeId = model.ExaminationTypeId;
                        cheatingStatus.IncidentId = model.IncidentId;
                        cheatingStatus.FromTermId = model.FromTermId;
                        cheatingStatus.ToTermId = model.ToTermId;
                        cheatingStatus.Detail = model.Detail;
                        cheatingStatus.IsActive = model.IsActive;
                        cheatingStatus.UpdatedAt = DateTime.UtcNow;
                        cheatingStatus.UpdatedBy = userId;

                        _db.SaveChanges();

                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        if (page == "cheating")
                        {
                            var academicLevelId = _academicProvider.GetTerm(model.TermId)?.AcademicLevelId ?? 0;
                            return RedirectToAction(nameof(Index), new { AcademicLevelId = academicLevelId, TermId = model.TermId, Code = model.StudentCode });
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        CreateSelectList(model.Student.Id, model.Student.AcademicInformation.AcademicLevelId, model.TermId);
                        _flashMessage.Danger(Message.UnableToEdit);
                        if (page == "cheating")
                        {
                            return View(model);
                        }
                        
                        return RedirectToAction(nameof(Details), "Student", new { id = model.StudentId, tabIndex = "10" });
                    }
                }
            }

            CreateSelectList(model.Student.Id, model.Student.AcademicInformation.AcademicLevelId, model.TermId);
            _flashMessage.Danger(Message.UnableToEdit);
            if (page == "cheating")
            {
                return View(model);
            }

            return RedirectToAction(nameof(Details), "Student", new { id = model.StudentId, tabIndex = "10" });
        }

        public ActionResult Delete(long id, string page, string returnUrl)
        {
            var model = _studentProvider.GetCheatingStatus(id);
            if (model == null)
            {
                _flashMessage.Danger(Message.UnableToDelete);
                if (page == "cheating")
                {
                    return RedirectToAction(nameof(Index));
                }
                    
                return RedirectToAction(nameof(Details), "Student");
            }

            _db.CheatingStatuses.Remove(model);
            _db.SaveChanges();
            _flashMessage.Confirmation(Message.SaveSucceed);
            if (page == "cheating")
            {
                var academicLevelId = _academicProvider.GetTerm(model.TermId)?.AcademicLevelId ?? 0;
                return RedirectToAction(nameof(Index), new { AcademicLevelId = academicLevelId, TermId = model.TermId });
            }
            
            return RedirectToAction(nameof(Details), "Student", new { id = model.StudentId, tabIndex = "10" });
        }

        private void CreateSelectList(Guid? studentId, long academicLevelId = 0, long termId = 0) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Course = _selectListProvider.GetCoursesByStudentAndTermForCheating(studentId, termId);
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.ExamType = _selectListProvider.GetExaminationTypes();
            ViewBag.Incidents = _selectListProvider.GetIncidents();
            ViewBag.Signatories = _selectListProvider.GetSignatorieNames();
            ViewBag.AllYesNoAnswer = _selectListProvider.GetAllYesNoAnswer();
            ViewBag.Instructors = _selectListProvider.GetInstructors();
        }
    }
}