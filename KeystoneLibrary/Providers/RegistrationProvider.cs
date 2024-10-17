using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Api;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.Report;
using KeystoneLibrary.Models.USpark;
using KeystoneLibrary.Models.ViewModel;
using KeystoneLibrary.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Net;
using System.Text;
using Vereyon.Web;

namespace KeystoneLibrary.Providers
{
    public class RegistrationProvider : BaseProvider, IRegistrationProvider
    {
        protected IStudentProvider _studentProvider { get; }

        private readonly IFlashMessage _flashMessage;
        private readonly IHttpClientProxy _httpClientProxy;

        public RegistrationProvider(ApplicationDbContext db,
                                    IMapper mapper,
                                    IStudentProvider studentProvider,
                                    IFlashMessage flashMessage,
                                    IConfiguration config,
                                    IHttpClientProxy httpClientProxy) : base(config, db, mapper)
        {
            _studentProvider = studentProvider;
            _flashMessage = flashMessage;
            _httpClientProxy = httpClientProxy;
        }

        public void UpdateStudentCreditLoadByRegistrationTerm(long academicLevelId)
        {
            var currentTerm = _db.Terms.AsNoTracking()
                                       .SingleOrDefault(x => x.AcademicLevelId == academicLevelId
                                                             && x.IsRegistration);
            if (currentTerm != null)
            {
                try
                {
                    _db.Database.ExecuteSql($"UPDATE student.[AcademicInformations] SET [MinimumCredit] = {currentTerm.MinimumCredit}, [MaximumCredit] = {currentTerm.MaximumCredit} WHERE [AcademicLevelId] = {academicLevelId}");
                }
                catch (Exception)
                {

                }
            }
        }

        public void AddSeat(Section section, int seat)
        {
            section.SeatAvailable = section.SeatAvailable == 0 ? 0 : section.SeatAvailable - seat;
            section.SeatUsed += seat;
        }

        public void ReturnSeat(Section section, int seat)
        {
            section.SeatUsed -= seat;
            section.SeatAvailable = section.SeatUsed > section.SeatLimit ? section.SeatAvailable
                                                                         : section.SeatAvailable + seat;
        }

        public List<RegistrationCourse> GetRegistrationCourses(Guid studentId, long termId)
        {
            var results = _db.RegistrationCourses.Include(x => x.Student)
                                                 .Include(x => x.Course)
                                                 .Include(x => x.Section)
                                                 .Include(x => x.Grade)
                                                 .Where(x => x.Student.Id == studentId
                                                             && x.TermId == termId)
                                                 .ToList();
            return results;
        }

        public RegistrationCourse GetRegistrationCourseById(long id)
        {
            var registrationCourse = _db.RegistrationCourses.Include(x => x.Student)
                                                             .Include(x => x.Course)
                                                             .Include(x => x.Section)
                                                             .Include(x => x.Grade)
                                                             .Where(x => x.Id == id)
                                                             .FirstOrDefault();
            return registrationCourse;
        }

        public List<RegistrationCourse> GetRegistrationCoursesByStudentCode(string code)
        {
            var registration = _db.RegistrationCourses.Include(x => x.Student)
                                                      .Include(x => x.Course)
                                                      .Include(x => x.Section)
                                                      .Include(x => x.Grade)
                                                      .Where(x => x.Student.Code == code
                                                                  && (x.Status == "a"
                                                                      || x.Status == "r"))
                                                      .IgnoreQueryFilters()
                                                      .ToList();
            return registration;
        }

        public List<RegistrationCourse> GetActiveRegistrationCourses(Guid id, long termId)
        {
            var results = _db.RegistrationCourses.Include(x => x.Section)
                                                     .ThenInclude(x => x.SectionDetails)
                                                     .ThenInclude(x => x.Room)
                                                 .Include(x => x.Section)
                                                     .ThenInclude(x => x.MainInstructor)
                                                     .ThenInclude(x => x.Title)
                                                 .Include(x => x.Course)
                                                 .Where(x => x.StudentId == id
                                                             && x.TermId == termId
                                                             && !x.IsTransferCourse
                                                             && x.Status != "d")
                                                 .ToList();
            return results;
        }

        public List<RegistrationCourse> GetRegistrationCoursesByCourseIdAndTermId(long courseId, long termId)
        {
            var results = _db.RegistrationCourses.Where(x => x.CourseId == courseId
                                                             && x.TermId == termId
                                                             && (x.Status == "a"
                                                                 || x.Status == "r"))
                                                 .ToList();
            return results;
        }

        public List<Slot> GetRegistrationSlots(Student student, long termId)
        {
            Int32.TryParse(student.Code.Last().ToString(), out var studentLastDigit);

            var slots = _db.Slots.Include(x => x.RegistrationSlotConditions)
                                     .ThenInclude(x => x.RegistrationCondition)
                                 .Where(x => x.RegistrationTerm.TermId == termId
                                             && (x.RegistrationSlotConditions.Any(y => (y.RegistrationCondition.AcademicProgramId == 0
                                                                                        || y.RegistrationCondition.AcademicProgramId == student.AcademicInformation.AcademicProgramId)
                                                                                        && (y.RegistrationCondition.FacultyId == 0
                                                                                            || y.RegistrationCondition.FacultyId == student.AcademicInformation.FacultyId)
                                                                                        && (y.RegistrationCondition.DepartmentId == 0
                                                                                            || y.RegistrationCondition.DepartmentId == student.AcademicInformation.DepartmentId)
                                                                                        && (y.RegistrationCondition.BatchCodeStart == 0
                                                                                            || y.RegistrationCondition.BatchCodeStart <= student.AcademicInformation.Batch)
                                                                                        && (y.RegistrationCondition.BatchCodeEnd == 0
                                                                                            || y.RegistrationCondition.BatchCodeEnd >= student.AcademicInformation.Batch)
                                                                                        && ((y.RegistrationCondition.LastDigitStart == 0 && y.RegistrationCondition.LastDigitEnd == 0)
                                                                                            || (y.RegistrationCondition.LastDigitStart <= y.RegistrationCondition.LastDigitEnd
                                                                                                && y.RegistrationCondition.LastDigitStart <= studentLastDigit
                                                                                                && y.RegistrationCondition.LastDigitEnd >= studentLastDigit)
                                                                                            || (y.RegistrationCondition.LastDigitStart > y.RegistrationCondition.LastDigitEnd
                                                                                                && y.RegistrationCondition.LastDigitStart <= studentLastDigit
                                                                                                || y.RegistrationCondition.LastDigitEnd >= studentLastDigit))
                                                                                        && (y.RegistrationCondition.IsGraduating == student.AcademicInformation.IsGraduating)
                                                                                        && (y.RegistrationCondition.IsAthlete == student.AcademicInformation.IsAthlete))))
                                 .OrderBy(x => x.StartedAt)
                                 .ToList();

            foreach (var item in slots)
            {
                foreach (var slotCondition in item.RegistrationSlotConditions)
                {
                    slotCondition.RegistrationCondition.Students = string.IsNullOrEmpty(slotCondition.RegistrationCondition.StudentCodes) ? new List<string>()
                                                                                                                                          : JsonConvert.DeserializeObject<List<string>>(slotCondition.RegistrationCondition.StudentCodes);
                }
            }

            slots = slots.Where(x => x.RegistrationSlotConditions
                         .Any(y => string.IsNullOrEmpty(y.RegistrationCondition.StudentCodes)
                                   || y.RegistrationCondition.Students.Contains(student.Code)))
                         .ToList();

            return slots;
        }

        public List<Course> GetOpenedCourses(long termId)
        {
            var course = _db.Sections.Include(x => x.Course)
                                     .Where(x => x.TermId == termId)
                                     .Select(x => x.Course)
                                     .Distinct()
                                     .ToList();
            return course;
        }

        public Term GetRegistrationTerm(long academiclevelId)
        {
            var term = _db.Terms.Where(x => x.AcademicLevelId == academiclevelId)
                                .SingleOrDefault(x => x.IsRegistration);
            if (term == null)
            {
                term = _db.Terms.Where(x => x.AcademicLevelId == academiclevelId)
                                .SingleOrDefault(x => x.IsCurrent);
            }

            return term;
        }

        public List<Section> GetSectionByCourseId(long termId, long courseId)
        {
            var sections = _db.Sections.Include(x => x.Course)
                                       .Include(x => x.RegistrationCourses)
                                           .ThenInclude(x => x.Withdrawals)
                                       .Where(x => !x.IsClosed
                                                   && x.CourseId == courseId
                                                   && x.TermId == termId)
                                       .ToList();

            if (sections != null && sections.Any())
            {
                sections = sections.OrderBy(x => x.NumberValue)
                                   .ToList();
            }

            return sections;
        }

        public List<Section> GetSectionByCourseIds(long termId, List<long> courseIds)
        {
            var sections = _db.Sections.Include(x => x.Course)
                                       .Include(x => x.RegistrationCourses)
                                           .ThenInclude(x => x.Withdrawals)
                                       .Where(x => !x.IsClosed
                                                   && x.TermId == termId
                                                   && courseIds.Contains(x.CourseId))
                                       .ToList();
            return sections;
        }

        public List<string> GetSectionNumbers()
        {
            var sections = _db.Sections.Select(x => x.Number)
                                       .Distinct()
                                       .ToList();
            return sections;
        }

        public List<string> GetSectionNumbersByCourses(long termId, List<long> courseIds)
        {
            var sections = _db.Sections.Where(x => x.TermId == termId
                                                   && courseIds.Contains(x.CourseId))
                                       .Select(x => x.Number)
                                       .Distinct()
                                       .ToList();
            return sections;
        }

        public List<Section> GetSectionsByInstructorId(long instructorId, long termId, long courseId)
        {
            var sections = _db.Sections.Include(x => x.SectionDetails)
                                       .Where(x => (instructorId == 0
                                                    || x.MainInstructorId == instructorId)
                                                    && x.TermId == termId
                                                    && x.CourseId == courseId)
                                       .Distinct()
                                       .ToList();
            return sections;
        }

        public ExaminationPeriod GetExamDate(long courseId, long termId)
        {
            var result = (from course in _db.ExaminationCoursePeriods
                          join period in _db.ExaminationPeriods on new { Period = course.Period ?? 0 } equals new { period.Period }
                          where course.CourseId == courseId
                                && period.TermId == termId
                          select new ExaminationPeriod
                          {
                              MidtermDate = course.HasMidterm ? period.MidtermDate : null,
                              MidtermStart = course.HasMidterm ? period.MidtermStart : null,
                              MidtermEnd = course.HasMidterm ? (course.MidtermHour ?? 0) > 0 ? period.MidtermStart + TimeSpan.FromHours(Convert.ToDouble(course.MidtermHour)) : period.MidtermEnd : null,
                              FinalDate = course.HasFinal ? period.FinalDate : null,
                              FinalStart = course.HasFinal ? period.FinalStart : null,
                              FinalEnd = course.HasFinal ? (course.FinalHour ?? 0) > 0 ? period.FinalStart + TimeSpan.FromHours(Convert.ToDouble(course.FinalHour)) : period.FinalEnd : null
                          }).FirstOrDefault();

            return result;
        }

        public Course GetCourse(long courseId)
        {
            var course = _db.Courses.Include(x => x.Faculty)
                                    .Include(x => x.Department)
                                    .SingleOrDefault(x => x.Id == courseId);
            return course;
        }

        public List<Course> GetCoursesByOpenSection(long termId, string sectionStatus)
        {
            var courses = _db.Sections.Include(x => x.Course)
                                      .Where(x => x.TermId == termId)
                                      .Select(x => x.Course)
                                      .Distinct()
                                      .ToList();
            return courses;
        }

        public List<Course> GetCoursesByCloseSection(long termId, string sectionStatus)
        {
            var courseIds = _db.Sections.Where(y => y.TermId == termId)
                                        .Select(x => x.CourseId)
                                        .Distinct()
                                        .ToList();

            var courses = _db.Courses.Where(x => !courseIds.Contains(x.Id))
                                     .ToList();
            return courses;
        }

        public List<Course> GetCourseByIds(List<long> courseIds)
        {
            var courses = _db.Courses.Where(x => courseIds.Contains(x.Id))
                                     .ToList();
            return courses;
        }

        public bool IsExistSection(long termId, long courseId, string sectionNumber)
        {
            var isExist = _db.Sections.Any(x => (x.TermId == termId
                                                 && x.CourseId == courseId
                                                 && x.Number == sectionNumber));
            return isExist;
        }

        public bool SectionExists(Section sectionToBeUpdated, long termId, long courseId, string sectionNumber)
        {
            var exists = _db.Sections.Any(x => x.Id != sectionToBeUpdated.Id
                                               && x.TermId == termId
                                               && x.CourseId == courseId
                                               && x.Number == sectionNumber);

            return exists;
        }

        public Section GetSection(long sectionId)
        {
            var section = _db.Sections.Include(x => x.Course)
                                      .Include(x => x.Term)
                                      .AsNoTracking()
                                      .SingleOrDefault(x => x.Id == sectionId);
            return section;
        }

        public List<Section> GetSections(List<long> sectionIds)
        {
            var sections = _db.Sections.Where(x => sectionIds.Contains(x.Id))
                                       .ToList();
            return sections;
        }

        public List<Section> GetChildSection(long parentId)
        {
            var childs = _db.Sections.Include(x => x.Course)
                                     .Include(x => x.Term)
                                     .Where(x => x.ParentSectionId == parentId)
                                     .AsNoTracking()
                                     .ToList();
            return childs;
        }

        public List<SectionDetail> IsExistSectionDetail(List<SectionDetail> models, long termId, long sectionId)
        {
            List<SectionDetail> duplicateSection = new List<SectionDetail>();
            var days = models.Select(x => x.Day);
            var instructors = models.Select(x => x.InstructorIds);
            var rooms = models.Select(x => x.RoomId);
            if (instructors != null || rooms != null)
            {
                var sectionDetails = _db.SectionDetails.Include(x => x.Section)
                                                           .ThenInclude(x => x.Course)
                                                       .Include(x => x.InstructorSections)
                                                       .Where(x => x.SectionId != sectionId
                                                                   && x.Section.TermId == termId
                                                                   && days.Contains(x.Day)
                                                                   && (x.RoomId == null || rooms.Contains(x.RoomId)));

                foreach (var item in models)
                {
                    var existTime = sectionDetails.Where(x => (item.StartTime == x.StartTime && item.EndTime == x.EndTime)
                                                               || (item.StartTime < x.StartTime && item.EndTime > x.StartTime)
                                                               || (item.StartTime >= x.StartTime && item.StartTime < x.EndTime));

                    foreach (var section in existTime)
                    {
                        var existSection = section.InstructorSections.Where(x => item.Instructors.Contains(x.InstructorId));
                        if (section.RoomId != null || section.InstructorSections.Any(x => item.Instructors.Contains(x.InstructorId)))
                        {
                            duplicateSection.Add(section);
                        }
                    }
                }
            }

            duplicateSection = duplicateSection.Distinct().ToList();
            return duplicateSection;
        }

        public List<ScheduleViewModel> GetAvailableSections(string courseCode, string courseName, string sectionNumber, long termId)
        {
            // var model = (from section in _db.Sections.Include(x => x.SectionDetails)
            //              join course in _db.Courses on section.CourseId equals course.Id
            //              join childSection in _db.SectionDetails on section.ParentSectionId equals childSection.SectionId into childSections
            //              where section.TermId == termId
            //                    && !section.IsParent
            //                    && !section.IsClosed
            //                    && (String.IsNullOrEmpty(courseCode)
            //                        || course.Code.Contains(courseCode))
            //                    && (string.IsNullOrEmpty(courseName)
            //                        || course.NameEn.StartsWith(courseName))
            //                    && (String.IsNullOrEmpty(sectionNumber)
            //                        || section.Number.Contains(sectionNumber))
            //              select new ScheduleViewModel()
            //              {
            //                  Id = section.Id,
            //                  CourseId = section.CourseId,
            //                  CourseName = course.CodeAndName,
            //                  CourseCode = course.Code,
            //                  Section = section.Number,
            //                  SeatLimit = section.SeatLimit,
            //                  SeatAvailable = section.SeatAvailable,
            //                  SeatUsed = section.SeatUsed,
            //                  CourseCreditText = section.Course.CreditText,
            //                  RegistrationCourseCredit = section.Course.RegistrationCredit,
            //                  MainInstructor = section.MainInstructor.Title.NameEn + " " + section.MainInstructor.FirstNameEn + " " + section.MainInstructor.LastNameEn,
            //                  ScheduleTimes = (childSections == null ? section.SectionDetails : section.SectionDetails.Union(childSections))
            //                                          .Select(y => _mapper.Map<SectionDetail, ClassScheduleTimeViewModel>(y))
            //                                          .OrderBy(y => y.Day)
            //                                          .ThenBy(y => y.StartTime)
            //                                          .ToList()
            //              })
            //                     .OrderBy(x => x.CourseCode)
            //                         .ThenBy(x => x.CourseName)
            //                             .ThenBy(x => x.Section)
            //                     .ToList();

            var model = _db.Sections.Where(x => x.TermId == termId
                                                && !x.IsClosed
                                                && (String.IsNullOrEmpty(courseCode)
                                                    || x.Course.Code.Contains(courseCode))
                                                && (string.IsNullOrEmpty(courseName)
                                                    || x.Course.NameEn.StartsWith(courseName))
                                                && (String.IsNullOrEmpty(sectionNumber)
                                                    || x.Number.Contains(sectionNumber)))
                                    .Select(x => new ScheduleViewModel()
                                    {
                                        Id = x.Id,
                                        CourseId = x.CourseId,
                                        CourseName = x.Course.NameEn,
                                        CourseCode = x.Course.Code,
                                        Section = x.Number,
                                        SectionType = x.SectionTypes,
                                        SeatLimit = x.SeatLimit,
                                        SeatAvailable = x.SeatAvailable,
                                        SeatUsed = x.SeatUsed,
                                        CourseCreditText = x.Course.CreditText,
                                        RegistrationCourseCredit = x.Course.RegistrationCredit,
                                        MainInstructor = x.MainInstructor.Title.NameEn + " " + x.MainInstructor.FirstNameEn + " " + x.MainInstructor.LastNameEn,
                                    })
                                    .OrderBy(x => x.CourseCode)
                                        .ThenBy(x => x.CourseName)
                                            .ThenBy(x => x.NumberValue)
                                    .ToList();

            var sectionIds = model.Select(x => x.Id).ToList();
            var sectionDetails = _db.SectionDetails.Include(x => x.Section)
                                                       .ThenInclude(x => x.Course)
                                                   .Where(x => sectionIds.Contains(x.SectionId))
                                                   .Select(y => _mapper.Map<SectionDetail, ClassScheduleTimeViewModel>(y))
                                                   .OrderBy(y => y.Day)
                                                   .ThenBy(y => y.StartTime)
                                                   .ToList();

            foreach (var item in model)
            {
                item.ScheduleTimes = sectionDetails.Where(x => x.SectionId == item.Id).ToList();
            }

            return model;
        }

        public string Serialize(List<long> ids)
        {
            var jsonString = ids == null ? null : JsonConvert.SerializeObject(ids);
            return jsonString;
        }

        public List<long> Deserialize(string jsonString)
        {
            var ids = String.IsNullOrEmpty(jsonString) ? null : JsonConvert.DeserializeObject<List<long>>(jsonString);
            return ids;
        }

        public List<Plan> GetPlans(long termId)
        {
            var plans = _db.Plans.Include(x => x.PlanSchedules)
                                 .Where(x => termId == 0
                                             || x.TermId == termId)
                                 .OrderBy(x => x.CreatedAt)
                                 .ToList();
            return plans;
        }

        public List<PlanSchedule> GetPlanSchedules(long planId)
        {
            var plannedSchedules = _db.PlanSchedules.Include(x => x.Plan)
                                                    .Where(x => x.PlanId == planId)
                                                    .ToList();
            return plannedSchedules;
        }

        public async Task<ApiResponse<string>> SubmitRegistration(string studentCode, string courseSectionRequest)
        {
            var apiResponse = await APIHelper.SubmitRegistration<string>(studentCode, courseSectionRequest, "r");
            return apiResponse;
        }

        
        public List<RegistrationCourse> GetRegistrationCourses(Guid studentId, long termId, List<RegistrationMatchCourse> courses)
        {
            var registrationCourses = new List<RegistrationCourse>();
            // Add
            var addCourses = courses.Where(x => x.RegistrationCourseId == 0);
            foreach (var course in addCourses)
            {
                registrationCourses.Add(new RegistrationCourse
                {
                    StudentId = studentId,
                    TermId = termId,
                    CourseId = course.CourseId,
                    SectionId = course.SectionId,
                    IsPaid = false,
                    IsLock = false,
                    IsSurveyed = false,
                    Status = "a",
                    IsActive = true
                });

                var section = _db.Sections.SingleOrDefault(x => x.Id == course.SectionId);
                if (section != null)
                {
                    AddSeat(section, 1);
                }
            }

            _db.RegistrationCourses.AddRange(registrationCourses);

            // Delete
            var _registrationCourses = GetActiveRegistrationCourses(studentId, termId);
            var deletedCourses = _registrationCourses.Where(x => !courses.Any(y => y.RegistrationCourseId == x.Id));
            foreach (var course in deletedCourses)
            {
                course.Status = "d";
                if (course.Section != null)
                {
                    ReturnSeat(course.Section, 1);
                }
            }

            // Edit
            var chaningSectionCourses = _registrationCourses.Where(x => courses.Any(y => y.RegistrationCourseId == x.Id
                                                                                        && y.CourseId == x.CourseId
                                                                                        && y.SectionId != x.SectionId));
            foreach (var course in chaningSectionCourses)
            {
                long? newSectionId = courses.SingleOrDefault(x => x.CourseId == course.CourseId).SectionId;

                var oldSection = _db.Sections.SingleOrDefault(x => x.Id == course.CourseId);
                var newSection = _db.Sections.SingleOrDefault(x => x.Id == newSectionId);

                if (oldSection != null)
                {
                    ReturnSeat(oldSection, 1);
                }

                if (newSection != null)
                {
                    AddSeat(newSection, 1);
                }

                course.SectionId = newSectionId;
            }

            var registeredCourseLogs = courses.Select(x => $"{ x.CourseCode }({ x.SectionNumber })");

            _db.RegistrationLogs.Add(new RegistrationLog
            {
                Channel = "w",
                StudentId = studentId,
                TermId = termId,
                Modifications = "Modify Registration Courses",
                RegistrationCourses = String.Join(", ", registeredCourseLogs),
                Type = "m",
                Round = 0
            });

            _db.SaveChanges();
            return registrationCourses;
        }

        public bool ModifyRegistrationCourse(List<SelectablePlannedSchedule> addingPlannedSchedules, long termId)
        {
            // Convert SelectablePlannedSchedule to RegistrationCourse
            var coursesToAdd = addingPlannedSchedules.SelectMany(x => JsonConvert.DeserializeObject<List<long>>(x.PlanSchedule.SectionIds)
                                                                                 .SelectMany(y => x.RegistrableStudentIds,
                                                                                             (y, z) => new RegistrationCourse
                                                                                             {
                                                                                                 StudentId = z,
                                                                                                 SectionId = y,
                                                                                                 TermId = termId,
                                                                                                 Status = "a",
                                                                                                 IsActive = true
                                                                                             }))
                                                     .ToList();

            // Get current registered courses of adding students
            var existingCourses = new List<RegistrationCourse>();
            var studentIds = addingPlannedSchedules.SelectMany(x => x.RegistrableStudentIds.SelectMany(y => GetActiveRegistrationCourses(y, termId)))
                                                   .ToList();

            // Initialize comparer for registration couese class
            var comparer = new RegistrationCourseEqualityComparer();

            // Adding courses and remove seat for sections
            var addingCourses = coursesToAdd.Except(existingCourses, comparer)
                                            .Join(_db.Sections.Include(x => x.Course),
                                                  x => x.SectionId,
                                                  y => y.Id,
                                                  (x, y) => new RegistrationCourse
                                                  {
                                                      StudentId = x.StudentId,
                                                      SectionId = x.SectionId,
                                                      Section = y,
                                                      CourseId = y.CourseId,
                                                      TermId = x.TermId,
                                                      Status = x.Status,
                                                      IsActive = x.IsActive
                                                  })
                                            .ToList();

            var registrationLogs = addingCourses.GroupBy(x => x.StudentId)
                                                .Select(x => new RegistrationLog
                                                {
                                                    Channel = "r",
                                                    Type = "r",
                                                    StudentId = x.Key,
                                                    TermId = termId,
                                                    Modifications = "Group Registration Courses",
                                                    RegistrationCourses = string.Join(", ", x.Select(y => $"{ y.Section.Course.Code }({ y.Section.Number })")),
                                                    Round = 1,
                                                    CreatedAt = DateTime.UtcNow,
                                                    CreatedBy = "km."
                                                })
                                                .ToList();

            // Start transaction
            using (var transaction = _db.Database.BeginTransaction())
            {
                var addingSections = addingCourses.GroupBy(x => x.Section)
                                                  .Select(x => new { Section = x.Key, Count = x.Count() });

                foreach (var item in addingSections)
                {
                    AddSeat(item.Section, item.Count);
                }

                _db.RegistrationCourses.AddRange(addingCourses);
                _db.RegistrationLogs.AddRange(registrationLogs);

                try
                {
                    _db.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }

            return true;
        }

        public List<Section> GetSectionsBySectionIds(List<long> sectionIds)
        {
            var sections = _db.Sections.Include(x => x.Course)
                                       .Where(x => sectionIds.Contains(x.Id))
                                       .OrderBy(x => x.NumberValue)
                                       .ToList();

            return sections;
        }

        public int GetRegisteringCredit(long registrationTermId, Guid studentId)
        {
            var registertedCredits = _db.RegistrationCourses.Include(x => x.Course)
                                                            .Where(x => x.TermId == registrationTermId
                                                                        && x.StudentId == studentId
                                                                        && x.Status != "d")
                                                            .Sum(x => x.Course.Credit);
            return registertedCredits;
        }

        public int GetAccumulativeCredit(Guid studentId)
        {
            var registertedCredits = _db.RegistrationCourses.Include(x => x.Course)
                                                            .Where(x => x.StudentId == studentId
                                                                        && (x.Status != "d"
                                                                            || x.Grade.Name.ToUpper() == "W")
                                                                        && x.GradeName != "x")
                                                            .Sum(x => x.Course.Credit);
            return registertedCredits;
        }

        public int GetAccumulativeRegistrationCredit(Guid studentId)
        {
            var registertedCredits = _db.RegistrationCourses.Include(x => x.Course)
                                                            .Where(x => x.StudentId == studentId
                                                                        && (x.Status != "d"
                                                                            || x.Grade.Name.ToUpper() == "W")
                                                                        && x.GradeName != "x")
                                                            .Sum(x => x.Course.RegistrationCredit);
            return registertedCredits;
        }

        private class RegistrationCourseEqualityComparer : IEqualityComparer<RegistrationCourse>
        {
            public bool Equals(RegistrationCourse x, RegistrationCourse y)
            {
                var result = x.SectionId == y.SectionId && x.StudentId == y.StudentId;
                return result;
            }

            public int GetHashCode(RegistrationCourse obj)
            {
                var hashCode = new { obj.SectionId, obj.StudentId }.GetHashCode();
                return hashCode;
            }
        }
        public decimal GetTotalRegistrationStudentByTermId(long termId)
        {
            decimal studentCount = _db.RegistrationCourses.Where(x => x.TermId == termId)
                                                          .Select(x => x.StudentId)
                                                          .Distinct()
                                                          .Count();
            return studentCount;
        }

        public string GetSectionNumberByRegistrationId(long registrationId)
        {
            var registration = _db.RegistrationCourses.Include(x => x.Section)
                                                      .FirstOrDefault(x => x.Id == registrationId);

            if (registration == null || registration.Section == null)
            {
                return "null";
            }

            return registration.Section.Number;
        }

        public string GetCreditMessage(RegistrationViewModel model)
        {
            if (model.AddingResults != null)
            {
                var courseIds = model.AddingResults.Select(x => x.CourseId).ToList();
                var courses = _db.Courses.Where(x => courseIds.Contains(x.Id))
                                         .Select(x => new
                                         {
                                             CourseId = x.Id,
                                             Credit = x.RegistrationCredit
                                         })
                                         .ToList();
                var sumCredit = courses.Sum(x => x.Credit);

                if (sumCredit > 0)
                {
                    var student = _db.Students.Where(x => x.Id == model.StudentId)
                                              .Select(x => new
                                              {
                                                  MaximumCredit = x.AcademicInformation.MaximumCredit,
                                                  MinimumCredit = x.AcademicInformation.MinimumCredit
                                              })
                                              .SingleOrDefault();

                    if (sumCredit < student.MinimumCredit || sumCredit > student.MaximumCredit)
                    {
                        return $" { sumCredit } ({ student.MinimumCredit } - { student.MaximumCredit }) \n";
                    }
                }
            }

            return "";
        }

        public RegistrationCourse GetRegistrationCourse(long id)
        {
            var registrationCourse = _db.RegistrationCourses.Include(x => x.Grade)
                                                            .Include(x => x.Course)
                                                            .Include(x => x.Section)
                                                                .ThenInclude(x => x.MainInstructor)
                                                            .Include(x => x.Student)
                                                            .SingleOrDefault(x => x.Id == id);
            return registrationCourse;
        }

        public List<RegistrationCourse> GetRegistrationCoursesByIds(List<long> ids)
        {
            var registrationCourse = _db.RegistrationCourses.Include(x => x.Grade)
                                                            .Include(x => x.Course)
                                                            .Where(x => ids.Contains(x.Id))
                                                            .ToList();
            return registrationCourse;
        }

        public RegistrationCourse GetStudentRegistrationCourseBySection(Guid studentId, long sectionId)
        {
            var registrationCourse = _db.RegistrationCourses.SingleOrDefault(x => x.StudentId == studentId
                                                                                  && x.SectionId == sectionId
                                                                                  && (x.Status == "a"
                                                                                      || x.Status == "r"));
            return registrationCourse;
        }

        public List<long> GetSectionIdsByCoursesRange(long termId, string courseCode, int courseNumberFrom, int courseNumberTo, int sectionFrom, int sectionTo)
        {
            var sectionIds = new List<long>();
            var courses = GetCourseCodeRanges(courseCode, courseNumberFrom, courseNumberTo);
            if (courses != null)
            {
                var courseIds = courses.Select(x => x.CourseId)
                                       .ToList();

                sectionIds = _db.Sections.Where(x => courseIds.Contains(x.CourseId)
                                                     && x.TermId == termId
                                                     && !x.IsClosed
                                                     && (sectionFrom == 0
                                                         || x.NumberValue >= sectionFrom)
                                                     && (sectionTo == 0
                                                         || x.NumberValue <= sectionTo))
                                         .Select(x => x.Id)
                                         .ToList();
            }

            return sectionIds;
        }

        public List<CourseCodeRangeViewModel> GetCourseCodeRanges(string courseCode, int courseNumberFrom, int courseNumberTo)
        {
            int codeLength = courseCode.Length;
            var courseCodes = _db.Courses.Where(x => x.Code.StartsWith(courseCode)
                                                     && x.TransferUniversityId == null)
                                         .Select(x => new CourseCodeRangeViewModel
                                         {
                                             CourseId = x.Id,
                                             CourseCode = x.Code,
                                             SubStringCode = x.Code.Substring(codeLength, x.Code.Length - 1)
                                         })
                                         .ToList();

            courseCodes = courseCodes.Where(x => x.SubStringCodeInt >= courseNumberFrom
                                                 && x.SubStringCodeInt <= courseNumberTo
                                                 && courseNumberFrom <= courseNumberTo)
                                     .ToList();
            return courseCodes;
        }

        public List<CloseSectionStudentList> GetCloseSectionStudentListsByStudents(List<StudentListViewModel> studentList, long sectionId)
        {
            var result = new List<CloseSectionStudentList>();

            var section = _db.Sections.Where(x => x.Id == sectionId)
                                      .Select(x => new
                                                    {
                                                        SectionId = x.Id,
                                                        CourseCode = x.Course.Code,
                                                        Number = x.Number,
                                                        ParentSectionId = x.ParentSectionId,
                                                        SectionTypes = x.SectionTypes
                                                    })
                                       .First();

            var studentCodes = studentList.Select(x => x.StudentCode).ToList();
            var students = _db.RegistrationCourses.Where(x => x.SectionId == sectionId
                                                     && studentCodes.Contains(x.Student.Code)
                                                     && x.Status != "d")
                                               .Select(x => new CloseSectionStudentList
                                                            {
                                                                StudentCode = x.Student.Code,
                                                                StudentTitle = x.Student.Title.NameEn,
                                                                StudentFirstName = x.Student.FirstNameEn,
                                                                StudentLastNameName = x.Student.LastNameEn,
                                                                CourseCode = x.Course.Code,
                                                                SectionNumber = x.Section.Number,
                                                                SectionType = x.Section.SectionTypes,
                                                                Division = x.Student.AcademicInformation.Faculty.NameEn,
                                                                Major = x.Student.AcademicInformation.Department.ShortNameEn,
                                                                Email = x.Student.Email,
                                                                PaymentStatus = x.IsPaid
                                                            })
                                               .OrderByDescending(x => x.SectionType)
                                                   .ThenBy(x => x.CourseCode)
                                                   .ThenBy(x => x.StudentCode)
                                               .ToList();
            foreach(var item in studentList)
            {
                var student = students.Where(x => x.StudentCode == item.StudentCode).FirstOrDefault();
                if(student == null)
                {
                    student = _db.Students.Where(x => x.Code == item.StudentCode)
                                          .IgnoreQueryFilters()
                                          .Select(x => new CloseSectionStudentList
                                                       {
                                                           StudentCode = x.Code,
                                                           StudentTitle = x.Title.NameEn,
                                                           StudentFirstName = x.FirstNameEn,
                                                           StudentLastNameName = x.LastNameEn,
                                                           CourseCode = section.CourseCode,
                                                           SectionNumber = section.Number,
                                                           SectionType = section.SectionTypes,
                                                           Division = x.AcademicInformation.Faculty.NameEn,
                                                           Major = x.AcademicInformation.Department.ShortNameEn,
                                                           Email = x.Email,
                                                           PaymentStatus = item.IsPaid
                                                       }).FirstOrDefault();
                    if (student != null)
                    {
                        result.Add(student);
                    }
                }
                else
                {
                    result.Add(student);
                }
            }
            
            return result;
        }

        public List<SignatureSheetStudentDetail> GetSignatureSheetStudentListsByStudents(List<StudentListViewModel> studentList, long sectionId)
        {
            var result = new List<SignatureSheetStudentDetail>();

            var section = _db.Sections.Where(x => x.Id == sectionId)
                                      .Select(x => new
                                                    {
                                                        SectionId = x.Id,
                                                        CourseCode = x.Course.Code,
                                                        Number = x.Number,
                                                        ParentSectionId = x.ParentSectionId,
                                                        SectionTypes = x.SectionTypes,
                                                        TermId = x.TermId
                                                    })
                                       .First();

            var studentCodes = studentList.Select(x => x.StudentCode).ToList();

            var students = _db.RegistrationCourses.Where(x => x.SectionId == sectionId
                                                        && studentCodes.Contains(x.Student.Code)
                                                        && x.Status != "d")
                                                  .Select(x => new SignatureSheetStudentDetail
                                                               {
                                                                   CourseCode = x.Course.Code,
                                                                   Code = x.Student.Code,
                                                                   Title = x.Student.Title.NameEn,
                                                                   LastName = x.Student.LastNameEn,
                                                                   MidName = x.Student.MidNameEn,
                                                                   FirstName = x.Student.FirstNameEn,
                                                                   Faculty = x.Student.AcademicInformation.Department.Faculty.Abbreviation,
                                                                   Department = x.Student.AcademicInformation.Department.Abbreviation,
                                                                   PaidStatus = x.IsPaid ? "Paid" : "Unpaid",
                                                                   WithdrawnStatus = x.GradeName == "W" ? "Withdrawn" : null,
                                                                   CreatedAt = x.CreatedAt
                                                               })
                                                  .ToList();
            foreach(var item in studentList)
            {
                var student = students.Where(x => x.Code == item.StudentCode).FirstOrDefault();
                if(student == null)
                {
                    student = _db.Students.Where(x => x.Code == item.StudentCode)
                                          .IgnoreQueryFilters()
                                          .Select(x => new SignatureSheetStudentDetail
                                                       {
                                                          CourseCode = section.CourseCode,
                                                           Code = x.Code,
                                                           Title = x.Title.NameEn,
                                                           LastName = x.LastNameEn,
                                                           MidName = x.MidNameEn,
                                                           FirstName = x.FirstNameEn,
                                                           Faculty = x.AcademicInformation.Department.Faculty.Abbreviation,
                                                           Department = x.AcademicInformation.Department.Abbreviation,
                                                           PaidStatus = item.IsPaid ? "Paid" : "Unpaid",
                                                           WithdrawnStatus = null, //should be, as to be withdraw must have data in keystone already
                                                           CreatedAt = item.CreatedAt
                                                       }).FirstOrDefault();
                    if (student != null)
                    {
                        result.Add(student);
                    }
                }
                else
                {
                    result.Add(student);
                }
            }
            
            return result;
        }

        public List<StudentApiViewModel> GetRegistrationCourseByStudentCodes(List<StudentListViewModel> studentList, long sectionId)
        {
            var studentCodes = studentList.Select(x => x.StudentCode).ToList();
            var section = _db.Sections.Where(x => x.Id == sectionId)
                                      .Select(x => new
                                                    {
                                                        SectionId = x.Id,
                                                        CourseCode = x.Course.Code,
                                                        Number = x.Number,
                                                        ParentSectionId = x.ParentSectionId
                                                    })
                                       .First();

            var result = new List<StudentApiViewModel>();
            var students =  _db.RegistrationCourses.AsNoTracking()
                                                   .Where(x => studentCodes.Contains(x.Student.Code)
                                                               && x.SectionId == sectionId
                                                               && x.Status != "d")
                                                   .Select(x => new StudentApiViewModel
                                                                   {
                                                                       StudentCode = x.Student.Code,
                                                                       SectionId = x.SectionId ?? 0,
                                                                       CourseCode = x.Course.Code,
                                                                       SectionNumber = x.Section.Number,
                                                                       ParentSectionId = x.Section.ParentSectionId,
                                                                       TitleEn = x.Student.Title.NameEn,
                                                                       NameEn = x.Student.FirstNameEn,
                                                                       MidNameEn = x.Student.MidNameEn,
                                                                       LastNameEn = x.Student.LastNameEn,
                                                                       Email = x.Student.Email,
                                                                       PersonalEmail = x.Student.PersonalEmail,
                                                                       IsPaid = x.IsPaid,
                                                                       Withdraw = x.GradeName == "W" ? "Yes" : "No",
                                                                       Department = x.Student.AcademicInformation.Department == null ? null
                                                                                                                                     : new DepartmentApiViewModel
                                                                                                                                         {
                                                                                                                                             DepartmentCode = x.Student.AcademicInformation.Department.Code,
                                                                                                                                             DepartmentNameEn = x.Student.AcademicInformation.Department.NameEn,
                                                                                                                                             ShortNameEn = x.Student.AcademicInformation.Department.ShortNameEn
                                                                                                                                         },
                                                                       Faculty = x.Student.AcademicInformation.Faculty == null ? null
                                                                                                                               : new FacultyApiViewModel
                                                                                                                               {
                                                                                                                                   FacultyCode = x.Student.AcademicInformation.Faculty.Code,
                                                                                                                                   FacultyNameEn = x.Student.AcademicInformation.Faculty.NameEn,
                                                                                                                                   ShortNameEn = x.Student.AcademicInformation.Faculty.ShortNameEn
                                                                                                                               },
                                                                       CurriculumVersion = x.Student.AcademicInformation.CurriculumVersion == null ? null 
                                                                                                                                                   : new CurriculumVersionApiViewModel
                                                                                                                                                   {
                                                                                                                                                       Code = x.Student.AcademicInformation.CurriculumVersion.Code,
                                                                                                                                                       NameEn = x.Student.AcademicInformation.CurriculumVersion.NameEn
                                                                                                                                                   }
                                                                   })
                                                   .ToList();
            foreach(var item in studentList)
            {
                var student = students.Where(x => x.StudentCode == item.StudentCode).FirstOrDefault();
                if(student == null)
                {
                    student = _db.Students.IgnoreQueryFilters()
                                          .Where(x => x.Code == item.StudentCode)
                                          .Select(x => new StudentApiViewModel
                                                       {
                                                           StudentCode = x.Code,
                                                           SectionId = section.SectionId,
                                                           CourseCode = section.CourseCode,
                                                           SectionNumber = section.Number,
                                                           ParentSectionId = section.ParentSectionId,
                                                           TitleEn = x.Title.NameEn,
                                                           NameEn = x.FirstNameEn,
                                                           MidNameEn = x.MidNameEn,
                                                           LastNameEn = x.LastNameEn,
                                                           Email = x.Email,
                                                           PersonalEmail = x.PersonalEmail,
                                                           IsPaid = item.IsPaid,
                                                           Withdraw = "No",
                                                           Department = x.AcademicInformation.Department == null ? null
                                                                                                                 : new DepartmentApiViewModel
                                                                                                                     {
                                                                                                                         DepartmentCode = x.AcademicInformation.Department.Code,
                                                                                                                         DepartmentNameEn = x.AcademicInformation.Department.NameEn,
                                                                                                                         ShortNameEn = x.AcademicInformation.Department.ShortNameEn
                                                                                                                     },
                                                           Faculty = x.AcademicInformation.Faculty == null ? null
                                                                                                           : new FacultyApiViewModel
                                                                                                           {
                                                                                                               FacultyCode = x.AcademicInformation.Faculty.Code,
                                                                                                               FacultyNameEn = x.AcademicInformation.Faculty.NameEn,
                                                                                                               ShortNameEn = x.AcademicInformation.Faculty.ShortNameEn
                                                                                                           },
                                                           CurriculumVersion = x.AcademicInformation.CurriculumVersion == null ? null 
                                                                                                                               : new CurriculumVersionApiViewModel
                                                                                                                               {
                                                                                                                                   Code = x.AcademicInformation.CurriculumVersion.Code,
                                                                                                                                   NameEn = x.AcademicInformation.CurriculumVersion.NameEn
                                                                                                                               }
                                                       }).FirstOrDefault();
                    if (student != null)
                    {
                        result.Add(student);
                    }
                }
                else
                {
                    result.Add(student);
                }
            }

            return result;
        }

        public List<AttendanceStudent> GetAttendanceSheetStudentListsByStudents(List<StudentListViewModel> studentList, long sectionId)
        {
            var result = new List<AttendanceStudent>();

            var section = _db.Sections.Where(x => x.Id == sectionId)
                                      .Select(x => new
                                                    {
                                                        SectionId = x.Id,
                                                        CourseCode = x.Course.Code,
                                                        Number = x.Number,
                                                        ParentSectionId = x.ParentSectionId,
                                                        SectionTypes = x.SectionTypes
                                                    })
                                       .First();

            var studentCodes = studentList.Select(x => x.StudentCode).ToList();
            var students = _db.RegistrationCourses.Where(x => x.SectionId == sectionId
                                                     && studentCodes.Contains(x.Student.Code)
                                                     && x.Status != "d")
                                               .Select(x => new AttendanceStudent
                                                            {
                                                                StudentCode = x.Student.Code,
                                                                ProfileImageURL = x.Student.ProfileImageURL,
                                                                Title = x.Student.Title.NameEn,
                                                                LastName = x.Student.LastNameEn,
                                                                MidName = x.Student.MidNameEn,
                                                                FirstName = x.Student.FirstNameEn,
                                                                CourseCode = section.CourseCode,
                                                                Department = x.Student.AcademicInformation.Department.Abbreviation,
                                                                Faculty = x.Student.AcademicInformation.Faculty.Abbreviation,
                                                                IsPaid = x.IsPaid,
                                                                PaidStatus = x.GradeName.ToUpper() == "W" ? "Withdrawn" : x.IsPaid ? "Paid" : "Unpaid",
                                                                CreatedAt = x.CreatedAt
                                                            })
                                               .ToList();
            foreach(var item in studentList)
            {
                var student =  students.Where(x => x.StudentCode == item.StudentCode).FirstOrDefault();
                if(student == null)
                {
                    student = _db.Students.Where(x => x.Code == item.StudentCode)
                                          .IgnoreQueryFilters()
                                          .Select(x => new AttendanceStudent
                                                       {
                                                           StudentCode = x.Code,
                                                           ProfileImageURL = x.ProfileImageURL,
                                                           Title = x.Title.NameEn,
                                                           LastName = x.LastNameEn,
                                                           MidName = x.MidNameEn,
                                                           FirstName = x.FirstNameEn,
                                                           CourseCode = section.CourseCode,
                                                           Department = x.AcademicInformation.Department.Abbreviation,
                                                           Faculty = x.AcademicInformation.Faculty.Abbreviation,
                                                           IsPaid = item.IsPaid,
                                                           CreatedAt = item.CreatedAt
                                                       }).FirstOrDefault();
                    if (student != null)
                    {
                        result.Add(student);
                    }
                }
                else
                {
                    result.Add(student);
                }
            }
            
            return result;
        }

        public List<StudentCourseEquivalentViewModel> GetRegistrationEquivalentCourses(List<StudentTransferCourseViewModel> courseList, long curriculumVersionId)
        {
            var now = DateTime.UtcNow;
            var courseIds = courseList.Select(x => x.CourseId)
                                      .ToList();

            var curriculumDependencies = _db.CurriculumDependencies.Where(x => x.DependencyType == "Equivalence" && x.CurriculumVersionId == curriculumVersionId).ToList();
            var courseEquivalents = (from courseEquivalent in _db.CourseEquivalents
                                     join course1 in _db.Courses on courseEquivalent.CourseId equals course1.Id
                                     join course2 in _db.Courses on courseEquivalent.EquilaventCourseId equals course2.Id
                                     where curriculumDependencies.Select(x => x.DependencyId).Contains(courseEquivalent.Id)
                                           && (courseEquivalent.EffectivedAt == null
                                               || courseEquivalent.EffectivedAt.Date <= now)
                                           && (courseEquivalent.EndedAt == null
                                               || courseEquivalent.EndedAt >= now)
                                     select new
                                     {
                                         courseEquivalent,
                                         course1,
                                         course2
                                     }).ToList();

            var newCurriculumCourses = (from courseGroup in _db.CourseGroups
                                        join curriculumCourse in _db.CurriculumCourses on courseGroup.Id equals curriculumCourse.CourseGroupId
                                        join course in _db.Courses on curriculumCourse.CourseId equals course.Id
                                        where courseGroup.CurriculumVersionId == curriculumVersionId
                                        select course).ToList();

            var studentCourseEquivalents = new List<StudentCourseEquivalentViewModel>();
            var allCourses = _db.Courses.Where(x => courseList.Select(y => y.CourseId).Contains(x.Id)).ToList();
            var grades = _db.Grades.Where(x => courseList.Select(y => y.GradeId).Contains(x.Id)).ToList();

            foreach (var course in courseList)
            {
                if (course.CourseId != 0)
                {
                    var _course = allCourses.SingleOrDefault(x => x.Id == course.CourseId);
                    var grade = _db.Grades.SingleOrDefault(x => x.Id == course.GradeId);
                    course.CourseCode = _course?.Code;
                    course.CourseName = _course?.CourseAndCredit ?? "N/A";
                    course.GradeName = grade?.Name;

                    var equivalent = new StudentCourseEquivalentViewModel();
                    equivalent.RegistrationCourseId = course.RegistrationCourseId;
                    equivalent.CurrentCourseId = course.CourseId;
                    if (course.CourseName == null)
                    {
                        equivalent.CurrentCourseName = allCourses.SingleOrDefault(x => x.Id == course.CourseId)?.CourseAndCredit ?? "N/A";
                        equivalent.CurrentCourseGrade = grades.SingleOrDefault(x => x.Id == course.GradeId).Name ?? "N/A";
                    }
                    else
                    {
                        equivalent.CurrentCourseName = $"{ course.CourseName }";
                        equivalent.CurrentCourseGrade = course.GradeName;
                    }

                    equivalent.TermId = course.TermId;
                    equivalent.TermText = _db.Terms.SingleOrDefault(x => x.Id == course.TermId).TermText;
                    equivalent.SectionId = course.SectionId;
                    equivalent.GradeId = course.GradeId;
                    equivalent.GradeName = grade?.Name;
                    equivalent.PreviousGrade = grade?.Name;
                    equivalent.NewGradeId = grade?.Id;
                    equivalent.IsChecked = "on";
                    var equipvalentNewCourseId = courseEquivalents.Select(x => x.course1.Id)
                                                                  .ToList();
                    var equipvalentCourseId = courseEquivalents.Select(x => x.course2.Id)
                                                               .ToList();

                    var courseSelectList = new List<SelectListItem>();
                    var selectList = new List<SelectListItem>();
                    if (newCurriculumCourses.Any() && newCurriculumCourses.Select(x => x.Id).Contains(course.CourseId))
                    {
                        equivalent.InCurriculum = true;
                        selectList = (from newCurriculumCourse in newCurriculumCourses
                                      select new SelectListItem
                                      {
                                          Text = newCurriculumCourse.CourseAndCredit,
                                          Value = newCurriculumCourse.Id.ToString()
                                      }).OrderBy(x => x.Text)
                                               .ToList();
                    }
                    else if (courseEquivalents.Any()
                             && courseEquivalents.Select(x => x.course1.Id).Contains(course.CourseId))
                    {
                        equivalent.InCurriculum = true;
                        selectList = (from courseEquivalent in courseEquivalents
                                      join newCourse in _db.Courses on courseEquivalent.courseEquivalent.EquilaventCourseId equals newCourse.Id
                                      select new SelectListItem
                                      {
                                          Text = newCourse.CourseAndCredit,
                                          Value = newCourse.Id.ToString()
                                      }).OrderBy(x => x.Text)
                                               .ToList();
                    }
                    else
                    {
                        equivalent.InCurriculum = false;
                        selectList = (from noCourse in _db.Courses
                                      select new SelectListItem
                                      {
                                          Text = noCourse.CourseAndCredit,
                                          Value = noCourse.Id.ToString()
                                      }).OrderBy(x => x.Text)
                                               .ToList();
                    }

                    if (selectList.Any())
                    {
                        courseSelectList.Add(new SelectListItem() { Value = null, Text = "-" });
                        courseSelectList.AddRange(selectList);
                    }

                    equivalent.CourseSelectList = new SelectList(selectList, "Value", "Text");
                    if (selectList.Any())
                    {
                        equivalent.NewCourseName = selectList.Select(x => x.Value).Contains(course.CourseId.ToString()) ? selectList.FirstOrDefault(x => x.Value == course.CourseId.ToString()).Text : selectList.FirstOrDefault().Text;
                        equivalent.NewCourseId = course.CourseId;
                    }

                    studentCourseEquivalents.Add(equivalent);
                }
            }

            return studentCourseEquivalents;
        }


        public List<StudentTransferCourseViewModel> GetRegistrationCourses(Guid studentId)
        {
            var studentAcademicInformation = _studentProvider.GetAcademicInformationByStudentId(studentId);
            if (studentAcademicInformation == null)
            {
                return null;
            }

            var result = (from registrationCourse in _db.RegistrationCourses
                          join course in _db.Courses on registrationCourse.CourseId equals course.Id
                          join student in _db.Students on registrationCourse.StudentId equals student.Id
                          join term in _db.Terms on registrationCourse.TermId equals term.Id
                          join section in _db.Sections on registrationCourse.SectionId equals section.Id into sectionTmp
                          from section in sectionTmp.DefaultIfEmpty()
                          where student.Id == studentId
                                && term.AcademicLevelId == studentAcademicInformation.AcademicLevelId
                                && registrationCourse.GradeId != null
                                && registrationCourse.Status.ToLower() != "d"
                          orderby term.AcademicYear, term.AcademicTerm
                          select new StudentTransferCourseViewModel()
                          {
                              RegistrationCourseId = registrationCourse.Id,
                              CourseId = course.Id,
                              CourseName = course.NameEnAndCredit,
                              CourseCode = course.Code,
                              TermId = term.Id,
                              TermText = term.TermText,
                              SectionId = section == null ? 0 : section.Id,
                              SectionNumber = section == null ? "-" : section.Number,
                              GradeId = registrationCourse.GradeId,
                              GradeName = registrationCourse.GradeName
                          }).ToList();

            var courseGroups = (from curriculumCourse in _db.CurriculumCourses
                                join courseGroup in _db.CourseGroups on curriculumCourse.CourseGroupId equals courseGroup.Id
                                where courseGroup.CurriculumVersionId == studentAcademicInformation.CurriculumVersionId
                                select new
                                {
                                    CurriculumCourse = curriculumCourse,
                                    CourseGroup = courseGroup
                                }).ToList();

            foreach (var item in result)
            {
                var courseGroup = courseGroups.FirstOrDefault(x => x.CurriculumCourse.CourseId == item.CourseId);
                if (courseGroup != null)
                {
                    item.CourseGroupId = courseGroup.CourseGroup.Id;
                    item.CourseGroupName = courseGroup.CourseGroup.NameEn;
                }
                else
                {
                    item.CourseGroupId = 0;
                    item.CourseGroupName = "No Course Group";
                }
            }

            return result;
        }

        public bool ChangeCurriculum(StudentTransferViewModel model, Student student, Term term)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                { 
                    // ACADEMIC INFO
                    var newCurriculumVersion = _db.CurriculumVersions.Include(x => x.Curriculum)
                                                                        .ThenInclude(x => x.Faculty)
                                                                     .Include(x => x.Curriculum)
                                                                        .ThenInclude(x => x.Department)
                                                                     .SingleOrDefault(x => x.Id == model.CurriculumVersionId);
                    
                    long oldCurriculumVersionId = 0;
                    var academicInformation = _db.AcademicInformations.SingleOrDefault(x => x.StudentId == model.StudentId);
                    if (academicInformation != null && newCurriculumVersion != null)
                    {
                        // UPDATE ACADEMIC INFO
                        oldCurriculumVersionId = academicInformation.CurriculumVersionId ?? 0;   
                        academicInformation.FacultyId = newCurriculumVersion.Curriculum.FacultyId;
                        academicInformation.DepartmentId = newCurriculumVersion.Curriculum.DepartmentId;
                        academicInformation.CurriculumVersionId = model.CurriculumVersionId;

                        // COUNT CHANGED TIME
                        if (model.CountChangedTime)
                        {
                            academicInformation.ChangedMajorCount += 1;
                        }

                        // UPDATE CURRICULUM INFO
                        var curriculumInfo = _db.CurriculumInformations.Include(x => x.Faculty)
                                                                       .Include(x => x.Department)
                                                                       .SingleOrDefault(x => x.StudentId == model.StudentId
                                                                                             && x.CurriculumVersionId == oldCurriculumVersionId);                                    
                                   
                        if (curriculumInfo != null)
                        {
                            curriculumInfo.FacultyId = newCurriculumVersion.Curriculum.FacultyId;
                            curriculumInfo.DepartmentId = newCurriculumVersion.Curriculum.DepartmentId;
                            curriculumInfo.CurriculumVersionId = model.CurriculumVersionId;
                        } 
                    }

                    // STUDENT TRASFER LOG
                    var studentTransferLog = new StudentTransferLog();
                    studentTransferLog.TermId = term.Id;
                    studentTransferLog.StudentId = model.StudentId;
                    studentTransferLog.Source = "Change Curriculum";
                    studentTransferLog.Remark = String.Format("('Old Curriculum Version Id':{0}, 'New Curriculum Version Id':{1})", oldCurriculumVersionId, model.CurriculumVersionId);
                    studentTransferLog.IsActive = true;
                    studentTransferLog.StudentTransferLogDetails = new List<StudentTransferLogDetail>();
                    _db.StudentTransferLogs.Add(studentTransferLog);

                    // REGISTRATION COURSES
                    var registrationCourses = _db.RegistrationCourses.Where(x => x.StudentId == model.StudentId
                                                                                 && x.Status.ToLower() != "d"
                                                                                 && x.GradeId != null)
                                                                     .ToList();

                    var transferCourseToUSpark = new KeystoneLibrary.Models.USpark.USparkTransferCourse
                    {
                        StudentCode = model.StudentCode,
                        OldCourses = new List<USparkTransferCourse.TransferCourseDetail>(),
                        NewCourses = new List<USparkTransferCourse.TransferCourseDetail>()
                    };


                    // DELETE OLD RECORDS
                    var registrationCourseIds = model.StudentCourseEquivalents.Select(x => x.RegistrationCourseId).ToList();
                    var deleteCourses = registrationCourses.Where(x => !registrationCourseIds.Contains(x.Id)).ToList();
                    foreach (var deleteCourse in deleteCourses)
                    {
                        deleteCourse.IsTransferCourse = true;
                        deleteCourse.Status = "d";
                        deleteCourse.IsActive = false;

                        transferCourseToUSpark.OldCourses.Add(new KeystoneLibrary.Models.USpark.USparkTransferCourse.TransferCourseDetail
                        {
                            KsCourseId = deleteCourse.CourseId,
                            KsSectionId = deleteCourse.SectionId,
                            KsTermId = deleteCourse.TermId,
                            Grade = deleteCourse.GradeName
                        }
                       );
                    }

                    var grades = _db.Grades.AsNoTracking().ToList();
                    
                    foreach (var course in model.StudentCourseEquivalents)
                    {
                        var currentCourse = registrationCourses.SingleOrDefault(x => x.Id == course.RegistrationCourseId);
                        if (course.NewGradeId != null 
                            && course.NewGradeId != currentCourse.GradeId)
                        {
                            // UPDATE CURRENT COURSE TO TRANSFER BECOURSE GRADE CHANGED
                            currentCourse.IsTransferCourse = true;
                            currentCourse.Status = "d";
                            currentCourse.IsActive = false;

                            transferCourseToUSpark.OldCourses.Add(new KeystoneLibrary.Models.USpark.USparkTransferCourse.TransferCourseDetail
                                                                  {
                                                                        KsCourseId = currentCourse.CourseId,
                                                                        KsSectionId = currentCourse.SectionId,
                                                                        KsTermId = currentCourse.TermId,
                                                                        Grade = currentCourse.GradeName
                                                                  });

                            // ADD NEW REGISTRATION COURSE
                            var registrationCourse = new RegistrationCourse()
                            {
                                StudentId = model.StudentId,
                                TermId = course.TermId,
                                CourseId = course.NewCourseId,
                                SectionId = currentCourse != null ? (course.NewCourseId == currentCourse.CourseId ? currentCourse.SectionId : null) : null,
                                GradeId = course.NewGradeId,
                                GradeName = grades.SingleOrDefault(x => x.Id == course.NewGradeId)?.Name ?? "N/A",
                                IsSurveyed = currentCourse != null ? currentCourse.IsSurveyed : true,
                                IsTransferCourse = currentCourse != null ? currentCourse.IsTransferCourse : false,
                                IsStarCourse = course.IsStarCourse,
                                IsPaid = currentCourse != null ? currentCourse.IsPaid : true,
                                IsGradePublished = currentCourse != null ? currentCourse.IsGradePublished : true,
                                Status = "a"
                            };

                            _db.RegistrationCourses.Add(registrationCourse);

                            var detail = new StudentTransferLogDetail()
                            {
                                StudentTransferLogId = studentTransferLog.Id,
                                PreviousRegistrationCourseId = currentCourse?.Id,
                                PreviousGrade = currentCourse?.GradeName ?? "",
                                CourseId = course.NewCourseId,
                                RegistrationCourseId = registrationCourse.Id,
                                CourseGroupId = null
                            };

                            _db.StudentTransferLogDetails.Add(detail);

                            transferCourseToUSpark.NewCourses.Add(new KeystoneLibrary.Models.USpark.USparkTransferCourse.TransferCourseDetail
                                {
                                    KsCourseId = registrationCourse.CourseId,
                                    KsSectionId = registrationCourse.SectionId,
                                    KsTermId = registrationCourse.TermId,
                                    Grade = registrationCourse.GradeName
                                }
                            );
                        }
                    }

                    TransferCourseToUspark(transferCourseToUSpark);

                    // UPDATE GPA
                    _studentProvider.UpdateTermGrade(model.StudentId, term.Id);
                    _studentProvider.UpdateCGPA(model.StudentId);

               
                    _db.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }

            return true;
        }

        public List<Term> GetStudentRegistrationTerms(string studentCode)
        {
            var terms = _db.RegistrationCourses.Include(x => x.Student)
                                               .Include(x => x.Term)
                                               .Where(x => x.Student.Code == studentCode
                                                           && x.Status.ToLower() != "d")
                                               .Select(x => x.Term)
                                               .Distinct()
                                               .OrderByDescending(x => x.AcademicYear)
                                                   .ThenByDescending(x => x.AcademicTerm)
                                               .ToList();
            return terms;
        }

        public bool IsRegistrationPeriod(DateTime datetime, long termId)
        {
            return _db.RegistrationTerms.Any(x => x.TermId == termId
                                                  && x.StartedAt != null
                                                  && x.StartedAt.Value.Date <= datetime.Date
                                                  && (x.EndedAt == null || x.EndedAt.Value.Date >= datetime.Date));
        }

        public void CallUSparkAPIAddSection(long sectionId)
        {
            var client = new RestClient($"{ _USparkAPIURL }/sections");
            //client.Timeout = -1;
            //var request = new RestRequest(Method.POST);
            var request = new RestRequest
            {
                Timeout = Timeout.InfiniteTimeSpan,
                Method = Method.Post
            };
            request.AddHeader("x-api-key", _USparkAPIKey);
            request.AddHeader("Content-Type", "application/json");
            var section = _db.Sections.Include(x => x.SectionDetails)
                                          .ThenInclude(x => x.Instructor)
                                      .Include(x => x.SectionDetails)
                                          .ThenInclude(x => x.Room)
                                      .Include(x => x.SectionSlots)
                                          .ThenInclude(x => x.Instructor)
                                      .Include(x => x.SectionSlots)
                                          .ThenInclude(x => x.Room)
                                      .Include(x => x.MidtermRoom)
                                      .Include(x => x.FinalRoom)
                                      .SingleOrDefault(x => x.Id == sectionId);
            var usSection = _mapper.Map<Section, KeystoneLibrary.Models.Api.SectionViewModel>(section);
            if (section.MidtermStart != null)
            {
                usSection.MidtermStart = new DateTime(section.MidtermStart.Value.Ticks).ToUniversalTime().TimeOfDay;
            }

            if (section.MidtermEnd != null)
            {
                usSection.MidtermEnd = new DateTime(section.MidtermEnd.Value.Ticks).ToUniversalTime().TimeOfDay;
            }

            if (section.FinalStart != null)
            {
                usSection.FinalStart = new DateTime(section.FinalStart.Value.Ticks).ToUniversalTime().TimeOfDay;
            }

            if (section.FinalEnd != null)
            {
                usSection.FinalEnd = new DateTime(section.FinalEnd.Value.Ticks).ToUniversalTime().TimeOfDay;
            }

            if (section.SectionDetails != null && section.SectionDetails.Any())
            {
                usSection.SectionDetails = section.SectionDetails.Select(x => _mapper.Map<SectionDetail, KeystoneLibrary.Models.Api.SectionDetailViewModel>(x))
                                                                 .ToList();
            }

            if (section.SectionSlots != null && section.SectionSlots.Any())
            {
                usSection.SectionSlots = section.SectionSlots.Select(x => _mapper.Map<SectionSlot, KeystoneLibrary.Models.Api.SectionSlotViewModel>(x))
                                                             .ToList();
            }

            var body = JsonConvert.SerializeObject(usSection);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);
            RestResponse response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception(response.ErrorMessage);
            }
        }

        public void CallUSparkAPICloseSection(long sectionId)
        {
            var client = new RestClient($"{ _USparkAPIURL }/sections/{ sectionId }/close");
            //client.Timeout = -1;
            //var request = new RestRequest(Method.PUT);
            var request = new RestRequest
            {
                Timeout = Timeout.InfiniteTimeSpan,
                Method = Method.Put
            };
            request.AddHeader("x-api-key", _USparkAPIKey);
            var body = @"";
            request.AddParameter("text/plain", body, ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);
            RestResponse response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception(response.ErrorMessage);
            }
        }

        public void CallUSparkAPIOpenSection(long sectionId)
        {
            var client = new RestClient($"{ _USparkAPIURL }/sections/{ sectionId }/open");
            //client.Timeout = -1;
            //var request = new RestRequest(Method.PUT);
            var request = new RestRequest
            {
                Timeout = Timeout.InfiniteTimeSpan,
                Method = Method.Put
            };
            request.AddHeader("x-api-key", _USparkAPIKey);
            var body = @"";
            request.AddParameter("text/plain", body, ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);
            RestResponse response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception(response.ErrorMessage);
            }
        }

        public int CallUSparkAPIGetCurrentSeat(long sectionId)
        {
            var client = new RestClient($"{ _USparkAPIURL }/sections/{ sectionId }/seat");
            //client.Timeout = -1;
            //var request = new RestRequest(Method.GET);
            var request = new RestRequest
            {
                Timeout = Timeout.InfiniteTimeSpan,
                Method = Method.Get
            };
            request.AddHeader("x-api-key", _USparkAPIKey);
            var body = @"";
            request.AddParameter("text/plain", body, ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);
            RestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            var apiContent = JsonConvert.DeserializeObject<ApiContent<int>>(response.Content);
            return apiContent.Data;
        }

        public void CallUSparkAPIUpdateSeat(long sectionId)
        {
            var section = _db.Sections.SingleOrDefault(x => x.Id == sectionId);
            var client = new RestClient($"{ _USparkAPIURL }/sections/{ section.Id }/seat");
            //client.Timeout = -1;
            //var request = new RestRequest(Method.PUT);
            var request = new RestRequest
            {
                Timeout = Timeout.InfiniteTimeSpan,
                Method = Method.Put
            };
            request.AddHeader("x-api-key", _USparkAPIKey);
            request.AddHeader("Content-Type", "application/json");
            var body = JsonConvert.SerializeObject(new
            {
                SeatLimit = section.SeatLimit
            });
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);
            RestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }

        public void CancelRegistration(Guid studentId, long termId)
        {
            var invoices = _db.Invoices.Include(x => x.InvoiceItems)
                                       .Where(x => !x.IsCancel
                                                   && !x.IsPaid
                                                   && x.StudentId == studentId
                                                   && x.TermId == termId
                                                   && x.Type == "r"
                                                   && x.InvoiceItems.Any(y => y.RegistrationCourseId != null))
                                       .ToList();
            foreach (var invoice in invoices)
            {
                invoice.IsCancel = true;
                invoice.IsActive = false;
                foreach (var item in invoice.InvoiceItems)
                {
                    item.Type = "d";
                    var course = _db.RegistrationCourses.SingleOrDefault(x => x.Id == item.RegistrationCourseId);
                    if (course != null)
                    {
                        course.Status = "d";
                    }
                }
            }

            _db.SaveChanges();
        }

        public async Task<USparkCalculateTuitionRequestViewModel> CallUSparkAPIGetPreregistrations(string studentCode, int academicYear, int term)
        {
            var endpoint = $"{ _USparkAPIURL }/preregistration?StudentCode={studentCode}&AcademicYear={academicYear}&Term={term}";

            var header = new Dictionary<string, string>
            {
                { "x-api-key", _USparkAPIKey }
            };
            
            var response = await _httpClientProxy.GetAsync(endpoint, header);

            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine(content);

            var apiContent = JsonConvert.DeserializeObject<ApiContent<USparkCalculateTuitionRequestViewModel>>(content);

            return apiContent.Data;
        }

        public List<StudentListViewModel> CallUSparkAPIGetStudentsBySectionId(long sectionId)
        {
            var client = new RestClient($"{ _USparkAPIURL }/sections/{ sectionId }/students");
            //client.Timeout = -1;
            //var request = new RestRequest(Method.GET);
            var request = new RestRequest
            {
                Timeout = Timeout.InfiniteTimeSpan,
                Method = Method.Get
            };
            request.AddHeader("x-api-key", _USparkAPIKey);
            //IRestResponse response = client.Execute(request);
            RestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            var apiContent = JsonConvert.DeserializeObject<ApiContent<List<StudentListViewModel>>>(response.Content);
            return apiContent.Data;
        }

        public void CallUSparkAPIUpdatePreregistrations(Guid studentId, string userId, long termId, IEnumerable<long> sectionIds)
        {
            var student = _db.Students.AsNoTracking().SingleOrDefault(x => x.Id == studentId);
            var modifiedSectionIds = sectionIds.Where(x => x > 0).ToList();
            // TODO : 
            if(student == null)
            {

            }

            var preregistrations = new USparkUpdateRegistrationViewModel
            {
                StudentCode = student.Code,
                Channel = "R",
                KSSectionIds = modifiedSectionIds,
                CreatedBy = userId,
                KSTermId = termId
            };

            var client = new RestClient($"{ _USparkAPIURL }/preregistration/student");
            //client.Timeout = -1;

            //var request = new RestRequest(Method.PUT);
            var request = new RestRequest
            {
                Timeout = Timeout.InfiniteTimeSpan,
                Method = Method.Put
            };
            request.AddHeader("x-api-key", _USparkAPIKey);
            request.AddHeader("Content-Type", "application/json");

            var body = JsonConvert.SerializeObject(preregistrations);

            request.AddParameter("application/json", body, ParameterType.RequestBody);

            //IRestResponse response = client.Execute(request);
            RestResponse response = client.Execute(request);

            Console.WriteLine(response.Content);

            if (!response.IsSuccessful)
            {
                ApiContent<int?> apiContent = null;
                try
                {
                    apiContent = JsonConvert.DeserializeObject<ApiContent<int?>>(response.Content);
                }
                catch (Exception)
                {
                }
                if (apiContent != null)
                {
                    throw new Exception(apiContent.Message);
                }
                throw new Exception(response.StatusDescription);
            }
        }

        public void CallUSparkAPICheckoutOrder(long receiptId)
        {
            var receipt = _db.Receipts.Include(x => x.Invoice)
                                      .Include(x => x.Student)
                                      .Include(x => x.ReceiptItems)
                                          .ThenInclude(x => x.FeeItem)
                                      .SingleOrDefault(x => x.Id == receiptId);

            var invoice = new KeystoneLibrary.Models.Api.KSPaymentOrder
            {
                Reference1 = receipt.Invoice.Reference1,
                Reference2 = receipt.Invoice.Reference2,
                KSTermID = receipt.TermId ?? 0,
                StudentCode = receipt.Student.Code,
                TotalAmount = receipt.TotalAmount,
                Number = receipt.Number,
                PaymentStartedAt = DateTime.UtcNow,
                PaymentEndedAt = DateTime.UtcNow,
                ReferenceNumber = string.Empty,
                KSInvoiceId = receipt.InvoiceId.ToString(),
                KSPaymentOrderDetails = receipt.ReceiptItems.Select(y => new KSPaymentOrderDetail
                {
                    Amount = y.TotalAmount,
                    ItemCode = y.FeeItem.Code,
                    ItemNameEn = y.FeeItem.NameEn,
                    ItemNameTh = y.FeeItem.NameTh
                })
                                                                          .ToList()
            };

            var client = new RestClient($"{ _USparkAPIURL }/payment/checkout_ks");
            //client.Timeout = -1;
            //var request = new RestRequest(Method.POST);
            var request = new RestRequest
            {
                Timeout = Timeout.InfiniteTimeSpan,
                Method = Method.Post
            };
            request.AddHeader("x-api-key", _USparkAPIKey);
            request.AddHeader("Content-Type", "application/json");
            var body = JsonConvert.SerializeObject(invoice);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);
            RestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }

        public void CallUSparkServiceAPIPaymentConfirm(long receiptId)
        {
            CheckAndGetUSparkService(out string baseUrl, out string apiKey);

            var receipt = _db.Receipts.Include(x => x.Invoice)
                                          .ThenInclude(x => x.InvoiceItems)
                                      .Include(x => x.Student)
                                      .Include(x => x.ReceiptItems)
                                          .ThenInclude(x => x.FeeItem)
                                      .SingleOrDefault(x => x.Id == receiptId);
            if (receipt == null)
            {
                throw new Exception("Cannot find receipt");
            }
            var creditNote = (from creditNoteTran in _db.InvoiceDeductTransactions
                              join invoice in _db.Invoices.Include(x => x.InvoiceItems) on creditNoteTran.InvoiceCreditNoteId equals invoice.Id
                              where creditNoteTran.InvoiceId == receipt.InvoiceId
                              select invoice
                             ).Distinct().FirstOrDefault();

            var order = GenerateUSparkOrderFromInvoice(receipt.Student.Code, receipt.TermId.Value, receipt.Invoice, creditNote);

            var url = baseUrl + "/payments/confirm";
            var client = new RestClient(url);
            //client.Timeout = -1;
            //var request = new RestRequest(Method.POST);
            var request = new RestRequest
            {
                Timeout = Timeout.InfiniteTimeSpan,
                Method = Method.Post
            };
            request.AddHeader("x-api-key", apiKey);
            request.AddHeader("Content-Type", "application/json");
            var body = JsonConvert.SerializeObject(order);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);
            RestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            if (response.IsSuccessful)
            {
                return;
            }
            else
            {
                throw new Exception($"Error {response.StatusCode} {response.StatusDescription}");
            }
        }

        public void CallUSparkServiceAPIWaiveInvoice(long invoiceId)
        {
            CheckAndGetUSparkService(out string baseUrl, out string apiKey);

            var invoice = _db.Invoices.Include(x => x.InvoiceItems)
                                          .ThenInclude(x => x.FeeItem)
                                      .Include(x => x.Student)
                                      .SingleOrDefault(x => x.Id == invoiceId);
            if (invoice == null)
            {
                throw new Exception("Cannot find invoice");
            }
            var creditNote = invoice.Type == "cr" ?
                              (from creditNoteTran in _db.InvoiceDeductTransactions
                               join anotherInv in _db.Invoices.Include(x => x.InvoiceItems) on creditNoteTran.InvoiceId equals anotherInv.Id
                               where creditNoteTran.InvoiceCreditNoteId == invoice.Id && creditNoteTran.InvoiceCreditNoteId != creditNoteTran.InvoiceId
                               select anotherInv
                             ).Distinct().FirstOrDefault()
                :
                             (from creditNoteTran in _db.InvoiceDeductTransactions
                              join anotherInv in _db.Invoices.Include(x => x.InvoiceItems) on creditNoteTran.InvoiceCreditNoteId equals anotherInv.Id
                              where creditNoteTran.InvoiceId == invoice.Id && creditNoteTran.InvoiceCreditNoteId != creditNoteTran.InvoiceId
                              select anotherInv
                             ).Distinct().FirstOrDefault();

            var order = GenerateUSparkOrderFromInvoice(invoice.Student.Code, invoice.TermId.Value, invoice, creditNote);

            var url = baseUrl + "/payments/update";
            var client = new RestClient(url);
            //client.Timeout = -1;
            //var request = new RestRequest(Method.POST);
            var request = new RestRequest
            {
                Timeout = Timeout.InfiniteTimeSpan,
                Method = Method.Post
            };
            request.AddHeader("x-api-key", apiKey);
            request.AddHeader("Content-Type", "application/json");
            var body = JsonConvert.SerializeObject(order);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);
            RestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            if (response.IsSuccessful)
            {
                return;
            }
            else
            {
                throw new Exception($"Error {response.StatusCode} {response.StatusDescription}");
            }
        }

        [Obsolete]
        private void CheckAndGetUSparkService(out string baseUrl, out string apiKey)
        {
            baseUrl = _config["USparkServiceUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new Exception("Not Config Service URL");
            }
            apiKey = _config["USparkServiceAPIKey"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new Exception("Not Config Service Key");
            }
        }

        public async Task<bool> UpdateCreditUspark(string studentCode, int maximumCredit, int minimumCredit)
        {
            var model = new
            {
                StudentCode = studentCode,
                CreditMax = maximumCredit,
                CreditMin = minimumCredit,
            };

            var bodyJson = JsonConvert.SerializeObject(model);

            // var url = _config["USparkUrl"] + "/students/update_registration_credit";
            // var apiKey = _config["USparkAPIKey"];
            // using(var client = new HttpClient())
            // {
            try
            {
                // var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
                // var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                // requestMessage.Headers.Add("x-api-key", apiKey);
                // requestMessage.Content = content;
                // var response = await client.SendAsync(requestMessage);
                // var rawResponse = await response.Content.ReadAsStringAsync();
                var client = new RestClient($"{ _USparkAPIURL }/students/update_registration_credit");
                //client.Timeout = -1;
                //var request = new RestRequest(Method.POST);
                var request = new RestRequest
                {
                    Timeout = Timeout.InfiniteTimeSpan,
                    Method = Method.Post
                };
                request.AddHeader("x-api-key", _USparkAPIKey);
                request.AddHeader("Content-Type", "application/json");
                var body = bodyJson;
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                //IRestResponse response = client.Execute(request);
                RestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return true;
            }
            // }
        }

        public async Task<bool> UpdateStudentCourseByPass(BodyUpdateStudentCourseByPass model)
        {
            var url = _config["USparkUrl"] + "/students/bypass_prerequisite";
            var bodyJson = JsonConvert.SerializeObject(model);
            // var apiKey = _config["USparkAPIKey"];

            // using(var client = new HttpClient())
            // {
            try
            {
                // var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

                // var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                // requestMessage.Headers.Add("x-api-key", apiKey);
                // requestMessage.Content = content;

                // var response = await client.SendAsync(requestMessage);
                // var rawResponse = await response.Content.ReadAsStringAsync();
                var client = new RestClient($"{ _USparkAPIURL }/students/bypass_prerequisite");
                //client.Timeout = -1;
                //var request = new RestRequest(Method.POST);
                var request = new RestRequest
                {
                    Timeout = Timeout.InfiniteTimeSpan,
                    Method = Method.Post
                };
                request.AddHeader("x-api-key", _USparkAPIKey);
                request.AddHeader("Content-Type", "application/json");
                var body = bodyJson;
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                //IRestResponse response = client.Execute(request);
                RestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return true;
            }
            // }
        }

        public async Task<bool> UpdateLockedStudentUspark(string studentCode, bool lockedRegistration, bool lockedPayment, bool lockedSignIn)
        {
            var model = new USparkUpdateRegistrationStatusStudentViewModel
            {
                IsRegistrationAllowed = !lockedRegistration,
                IsPaymentAllowed = !lockedPayment,
                IsSignInAllowed = !lockedSignIn,
            };
            var bodyJson = JsonConvert.SerializeObject(model);
            // var url = _config["USparkUrl"] + "/students/update_registration_credit";
            // var apiKey = _config["USparkAPIKey"];
            // using(var client = new HttpClient())
            // {
            try
            {
                // var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
                // var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                // requestMessage.Headers.Add("x-api-key", apiKey);
                // requestMessage.Content = content;
                // var response = await client.SendAsync(requestMessage);
                // var rawResponse = await response.Content.ReadAsStringAsync();
                var client = new RestClient($"{ _USparkAPIURL }/students/{ studentCode }/registration_status");
                //client.Timeout = -1;
                //var request = new RestRequest(Method.POST);
                var request = new RestRequest
                {
                    Timeout = Timeout.InfiniteTimeSpan,
                    Method = Method.Post
                };
                request.AddHeader("x-api-key", _USparkAPIKey);
                request.AddHeader("Content-Type", "application/json");
                var body = bodyJson;
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                //IRestResponse response = client.Execute(request);
                RestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return false;
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    _flashMessage.Danger("USpark: Something went wrong. Please try again later.");
                    return true;
                }
                else
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                    return true;
                }
            }
            catch
            {
                return true;
            }
            // }
        }

        public void AddRegistrationCourses(List<RegistrationCourse> registrationCourses)
        {
            _db.RegistrationCourses.AddRange(registrationCourses.Where(x => x.Id == 0));
            _db.SaveChanges();
        }

        public string GetStudentState(Guid studentId, long termId)
        {
            var studentState = _db.StudentStates.FirstOrDefault(x => x.StudentId == studentId
                                                                     && x.TermId == termId);
            if (studentState != null)
            {
                if (studentState.State == "REG")
                {
                    return "r";
                }
                else 
                {
                    return "a";
                }
            }
            else
            {
                return "r";
            }
        }

        public void DeleteAllNotPaidCourse(Guid studentId, long termId)
        {
            var courses = new List<AddingViewModel>();
            // List<RegistrationModificationViewModel> registrationLogs = new List<RegistrationModificationViewModel>();

            var registrationCourses = _db.RegistrationCourses.Include(x => x.Section)
                                                             .Include(x => x.Course)
                                                             .Where(x => studentId == x.StudentId
                                                                         && x.TermId == termId
                                                                         && x.Status != "d")
                                                             .ToList();

            var retainedCourses = registrationCourses.Where(x => x.IsPaid).ToList();
            if (retainedCourses != null && retainedCourses.Any())
            {
                foreach (var retainedCourse in retainedCourses)
                {
                    courses.Add(new AddingViewModel()
                    {
                        CourseCode = retainedCourse.Course.CodeAndCredit,
                        SectionNumber = retainedCourse.Section.Number,
                        Status = "r"
                    });
                }
            }

            var deletedCourses = registrationCourses.Where(x => !x.IsPaid).ToList();
            if (deletedCourses != null && deletedCourses.Any())
            {
                foreach (var deletedCourse in deletedCourses)
                {
                    deletedCourse.Status = "d";
                    ReturnSeat(deletedCourse.Section, 1);

                    var course = _db.Courses.Find(deletedCourse.CourseId);
                    var section = _db.Sections.SingleOrDefault(x => x.Id == deletedCourse.SectionId);
                    courses.Add(new AddingViewModel()
                    {
                        CourseCode = course.CodeAndCredit,
                        SectionNumber = section.Number,
                        Status = "d"
                    });
                }
            }

            // if (courses.Any(x => x.Status == "r"))
            // {
            //     RegistrationModificationViewModel retainedCourseText = new RegistrationModificationViewModel();
            //     retainedCourseText.Type = "Retained";
            //     retainedCourseText.Courses = String.Join(", ", courses.Where(x => x.Status == "r").Select(x => $"{ x.CourseCode }({ x.SectionNumber })"));
            //     registrationLogs.Add(retainedCourseText);
            // }

            // if (courses.Any(x => x.Status == "d"))
            // {
            //     RegistrationModificationViewModel discardCourseText = new RegistrationModificationViewModel();
            //     discardCourseText.Type = "Discard";
            //     discardCourseText.Courses = String.Join(", ", courses.Where(x => x.Status == "d").Select(x => $"{ x.CourseCode }({ x.SectionNumber })"));
            //     registrationLogs.Add(discardCourseText);
            // }

            // var registeredCourseLogs = courses.Where(x => x.Status != "d").Select(x => $"{ x.CourseCode }({ x.SectionNumber })").ToList();
            // _db.RegistrationLogs.Add(new RegistrationLog
            // {
            //     Channel = "r",
            //     StudentId = studentId,
            //     TermId = termId,
            //     Modifications = JsonConvert.SerializeObject(registrationLogs),
            //     RegistrationCourses = JsonConvert.SerializeObject(registeredCourseLogs),
            //     Type = "d",
            //     Round = 0
            // });

            _db.SaveChanges();
        }

        public bool UpdateStudentState(Guid studentId, string studentCode, long termId, string state, string updateFrom, out string errorMsg)
        {
            //updateFrom:  KS = from Keystone, US = USpark called API USparkStudentStateController
            errorMsg = string.Empty;
            var studentState = _db.StudentStates.FirstOrDefault(x => x.StudentId == studentId
                                                                     && x.TermId == termId);
            if (studentState == null)
            {
                studentState = new StudentState
                {
                    StudentId = studentId,
                    TermId = termId,
                    State = state
                };

                _db.StudentStates.Add(studentState);
            }
            else
            {
                if (studentState.State == state)
                {
                    return true;
                }

                studentState.State = state;
            }

            _db.StudentStateLogs.Add(new StudentStateLog
            {
                StudentId = studentId,
                TermId = termId,
                State = state,
                UpdateFrom = updateFrom
            });

            _db.SaveChanges();

            // call api
            var client = new RestClient($"{ _USparkAPIURL }/students/update_state");
            //client.Timeout = -1;
            //var request = new RestRequest(Method.POST);
            var request = new RestRequest
            {
                Timeout = Timeout.InfiniteTimeSpan,
                Method = Method.Post
            };
            request.AddHeader("x-api-key", _USparkAPIKey);
            request.AddHeader("Content-Type", "application/json");
            var body = new
            {
                ksTermId = termId,
                studentCode = studentCode,
                state = state
            };
            request.AddParameter("application/json", JsonConvert.SerializeObject(body), ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);
            RestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                errorMsg = response.ErrorMessage;
                return false;
            }
        }

        public async Task GetRegistrationCourseFromUspark(Guid studentId, long termId, string userId)
        {
            // call api get registration 
            var student = _db.Students.IgnoreQueryFilters()
                                      .AsNoTracking()
                                      .SingleOrDefault(x => x.Id == studentId);

            var term = _db.Terms.AsNoTracking()
                                .SingleOrDefault(x => x.Id == termId);

            // TODO : NULL CHECK

            if(student == null || term == null)
            {

            }

            var preregistrations = await CallUSparkAPIGetPreregistrations(student.Code, term.AcademicYear, term.AcademicTerm);

            UpsertRegistrationCourses(preregistrations.Preregistrations, preregistrations.StudentCode, preregistrations.KSTermId, userId);
            UpsertRegistrationLog(preregistrations.RegistrationLogs, preregistrations.StudentCode, preregistrations.KSTermId);
        }

        public List<RegistrationCourse> ModifyRegistrationCourse(
            Guid studentId, long termId, string round,
            List<AddingViewModel> addingResults,
            out List<RegistrationCourse> newCourses,
            out List<RegistrationCourse> deleteUnpaidCourses,
            out List<RegistrationCourse> deletePaidCourses,
            string channel)
        {
            newCourses = new List<RegistrationCourse>();
            deleteUnpaidCourses = new List<RegistrationCourse>();
            deletePaidCourses = new List<RegistrationCourse>();

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var courses = new List<AddingViewModel>();
                    var modifyRegistrationCourses = new List<RegistrationCourse>();
                    var registrationCourses = _db.RegistrationCourses.Where(x => x.StudentId == studentId
                                                                                 && x.TermId == termId
                                                                                 && !x.IsTransferCourse
                                                                                 && x.Status != "d")
                                                                     .ToList();;

                    if (addingResults.Any(x => x.CourseId > 0))
                    {
                        var modifyCourses = addingResults.Where(x => x.CourseId > 0).ToList();
                        foreach (var modifyCourse in modifyCourses)
                        {
                            var course = _db.Courses.Find(modifyCourse.CourseId);
                            // Add new Course
                            if (!registrationCourses.Any(x => x.CourseId == modifyCourse.CourseId))
                            {
                                var newCourse = new RegistrationCourse()
                                {
                                    StudentId = studentId,
                                    TermId = termId,
                                    CourseId = modifyCourse.CourseId,
                                    SectionId = modifyCourse.SectionId,
                                    GradeName = "x",
                                    IsPaid = false,
                                    IsLock = false,
                                    IsSurveyed = false,
                                    Status = round,
                                    IsActive = true,
                                    Channel = "r",
                                    Refunds = modifyCourse.RefundItems
                                };
                                _db.RegistrationCourses.Add(newCourse);
                                modifyRegistrationCourses.Add(newCourse);
                                newCourses.Add(newCourse);

                                var section = _db.Sections.SingleOrDefault(x => x.Id == modifyCourse.SectionId);
                                if (section != null)
                                {
                                    // AddSeat(section, 1);
                                    courses.Add(new AddingViewModel()
                                    {
                                        CourseCode = course.CodeAndCredit,
                                        SectionNumber = section.Number,
                                        Status = "c"
                                    });
                                }
                            }
                            else if (registrationCourses.Any(x => x.CourseId == modifyCourse.CourseId
                                                                  && x.SectionId == modifyCourse.SectionId))
                            {
                                var existingCourse = registrationCourses.FirstOrDefault(x => x.CourseId == modifyCourse.CourseId
                                                                                             && x.SectionId == modifyCourse.SectionId);
                                modifyRegistrationCourses.Add(existingCourse);
                                if (!existingCourse.IsPaid)
                                {
                                    newCourses.Add(existingCourse);
                                }
                            }
                            // Modify Course - Change Section
                            else if (!registrationCourses.Any(x => x.CourseId == modifyCourse.CourseId
                                                                   && x.SectionId == modifyCourse.SectionId))
                            {
                                var changingCourse = registrationCourses.FirstOrDefault(x => x.CourseId == modifyCourse.CourseId);

                                if (changingCourse != null)
                                {
                                    // If paid - need to add new section and delete old section for credit note
                                    if (changingCourse.IsPaid)
                                    {
                                        // New section
                                        var newCourse = new RegistrationCourse()
                                        {
                                            StudentId = studentId,
                                            TermId = termId,
                                            CourseId = modifyCourse.CourseId,
                                            SectionId = modifyCourse.SectionId,
                                            GradeName = "x",
                                            IsPaid = false,
                                            IsLock = false,
                                            IsSurveyed = false,
                                            Status = round,
                                            IsActive = true,
                                            Channel = "r",
                                            Refunds = modifyCourse.RefundItems
                                        };
                                        _db.RegistrationCourses.Add(newCourse);
                                        modifyRegistrationCourses.Add(newCourse);
                                        newCourses.Add(newCourse);

                                        var newSection = _db.Sections.SingleOrDefault(x => x.Id == modifyCourse.SectionId);
                                        if (newSection != null)
                                        {
                                            // AddSeat(section, 1);
                                            courses.Add(new AddingViewModel()
                                            {
                                                CourseCode = course.CodeAndCredit,
                                                SectionNumber = newSection.Number,
                                                Status = "c"
                                            });
                                        }
                                    } 
                                    else 
                                    {
                                        string sectonNumber = "";
                                        // var oldSection = _db.Sections.SingleOrDefault(x => x.Id == changingCourse.SectionId);
                                        // sectonNumber = oldSection.Number;
                                        if (changingCourse.SectionId != modifyCourse.SectionId)
                                        {
                                            var newSection = _db.Sections.SingleOrDefault(x => x.Id == modifyCourse.SectionId);
                                            sectonNumber = newSection.Number;
                                            // ReturnSeat(oldSection, 1);
                                            // AddSeat(newSection, 1);

                                            changingCourse.SectionId = modifyCourse.SectionId;
                                            courses.Add(new AddingViewModel()
                                            {
                                                CourseCode = course.CodeAndCredit,
                                                SectionNumber = sectonNumber,
                                                Status = "c"
                                            });
                                        } 
                                        else 
                                        {
                                            courses.Add(new AddingViewModel()
                                            {
                                                CourseCode = course.CodeAndCredit,
                                                SectionNumber = sectonNumber,
                                                Status = "u"
                                            });
                                        }
                                        modifyRegistrationCourses.Add(changingCourse);
                                        newCourses.Add(changingCourse);
                                    }
                                }
                            }
                        }

                        // Delete Course
                        var deletedCourses = registrationCourses.Where(x => !addingResults.Any(y => y.SectionId == x.SectionId)).ToList();
                        foreach (var deletedCourse in deletedCourses)
                        {
                            deletedCourse.Status = "d";
                            // ReturnSeat(deletedCourse.Section, 1);

                            var course = _db.Courses.Find(deletedCourse.CourseId);
                            var section = _db.Sections.SingleOrDefault(x => x.Id == deletedCourse.SectionId);
                            courses.Add(new AddingViewModel()
                            {
                                CourseCode = course.CodeAndCredit,
                                SectionNumber = section.Number,
                                Status = "d"
                            });   

                            if (deletedCourse.IsPaid)
                            {
                                deletePaidCourses.Add(deletedCourse);
                            } else 
                            {
                                deleteUnpaidCourses.Add(deletedCourse);
                            }
                        }
                       
                        _db.SaveChanges();
                    }
                    else
                    {
                        // Delete Course
                        foreach (var deletedCourse in registrationCourses)
                        {
                            deletedCourse.Status = "d";
                            // ReturnSeat(deletedCourse.Section, 1);

                            var course = _db.Courses.Find(deletedCourse.CourseId);
                            var section = _db.Sections.SingleOrDefault(x => x.Id == deletedCourse.SectionId);
                            courses.Add(new AddingViewModel()
                            {
                                CourseCode = course.CodeAndCredit,
                                SectionNumber = section.Number,
                                Status = "d"
                            });

                            if (deletedCourse.IsPaid)
                            {
                                deletePaidCourses.Add(deletedCourse);
                            }
                            else 
                            {
                                deleteUnpaidCourses.Add(deletedCourse);
                            }
                        }
                        _db.SaveChanges();
                    }

                    // if(IsChangeCourse)
                    // {
                    //     List<RegistrationModificationViewModel> registrationLogs = new List<RegistrationModificationViewModel>();
                    //     if (courses.Any(x => x.Status == "c"))
                    //     {
                    //         RegistrationModificationViewModel newCourseText = new RegistrationModificationViewModel();
                    //         newCourseText.Type = "New";
                    //         newCourseText.Courses = String.Join(", ", courses.Where(x => x.Status == "c").Select(x => $"{ x.CourseCode }({ x.SectionNumber })"));
                    //         registrationLogs.Add(newCourseText);
                    //     }

                    //     if (courses.Any(x => x.Status == "u"))
                    //     {
                    //         RegistrationModificationViewModel retainedCourseText = new RegistrationModificationViewModel();
                    //         retainedCourseText.Type = "Retained";
                    //         retainedCourseText.Courses = String.Join(", ", courses.Where(x => x.Status == "u").Select(x => $"{ x.CourseCode }({ x.SectionNumber })"));
                    //         registrationLogs.Add(retainedCourseText);
                    //     }

                    //     if (courses.Any(x => x.Status == "d"))
                    //     {
                    //         RegistrationModificationViewModel discardCourseText = new RegistrationModificationViewModel();
                    //         discardCourseText.Type = "Discard";
                    //         discardCourseText.Courses = String.Join(", ", courses.Where(x => x.Status == "d").Select(x => $"{ x.CourseCode }({ x.SectionNumber })"));
                    //         registrationLogs.Add(discardCourseText);
                    //     }

                    //     var registeredCourseLogs = courses.Where(x => x.Status != "d").Select(x => $"{ x.CourseCode }({ x.SectionNumber })").ToList();
                    //     _db.RegistrationLogs.Add(new RegistrationLog
                    //     {
                    //         Channel = channel,
                    //         StudentId = studentId,
                    //         TermId = termId,
                    //         Modifications = JsonConvert.SerializeObject(registrationLogs),
                    //         RegistrationCourses = JsonConvert.SerializeObject(registeredCourseLogs),
                    //         Type = round,
                    //         Round = 0
                    //     });
                    //     _db.SaveChanges();
                    // }
                    transaction.Commit();

                    return modifyRegistrationCourses;
                }
                catch
                {
                    transaction.Rollback();
                    return null;
                }
            }
        }

        public void SimulateModifyRegistrationCourse(Guid studentId, long termId, List<AddingViewModel> addingResults, out List<RegistrationCourse> newCourses, out List<RegistrationCourse> deletePaidCourses)
        {
            newCourses = new List<RegistrationCourse>();
            deletePaidCourses = new List<RegistrationCourse>();

            var registrationCourses = _db.RegistrationCourses.Where(x => x.StudentId == studentId
                                                                                 && x.TermId == termId
                                                                                 && !x.IsTransferCourse
                                                                                 && x.Status != "d")
                                                             .ToList();

            if (addingResults.Any(x => x.CourseId > 0))
            {
                var modifyCourses = addingResults.Where(x => x.CourseId > 0).ToList();
                foreach (var modifyCourse in modifyCourses)
                {
                    var course = _db.Courses.Find(modifyCourse.CourseId);
                    // Add new Course
                    if (!registrationCourses.Any(x => x.CourseId == modifyCourse.CourseId))
                    {
                        var newCourse = new RegistrationCourse()
                                        {
                                            StudentId = studentId,
                                            TermId = termId,
                                            CourseId = modifyCourse.CourseId,
                                            SectionId = modifyCourse.SectionId,
                                        };
                        newCourses.Add(newCourse);
                    }
                    else if (registrationCourses.Any(x => x.CourseId == modifyCourse.CourseId
                                                            && x.SectionId == modifyCourse.SectionId))
                    {
                        var existingCourse = registrationCourses.FirstOrDefault(x => x.CourseId == modifyCourse.CourseId
                                                                                        && x.SectionId == modifyCourse.SectionId);
                        if (!existingCourse.IsPaid)
                        {
                            var newCourse = new RegistrationCourse()
                                            {
                                                StudentId = studentId,
                                                TermId = termId,
                                                CourseId = existingCourse.CourseId,
                                                SectionId = existingCourse.SectionId,
                                            };
                            newCourses.Add(newCourse);
                        }
                    }
                    // Modify Course - Change Section
                    else if (!registrationCourses.Any(x => x.CourseId == modifyCourse.CourseId
                                                            && x.SectionId == modifyCourse.SectionId))
                    {
                        var changingCourse = registrationCourses.FirstOrDefault(x => x.CourseId == modifyCourse.CourseId);

                        if (changingCourse != null)
                        {
                            var newCourse = new RegistrationCourse()
                                            {
                                                StudentId = studentId,
                                                TermId = termId,
                                                CourseId = changingCourse.CourseId,
                                                SectionId = changingCourse.SectionId,
                                            };
                            newCourses.Add(newCourse);
                        }
                    }
                }

                // Delete Course
                var deletedCourses = registrationCourses.Where(x => !addingResults.Any(y => y.SectionId == x.SectionId)).ToList();
                foreach (var deletedCourse in deletedCourses)
                {
                    if (deletedCourse.IsPaid)
                    {
                        var deleteExistingCourse = new RegistrationCourse()
                                                    {
                                                        Id = deletedCourse.Id,
                                                        StudentId = studentId,
                                                        TermId = termId,
                                                        CourseId = deletedCourse.CourseId,
                                                        SectionId = deletedCourse.SectionId,
                                                    };
                        deletePaidCourses.Add(deleteExistingCourse);
                    } 
                }
        }
            else
            {
                // Delete Course
                foreach (var registrationCourse in registrationCourses)
                {
                    if (registrationCourse.IsPaid)
                    {
                        var deleteCourse = new RegistrationCourse()
                                            {
                                                Id = registrationCourse.Id,
                                                StudentId = studentId,
                                                TermId = termId,
                                                CourseId = registrationCourse.CourseId,
                                                SectionId = registrationCourse.SectionId,
                                            };
                        deletePaidCourses.Add(deleteCourse);
                    }
                }
            }
        }

        public bool UpdateWriteList(UpdateWhiteListViewModel model, long sectionId)
        {
            var body = JsonConvert.SerializeObject(model);
            try
            {
                var client = new RestClient($"{ _USparkAPIURL }/sections/{ sectionId }");
                //client.Timeout = -1;
                //var request = new RestRequest(Method.PUT);
                var request = new RestRequest
                {
                    Timeout = Timeout.InfiniteTimeSpan,
                    Method = Method.Put
                };
                request.AddHeader("x-api-key", _USparkAPIKey);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                //IRestResponse response = client.Execute(request);
                RestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return false;
                }
                else if(response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _flashMessage.Danger("Unable to edit, Update to Uspark fail : 401 Unauthorized");
                    return true;
                }
                else if(response.StatusCode == HttpStatusCode.BadRequest)
                {
                    _flashMessage.Danger("Unable to edit, Update to Uspark fail : 400 BadRequest");
                    return true;
                }
                else
                {
                    _flashMessage.Danger("Unable to edit, Update to Uspark fail : " + response.StatusCode);
                    return true;
                }
            }
            catch
            {
                return true;
            }
        }

        public USparkOrder SimulateGenerateUSparkOrderFromInvoice(string studentCode, long termId, Invoice invoice, Invoice dropInvoice)
        {
            var order = new USparkOrder();
            order.KSTermID = termId;
            order.StudentCode = studentCode;
            order.KSInvoiceID = invoice.Id;
            order.Number = invoice != null ? invoice.Number : null;
            order.Reference1 = invoice != null ? invoice.Reference1 : null;
            order.Reference2 = invoice != null ? invoice.Reference2 : null;
            order.OrderDetails = new List<USparkOrderDetail>();

            var invoiceItems = new List<InvoiceItem>();
            if(invoice.InvoiceItems != null && invoice.InvoiceItems.Any())
            {
                invoiceItems.AddRange(invoice.InvoiceItems);
            }

            if (dropInvoice != null && dropInvoice.InvoiceItems != null && dropInvoice.InvoiceItems.Any())
            {
                invoiceItems.AddRange(dropInvoice.InvoiceItems);
            }
            
            foreach (var invoiceItem in invoiceItems)
            {
                var feeItem = _db.FeeItems.SingleOrDefault(x => x.Id == invoiceItem.FeeItemId);
                order.OrderDetails.Add(new USparkOrderDetail()
                {
                    Amount = invoiceItem.FeeItemId == 1 ? invoiceItem.TotalAmount + invoiceItem.DiscountAmount : invoiceItem.TotalAmount,
                    ItemCode = feeItem.Code,
                    ItemNameEn = feeItem.NameEn,
                    ItemNameTh = feeItem.NameTh,
                    KSRegistrationCourseId = invoiceItem.RegistrationCourseId,
                    KSCourseId = invoiceItem.CourseId,
                    KSSectionId = invoiceItem.SectionId,
                    LumpSumAddDrop = invoiceItem.LumpSumAddDrop
                });
            }
            order.TotalAmount = invoiceItems.Sum(x => x.FeeItemId == 1 ? x.TotalAmount + x.DiscountAmount : x.TotalAmount);
            return order;
        }

        public USparkOrder GenerateUSparkOrderFromInvoice(string studentCode, long termId, Invoice invoice, Invoice dropInvoice)
        {
            var order = new USparkOrder();
            //order.OrderId = orderId;
            order.Reference1 = invoice.Reference1;
            order.Reference2 = invoice.Reference2;
            order.KSTermID = termId;
            order.StudentCode = studentCode;
            order.Number = invoice.Number;
            order.ReferenceNumber = invoice.RunningNumber;
            order.OrderDetails = new List<USparkOrderDetail>();
            order.CreatedAt = invoice.CreatedAt;
            order.InvoiceExpiryDate = invoice.PaymentExpireAt ?? DateTime.Today.AddDays(15);
            order.IsPaid = invoice.IsPaid;
            order.KSInvoiceID = invoice.Id;

            var invoiceItems = new List<InvoiceItem>();
            if(invoice.InvoiceItems != null && invoice.InvoiceItems.Any())
            {
                invoiceItems.AddRange(invoice.InvoiceItems);
            }

            if(invoice.Id == 0 || invoice.TotalAmount == 0)
            {
                var student = _db.Students.FirstOrDefault(x => x.Code == studentCode);
                if(_db.Invoices.Any(x => x.StudentId == student.Id
                                         && !x.IsCancel
                                         && x.Type == "cr"))
                {
                    var creditNote = _db.Invoices.Include(x => x.InvoiceItems)
                                                 .Where(x => x.StudentId == student.Id
                                                             && !x.IsCancel
                                                             && x.Type == "cr")
                                                 .OrderByDescending(x => x.Id)
                                                 .FirstOrDefault();
                    order.Reference1 = creditNote.Reference1;
                    order.Reference2 = creditNote.Reference2;
                    order.KSTermID = termId;
                    order.StudentCode = studentCode;
                    order.Number = creditNote.Number;
                    order.ReferenceNumber = creditNote.RunningNumber;
                    order.OrderDetails = new List<USparkOrderDetail>();
                    order.CreatedAt = creditNote.CreatedAt;
                    order.InvoiceExpiryDate = creditNote.PaymentExpireAt ?? DateTime.Today.AddDays(15);
                    order.IsPaid = creditNote.IsPaid;
                    order.KSInvoiceID = creditNote.Id;
                    dropInvoice.InvoiceItems = creditNote.InvoiceItems;
                }
            }

            if (dropInvoice != null && dropInvoice.InvoiceItems != null && dropInvoice.InvoiceItems.Any())
            {
                invoiceItems.AddRange(dropInvoice.InvoiceItems);
            }
            
            var feeItems = _db.FeeItems.Where(x => invoiceItems.Select(y => y.FeeItemId).Contains(x.Id)).ToList();
            foreach (var invoiceItem in invoiceItems)
            {
                var feeItem = feeItems.SingleOrDefault(x => x.Id == invoiceItem.FeeItemId);
                order.OrderDetails.Add(new USparkOrderDetail()
                {
                    //OrderId = orderId,
                    Amount = invoiceItem.FeeItemId == 1 ? invoiceItem.TotalAmount + invoiceItem.DiscountAmount : invoiceItem.TotalAmount,
                    ItemCode = feeItem.Code,
                    ItemNameEn = feeItem.NameEn,
                    ItemNameTh = feeItem.NameTh,
                    KSRegistrationCourseId = invoiceItem.RegistrationCourseId,
                    KSCourseId = invoiceItem.CourseId,
                    KSSectionId = invoiceItem.SectionId,
                    LumpSumAddDrop = invoiceItem.LumpSumAddDrop
                });
            }
            order.TotalAmount = invoiceItems.Sum(x => x.FeeItemId == 1 ? x.TotalAmount + x.DiscountAmount : x.TotalAmount);
            return order;
        }

        public void UpdateStudentStateToAdd(Guid studentId, long termId)
        {
            var studentState = _db.StudentStates.FirstOrDefault(x => x.StudentId == studentId
                                                                     && x.TermId == termId);
            
            if (studentState != null)
            {
                studentState.State = "ADD";
                _db.SaveChanges();
            }
            else 
            {
                StudentState newStudentState = new StudentState()
                {
                    StudentId = studentId,
                    TermId = termId,
                    State = "ADD"
                };
                _db.StudentStates.Add(newStudentState);
                _db.SaveChanges();
            }
        }

        public void UpsertRegistrationCourses(IEnumerable<USparkPreregistrationViewModel> courses, string studentCode, long KSTermId, string userId)
        {
            if(courses == null || !courses.Any())
            {
                return;
            }

            var student = _db.Students.AsNoTracking()
                                      .SingleOrDefault(x => x.Code == studentCode);

            var term = _db.Terms.AsNoTracking()
                                .SingleOrDefault(x => x.Id == KSTermId);

            if(student == null)
            {
                throw new InvalidOperationException($"Student with code ({studentCode}) not found");
            }

            if (term == null)
            {
                throw new InvalidOperationException($"Term with id ({KSTermId}) not found");
            }

            var registrationCourses = _db.RegistrationCourses.IgnoreQueryFilters()
                                                             .Where(x => x.StudentId == student.Id
                                                                         && x.TermId == KSTermId)
                                                             .ToList();

            var newRegistrationCourses = new List<RegistrationCourse>();

            var sectionIds = courses.Select(x => x.KSSectionId).ToList();

            var sections = _db.Sections.AsNoTracking()
                                       .Where(x => sectionIds.Contains(x.Id))
                                       .ToList();

            var gradeId = _db.Grades.FirstOrDefault(x => x.Name == "X")?.Id;

            foreach (var course in courses)
            {
                var courseStatus = course.Status.ToLower();

                var matchingRegistration = registrationCourses.SingleOrDefault(x => x.USPreregistrationId == course.Id);

                if(matchingRegistration != null)
                {
                    matchingRegistration.Status = courseStatus;
                }
                else
                {
                    var sectionData = sections.SingleOrDefault(x => x.Id == course.KSSectionId);

                    if(sectionData == null)
                    {
                        throw new InvalidOperationException($"Section with id ({course.KSSectionId}) not found.");
                    }

                    var isDeleteCourse = courseStatus == "d";

                    var nonAssignIdRegistrationCourses = isDeleteCourse ?  registrationCourses.OrderByDescending(x => x.CreatedAt)
                                                                                              .FirstOrDefault(x => x.SectionId == sectionData.Id 
                                                                                                  && x.Status == courseStatus
                                                                                                  && x.IsActive
                                                                                                  && !x.USPreregistrationId.HasValue)
                                                                        : registrationCourses.OrderByDescending(x => x.CreatedAt)
                                                                                             .FirstOrDefault(x => x.SectionId == sectionData.Id 
                                                                                                  && x.Status != "d"
                                                                                                  && x.IsActive
                                                                                                  && !x.USPreregistrationId.HasValue);

                    if (nonAssignIdRegistrationCourses != null)
                    {
                        nonAssignIdRegistrationCourses.USPreregistrationId = course.Id;
                    }
                    else
                    {
                        var newCourse = new RegistrationCourse
                        {
                            StudentId = student.Id,
                            TermId = term.Id,
                            CourseId = sectionData.CourseId,
                            SectionId = sectionData.Id,
                            IsPaid = course.IsPaid,
                            IsSurveyed = false,
                            Status = courseStatus,
                            CreatedAt = course.CreatedAt,
                            CreatedBy = userId ?? "USpark",
                            UpdatedAt = course.UpdatedAt,
                            UpdatedBy = userId ?? "USpark",
                            Channel = course.Channel,
                            USPreregistrationId = course.Id,
                            GradeName = "x",
                            GradeId = gradeId
                        };

                        newRegistrationCourses.Add(newCourse);
                    }
                }
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                if (newRegistrationCourses.Any())
                {
                    _db.RegistrationCourses.AddRange(newRegistrationCourses);
                }

                transaction.Commit();
            }

            _db.SaveChangesNoAutoUserIdAndTimestamps();
        }

        public void UpsertRegistrationLog(IEnumerable<USparkRegistrationLogViewModel> logs, string studentCode, long KSTermId)
        {
            if(logs == null || !logs.Any())
            {
                return;
            }

            var student = _db.Students.AsNoTracking()
                                      .SingleOrDefault(x => x.Code == studentCode);

            var term = _db.Terms.AsNoTracking()
                                .SingleOrDefault(x => x.Id == KSTermId);

            if (student == null)
            {
                throw new InvalidOperationException($"Student with code ({studentCode}) not found");
            }

            if (term == null)
            {
                throw new InvalidOperationException($"Term with id ({KSTermId}) not found");
            }

            var registrationLogs = _db.RegistrationLogs.AsNoTracking()
                                                       .IgnoreQueryFilters()
                                                       .Where(x => x.StudentId == student.Id
                                                                   && x.TermId == term.Id)
                                                       .ToList();

            var newRegistrationLogs = new List<RegistrationLog>();

            foreach(var request in logs)
            {
                var matchingLog = registrationLogs.SingleOrDefault(x => x.USparkId == request.Id);

                if(matchingLog != null)
                {
                    continue;
                }

                var newLog = new RegistrationLog
                {
                    USparkId = request.Id,
                    StudentId = student.Id,
                    TermId = term.Id,
                    Modifications = request.Modifications,
                    RegistrationCourses = request.RegistrationCourses,
                    Type = null,
                    Round = request.Round,
                    Channel = request.Channel,
                    CreatedAt = request.CreatedAt,
                    CreatedBy = request.CreatedBy
                };

                newRegistrationLogs.Add(newLog);
            }

            if (newRegistrationLogs.Any())
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    _db.RegistrationLogs.AddRange(newRegistrationLogs);

                    _db.SaveChangesNoAutoUserIdAndTimestamps();

                    transaction.Commit();
                }
            }
        }

        public void TransferCourseToUspark(USparkTransferCourse model)
        {
            var body = JsonConvert.SerializeObject(model);

            var endpoint = $"{ _USparkAPIURL }/students/transferCourse";

            var content = new StringContent(body, Encoding.UTF8, "application/json");

            var header = new Dictionary<string, string>
            {
                {"x-api-key", _USparkAPIKey }
            };

            var response =  _httpClientProxy.PutAsync(endpoint, header, content).Result;

            var responseContent = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return; 
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ArgumentException(responseContent);
            }
            else if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception(responseContent);
            }
            else
            {
                throw new Exception(responseContent);
            }
        }
    }
}