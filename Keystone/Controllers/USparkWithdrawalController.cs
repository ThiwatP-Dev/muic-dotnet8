using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.USpark;
using KeystoneLibrary.Exceptions;
using KeystoneLibrary.Models.DataModels.Withdrawals;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.EntityFrameworkCore;

namespace Keystone.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class USparkWithdrawalController : BaseController
    {
        public USparkWithdrawalController(ApplicationDbContext db) : base(db) { }

        [HttpGet("Student/Withdrawal")]
        public IActionResult GetWithdrawals(string studentCode, long termId)
        {
            var termStartedAt = _db.Terms.AsNoTracking().FirstOrDefault(x => x.Id == termId)?.StartedAt;
            var periods = _db.WithdrawalPeriods.Where(x => x.TermId == termId
                                                           && x.Type == "u")
                                               .Select(x => new 
                                                            {
                                                                x.StartedAt,
                                                                x.EndedAt
                                                            });
            if (periods.Any())
            {
                var today = DateTime.UtcNow;
                var period = periods.FirstOrDefault(x => today >= x.StartedAt
                                                         && today <= x.EndedAt);
                var isWithdrawalPeriod = period != null;
                var previousPeriod = periods.OrderByDescending(x => x.EndedAt).FirstOrDefault(x => today >= x.EndedAt.Date);
                var upcomingPeriod = periods.OrderByDescending(x => x.StartedAt).FirstOrDefault(x => today <= x.StartedAt.Date);
                var students = from student in _db.Students
                               join academic in _db.AcademicInformations on student.Id equals academic.StudentId
                               let courses = from registration in _db.RegistrationCourses
                                             join course in _db.Courses on registration.CourseId equals course.Id
                                             let statuses = (from withdrawal in _db.Withdrawals
                                                             let logs = _db.WithdrawalLogs.Where(x => x.WithdrawalId == withdrawal.Id)
                                                                                          .OrderByDescending(x => x.CreatedAt)
                                                             where withdrawal.RegistrationCourseId == registration.Id
                                                             orderby withdrawal.CreatedAt descending
                                                             select new
                                                             {
                                                                 withdrawal.Status,
                                                                 withdrawal.Remark,
                                                                 logs
                                                             }
                                                             )
                                             where registration.StudentId == student.Id
                                                   && registration.TermId == termId
                                                   && !registration.IsGradePublished
                                                   && !registration.IsStarCourse
                                                   && !registration.IsTransferCourse
                                                   && registration.Status != "d"
                                             select new 
                                                    {
                                                        registration.CourseId,
                                                        CourseCode = course.Code,
                                                        CourseLecture = course.Lecture,
                                                        CourseLab = course.Lab,
                                                        CourseOther = course.Other,
                                                        CourseNameEn = course.NameEn,
                                                        CourseNameTh = course.NameTh,
                                                        registration.SectionId,
                                                        course.Credit,
                                                        course.RegistrationCredit,
                                                        Status = statuses.FirstOrDefault(),
                                                        registration.UpdatedAt,
                                                    }
                               where student.Code == studentCode
                               select new 
                                      {
                                          academic.MinimumCredit,
                                          academic.CreditComp,
                                          Courses = courses.ToList()
                                      };

                if (students.Any())
                {
                    var student = students.First();
                    var result = new USparkWithdrawalViewModel
                                 {
                                     IsWithdrawalPeriod = isWithdrawalPeriod,
                                     StartedAt = period?.StartedAt ?? previousPeriod?.StartedAt ?? upcomingPeriod?.StartedAt ?? termStartedAt ?? today,
                                     EndedAt = period?.EndedAt ?? previousPeriod?.EndedAt ?? upcomingPeriod?.EndedAt ?? termStartedAt ?? today,
                                     MinCredit = student.MinimumCredit,                                    
                                     Courses = student.Courses
                                                      .Where(x => isWithdrawalPeriod || GetWithdrawalStatus(x.Status?.Status) != "Available")
                                                      .Select(x => {
                                                          var withdrawcouse = new USparkWithdrawalCourse
                                                          {
                                                              Id = x.CourseId,
                                                              Code = GetCourseCode(x.CourseCode, x.CourseLecture, x.CourseLab, x.CourseOther),
                                                              NameEn = x.CourseNameEn,
                                                              NameTh = x.CourseNameTh,
                                                              SectionId = x.SectionId,
                                                              Credit = x.Credit,
                                                              RegistrationCredit = x.RegistrationCredit,
                                                              Status = GetWithdrawalStatus(x.Status?.Status),
                                                              //Remark = x.Status?.Remark ?? string.Empty,
                                                              UpdatedAt = x.UpdatedAt
                                                          };
                                                          if (x.Status != null)
                                                          {
                                                              var approveOrRejectLog = x.Status.logs?.FirstOrDefault(y => y.Status == "a" || y.Status == "r");
                                                              withdrawcouse.UpdatedAt = x.Status.logs?.Max(y => y.UpdatedAt) ?? withdrawcouse.UpdatedAt;
                                                              if (approveOrRejectLog != null)
                                                              {
                                                                  withdrawcouse.Remark 
                                                                    = ( !string.IsNullOrEmpty(x.Status.Remark) ? $"Remark:\n{x.Status.Remark}\n\n" : String.Empty)
                                                                        + (!string.IsNullOrEmpty(approveOrRejectLog.Remark) ? $"Instructor Remark:\n{approveOrRejectLog.Remark}\n" : String.Empty);
                                                              }
                                                              else
                                                              {
                                                                  withdrawcouse.Remark = x.Status?.Remark ?? string.Empty;
                                                              }
                                                          }
                                                          return withdrawcouse;
                                                      })
                                 };
                    result.RegistrationCredit = result?.Courses?.Sum(x => x.RegistrationCredit) ?? 0;
                    return Success(result);
                }

                return Error(WithdrawalAPIException.StudentsNotFound());
            }

            return Error(WithdrawalAPIException.WithdrawalPeriodNotFound());
        }

        [HttpPost("Student/Withdrawal")]
        public IActionResult Withdrawal(USparkWithdrawalCriteria model)
        {
            var registrations = _db.RegistrationCourses.Include(x => x.Student)
                                                       .Include(x => x.Course)
                                                       .Include(x => x.Section)
                                                       .Include(x => x.Term)
                                                            .ThenInclude(x => x.TermType)
                                                       .Where(x => x.Student.Code == model.StudentCode
                                                                   && x.Status != "d"
                                                                   && !x.IsGradePublished
                                                                   && !x.IsStarCourse
                                                                   && !x.IsTransferCourse)
                                                       .Select(x => new 
                                                                    {
                                                                        x.Id,
                                                                        x.StudentId,
                                                                        x.TermId,
                                                                        x.SectionId,
                                                                        x.Course.RegistrationCredit,
                                                                        x.IsPaid,
                                                                        InstructorId = x.Section.MainInstructorId,
                                                                        TermTypeCode = x.Term.TermType.Code,
                                                                        x.Section.IsWithdrawable,
                                                                        x.GradeId
                                                                    });

            // var registrations = from registration in _db.RegistrationCourses
            //                     join student in _db.Students on registration.StudentId equals student.Id
            //                     join section in _db.Sections on registration.SectionId equals section.Id
            //                     join term in _db.Terms on registration.TermId equals term.Id
            //                     join termType in _db.TermTypes on term.TermTypeId equals termType.Id
            //                     where student.Code == model.StudentCode
            //                           && !registration.IsGradePublished
            //                           && !registration.IsStarCourse
            //                           && !registration.IsTransferCourse
            //                           && registration.Status != "d"
            //                           && registration.SectionId == model.SectionId
            //                     select new 
            //                            {
            //                                registration.Id,
            //                                registration.StudentId,
            //                                registration.TermId,
            //                                InstructorId = section.MainInstructorId,
            //                                TermTypeCode = termType.Code
            //                            };
            if (registrations.Any())
            {
                var registration = registrations.Where(x => x.SectionId == model.SectionId).First();

                if(registration == null)
                {
                    return Error(WithdrawalAPIException.RegistrationCourseNotFound());  
                }
                if (!registration.IsWithdrawable)
                {
                    return Error(WithdrawalAPIException.SectionNotAllowWithdrawal());
                }

                var today = DateTime.UtcNow;
                var periods = _db.WithdrawalPeriods.Where(x => x.TermId == registration.TermId
                                                               && x.Type == "u"
                                                               && today >= x.StartedAt
                                                               && today <= x.EndedAt);
                if (periods.Any())
                {
                    var withdrawalGrade = GetWithdrawalGrade();
                    if (withdrawalGrade == null)
                    {
                        return Error(WithdrawalAPIException.WithdrawalGradeNotFound());
                    }

                    var existWithdrawalList = _db.Withdrawals.AsNoTracking()
                                                             .Include(x => x.RegistrationCourse)
                                                             .Where(x => x.RegistrationCourse.TermId == registration.TermId                                                                            
                                                                             && x.IsActive
                                                                             && x.Status != "c" 
                                                                             && x.Status != "r"
                                                                             && x.StudentId == registration.StudentId
                                                             )
                                                             .ToList();
                    var approvedAndAlreadyAskedWithdrawSectionIdList = existWithdrawalList.Select(x => x.RegistrationCourse.SectionId).ToList();

                    // CHECK CREDIT LOAD
                    var totalMinimumCredit = 12; //TODO: get this number from correct way
                    var courseExceptWithdrawSections = registrations.Where(x => x.SectionId != model.SectionId
                                                                                && !approvedAndAlreadyAskedWithdrawSectionIdList.Contains(x.SectionId)
                                                                                && (x.GradeId == null || x.GradeId != withdrawalGrade.Id)
                                                                                && x.IsPaid)
                                                                    .ToList();
                    var totalRegistrationCredits = courseExceptWithdrawSections != null ? courseExceptWithdrawSections.Sum(x => x.RegistrationCredit) : 0;
                    if (totalRegistrationCredits < totalMinimumCredit)
                    {
                        return Error(WithdrawalAPIException.MinimumCreditLimit());
                    }
               

                    var existWithdrawal = _db.Withdrawals.AsNoTracking()
                                                         .Any(x => x.RegistrationCourseId == registration.Id
                                                                      && x.IsActive
                                                                      && x.Status != "c"
                                                             );
                    if (existWithdrawal)
                    {
                        return Error(WithdrawalAPIException.ExistWithdrawal());
                    }

                    using (var transaction = _db.Database.BeginTransaction())
                    {
                        try
                        {
                            var withdrawal = new Withdrawal
                                             {
                                                 RegistrationCourseId = registration.Id,
                                                 Type = "u",
                                                 RequestedAt = DateTime.UtcNow,
                                                 Remark = model.Reason,
                                                 InstructorId = registration.InstructorId,
                                                 StudentId = registration.StudentId,
                                                 Status = "w",
                                                 CreatedBy = registration.StudentId + "",
                                                 UpdatedBy = registration.StudentId + "",
                                                 CreatedAt = DateTime.UtcNow,
                                                 UpdatedAt = DateTime.UtcNow,
                            };

                            _db.Withdrawals.Add(withdrawal);
                            _db.SaveChangesNoAutoUserIdAndTimestamps();

                            var log = new WithdrawalLog
                                      {
                                          WithdrawalId = withdrawal.Id,
                                          Status = withdrawal.Status,
                                          Remark = model.Reason,
                                          CreatedBy = registration.StudentId + "",
                                          UpdatedBy = registration.StudentId + "",
                                          CreatedAt = DateTime.UtcNow,
                                          UpdatedAt = DateTime.UtcNow,
                                      };

                            _db.WithdrawalLogs.Add(log);
                            _db.SaveChangesNoAutoUserIdAndTimestamps();

                            transaction.Commit();
                            return Success(0);
                        }
                        catch
                        {
                            transaction.Rollback();
                            return Error(ApiException.Forbidden());
                        }
                    }
                }

                return Error(WithdrawalAPIException.NotInWithdrawalPeriod());
            }

            return Error(WithdrawalAPIException.RegistrationCourseNotFound());            
        }

        [HttpPost("Student/Withdrawal/Cancel")]
        public IActionResult Cancel(USparkWithdrawalCriteria model)
        {
            var withdrawals = from withdrawal in _db.Withdrawals
                              join registration in _db.RegistrationCourses on withdrawal.RegistrationCourseId equals registration.Id
                              join student in _db.Students on withdrawal.StudentId equals student.Id
                              where student.Code == model.StudentCode
                                    && registration.SectionId == model.SectionId
                                    && !registration.IsGradePublished
                                    && !registration.IsStarCourse
                                    && !registration.IsTransferCourse
                                    && registration.Status != "d"
                                    && withdrawal.Status == "w"
                              select withdrawal;

            if (withdrawals.Any())
            {
                var withdrawal = withdrawals.First();
                using (var transaction = _db.Database.BeginTransaction())
                {
                    withdrawal.Status = "c";
                    withdrawal.IsActive = false;
                    _db.WithdrawalLogs.Add(new WithdrawalLog
                                           {
                                               WithdrawalId = withdrawal.Id,
                                               Status = withdrawal.Status,
                                               Remark = model.Reason,
                                               CreatedBy = withdrawal.CreatedBy,
                                               UpdatedBy = withdrawal.CreatedBy,
                                               CreatedAt = DateTime.UtcNow,
                                               UpdatedAt = DateTime.UtcNow,
                    });
                                            
                    try
                    {
                        _db.SaveChangesNoAutoUserIdAndTimestamps();
                        transaction.Commit();

                        return Success(0);
                    }
                    catch
                    {
                        transaction.Rollback();
                        return Error(ApiException.Forbidden());
                    }
                }
            }

            return Error(WithdrawalAPIException.WithdrawalNotFound());
        }

        [HttpGet("Instructor/Withdrawal/Students")]
        public IActionResult GetWithdrawalStudents(long instructorId, long sectionId)
        {
            var withdrawals = (from withdrawal in _db.Withdrawals
                               join student in _db.Students on withdrawal.StudentId equals student.Id
                               join registration in _db.RegistrationCourses on withdrawal.RegistrationCourseId equals registration.Id
                               join section in _db.Sections on registration.SectionId equals section.Id
                               join masterSection in _db.Sections on section.ParentSectionId equals masterSection.Id into jointSections
                               from masterSection in jointSections.DefaultIfEmpty()
                               let logs = _db.WithdrawalLogs.Where(x => x.WithdrawalId == withdrawal.Id)
                                                            .OrderByDescending(x => x.UpdatedAt)
                               where (registration.SectionId == sectionId
                                           || (masterSection != null && masterSection.Id == sectionId)
                                         )
                                     && registration.Status != "d"
                                     && (section.MainInstructorId == instructorId
                                         || _db.SectionSlots.Any(x => x.SectionId == registration.SectionId
                                                                      && x.InstructorId == instructorId))
                               select new 
                                      {
                                          student.Code,
                                          withdrawal.Status,
                                          withdrawal.Remark,
                                          withdrawal.UpdatedAt,
                                          logs
                                      }).ToList();

            if (withdrawals.Any())
            {
                var results = new List<WithdrawalStudentViewModel>();
                foreach (var item in withdrawals)
                {
                    var withdrawal = new WithdrawalStudentViewModel
                                    {
                                        Code = item.Code,
                                        Status = GetWithdrawalStatus(item.Status),
                                        //Reason = item.Remark ?? string.Empty,
                                        UpdatedAt = item.UpdatedAt
                                    };
                    if (item.Status != null)
                    {
                        var approveOrRejectLog = item.logs?.FirstOrDefault(y => y.Status == "a" || y.Status == "r");
                        withdrawal.UpdatedAt = item.logs?.Max(y => y.UpdatedAt) ?? withdrawal.UpdatedAt;
                        if (approveOrRejectLog != null)
                        {
                            withdrawal.Reason
                              = (!string.IsNullOrEmpty(item.Remark) ? $"Student Remark:\n{item.Remark}\n\n" : String.Empty)
                                  + (!string.IsNullOrEmpty(approveOrRejectLog.Remark) ? $"Remark:\n{approveOrRejectLog.Remark}\n" : String.Empty);
                        }
                        else
                        {
                            withdrawal.Reason = item.Remark ?? string.Empty;
                        }
                    }
                    results.Add(withdrawal);
                }

                return Success(results);
            }

            return NotFound();
        }

        [HttpPost("Instructor/Withdrawal/Students/Approve")]
        public IActionResult Approve(USparkWithdrawalCriteria model)
        {
            var registrations = from registration in _db.RegistrationCourses
                                join student in _db.Students on registration.StudentId equals student.Id
                                join section in _db.Sections on registration.SectionId equals section.Id
                                join masterSection in _db.Sections on section.ParentSectionId equals masterSection.Id into jointSections
                                from masterSection in jointSections.DefaultIfEmpty()
                                where student.Code == model.StudentCode
                                      && (registration.SectionId == model.SectionId
                                           || (masterSection != null && masterSection.Id == model.SectionId)
                                         )
                                      && !registration.IsGradePublished
                                      && !registration.IsStarCourse
                                      && !registration.IsTransferCourse
                                      && registration.Status != "d"
                                select registration;
            if (registrations.Any())
            {
                var registration = registrations.First();
                var withdrawal = _db.Withdrawals.FirstOrDefault(x => x.RegistrationCourseId == registration.Id
                                                                     && x.Status == "w");
                if (withdrawal == null)
                {
                    return Error(WithdrawalAPIException.WithdrawalNotFound());
                }

                var withdrawalGrade = GetWithdrawalGrade();
                if (withdrawalGrade == null)
                {
                    return Error(WithdrawalAPIException.WithdrawalGradeNotFound());
                }

                var instructorUser = (from instructor in _db.Instructors
                                      join user in _db.Users on instructor.Code equals user.UserName
                                      where instructor.Id == withdrawal.InstructorId
                                      select user).FirstOrDefault();

                using (var transaction = _db.Database.BeginTransaction())
                {
                    withdrawal.Status = "a";
                    withdrawal.UpdatedBy = instructorUser != null ? instructorUser.Id : "instructorId:" + withdrawal.InstructorId;
                    withdrawal.UpdatedAt = DateTime.UtcNow;
                    _db.WithdrawalLogs.Add(new WithdrawalLog
                                           {
                                               WithdrawalId = withdrawal.Id,
                                               Status = withdrawal.Status,
                                               Remark = model.Reason,
                                               CreatedBy = withdrawal.UpdatedBy,
                                               UpdatedBy = withdrawal.UpdatedBy,
                                               CreatedAt = DateTime.UtcNow,
                                               UpdatedAt = DateTime.UtcNow,
                                            });

                    var gradeLog = new GradingLog
                                   {
                                       RegistrationCourseId = registration.Id,
                                       PreviousGrade = registration.GradeName,
                                       CurrentGrade = "W",
                                       Type = "w",
                                       CreatedBy = null, //withdrawal.UpdatedBy,
                                       UpdatedBy = null, //withdrawal.UpdatedBy,
                                       CreatedAt = DateTime.UtcNow,
                                       UpdatedAt = DateTime.UtcNow,
                                   };
                    
                    registration.GradeId = withdrawalGrade.Id;
                    registration.GradeName = withdrawalGrade.Name;
                    registration.IsGradePublished = false;

                    _db.GradingLogs.Add(gradeLog);

                    try
                    {
                        _db.SaveChangesNoAutoUserIdAndTimestamps();
                        transaction.Commit();

                        return Success(0);
                    }
                    catch
                    {
                        transaction.Rollback();
                        return Error(ApiException.Forbidden());
                    }
                }
            }

            return Error(WithdrawalAPIException.RegistrationCourseNotFound());
        }

        [HttpPost("Instructor/Withdrawal/Students/Reject")]
        public IActionResult Reject(USparkWithdrawalCriteria model)
        {
            var withdrawals = from withdrawal in _db.Withdrawals
                              join registration in _db.RegistrationCourses on withdrawal.RegistrationCourseId equals registration.Id
                              join section in _db.Sections on registration.SectionId equals section.Id
                              join masterSection in _db.Sections on section.ParentSectionId equals masterSection.Id into jointSections
                              from masterSection in jointSections.DefaultIfEmpty()
                              join student in _db.Students on withdrawal.StudentId equals student.Id
                              where student.Code == model.StudentCode
                                    && (registration.SectionId == model.SectionId
                                        || (masterSection != null && masterSection.Id == model.SectionId)
                                       )
                                    && !registration.IsGradePublished
                                    && !registration.IsStarCourse
                                    && !registration.IsTransferCourse
                                    && registration.Status != "d"
                                    && withdrawal.Status == "w"
                              select withdrawal;

           
            if (withdrawals.Any())
            {
                var withdrawal = withdrawals.First();
                var instructorUser = (from instructor in _db.Instructors
                                      join user in _db.Users on instructor.Code equals user.UserName
                                      where instructor.Id == withdrawal.InstructorId
                                      select user).FirstOrDefault();

                using (var transaction = _db.Database.BeginTransaction())
                {
                    withdrawal.Status = "r";
                    withdrawal.UpdatedBy = instructorUser != null ? instructorUser.Id : "instructorId:" + withdrawal.InstructorId;
                    withdrawal.UpdatedAt = DateTime.UtcNow;
                    _db.WithdrawalLogs.Add(new WithdrawalLog
                                           {
                                               WithdrawalId = withdrawal.Id,
                                               Status = withdrawal.Status,
                                               Remark = model.Reason,
                                               CreatedBy = withdrawal.UpdatedBy,
                                               UpdatedBy = withdrawal.UpdatedBy,
                                               CreatedAt = DateTime.UtcNow,
                                               UpdatedAt = DateTime.UtcNow,
                                           });
                                            
                    try
                    {
                        _db.SaveChangesNoAutoUserIdAndTimestamps();
                        transaction.Commit();

                        return Success(0);
                    }
                    catch
                    {
                        transaction.Rollback();
                        return Error(ApiException.Forbidden());
                    }
                }
            }

            return Error(WithdrawalAPIException.WithdrawalNotFound());
        }

        private string GetCourseCode(string code, decimal lecture, decimal lab, decimal other)
        {
            return $"{ code } ({ lecture.ToString(StringFormat.GeneralDecimal) }-{ lab.ToString(StringFormat.GeneralDecimal) }-{ other.ToString(StringFormat.GeneralDecimal) })";
        }

        private string GetWithdrawalStatus(string status)
        {
            switch (status)
            {
                case "w":
                    return "Waiting";
                case "a":
                    return "Approved";
                case "r":
                    return "Reject";
                case "c":
                    return "Cancel";
                default:
                    return "Available";
            }
        }

        private Grade GetWithdrawalGrade()
        {
            return _db.Grades.SingleOrDefault(x => x.Name.ToUpper() == "W");
        }
    }
}