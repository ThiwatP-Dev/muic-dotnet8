using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.Schedules;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace KeystoneLibrary.Providers
{
    public class ScheduleProvider : BaseProvider, IScheduleProvider
    {
        public ScheduleProvider(ApplicationDbContext db, IMapper mapper) : base(db, mapper)  { }

        public GeneratedSchedultResult GenerateSchedules(GenerateSchedule model)
        {
            var courses = GetCourses(model.SemesterId, model.CourseSections);

            var sortedCourses = courses.OrderBy(x => x.Sections.Count()).ToList();

            //Generate Schedule
            //
            ScheduleGeneratorProvider scheduleGenerator = new ScheduleGeneratorProvider(sortedCourses);

            var schedules = scheduleGenerator.Generate();
            
            //Map Generated Schedules to Model
            //
            var sectionSeatViewModels = GetGeneratedSchedule(model.SemesterId, schedules);

            //Map Generated Schedules Result
            //
            var generatedSchedultResult = new GeneratedSchedultResult();

            if (sectionSeatViewModels.Any())
            {
                generatedSchedultResult.IsSucceed = true;
                generatedSchedultResult.GeneratedScheduleViewModel = new GeneratedScheduleViewModel { Courses = sortedCourses, Schedules = sectionSeatViewModels };
                generatedSchedultResult.ExamConflictMessage = sectionSeatViewModels.FirstOrDefault().ExamConflictMessage;

                return generatedSchedultResult;
            }
            else if (scheduleGenerator.ErrorList.Any())
            {
                generatedSchedultResult.IsSucceed = false;
                generatedSchedultResult.ErrorMessage = String.Format("Class Time Conflict\n{0}", String.Join(", ", scheduleGenerator.ErrorList.Take(3)));
            }
            else
            {
                generatedSchedultResult.IsSucceed = false;
                generatedSchedultResult.ErrorMessage = "Schedule not found";
            }

            return generatedSchedultResult;
        }
        
        private List<ScheduleSectionViewModel> GetGeneratedSchedule(long semesterId, List<SectionViewModel[]> schedules)
        {
            var scheduleSections = new List<ScheduleSectionViewModel>();
            var sectionIds = new List<long>();

            var sections = _db.Sections.Where(x => x.TermId == semesterId)
                                       .ToList();

            foreach (var item in schedules)
            {
                for (int i = 0; i < item.Count(); ++i)
                {
                    sectionIds.Add(item[i].Id);
                }

                var scheduleSection = new ScheduleSectionViewModel
                {
                    SectionIds = sectionIds,
                    ExamConflictMessage = CreateExaminationConflictMessage(sections, sectionIds)
                };

                scheduleSections.Add(scheduleSection);

                sectionIds = new List<long>();
            }

            return scheduleSections;
        }
        
        private List<SemesterCourseViewModel> GetCourses(long semesterId, List<CourseSection> courseSections)
        {
            var courses = _db.Courses.ToList();

            var sections = _db.Sections.Include(x => x.SectionDetails)
                                       .IgnoreQueryFilters()
                                       .Where(x => x.TermId == semesterId)
                                       .ToList();

            var semesterCourseViewModels = new List<SemesterCourseViewModel>();
            var sectionViewModels = new List<SectionViewModel>();

            foreach(var item in courseSections)
            {
                var selectedSections = sections.Where(x => item.SectionIds.Contains(x.Id));
                var course = courses.SingleOrDefault(x => x.Id == item.CourseId);

                foreach (var section in selectedSections)
                {
                    var sectionViewModel = new SectionViewModel();
                    sectionViewModel = _mapper.Map<Section, SectionViewModel>(section);
                    sectionViewModel.ClassSchedules = section.SectionDetails.Select(x => _mapper.Map<SectionDetail, ClassScheduleTimeViewModel>(x)).ToList();
                    foreach (var classSchedule in sectionViewModel.ClassSchedules)
                    {
                        classSchedule.CourseCode = course.Code;
                        classSchedule.SectionNumber = section.Number;
                    }

                    sectionViewModel.ExamSchedule = _mapper.Map<Section, ExaminationSchedule>(section);
                    sectionViewModels.Add(sectionViewModel);
                }

                var courseDetailViewModel = new SemesterCourseViewModel
                {
                    Id = course.Id,
                    Code = course.Code,
                    NameEn = course.NameEn,
                    NameTh = course.NameTh,
                    Credit = course.Credit,
                    RegistrationCredit = course.RegistrationCredit,
                    Sections = sectionViewModels
                };

                semesterCourseViewModels.Add(courseDetailViewModel);
                sectionViewModels = new List<SectionViewModel>();
            }

            return semesterCourseViewModels;
        }

        public string CreateExaminationConflictMessage(List<long> sectionIds)
        {
            var sections = _db.Sections.ToList();
            return CreateExaminationConflictMessage(sections, sectionIds);
        }

        private string CreateExaminationConflictMessage(List<Section> sections, List<long> sectionIds)
        {
            var midtermConflictedSections = sections.Where(x => sectionIds.Contains(x.Id) && IsValidDateTime(x.MidtermDate, x.MidtermStart))
                                                    .GroupBy(x => new { x.MidtermDate, x.MidtermStart })
                                                    .Where(x => x.Count() > 1);

            if (midtermConflictedSections.Any())
            {
                var conflictedMidtermCourses = string.Join("\n", 
                                                           midtermConflictedSections.Select(x => string.Join(" and ", 
                                                                                                             x.Select(y => $"{ y.Course.Code } ({ y.Number })"))));
                
                return conflictedMidtermCourses;
            }
            else
            {
                var finalConflictedSections = sections.Where(x => sectionIds.Contains(x.Id) && IsValidDateTime(x.MidtermDate, x.MidtermStart))
                                                      .GroupBy(x => new { x.FinalDate, x.FinalStart })
                                                      .Where(x => x.Count() > 1);
                
                if (finalConflictedSections.Any())
                {
                    var conflictedFinalCourses = string.Join("\n", 
                                                             finalConflictedSections.Select(x => string.Join(" and ", 
                                                                                                             x.Select(y => $"{ y.Course.Code } ({ y.Number })"))));
            
                    return conflictedFinalCourses;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private bool HasExaminationConflict(List<Section> sections, List<long> sectionIds)
        {
            var hasMidtermConflict = sections.Where(x => sectionIds.Contains(x.Id) && IsValidDateTime(x.MidtermDate, x.MidtermStart))
                                             .GroupBy(x => new { x.MidtermDate, x.MidtermStart })
                                             .Any(x => x.Count() > 1);
            
            var hasFinalConflict = sections.Where(x => sectionIds.Contains(x.Id) && IsValidDateTime(x.FinalDate, x.FinalStart))
                                           .GroupBy(x => new { x.FinalDate, x.FinalStart })
                                           .Any(x => x.Count() > 1);

            return hasMidtermConflict || hasFinalConflict;
        }
        
        private bool IsValidDateTime(DateTime? date, TimeSpan? time)
        {
            return (date != null && date != new DateTime()) && (time.HasValue && time.Value.Ticks != 0);
        }

        public List<ScheduleViewModel> GetSchedule(List<long> sectionIds)
        {
            var schedules = _db.SectionDetails.Include(x => x.Section)
                                                  .ThenInclude(x => x.Course)
                                              .Include(x => x.Section)
                                                    .ThenInclude(x => x.MainInstructor)
                                                    .ThenInclude(x => x.Title)
                                              .Include(x => x.InstructorSections)
                                                  .ThenInclude(x => x.Instructor)
                                              .Include(x => x.Room)
                                              .Where(x => !x.Section.IsClosed
                                                          && sectionIds.Contains(x.SectionId))
                                              .GroupBy(x => x.SectionId)
                                              .AsEnumerable()
                                              .Select((x, index) => new ScheduleViewModel
                                                                    {
                                                                        CourseCode = x.First().Section.Course.Code,
                                                                        CourseName = x.First().Section.Course.NameEn,
                                                                        CourseCodeAndCredit = x.First().Section.Course.CodeAndCredit,
                                                                        MainInstructorFullNameEn = x.First().Section.MainInstructor == null ? "" : x.First().Section.MainInstructor.FullNameEn,
                                                                        Section = x.First().Section.IsParent
                                                                                  ? "" : x.First().Section.Number,
                                                                        Id = x.First().SectionId,
                                                                        SeatUsed = x.First().Section.SeatUsed,
                                                                        MidtermDate = x.First().Section.MidtermString,
                                                                        MidtermTime = x.First().Section.MidtermTime,
                                                                        FinalDate = x.First().Section.FinalString,
                                                                        FinalTime = x.First().Section.FinalTime,
                                                                        ColorCode = ColorProvider.ColorCodeGenerator(index),
                                                                        ScheduleTimes = x.Select(y => _mapper.Map<SectionDetail, ClassScheduleTimeViewModel>(y))
                                                                                                             .OrderBy(y => y.CourseCode)
                                                                                                                .ThenBy(y => y.Day)
                                                                                                                .ThenBy(y => y.StartTime)
                                                                                                             .ToList()
                                                                    })
                                              .OrderBy(x => x.CourseCode)
                                                  .ThenBy(x => x.CourseName)
                                                  .ThenBy(x => x.Section)
                                              .ToList();
            return schedules;
        }

        public List<ScheduleViewModel> GetInstructorSchedule(long termId, long instructorId)
        {
            var schedules = _db.InstructorSections.Include(x => x.SectionDetail)
                                                       .ThenInclude(x => x.Section)
                                                       .ThenInclude(x => x.Course)
                                                   .Include(x => x.SectionDetail)
                                                       .ThenInclude(x => x.Room)
                                                   .Include(x => x.Instructor)
                                                   .Where(x => !x.SectionDetail.Section.IsClosed
                                                               && x.InstructorId == instructorId
                                                               && x.SectionDetail.Section.TermId == termId)
                                                   .GroupBy(x => x.SectionDetail.SectionId)
                                                   .AsEnumerable()
                                                   .Select((x, index) => new ScheduleViewModel
                                                                         {
                                                                             CourseName = x.First().SectionDetail.Section.Course.NameEn,
                                                                             CourseCode = x.First().SectionDetail.Section.Course.Code,
                                                                             Section = x.First().SectionDetail.Section.IsParent ? 
                                                                                       "" : x.First().SectionDetail.Section.Number,
                                                                             Id = x.First().SectionDetail.SectionId,
                                                                             SeatUsed = x.First().SectionDetail.Section.SeatUsed,
                                                                             MidtermDate = x.First().SectionDetail.Section.MidtermString,
                                                                             MidtermTime = x.First().SectionDetail.Section.MidtermTime,
                                                                             FinalDate = x.First().SectionDetail.Section.FinalString,
                                                                             FinalTime = x.First().SectionDetail.Section.FinalTime,
                                                                             ColorCode = ColorProvider.ColorCodeGenerator(index),
                                                                             ScheduleTimes = x.Select(y => _mapper.Map<InstructorSection, ClassScheduleTimeViewModel>(y)).ToList()
                                                                         })
                                                   .OrderBy(x => x.CourseCode)
                                                   .ThenBy(x => x.CourseName)
                                                   .ThenBy(x => x.Section)
                                                   .ToList();
            return schedules;
        }

        public List<ScheduleViewModel> GetExaminationSchedule(List<long> sectionIds, string scheduleType)
        {
            var Examination = _db.Sections.Include(x => x.Course)
                                          .Where(x => !x.IsClosed
                                                       && sectionIds.Contains(x.Id))
                                          .GroupBy(x => x.Id)
                                          .AsEnumerable()
                                          .Select((x, index) => new ScheduleViewModel
                                                                {
                                                                    CourseCode = x.First().Course.Code,
                                                                    CourseName = x.First().Course.NameEn,
                                                                    Section = x.First().IsParent
                                                                              ? "" : x.First().Number,
                                                                    Id = x.First().Id,
                                                                    SeatUsed = x.First().SeatUsed,
                                                                    MidtermDate = x.First().Midterm,
                                                                    FinalDate = x.First().Final,
                                                                    ColorCode = ColorProvider.ColorCodeGenerator(index),
                                                                    ScheduleTimes = x.Select(y => new ClassScheduleTimeViewModel
                                                                                                  {
                                                                                                      SectionNumber = x.First().Number,
                                                                                                      DayOfWeek = scheduleType == "Midterm" ? x.FirstOrDefault().DayOfWeekMidterm : x.FirstOrDefault().DayOfWeekFinal,
                                                                                                      ExaminationTime = scheduleType == "Midterm" ? x.First().MidtermTime : x.First().FinalTime,
                                                                                                      StartTime = scheduleType == "Midterm" ? x.First().MidtermStartTime : x.First().FinalStartTime,
                                                                                                      EndTime = scheduleType == "Midterm" ? x.First().MidtermEndTime : x.First().FinalEndTime,
                                                                                                      Day = scheduleType == "Midterm" ? x.First().MidtermDay : x.First().FinalDay,
                                                                                                      Room = scheduleType == "Midterm" ? x.First().MidtermRoom.NameEn : x.First().FinalRoom.NameEn,
                                                                                                      ExaminationDate = scheduleType == "Midterm" ? x.First().MidtermString : x.First().FinalString
                                                                                                  })
                                                                                     .ToList()
                                                                })
                                          .OrderBy(x => x.CourseCode)
                                              .ThenBy(x => x.CourseName)
                                              .ThenBy(x => x.Section)
                                          .ToList();
            return Examination;               
        }

        public List<ScheduleViewModel> GetTeachingSchedule(List<RoomSlot> roomSlots, string type)
        {
            var schedules = new List<ScheduleViewModel>();
            if (type == "s")
            {
                schedules = roomSlots.Where(x => x.SectionSlotId != null)
                                     .GroupBy(x => x.SectionSlot.SectionId)
                                     .AsEnumerable()
                                     .Select((x, index) => new ScheduleViewModel
                                                           {
                                                               CourseCode = x.First().SectionSlot.Section.Course.Code,
                                                               CourseName = x.First().SectionSlot.Section.Course.NameEn,
                                                               Section = x.First().SectionSlot.Section.IsParent
                                                                         ? "" : x.First().SectionSlot.Section.Number,
                                                               Id = x.First().SectionSlot.SectionId,
                                                               SeatUsed = x.First().SectionSlot.Section.SeatUsed,
                                                               MidtermDate = x.First().SectionSlot.Section.MidtermString,
                                                               MidtermTime = x.First().SectionSlot.Section.MidtermTime,
                                                               FinalDate = x.First().SectionSlot.Section.FinalString,
                                                               FinalTime = x.First().SectionSlot.Section.FinalTime,
                                                               ColorCode = ColorProvider.ColorCodeGenerator(index),
                                                               ScheduleTimes = x.GroupBy(y => y.SectionSlot.Section)
                                                                                .Select(y => new ClassScheduleTimeViewModel
                                                                                             {
                                                                                                 Day = x.First().Day,
                                                                                                 DayOfWeek = x.First().Dayofweek,
                                                                                                 StartTime = x.First().StartTime,
                                                                                                 EndTime = x.First().EndTime,
                                                                                                 TimeText = x.First().Time,
                                                                                                 CourseCode = x.First().SectionSlot.Section.Course.Code,
                                                                                                 CourseName = x.First().SectionSlot.Section.Course.NameEn,
                                                                                                 SectionNumber = x.First().SectionSlot.Section.Number,
                                                                                                 Room = x.First().SectionSlot.Room == null ? "" 
                                                                                                        : x.First().SectionSlot.Room.NameEn
                                                                                             })
                                                                                .ToList()
                                                           })
                                     .OrderBy(x => x.CourseCode)
                                         .ThenBy(x => x.CourseName)
                                         .ThenBy(x => x.Section)
                                     .ToList();
            }
            else if (type == "e")
            {
                schedules = roomSlots.Where(x => x.ExaminationReservationId != null)
                                     .GroupBy(x => x.ExaminationReservation.SectionId)
                                     .AsEnumerable()
                                     .Select((x, index) => new ScheduleViewModel
                                                         {
                                                             CourseCode = x.First().ExaminationReservation.Section.Course.Code,
                                                             CourseName = x.First().ExaminationReservation.Section.Course.NameEn,
                                                             Section = x.First().ExaminationReservation.Section.IsParent
                                                                         ? "" : x.First().ExaminationReservation.Section.Number,
                                                             Id = x.First().ExaminationReservation.SectionId,
                                                             SeatUsed = x.First().ExaminationReservation.Section.SeatUsed,
                                                             MidtermDate = x.First().ExaminationReservation.Section.MidtermString,
                                                             MidtermTime = x.First().ExaminationReservation.Section.MidtermTime,
                                                             FinalDate = x.First().ExaminationReservation.Section.FinalString,
                                                             FinalTime = x.First().ExaminationReservation.Section.FinalTime,
                                                             ColorCode = ColorProvider.ColorCodeGenerator(index),
                                                             ScheduleTimes = x.GroupBy(y => y.ExaminationReservation.Section)
                                                                              .Select(y => new ClassScheduleTimeViewModel
                                                                                           {
                                                                                               Day = x.First().Day,
                                                                                               DayOfWeek = x.First().Dayofweek,
                                                                                               StartTime = x.First().StartTime,
                                                                                               EndTime = x.First().EndTime,
                                                                                               TimeText = x.First().Time,
                                                                                               CourseCode = x.First().ExaminationReservation.Section.Course.Code,
                                                                                               CourseName = x.First().ExaminationReservation.Section.Course.NameEn,
                                                                                               SectionNumber = x.First().ExaminationReservation.Section.Number,
                                                                                               Room = x.First().ExaminationReservation.Room == null ? "" 
                                                                                                   : x.First().ExaminationReservation.Room.NameEn
                                                                                           })
                                                                             .ToList()
                                                         })
                                     .OrderBy(x => x.CourseCode)
                                         .ThenBy(x => x.CourseName)
                                         .ThenBy(x => x.Section)
                                     .ToList();                
            }
            else
            {
                schedules = roomSlots.Where(x => x.RoomReservationId != null)
                                     .GroupBy(x => x.RoomReservation)
                                     .AsEnumerable()
                                     .Select((x, index) => new ScheduleViewModel
                                                           {
                                                               CourseCode = "",
                                                               CourseName = x.First().RoomReservation.Name,
                                                               Section = "",
                                                               Id = 0,
                                                               SeatUsed = 0,
                                                               MidtermDate = "",
                                                               MidtermTime = "",
                                                               FinalDate = "",
                                                               FinalTime = "",
                                                               ColorCode = ColorProvider.ColorCodeGenerator(index),
                                                               ScheduleTimes = x.GroupBy(y => y.RoomReservation.Name)
                                                                                .Select(y => new ClassScheduleTimeViewModel
                                                                                             {
                                                                                                 Day = x.First().Day,
                                                                                                 DayOfWeek = x.First().Dayofweek,
                                                                                                 StartTime = x.First().StartTime,
                                                                                                 EndTime = x.First().EndTime,
                                                                                                 TimeText = x.First().Time,
                                                                                                 CourseCode = "",
                                                                                                 CourseName = x.First().RoomReservation.Name,
                                                                                                 SectionNumber = "",
                                                                                                 Room = x.First().RoomReservation.Room.NameEn == null ? "" 
                                                                                                        : x.First().RoomReservation.Room.NameEn
                                                                                             })
                                                                                .ToList()
                                                           })
                                     .OrderBy(x => x.CourseCode)
                                         .ThenBy(x => x.CourseName)
                                         .ThenBy(x => x.Section)
                                     .ToList();
            }
            
            return schedules;
        }

        public string GetClassConflictMessage(List<long> sectionIds)
        {
            StringBuilder errorMessage = new StringBuilder();
            var sectionDetails = _db.SectionDetails.Where(x => sectionIds.Contains(x.SectionId))
                                                   .Include(x => x.Section)
                                                       .ThenInclude(x => x.Course)
                                                   .Select(x => new 
                                                   {
                                                       CourseCode = x.Section.Course.Code,
                                                       SectionNumber = x.Section.Number,
                                                       x.Day,
                                                       x.StartTime,
                                                       x.EndTime
                                                   })
                                                   .ToList();
            sectionDetails = sectionDetails.OrderBy(x => x.CourseCode)
                                               .ThenBy(x => x.SectionNumber)
                                               .ThenBy(x => x.Day)
                                               .ThenBy(x => x.StartTime)
                                               .ToList(); 
            for (int i = 0; i < sectionDetails.Count; i++)
            {
                for (int j = i + 1; j < sectionDetails.Count; j++)
                {
                    var detail1 = sectionDetails[i];
                    var detail2 = sectionDetails[j];
                    if (detail1.Day == detail2.Day
                        && detail1.StartTime < detail2.EndTime
                        && detail1.EndTime > detail2.StartTime)
                    {
                        string course1 = $"{ detail1.CourseCode } ({ detail1.SectionNumber })";
                        string time1 = $"{ Enum.GetName(typeof(DayOfWeek), detail1.Day).Substring(0, 3).ToUpper() } { detail1.StartTime.ToString(StringFormat.TimeSpan) } - { detail1.EndTime.ToString(StringFormat.TimeSpan) }";
                        string course2 = $"{ detail2.CourseCode } ({ detail2.SectionNumber })";
                        string time2 = $"{ Enum.GetName(typeof(DayOfWeek), detail2.Day).Substring(0, 3).ToUpper() } { detail2.StartTime.ToString(StringFormat.TimeSpan) } - { detail2.EndTime.ToString(StringFormat.TimeSpan) }";
                        errorMessage.Append($"{course1} {time1} and {course2} {time2}\n");
                    }
                }
            }

            return errorMessage.ToString();
        }

        public string GetExamConflictMessage(List<long> sectionIds)
        {
            StringBuilder errorMessage = new StringBuilder();
            var sections = _db.Sections.Include(x => x.Course)
                                       .Where(x => sectionIds.Contains(x.Id))
                                       .Select(x => new 
                                       {
                                           CourseCode = x.Course.Code,
                                           x.Number,
                                           x.MidtermDate,
                                           x.MidtermStart,
                                           x.MidtermEnd,
                                           x.FinalDate,
                                           x.FinalStart,
                                           x.FinalEnd
                                       })
                                       .ToList();
            // midterm
            sections = sections.OrderBy(x => x.CourseCode)
                               .ThenBy(x => x.Number)
                               .ThenBy(x => x.MidtermDate)
                               .ThenBy(x => x.MidtermStart)
                               .ToList();
            for (int i = 0; i < sections.Count; i++)
            {
                for (int j = i + 1; j < sections.Count; j++)
                {
                    var section1 = sections[i];
                    var section2 = sections[j];
                    if (section1.MidtermDate == section2.MidtermDate
                        && section1.MidtermStart < section2.MidtermEnd
                        && section1.MidtermEnd > section2.MidtermStart)
                    {
                        string course1 = $"{ section1.CourseCode } ({ section1.Number })";
                        string time1 = $"{ section1.MidtermDate?.ToString(StringFormat.ShortDate) } { section1.MidtermStart?.ToString(StringFormat.TimeSpan) } - { section1.MidtermEnd?.ToString(StringFormat.TimeSpan) }";
                        string course2 = $"{ section2.CourseCode } ({ section2.Number })";
                        string time2 = $"{ section2.MidtermDate?.ToString(StringFormat.ShortDate) } { section2.MidtermStart?.ToString(StringFormat.TimeSpan) } - { section2.MidtermEnd?.ToString(StringFormat.TimeSpan) }";
                        errorMessage.Append($"Midterm: {course1} {time1} and {course2} {time2}\n");
                    }
                }
            }

            // final
            sections = sections.OrderBy(x => x.CourseCode)
                               .ThenBy(x => x.Number)
                               .ThenBy(x => x.FinalDate)
                               .ThenBy(x => x.FinalStart)
                               .ToList();
            for (int i = 0; i < sections.Count; i++)
            {
                for (int j = i + 1; j < sections.Count; j++)
                {
                    var section1 = sections[i];
                    var section2 = sections[j];
                    if (section1.FinalDate == section2.FinalDate
                        && section1.FinalStart < section2.FinalEnd
                        && section1.FinalEnd > section2.FinalStart)
                    {
                        string course1 = $"{ section1.CourseCode } ({ section1.Number })";
                        string time1 = $"{ section1.FinalDate?.ToString(StringFormat.ShortDate) } { section1.FinalStart?.ToString(StringFormat.TimeSpan) } - { section1.FinalEnd?.ToString(StringFormat.TimeSpan) }";
                        string course2 = $"{ section2.CourseCode } ({ section2.Number })";
                        string time2 = $"{ section2.FinalDate?.ToString(StringFormat.ShortDate) } { section2.FinalStart?.ToString(StringFormat.TimeSpan) } - { section2.FinalEnd?.ToString(StringFormat.TimeSpan) }";
                        errorMessage.Append($"Final: {course1} {time1} and {course2} {time2}\n");
                    }
                }
            }

            return errorMessage.ToString();
        }

        public List<ScheduleViewModel> GetRoomSchedulePreview(List<RoomSlot> roomSlots, string type = "")
        {
            var schedules = new List<ScheduleViewModel>();
            var index = 0;
            if(string.IsNullOrEmpty(type) || type == "s")
            {
                var schedulesStudy = roomSlots.Where(x => x.SectionSlotId != null && x.UsingType == "s")
                                            .GroupBy(x => x.SectionSlot.SectionId)
                                            .AsEnumerable()
                                            .Select((x) => new ScheduleViewModel
                                                                    {
                                                                        CourseCode = x.First().SectionSlot.Section.Course.Code,
                                                                        CourseName = x.First().SectionSlot.Section.Course.NameEn,
                                                                        Section = x.First().SectionSlot.Section.IsParent ? "" : x.First().SectionSlot.Section.Number,
                                                                        Id = x.First().SectionSlot.SectionId,
                                                                        SeatUsed = x.First().SectionSlot.Section.SeatUsed,
                                                                        MidtermDate = x.First().SectionSlot.Section.MidtermString,
                                                                        MidtermTime = x.First().SectionSlot.Section.MidtermTime,
                                                                        FinalDate = x.First().SectionSlot.Section.FinalString,
                                                                        FinalTime = x.First().SectionSlot.Section.FinalTime,
                                                                        ColorCode = ColorProvider.ColorCodeGenerator(index++),
                                                                        ScheduleTimes = x.Select(y => new ClassScheduleTimeViewModel
                                                                                                    {
                                                                                                        Day = y.Day,
                                                                                                        DayOfWeek = y.Dayofweek,
                                                                                                        StartTime = y.StartTime,
                                                                                                        EndTime = y.EndTime,
                                                                                                        TimeText = y.Time,
                                                                                                        CourseCode = y.SectionSlot.Section.Course.Code,
                                                                                                        CourseName = y.SectionSlot.Section.Course.NameEn,
                                                                                                        SectionNumber = y.SectionSlot.Section.Number,
                                                                                                        InstructorNameEn = y.SectionSlot.Instructor?.FullNameEn ?? "",
                                                                                                        InstructorShortName = y.SectionSlot.Instructor?.ShortNameEn ?? "",
                                                                                                        Room = y.SectionSlot.Room == null ? "" 
                                                                                                                : y.SectionSlot.Room.NameEn
                                                                                                    })
                                                                                        .OrderBy(z => z.StartTime).ToList()
                                                                    })
                                            .OrderBy(x => x.CourseCode)
                                                .ThenBy(x => x.CourseName)
                                                .ThenBy(x => x.Section)
                                            .ToList();  

                schedules.AddRange(schedulesStudy);

                var schedulesStudyWithOutSlot = roomSlots.Where(x => x.SectionSlotId == null 
                                                                         && x.RoomReservationId != null
                                                                         && x.UsingType == "s")
                                                         .GroupBy(x => x.RoomReservation.Id)
                                                         .AsEnumerable()
                                                         .Select((x) => new ScheduleViewModel
                                                         {
                                                             CourseCode = x.First().RoomReservation.Name + "(S)",
                                                             CourseName = x.First().RoomReservation.Name,
                                                             Section = "",
                                                             Id = 0,
                                                             SeatUsed = 0,
                                                             MidtermDate = "",
                                                             MidtermTime = "",
                                                             FinalDate = "",
                                                             FinalTime = "",
                                                             ColorCode = ColorProvider.ColorCodeGenerator(index++),
                                                             ScheduleTimes = x.Select(y => new ClassScheduleTimeViewModel
                                                             {
                                                                 Day = y.Day,
                                                                 DayOfWeek = y.Dayofweek,
                                                                 StartTime = y.StartTime,
                                                                 EndTime = y.EndTime,
                                                                 TimeText = y.Time,
                                                                 CourseCode = y.RoomReservation.Name,
                                                                 CourseName = y.RoomReservation.Name,
                                                                 Type = "S",
                                                                 SectionNumber = x.First().UsingTypeText,
                                                                 Room = y.RoomReservation.Room.NameEn == null ? ""
                                                                                                       : y.RoomReservation.Room.NameEn
                                                             })
                                                                              .OrderBy(z => z.StartTime).ToList()
                                                         })
                                                         .OrderBy(x => x.CourseCode)
                                                             .ThenBy(x => x.CourseName)
                                                             .ThenBy(x => x.Section)
                                                         .ToList();
                schedules.AddRange(schedulesStudyWithOutSlot);
            }

            if(string.IsNullOrEmpty(type) || type == "e")
            {
                var schedulesExamination = roomSlots.Where(x => x.ExaminationReservationId != null)
                                                    .GroupBy(x => x.ExaminationReservation.SectionId)
                                                    .AsEnumerable()
                                                    .Select((x) => new ScheduleViewModel
                                                                        {
                                                                            CourseCode = x.First().ExaminationReservation.Section.Course.Code + "(" + x.First().ExaminationReservation.ExaminationType.Abbreviation +")",
                                                                            CourseName = x.First().ExaminationReservation.Section.Course.NameEn,
                                                                            Section = x.First().ExaminationReservation.Section.IsParent ? "" : x.First().ExaminationReservation.Section.Number,
                                                                            Id = x.First().ExaminationReservation.SectionId,
                                                                            SeatUsed = x.First().ExaminationReservation.Section.SeatUsed,
                                                                            MidtermDate = x.First().ExaminationReservation.Section.MidtermString,
                                                                            MidtermTime = x.First().ExaminationReservation.Section.MidtermTime,
                                                                            FinalDate = x.First().ExaminationReservation.Section.FinalString,
                                                                            FinalTime = x.First().ExaminationReservation.Section.FinalTime,
                                                                            ColorCode = ColorProvider.ColorCodeGenerator(index++),
                                                                            ScheduleTimes = x.Select(y => new ClassScheduleTimeViewModel
                                                                                                          {
                                                                                                              Day = y.Day,
                                                                                                              DayOfWeek = y.Dayofweek,
                                                                                                              StartTime = y.StartTime,
                                                                                                              EndTime = y.EndTime,
                                                                                                              TimeText = y.Time,
                                                                                                              CourseCode = y.ExaminationReservation.Section.Course.Code,
                                                                                                              CourseName = y.ExaminationReservation.Section.Course.NameEn,
                                                                                                              SectionNumber = y.ExaminationReservation.Section.Number,
                                                                                                              Type = y.ExaminationReservation.ExaminationType.Abbreviation,
                                                                                                              InstructorNameEn = y.ExaminationReservation.Instructor?.FullNameEn ?? "",
                                                                                                              InstructorShortName = y.ExaminationReservation.Instructor?.ShortNameEn ?? "",
                                                                                                              Room = y.ExaminationReservation.Room == null ? "" 
                                                                                                                  : y.ExaminationReservation.Room.NameEn
                                                                                                          })
                                                                                            .OrderBy(z => z.StartTime).ToList()
                                                                        })
                                                    .OrderBy(x => x.CourseCode)
                                                        .ThenBy(x => x.CourseName)
                                                        .ThenBy(x => x.Section)
                                                    .ToList();                
                schedules.AddRange(schedulesExamination); 
            }

            if(string.IsNullOrEmpty(type) || type == "a") 
            {
                var schedulesReservation = roomSlots.Where(x => x.RoomReservationId != null
                                                                && x.UsingType == "a")
                                                    .GroupBy(x => x.RoomReservation.Id)
                                                    .AsEnumerable()
                                                    .Select((x) => new ScheduleViewModel
                                                                          {
                                                                              CourseCode = x.First().RoomReservation.Name + "(A)",
                                                                              CourseName = "",
                                                                              Section = "",
                                                                              Id = 0,
                                                                              SeatUsed = 0,
                                                                              MidtermDate = "",
                                                                              MidtermTime = "",
                                                                              FinalDate = "",
                                                                              FinalTime = "",
                                                                              ColorCode = ColorProvider.ColorCodeGenerator(index++),
                                                                              ScheduleTimes = x.Select(y => new ClassScheduleTimeViewModel
                                                                                                            {
                                                                                                                Day = y.Day,
                                                                                                                DayOfWeek = y.Dayofweek,
                                                                                                                StartTime = y.StartTime,
                                                                                                                EndTime = y.EndTime,
                                                                                                                TimeText = y.Time,
                                                                                                                CourseCode = y.RoomReservation.Name,
                                                                                                                CourseName = y.RoomReservation.Name,
                                                                                                                Type = "A",
                                                                                                                SectionNumber = x.First().UsingTypeText,
                                                                                                                Room = y.RoomReservation.Room.NameEn == null ? "" 
                                                                                                                       : y.RoomReservation.Room.NameEn
                                                                                                            })
                                                                                               .OrderBy(z => z.StartTime).ToList()
                                                                          })
                                                    .OrderBy(x => x.CourseCode)
                                                        .ThenBy(x => x.CourseName)
                                                        .ThenBy(x => x.Section)
                                                    .ToList();
                schedules.AddRange(schedulesReservation);   
            }

            if(string.IsNullOrEmpty(type) || type == "m") 
            {
                var schedulesReservation = roomSlots.Where(x => x.RoomReservationId != null
                                                                && x.UsingType == "m")
                                                    .GroupBy(x => x.RoomReservation.Id)
                                                    .AsEnumerable()
                                                    .Select((x) => new ScheduleViewModel
                                                                          {
                                                                              CourseCode = x.First().RoomReservation.Name + "(M)",
                                                                              CourseName = x.First().RoomReservation.Name,
                                                                              Section = "",
                                                                              Id = 0,
                                                                              SeatUsed = 0,
                                                                              MidtermDate = "",
                                                                              MidtermTime = "",
                                                                              FinalDate = "",
                                                                              FinalTime = "",
                                                                              ColorCode = ColorProvider.ColorCodeGenerator(index++),
                                                                              ScheduleTimes = x.Select(y => new ClassScheduleTimeViewModel
                                                                                                            {
                                                                                                                Day = y.Day,
                                                                                                                DayOfWeek = y.Dayofweek,
                                                                                                                StartTime = y.StartTime,
                                                                                                                EndTime = y.EndTime,
                                                                                                                TimeText = y.Time,
                                                                                                                CourseCode = y.RoomReservation.Name,
                                                                                                                CourseName = y.RoomReservation.Name,
                                                                                                                Type = "M",
                                                                                                                SectionNumber = x.First().UsingTypeText,
                                                                                                                Room = y.RoomReservation.Room.NameEn == null ? ""
                                                                                                                       : y.RoomReservation.Room.NameEn
                                                                                                            })
                                                                                               .OrderBy(z => z.StartTime).ToList()
                                                                          })
                                                    .OrderBy(x => x.CourseCode)
                                                        .ThenBy(x => x.CourseName)
                                                        .ThenBy(x => x.Section)
                                                    .ToList();
                schedules.AddRange(schedulesReservation);   
            }

            return schedules;
        }

        public List<ScheduleViewModel> GetRoomSchedulePreview(List<RoomScheduleSectionDetailViewModel> sectionDetails)
        {
            var schedules = new List<ScheduleViewModel>();

            schedules = sectionDetails.GroupBy(x => x.SectionId)
                                      .AsEnumerable()
                                      .Select((x, index) => new ScheduleViewModel
                                                            {
                                                                CourseCode = x.First().CourseCode,
                                                                CourseName = x.First().CourseName,
                                                                Section = x.First().SectionNumber,
                                                                Id = x.First().SectionId,
                                                                SeatUsed = x.First().SeatUsed,
                                                                MidtermDate = x.First().MidtermString,
                                                                MidtermTime = x.First().MidtermTime,
                                                                FinalDate = x.First().FinalString,
                                                                FinalTime = x.First().FinalTime,
                                                                ColorCode = ColorProvider.ColorCodeGenerator(index),
                                                                ScheduleTimes = x.Select(y => new ClassScheduleTimeViewModel
                                                                                              {
                                                                                                  Day = y.Day,
                                                                                                  DayOfWeek = y.Dayofweek,
                                                                                                  StartTime = y.StartTime,
                                                                                                  EndTime = y.EndTime,
                                                                                                  TimeText = y.Time,
                                                                                                  CourseCode = y.CourseCode,
                                                                                                  CourseName = y.CourseName,
                                                                                                  SectionNumber = y.SectionNumber,
                                                                                                  InstructorNameEn = y.InstructorNameEn,
                                                                                                  InstructorShortName = y.InstructorShortNameEn,
                                                                                                  Room = y.RoomName
                                                                                              })
                                                                                 .OrderBy(z => z.StartTime).ToList()
                                                            })
                                      .OrderBy(x => x.CourseCode)
                                          .ThenBy(x => x.CourseName)
                                          .ThenBy(x => x.Section)
                                      .ToList();
            
            return schedules;
        }
    }
}