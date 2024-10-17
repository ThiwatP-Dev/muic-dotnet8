using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Data;
using Vereyon.Web;
using Microsoft.AspNetCore.Mvc;
using KeystoneLibrary.Models.DataModels;
using Newtonsoft.Json;
using KeystoneLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("SectionSlot", "")]
    public class SectionSlotController : BaseController
    {
        protected readonly ISectionProvider _sectionProvider;
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IRoomProvider _roomProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly ICacheProvider _cacheProvider;

        public SectionSlotController(ApplicationDbContext db,
                                     IFlashMessage flashMessage,
                                     ISelectListProvider selectListProvider,
                                     IAcademicProvider academicProvider,
                                     IRoomProvider roomProvider,
                                     ISectionProvider sectionProvider,
                                     ICacheProvider cacheProvider,
                                     IDateTimeProvider dateTimeProvider) : base(db, flashMessage, selectListProvider) 
        {
            _sectionProvider = sectionProvider;
            _academicProvider = academicProvider;
            _roomProvider = roomProvider;
            _dateTimeProvider = dateTimeProvider;
            _cacheProvider = cacheProvider;


        }

        public ActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.CourseId);

            var sectionNumberFilter = criteria?.SectionNumber?.Trim() ?? "";
            var codeAndNameFilter = criteria?.CodeAndName?.Trim() ?? "";
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0 || string.IsNullOrEmpty(codeAndNameFilter) || string.IsNullOrEmpty(sectionNumberFilter))
            {
                criteria.AcademicLevelId = _db.AcademicLevels.SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
                criteria.TermId = _cacheProvider.GetCurrentTerm(criteria.AcademicLevelId).Id;
                CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.CourseId);
                _flashMessage.Warning(Message.RequiredData);
                return View(new SectionSlotViewModel
                            {
                                Criteria = criteria
                            });
            }


            var section = _db.Sections.AsNoTracking()
                                      .Where(x => x.TermId == criteria.TermId
                                                      && (x.Course.Code.Contains(codeAndNameFilter)
                                                          || x.Course.NameEn.Contains(codeAndNameFilter))
                                                      && x.Number.Equals(sectionNumberFilter))
                                      .FirstOrDefault();

            var model = new List<SectionSlotDetailViewModel>();
            model = _db.SectionSlots.Where(x => x.Section.TermId == criteria.TermId
                                                && (x.Section.Course.Code.Contains(codeAndNameFilter)
                                                    || x.Section.Course.NameEn.Contains(codeAndNameFilter))
                                                && x.Section.Number.Equals(sectionNumberFilter))
                                    .Select(x => new SectionSlotDetailViewModel
                                                 {
                                                     Id = x.Id,
                                                     SectionId = x.SectionId,
                                                     Number = x.Section.Number,
                                                     TotalWeeks = x.Section.TotalWeeks,
                                                     Date = x.Date,
                                                     StartTime = x.StartTime,
                                                     EndTime = x.EndTime,
                                                     InstructorId = x.InstructorId,
                                                     InstructorCode = x.Instructor.Code,
                                                     Title = x.Instructor.Title.NameEn,
                                                     FirstNameEn = x.Instructor.FirstNameEn,
                                                     LastNameEn = x.Instructor.LastNameEn,
                                                     Room = x.Room.NameEn,
                                                     CourseCode =  x.Section.Course.Code,
                                                     CourseCredit =  x.Section.Course.Credit,
                                                     CourseLecture =  x.Section.Course.Lecture,
                                                     CourseOther =  x.Section.Course.Other,
                                                     CourseLab =  x.Section.Course.Lab,
                                                     CourseRateId = x.Section.Course.CourseRateId,
                                                     CourseName =  x.Section.Course.NameEn,
                                                     Status = x.Status,
                                                     IsMakeUpClass = x.IsMakeUpClass,
                                                     IsExam = false,
                                                     Remark = x.Remark,
                                                     TeachingTypeId = x.TeachingTypeId,
                                                     TeachingType = x.TeachingType.NameEn,
                                                     IsActive = x.IsActive
                                                 })
                                    .IgnoreQueryFilters()
                                    .ToList();
                                    
            var slotExam = _db.ExaminationReservations.Where(x => x.Section.TermId == criteria.TermId
                                                                  && ((x.ExaminationTypeId == 1 && !x.Section.IsDisabledMidterm)
                                                                      || (x.ExaminationTypeId == 2 && !x.Section.IsDisabledFinal))
                                                                  && (x.Section.Course.Code.Contains(codeAndNameFilter)
                                                                      || x.Section.Course.NameEn.Contains(codeAndNameFilter))
                                                                  && x.Section.Number.Equals(sectionNumberFilter))
                                                      .Select(x => new SectionSlotDetailViewModel
                                                                   {
                                                                       Id = x.Id,
                                                                       SectionId = x.SectionId,
                                                                       Number = x.Section.Number,
                                                                       TotalWeeks = x.Section.TotalWeeks,
                                                                       Date = x.Date,
                                                                       StartTime = x.StartTime,
                                                                       EndTime = x.EndTime,
                                                                       InstructorId = x.InstructorId,
                                                                       InstructorCode = x.Instructor.Code,
                                                                       Title = x.Instructor.Title.NameEn,
                                                                       FirstNameEn = x.Instructor.FirstNameEn,
                                                                       LastNameEn = x.Instructor.LastNameEn,
                                                                       Room = x.Room.NameEn,
                                                                       CourseCode =  x.Section.Course.Code,
                                                                       CourseCredit =  x.Section.Course.Credit,
                                                                       CourseLecture =  x.Section.Course.Lecture,
                                                                       CourseOther =  x.Section.Course.Other,
                                                                       CourseLab =  x.Section.Course.Lab,
                                                                       CourseRateId = x.Section.Course.CourseRateId,
                                                                       CourseName =  x.Section.Course.NameEn,
                                                                       Status = x.Status,
                                                                       IsExam = true,
                                                                       Remark = x.StudentRemark,
                                                                       TeachingType = x.ExaminationType.NameEn,
                                                                       ExaminationTypeId = x.ExaminationTypeId,
                                                                       EaminationReservationStatus = x.Status,
                                                                       IsActive = x.IsActive
                                                                   })
                                                      .IgnoreQueryFilters()
                                                      .ToList();

            if(slotExam != null && slotExam.Any())
            {
                model.AddRange(slotExam);
            }
            var result = new SectionSlotViewModel
                         {
                             Criteria = criteria,
                             SectionSlots = model.Where(x => x.IsActive)
                                                    .OrderBy(x => x.Date)
                                                    .ThenBy(x => x.StartTime)
                                                    .ThenBy(x => x.EndTime)
                                                 .ToList(),
                             SectionId = section != null ? section.Id : 0,
            };
            return View(result);
        }

        public ActionResult Create(long id, int totalWeeks, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            var sectionSlots = GenerateSectionSlots(id, totalWeeks);
            return View(sectionSlots);
        }

        public ActionResult CreateModal(long sectionId) 
        {
            CreateSelectList(0, 0, 0, DateTime.UtcNow.Date, "00:00", "00:00");
            var sectionSlot = new SectionSlot
                              {
                                  SectionId = sectionId,
                                  Date = DateTime.Now
                              };

            return PartialView("_FormModal", sectionSlot);
        }

        [PermissionAuthorize("SectionSlot", PolicyGenerator.Write)]
        [HttpPost]
        public ActionResult CreateSectionDetail(SectionSlot model, string returnUrl)
        {
            var section = _sectionProvider.GetSection(model.SectionId);
            var existedRoomSlot = _roomProvider.IsExistedRoomSlot(model.RoomId ?? 0, model.Date, model.StartTime, model.EndTime);
            var errorMessage = "";
            var dateNow = DateTime.UtcNow.AddHours(7).Date;
            if (model.StartTime >= model.EndTime)
            {
                _flashMessage.Danger(Message.InvalidStartedTime);
                return RedirectToAction(nameof(Index), new
                                                       {
                                                           AcademicLevelId = section.Term.AcademicLevelId,
                                                           TermId = section.TermId,
                                                           CodeAndName = section.Course.Code,
                                                           SectionNumber = section.Number
                                                       });
            }

            var holidays = _db.MuicHolidays.AsNoTracking()
                                           .Where(x => x.IsActive
                                                            && x.StartedAt <= model.Date
                                                            && x.EndedAt >= model.Date)
                                           .ToList();
            if (!model.IsMakeUpClass && holidays.Any())
            {
                _flashMessage.Danger(Message.ReservedDate);
                return RedirectToAction(nameof(Index), new
                {
                    AcademicLevelId = section.Term.AcademicLevelId,
                    TermId = section.TermId,
                    CodeAndName = section.Course.Code,
                    SectionNumber = section.Number
                });
            }
            if (model.IsMakeUpClass 
                    && (_db.ReservationCalendars.Any(x => x.StartedAt <= dateNow
                                                                         && x.EndedAt >= dateNow
                                                                         && x.IsActive)
                            || holidays.Any(x => !x.IsMakeUpAble))
                    )
            {
                _flashMessage.Danger(Message.ReservedDate);
                return RedirectToAction(nameof(Index), new
                {
                    AcademicLevelId = section.Term.AcademicLevelId,
                    TermId = section.TermId,
                    CodeAndName = section.Course.Code,
                    SectionNumber = section.Number
                });
            }


            if (existedRoomSlot != null)
            {
                if(existedRoomSlot.SectionSlotId != null || existedRoomSlot.SectionSlotId != 0)
                {
                    var sectionSlot = _db.SectionSlots.Include(x => x.Section)
                                                          .ThenInclude(x => x.Course)
                                                      .Where(x => x.Id == existedRoomSlot.SectionSlotId)
                                                      .Select(x => new 
                                                                   {
                                                                       SectionNumber = x.Section.Number,
                                                                       CourseCodeAndCredit = x.Section.Course.CodeAndCredit
                                                                   })
                                                      .First();
                    errorMessage += $"{ sectionSlot.CourseCodeAndCredit } Section Number: { sectionSlot.SectionNumber } { existedRoomSlot.UsingTypeText } { existedRoomSlot.Room?.NameEn ?? "" } { existedRoomSlot.DateAndTime }";
                }
                else
                {
                    errorMessage += $"{ existedRoomSlot.UsingTypeText } { existedRoomSlot.Room?.NameEn ?? "" } { existedRoomSlot.DateAndTime }";
                }
                _flashMessage.Warning("Duplicate Room " + errorMessage);
                return RedirectToAction(nameof(Index), new
                                                       {
                                                           AcademicLevelId = section.Term.AcademicLevelId,
                                                           TermId = section.TermId,
                                                           CodeAndName = section.Course.Code,
                                                           SectionNumber = section.Number
                                                       });
            }

            try
            {
                model.Day = (int)model.Date.DayOfWeek;
                _db.SectionSlots.Add(model);
                _roomProvider.CreateRoomSlotBySectionSlot(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
            }

            return RedirectToAction(nameof(Index), new
                                                   {
                                                       AcademicLevelId = section.Term.AcademicLevelId,
                                                       TermId = section.TermId,
                                                       CodeAndName = section.Course.Code,
                                                       SectionNumber = section.Number
                                                   });
        }

        [PermissionAuthorize("SectionSlot", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Section section)
        {
            var term = _academicProvider.GetTerm(section.TermId);
            section.AcademicLevelId = term.AcademicLevelId;
            var course = _db.Courses.SingleOrDefault(x => x.Id == section.CourseId);
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    section.SectionSlots.ForEach(x => 
                    {
                        x.SectionId = section.Id;
                    });
                    
                    _db.SectionSlots.AddRange(section.SectionSlots);
                    _db.SaveChanges();

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction("Index", "CourseToBeOffered", new 
                                                                          {
                                                                              CodeAndName = course.Code,
                                                                              AcademicLevelId = section.AcademicLevelId, 
                                                                              TermId = section.TermId 
                                                                          });
                }
                catch
                {
                    transaction.Rollback();
                    CreateSelectList();
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(section);
                }
            }
        }

        public ActionResult Edit(long id, int totalWeeks, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            Section section = null;
            if (totalWeeks == 0)
            {
                section = Find(id);
            }
            else
            {
                section = GenerateSectionSlots(id, totalWeeks);
            }

            return View(section);
        }

        public ActionResult EditModal(long id) 
        {
            CreateSelectList();
            ViewBag.Rooms = _selectListProvider.GetRooms();
            var sectionSlot = _db.SectionSlots.SingleOrDefault(x => x.Id == id);
            return PartialView("_FormModal", sectionSlot);
        }

        [PermissionAuthorize("SectionSlot", PolicyGenerator.Write)]
        [HttpPost]
        public async Task<ActionResult> EditSectionDetail(long id)
        {
            var model = _db.SectionSlots.SingleOrDefault(x => x.Id == id);
            var section = _sectionProvider.GetSection(model.SectionId);
            var errorMessage = "";
            if (ModelState.IsValid && await TryUpdateModelAsync<SectionSlot>(model))
            {
                var existedRoomSlot = _roomProvider.IsExistedRoomSlot(model.RoomId ?? 0, model.Date, model.StartTime, model.EndTime);
                if (model.StartTime >= model.EndTime)
                {
                    _flashMessage.Danger(Message.InvalidStartedTime);
                    return RedirectToAction(nameof(Index), new
                                                           {
                                                               AcademicLevelId = section.Term.AcademicLevelId,
                                                               TermId = section.TermId,
                                                               CodeAndName = section.Course.Code,
                                                               SectionNumber = section.Number
                                                           });
                }

                var holidays = _db.MuicHolidays.AsNoTracking()
                                          .Where(x => x.IsActive
                                                           && x.StartedAt <= model.Date
                                                           && x.EndedAt >= model.Date)
                                          .ToList();
                if (!model.IsMakeUpClass && holidays.Any())
                {
                    _flashMessage.Danger(Message.ReservedDate);
                    return RedirectToAction(nameof(Index), new
                    {
                        AcademicLevelId = section.Term.AcademicLevelId,
                        TermId = section.TermId,
                        CodeAndName = section.Course.Code,
                        SectionNumber = section.Number
                    });
                }
                var dateNow = DateTime.UtcNow.AddHours(7).Date;
                if (model.IsMakeUpClass && _db.ReservationCalendars.Any(x => x.StartedAt <= dateNow
                                                                         && x.EndedAt >= dateNow
                                                                         && x.IsActive)
                                        && holidays.Any(x =>!x.IsMakeUpAble)
                    )
                {
                    _flashMessage.Danger(Message.ReservedDate);
                    return RedirectToAction(nameof(Index), new
                    {
                        AcademicLevelId = section.Term.AcademicLevelId,
                        TermId = section.TermId,
                        CodeAndName = section.Course.Code,
                        SectionNumber = section.Number
                    });
                }

                if (existedRoomSlot != null && existedRoomSlot?.SectionSlotId != model.Id && model.Status != "c")
                {
                    if(existedRoomSlot.SectionSlotId != null || existedRoomSlot.SectionSlotId != 0)
                    {
                        var sectionSlot = _db.SectionSlots.Include(x => x.Section)
                                                              .ThenInclude(x => x.Course)
                                                          .Where(x => x.Id == existedRoomSlot.SectionSlotId)
                                                          .Select(x => new 
                                                                       {
                                                                           SectionNumber = x.Section.Number,
                                                                           CourseCodeAndCredit = x.Section.Course.CodeAndCredit
                                                                       })
                                                          .First();
                        errorMessage += $"{ sectionSlot.CourseCodeAndCredit } Section Number: { sectionSlot.SectionNumber } { existedRoomSlot.UsingTypeText } { existedRoomSlot.Room?.NameEn ?? "" } { existedRoomSlot.DateAndTime }";
                    }
                    else
                    {
                        errorMessage += $"{ existedRoomSlot.UsingTypeText } { existedRoomSlot.Room?.NameEn ?? "" } { existedRoomSlot.DateAndTime }";
                    }

                    _flashMessage.Warning("Duplicate Room " + errorMessage);
                    return RedirectToAction(nameof(Index), new
                                                           {
                                                               AcademicLevelId = section.Term.AcademicLevelId,
                                                               TermId = section.TermId,
                                                               CodeAndName = section.Course.Code,
                                                               SectionNumber = section.Number
                                                           });
                }



                try
                {
                    model.Day = (int)model.Date.DayOfWeek;
                    await _db.SaveChangesAsync();
                    _roomProvider.CancelRoomSlotBySectionSlotId(model.Id);
                    if(model.Status != "c")
                    {
                        _roomProvider.CreateRoomSlotBySectionSlot(model);
                    }
                    await _db.SaveChangesAsync();

                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch 
                { 
                    _flashMessage.Danger(Message.UnableToEdit);
                }  
            }

            return RedirectToAction(nameof(Index), new
                                                   {
                                                       AcademicLevelId = section.Term.AcademicLevelId,
                                                       TermId = section.TermId,
                                                       CodeAndName = section.Course.Code,
                                                       SectionNumber = section.Number
                                                   });
        }

        [PermissionAuthorize("SectionSlot", PolicyGenerator.Write)]
        [HttpPost]
        public ActionResult BatchEdit(Criteria criteria, List<SectionSlotDetailViewModel> sectionSlots, long? teachingTypeId, long? instructorId, string status, bool? isMakeUpClass)
        {
            var updateList = sectionSlots?.Where(x => x.IsChecked == "on" && !x.IsExam)
                                          .ToList();      
            if (updateList.Any())
            {
                if (teachingTypeId.HasValue || instructorId.HasValue || !string.IsNullOrEmpty(status) || isMakeUpClass.HasValue) {
                    var sectionSlotId = updateList.Select(x => x.Id).ToList();
                    var sectionSlotDbs = _db.SectionSlots.Where(x => sectionSlotId.Contains(x.Id)).ToList();
                    foreach (var sectionSlot in sectionSlotDbs)
                    {
                        if (teachingTypeId.HasValue)
                        {
                            sectionSlot.TeachingTypeId = teachingTypeId.Value;
                        }
                        if (instructorId.HasValue)
                        {
                            sectionSlot.InstructorId = instructorId.Value;
                        }
                        if (!string.IsNullOrEmpty(status))
                        {
                            // currently disabled as not in requirement
                            //TODO: Logic to cancel / create room slot, see EditSectionDetailFunction
                        }
                        if (isMakeUpClass.HasValue)
                        {
                            // currently disabled as not in requirement
                        }
                    }
                    _db.SaveChanges();

                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                else
                {
                    _flashMessage.Warning(Message.RequiredData + " Must have at least one data to update.");
                }
            }
            else
            {
                _flashMessage.Warning(Message.RequiredData + " Please select at least one slot to update.");
            }
            return RedirectToAction(nameof(Index), new
            {
                AcademicLevelId = criteria?.AcademicLevelId,
                TermId = criteria?.TermId,
                CodeAndName = criteria?.CodeAndName,
                SectionNumber = criteria?.SectionNumber
            });
        }

        [PermissionAuthorize("SectionSlot", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Section section, string returnUrl)
        {
            var term = _academicProvider.GetTerm(section.TermId);
            section.AcademicLevelId = term.AcademicLevelId;
            var course = _db.Courses.SingleOrDefault(x => x.Id == section.CourseId);
            var dateNow = DateTime.UtcNow.AddHours(7).Date;
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var originalSection = Find(section.Id);
                    if (originalSection.SectionSlots != null && originalSection.SectionSlots.Any())
                    {
                        _db.SectionSlots.RemoveRange(originalSection.SectionSlots);
                    }

                    section.SectionSlots.ForEach(x => 
                    {
                        if (x.IsMakeUpClass 
                                && _db.ReservationCalendars.Any(y => y.StartedAt <= dateNow
                                                                                 && y.EndedAt >= dateNow
                                                                                 && y.IsActive)
                                && _db.MuicHolidays.Any(y => y.StartedAt <= x.Date
                                                                && y.EndedAt >= x.Date
                                                                && !y.IsMakeUpAble
                                                                && y.IsActive))
                        {        
                            _flashMessage.Danger(Message.ReservedDate);
                            throw new Exception(Message.ReservedDate);
                        }
                        x.SectionId = section.Id;
                        x.Day = (int)x.Date.DayOfWeek;
                    });
                    
                    _db.SectionSlots.AddRange(section.SectionSlots);
                    _db.SaveChanges();

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction("Index", "CourseToBeOffered", new Criteria 
                                                                          {
                                                                              CodeAndName = course.Code,
                                                                              AcademicLevelId = section.AcademicLevelId, 
                                                                              TermId = section.TermId 
                                                                          });
                }
                catch
                {
                    transaction.Rollback();
                    CreateSelectList();
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(section);
                }
            }
        }

        [PermissionAuthorize("SectionSlot", PolicyGenerator.Write)]
        public ActionResult Delete(long id, string returnUrl)
        {
            var slot = _db.SectionSlots.SingleOrDefault(x => x.Id == id);
            try
            {
                //_db.SectionSlots.Remove(slot);
                _roomProvider.CancelRoomSlotBySectionSlotId(slot.Id);
                slot.IsActive = false;

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return Redirect(returnUrl);
        }

        private Section Find(long id)
        {
            var section = _db.Sections.SingleOrDefault(x => x.Id == id);
            section.Course = _db.Courses.SingleOrDefault(x => x.Id == section.CourseId);
            section.Term = _db.Terms.SingleOrDefault(x => x.Id == section.TermId);
            section.SectionSlots = _sectionProvider.GetSectionSlotsBySectionId(id);
            section.SectionDetails = _sectionProvider.GetSectionDetailsBySectionId(id);

            return section;
        }

        private Section GenerateSectionSlots(long id, int totalWeeks)
        {
            var section = Find(id);
            section.SectionSlots = new List<SectionSlot>();
            section.SectionSlots = _sectionProvider.GenerateSectionSlots(totalWeeks, section);
            return section;
        }

        private void CreateSelectList(long academicLevelId = 0, long termId = 0, long courseId = 0, DateTime? date = null, string start = null, string end = null) 
        {
            TimeSpan? startedAt = _dateTimeProvider.ConvertStringToTime(start);
            TimeSpan? endedAt = _dateTimeProvider.ConvertStringToTime(end);
            ViewBag.Instructors = _selectListProvider.GetInstructors();
            ViewBag.Dayofweeks = _selectListProvider.GetDayOfWeek(); 
            ViewBag.TeachingTypes = _selectListProvider.GetTeachingTypes();
            ViewBag.Status = _selectListProvider.GetSectionSlotStatus();
            ViewBag.RoomJs = JsonConvert.SerializeObject(_selectListProvider.GetRooms());
            ViewBag.InstructorJs = JsonConvert.SerializeObject(_selectListProvider.GetInstructors());
            ViewBag.DayofweekJs = JsonConvert.SerializeObject(_selectListProvider.GetDayOfWeek()); 
            ViewBag.TeachingTypeJs = JsonConvert.SerializeObject(_selectListProvider.GetTeachingTypes());
            ViewBag.StatusJs = JsonConvert.SerializeObject(_selectListProvider.GetSectionSlotStatus());
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();

            if (date != null && startedAt != null && endedAt != null)
            {
                ViewBag.Rooms = _selectListProvider.GetAvailableRoom(date.Value, startedAt.Value, endedAt.Value, string.Empty);
            }
            
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }

            if (termId != 0)
            {
                ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);

                if (courseId != 0)
                {
                    ViewBag.Sections = _selectListProvider.GetSections(termId, courseId);
                }
            }
        }
    }
}