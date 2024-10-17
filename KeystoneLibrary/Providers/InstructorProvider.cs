using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.EntityFrameworkCore;

namespace KeystoneLibrary.Providers
{
    public class InstructorProvider : IInstructorProvider
    {
        protected readonly ApplicationDbContext _db;

        public InstructorProvider(ApplicationDbContext db)
        {
            _db = db;
        }

        public Instructor GetInstructor(long id)
        {
            var instructor = _db.Instructors.Include(x => x.Nationality)
                                            .Include(x => x.Race)
                                            .Include(x => x.Religion)
                                            .Include(x => x.Country)
                                            .Include(x => x.Province)
                                            .Include(x => x.District)
                                            .Include(x => x.Subdistrict)
                                            .Include(x => x.City)
                                            .Include(x => x.State)
                                            .Include(x => x.Title)
                                            .Include(x => x.InstructorWorkStatus)
                                                .ThenInclude(x => x.Faculty)
                                            .Include(x => x.InstructorWorkStatus)
                                                .ThenInclude(x => x.Department)
                                            .Include(x => x.InstructorWorkStatus)
                                                .ThenInclude(x => x.AcademicLevel)
                                            .Include(x => x.InstructorWorkStatus)
                                                .ThenInclude(x => x.InstructorType)
                                            .Include(x => x.InstructorWorkStatus)
                                                .ThenInclude(x => x.InstructorRanking)
                                            .IgnoreQueryFilters()
                                            .SingleOrDefault(x => x.Id == id);
            return instructor;
        }

        public Instructor GetInstructorByCode(string code)
        {
            var instructor = _db.Instructors.Include(x => x.Title)
                                            .IgnoreQueryFilters()
                                            .SingleOrDefault(x => x.Code == code);
            return instructor;
        }

        public Instructor GetInstructorProfile(long id)
        {
            var instructor = GetInstructor(id);
            if (instructor != null)
            {
                var teachingStatus = (from sectionDetail in _db.SectionDetails
                                      join section in _db.Sections on sectionDetail.SectionId equals section.Id
                                      join sectionSlot in _db.SectionSlots on section.Id equals sectionSlot.SectionId
                                      join term in _db.Terms on section.TermId equals term.Id
                                      join course in _db.Courses on section.CourseId equals course.Id
                                      join room in _db.Rooms on sectionDetail.RoomId equals room.Id into rooms
                                      from room in rooms.DefaultIfEmpty()
                                      where sectionSlot.InstructorId == id
                                      group new { sectionDetail, sectionSlot, section, term, course, room }
                                      by section.TermId into sectionDetailGroup
                                      select new InstructorTeachingStatusViewModel
                                      {
                                          TermId = sectionDetailGroup.Key,
                                          Term = sectionDetailGroup.FirstOrDefault().term.TermText,
                                          AcademicTerm = sectionDetailGroup.FirstOrDefault().term.AcademicTerm,
                                          AcademicYear = sectionDetailGroup.FirstOrDefault().term.AcademicYear,
                                          InstructorTeachingDetails = sectionDetailGroup.GroupBy(x => x.section.Id)
                                                                                               .Select(x => new InstructorTeachingDetail
                                                                                               {
                                                                                                   AcademicLevelId = x.FirstOrDefault().term.AcademicLevelId,
                                                                                                   TermId = x.FirstOrDefault().term.Id,
                                                                                                   CourseId = x.FirstOrDefault().section.CourseId,
                                                                                                   SectionId = x.Key,
                                                                                                   Section = x.FirstOrDefault().section.Number,
                                                                                                   CourseCode = x.FirstOrDefault().course.Code,
                                                                                                   CourseName = x.FirstOrDefault().course.NameEn,
                                                                                                   Midterm = x.FirstOrDefault().section.Midterm,
                                                                                                   Final = x.FirstOrDefault().section.Final,
                                                                                                   IsClosed = x.FirstOrDefault().section.IsClosed,
                                                                                                   SectionDetails = x.Select(y => y.sectionDetail.DayofweekAndTime)
                                                                                                                                  .Distinct()
                                                                                                                                  .ToList(),
                                                                                                   Rooms = x.Where(y => y.room != null)
                                                                                                                         .Select(y => y.room.NameEn)
                                                                                                                         .Distinct()
                                                                                                                         .ToList()
                                                                                               }).ToList()
                                      }).OrderByDescending(x => x.AcademicYear)
                                               .ThenByDescending(x => x.AcademicTerm)
                                               .ToList();

                instructor.InstructorTeachingStatuses = teachingStatus;
            }

            return instructor;
        }

        public List<Instructor> GetInstructors(List<long> ids)
        {
            var instructors = _db.Instructors.Include(x => x.Nationality)
                                             .Include(x => x.Race)
                                             .Include(x => x.Religion)
                                             .Include(x => x.Country)
                                             .Include(x => x.Province)
                                             .Include(x => x.District)
                                             .Include(x => x.Subdistrict)
                                             .Include(x => x.City)
                                             .Include(x => x.State)
                                             .Include(x => x.Title)
                                             .Include(x => x.InstructorWorkStatus)
                                                .ThenInclude(x => x.Faculty)
                                             .Include(x => x.InstructorWorkStatus)
                                                .ThenInclude(x => x.Department)
                                             .Include(x => x.InstructorWorkStatus)
                                                .ThenInclude(x => x.AcademicLevel)
                                             .Where(x => ids.Contains(x.Id))
                                             .ToList();
            return instructors;
        }

        public List<Instructor> GetInstructors()
        {
            var instructors = _db.Instructors.Include(x => x.Title)
                                             .Include(x => x.InstructorWorkStatus)
                                             .ToList();
            return instructors;
        }

        public List<InstructorInfoViewModel> GetInstructors(Criteria criteria)
        {
            var instructors = _db.Instructors.Where(x => ((string.IsNullOrEmpty(criteria.Code) || x.Code.Contains(criteria.Code))
                                                           && (string.IsNullOrEmpty(criteria.FirstName)
                                                               || (!string.IsNullOrEmpty(x.FirstNameEn)
                                                                   && !string.IsNullOrEmpty(x.FirstNameTh)
                                                                   && (x.FirstNameEn.Contains(criteria.FirstName)
                                                                       || x.FirstNameTh.Contains(criteria.FirstName))))
                                                           && (string.IsNullOrEmpty(criteria.LastName)
                                                               || (!string.IsNullOrEmpty(x.LastNameEn)
                                                                   && !string.IsNullOrEmpty(x.LastNameTh)
                                                                   && (x.LastNameEn.Contains(criteria.LastName)
                                                                       || x.LastNameTh.Contains(criteria.LastName))))
                                                           && (criteria.FacultyId == 0
                                                               || x.InstructorWorkStatus.Faculty.Id == criteria.FacultyId)
                                                           && (criteria.DepartmentId == 0
                                                               || x.InstructorWorkStatus.Department.Id == criteria.DepartmentId)
                                                           && (criteria.Status == "All"
                                                               || string.IsNullOrEmpty(criteria.Status)
                                                               || x.IsActive == Convert.ToBoolean(criteria.Status))
                                                           && (criteria.InstructorTypeId == 0
                                                               || x.InstructorWorkStatus.InstructorTypeId == criteria.InstructorTypeId)))
                                                  .Select(x => new InstructorInfoViewModel 
                                                               {
                                                                    Code = x.Code,
                                                                    FirstNameEn = x.FirstNameEn,
                                                                    LastNameEn = x.LastNameEn,
                                                                    TitleEn = x.Title.NameEn,
                                                                    FacultyCode = x.InstructorWorkStatus.Faculty.Code,
                                                                    Faculty = x.InstructorWorkStatus.Faculty.NameEn,
                                                                    DepartmentCode = x.InstructorWorkStatus.Department.Code,
                                                                    Department = x.InstructorWorkStatus.Department.NameEn,
                                                                    IsActive = x.IsActive,
                                                                    Email = x.Email,
                                                                    Type = x.InstructorWorkStatus.InstructorType.NameEn,
                                                                    ProfileImageURL = x.ProfileImageURL,
                                                               })
                                                  .IgnoreQueryFilters()
                                                  .ToList();

            if (criteria.TermId > 0)
            {
                var allMainInstructor = _db.Sections.AsNoTracking()
                                                    .Where(x => x.TermId == criteria.TermId
                                                                    && x.MainInstructorId > 0)
                                                    .Select(x => x.MainInstructor.Code)
                                                    .Distinct()
                                                    .ToList();
                var allSlotInstructors = _db.SectionSlots.AsNoTracking()
                                                         .Where(x => x.Section.TermId == criteria.TermId
                                                                        && x.InstructorId > 0
                                                                        && x.Status != "c")
                                                         .Select(x => x.Instructor.Code)
                                                         .Distinct()
                                                         .ToList();
                allMainInstructor = allMainInstructor.Union(allSlotInstructors)
                                                     .Distinct()
                                                     .ToList();
                instructors = instructors.Where(x => allMainInstructor.Contains(x.Code))
                                         .ToList();
            }

            return instructors;
        }

        public List<Instructor> GetInstructorsByFacultyId(long id)
        {
            var instructors = _db.Instructors.Include(x => x.Title)
                                             .Include(x => x.InstructorWorkStatus)
                                             .Where(x => x.InstructorWorkStatus.FacultyId == id)
                                             .ToList();
            return instructors;
        }

        public List<Instructor> GetTermInstructorsByCourseId(long termId, long courseId)
        {
            var instructors = _db.InstructorSections.Include(x => x.Instructor)
                                                        .ThenInclude(x => x.Title)
                                                    .Include(x => x.SectionDetail)
                                                        .ThenInclude(x => x.Section)
                                                    .Where(x => x.SectionDetail.Section.TermId == termId
                                                                && x.SectionDetail.Section.CourseId == courseId)
                                                    .Select(x => x.Instructor)
                                                    .Distinct()
                                                    .ToList();

            return instructors;
        }

        public List<Instructor> GetTermInstructorsBySectionId(long sectionId)
        {
            var instructors = _db.InstructorSections.Include(x => x.Instructor)
                                                        .ThenInclude(x => x.Title)
                                                    .Include(x => x.SectionDetail)
                                                    .Where(x => x.SectionDetail.SectionId == sectionId)
                                                    .Select(x => x.Instructor)
                                                    .Distinct()
                                                    .ToList();

            return instructors;
        }

        public bool CheckCourseCoordinator(long termId, long courseId, long instructorId)
        {
            var checkCourseCoordinator = _db.Coordinators.Any(x => x.TermId == termId
                                                                   && x.CourseId == courseId
                                                                   && x.InstructorId == instructorId);
            return checkCourseCoordinator;
        }

        public List<Section> GetInstructorSection(long termId, long courseId, long instructorId)
        {
            var sections = _db.InstructorSections.Include(x => x.SectionDetail)
                                                     .ThenInclude(x => x.Section)
                                                 .Where(x => x.SectionDetail.Section.TermId == termId
                                                             && x.SectionDetail.Section.CourseId == courseId
                                                             && x.InstructorId == instructorId)
                                                 .Select(x => x.SectionDetail.Section)
                                                 .Distinct();

            var sectionDetails = sections.Include(x => x.Course)
                                         .Include(x => x.RegistrationCourses)
                                             .ThenInclude(x => x.Withdrawals)
                                         .OrderBy(x => x.Course.Code)
                                             .ThenBy(x => x.Number)
                                         .ToList();
            return sectionDetails;
        }

        public bool IsExistInstructor(string Code)
        {
            var instructor = _db.Instructors.IgnoreQueryFilters()
                                         .SingleOrDefault(x => x.Code == Code);

            var isExist = instructor != null && instructor.Code == Code;
            return isExist;
        }

        public List<Instructor> GetInstructorBySection(List<long> sectionIds)
        {
            var instructors = _db.InstructorSections.Include(x => x.Instructor)
                                                        .ThenInclude(x => x.Title)
                                                    .Include(x => x.SectionDetail)
                                                    .Where(x => sectionIds.Contains(x.SectionDetail.SectionId))
                                                    .Select(x => x.Instructor)
                                                    .Distinct()
                                                    .ToList();

            return instructors;
        }

        public void AssignAdvisee(List<Guid> studentListId, long instructorId)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                var instructor = _db.Instructors.FirstOrDefault(x => x.Id == instructorId);
                if (instructor == null) throw new KeyNotFoundException("Instructor");
                foreach (var studentId in studentListId)
                {
                    var student = _db.Students
                        .Include(x => x.AcademicInformation)
                        .FirstOrDefault(x => x.Id == studentId);
                    if (student == null) throw new KeyNotFoundException("StudentID");

                    var logs = _db.AdvisorStudents.Where(x => x.StudentId == studentId
                        && x.IsActive);
                    if (logs.Count() > 1)
                    {
                        throw new Exception("There should not be two active advisor for same student for now");
                    } 
                    else if (logs.Count() > 0)
                    {
                        var log = logs.First();
                        log.EndedAt = DateTime.UtcNow;
                        log.IsActive = false;
                    }
                    _db.AdvisorStudents.Add(new Models.DataModels.Advising.AdvisorStudent
                    {
                        EndedAt = DateTime.MaxValue,
                        InstructorId = instructorId,
                        StartedAt = DateTime.UtcNow,
                        StudentId = studentId,
                        IsActive = true,
                    }) ;

                    student.AcademicInformation.AdvisorId = instructorId;                    
                }
                _db.SaveChanges();
                transaction.Commit();
            }
        }
    }
}