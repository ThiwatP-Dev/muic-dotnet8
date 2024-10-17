using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Withdrawals;
using Microsoft.EntityFrameworkCore;

namespace KeystoneLibrary.Providers
{
    public class WithdrawalProvider : IWithdrawalProvider
    {
        protected readonly ApplicationDbContext _db;
        protected readonly IAcademicProvider _academicProvider;

        public WithdrawalProvider(ApplicationDbContext db,IAcademicProvider academicProvider)
        {
            _db = db;
            _academicProvider = academicProvider;
        }

        public Withdrawal GetWithdrawal(long id)
        {
            var model = _db.Withdrawals.IgnoreQueryFilters()
                                       .SingleOrDefault(x => x.Id == id);
            
            return model;                           
        }

        public bool IsInWithdrawalPeriod(long academicLevelId, string type, DateTime requestedAt)
        {
            var isInWithdrawalPeriod = _db.WithdrawalPeriods.Any(x => x.StartedAt.Date <= requestedAt.Date
                                                                      && requestedAt.Date <= x.EndedAt.Date
                                                                      && x.AcademicLevelId == academicLevelId
                                                                      && x.Type == type);
            return isInWithdrawalPeriod;
        }

        public List<Withdrawal> GetWithdrawalsByRegistrationCourses(List<long> registrationCourseIds, string type)
        {
            var withdrawals = _db.Withdrawals.Where(x => registrationCourseIds.Contains(x.RegistrationCourseId)
                                                         && x.Type == type
                                                         && x.Status != "r"
                                                         )
                                             .ToList();
            return withdrawals;
        }

        public bool IsPeriodExisted(WithdrawalPeriod period)
        {
            var isPeriodExisted = _db.WithdrawalPeriods.Any(x => x.Id != period.Id
                                                                 && x.AcademicLevelId == period.AcademicLevelId
                                                                 && x.Type == period.Type
                                                                 && x.TermId == period.TermId
                                                                 && ((x.StartedAt <= period.StartedAt
                                                                     && period.StartedAt <= x.EndedAt)
                                                                     || (x.StartedAt <= period.EndedAt
                                                                     && period.EndedAt <= x.EndedAt)));
            return isPeriodExisted;
        }
        
        public List<WithdrawalException> GetWithdrawalExceptionsByDepartmentIds(List<long> departmentIds)
        {
            var withdrawalExceptions = _db.WithdrawalExceptions.Where(x => departmentIds.Contains(x.DepartmentId ?? 0))
                                                               .ToList();
            return withdrawalExceptions;
        }
        public WithdrawalReportViewModel GetWithdrawalReport(Criteria criteria)
        {
            var model = new WithdrawalReportViewModel();
            model.Criteria = criteria;
            model.Term = _academicProvider.GetTerm(criteria.TermId).TermText;
            var query = _db.Withdrawals.AsNoTracking()
                                       .Where(x => x.RegistrationCourse.TermId == criteria.TermId
                                                   && (!criteria.CourseIds.Any()
                                                       || criteria.CourseIds.Contains(x.RegistrationCourse.Course.Id))
                                                   && x.Status == "a")
                                       .Select(x => new
                                                    {
                                                        CourseCode = x.RegistrationCourse.Course.Code,
                                                        CourseSpecialChar = x.RegistrationCourse.Course.SpecialChar,
                                                        CourseName = x.RegistrationCourse.Course.NameEn,
                                                        InstructorCodeAndName = x.Instructor.CodeAndName,
                                                        StudentId = x.StudentId,
                                                        StudentCode = x.Student.Code,
                                                        StudentFirstName = x.Student.FirstNameEn,
                                                        StudentLastName = x.Student.LastNameEn,
                                                        StudentMidName = x.Student.MidNameEn,
                                                        DepartmentName = x.Student.AcademicInformation.Department.NameEn,
                                                        SectionNumber = x.RegistrationCourse.Section.Number,
                                                        StudentTitle = x.Student.Title.NameEn,
                                                        Type = x.Type,
                                                        InstructorCode = x.Instructor.Code,
                                                        InstructorFirstName = x.Instructor.FirstNameEn,
                                                        InstructorLastName = x.Instructor.LastNameEn,
                                                        InstructorTitle = x.Instructor.Title.NameEn,
                                                        Remark = x.Remark
                                                    })
                                       .ToList();
            if (criteria.GroupWithdrawBy == "c")
            {
                model.WithdrawalReportByCourses = query.GroupBy(x => new
                                                                     {
                                                                         x.CourseCode,
                                                                         x.CourseSpecialChar,
                                                                         x.CourseName
                                                                     })
                                                       .Select(x => new WithdrawalReportByCourse
                                                                    {
                                                                        CourseCode = x.Key.CourseCode,
                                                                        CourseName = x.Key.CourseName,
                                                                        CourseSpecialChar = x.Key.CourseSpecialChar,
                                                                        Withdrawals = x.Select(y => new WithdrawalReportByCourse.Item
                                                                                                    {
                                                                                                        InstructorCode = y.InstructorCode,
                                                                                                        InstructorTitle = y.InstructorTitle,
                                                                                                        InstructorFirstName = y.InstructorFirstName,
                                                                                                        InstructorLastName = y.InstructorLastName,
                                                                                                        StudentCode = y.StudentCode,
                                                                                                        StudentFirstName = y.StudentFirstName,
                                                                                                        StudentLastName = y.StudentLastName,
                                                                                                        StudentMidName = y.StudentMidName,
                                                                                                        SectionNumber = y.SectionNumber,
                                                                                                        StudentTitle = y.StudentTitle,
                                                                                                        DepartmentName = y.DepartmentName,
                                                                                                        Type = y.Type,
                                                                                                        Remark = y.Remark
                                                                                                    })
                                                                                       .OrderBy(z => z.SectionNumber)
                                                                                       .ThenBy(z => z.StudentCode)
                                                                                       .ToList()
                                                                    })
                                                       .OrderBy(x => x.CourseCode)
                                                       .ToList();
            }
            else if (criteria.GroupWithdrawBy == "sc")
            {
                model.WithdrawalReportByStudents = query.GroupBy(x => new
                                                                      {
                                                                          x.StudentId,
                                                                          x.StudentTitle,
                                                                          x.StudentFirstName,
                                                                          x.StudentLastName,
                                                                          x.StudentMidName,
                                                                          x.StudentCode
                                                                      })
                                                        .Select(x => new WithdrawalReportByStudent
                                                                     {
                                                                         StudentCode = x.Key.StudentCode,
                                                                         StudentTitle = x.Key.StudentTitle,
                                                                         StudentFirstName = x.Key.StudentFirstName,
                                                                         StudentMidName = x.Key.StudentMidName,
                                                                         StudentLastName = x.Key.StudentLastName,
                                                                         DepartmentName = x.FirstOrDefault()?.DepartmentName ?? string.Empty,
                                                                         Withdrawals = x.Select(y => new WithdrawalReportByStudent.Item
                                                                                                     {
                                                                                                         CourseCode = y.CourseCode,
                                                                                                         CourseName = y.CourseName + y.CourseSpecialChar,
                                                                                                         InstructorCode = y.InstructorCode,
                                                                                                         InstructorFirstName = y.InstructorFirstName,
                                                                                                         InstructorLastName = y.InstructorLastName,
                                                                                                         InstructorTitle = y.InstructorTitle,
                                                                                                         SectionNumber = y.SectionNumber,
                                                                                                         Type = y.Type,
                                                                                                         Remark = y.Remark
                                                                                                     })
                                                                                        .OrderBy(z => z.CourseCode)
                                                                                        .ThenBy(z => z.SectionNumber)
                                                                                        .ToList()
                                                                     })
                                                        .OrderBy(x => x.StudentCode)
                                                        .ToList();
            }
            else if (criteria.GroupWithdrawBy == "sm")
            {
                model.WithdrawalReportByStudents = query.GroupBy(x => new
                                                                      {
                                                                          x.StudentId,
                                                                          x.StudentTitle,
                                                                          x.StudentFirstName,
                                                                          x.StudentLastName,
                                                                          x.StudentMidName,
                                                                          x.StudentCode
                                                                      })
                                                        .Select(x => new WithdrawalReportByStudent
                                                                     {
                                                                         StudentCode = x.Key.StudentCode,
                                                                         StudentTitle = x.Key.StudentTitle,
                                                                         StudentFirstName = x.Key.StudentFirstName,
                                                                         StudentMidName = x.Key.StudentMidName,
                                                                         StudentLastName = x.Key.StudentLastName,
                                                                         DepartmentName = x.FirstOrDefault()?.DepartmentName ?? string.Empty,
                                                                         Withdrawals = x.Select(y => new WithdrawalReportByStudent.Item
                                                                                                     {
                                                                                                         CourseCode = y.CourseCode,
                                                                                                         CourseName = y.CourseName + y.CourseSpecialChar,
                                                                                                         InstructorCode = y.InstructorCode,
                                                                                                         InstructorFirstName = y.InstructorFirstName,
                                                                                                         InstructorLastName = y.InstructorLastName,
                                                                                                         InstructorTitle = y.InstructorTitle,
                                                                                                         SectionNumber = y.SectionNumber,
                                                                                                         Type = y.Type,
                                                                                                         Remark = y.Remark
                                                                                                     })
                                                                                        .OrderBy(z => z.CourseCode)
                                                                                        .ThenBy(z => z.SectionNumber)
                                                                                        .ToList()
                                                                     })
                                                        .OrderBy(x => x.DepartmentName)
                                                        .ThenBy(x => x.StudentCode)
                                                        .ToList();
            }
            return model;
        }
        public List<WithdrawalException> GetExceptionalFaculties(long facultyId = 0, long departmentId = 0)
        {
            var withdrawalExceptions = _db.WithdrawalExceptions.Include(x => x.Department)
                                                               .Include(x => x.Faculty)
                                                               .Where(x => x.CourseId == null
                                                                           && (facultyId == 0
                                                                               || x.FacultyId == facultyId)
                                                                           && (departmentId == 0
                                                                               || departmentId == x.DepartmentId))
                                                               .OrderBy(x => x.Faculty.NameEn)
                                                                   .ThenBy(x => x.Department.NameEn)
                                                               .ToList();
            return withdrawalExceptions;
        }

        public List<WithdrawalException> GetExceptionalCourses(string courseName)
        {
            var exceptionalCourses = _db.WithdrawalExceptions.Include(x => x.Course)
                                                             .Where(x => (string.IsNullOrEmpty(courseName)
                                                                         || x.Course.Code.StartsWith(courseName)
                                                                         || x.Course.NameEn.StartsWith(courseName)
                                                                         || x.Course.NameTh.StartsWith(courseName))
                                                                         && x.FacultyId == null
                                                                         && x.DepartmentId == null)
                                                             .OrderBy(x => x.Course.Code)
                                                             .ToList();
            return exceptionalCourses;
        }

        public bool IsExistExceptionCourse(long courseId)
        {
            var isExisted = _db.WithdrawalExceptions.Any(x => x.CourseId == courseId);
            return isExisted;
        }

        public bool IsExistExceptionDepartment(long facultyId, long departmentId)
        {
            var isExisted = _db.WithdrawalExceptions.Any(x => x.FacultyId == facultyId
                                                              && x.DepartmentId == departmentId);
            return isExisted;
        }
    }
}