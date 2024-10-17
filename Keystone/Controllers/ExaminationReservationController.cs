using KeystoneLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.EntityFrameworkCore;
using KeystoneLibrary.Models.DataModels;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("ExaminationReservation", "")]
    public class ExaminationReservationController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly ISectionProvider _sectionProvider;
        protected readonly IReservationProvider _reservationProvider;
        protected readonly ICacheProvider _cacheProvider;

        public ExaminationReservationController(ApplicationDbContext db,
                                                ISelectListProvider selectListProvider,
                                                IFlashMessage flashMessage,
                                                IDateTimeProvider dateTimeProvider,
                                                ISectionProvider sectionProvider,
                                                ICacheProvider cacheProvider,
                                                IReservationProvider reservationProvider) : base(db, flashMessage, selectListProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _sectionProvider = sectionProvider;
            _reservationProvider = reservationProvider;
            _cacheProvider = cacheProvider;
        }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            var user = GetUser();
            if(criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                criteria.AcademicLevelId = _db.AcademicLevels.SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
                criteria.TermId = _cacheProvider.GetCurrentTerm(criteria.AcademicLevelId).Id;
            }
            criteria.Status = string.IsNullOrEmpty(criteria.Status) ? "w" : criteria.Status;
            CreateSelectList(criteria.DateCheck, null, null, criteria.AcademicLevelId, criteria.TermId, criteria.CourseId, "defaultWaiting");
            var examinationRoom = _db.ExaminationReservations.Include(x => x.Term)
                                                             .Include(x => x.Instructor)
                                                             .Include(x => x.ExaminationType)
                                                             .Include(x => x.Room)
                                                             .Include(x => x.Section)
                                                                 .ThenInclude(x => x.Course)
                                                             .Where(x => (criteria.AcademicLevelId == 0
                                                                          || criteria.AcademicLevelId == x.Term.AcademicLevelId)
                                                                          && (criteria.TermId == 0
                                                                              || criteria.TermId == x.TermId)
                                                                          && (criteria.CourseId == 0
                                                                              || criteria.CourseId == x.Section.CourseId)
                                                                          && user.Id == x.CreatedBy
                                                                          && (criteria.ExaminationTypeId == 0
                                                                              || x.ExaminationTypeId == criteria.ExaminationTypeId)
                                                                          && (criteria.Status == "all"
                                                                              || x.Status == criteria.Status)
                                                                          && (criteria.SectionId == 0
                                                                              || criteria.SectionId == x.SectionId))
                                                             .IgnoreQueryFilters()
                                                             .GetPaged(criteria, page);
            return View(examinationRoom);
        }

        public ActionResult Details(long id)
        {    
            var model = _reservationProvider.GetExaminationReservation(id);
            return PartialView("~/Views/ExaminationReservation/_Details.cshtml", model);
        }

        [PermissionAuthorize("ExaminationReservation", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var academicLevelId = _db.AcademicLevels.SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
            var termId = _cacheProvider.GetCurrentTerm(academicLevelId).Id;
            CreateSelectList(null, null, null, academicLevelId, termId);
            return View(new ExaminationReservation
                        {
                            AcademicLevelId = academicLevelId,
                            TermId = termId
                        });
        }

        [PermissionAuthorize("ExaminationReservation", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ExaminationReservation model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            TimeSpan? startedAt = _dateTimeProvider.ConvertStringToTime(model.StartTimeText);
            TimeSpan? endedAt = _dateTimeProvider.ConvertStringToTime(model.EndTimeText);
            var user = GetUser();
            if (model.StartTimeText == null || model.EndTimeText == null || model.AcademicLevelId == 0 || model.CourseId == 0 || model.SectionId == 0
                || startedAt == null || endedAt == null)
            {
                CreateSelectList(model.Date, model.StartTimeText, model.EndTimeText, model.AcademicLevelId, model.TermId, model.CourseId);
                _flashMessage.Danger(Message.RequiredData);
                return View(model);
            }

            if(startedAt > endedAt)
            {
                CreateSelectList(model.Date, model.StartTimeText, model.EndTimeText, model.AcademicLevelId, model.TermId, model.CourseId);
                _flashMessage.Danger(Message.InvalidStartedTime);
                return View(model);
            }
            var section = _db.Sections.SingleOrDefault(x => x.Id == model.SectionId);
            if(section.IsClosed)
            {
                _flashMessage.Danger(Message.SectionIsClose);
                return View(model);
            }
            if(section.IsDisabledFinal && model.ExaminationTypeId == 2)
            {
                _flashMessage.Danger(Message.IsDisabledFinal);
                return View(model);
            }
            else if(section.IsDisabledMidterm && model.ExaminationTypeId == 1)
            {
                _flashMessage.Danger(Message.IsDisabledMidterm);
                return View(model);
            }

            using(var transaction = _db.Database.BeginTransaction())
            {
                model.StartTime = startedAt ?? new TimeSpan();
                model.EndTime = endedAt ?? new TimeSpan();
                string examinationType = _db.ExaminationTypes.SingleOrDefault(x => x.Id == model.ExaminationTypeId).NameEn.ToLower();
                model.Status = examinationType.Contains("midterm") ? "c" : "w";
                model.SenderType = user.InstructorId == null ? "a" : "i";
                var result = _reservationProvider.UpdateExaminationReservation(model);
                if(!string.IsNullOrEmpty(result.Message))
                {
                    _flashMessage.Warning(result.Message);
                }

                switch(result.Status)
                {
                    case UpdateExamStatus.SaveExamSucceed : 
                        var criteria = _db.Sections.Where(x => x.Id == result.ExaminationReservation.SectionId)
                                                   .Select(x => new Criteria
                                                                   {
                                                                       AcademicLevelId = x.Term.AcademicLevelId,
                                                                       TermId = x.TermId,
                                                                       CourseId = x.CourseId,
                                                                       SectionId = x.Id
                                                                   })
                                                   .FirstOrDefault();
                        transaction.Commit();      
                        _flashMessage.Confirmation(Message.SaveSucceed);              
                        return Redirect(returnUrl);
                    case UpdateExamStatus.UpdateExamSuccess : 
                        transaction.Commit(); 
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return Redirect(returnUrl);

                    case UpdateExamStatus.ExaminationAlreadyApproved :  
                        transaction.Rollback();      
                        _flashMessage.Warning(Message.DataAlreadyExist);
                        CreateSelectList(model.Date, model.StartTimeText, model.EndTimeText, model.AcademicLevelId, model.TermId, model.CourseId);
                        return RedirectToAction(nameof(Edit), new { id = result.ExaminationReservation.Id });

                    default : 
                        transaction.Rollback();    
                        _flashMessage.Danger(Message.UnableToCreate);
                        CreateSelectList(model.Date, model.StartTimeText, model.EndTimeText, model.AcademicLevelId, model.TermId, model.CourseId);
                        return View(model);
                }
            }
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            var model = _reservationProvider.GetExaminationReservation(id);
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.Date, model.StartTimeText, model.EndTimeText, model.AcademicLevelId, model.TermId, model.CourseId);
            return View(model);
        }
        
        [HttpGet]
        public long GetInstructorIdBySectionId(long sectionId)
        {
            var instructorId = _db.Sections.IgnoreQueryFilters().SingleOrDefault(x => x.Id == sectionId)?.MainInstructorId ?? 0;
            return instructorId;
        }

        [PermissionAuthorize("ExaminationReservation", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ExaminationReservation model, string returnUrl)
        {
            TimeSpan? startedAt = _dateTimeProvider.ConvertStringToTime(model.StartTimeText);
            TimeSpan? endedAt = _dateTimeProvider.ConvertStringToTime(model.EndTimeText);
            ViewBag.ReturnUrl = returnUrl;
            if (model.StartTimeText == null || model.EndTimeText == null || model.AcademicLevelId == 0 || model.CourseId == 0 || model.SectionId == 0
                || startedAt == null || endedAt == null)
            {
                CreateSelectList(model.Date, model.StartTimeText, model.EndTimeText, model.AcademicLevelId, model.TermId, model.CourseId);
                _flashMessage.Danger(Message.UnableToCreate);
                return View(model);
            }

            if (_reservationProvider.IsExistedExaminationReservation(model.SectionId, model.ExaminationTypeId, model.Id))
            {
                CreateSelectList(model.Date, model.StartTimeText, model.EndTimeText, model.AcademicLevelId, model.TermId, model.CourseId);
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }
            var section = _db.Sections.SingleOrDefault(x => x.Id == model.SectionId);

            if(section.IsClosed)
            {
                _flashMessage.Danger(Message.SectionIsClose);
                return View(model);
            }
            
            if(section.IsDisabledFinal && model.ExaminationTypeId == 2)
            {
                _flashMessage.Danger(Message.IsDisabledFinal);
                return View(model);
            }
            else if(section.IsDisabledMidterm && model.ExaminationTypeId == 1)
            {
                _flashMessage.Danger(Message.IsDisabledMidterm);
                return View(model);
            }
            var examinationReservation = _reservationProvider.GetExaminationReservation(model.Id);
            if (ModelState.IsValid && await TryUpdateModelAsync<ExaminationReservation>(examinationReservation))
            {
                examinationReservation.StartTime = startedAt.Value;
                examinationReservation.EndTime = endedAt.Value;                
                using(var transaction = _db.Database.BeginTransaction())
                {
                    model.StartTime = startedAt ?? new TimeSpan();
                    model.EndTime = endedAt ?? new TimeSpan();
                    var result = _reservationProvider.UpdateExaminationReservation(model);
                    if(!string.IsNullOrEmpty(result.Message))
                    {
                        _flashMessage.Warning(result.Message);
                    }

                    switch(result.Status)
                    {
                        case UpdateExamStatus.SaveExamSucceed : 
                            var criteria = _db.Sections.Where(x => x.Id == result.ExaminationReservation.SectionId)
                                                       .Select(x => new Criteria
                                                                       {
                                                                           AcademicLevelId = x.Term.AcademicLevelId,
                                                                           TermId = x.TermId,
                                                                           CourseId = x.CourseId,
                                                                           SectionId = x.Id
                                                                       })
                                                       .FirstOrDefault();
                            transaction.Commit();      
                            _flashMessage.Confirmation(Message.SaveSucceed);              
                            return Redirect(returnUrl);
                        case UpdateExamStatus.UpdateExamSuccess : 
                            transaction.Commit(); 
                            _flashMessage.Confirmation(Message.SaveSucceed);
                            return Redirect(returnUrl);

                        case UpdateExamStatus.ExaminationAlreadyApproved :  
                            transaction.Rollback();      
                            _flashMessage.Warning(Message.DataAlreadyExist);
                            CreateSelectList(model.Date, model.StartTimeText, model.EndTimeText, model.AcademicLevelId, model.TermId, model.CourseId);
                            return RedirectToAction(nameof(Edit), new { id = result.ExaminationReservation.Id });

                        default : 
                            transaction.Rollback();    
                            _flashMessage.Danger(Message.UnableToCreate);
                            CreateSelectList(model.Date, model.StartTimeText, model.EndTimeText, model.AcademicLevelId, model.TermId, model.CourseId);
                            return View(model);
                    }
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.Date, model.StartTimeText, model.EndTimeText, model.AcademicLevelId, model.TermId, model.CourseId);
            return View(model);
        }

        [PermissionAuthorize("ExaminationReservation", PolicyGenerator.Write)]
        public ActionResult Delete(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = _reservationProvider.GetExaminationReservation(id);
            try
            {
                // Update Section       
                var section = _db.Sections.SingleOrDefault(x => x.Id == model.SectionId);
                string examinationType = _db.ExaminationTypes.SingleOrDefault(x => x.Id == model.ExaminationTypeId).Abbreviation;
                if (examinationType == "MT")
                {
                    section.MidtermDate = null;
                    section.MidtermStart = null;
                    section.MidtermEnd = null;
                    section.MidtermRoomId = null;
                }
                else
                {
                    section.FinalDate = null;
                    section.FinalStart = null;
                    section.FinalEnd = null;
                    section.FinalRoomId = null;
                }
                _db.ExaminationReservations.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return Redirect(returnUrl);
        }

        private void CreateSelectList(DateTime? date = null, string start = null, string end = null, long academicLevelId = 0, long termId = 0, long courseId = 0, string currentStatus = "")
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.ExaminationTypes = _selectListProvider.GetExaminationTypes();
            ViewBag.Statuses = _selectListProvider.GetYesNoAnswer();
            ViewBag.Instructors = _selectListProvider.GetInstructors();
            ViewBag.ReservationStatuses = _selectListProvider.GetReservationStatuses(currentStatus);
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                if (termId != 0)
                {
                    ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);
                    if (courseId != 0)
                    {
                        ViewBag.Sections = _selectListProvider.GetSections(termId, courseId);
                        // wait data // ViewBag.Instructors = _selectListProvider.GetTermInstructorsByCourseId(termId, courseId);
                    }
                }
            }

            TimeSpan? startedAt = _dateTimeProvider.ConvertStringToTime(start);
            TimeSpan? endedAt = _dateTimeProvider.ConvertStringToTime(end);
            if (date != null && startedAt != null && endedAt != null)
            {
                ViewBag.Rooms = _selectListProvider.GetAvailableRoom(date.Value, startedAt.Value, endedAt.Value, "i");
            }
        }

        // private 
    }
}