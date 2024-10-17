using KeystoneLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("ExaminationReservationManagement", "")]
    public class ExaminationReservationManagementController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly ISectionProvider _sectionProvider;
        protected readonly IReservationProvider _reservationProvider;
        protected readonly IRoomProvider _roomProvider;        
        protected readonly ICacheProvider _cacheProvider;

        public ExaminationReservationManagementController(ApplicationDbContext db,
                                                          ISelectListProvider selectListProvider,
                                                          IFlashMessage flashMessage,
                                                          IDateTimeProvider dateTimeProvider,
                                                          ISectionProvider sectionProvider,
                                                          IRoomProvider roomProvider,
                                                          ICacheProvider cacheProvider,
                                                          IReservationProvider reservationProvider) : base(db, flashMessage, selectListProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _sectionProvider = sectionProvider;
            _roomProvider = roomProvider;
            _reservationProvider = reservationProvider;
            _cacheProvider = cacheProvider;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList("defaultWaiting", criteria.AcademicLevelId, criteria.TermId, criteria.CampusId);
            if(criteria.AcademicLevelId == 0 && criteria.TermId == 0)
            {
                criteria.Status = string.IsNullOrEmpty(criteria.Status) ? "w" : criteria.Status;
                criteria.AcademicLevelId = _db.AcademicLevels.SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
                criteria.TermId = _cacheProvider.GetCurrentTerm(criteria.AcademicLevelId).Id;
                CreateSelectList("defaultWaiting", criteria.AcademicLevelId, criteria.TermId, criteria.CourseId,criteria.CampusId);
                return View(new PagedResult<ExaminationReservationManagementViewModel>()
                            {
                                Criteria = criteria
                            });

            }
            DateTime? starteAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);

            var sections = _db.Sections.Where(x => criteria.AcademicLevelId == x.Term.AcademicLevelId
                                                    && criteria.TermId == x.TermId
                                                    && (criteria.CourseId == 0
                                                        || criteria.CourseId == x.CourseId)
                                                    && (criteria.SectionId == 0
                                                          || criteria.SectionId == x.Id)
                                                    && x.Status != "r")
                                       .Select(x => new ExaminationReservationManagementViewModel
                                                    {
                                                        TermText = x.Term.AcademicTerm + "/" + x.Term.AcademicYear,
                                                        SectionId = x.Id,
                                                        CourseCode = x.Course.CodeAndSpecialChar,
                                                        CourseName = x.Course.NameEn,
                                                        CourseCredit = x.Course.Credit,
                                                        CourseLecture = x.Course.Lecture,
                                                        CourseLab = x.Course.Lab,
                                                        CourseOther = x.Course.Other,
                                                        InstructorFullNameEn = x.MainInstructor == null ? "" : x.MainInstructor.Title.NameEn + " " + x.MainInstructor.FirstNameEn + " " + x.MainInstructor.LastNameEn,
                                                        SectionNumber = x.Number,
                                                        SeatUsed = x.SeatUsed,
                                                        ParentSeatUsed = x.ParentSection == null ? 0 : x.ParentSection.SeatUsed,
                                                        ParentSectionId = x.ParentSectionId,
                                                        ParentSectionCourseCode = x.ParentSection.Course.Code,
                                                        ParentSectionNumber = x.ParentSection.Number,
                                                        IsDisabledFinal = x.IsDisabledFinal,
                                                        IsDisabledMidterm = x.IsDisabledMidterm
                                                    })
                                       .ToList();

            var examinationRoom = _db.ExaminationReservations.Where(x => criteria.AcademicLevelId == x.Term.AcademicLevelId
                                                                         && criteria.TermId == x.TermId
                                                                         && (criteria.CourseId == 0
                                                                             || criteria.CourseId == x.Section.CourseId)
                                                                         && (criteria.SectionId == 0
                                                                               || criteria.SectionId == x.SectionId)
                                                                         && ((starteAt == null || x.Date.Date >= starteAt.Value.Date)
                                                                         && (endedAt == null || x.Date.Date <= endedAt.Value.Date))
                                                                         && !x.Section.IsClosed
                                                                         && (criteria.TermId == 0 || x.Section.TermId == criteria.TermId)
                                                                         && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                                             || x.Section.Course.Code.Contains(criteria.CodeAndName)
                                                                             || x.Section.Course.NameEn.Contains(criteria.CodeAndName))
                                                                         && (string.IsNullOrEmpty(criteria.SectionNumber) 
                                                                             || x.Section.Number.Contains(criteria.SectionNumber))
                                                                         && (criteria.CampusId == 0
                                                                             || x.Room.Building.CampusId == criteria.CampusId)
                                                                         && (criteria.BuildingId == 0
                                                                             || x.Room.BuildingId == criteria.BuildingId)
                                                                         && (criteria.Floor == null
                                                                             || x.Room.Floor == criteria.Floor)
                                                                         && (string.IsNullOrEmpty(criteria.RoomName)
                                                                             || x.Room.NameEn.Contains(criteria.RoomName))
                                                                         && (criteria.ExaminationTypeId == 0
                                                                             || x.ExaminationTypeId == criteria.ExaminationTypeId)
                                                                         && (criteria.Status == "all"
                                                                             || x.Status == criteria.Status
                                                                             || (criteria.Status == "ne"
                                                                                 && (((criteria.ExaminationTypeId == 0
                                                                                       && ((x.ExaminationType.NameEn.Contains("final")
                                                                                            && x.Section.IsDisabledFinal)
                                                                                            || x.Section.IsDisabledMidterm
                                                                                               && x.ExaminationType.NameEn.Contains("midterm"))))
                                                                                     || (x.ExaminationType.NameEn.Contains("midterm") && x.Section.IsDisabledMidterm)
                                                                                     || (x.ExaminationType.NameEn.Contains("final") && x.Section.IsDisabledFinal))))
                                                                         && (string.IsNullOrEmpty(criteria.SenderType)
                                                                             || criteria.SenderType == x.SenderType))
                                                             .Select(x => new ExaminationReservationManagementViewModel
                                                                          {
                                                                              Id = x.Id,
                                                                              TermText = x.Term.AcademicTerm + "/" + x.Term.AcademicYear,
                                                                              SectionId = x.SectionId,
                                                                              CourseCode = x.Section.Course.CodeAndSpecialChar,
                                                                              CourseName = x.Section.Course.NameEn,
                                                                              CourseCredit = x.Section.Course.Credit,
                                                                              CourseLecture = x.Section.Course.Lecture,
                                                                              CourseLab = x.Section.Course.Lab,
                                                                              CourseOther = x.Section.Course.Other,
                                                                              InstructorFullNameEn = x.Instructor == null ? "" : x.Instructor.Title.NameEn + " " + x.Instructor.FirstNameEn + " " + x.Instructor.LastNameEn,
                                                                              SectionNumber = x.Section.Number,
                                                                              SeatUsed = x.Section.SeatUsed,
                                                                              ParentSeatUsed = x.Section.ParentSection == null ? 0 : x.Section.ParentSection.SeatUsed,
                                                                              ParentSectionId = x.Section.ParentSectionId,
                                                                              ParentSectionCourseCode = x.Section.ParentSection.Course.Code,
                                                                              ParentSectionNumber = x.Section.ParentSection.Number,
                                                                              Date = x.Date,
                                                                              StartTime = x.StartTime,
                                                                              EndTime = x.EndTime,
                                                                              ExaminationType = x.ExaminationType.NameEn,
                                                                              StudentRemark = x.StudentRemark,
                                                                              Room = x.Room.NameEn,
                                                                              RoomId = x.RoomId,
                                                                              TotalProctor = x.TotalProctor,
                                                                              UseProctor = x.UseProctor,
                                                                              AbsentInstructor = x.AbsentInstructor,
                                                                              AllowBooklet = x.AllowBooklet,
                                                                              AllowCalculator = x.AllowCalculator,
                                                                              AllowAppendix = x.AllowAppendix,
                                                                              AllowOpenbook = x.AllowOpenbook,
                                                                              Status = x.Status,
                                                                              SenderTypeText = x.SenderTypeText,
                                                                              ApprovedAt = x.ApprovedAt,
                                                                              ApprovedBy = x.ApprovedBy
                                                                          })
                                                             .GetPaged(criteria, page, true);

            var sectionIds = sections.Select(x => x.SectionId).ToList();
            var sectionIdsNullable = sections.Select(x => (long?)x.SectionId).ToList();
            var parentSectionIds = sections.Where(x => x.ParentSectionId != null).Select(x => x.ParentSectionId);
            var jointTotalSeatUsed = _db.Sections.Where(x => parentSectionIds.Contains(x.ParentSectionId))
                                                 .Select(x => new 
                                                  {
                                                      Id = x.Id,
                                                      ParentSectionId = x.ParentSectionId,
                                                      SeatUsed = x.SeatUsed
                                                  })
                                                 .ToList();
            var jointSections = _db.Sections.Where(x => sectionIdsNullable.Contains(x.ParentSectionId))
                                            .Select(x => new JointSectionCourseToBeOfferedViewModel
                                                        {
                                                             Id = x.Id,
                                                             ParentSectionId = x.ParentSectionId,
                                                             SeatUsed = x.SeatUsed,
                                                             Number = x.Number,
                                                             CourseCode = x.Course.CodeAndSpecialChar,
                                                        })
                                            .ToList();

            if(criteria.Status == "ne" || criteria.Status == "all")
            {
                var examType = _db.ExaminationTypes;
                var final = examType.Where(x => x.NameEn.Contains("final")).FirstOrDefault();
                var midterm = examType.Where(x => x.NameEn.Contains("midterm")).FirstOrDefault();
                if(criteria.ExaminationTypeId == 0 || final.Id == criteria.ExaminationTypeId)
                {
                    foreach (var item in sections.Where(x => x.IsDisabledFinal).ToList())
                    {
                        var existExamReservation = examinationRoom.Results.FirstOrDefault(x => x.SectionId == item.SectionId && x.ExaminationType == final.NameEn);
                        if (existExamReservation != null)
                        {
                            examinationRoom.Results.Remove(existExamReservation);
                        }
 
                        item.ExaminationType = final.NameEn;
                        item.Status = "ne";
                        item.JointSections = jointSections.Where(x => x.ParentSectionId == item.SectionId)
                                                        .ToList();
                        var jointsString = item.JointSections.Any() ? item.JointSections.Select(x => x.CourseCode + "(" + x.Number + ")").ToList() : new List<string>();
                        item.JointSectionsString = string.Join(", ", jointsString);
                        if (item.ParentSectionId == null)
                        {
                            item.TotalStudent = item.SeatUsed + jointSections.Where(x => x.ParentSectionId == item.SectionId).Sum(x => x.SeatUsed);
                        }
                        else
                        {
                            item.TotalStudent = item.ParentSeatUsed + jointTotalSeatUsed.Where(x => x.ParentSectionId == item.ParentSectionId).Sum(x => x.SeatUsed);
                        }
                        examinationRoom.Results.Add(item);
                        
                    }
                }

                if(criteria.ExaminationTypeId == 0 || midterm.Id == criteria.ExaminationTypeId)
                {
                    foreach (var item in sections.Where(x => x.IsDisabledMidterm).ToList())
                    {
                        var existExamReservation = examinationRoom.Results.FirstOrDefault(x => x.SectionId == item.SectionId && x.ExaminationType == midterm.NameEn);
                        if (existExamReservation != null)
                        {
                            examinationRoom.Results.Remove(existExamReservation);
                        }

                        item.ExaminationType = midterm.NameEn;
                        item.Status = "ne";
                        item.JointSections = jointSections.Where(x => x.ParentSectionId == item.SectionId)
                                                        .ToList();
                        var jointsString = item.JointSections.Any() ? item.JointSections.Select(x => x.CourseCode + "(" + x.Number + ")").ToList() : new List<string>();
                        item.JointSectionsString = string.Join(", ", jointsString);
                        if (item.ParentSectionId == null)
                        {
                            item.TotalStudent = item.SeatUsed + jointSections.Where(x => x.ParentSectionId == item.SectionId).Sum(x => x.SeatUsed);
                        }
                        else
                        {
                            item.TotalStudent = item.ParentSeatUsed + jointTotalSeatUsed.Where(x => x.ParentSectionId == item.ParentSectionId).Sum(x => x.SeatUsed);
                        }
                        examinationRoom.Results.Add(item);
                    }
                }

                examinationRoom.RowCount = examinationRoom.Results.Count();
            }

            foreach (var item in examinationRoom.Results)
            {

                item.JointSections = jointSections.Where(x => x.ParentSectionId == item.SectionId)
                                                  .ToList();
                var jointsString = item.JointSections.Any() ? item.JointSections.Select(x => x.CourseCode + "(" + x.Number + ")").ToList() : new List<string>();
                item.JointSectionsString = string.Join(", ", jointsString);
                if (item.ParentSectionId == null)
                {
                    item.TotalStudent = item.SeatUsed + jointSections.Where(x => x.ParentSectionId == item.SectionId).Sum(x => x.SeatUsed);
                }
                else
                {
                    item.TotalStudent = item.ParentSeatUsed + jointTotalSeatUsed.Where(x => x.ParentSectionId == item.ParentSectionId).Sum(x => x.SeatUsed);
                }
                item.ApprovedAtText = _dateTimeProvider.ConvertFromUtcToSE(item.ApprovedAt)?.ToString(StringFormat.ShortDateTime);
                if(item.ApprovedBy != null)
                {
                    var approvedUser = _db.Users.Where(x => x.Id == item.ApprovedBy).SingleOrDefault();
                    if (approvedUser != null)
                    {
                        var approveInstructor = _db.Instructors.Where(x => x.Id == approvedUser.InstructorId)
                                                            .Select(x => new
                                                            {
                                                                Title = x.Title.NameEn,
                                                                FirstName = x.FirstNameEn,
                                                                LastNameEn = x.LastNameEn
                                                            })
                                                            .SingleOrDefault();
                        if (approveInstructor != null)
                        {
                            item.ApprovedByFullNameEn = $"{ approveInstructor.Title } { approveInstructor.FirstName } { approveInstructor.LastNameEn }";
                        }
                        else if (!string.IsNullOrEmpty(approvedUser.FirstnameEN))
                        {
                            item.ApprovedByFullNameEn = $"{ approvedUser.FirstnameEN } { approvedUser.LastnameEN }";
                        }
                        else
                        {
                            item.ApprovedByFullNameEn = $"{ approvedUser.UserName }";
                        }
                    }
                    else
                    {
                        item.ApprovedByFullNameEn = item.ApprovedBy;
                    }
                }                             
            }
            return View(examinationRoom);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            var model = _reservationProvider.GetExaminationReservation(id);
            model.StartTimeText = model.StartTime.ToString(StringFormat.TimeSpan);
            model.EndTimeText = model.EndTime.ToString(StringFormat.TimeSpan);
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.Status, model.AcademicLevelId, model.TermId);
            return PartialView("_EditExaminationReservation", model);
        }

        [PermissionAuthorize("ExaminationReservationManagement", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = _reservationProvider.GetExaminationReservation(id ?? 0);
            model.StartTimeText = model.StartTime.ToString(StringFormat.TimeSpan);
            model.EndTimeText = model.EndTime.ToString(StringFormat.TimeSpan);
            var previousStatus = model.Status;
            if (ModelState.IsValid && await TryUpdateModelAsync<ExaminationReservation>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    if (model.Status == "a" && model.RoomId != null)
                    {
                        bool isOverlapped = false;

                        //check with instructor
                        if (!model.AbsentInstructor)
                        {
                            TimeSpan? startedAt = _dateTimeProvider.ConvertStringToTime(model.StartTimeText);
                            TimeSpan? endedAt = _dateTimeProvider.ConvertStringToTime(model.EndTimeText);
                            //check section slot
                            if (_reservationProvider.IsExamExisted(model.Id, model.ExaminationTypeId, model.InstructorId ?? 0, model.RoomId ?? 0, model.Date, startedAt, endedAt))
                            {
                                isOverlapped = true;
                                // _flashMessage.Danger($"There is section slot with instructor for {model.DateText} at {model.StartTimeText} to {model.EndTimeText}");
                            }
                            else if (_sectionProvider.IsSectionSlotExisted(model.SectionId, model.InstructorId ?? 0, 0, model.Date, startedAt, endedAt))
                            //check exam reservation
                            {
                                var sectionSlots = _db.SectionSlots.Where(x => x.SectionId == model.SectionId
                                                                              && x.Date == model.Date
                                                                              && x.StartTime < model.EndTime
                                                                              && x.EndTime > model.StartTime
                                                                              && x.Status != "c"
                                                                              && (model.InstructorId == 0 || x.InstructorId == model.InstructorId))
                                                                  .ToList();

                                sectionSlots.Select(x => {
                                                            x.Status = "c";
                                                            return x;
                                                        })
                                           .ToList();
                                var sectionSlotIds = sectionSlots.Select(x => x.Id).ToList();
                                var roomSlots = _roomProvider.GetRoomSlotsBySectionSlotIds(sectionSlotIds);
                                _reservationProvider.CancelRoomSlot(roomSlots);

                                _db.SaveChanges();
                                // _flashMessage.Danger($"There is examination reservation with instructor for {model.DateText} at {model.StartTimeText} to {model.EndTimeText}");
                            }
                        }

                        if (isOverlapped)
                        {
                            CreateSelectList(model.Status, model.AcademicLevelId, model.TermId);
                            _flashMessage.Danger(Message.DataAlreadyExist);
                            return Redirect(returnUrl);
                        }
                        model.ApprovedAt = DateTime.UtcNow;
                        model.ApprovedBy = GetUser()?.Id ?? "" ;

                        var newRoomSlot = new RoomSlot
                        {
                            TermId = model.TermId,
                            RoomId = model.RoomId ?? 0,
                            Date = model.Date,
                            Day = (int)model.Date.DayOfWeek,
                            StartTime = model.StartTime,
                            EndTime = model.EndTime,
                            UsingType = "e",
                            ExaminationReservationId = model.Id
                        };
                        var roomSlot = _db.RoomSlots.Where(x => x.TermId == newRoomSlot.TermId
                                                                    && x.RoomId == newRoomSlot.RoomId
                                                                    && x.Date == newRoomSlot.Date
                                                                    && x.Day == newRoomSlot.Day
                                                                    && x.StartTime == newRoomSlot.StartTime
                                                                    && x.EndTime == newRoomSlot.EndTime
                                                                    && x.UsingType == newRoomSlot.UsingType
                                                                    && x.ExaminationReservationId == newRoomSlot.ExaminationReservationId
                                                                    && x.IsActive
                                                                    && !x.IsCancel
                                                           )
                                                    .ToList();
                        if (roomSlot == null || roomSlot.Count < 1)
                        {
                            var cancelExaminationRoomSlot = _reservationProvider.GetExaminationRoomSlots(model.Id);
                            var cancel = _reservationProvider.CancelRoomSlot(cancelExaminationRoomSlot);

                            _db.RoomSlots.Add(newRoomSlot);
                        }

                        // Update Examination Date in Section
                        var section = _db.Sections.SingleOrDefault(x => x.Id == model.SectionId);
                        string examinationType = _db.ExaminationTypes.SingleOrDefault(x => x.Id == model.ExaminationTypeId).NameEn.ToLower();
                        var jointSections = _db.Sections.Where(x => x.ParentSectionId == section.Id).ToList();
                        if (examinationType.Contains("midterm"))
                        {
                            section.MidtermDate = model.Date;
                            section.MidtermStart = model.StartTime;
                            section.MidtermEnd = model.EndTime;
                            section.MidtermRoomId = model.RoomId;

                            if(jointSections != null && jointSections.Any())
                            {
                                foreach (var jointSection in jointSections)
                                {
                                    jointSection.MidtermDate = model.Date;
                                    jointSection.MidtermStart = model.StartTime;
                                    jointSection.MidtermEnd = model.EndTime;
                                    jointSection.MidtermRoomId = model.RoomId;
                                }
                            }
                        }
                        else
                        {
                            section.FinalDate = model.Date;
                            section.FinalStart = model.StartTime;
                            section.FinalEnd = model.EndTime;
                            section.FinalRoomId = model.RoomId;
                            if(jointSections != null && jointSections.Any())
                            {
                                foreach (var jointSection in jointSections)
                                {
                                    jointSection.FinalDate = model.Date;
                                    jointSection.FinalStart = model.StartTime;
                                    jointSection.FinalEnd = model.EndTime;
                                    jointSection.FinalRoomId = model.RoomId;
                                }
                            }
                        }
                        _db.SaveChanges();
                    }

                    if (model.Status == "r")
                    {
                        var cancelExaminationRoomSlot = _reservationProvider.GetExaminationRoomSlots(id ?? 0);
                        var cancel = _reservationProvider.CancelRoomSlot(cancelExaminationRoomSlot);
                        if (!cancel)
                        {
                            _flashMessage.Danger(Message.UnableToCancelExaminationRoomSlot);
                            return Redirect(returnUrl);
                        }

                        // Update Section       
                        var section = _db.Sections.SingleOrDefault(x => x.Id == model.SectionId);
                        var jointSections = _db.Sections.Where(x => x.ParentSectionId == section.Id).ToList();
                        string examinationType = _db.ExaminationTypes.SingleOrDefault(x => x.Id == model.ExaminationTypeId).Abbreviation;
                        if (examinationType == "MT")
                        {
                            section.MidtermDate = null;
                            section.MidtermStart = null;
                            section.MidtermEnd = null;
                            section.MidtermRoomId = null;

                            if(jointSections != null && jointSections.Any())
                            {
                                foreach (var jointSection in jointSections)
                                {
                                    jointSection.MidtermDate = null;
                                    jointSection.MidtermStart = null;
                                    jointSection.MidtermEnd = null;
                                    jointSection.MidtermRoomId = null;
                                }
                            }
                        }
                        else
                        {
                            section.FinalDate = null;
                            section.FinalStart = null;
                            section.FinalEnd = null;
                            section.FinalRoomId = null;

                            if(jointSections != null && jointSections.Any())
                            {
                                foreach (var jointSection in jointSections)
                                {
                                    jointSection.FinalDate = null;
                                    jointSection.FinalStart = null;
                                    jointSection.FinalEnd = null;
                                    jointSection.FinalRoomId = null;
                                }
                            }
                        }
                        model.ApprovedAt = null;
                        model.ApprovedBy = null;

                        _db.SaveChanges();
                    }
                    else if(model.Status == "w")
                    {
                        if (previousStatus == "a") //was approved 
                        {
                            var cancelExaminationRoomSlot = _reservationProvider.GetExaminationRoomSlots(id ?? 0);

                            if (cancelExaminationRoomSlot != null)
                            {
                                var cancel = _reservationProvider.CancelRoomSlot(cancelExaminationRoomSlot);
                                if (!cancel)
                                {
                                    _flashMessage.Danger(Message.UnableToCancelExaminationRoomSlot);
                                    return Redirect(returnUrl);
                                }
                            }
                        }
                        
                        var section = _db.Sections.SingleOrDefault(x => x.Id == model.SectionId);
                        string examinationType = _db.ExaminationTypes.SingleOrDefault(x => x.Id == model.ExaminationTypeId).NameEn.ToLower();
                        var jointSections = _db.Sections.Where(x => x.ParentSectionId == section.Id).ToList();
                        if (examinationType.Contains("midterm"))
                        {
                            section.MidtermDate = model.Date;
                            section.MidtermStart = model.StartTime;
                            section.MidtermEnd = model.EndTime;
                            section.MidtermRoomId = model.RoomId;

                            if(jointSections != null && jointSections.Any())
                            {
                                foreach (var jointSection in jointSections)
                                {
                                    jointSection.MidtermDate = model.Date;
                                    jointSection.MidtermStart = model.StartTime;
                                    jointSection.MidtermEnd = model.EndTime;
                                    jointSection.MidtermRoomId = model.RoomId;
                                }
                            }
                        }
                        else
                        {
                            section.FinalDate = model.Date;
                            section.FinalStart = model.StartTime;
                            section.FinalEnd = model.EndTime;
                            section.FinalRoomId = model.RoomId;
                            if(jointSections != null && jointSections.Any())
                            {
                                foreach (var jointSection in jointSections)
                                {
                                    jointSection.FinalDate = model.Date;
                                    jointSection.FinalStart = model.StartTime;
                                    jointSection.FinalEnd = model.EndTime;
                                    jointSection.FinalRoomId = model.RoomId;
                                }
                            }
                        }
                        model.ApprovedAt = null;
                        model.ApprovedBy = null;
                        _db.SaveChanges();
                    }

                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                    CreateSelectList(model.Status, model.AcademicLevelId, model.TermId);
                }

                return Redirect(returnUrl);
            }

            _flashMessage.Danger(Message.UnableToEdit);
            CreateSelectList(model.Status, model.AcademicLevelId, model.TermId);
            return Redirect(returnUrl);
        }

        [PermissionAuthorize("ExaminationReservationManagement", PolicyGenerator.Write)]
        public ActionResult Delete(long id, string returnUrl)
        {
            var model = _reservationProvider.GetExaminationReservation(id);
            var cancelRoomSlot = _reservationProvider.GetExaminationRoomSlots(id);
            var cancel = _reservationProvider.CancelRoomSlot(cancelRoomSlot);
            if (!cancel)
            {
                _flashMessage.Danger(Message.UnableToCancelExaminationRoomSlot);
                return Redirect(returnUrl);
            }
            
            try
            {
                model.IsActive = false;
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return Redirect(returnUrl);
        }
        [HttpPost]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult ExportExcel(List<ExaminationReservationManagementViewModel> results, string returnUrl)
        {
            if (results != null && results.Any())
            {
                using (var wb = GenerateWorkBook(results))
                {
                    return wb.Deliver($"Examination Reservation Report.xlsx");
                }
            }

            return Redirect(returnUrl);
        }

        private XLWorkbook GenerateWorkBook(List<ExaminationReservationManagementViewModel> results)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;
            ws.Cell(row, column++).Value = "TERM";
            ws.Cell(row, column++).Value = "COURSE";
            ws.Cell(row, column++).Value = "COURSE NAME";
            ws.Cell(row, column++).Value = "SECTION";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "SECTION TYPE";

            ws.Cell(row, column++).Value = "MASTER/JOINT";
            ws.Cell(row, column++).Value = "INSTRUCTOR";
            ws.Cell(row, column++).Value = "EXAMINATION TYPE";
            ws.Cell(row, column++).Value = "ROOM";
            ws.Cell(row, column++).Value = "DATE";
            ws.Cell(row, column++).Value = "TIME";
            ws.Cell(row, column++).Value = "TOTAL STUDENT";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "USE PROCTOR";

            ws.Cell(row, column++).Value = "PROCTOR";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "ABSENT/JOIN OTHER SECTION INSTRUCTOR";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "BOOKLET";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "CALCULATOR";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "APPENDIX";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "TEXTBOOK";
            ws.Cell(row, column++).Value = "STATUS";
            ws.Cell(row, column++).Value = "SENDER";
            ws.Cell(row, column++).Value = "APPROVED DATE";
            ws.Cell(row, column++).Value = "APPROVED BY";
            ws.Cell(row++, column).Value = "STUDENT REMARK";

            foreach (var item in results)
            {
                column = 1;
                ws.Cell(row, column++).SetValue<string>(item.TermText);
                ws.Cell(row, column++).Value = item.CourseCodeAndCredit;
                ws.Cell(row, column++).Value = item.CourseName;
                ws.Cell(row, column++).Value = item.SectionNumber;

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.SectionTypes;

                ws.Cell(row, column++).Value = item.JointSectionsString;
                ws.Cell(row, column++).Value = item.InstructorFullNameEn;
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.ExaminationType;
                ws.Cell(row, column++).Value = item.Room;
                ws.Cell(row, column++).Value = item.DateText;
                ws.Cell(row, column++).Value = item.Time;
                ws.Cell(row, column++).Value = item.TotalStudent;

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.Status == "ne" ? "" : item.UseProctor ? "Yes" : "No";

                ws.Cell(row, column++).Value = item.Status == "ne" ? "" : item.TotalProctor.ToString();

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.Status == "ne" ? "" : item.AbsentInstructor ? "Yes" : "No";

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.Status == "ne" ? "" : item.AllowBooklet ? "Yes" : "No";

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.Status == "ne" ? "" : item.AllowCalculator ? "Yes" : "No";

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.Status == "ne" ? "" : item.AllowAppendix ? "Yes" : "No";

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.Status == "ne" ? "" : item.AllowOpenbook ? "Yes" : "No";

                ws.Cell(row, column++).Value = item.StatusText;
                ws.Cell(row, column++).Value = item.SenderTypeText;
                ws.Cell(row, column++).Value = item.ApprovedAtText;
                ws.Cell(row, column++).Value = item.ApprovedByFullNameEn;
                ws.Cell(row++, column).Value = item.StudentRemark;
            }
            
            ws.Columns().AdjustToContents();
            ws.Rows().AdjustToContents();
            return wb;
        }

        [PermissionAuthorize("ExaminationReservationManagement", PolicyGenerator.Write)]
        public IActionResult AddSlot(long id, string returnUrl)
        {
            var model = _reservationProvider.GetExaminationReservation(id);
            var checkRoomSlot = _reservationProvider.GetExaminationRoomSlots(id);
            var cancelRoom = _reservationProvider.CancelRoomSlot(checkRoomSlot);
            if (!cancelRoom)
            {
                _flashMessage.Danger(Message.UnableToCancelExaminationRoomSlot);
                return RedirectToAction(nameof(Index), new { StartedAt = model.StartTimeText, EndedAt = model.EndTimeText });
            }

            try
            {
                model.Status = "a";
                _db.RoomSlots.Add(new RoomSlot
                                  {
                                      TermId = model.TermId,
                                      RoomId = model.RoomId ?? 0,
                                      Date = model.Date,
                                      Day = (int)model.Date.DayOfWeek,
                                      StartTime = model.StartTime,
                                      EndTime = model.EndTime,
                                      UsingType = "e",
                                      ExaminationReservationId = model.Id
                                  });
                // Update Section
                var section = _db.Sections.SingleOrDefault(x => x.Id == model.SectionId);
                var jointSections = _db.Sections.Where(x => x.ParentSectionId == section.Id).ToList();
                string examinationType = _db.ExaminationTypes.SingleOrDefault(x => x.Id == model.ExaminationTypeId).Abbreviation;
                if (examinationType == "MT")
                {
                    section.MidtermDate = model.Date;
                    section.MidtermStart = model.StartTime;
                    section.MidtermEnd = model.EndTime;
                    section.MidtermRoomId = model.RoomId;
                    if(jointSections != null && jointSections.Any())
                    {
                        if(jointSections != null && jointSections.Any())
                        {
                            foreach (var jointSection in jointSections)
                            {
                                jointSection.MidtermDate = model.Date;
                                jointSection.MidtermStart = model.StartTime;
                                jointSection.MidtermEnd = model.EndTime;
                                jointSection.MidtermRoomId = model.RoomId;
                            }
                        }
                    }
                }
                else
                {
                    section.FinalDate = model.Date;
                    section.FinalStart = model.StartTime;
                    section.FinalEnd = model.EndTime;
                    section.FinalRoomId = model.RoomId;
                    if(jointSections != null && jointSections.Any())
                    {
                        foreach (var jointSection in jointSections)
                        {
                            jointSection.FinalDate = model.Date;
                            jointSection.FinalStart = model.StartTime;
                            jointSection.FinalEnd = model.EndTime;
                            jointSection.FinalRoomId = model.RoomId;
                        }
                    }
                }

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCreate);
            }

            model.StartTimeText = model.Date.ToString(StringFormat.ShortDate);
            model.EndTimeText = model.Date.ToString(StringFormat.ShortDate);
            return RedirectToAction(nameof(Index), new { StartedAt = model.StartTimeText, EndedAt = model.EndTimeText });
        }

        [PermissionAuthorize("ExaminationReservationManagement", PolicyGenerator.Write)]
        public IActionResult WaitingSlot(long id)
        {
            var model = _reservationProvider.GetExaminationReservation(id);
            model.StartTimeText = model.Date.ToString(StringFormat.ShortDate);
            model.EndTimeText = model.Date.ToString(StringFormat.ShortDate);
            var cancelRoomSlot = _reservationProvider.GetExaminationRoomSlots(id);
            var cancel = _reservationProvider.CancelRoomSlot(cancelRoomSlot);
            if (!cancel)
            {
                _flashMessage.Danger(Message.UnableToCancelExaminationRoomSlot);
                return RedirectToAction(nameof(Index), new { StartedAt = model.StartTimeText, EndedAt = model.EndTimeText });
            }
            
            try
            {
                model.Status = "w";
                var section = _db.Sections.SingleOrDefault(x => x.Id == model.SectionId);
                var jointSections = _db.Sections.Where(x => x.ParentSectionId == section.Id).ToList();
                string examinationType = _db.ExaminationTypes.SingleOrDefault(x => x.Id == model.ExaminationTypeId).Abbreviation;
                if (examinationType == "MT")
                {
                    section.MidtermDate = model.Date;
                    section.MidtermStart = model.StartTime;
                    section.MidtermEnd = model.EndTime;
                    section.MidtermRoomId = model.RoomId;
                    if(jointSections != null && jointSections.Any())
                    {
                        if(jointSections != null && jointSections.Any())
                        {
                            foreach (var jointSection in jointSections)
                            {
                                jointSection.MidtermDate = model.Date;
                                jointSection.MidtermStart = model.StartTime;
                                jointSection.MidtermEnd = model.EndTime;
                                jointSection.MidtermRoomId = model.RoomId;
                            }
                        }
                    }
                }
                else
                {
                    section.FinalDate = model.Date;
                    section.FinalStart = model.StartTime;
                    section.FinalEnd = model.EndTime;
                    section.FinalRoomId = model.RoomId;
                    if(jointSections != null && jointSections.Any())
                    {
                        foreach (var jointSection in jointSections)
                        {
                            jointSection.FinalDate = model.Date;
                            jointSection.FinalStart = model.StartTime;
                            jointSection.FinalEnd = model.EndTime;
                            jointSection.FinalRoomId = model.RoomId;
                        }
                    }
                }

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index), new { StartedAt = model.StartTimeText, EndedAt = model.EndTimeText });
        }

        [PermissionAuthorize("ExaminationReservationManagement", PolicyGenerator.Write)]
        public IActionResult RejectSlot(long id)
        {
            var model = _reservationProvider.GetExaminationReservation(id);
            model.StartTimeText = model.Date.ToString(StringFormat.ShortDate);
            model.EndTimeText = model.Date.ToString(StringFormat.ShortDate);
            var cancelRoomSlot = _reservationProvider.GetExaminationRoomSlots(id);
            var cancel = _reservationProvider.CancelRoomSlot(cancelRoomSlot);
            if (!cancel)
            {
                _flashMessage.Danger(Message.UnableToCancelExaminationRoomSlot);
                return RedirectToAction(nameof(Index), new { StartedAt = model.StartTimeText, EndedAt = model.EndTimeText });
            }

            try
            {
                model.Status = "r";
                model.IsActive = false;
                var section = _db.Sections.SingleOrDefault(x => x.Id == model.SectionId);
                var jointSections = _db.Sections.Where(x => x.ParentSectionId == section.Id).ToList();
                string examinationType = _db.ExaminationTypes.SingleOrDefault(x => x.Id == model.ExaminationTypeId).Abbreviation;
                if (examinationType == "MT")
                {
                    section.MidtermDate = null;
                    section.MidtermStart = null;
                    section.MidtermEnd = null;
                    section.MidtermRoomId = null;
                    if(jointSections != null && jointSections.Any())
                    {
                        foreach (var jointSection in jointSections)
                        {
                            jointSection.MidtermDate = null;
                            jointSection.MidtermStart = null;
                            jointSection.MidtermEnd = null;
                            jointSection.MidtermRoomId = null;
                        }
                    }
                }
                else
                {
                    section.FinalDate = null;
                    section.FinalStart = null;
                    section.FinalEnd = null;
                    section.FinalRoomId = null;
                    if(jointSections != null && jointSections.Any())
                    {
                        foreach (var jointSection in jointSections)
                        {
                            jointSection.FinalDate = null;
                            jointSection.FinalStart = null;
                            jointSection.FinalEnd = null;
                            jointSection.FinalRoomId = null;
                        }
                    }
                }
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            model.StartTimeText = model.Date.ToString(StringFormat.ShortDate);
            model.EndTimeText = model.Date.ToString(StringFormat.ShortDate);
            return RedirectToAction(nameof(Index), new { StartedAt = model.StartTimeText, EndedAt = model.EndTimeText });
        }

        public string CheckRoom(long id, long roomId = 0)
        {
            if(roomId == 0)
            {
                return string.Empty;
            }

            var exam = _reservationProvider.GetExaminationReservation(id);
            var rooms = _roomProvider.IsExistedRoomSlot(roomId, exam.Date, exam.StartTime, exam.EndTime, exam.Id);
            if(rooms != null)
            {
                return Message.RoomIsNotAvailable;
            }

            return string.Empty;
        }
        private void CreateSelectList(string currentStatus = "", long academicLevelId = 0, long termId = 0, long courseId = 0, long campusId = 0)
        {
            ViewBag.Campuses = _selectListProvider.GetCampuses();
            ViewBag.Instructors = _selectListProvider.GetInstructors();
            ViewBag.ReservationStatuses = _selectListProvider.GetReservationStatuses(currentStatus);
            ViewBag.SenderTypes = _selectListProvider.GetSenderTypes();
            ViewBag.ExaminationTypes = _selectListProvider.GetExaminationTypes();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (campusId != 0)
            {
                ViewBag.Buildings = _selectListProvider.GetBuildings(campusId);
            }

            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);

                if (termId != 0)
                {
                    ViewBag.Courses = _selectListProvider.GetCoursesByAcademicLevelAndTerm(academicLevelId, termId);

                    if (courseId != 0)
                    {
                        ViewBag.Sections = _selectListProvider.GetSectionByCourseId(termId, courseId);
                    }
                }
            }



            // TimeSpan? startedAt = _dateTimeProvider.ConvertStringToTime(start);
            // TimeSpan? endedAt = _dateTimeProvider.ConvertStringToTime(end);
            // if (date != null && startedAt != null && endedAt != null)
            // {
            //     ViewBag.Rooms = _selectListProvider.GetAvailableRoom(date.Value, startedAt.Value, endedAt.Value, "i", roomId);
            // }
            ViewBag.Rooms = _selectListProvider.GetRooms();
        }
    }
}