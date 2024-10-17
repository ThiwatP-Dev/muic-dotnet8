using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.Schedules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace KeystoneLibrary.Providers
{
    public class SectionProvider : BaseProvider, ISectionProvider
    {
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;
        public SectionProvider(ApplicationDbContext db, 
                               IMapper mapper, 
                               IRegistrationProvider registrationProvider,
                               IDateTimeProvider dateTimeProvider) : base(db, mapper)
        {
            _registrationProvider = registrationProvider;
            _dateTimeProvider = dateTimeProvider;
        }

        public SectionViewModel GetSectionBySemester(long sectionId, long termId)
        {
            var section = _db.Sections.Include(x => x.Course)
                                      .Include(x => x.SectionDetails)
                                      .Where(x => x.TermId == termId)
                                      .IgnoreQueryFilters()
                                      .SingleOrDefault(x => x.Id == sectionId);

            var sectionViewModel = new SectionViewModel();
            sectionViewModel = _mapper.Map<Section, SectionViewModel>(section);
            sectionViewModel.ClassSchedules = section.SectionDetails.Select(x => _mapper.Map<SectionDetail, ClassScheduleTimeViewModel>(x))
                                                                    .OrderBy(x => x.Day)
                                                                    .ToList();
            sectionViewModel.ExamSchedule = _mapper.Map<Section, ExaminationSchedule>(section);
            return sectionViewModel;
        }

        public Section GetSection(long id)
        {
            var section = _db.Sections.Include(x => x.Course)
                                      .Include(x => x.Term)
                                      .Include(x => x.SectionSlots)
                                          .ThenInclude(x => x.TeachingType)
                                      .Include(x => x.SectionSlots)
                                          .ThenInclude(x => x.Room)
                                      .Include(x => x.SectionSlots)
                                          .ThenInclude(x => x.Instructor)
                                      .Include(x => x.SectionDetails)
                                          .ThenInclude(x => x.TeachingType)
                                      .Include(x => x.SectionDetails)
                                          .ThenInclude(x => x.Instructor)
                                      .Include(x => x.SectionDetails)
                                          .ThenInclude(x => x.Room)
                                      .Include(x => x.ParentSection)
                                      .SingleOrDefault(x => x.Id == id);
            return section;
        }
        
        public bool CloseSection(Section section, out string errorMessage)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                var isCloseCompleted = CloseSection(section, out errorMessage, transaction);

                if (!isCloseCompleted)
                {
                    transaction.Rollback();
                }
                else
                {
                    transaction.Commit();
                }

                return isCloseCompleted;
            }
        }

        public bool CloseSection(Section section, out string errorMessage, IDbContextTransaction transaction)
        {
            try
            {
                section.Status = "c";
                section.IsClosed = true;
                section.ClosedSectionAt = DateTime.UtcNow;
                section.MidtermDate = null;
                section.MidtermStart = null;
                section.MidtermEnd = null;
                section.MidtermRoomId = null;
                section.FinalDate = null;
                section.FinalStart = null;
                section.FinalEnd = null;
                section.FinalRoomId = null;
                if (!CancelSectionRoomSlot(section.Id))
                {
                    errorMessage = Message.UnableToCancelSectionRoomSlot;
                    return false;
                }

                if (!CancelExaminationRoomSlot(section.Id))
                {
                    errorMessage = Message.UnableToCancelExaminationRoomSlot;
                    return false;
                }

                _db.SaveChanges();
                errorMessage = "";
                return true;
            }
            catch
            {
                errorMessage = Message.UnableToCancel;
                return false;
            }
        }

        public bool CancelSectionRoomSlot(long sectionId)
        {
            var sectionSlots = _db.SectionSlots.Where(x => x.SectionId == sectionId)
                                               .ToList();

            var sectionSlotIds = sectionSlots.Select(x => x.Id);
            var roomSlot = _db.RoomSlots.Where(x => sectionSlotIds.Contains(x.SectionSlotId ?? 0))
                                        .ToList();

            sectionSlots.Select(x => {
                                         x.Status = "c";
                                         return x;
                                     })
                        .ToList();

            roomSlot.Select(x => {
                                     x.IsCancel = true;
                                     x.IsActive = false;
                                     return x;
                                 })
                    .ToList();

            try
            {
                _db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CancelExaminationRoomSlot(long sectionId)
        {
            var examinationReservations = _db.ExaminationReservations.Where(x => x.SectionId == sectionId).ToList();
            var examinationSlotIds = examinationReservations.Select(x => x.Id);
            var roomSlot = _db.RoomSlots.Where(x => examinationSlotIds.Contains(x.ExaminationReservationId ?? 0))
                                        .ToList();

            examinationReservations.Select(x => {
                                                    x.Status = "r";
                                                    x.IsActive = false;
                                                    return x;
                                                })
                                   .ToList();

            roomSlot.Select(x => {
                                     x.IsCancel = true;
                                     x.IsActive = false;
                                     return x;
                                 })
                    .ToList();

            try
            {
                _db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool OpenSection(Section section)
        {
            try
            {
                section.IsClosed = false;
                section.OpenedSectionAt = DateTime.Now;

                if (!section.ParentSectionId.HasValue)
                {
                    var sectionSlot = _db.SectionSlots.Where(x => x.SectionId == section.Id)
                        .ToList();
                    if (sectionSlot.All(x => x.Status == "c"))
                    {
                        sectionSlot.Select(x => {
                            x.IsActive = false;
                            return x;
                        }).ToList();
                        // SECTION SLOT - ONLY PARENT SECTION
                        if (!section.ParentSectionId.HasValue
                            && section.TotalWeeks > 0
                            && section.SectionDetails.Any())
                        {
                            var sectionSlots = GenerateSectionSlots(section.TotalWeeks, section);
                            _db.SectionSlots.AddRange(sectionSlots);
                        }
                    }
                }

                _db.SaveChanges();
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<SectionSlot> GenerateSectionSlots(int totalWeeks, Section section)
        {
            List<SectionSlot> slots = new List<SectionSlot>();
            foreach (var detail in section.SectionDetails)
            {
                var holidays = _db.MuicHolidays.AsNoTracking()
                                               .Where(x => x.IsActive
                                                               && x.StartedAt <= section.OpenedAt.AddDays(totalWeeks*7)
                                                               && x.EndedAt >= section.OpenedAt)
                                               .Select(x => new
                                               {
                                                   x.StartedAt,
                                                   x.EndedAt
                                               })
                                               .Distinct()
                                               .ToList();

                var holidaysEachDate = new List<DateTime>();
                foreach (var holiday in holidays)
                {
                    for (DateTime date = holiday.StartedAt.Date; date <= holiday.EndedAt.Date; date = date.AddDays(1))
                    {
                        holidaysEachDate.Add(date);
                    }
                }
                holidaysEachDate = holidaysEachDate.Distinct().ToList();

                var calendars = _db.Calendars.Where(x => x.Date >= section.OpenedAt
                                                         && x.DayOfWeek == detail.Day
                                                   )
                                             .Take(totalWeeks)
                                             .ToList();

                slots.AddRange(calendars.Select(x => new SectionSlot
                                                     {
                                                         SectionId = section.Id,
                                                         TeachingTypeId = detail.TeachingTypeId,
                                                         Day = detail.Day,
                                                         Date = x.Date,
                                                         StartTime = detail.StartTime,
                                                         EndTime = detail.EndTime,
                                                         InstructorId = detail.InstructorId,
                                                         RoomId = detail.RoomId,
                                                         Status = holidaysEachDate.Contains(x.Date) ? "c" : "w",
                                                         Remark = holidaysEachDate.Contains(x.Date) ? "Auto Cancel as University Holiday" : "",
                                                         IsActive = true
                                                     }).ToList());
            }
            
            return slots.OrderBy(x => x.Date).ThenBy(x => x.StartTime).ToList();
        }

        public List<SectionSlot> GetSectionSlotsBySectionId(long sectionId)
        {
            var slots = _db.SectionSlots.Where(x => x.SectionId == sectionId)
                                        .OrderBy(x => x.Date)
                                            .ThenBy(x => x.StartTime)
                                        .ToList();
            
            return slots;
        }

        public List<SectionDetail> GetSectionDetailsBySectionId(long sectionId)
        {
            var sectionDetails = _db.SectionDetails.Where(x => x.SectionId == sectionId)
                                                   .OrderBy(x => x.Day)
                                                       .ThenBy(x => x.StartTime)
                                                   .ToList();
            
            return sectionDetails;
        }

        public List<RoomScheduleSectionDetailViewModel> GetSectionDetailsByRoomAndTerm(long roomId, long termId, string sectionStatus = "")
        {
            var sectionDetails = _db.SectionDetails.Where(x => x.RoomId == roomId
                                                               && x.Section.TermId == termId
                                                               && x.Section.ParentSectionId == null
                                                               && (string.IsNullOrEmpty(sectionStatus)
                                                                   || x.Section.Status == sectionStatus)
                                                               && !x.Section.IsClosed)
                                                   .Select(x => new RoomScheduleSectionDetailViewModel
                                                                {
                                                                    SectionId = x.SectionId,
                                                                    CourseCode = x.Section.Course.Code,
                                                                    CourseName = x.Section.Course.NameEn,
                                                                    SectionNumber = x.Section.Number,
                                                                    FinalDate = x.Section.FinalDate,
                                                                    MidtermDate = x.Section.MidtermDate,
                                                                    FinalStart = x.Section.FinalStart,
                                                                    FinalEnd = x.Section.FinalEnd,
                                                                    MidtermEnd = x.Section.MidtermEnd,
                                                                    MidtermStart = x.Section.MidtermStart,
                                                                    SeatUsed = x.Section.SeatUsed,
                                                                    TitleName = x.Section.MainInstructor.Title.NameEn,
                                                                    FirstNameEn = x.Section.MainInstructor.FirstNameEn,
                                                                    LastNameEn = x.Section.MainInstructor.LastNameEn,
                                                                    Day = x.Day,
                                                                    StartTime = x.StartTime,
                                                                    EndTime = x.EndTime,
                                                                    RoomName = x.Room.NameEn
                                                                })
                                                   .ToList();
            
            return sectionDetails;
        }
        
        public bool UpdateOpenedOrClosedAtSection(Section section, out string errorMessage)
        {
            // Close
            if (section.IsClosed)
            {
                var registrationCourse = _db.RegistrationCourses.Where(x => x.SectionId == section.Id
                                                                            && (x.Status == "a"
                                                                                || x.Status == "r"))
                                                                .ToList();
                if (registrationCourse != null || registrationCourse.Any())
                {
                    section.ClosedSectionAt = DateTime.UtcNow;
                    errorMessage = Message.StudentsInSection;
                    return false;
                }

                section.ClosedSectionAt = DateTime.UtcNow;
                errorMessage = "";

                // if (_registrationProvider.IsRegistrationPeriod(DateTime.UtcNow, section.TermId))
                // {
                    try
                    {
                        _registrationProvider.CallUSparkAPICloseSection(section.Id);
                    }
                    catch (Exception e)
                    {
                        errorMessage = e.Message;
                        return false;
                    }
                // }

                return true;
            }
            else // Open
            {
                var sectionToUpdate = _db.Sections.SingleOrDefault(x => x.Id == section.Id);
                if (sectionToUpdate != null && !sectionToUpdate.IsClosed) // Already Open
                {
                    errorMessage = "";
                    return true;
                }

                // if (_registrationProvider.IsRegistrationPeriod(DateTime.Now, section.TermId))
                // {
                    try
                    {
                        _registrationProvider.CallUSparkAPIOpenSection(section.Id);
                    }
                    catch (Exception e)
                    {
                        errorMessage = e.Message;
                        return false;
                    }
                // }

                section.OpenedSectionAt = DateTime.Now;
                errorMessage = "";
                return true;
            }
        }

        public bool HaveStudentsInSection(long sectionId)
        {
            var registrationCourse = _db.RegistrationCourses.Where(x => x.SectionId == sectionId
                                                                        && (x.Status == "a"
                                                                            || x.Status == "r"))
                                                            .ToList();
            return registrationCourse != null && registrationCourse.Any() ? false : true;
        }
        
        public bool IsSectionSlotExisted(long sectionId, long InstructorId, long roomId, DateTime date, TimeSpan? start, TimeSpan? end)
        {
            return _db.SectionSlots.Any(x => x.SectionId == sectionId
                                             && x.Date == date
                                             && x.StartTime < end
                                             && x.EndTime > start
                                             && x.Status != "c"
                                             && (InstructorId == 0 || x.InstructorId == InstructorId)
                                             && (roomId == 0 || x.RoomId == roomId));
        }

        public bool IsSectionSlotExisted(long InstructorId, long roomId, DateTime date, TimeSpan? start, TimeSpan? end)
        {
            return _db.SectionSlots.Any(x => x.Date == date
                                             && x.StartTime < end
                                             && x.EndTime > start
                                             && (InstructorId == 0 || x.InstructorId == InstructorId)
                                             && (roomId == 0 || x.RoomId == roomId)
                                             && x.Status != "c");
        }

        public List<Section> GetJointSections(long sectionId)
        {
            var sections = _db.Sections.Where(x => x.ParentSectionId == sectionId)
                                       .ToList();
            return sections;
        }

        public string GetNextSectionNumber(long courseId, long termId)
        {
            var sectionNumber = (_db.Sections.Where(x => x.CourseId == courseId
                                                         && x.TermId == termId)
                                             .Max(x => (int?)x.NumberValue) ?? 0) + 1;
            return Convert.ToString(sectionNumber);
        }

        public void ReCalculateSeatAvailable(long sectionId)
        {
            var section = _db.Sections.SingleOrDefault(x => x.Id == sectionId);
            long masterSectionId = section.ParentSectionId.HasValue ? section.ParentSectionId.Value : section.Id;
            var masterSection = _db.Sections.SingleOrDefault(x => x.Id == masterSectionId);
            int seatUsedMaster = masterSection != null ? masterSection.SeatUsed : 0;

            var jointSections = _db.Sections.Where(x =>x.ParentSectionId == masterSectionId).ToList();
            int seatUsedForJoints = (jointSections != null && jointSections.Any()) ? jointSections.Sum(x => x.SeatUsed) : 0;

            // Update Master Section
            if(masterSection != null)
            {         
                masterSection.SeatAvailable = masterSection.SeatLimit - (seatUsedMaster + seatUsedForJoints);
                // Update Joint Section
                if(jointSections != null && jointSections.Any())
                {
                    foreach (var jointSection in jointSections)
                    {
                        jointSection.SeatAvailable = jointSection.SeatLimit - jointSection.SeatUsed;
                        if(jointSection.SeatAvailable > masterSection.SeatAvailable)
                        {
                            jointSection.SeatAvailable = masterSection.SeatAvailable;
                        }
                    }
                }
                 _db.SaveChanges();
            } 
        }

        public List<SectionReportViewModel> GetSectionReportViewModel(Criteria criteria)
        {
            DateTime? startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);
            var sections = _db.Sections.Where(x => x.TermId == criteria.TermId
                                                   && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                       || x.Course.Code.Contains(criteria.CodeAndName)
                                                       || x.Course.NameEn.Contains(criteria.CodeAndName)
                                                       || x.Course.NameTh.Contains(criteria.CodeAndName)
                                                       || (criteria.CodeAndName.Contains("*")
                                                           && x.Course.CourseRateId == 2))
                                                   && (criteria.FacultyId == 0
                                                       || x.Course.FacultyId == criteria.FacultyId)
                                                   && (string.IsNullOrEmpty(criteria.IsClosed)
                                                       || x.IsClosed == Convert.ToBoolean(criteria.IsClosed))
                                                   && (string.IsNullOrEmpty(criteria.SeatAvailable)
                                                       || (Convert.ToBoolean(criteria.SeatAvailable) ? x.SeatAvailable > 0
                                                                                                       : x.SeatAvailable == 0))
                                                   && (string.IsNullOrEmpty(criteria.HaveMidterm)
                                                       || x.MidtermDate != null == Convert.ToBoolean(criteria.HaveMidterm))
                                                   && (string.IsNullOrEmpty(criteria.HaveFinal)
                                                       || x.FinalDate != null == Convert.ToBoolean(criteria.HaveFinal))
                                                   && (string.IsNullOrEmpty(criteria.Status)
                                                       || x.Status == criteria.Status)
                                                   && (startedAt == null
                                                       || x.OpenedSectionAt.Value.Date >= startedAt.Value.Date)
                                                   && (endedAt == null
                                                       || x.OpenedSectionAt.Value.Date <= endedAt.Value.Date)
                                                   && (string.IsNullOrEmpty(criteria.CreatedBy)
                                                       || x.CreatedBy == criteria.CreatedBy)
                                                   && (string.IsNullOrEmpty(criteria.SectionType)
                                                       || (criteria.SectionType == "o" ? x.IsOutbound
                                                                                       : criteria.SectionType == "g"
                                                                                       ? x.IsSpecialCase
                                                                                       : criteria.SectionType == "j" 
                                                                                       ? x.ParentSectionId != null
                                                                                       : x.ParentSectionId == null)))
                                       .Select(x => new SectionReportViewModel
                                                    {
                                                        Id = x.Id,
                                                        CourseCode = x.Course.Code,
                                                        CourseNameEn = x.Course.NameEn,
                                                        CourseCredit = x.Course.Credit,
                                                        CourseRateId = x.Course.CourseRateId,
                                                        CourseLab = x.Course.Lab,
                                                        CourseLecture = x.Course.Lecture,
                                                        CourseOther = x.Course.Other,
                                                        FacultyNameEn = x.Course.Faculty.NameEn,
                                                        Number = x.Number,
                                                        MidtermDate = x.MidtermDate,
                                                        MidtermStart = x.MidtermStart,
                                                        MidtermEnd = x.MidtermEnd,
                                                        FinalDate = x.FinalDate,
                                                        FinalStart = x.FinalStart,
                                                        FinalEnd = x.FinalEnd,
                                                        SeatLimit = x.SeatLimit,
                                                        SeatUsed = x.SeatUsed,
                                                        PlanningSeat = x.PlanningSeat,
                                                        Status = x.Status,
                                                        IsClosed = x.IsClosed,
                                                        ApprovedBy = x.ApprovedBy,
                                                        ApprovedAt = x.ApprovedAt,
                                                        Remark = x.Remark,
                                                        ParentSection = x.ParentSectionId,
                                                        OpenedSectionAt = x.OpenedSectionAt,
                                                        ClosedSectionAt = x.ClosedSectionAt,
                                                        SeatAvailable = x.SeatAvailable,
                                                        ParentSectionId = x.ParentSectionId,
                                                        ParentSectionCourseCode = x.ParentSection.Course.Code,
                                                        ParentSectionNumber = x.ParentSection.Number,
                                                        ParentSeatUsed = x.ParentSection == null ? 0 : x.ParentSection.SeatUsed,
                                                        ParentCourseRateId = x.ParentSection.Course.CourseRateId,
                                                        IsOutbound = x.IsOutbound,
                                                        IsSpecialCase = x.IsSpecialCase,
                                                        Title = x.MainInstructor.Title.NameEn,
                                                        FirstNameEn = x.MainInstructor.FirstNameEn,
                                                        LastNameEn = x.MainInstructor.LastNameEn,
                                                        CreatedBy = x.CreatedBy
                                                    })
                                       .OrderBy(x => x.CourseCode)
                                           .ThenBy(x => x.Number)
                                       .ToList();

            sections = sections.Where(x => ((criteria.StudentLessThan ?? 0) == 0 || x.SeatUsed <= criteria.StudentLessThan)
                                            && (string.IsNullOrEmpty(criteria.IsNoStudent) 
                                            || (Convert.ToBoolean(criteria.IsNoStudent) ? x.SeatUsed == 0 : x.SeatUsed > 0)))
                               .ToList();

            var sectionIds = sections.Select(x => x.Id).ToList();
            var sectionIdsNullable = sections.Select(x => (long?)x.Id).ToList();
            var parentSectionIds = sections.Where(x => x.ParentSectionId != null).Select(x => x.ParentSectionId);
            var jointTotalSeatUsed = _db.Sections.Where(x => parentSectionIds.Contains(x.ParentSectionId));

            var jointSections = _db.Sections.Where(x => sectionIdsNullable.Contains(x.ParentSectionId))
                                            .Select(x => new JointSectionReportViewModel
                                                         {
                                                             Id = x.Id,
                                                             ParentSectionId = x.ParentSectionId,
                                                             Number = x.Number,
                                                             CourseCode = x.Course.Code,
                                                             SeatUsed = x.SeatUsed,
                                                             CourseRateId = x.Course.CourseRateId
                                                         })
                                            .ToList();

            var sectionDetails = _db.SectionDetails.Where(x => sectionIds.Contains(x.SectionId)
                                                               && (criteria.InstructorId == 0
                                                                   || x.InstructorSections.Any(y => y.InstructorId == criteria.InstructorId)))
                                                   .Select(x => new SectionDetailReportViewModel
                                                                {
                                                                    SectionDetailId = x.Id,
                                                                    SectionId = x.SectionId,
                                                                    Day = x.Day,
                                                                    StartTime = x.StartTime,
                                                                    EndTime = x.EndTime,
                                                                    RoomNameEn = x.Room.NameEn
                                                                })
                                                   .OrderBy(x => x.Day)
                                                       .ThenBy(x => x.StartTime)
                                                   .ToList();

            var sectionDetailIds = sectionDetails.Select(x => x.SectionDetailId).ToList();
            var instructorSections = _db.InstructorSections.Where(x => sectionDetailIds.Contains(x.SectionDetailId))
                                                           .Select(x => new 
                                                                        {
                                                                            SectionDetailId = x.SectionDetailId,
                                                                            FirstNameEn = x.Instructor.FirstNameEn,
                                                                            LastNameEn = x.Instructor.LastNameEn,
                                                                            Title = x.Instructor.Title.NameEn
                                                                        })
                                                           .ToList();

            foreach (var item in sectionDetails)
            {
                item.InstructorSections = instructorSections.Where(x => x.SectionDetailId == item.SectionDetailId)
                                                            .Select(x => $"{ x.Title } { x.FirstNameEn } { x.LastNameEn }")
                                                            .ToList();
            }

            var users = _db.Users.Where(x => sections.Select(y => y.CreatedBy).Contains(x.Id))
                                 .ToList();

            var instructors = _db.Instructors.Include(x => x.Title)
                                             .Where(x => users.Select(y => y.InstructorId).Contains(x.Id))
                                             .ToList();

            foreach (var item in sections)
            {
                var user = users.SingleOrDefault(x => x.Id == item.CreatedBy);
                var instructor = user != null ? instructors.SingleOrDefault(x => x.Id == user.InstructorId) : null;
                if (instructor != null)
                {
                    item.CreatedByText = $"{ instructor.Title.NameEn } { instructor.FirstNameEn } { instructor.LastNameEn }";
                }
                else if (user != null && !string.IsNullOrEmpty(user.FirstnameEN))
                {
                    item.CreatedByText = $"{ user.FirstnameEN } { user.LastnameEN }"; ;
                }
                else if (user != null)
                {
                    item.CreatedByText = user.UserName;
                }
                else
                {
                    item.CreatedByText = item.CreatedBy;
                }

                item.JointSections = jointSections.Where(x => x.ParentSectionId == item.Id)
                                                  .ToList();

                if (item.ParentSectionId == null)
                {
                    item.TotalSeatUsed = item.SeatUsed + item.JointSections.Sum(x => x.SeatUsed);
                }
                else
                {
                    item.TotalSeatUsed = item.ParentSeatUsed + jointTotalSeatUsed.Where(x => x.ParentSectionId == item.ParentSectionId)
                                                                                 .Sum(x => x.SeatUsed);
                }
                                                
                item.SectionDetails = sectionDetails.Where(x => x.SectionId == item.Id)
                                                    .ToList();
            }

            return sections;
        }
    }
}