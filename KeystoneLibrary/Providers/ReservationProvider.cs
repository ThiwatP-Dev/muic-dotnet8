using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.Report;
using Microsoft.EntityFrameworkCore;

namespace KeystoneLibrary.Providers
{
    public class ReservationProvider : BaseProvider, IReservationProvider
    {
        protected ISectionProvider _sectionProvider;
        protected IDateTimeProvider _dateTimeProvider;

        public ReservationProvider(ApplicationDbContext db,
                                   IMapper mapper,
                                   IDateTimeProvider dateTimeProvider,
                                   ISectionProvider sectionProvider) : base(db, mapper)
        {
            _sectionProvider = sectionProvider;
            _dateTimeProvider = dateTimeProvider;
        }

        public RoomReservation GetRoomReservation(long? id)
        {
            var roomReservation = _db.RoomReservations.Include(x => x.Term)
                                                      .IgnoreQueryFilters()
                                                      .SingleOrDefault(x => x.Id == id);

            if (roomReservation == null)
                return null;

            roomReservation.AcademicLevelId = roomReservation.Term.AcademicLevelId;
            roomReservation.StartTimeText = roomReservation.StartTime.ToString(StringFormat.TimeSpan);
            roomReservation.EndTimeText = roomReservation.EndTime.ToString(StringFormat.TimeSpan);
            return roomReservation;
        }

        public List<RoomReservationSlotViewModel> GetRoomSlotByReservation(long id)
        {
            var roomReservation = _db.RoomSlots.Where(x => x.RoomReservationId == id)
                                                .Select(x => new RoomReservationSlotViewModel
                                                {
                                                    Id = x.Id,
                                                    RoomNameEn = x.Room.NameEn,
                                                    Day = x.Day,
                                                    Date = x.Date,
                                                    StartTime = x.StartTime,
                                                    EndTime = x.EndTime,
                                                    IsCancel = x.IsCancel,
                                                    IsChecked = "on",
                                                    RoomReservationId = x.RoomReservationId
                                                })
                                               .OrderBy(x => x.Date)
                                               .ToList();
            return roomReservation;
        }

        public List<RoomSlot> GenerateRoomSlotByRoomReservation(RoomReservation roomReservation)
        {
            if (roomReservation.DateFrom.Date == roomReservation.DateTo.Date)
            {
                List<RoomSlot> roomSlots = new List<RoomSlot>();
                var roomSlot = new RoomSlot()
                {
                    TermId = roomReservation.TermId,
                    RoomId = roomReservation.RoomId,
                    Day = (int)roomReservation.DateFrom.Date.DayOfWeek,
                    Date = roomReservation.DateFrom.Date,
                    StartTime = roomReservation.StartTime,
                    EndTime = roomReservation.EndTime,
                    UsingType = roomReservation.UsingType,
                    RoomReservationId = roomReservation.Id,
                    IsCancel = false
                };

                roomSlots.Add(roomSlot);
                return roomSlots;
            }
            else
            {
                var days = _db.Calendars.Where(x => x.Date >= roomReservation.DateFrom
                                                    && x.Date <= roomReservation.DateTo);
                if (roomReservation.IsSunday
                    || roomReservation.IsMonday
                    || roomReservation.IsTuesday
                    || roomReservation.IsWednesday
                    || roomReservation.IsThursday
                    || roomReservation.IsFriday
                    || roomReservation.IsSaturday)
                {
                    days = days.Where(x => (roomReservation.IsSunday && x.DayOfWeek == (int)DayOfWeek.Sunday)
                                        || (roomReservation.IsMonday && x.DayOfWeek == (int)DayOfWeek.Monday)
                                        || (roomReservation.IsTuesday && x.DayOfWeek == (int)DayOfWeek.Tuesday)
                                        || (roomReservation.IsWednesday && x.DayOfWeek == (int)DayOfWeek.Wednesday)
                                        || (roomReservation.IsThursday && x.DayOfWeek == (int)DayOfWeek.Thursday)
                                        || (roomReservation.IsFriday && x.DayOfWeek == (int)DayOfWeek.Friday)
                                        || (roomReservation.IsSaturday && x.DayOfWeek == (int)DayOfWeek.Saturday));
                }

                var roomSlots = days.Select(x => new RoomSlot
                {
                    TermId = roomReservation.TermId,
                    RoomId = roomReservation.RoomId,
                    Day = x.DayOfWeek,
                    Date = x.Date,
                    StartTime = roomReservation.StartTime,
                    EndTime = roomReservation.EndTime,
                    UsingType = roomReservation.UsingType,
                    RoomReservationId = roomReservation.Id,
                    IsCancel = false
                })
                                    .ToList();

                return roomSlots;
            }
        }

        public bool IsExamExisted(long id, long examinationTypeId, long instructorId, long roomId, DateTime date, TimeSpan? start, TimeSpan? end)
        {
            return _db.ExaminationReservations.Any(x => x.Id != id
                                                        && x.ExaminationTypeId == examinationTypeId
                                                        && (roomId == 0 || x.RoomId == roomId)
                                                        && (instructorId == 0 || (x.InstructorId == instructorId && !x.AbsentInstructor))
                                                        && x.Status != "r"
                                                        && x.Date == date
                                                        && x.StartTime < end
                                                        && x.EndTime > start);
        }
        public bool IsExamExistedOnlyApproval(long id, long examinationTypeId, long instructorId, long roomId, DateTime date, TimeSpan? start, TimeSpan? end)
        {
            return _db.ExaminationReservations.Any(x => x.Id != id
                                                        && x.ExaminationTypeId == examinationTypeId
                                                        && (roomId == 0 || x.RoomId == roomId)
                                                        && (instructorId == 0 || x.InstructorId == instructorId)
                                                        && x.Status == "a"
                                                        && x.Date == date
                                                        && x.StartTime < end
                                                        && x.EndTime > start);
        }
        public bool IsRoomExisted(RoomReservation roomReservation)
        {
            var reservationDates = GenerateSelectedDate(roomReservation);
            var isOverlap = false;

            foreach (var reservationDate in reservationDates)
            {
                // ROOM SLOT
                isOverlap = (_db.RoomSlots.Any(x => x.RoomReservationId != roomReservation.Id
                                                    && !x.IsCancel
                                                    && x.RoomId == roomReservation.RoomId
                                                    && reservationDate.Date == x.Date
                                                    && x.StartTime < roomReservation.EndTime
                                                    && x.EndTime > roomReservation.StartTime));

                if (!isOverlap)
                {
                    // EXISTS RESERVATION
                    var existsReservations = _db.RoomReservations.AsNoTracking()
                                                                 .Where(x => x.Id != roomReservation.Id
                                                                            && x.RoomId == roomReservation.RoomId
                                                                            && x.DateFrom <= reservationDate.Date
                                                                            && x.DateTo >= reservationDate.Date
                                                                            && x.StartTime < roomReservation.EndTime
                                                                            && x.EndTime > roomReservation.StartTime
                                                                            && x.Status != "r"
                                                                            && x.Status != "c")
                                                                 .ToList();

                    if (existsReservations != null && existsReservations.Any())
                    {
                        foreach (var existsReservation in existsReservations)
                        {
                            var alreadyHaveRoomSlot = _db.RoomSlots.AsNoTracking()
                                                                   .IgnoreQueryFilters()
                                                                   .Where(x => x.RoomReservationId == existsReservation.Id
                                                                       && x.IsActive
                                                                   ).ToList();
                             
                            var compareDates = GenerateSelectedDate(existsReservation);
                            if (compareDates.Any(x => x.Date == reservationDate.Date
                                                      && existsReservation.StartTime < roomReservation.EndTime
                                                      && existsReservation.EndTime > roomReservation.StartTime))
                            {
                                //if already have room slot check if it cancel
                                if (alreadyHaveRoomSlot != null && alreadyHaveRoomSlot.Any())
                                {
                                    var existSlot = alreadyHaveRoomSlot.FirstOrDefault(x => x.Date == reservationDate.Date
                                                          && existsReservation.StartTime < roomReservation.EndTime
                                                          && existsReservation.EndTime > roomReservation.StartTime);
                                    if (existSlot != null && existSlot.IsCancel)
                                    {
                                        continue;
                                    }
                                }
                                return true;
                            }
                        }
                    }
                }
            }
            return isOverlap;
        }

        public List<DateTime> GenerateSelectedDate(RoomReservation roomReservation)
        {
            var selectedDates = new List<DateTime>();
            if (roomReservation.DateFrom.Date == roomReservation.DateTo.Date)
            {
                selectedDates.Add(roomReservation.DateFrom);
            }
            else
            {
                DateTime compare = roomReservation.DateFrom;
                while (compare <= roomReservation.DateTo)
                {
                    if (roomReservation.IsMonday && compare.Date.DayOfWeek == DayOfWeek.Monday)
                    {
                        selectedDates.Add(compare.Date);
                    }
                    else if (roomReservation.IsTuesday && compare.Date.DayOfWeek == DayOfWeek.Tuesday)
                    {
                        selectedDates.Add(compare.Date);
                    }
                    else if (roomReservation.IsWednesday && compare.Date.DayOfWeek == DayOfWeek.Wednesday)
                    {
                        selectedDates.Add(compare.Date);
                    }
                    else if (roomReservation.IsThursday && compare.Date.DayOfWeek == DayOfWeek.Thursday)
                    {
                        selectedDates.Add(compare.Date);
                    }
                    else if (roomReservation.IsFriday && compare.Date.DayOfWeek == DayOfWeek.Friday)
                    {
                        selectedDates.Add(compare.Date);
                    }
                    else if (roomReservation.IsSaturday && compare.Date.DayOfWeek == DayOfWeek.Saturday)
                    {
                        selectedDates.Add(compare.Date);
                    }
                    else if (roomReservation.IsSunday && compare.Date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        selectedDates.Add(compare.Date);
                    }
                    compare = compare.AddDays(1);
                }
            }
            return selectedDates.ToList();
        }

        public List<RoomSlot> GetExaminationRoomSlots(long examinationReservationId)
        {
            var reservations = _db.RoomSlots.Where(x => x.ExaminationReservationId == examinationReservationId
                                                        && x.IsCancel == false)
                                            .ToList();
            return reservations;
        }

        public bool CancelRoomSlot(List<RoomSlot> roomSlots)
        {
            try
            {
                roomSlots.Select(x =>
                {
                    x.IsCancel = true;
                    return x;
                })
                         .ToList();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public ExaminationReservation GetExaminationReservation(long id)
        {
            var model = _db.ExaminationReservations.Include(x => x.Term)
                                                       .ThenInclude(x => x.AcademicLevel)
                                                   .Include(x => x.Section)
                                                       .ThenInclude(x => x.Course)
                                                   .Include(x => x.Room)
                                                   .Include(x => x.ExaminationType)
                                                   .IgnoreQueryFilters()
                                                   .SingleOrDefault(x => x.Id == id);

            model.AcademicLevelId = model.Term.AcademicLevelId;
            model.CourseId = model.Section.CourseId;
            model.StartTimeText = model.StartTime.ToString(StringFormat.TimeSpan);
            model.EndTimeText = model.EndTime.ToString(StringFormat.TimeSpan);
            return model;
        }

        public bool IsExistedExaminationReservation(long sectionId, long examinationTypeId, long id = 0)
        {
            var isExisted = _db.ExaminationReservations.Any(x => x.Id != id
                                                                 && x.SectionId == sectionId
                                                                 && x.ExaminationTypeId == examinationTypeId
                                                                 && (x.Status == "a" || x.Status == "w"));

            return isExisted;
        }

        public bool IsMeetingRoom(long roomId)
        {
            return _db.Rooms.Any(x => x.Id == roomId
                                      && x.RoomType.Name == "Meeting Room");
        }

        public bool IsPeriodReservationExisted(RoomReservation model)
        {
            // var days = _db.Calendars.Where(x => x.Date >= model.DateFrom
            //                                     && x.Date <= model.DateTo);

            // var selectedDates = new List<DateTime>();
            var dateNow = DateTime.UtcNow.AddHours(7).Date;
            bool isOutOfPeriod = _db.ReservationCalendars.Any(x => x.StartedAt <= dateNow
                                                                 && x.EndedAt >= dateNow
                                                                 && x.IsActive);

            return isOutOfPeriod;
            // if (model.DateFrom.Date == model.DateTo.Date)
            // {
            //     selectedDates.Add(model.DateFrom);
            // }
            // else
            // {
            //     foreach (var day in days)
            //     {
            //         if (model.IsMonday && day.Date.DayOfWeek == DayOfWeek.Monday)
            //         {
            //             selectedDates.Add(day.Date);
            //         }
            //         else if (model.IsTuesday && day.Date.DayOfWeek == DayOfWeek.Tuesday)
            //         {
            //             selectedDates.Add(day.Date);
            //         }
            //         else if (model.IsWednesday && day.Date.DayOfWeek == DayOfWeek.Wednesday)
            //         {
            //             selectedDates.Add(day.Date);
            //         }
            //         else if (model.IsThursday && day.Date.DayOfWeek == DayOfWeek.Thursday)
            //         {
            //             selectedDates.Add(day.Date);
            //         }
            //         else if (model.IsFriday && day.Date.DayOfWeek == DayOfWeek.Friday)
            //         {
            //             selectedDates.Add(day.Date);
            //         }
            //         else if (model.IsSaturday && day.Date.DayOfWeek == DayOfWeek.Saturday)
            //         {
            //             selectedDates.Add(day.Date);
            //         }
            //         else if (model.IsSunday && day.Date.DayOfWeek == DayOfWeek.Sunday)
            //         {
            //             selectedDates.Add(day.Date);
            //         }
            //     }
            // }

            // foreach (var selectedDate in selectedDates)
            // {
            //     isOutOfPeriod = _db.ReservationCalendars.Any(x => x.StartedAt <= dateNow
            //                                                      && x.EndedAt >= dateNow
            //                                                      && x.IsActive);

            //     if (isOutOfPeriod)
            //     {
            //         return isOutOfPeriod;
            //     }
            // }

            // return isOutOfPeriod;
        }

        public UpdateExaminationReservationViewModel UpdateExaminationReservation(ExaminationReservation model)
        {
            var result = new UpdateExaminationReservationViewModel { };
            var exam = _db.ExaminationReservations.Where(x => x.SectionId == model.SectionId && x.ExaminationTypeId == model.ExaminationTypeId).FirstOrDefault();

            if (model.StartTime > model.EndTime)
            {
                result.Message = $"Error!, Start Time {model.StartTime} must less than {model.EndTime}";
                result.Status = UpdateExamStatus.SaveExamFail;
                return result;
            }
            
            if (!model.AbsentInstructor)
            {
                //check overlap section slot
                if (_sectionProvider.IsSectionSlotExisted(model.SectionId, model.InstructorId ?? 0, model.RoomId ?? 0, model.Date, model.StartTime, model.EndTime))
                {
                    result.Message = $"Warning!, There is overlap section slot instructor for {model.DateText} at {model.StartTime} to {model.EndTime}";
                    result.Status = UpdateExamStatus.OverlapSectionSlot;
                    if (model.SenderType == "api")
                    {
                        return result;
                    }
                }

                if (model.SenderType == "api")
                {
                    if (IsExamExisted(exam?.Id ?? 0, model.ExaminationTypeId, model.InstructorId ?? 0, model.RoomId ?? 0, model.Date, model.StartTime, model.EndTime))
                    {
                        result.ExaminationReservation = exam;
                        result.Status = UpdateExamStatus.OverlapExam;
                        return result;
                    }
                    //Check instructor 
                    if (model.InstructorId > 0)
                    {
                        if (_sectionProvider.IsSectionSlotExisted(model.InstructorId ?? 0, model.RoomId ?? 0, model.Date, model.StartTime, model.EndTime))
                        {
                            result.Message = $"Warning!, There is overlap section slot instructor for {model.DateText} at {model.StartTime} to {model.EndTime}";
                            result.Status = UpdateExamStatus.OverlapSectionSlot;
                            return result;
                        }
                        //API mode will check if instuctor are booking any others examreservation in the same time
                        bool isOtherExamInstructorAsProctor = IsOtherExamReservationExist(model.InstructorId.Value, model.Date, model.StartTime, model.EndTime, model.SectionId);
                        if (isOtherExamInstructorAsProctor)
                        {
                            result.Message = $"Warning!, There is overlap exam slot by instructor for {model.DateText} at {model.StartTime} to {model.EndTime}";
                            result.Status = UpdateExamStatus.OverlapSectionSlot;
                            return result;
                        }
                    }
                }

                if (IsExamExistedOnlyApproval(exam?.Id ?? 0, model.ExaminationTypeId, model.InstructorId ?? 0, model.RoomId ?? 0, model.Date, model.StartTime, model.EndTime))
                {
                    result.Status = UpdateExamStatus.OverlapExamOnlyStatusApproval;
                    return result;
                }

            }
            try
            {
                string examinationType = _db.ExaminationTypes.SingleOrDefault(x => x.Id == model.ExaminationTypeId).NameEn.ToLower();
                if (exam != null)
                {
                    // CreateSelectList(model.Date, model.StartTimeText, model.EndTimeText, model.AcademicLevelId, model.TermId, model.CourseId);
                    if (exam.Status == "a")
                    {
                        result.Status = UpdateExamStatus.ExaminationAlreadyApproved;
                        result.Message = "Examination is already approved";
                        result.ExaminationReservation = exam;
                        return result;
                    }
                    else
                    {
                        try
                        {
                            if (examinationType.Contains("final") && model.Status == "c")
                            {
                                exam.StudentRemark = model.StudentRemark;
                                exam.ProctorRemark = model.ProctorRemark;
                                exam.AbsentInstructor = model.AbsentInstructor;
                                exam.AllowBooklet = model.AllowBooklet;
                                exam.AllowCalculator = model.AllowCalculator;
                                exam.AllowAppendix = model.AllowAppendix;
                                exam.AllowOpenbook = model.AllowOpenbook;
                                exam.Status = model.Status;
                                exam.UseProctor = model.UseProctor;
                                exam.TotalProctor = model.TotalProctor;
                                _db.SaveChanges();
                            }
                            else
                            {
                                exam.Date = model.Date;
                                exam.StartTime = model.StartTime;
                                exam.EndTime = model.EndTime;
                                exam.UseProctor = model.UseProctor;
                                exam.SenderType = model.SenderType;
                                exam.StudentRemark = model.StudentRemark;
                                exam.ProctorRemark = model.ProctorRemark;
                                exam.AbsentInstructor = model.AbsentInstructor;
                                exam.AllowBooklet = model.AllowBooklet;
                                exam.AllowCalculator = model.AllowCalculator;
                                exam.AllowAppendix = model.AllowAppendix;
                                exam.AllowOpenbook = model.AllowOpenbook;
                                exam.TotalProctor = model.TotalProctor;
                                exam.Status = model.Status;
                                if (model.InstructorId > 0)
                                {
                                    exam.InstructorId = model.InstructorId;
                                }
                                if (model.RoomId > 0)
                                {
                                    exam.RoomId = model.RoomId;
                                }
                                _db.SaveChanges();
                            }
                            result.Status = UpdateExamStatus.UpdateExamSuccess;
                            result.ExaminationReservation = exam;

                        }
                        catch
                        {
                            result.Status = UpdateExamStatus.UpdateExamFail;
                            return result;
                        }
                        // _flashMessage.Warning("Please edit again by function \"Edit\"");
                        // return RedirectToAction(nameof(Edit), new { id = examId });
                    }
                }
                else
                {
                    exam = new ExaminationReservation
                    {
                        TermId = model.TermId,
                        SectionId = model.SectionId,
                        ExaminationTypeId = model.ExaminationTypeId,
                        Date = model.Date,
                        StartTime = model.StartTime,
                        EndTime = model.EndTime,
                        UseProctor = model.UseProctor,
                        Status = model.Status,
                        SenderType = model.SenderType,
                        StudentRemark = model.StudentRemark,
                        ProctorRemark = model.ProctorRemark,
                        AbsentInstructor = model.AbsentInstructor,
                        AllowBooklet = model.AllowBooklet,
                        AllowCalculator = model.AllowCalculator,
                        AllowAppendix = model.AllowAppendix,
                        AllowOpenbook = model.AllowOpenbook,
                        TotalProctor = model.TotalProctor
                    };
                    if (model.InstructorId > 0)
                    {
                        exam.InstructorId = model.InstructorId;
                    }
                    if (model.RoomId > 0)
                    {
                        exam.RoomId = model.RoomId;
                    }
                    _db.ExaminationReservations.Add(exam);
                    result.Status = UpdateExamStatus.SaveExamSucceed;
                    _db.SaveChanges();
                    result.ExaminationReservation = exam;
                }

                if (exam.Status == "r")
                {
                    var cancelExaminationRoomSlot = GetExaminationRoomSlots(exam.Id);
                    var cancel = CancelRoomSlot(cancelExaminationRoomSlot);

                    // Update Section       
                    var section = _db.Sections.SingleOrDefault(x => x.Id == model.SectionId);
                    var jointSections = _db.Sections.Where(x => x.ParentSectionId == section.Id).ToList();
                    if (examinationType.Contains("midterm"))
                    {
                        section.MidtermDate = null;
                        section.MidtermStart = null;
                        section.MidtermEnd = null;
                        section.MidtermRoomId = null;
                        if (model.SenderType == "api")
                        {
                            section.IsDisabledMidterm = true;
                        }

                        if (jointSections != null && jointSections.Any())
                        {
                            foreach (var jointSection in jointSections)
                            {
                                jointSection.MidtermDate = null;
                                jointSection.MidtermStart = null;
                                jointSection.MidtermEnd = null;
                                jointSection.MidtermRoomId = null;
                                if (model.SenderType == "api")
                                {
                                    jointSection.IsDisabledMidterm = true;
                                }
                            }
                        }
                    }
                    else if (examinationType.Contains("final"))
                    {
                        section.FinalDate = null;
                        section.FinalStart = null;
                        section.FinalEnd = null;
                        section.FinalRoomId = null;
                        if (model.SenderType == "api")
                        {
                            section.IsDisabledFinal = true;
                        }

                        if (jointSections != null && jointSections.Any())
                        {
                            foreach (var jointSection in jointSections)
                            {
                                jointSection.FinalDate = null;
                                jointSection.FinalStart = null;
                                jointSection.FinalEnd = null;
                                jointSection.FinalRoomId = null;
                                if (model.SenderType == "api")
                                {
                                    jointSection.IsDisabledFinal = true;
                                }
                            }
                        }
                    }

                    _db.SaveChanges();
                }
                else if (exam.Status == "w" || (exam.Status == "c" && examinationType.Contains("midterm")))
                {
                    // Update Examination Date in Section
                    var section = _db.Sections.SingleOrDefault(x => x.Id == exam.SectionId);
                    var jointSections = _db.Sections.Where(x => x.ParentSectionId == section.Id).ToList();
                    if (examinationType.Contains("midterm"))
                    {
                        section.MidtermDate = exam.Date;
                        section.MidtermStart = exam.StartTime;
                        section.MidtermEnd = exam.EndTime;
                        section.MidtermRoomId = exam.RoomId;

                        if (jointSections != null && jointSections.Any())
                        {
                            foreach (var jointSection in jointSections)
                            {
                                jointSection.MidtermDate = exam.Date;
                                jointSection.MidtermStart = exam.StartTime;
                                jointSection.MidtermEnd = exam.EndTime;
                                jointSection.MidtermRoomId = exam.RoomId;
                            }
                        }
                    }
                    else if (examinationType.Contains("final"))
                    {
                        section.FinalDate = exam.Date;
                        section.FinalStart = exam.StartTime;
                        section.FinalEnd = exam.EndTime;
                        section.FinalRoomId = exam.RoomId;
                        if (jointSections != null && jointSections.Any())
                        {
                            foreach (var jointSection in jointSections)
                            {
                                jointSection.FinalDate = exam.Date;
                                jointSection.FinalStart = exam.StartTime;
                                jointSection.FinalEnd = exam.EndTime;
                                jointSection.FinalRoomId = exam.RoomId;
                            }
                        }
                    }
                    _db.SaveChanges();
                }
            }
            catch
            {
                result.Status = UpdateExamStatus.SaveExamFail;
            }
            return result;
        }

        public bool IsOtherExamReservationExist(long instructorId, DateTime date, TimeSpan startTime, TimeSpan endTime, long notInSectionId)
        {
            return _db.ExaminationReservations.Any(x => x.InstructorId == instructorId
                                                          && x.Date == date
                                                          && x.StartTime < endTime
                                                          && x.EndTime > startTime
                                                          && x.Status != "r"
                                                          && (notInSectionId == 0 || x.SectionId != notInSectionId)
                                                          && !x.AbsentInstructor
                                                  );
        }

        public bool IsExamReservationExist(long sectionId, long instructorId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            return _db.ExaminationReservations.Any(x => x.SectionId == sectionId
                                                          && x.Date == date
                                                          && x.StartTime < endTime
                                                          && x.EndTime > startTime
                                                          && x.Status != "r"
                                                          && (instructorId == 0 || x.InstructorId == instructorId)
                                                  );
        }

        public AllSectionTimeSlotReportViewModel GetAllSectionTimeSlotReport(Criteria criteria)
        {
            AllSectionTimeSlotReportViewModel report = new AllSectionTimeSlotReportViewModel()
            {
                Criteria = criteria,
            };
            DateTime? startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);
            if (startedAt == null || endedAt == null)
            {
                throw new Exception("Started At or Ended At Required");
            }
            if (startedAt > endedAt)
            {
                throw new Exception("Started At must less than endedAt");
            }
            startedAt = startedAt.Value.Date;
            endedAt = endedAt.Value.Date;

            var allRoomSlotOnTime = _db.RoomSlots.AsNoTracking()
                                                 .Where(x => x.IsActive
                                                                && !x.IsCancel
                                                                && x.Date >= startedAt
                                                                && x.Date <= endedAt
                                                                && (criteria.CampusId == 0 || x.Room.Building.CampusId == criteria.CampusId)
                                                                && (criteria.BuildingId == 0 || x.Room.BuildingId == criteria.BuildingId)
                                                       )
                                                 .ToList();


            var forcedShowUsingType = new List<string> { "Studying", "Activity" , "Examination", "Meeting"
                //, "N/A"
            };
            var dayOfWeeks = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().ToList();

            int startHour = 8;
            int endHour = 21;
            int interval = 1;

            foreach (var usingType in forcedShowUsingType)
            {
                var subReport = new AllSectionTimeSlotReportViewModel.AllSectionTimeSlotByUsingTypeReportItem();

                var allRoomSlotInUsingType = allRoomSlotOnTime.Where(x => x.UsingTypeText == usingType)
                                                              .ToList();
                subReport.UsingTypeText = usingType;
                var isGenerateHeader = false;

                for (int dayOfWeekIndex = 0; dayOfWeekIndex < dayOfWeeks.Count; ++dayOfWeekIndex)
                {
                    var allRoomSlotInDayOfWeek = allRoomSlotInUsingType.Where(x => x.Day == dayOfWeekIndex)
                                                                       .ToList();

                    var rowData = new AllSectionTimeSlotReportViewModel.AllSectionTimeSlotByUsingTypeReportItem.RowData()
                    {
                        DayOfWeekText = dayOfWeeks[dayOfWeekIndex].ToString("f").Substring(0, 3)
                    };

                    for (int hourIndex = startHour; hourIndex < endHour; hourIndex += interval)
                    {
                        TimeSpan timeFrom = new TimeSpan(hourIndex, 0, 0);
                        TimeSpan timeTo = new TimeSpan(hourIndex + interval, 0, 0);

                        if (!isGenerateHeader)
                        {
                            subReport.Header.Add($"{timeFrom:h\\:mm}-{timeTo:h\\:mm}");
                        }

                        var countRoomUsedInTimeSlot = allRoomSlotInDayOfWeek.Count(x => x.StartTime < timeTo && timeFrom < x.EndTime);
                        rowData.Values.Add(countRoomUsedInTimeSlot);
                    }
                    isGenerateHeader = true;

                    subReport.Rows.Add(rowData);
                }

                report.Items.Add(subReport);
            }

            return report;
        }

        public List<ExportListReportViewModel> GetExaminationListReport(Criteria criteria)
        {
            var selectExamType = _db.ExaminationTypes.AsNoTracking().FirstOrDefault(x => x.Id == criteria.ExaminationTypeId);
            var final = _db.ExaminationTypes.AsNoTracking().FirstOrDefault(x => x.NameEn.Contains("final"));
            var midterm = _db.ExaminationTypes.AsNoTracking().FirstOrDefault(x => x.NameEn.Contains("midterm"));
            if (final == null || midterm == null)
            {
                throw new Exception("Config wrong");
            }
            if (selectExamType == null)
            {
                throw new ArgumentException();
            }
            var isMidTerm = selectExamType.Id == midterm.Id;
            var isFinal = selectExamType.Id == final.Id;

            var allSections = _db.Sections.AsNoTracking()
                                          .Include(x => x.Term)
                                          .Include(x => x.Course)
                                              .ThenInclude(x => x.Faculty)
                                          .Include(x => x.ParentSection)
                                              .ThenInclude(x => x.Course)
                                          .Include(x => x.MainInstructor)
                                              .ThenInclude(x => x.Title)
                                          .Where(x => x.TermId == criteria.TermId
                                                        && x.Status != "r"
                                                        && !x.IsClosed
                                                        && (criteria.CourseId == 0 
                                                                || x.CourseId == criteria.CourseId 
                                                                || x.ParentSection.CourseId == criteria.CourseId
                                                                || x.ChildrenSections.Any(y => y.CourseId == criteria.CourseId)
                                                                )
                                                        && (criteria.SectionId == 0 
                                                                || x.Id == criteria.SectionId 
                                                                || x.ParentSectionId == criteria.SectionId
                                                                || x.ChildrenSections.Any(y => y.Id == criteria.SectionId)
                                                                )
                                                        && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                            || x.Course.Code.Contains(criteria.CodeAndName)
                                                            || x.Course.NameEn.Contains(criteria.CodeAndName))
                                                        && (string.IsNullOrEmpty(criteria.SectionNumber)
                                                            || x.Number.Contains(criteria.SectionNumber))
                                                  )
                                          .ToList();

            var allSectionIds = allSections.Select(x => x.Id).ToList();

            var allExamReservations = _db.ExaminationReservations.AsNoTracking()
                                                                 .Include(x => x.Term)
                                                                 .Include(x => x.Room)
                                                                    .ThenInclude(x => x.Building)
                                                                 .Include(x => x.Section)
                                                                    .ThenInclude(x => x.Course)
                                                                        .ThenInclude(x => x.Faculty)
                                                                 .Include(x => x.Section)
                                                                    .ThenInclude(x => x.ParentSection)
                                                                        .ThenInclude(x => x.Course)
                                                                 .Include(x => x.Instructor)
                                                                    .ThenInclude(x => x.Title)
                                                                 .Where(x => x.TermId == criteria.TermId
                                                                                && x.Status != "r"
                                                                                && allSectionIds.Contains(x.SectionId)
                                                                                && x.ExaminationTypeId == criteria.ExaminationTypeId)
                                                                 .ToList();
            var allExamReservationSectionIds = allExamReservations.Select(x => x.SectionId).ToList();

            DateTime? starteAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);

            var noExamReservationSections = allSections.Where(x => !allExamReservationSectionIds.Contains(x.Id))
                                                       .ToList();

            if (isMidTerm)
            {
                noExamReservationSections = noExamReservationSections.Where(x => (starteAt == null || !x.MidtermDate.HasValue || x.MidtermDate.Value.Date >= starteAt.Value.Date)
                                                                                    && (endedAt == null || !x.MidtermDate.HasValue || x.MidtermDate.Value.Date <= endedAt.Value.Date))
                                                                     .ToList();
            }
            else if (isFinal)
            {
                noExamReservationSections = noExamReservationSections.Where(x => (starteAt == null || !x.FinalDate.HasValue || x.FinalDate.Value.Date >= starteAt.Value.Date)
                                                                                   && (endedAt == null || !x.FinalDate.HasValue || x.FinalDate.Value.Date <= endedAt.Value.Date))
                                                                     .ToList();
            }

            var filterAllExamReservation = allExamReservations.Where(x => (criteria.CampusId == 0
                                                                                  || (x.Room != null && x.Room.Building != null && x.Room.Building.CampusId == criteria.CampusId))
                                                                              && (criteria.BuildingId == 0
                                                                                  || (x.Room != null && x.Room.BuildingId == criteria.BuildingId))
                                                                              && (criteria.Floor == null
                                                                                  || (x.Room != null && x.Room.Floor == criteria.Floor))
                                                                              && (string.IsNullOrEmpty(criteria.RoomName)
                                                                                  || (x.Room != null && !string.IsNullOrEmpty(x.Room.NameEn) && x.Room.NameEn.Contains(criteria.RoomName)))
                                                                              )
                                                              .ToList();

            var result = filterAllExamReservation.Select(x => new ExportListReportViewModel
            {
                AbsentInstructor = x.AbsentInstructor,
                AllowAppendix = x.AllowAppendix,
                AllowBooklet = x.AllowBooklet,
                AllowCalculator = x.AllowCalculator,
                AllowFomulaSheet = false,
                AllowOpenbook = x.AllowOpenbook,
                ApprovedAt = x.ApprovedAt,
                ApprovedBy = x.ApprovedBy,
                CourseId = x.Section.CourseId,
                CourseCode = x.Section.Course.Code,
                CourseCredit = x.Section.Course.Credit,
                CourseLab = x.Section.Course.Lab,
                CourseLecture = x.Section.Course.Lecture,
                CourseName = x.Section.Course.NameEn,
                CourseOther = x.Section.Course.Other,
                Date = x.Date,
                Division = x.Section.Course.Faculty?.Abbreviation ?? "",
                EndTime = x.EndTime,
                ExaminationType = selectExamType.NameEn,
                HeadParentSectionId = x.Section.ParentSectionId == null ? x.SectionId : x.Section.ParentSectionId,
                Id = x.Id,
                InstructorFullNameEn = x.Instructor == null ? "" : x.Instructor.Title.NameEn + " " + x.Instructor.FirstNameEn + " " + x.Instructor.LastNameEn,
                IsClosed = x.Section.IsClosed,
                IsDisabledFinal = x.Section.IsDisabledFinal,
                IsDisabledMidterm = x.Section.IsDisabledMidterm,
                IsOutbound = x.Section.IsOutbound,
                IsSpecialCase = x.Section.IsSpecialCase,
                ParentSeatUsed = x.Section.ParentSection == null ? 0 : x.Section.ParentSection.SeatUsed,
                ParentSectionCourseCode = x.Section.ParentSection == null ? "" : x.Section.ParentSection.Course.Code,
                ParentSectionId = x.Section.ParentSectionId,
                ParentSectionNumber = x.Section.ParentSection == null ? "" : x.Section.ParentSection.Number,
                ProctorName = "",
                Room = x.Room == null ? "" : x.Room.NameEn,
                RoomId = x.RoomId,
                SeatUsed = x.Section.SeatUsed,
                SectionId = x.SectionId,
                SectionNumber = x.Section.Number,
                SenderTypeText = x.SenderTypeText,
                StartTime = x.StartTime,
                Status = x.Status,
                StudentRemark = x.StudentRemark,
                TermText = x.Term.AcademicTerm + "/" + x.Term.AcademicYear,
                TotalProctor = x.TotalProctor,
                UseProctor = x.UseProctor,
            }).ToList();

            var noExamReservationSectionsReseult = noExamReservationSections.Select(x => new ExportListReportViewModel
            {
                CourseId = x.CourseId,
                CourseCode = x.Course.Code,
                CourseCredit = x.Course.Credit,
                CourseLab = x.Course.Lab,
                CourseLecture = x.Course.Lecture,
                CourseName = x.Course.NameEn,
                CourseOther = x.Course.Other,
                Date = isMidTerm ? x.MidtermDate : x.FinalDate,
                Division = x.Course.Faculty?.Abbreviation ?? "",
                EndTime = isMidTerm ? x.MidtermEnd : x.FinalEnd,
                ExaminationType = selectExamType.NameEn,
                HeadParentSectionId = x.ParentSectionId == null ? x.Id : x.ParentSectionId,
                Id = 0,
                InstructorFullNameEn = x.MainInstructor == null ? "" : x.MainInstructor.Title.NameEn + " " + x.MainInstructor.FirstNameEn + " " + x.MainInstructor.LastNameEn,
                IsClosed = x.IsClosed,
                IsDisabledFinal = x.IsDisabledFinal,
                IsDisabledMidterm = x.IsDisabledMidterm,
                IsOutbound = x.IsOutbound,
                IsSpecialCase = x.IsSpecialCase,
                ParentSeatUsed = x.ParentSection == null ? 0 : x.ParentSection.SeatUsed,
                ParentSectionCourseCode = x.ParentSection == null ? "" : x.ParentSection.Course.Code,
                ParentSectionId = x.ParentSectionId,
                ParentSectionNumber = x.ParentSection == null ? "" : x.ParentSection.Number,
                ProctorName = "",
                //Room = x.Room.NameEn,
                //RoomId = x.Room.Id,
                SeatUsed = x.SeatUsed,
                SectionId = x.Id,
                SectionNumber = x.Number,
                SenderTypeText = "",
                StartTime = isMidTerm ? x.MidtermStart : x.FinalStart,
                Status = isMidTerm ? (x.IsDisabledMidterm ? "ne" : "nr") : (x.IsDisabledFinal ? "ne" : "nd"),
                StudentRemark = "",
                TermText = x.Term.AcademicTerm + "/" + x.Term.AcademicYear,
                //TotalProctor = x.TotalProctor,
                //UseProctor = x.UseProctor,
            }).ToList();
            result.AddRange(noExamReservationSectionsReseult);

            var jointSections = allSections.Where(x => x.ParentSectionId.HasValue
                                                         && x.ParentSectionId > 0)
                                           .ToList();
            var parentSectionIds = allSections.Where(x => x.ParentSectionId != null)
                                              .Select(x => x.ParentSectionId)
                                              .ToList();
            var jointTotalSeatUsed = _db.Sections.Where(x => parentSectionIds.Contains(x.ParentSectionId))
                                                 .Select(x => new
                                                 {
                                                     Id = x.Id,
                                                     ParentSectionId = x.ParentSectionId,
                                                     SeatUsed = x.SeatUsed
                                                 })
                                                 .ToList();

            foreach (var item in result)
            {

                item.JointSections = jointSections.Where(x => x.ParentSectionId == item.SectionId)
                                                  .Select(x => new JointSectionCourseToBeOfferedViewModel
                                                  {
                                                      Id = x.Id,
                                                      ParentSectionId = x.ParentSectionId,
                                                      SeatUsed = x.SeatUsed,
                                                      Number = x.Number,
                                                      CourseCode = x.Course.CodeAndSpecialChar,
                                                  })
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
                    var parent = result.FirstOrDefault(x => x.SectionId == item.ParentSectionId);
                    do
                    {
                        if (parent != null)
                        {
                            item.Date = parent.Date;
                            item.StartTime = parent.StartTime;
                            item.EndTime = parent.EndTime;

                            item.Room = parent.Room;
                            item.RoomId = parent.RoomId;
                            item.UseProctor = parent.UseProctor;
                            item.ProctorName = parent.ProctorName;
                            item.TotalProctor = parent.TotalProctor;
                            item.AbsentInstructor = parent.AbsentInstructor;
                            item.AllowOpenbook = parent.AllowOpenbook;
                            item.AllowCalculator = parent.AllowCalculator;
                            item.AllowFomulaSheet = parent.AllowFomulaSheet;
                            item.AllowAppendix = parent.AllowAppendix;
                            item.AllowBooklet = parent.AllowBooklet;
                            item.StudentRemark = parent.StudentRemark;
                            item.Status = parent.Status;

                            if (!string.IsNullOrEmpty(parent.InstructorFullNameEn))
                            {
                                item.InstructorFullNameEn = parent.InstructorFullNameEn;
                            }

                            parent = result.FirstOrDefault(x => parent.ParentSectionId != null && parent.ParentSectionId > 0 && x.SectionId == parent.ParentSectionId);
                        }
                    } while (parent != null);
                }
            }

            result = result.Where(x => (starteAt == null || !x.Date.HasValue || x.Date >= starteAt.Value.Date)
                                           && (endedAt == null || !x.Date.HasValue || x.Date <= endedAt.Value.Date)
                                           && (criteria.CourseId == 0 || x.CourseId == criteria.CourseId)
                                           && (criteria.SectionId == 0 || x.Id == criteria.SectionId)
                                           )
                           .ToList();

            if (criteria.CourseId > 0)
            {
                result = result.Select(x =>
                {
                    x.ForceShowStatus = true;
                    return x;
                }).ToList();
            }

            result = result.OrderBy(x => x.Date ?? DateTime.MaxValue)
                           .ThenBy(x => x.StartTime ?? TimeSpan.MaxValue)
                           .ThenBy(x => x.EndTime)
                           .ThenBy(x => x.HeadParentSectionId)
                           .ThenBy(x => x.ParentSectionId ?? 0)
                           .ToList();

            return result;
        }
    }
}