using KeystoneLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KeystoneLibrary.Models.USpark;
using KeystoneLibrary.Exceptions;
using KeystoneLibrary.Models.DataModels.Graduation;
using KeystoneLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using KeystoneLibrary.Models.ViewModel;

namespace Keystone.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class USparkGraduatingController : BaseController
    {
        private readonly ICurriculumProvider _curriculumProvider;
        private readonly IGraduationProvider _graduationProvider;

        public USparkGraduatingController(ApplicationDbContext db,
                                          ICurriculumProvider curriculumProvider,
                                          IGraduationProvider graduationProvider) : base(db)
        {
            _curriculumProvider = curriculumProvider;
            _graduationProvider = graduationProvider;
        }

        [HttpPost("GraduatingRequest")]
        public IActionResult GraduatingRequest(USparkGraduatingViewModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return Error(ApiException.Forbidden());
            }

            var student = _db.Students.AsNoTracking()
                                      .SingleOrDefault(x => x.Code == model.StudentCode);

            if (student == null)
            {
                return Error(StudentAPIException.StudentsNotFound());
            }

            var graduatingRequest = _db.GraduatingRequests.SingleOrDefault(x => x.Student.Code == model.StudentCode);

            if (graduatingRequest == null)
            {
                return Error(GraduatingAPIException.RequestNotFound());
            }

            if (graduatingRequest.RequestedDate.HasValue && graduatingRequest.Status != "r")
            {
                return Error(GraduatingAPIException.RequestExists());
            }

            if (!string.IsNullOrEmpty(graduatingRequest.Status) && graduatingRequest.Status != "r")
                return Error(GraduatingAPIException.RequestAttemped());

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    graduatingRequest.ExpectedAcademicTerm = model.ExpectedAcademicTerm;
                    graduatingRequest.ExpectedAcademicYear = model.ExpectedAcademicYear;
                    graduatingRequest.RequestedDate = DateTime.UtcNow;
                    graduatingRequest.Status = "w";

                    var log = new GraduatingRequestLog()
                                {
                                    GraduatingRequestId = graduatingRequest.Id,
                                    Status = "w",
                                    Remark = "Student start to request"
                                };
                    _db.GraduatingRequestLogs.Add(log);                                            

                    // var coursePredictions = _db.CoursePredictions.Where(x => x.GraduatingRequestId == graduatingRequest.Id);
                    // if (coursePredictions != null && coursePredictions.Any())
                    // {
                    //     _db.CoursePredictions.RemoveRange(coursePredictions);
                    //     _db.SaveChanges();
                    // }
                  
                    // if (model.Courses != null && model.Courses.Any())
                    // {
                    //     var predictions = model.Courses.Select(x => new CoursePrediction
                    //                                                 {
                    //                                                     GraduatingRequestId = graduatingRequest.Id,
                    //                                                     AcademicTerm = x.AcademicTerm,
                    //                                                     AcademicYear = x.AcademicYear,
                    //                                                     CourseId = x.CourseId
                    //                                                 });

                    //     _db.CoursePredictions.AddRange(predictions);
                    // }

                    _db.SaveChanges();
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

        [HttpPost("GraduatingRequest/CoursePredictions")]
        public IActionResult CoursePredictions(USparkGraduatingViewModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return Error(ApiException.Forbidden());
            }

            if (_db.Students.Any(x => x.Code == model.StudentCode))
            {
                if (!_db.GraduatingRequests.Any(x => x.Student.Code == model.StudentCode))
                {
                    return Error(GraduatingAPIException.RequestExists());
                }

                var graduatingRequest = _db.GraduatingRequests.First(x => x.Student.Code == model.StudentCode);
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        var coursePredictions = _db.CoursePredictions.Where(x => x.GraduatingRequestId == graduatingRequest.Id);
                        if (coursePredictions != null && coursePredictions.Any())
                        {
                            _db.CoursePredictions.RemoveRange(coursePredictions);
                            _db.SaveChanges();
                        }
                  
                        if (model.Courses != null && model.Courses.Any())
                        {
                            var predictions = model.Courses.Select(x => new CoursePrediction
                                                                        {
                                                                            GraduatingRequestId = graduatingRequest.Id,
                                                                            AcademicTerm = x.AcademicTerm,
                                                                            AcademicYear = x.AcademicYear,
                                                                            CourseId = x.CourseId
                                                                        });

                            _db.CoursePredictions.AddRange(predictions);
                        }

                        _db.SaveChanges();
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

            return Error(StudentAPIException.StudentsNotFound());
        }

        [HttpGet("GraduatingRequest/{studentCode}")]
        public IActionResult GraduatingRequest(string studentCode)
        {
            if (_db.Students.Any(x => x.Code == studentCode))
            {                
                var students = (from student in _db.Students
                                join academicInfo in _db.AcademicInformations on student.Id equals academicInfo.StudentId
                                where student.Code == studentCode
                                select new { student.Id, academicInfo.AcademicLevelId}).AsNoTracking().ToList();

                (var studentId, var academicLevelId) = students.Select(x => (x.Id, x.AcademicLevelId)).First();

                var requests = (from request in _db.GraduatingRequests
                                let courses = from prediction in _db.CoursePredictions
                                              join course in _db.Courses on prediction.CourseId equals course.Id
                                              where prediction.GraduatingRequestId == request.Id
                                              select new 
                                                     {
                                                         prediction.AcademicYear,
                                                         prediction.AcademicTerm,
                                                         prediction.CourseId,
                                                         course.Code,
                                                         course.NameEn,
                                                         course.NameTh,
                                                         course.Credit
                                                     }
                                where request.StudentId == studentId
                                select new 
                                {
                                    request.Id,
                                    Status = request.Status,
                                    request.ExpectedAcademicTerm,
                                    request.ExpectedAcademicYear,
                                    Courses = courses.ToList(),
                                    request.RequestedDate
                                }).AsNoTracking();

                var result = new USparkGraduatingRequestViewModel
                             {
                                 Terms = _graduationProvider.GetGraduatingTerms(academicLevelId, 4, 4)
                             };

                if (requests != null && requests.Any())
                {
                    var request = requests.First();
                    result.Id = request.Id;
                    result.ExpectedAcademicTerm = request.ExpectedAcademicTerm;
                    result.ExpectedAcademicYear = request.ExpectedAcademicYear;
                    result.Status = GraduatingRequestStatusText(request.Status);

                    // To support old data that input wrong term
                    if (result.ExpectedAcademicYear.HasValue
                            && result.ExpectedAcademicTerm.HasValue
                            && !result.Terms.Any(x => x.Year == result.ExpectedAcademicYear && x.Term == result.ExpectedAcademicTerm))
                    {
                        result.Terms = result.Terms.Append(new USparkGraduatingTerm
                                                            {
                                                                IsCurrent = false,
                                                                IsSummer = result.ExpectedAcademicTerm % 4 == 0, //Duplicate logic in _graduationProvider.GetGraduatingTerms
                                                                Term = result.ExpectedAcademicTerm.Value,
                                                                Year = result.ExpectedAcademicYear.Value
                                                            }).OrderBy(x => x.Year)
                                                            .ThenBy(x => x.Term)
                                                            .ToList();
                    }

                    if (request.Courses != null && request.Courses.Any())
                    {
                        result.TermCourses = request.Courses.GroupBy(x => new { x.AcademicYear, x.AcademicTerm })
                                                            .Select(x => new USparkGraduatingTermCourse
                                                                         {
                                                                             Year = x.Key.AcademicYear,
                                                                             Term = x.Key.AcademicTerm,
                                                                             Courses = x.Select(y => new USparkGraduatingCourse
                                                                                                     {
                                                                                                         Id = y.CourseId,
                                                                                                         Code = y.Code,
                                                                                                         NameEn = y.NameEn,
                                                                                                         NameTh = y.NameTh,
                                                                                                         Credit = y.Credit
                                                                                                     })
                                                                         });
                    }
                }

                return Success(result);
            }

            return Error(StudentAPIException.StudentsNotFound());
        }

        [HttpGet("GraduationRequest/{studentCode}/available")]
        public IActionResult CheckAvailableForGraduationCheckingRequest(string studentCode)
        {
            var student = _db.Students.AsNoTracking()
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.CurriculumVersion)
                                      .FirstOrDefault(x => x.Code == studentCode);

            if(student == null)
            {
                return Error(StudentAPIException.StudentsNotFound());
            }

            var expectedCredit = student.AcademicInformation?.CurriculumVersion?.ExpectCredit ?? 0;
            var creditComp = student.AcademicInformation?.CreditComp ?? 0;

            if(creditComp == 0 || expectedCredit > creditComp)
            {
                return Error(GraduatingAPIException.ExpectedCreditNotReach());
            }

            return Success(null);
        }

        [HttpPut("GraduatingRequest/Contact")]
        public IActionResult UpdateContact(USparkGraduatingViewModel model)
        {
            var student = _db.Students.Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.CurriculumVersion)
                                      .FirstOrDefault(x => x.Code == model.StudentCode);
            if (student != null)
            {
                if ((student.AcademicInformation.CurriculumVersion.ExpectCredit ?? 0) > student.AcademicInformation.CreditComp)
                    return Error(GraduatingAPIException.ExpectedCreditNotReach());

                // Update Student Contact
                student.TelephoneNumber3 = model.Telephone;
                student.PersonalEmail2 = model.Email;
                
                // Update Graduating Request
                if (_db.GraduatingRequests.Any(x => x.Student.Code == model.StudentCode))
                {
                    var graduatingRequest = _db.GraduatingRequests.First(x => x.Student.Code == model.StudentCode);
                    graduatingRequest.Telephone = model.Telephone;
                    graduatingRequest.Email = model.Email;
                    graduatingRequest.Status = "";
                }
                else // Create New Graduating Request
                {
                   var graduatingRequest = new GraduatingRequest
                                           {
                                                StudentId = student.Id,
                                                Telephone = model.Telephone,
                                                Email = model.Email,
                                                Channel = model.Channel,
                                                Status = "",
                                                IsAcceptTerm = true,
                                                // GraduatingRequestLogs = new List<GraduatingRequestLog>
                                                //                         {
                                                //                             new GraduatingRequestLog
                                                //                             {
                                                //                                 Status = "w",
                                                //                                 Remark = "Student start to request"
                                                //                             }
                                                //                         }
                                           };

                    using (var transaction = _db.Database.BeginTransaction())
                    {
                        try
                        {
                            _db.GraduatingRequests.Add(graduatingRequest);
                            _db.SaveChanges();
                            
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

                try
                {
                    _db.SaveChanges();
                    
                    return Success(0);
                }
                catch
                {
                    return Error(ApiException.Forbidden());
                }
            }

            return Error(GraduatingAPIException.RequestNotFound());
        }

        [HttpGet("GraduatingRequest/{studentCode}/Contact")]
        public IActionResult GetGraduatingRequestContact(string studentCode)
        {
            var student = _db.Students.AsNoTracking()
                                      .IgnoreQueryFilters()
                                      .SingleOrDefault(x => x.Code == studentCode);

            if(student == null)
            {
                return Error(GraduatingAPIException.RequestNotFound());
            }

            var response = new USparkGraduatingRequestContactViewModel
            {
                StudentCode = student.Code
            };

            var request = _db.GraduatingRequests.Where(x => x.RequestedDate.HasValue
                                                            && x.Status != "r")
                                                .SingleOrDefault(x => x.StudentId == student.Id);

            if (request == null)
                return Success(response);

            response.Email = request.Email;
            response.PhoneNumber = request.Telephone;

            return Success(response);
        }



        [HttpPost("GraduatingRequest/StartOver")]
        public IActionResult StartOver(USparkGraduatingViewModel model)
        {
            if (_db.GraduatingRequests.Any(x => x.Student.Code == model.StudentCode))
            {
                var request = _db.GraduatingRequests.FirstOrDefault(x => x.Student.Code == model.StudentCode);
                if (request == null)
                    return Error(GraduatingAPIException.RequestExists());

                request.RequestedDate = null;

                _db.GraduatingRequestLogs.Add(new GraduatingRequestLog
                                              {
                                                  GraduatingRequestId = request.Id,
                                                  Status = request.Status,
                                                  Remark = "Star Over"
                                              });
                
                try
                {
                    _db.SaveChanges();
                    
                    return Success(0);
                }
                catch
                {
                    return Error(ApiException.Forbidden());
                }
            }

            return Error(GraduatingAPIException.RequestNotFound());
        }

        [HttpGet("GraduatingRequest/Curriculum/{studentCode}")]
        public IActionResult CurriculumCourses(string studentCode)
        {
            var student = _db.Students.Include(x => x.AcademicInformation)
                                      .AsNoTracking()
                                      .SingleOrDefault(x => x.Code == studentCode);
            if (student != null)
            {
                var version = _curriculumProvider.GetCurriculumVersion(student.AcademicInformation.CurriculumVersionId ?? 0);
                if (version == null)
                    return Error(GraduatingAPIException.CurriculumVersionNotFound());

                var curriculum = new USparkCurriculum
                                 {
                                     Id = version.Id,
                                     NameEn = version.NameEn,
                                     NameTh = version.NameTh,
                                     DescriptionEn = version.Curriculum.DescriptionEn,
                                     DescriptionTh = version.Curriculum.DescriptionTh,
                                     TotalCredit = version.TotalCredit,
                                     AcademicYear = student.AdmissionInformation?.AdmissionTerm?.AcademicYear ?? 0,
                                     UpdatedAt = version.UpdatedAt,
                                     CourseGroups = _curriculumProvider.GetCourseGroupWithRegistrationCourses(student.Id
                                                                                                              , student.AcademicInformation.CurriculumVersionId ?? 0
                                                                                                              , out int totalCourseGroup)
                                 };
                
                return Success(curriculum);
            }
            
            return Error(StudentAPIException.StudentsNotFound());
        }

        private string GraduatingRequestStatusText(string status)
        {
            switch (status)
            {
                case "w":
                    return "Submitted";
                case "a":
                    return "Accepted";
                case "p":
                    return "Checking in progress";
                case "c":
                    return "Completed";
                case "r":
                    //return "Rejected";
                    return null;
                case "t":
                    return "Returned";
                default:
                    return "N/A";
            }
        }
    }
}