using KeystoneLibrary.Data;
using KeystoneLibrary.Models.DataModels.Graduation;
using Microsoft.EntityFrameworkCore;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Enumeration;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.USpark;

namespace KeystoneLibrary.Providers
{
    public class GraduationProvider : BaseProvider, IGraduationProvider
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly ICurriculumProvider _curriculumProvider;
        protected readonly IStudentProvider _studentProvider;
        protected readonly IStudentPhotoProvider _studentPhotoProvider;

        public GraduationProvider(ApplicationDbContext db,
                                  IDateTimeProvider dateTimeProvider,
                                  ICurriculumProvider curriculumProvider,
                                  IStudentProvider studentProvider,
                                  IStudentPhotoProvider studentPhotoProvider) : base(db)
        {
            _dateTimeProvider = dateTimeProvider;
            _curriculumProvider = curriculumProvider;
            _studentProvider = studentProvider;
            _studentPhotoProvider = studentPhotoProvider;
        }

        public long GetGraduatingRequestByStaff(Guid studentId)
        {
            if (!_db.GraduatingRequests.Any(x => x.StudentId == studentId))
            {   
                GraduatingRequest graduatingRequest = new GraduatingRequest()
                {
                    StudentId = studentId,
                    Channel = "o",
                    Status = "w"
                };
                _db.GraduatingRequests.Add(graduatingRequest);
                _db.SaveChanges();
                return graduatingRequest.Id;
            }
            else 
            {
               var graduatingRequest = _db.GraduatingRequests.FirstOrDefault(x => x.StudentId == studentId);
               return graduatingRequest.Id;
            }
        }

        public IOrderedQueryable<GraduatingRequestExcelViewModel> GetGraduatingRequest(Criteria criteria)
        {
            DateTime? start = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? end = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);

            start = start ?? DateTime.MinValue;
            end = end ?? DateTime.MaxValue;

            var expectTerm = _db.Terms.AsNoTracking()
                                      .SingleOrDefault(x => x.Id == criteria.ExpectedGraduationTermId);

            int expectAcademicTerm = expectTerm != null ? expectTerm.AcademicTerm : 0;
            int expectAcademicYear = expectTerm != null ? expectTerm.AcademicYear : 0;

            var model = _db.Students.AsNoTracking()
                                    .IgnoreQueryFilters()
                                    .Where(x => (x.AcademicInformation.CurriculumVersion.ExpectCredit ?? 0) <= x.AcademicInformation.CreditComp
                                                // && x.StudentStatus != "g"
                                                && ((criteria.RequestType == null
                                                        && (!x.GraduatingRequest.RequestedDate.HasValue 
                                                            || ((string.IsNullOrEmpty(criteria.StartedAt) || x.GraduatingRequest.RequestedDate.Value.Date >= start.Value.Date)
                                                                 && (string.IsNullOrEmpty(criteria.EndedAt) || x.GraduatingRequest.RequestedDate.Value.Date <= end.Value.Date) )) )
                                                    || (Convert.ToBoolean(criteria.RequestType) 
                                                            && x.GraduatingRequest.RequestedDate.HasValue 
                                                            && (string.IsNullOrEmpty(criteria.StartedAt) || x.GraduatingRequest.RequestedDate.Value.Date >= start.Value.Date)
                                                            && (string.IsNullOrEmpty(criteria.EndedAt) || x.GraduatingRequest.RequestedDate.Value.Date <= end.Value.Date) ) 
                                                    || (!Convert.ToBoolean(criteria.RequestType) && !x.GraduatingRequest.RequestedDate.HasValue) )
                                                && (string.IsNullOrEmpty(criteria.Code)
                                                    || criteria.Code.Contains(x.Code))
                                                && (string.IsNullOrEmpty(criteria.Status)
                                                    || x.GraduatingRequest.Status == criteria.Status)
                                                && (criteria.AcademicLevelId == 0
                                                    || x.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId)
                                                && (criteria.FacultyId == 0
                                                    || x.AcademicInformation.FacultyId == criteria.FacultyId)
                                                && (criteria.DepartmentId == 0
                                                    || x.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                && (criteria.CurriculumId == 0
                                                    || x.AcademicInformation.CurriculumVersion.CurriculumId == criteria.CurriculumId)
                                                && (criteria.CurriculumVersionId == 0
                                                    || x.AcademicInformation.CurriculumVersionId == criteria.CurriculumVersionId)
                                                && (criteria.ExpectedGraduationYear == null
                                                    || x.GraduatingRequest.ExpectedAcademicYear == criteria.ExpectedGraduationYear)
                                                && (criteria.ExpectedGraduationTerm == null
                                                    || x.GraduatingRequest.ExpectedAcademicTerm == criteria.ExpectedGraduationTerm)
                                                //&& (start == null
                                                //    || x.GraduatingRequest.CreatedAt.Date >= start.Value.Date)
                                                //&& (end == null
                                                //    || x.GraduatingRequest.CreatedAt.Date <= end.Value.Date)
                                                
                                                && (criteria.StartStudentBatch == null
                                                    || x.AcademicInformation.Batch == criteria.StartStudentBatch)
                                                && (string.IsNullOrEmpty(criteria.StudentStatus)
                                                    || x.StudentStatus == criteria.StudentStatus)
                                                && (string.IsNullOrEmpty(criteria.IsGraduated)
                                                    || (Convert.ToBoolean(criteria.IsGraduated) ? x.StudentStatus.StartsWith("g")
                                                                                                : !x.StudentStatus.StartsWith("g"))))
                                    .Select(x => new GraduatingRequestExcelViewModel
                                                     {  
                                                        StudentId = x.Id,
                                                        StudentCode = x.Code,
                                                        StudentStatus = x.StudentStatus,
                                                        TitleEn = x.Title.NameEn,
                                                        TitleTh = x.Title.NameTh,
                                                        FirstNameTh = x.FirstNameTh,
                                                        FirstNameEn = x.FirstNameEn,
                                                        MidNameTh = x.MidNameTh,
                                                        MidNameEn = x.MidNameEn,
                                                        LastNameTh = x.LastNameTh,
                                                        LastNameEn = x.LastNameEn,
                                                        Email = x.PersonalEmail2,
                                                        TelephoneNumber = x.TelephoneNumber3,
                                                        FacultyNameEn = x.AcademicInformation.Faculty.NameEn,
                                                        FacultyNameTh = x.AcademicInformation.Faculty.NameTh,
                                                        FacultyCode = x.AcademicInformation.Faculty.Code,
                                                        DepartmentNameEn = x.AcademicInformation.Department.NameEn,
                                                        DepartmentNameTh = x.AcademicInformation.Department.NameTh,
                                                        DepartmentCode = x.AcademicInformation.Department.Code,
                                                        CurriculumVersionCode = x.AcademicInformation.CurriculumVersion.Code,
                                                        CurriculumVersionNameEn = x.AcademicInformation.CurriculumVersion.NameEn,
                                                        CurriculumVersionNameTh = x.AcademicInformation.CurriculumVersion.NameTh,
                                                        TotalCredit = x.AcademicInformation.CurriculumVersion.TotalCredit,
                                                        CreditComp = x.AcademicInformation.CreditComp,
                                                        CreditEarn = x.AcademicInformation.CreditEarned,
                                                        GPA = x.AcademicInformation.GPA,
                                                        GraduatingRequestId = x.GraduatingRequest.Id,
                                                        ExpectedAcademicTerm = x.GraduatingRequest.ExpectedAcademicTerm,
                                                        ExpectedAcademicYear = x.GraduatingRequest.ExpectedAcademicYear,
                                                        RequestedDate = x.GraduatingRequest.RequestedDate,
                                                        RequestStatus = x.GraduatingRequest.Status,
                                                     })
                                    .OrderBy(x => x.StudentCode);

            return model;
        }

        public async Task<GraduatingRequestViewModel> GetGraduatingRequestDetail(long graduatingRequestId)
        {
            var model = new GraduatingRequestViewModel();

            model.GraduatingRequestId = graduatingRequestId;


            model.GraduatingRequest = _db.GraduatingRequests.AsNoTracking()
                                                            .SingleOrDefault(x => x.Id == graduatingRequestId);

            
            model.Student = _db.Students.Include(x => x.AcademicInformation)
                                            .ThenInclude(x => x.CurriculumVersion)
                                            .ThenInclude(x => x.Curriculum)
                                        .Include(x => x.AcademicInformation)
                                            .ThenInclude(x => x.Faculty)
                                        .Include(x => x.AcademicInformation)
                                            .ThenInclude(x => x.Department)
                                        .Include(x => x.AcademicInformation)
                                            .ThenInclude(x => x.Advisor)
                                            .ThenInclude(x => x.Title)
                                        .Include(x => x.Nationality)
                                        .Include(x => x.StudentFeeType)
                                        .Include(x => x.Title)
                                        .AsNoTracking()
                                        .SingleOrDefault(x => x.Id == model.GraduatingRequest.StudentId);

            if (model.Student != null)
            {
                try
                {
                    model.Student.ProfileImageURL = await _studentPhotoProvider.GetStudentImg(model.Student.Code);
                }
                catch (Exception)
                {
                }
            }

            model.GraduationInformation = _db.GraduationInformations.FirstOrDefault(x => x.StudentId == model.Student.Id);

            // GET SPECIALIZATION GROUP
            var curriculumInfo = _db.CurriculumInformations.AsNoTracking()
                                                           .FirstOrDefault(x => x.StudentId == model.Student.Id);

            model.SpecializationGroups = _db.SpecializationGroupInformations.Include(x => x.SpecializationGroup)
                                                                            .Where(x => x.CurriculumInformationId == curriculumInfo.Id)
                                                                            .AsNoTracking()
                                                                            .Select(x => x.SpecializationGroup)
                                                                            .ToList();
            
            // GET REGISTRATION COURSES IN CURRENT TERM
            var currentTerm = model.Student.AcademicInformation != null
                              ? null : _db.Terms.FirstOrDefault(x => x.IsCurrent
                                                                     && x.AcademicLevelId == model.Student.AcademicInformation.AcademicLevelId);
            if (currentTerm != null)
            {
                model.CurrentTerm = currentTerm.TermText;
                // GET REGISTRATION COURSES
                var regisCourses =  _db.RegistrationCourses.Include(x => x.Course)
                                                           .Include(x => x.Term)
                                                           .Include(x => x.Grade)
                                                           .Where(x => x.StudentId == model.Student.Id 
                                                                       && x.Status != "d"
                                                                       && x.TermId == currentTerm.Id)
                                                           .AsNoTracking()
                                                           .OrderBy(x => x.Course.Code)
                                                           .ToList();
                    
                model.RegistrationCourses = regisCourses;
                model.CurrentTerm = currentTerm.TermText;
            }

            // GET COURSE PREDICTIONS
            model.CoursePredictions = GetCoursePredictions(graduatingRequestId);

            // GET GRADUATION COURSE GROUP MODIFICATION
            model.CurriculumVersionId = model.Student.AcademicInformation.CurriculumVersionId ?? 0;
            var curriculumVersion = _curriculumProvider.GetCurriculumVersion(model.CurriculumVersionId);

            int totalModificationCourseGroup = 0;
            var modificationCourseGroups = _curriculumProvider.GetCourseGroupModifications(model.CurriculumVersionId
                                                                                            , model.Student.Id
                                                                                            , graduatingRequestId
                                                                                            , out totalModificationCourseGroup);

            //  GET CURRICULUM COURSE GROUPS
            int totalCourseGroup = 0;
            var courseGroups = _curriculumProvider.GetCourseGroupWithRegistrationCourses(model.Student.Id
                                                                                         , model.CurriculumVersionId
                                                                                         , out totalCourseGroup);

            model.TotalCourseGroup = totalCourseGroup - 1;
            model.TotalCurriculumVersionCredit = curriculumVersion.TotalCredit;
            model.IsPublish = model.GraduatingRequest.IsPublished;
            model.CurriculumCourseGroups = courseGroups;
            model.CurriculumCourseGrouping = modificationCourseGroups;
            
            // GET OTHER COURSE GROUP                                               
            // var otherGroup = _curriculumProvider.GetOtherCourseGroupRegistrations(model.Student.Id, courses);
            // model.OtherCourseGroups = otherGroup;

            // MAPPING CURRICULUM COURSES

            model.GraduatingRequestLogs = _db.GraduatingRequestLogs.Where(x => x.GraduatingRequestId == graduatingRequestId)
                                                                   .ToList();

            model.GraduatingRequestLogs = model.GraduatingRequestLogs.Where(x => _db.Users.Any(y => y.Id == x.UpdatedBy))
                                                                     .Select(x => {
                                                                                      var user = _db.Users.Where(y => y.Id == x.UpdatedBy).SingleOrDefault();
                                                                                      var instructor = _db.Instructors.Where(y => y.Id == user.InstructorId)
                                                                                                                      .Select(y => new
                                                                                                                                   {
                                                                                                                                       Title = y.Title.NameEn,
                                                                                                                                       FirstName = y.FirstNameEn,
                                                                                                                                       LastName = y.LastNameEn
                                                                                                                                   })
                                                                                                                      .SingleOrDefault();

                                                                                      x.UpdatedByFullName = instructor == null ? (user == null? "" : 
                                                                                                                                    (string.IsNullOrEmpty(user.FirstnameEN) ? $"{user.UserName}" : 
                                                                                                                                    $"{user.FirstnameEN} {user.LastnameEN}"
                                                                                                                                    )
                                                                                                                                  )
                                                                                                                               : $"{ instructor.Title } { instructor.FirstName }{ instructor.LastName }";
                                                                                      return x;
                                                                                  })
                                                                     .ToList();
            // model.GroupingLogRegistrations = _curriculumProvider.GetCourseGroupRegistrationLogs(curriculumVersionId
            //                                                                                     , model.Student.Id);
            return model;
        }

        public CourseGroupModification AddCourseToGraduationCourseGroup(Guid studentId, long courseId, long courseGroupId, long? requiredGradeId)
        {
            var course = new CourseGroupModification();
            if (!_db.CourseGroupModifications.Any(x => x.StudentId == studentId
                                                       && x.CourseGroupId == courseGroupId
                                                       && x.CourseId == courseId))
            {
                course = new CourseGroupModification()
                         {
                             StudentId = studentId,
                             CourseId = courseId,
                             RequiredGradeId = requiredGradeId,
                             CourseGroupId = courseGroupId,
                             IsAddManually = true
                         };
                _db.CourseGroupModifications.Add(course);
                _db.SaveChanges();
            }

            return course;
        }

        public void DisabledCourseToGraduationCourseGroup(Guid studentId, long courseId, long courseGroupId)
        {
            var course = _db.CourseGroupModifications.FirstOrDefault(x => x.StudentId == studentId
                                                                          && x.CourseGroupId == courseGroupId
                                                                          && x.CourseId == courseId);
            if (course == null)
            {
                CourseGroupModification newCourse = new CourseGroupModification()
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    CourseGroupId = courseGroupId,
                    IsDisabled = true
                };
                _db.CourseGroupModifications.Add(newCourse);
            } 
            else 
            {
                course.IsDisabled = true; 
            }
            _db.SaveChanges();
        }

        public void ChangeStatus(long id, string status, string remark, bool isPublish)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var graduatingRequest = _db.GraduatingRequests.SingleOrDefault(x => x.Id == id);
                    if (graduatingRequest != null)
                    {
                        graduatingRequest.Status = status;
                        graduatingRequest.Remark = remark;
                        graduatingRequest.IsPublished = isPublish;

                        var log = new GraduatingRequestLog();
                        log.GraduatingRequestId = id;
                        log.Remark = remark;
                        log.Status = status;
                        log.IsPublished = isPublish;
                        _db.GraduatingRequestLogs.Add(log);

                        _db.SaveChanges();

                        //Auto Start Over
                        if (graduatingRequest.Status == "r")
                        {
                            _db.GraduatingRequestLogs.Add(new GraduatingRequestLog
                            {
                                GraduatingRequestId = id,
                                Status = null,
                                Remark = $"Status reject Auto start over. Old request date = {graduatingRequest.RequestedDate}, OldEmail = {graduatingRequest.Email}, OldPhone = {graduatingRequest.Telephone}"
                            });

                            graduatingRequest.RequestedDate = null;

                            _db.SaveChanges();
                        }
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }
        }

        public List<CoursePrediction> GetCoursePredictions(long graduatingRequestId)
        {
            var coursePredictions = _db.CoursePredictions.Where(x => x.GraduatingRequestId == graduatingRequestId).ToList();
            foreach (var coursePrediction in coursePredictions)
            {
                coursePrediction.Courses = new List<Course>();
                coursePrediction.TotalCredit = 0;
                var course = _db.Courses.FirstOrDefault(x => x.Id == coursePrediction.CourseId);
                if (course != null)
                {
                    coursePrediction.Courses.Add(course);
                    coursePrediction.TotalCredit = course.Credit;
                }
                // coursePrediction.CourseIdList = JsonConvert.DeserializeObject<List<long>>(coursePrediction.CourseIds);
                // coursePrediction.Courses = _db.Courses.Where(x => coursePrediction.CourseIdList.Contains(x.Id)).ToList();
                // coursePrediction.TotalCredit = coursePrediction.Courses.Sum(x => x.Credit);
            }
            return coursePredictions;
        }

        public void SaveCoursePredictions(long graduatingRequestId, List<CoursePrediction> coursePredictions)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var currentCoursePredictions = _db.CoursePredictions.Where(x => x.GraduatingRequestId == graduatingRequestId);
                    _db.CoursePredictions.RemoveRange(currentCoursePredictions);

                    foreach (var prediction in coursePredictions)
                    {
                        var coursePrediction = new CoursePrediction();
                        coursePrediction.GraduatingRequestId = graduatingRequestId;
                        // coursePrediction.TermId = prediction.TermId;
                        // coursePrediction.CourseIds = JsonConvert.SerializeObject(prediction.CourseIdList) ?? "[]";
                        _db.CoursePredictions.Add(coursePrediction);
                    }

                    _db.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }
        }
        public GraduatingRequestViewModel GetCourseGroupingLogs(long graduatingRequestId)
        {
            var model = new GraduatingRequestViewModel();
            model.GraduatingRequest = _db.GraduatingRequests.SingleOrDefault(x => x.Id == graduatingRequestId);
            model.Student = _db.Students.SingleOrDefault(x => x.Id == model.GraduatingRequest.StudentId);
            model.CourseGroupingLogs = _db.CourseGroupingLogs.Where(x => x.GraduatingRequestId == graduatingRequestId).ToList();
            return model;
        }
        public long DeleteCourseGroupingLog(long CourseGroupingLogId) 
        {
            long graduatingRequestId = 0;
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var courseGroupingLog = _db.CourseGroupingLogs.Include(x => x.CourseGroupingDetails).SingleOrDefault(x => x.Id == CourseGroupingLogId);
                    graduatingRequestId = courseGroupingLog.GraduatingRequestId;
                    _db.CourseGroupingDetails.RemoveRange(courseGroupingLog.CourseGroupingDetails);
                    _db.CourseGroupingLogs.Remove(courseGroupingLog);
                    _db.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
                return graduatingRequestId;
            }
        }

        public long UpdateCourseGroupingLogTogglePublish(long CourseGroupingLogId, bool IsPublished)
        {
            long graduatingRequestId = 0;
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var courseGroupingLog = _db.CourseGroupingLogs.SingleOrDefault(x => x.Id == CourseGroupingLogId);
                    graduatingRequestId = courseGroupingLog.GraduatingRequestId;
                    courseGroupingLog.IsPublished = IsPublished;

                    if (IsPublished)
                    {
                        var sameRequestCourseGroupingLogs = _db.CourseGroupingLogs.Where(x => x.Id != CourseGroupingLogId && x.GraduatingRequestId == courseGroupingLog.GraduatingRequestId);
                        foreach (var log in sameRequestCourseGroupingLogs)
                        {
                            log.IsPublished = false;
                        }
                    }

                    _db.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
                return graduatingRequestId;
            }
        }

        public long UpdateCourseGroupingLogToggleApprove(long CourseGroupingLogId, bool IsApproved)
        {
            long graduatingRequestId = 0;
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var courseGroupingLog = _db.CourseGroupingLogs.SingleOrDefault(x => x.Id == CourseGroupingLogId);
                    graduatingRequestId = courseGroupingLog.GraduatingRequestId;
                    courseGroupingLog.IsApproved = IsApproved;

                    if (IsApproved)
                    {
                        var sameRequestCourseGroupingLogs = _db.CourseGroupingLogs.Where(x => x.Id != CourseGroupingLogId && x.GraduatingRequestId == courseGroupingLog.GraduatingRequestId);
                        foreach (var log in sameRequestCourseGroupingLogs)
                        {
                            log.IsApproved = false;
                        }
                    }

                    _db.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
                return graduatingRequestId;
            }
        }
        public GraduatingRequestViewModel GetCourseGroupingDetail(long graduatingRequestId)
        {
            var model = new GraduatingRequestViewModel();
            model.GraduatingRequest = _db.GraduatingRequests.SingleOrDefault(x => x.Id == graduatingRequestId);
            model.Student = _db.Students.Include(x => x.AcademicInformation)
                                            .ThenInclude(x => x.CurriculumVersion)
                                            .ThenInclude(x => x.Curriculum)
                                        .Include(x => x.AcademicInformation)
                                            .ThenInclude(x => x.Faculty)
                                        .Include(x => x.AcademicInformation)
                                            .ThenInclude(x => x.Department)
                                        .SingleOrDefault(x => x.Id == model.GraduatingRequest.StudentId);

            var registrationCourses = _db.RegistrationCourses.Include(x => x.Course)
                                                             .Include(x => x.Grade)
                                                             .Where(x => x.StudentId == model.GraduatingRequest.StudentId 
                                                                         && x.Status != "d"
                                                                         && x.IsGradePublished 
                                                                         && x.Grade.Name != "U")
                                                             .OrderBy(x => x.Course.Code)
                                                             .Select(x => new 
                                                                          {
                                                                              x.Id,
                                                                              x.CourseId,
                                                                              x.Course.Code,
                                                                              x.Course.NameEnAndCredit,
                                                                              x.GradeName,
                                                                              x.Grade.Weight,
                                                                              x.IsStarCourse
                                                                          })
                                                             .ToList();
            var courseGroups = _db.CurriculumCourses.Include(x => x.CourseGroup)
                                                    .Include(x => x.Grade)
                                                    .Where(x => x.CourseGroup.CurriculumVersionId == model.Student.AcademicInformation.CurriculumVersionId)
                                                    .Select(x => new 
                                                                 {
                                                                     x.CourseGroupId,
                                                                     x.CourseGroup.NameEn,
                                                                     x.CourseId,
                                                                     GradeWeight = x.RequiredGradeId == null ? null : x.Grade.Weight,
                                                                     GradeName = x.RequiredGradeId == null ? string.Empty : x.Grade.Name
                                                                 })
                                                    .ToList();

            model.CourseGroupingCreates = new List<CourseGroupingCreateViewModel>();
            foreach (var courses in registrationCourses.GroupBy(x => x.CourseId))
            {
                var course = courses.FirstOrDefault();
                var courseCode = course.Code;
                var courseName = course.NameEnAndCredit;
                var groups = courseGroups.Where(x => x.CourseId == courses.Key)
                                         .ToList();
                if(!courses.Any(x=>!x.IsStarCourse))
                {
                    model.CourseGroupingCreates.Add(new CourseGroupingCreateViewModel
                                                        {
                                                            CourseId = courses.Key,
                                                            CourseCode = courseCode,
                                                            CourseName = courseName,
                                                            CourseGroupId = -1,
                                                            CourseGroupName = "Star",
                                                            RegistrationCourseIds = courses.Select(x => x.Id)
                                                                                           .ToList()
                                                        });
                } 
                else if (groups != null && groups.Any())
                {
                    if (_db.CurriculumBlacklistCourses.Any(x => x.CurriculumVersionId == model.Student.AcademicInformation.CurriculumVersionId
                                                                && x.CourseId == courses.Key))
                    {
                        model.CourseGroupingCreates.Add(new CourseGroupingCreateViewModel
                                                        {
                                                            CourseId = courses.Key,
                                                            CourseCode = courseCode,
                                                            CourseName = courseName,
                                                            CourseGroupId = -2,
                                                            CourseGroupName = "Not Count",
                                                            RegistrationCourseIds = courses.Select(x => x.Id)
                                                                                           .ToList()
                                                        });
                    }
                    else
                    {
                        foreach (var group in groups)
                        {
                            var _group = new CourseGroupingCreateViewModel
                                         {
                                             CourseId = courses.Key,
                                             CourseCode = courseCode,
                                             CourseName = courseName,
                                             CourseGroupId = group.CourseGroupId,
                                             CourseGroupName = group.NameEn
                                         };
                            if (group.GradeWeight == null)
                            {
                                _group.RegistrationCourseIds = courses.Select(x => x.Id)
                                                                      .ToList();
                                model.CourseGroupingCreates.Add(_group);
                            }
                            else
                            {
                                // if (group.GradeWeight == 0)
                                // {
                                //     _group.RegistrationCourseIds = courses.Where(x => x.GradeName == group.GradeName)
                                //                                           .Select(x => x.Id)
                                //                                           .ToList();
                                // }
                                // else
                                // {
                                _group.RegistrationCourseIds = courses.Where(x => x.Weight >= group.GradeWeight)
                                                                        .Select(x => x.Id)
                                                                        .ToList();
                                // }

                                if (_group.RegistrationCourseIds != null && _group.RegistrationCourseIds.Any())
                                {
                                    model.CourseGroupingCreates.Add(_group);
                                }
                            }
                        }
                    }
                }
                else
                {
                    model.CourseGroupingCreates.Add(new CourseGroupingCreateViewModel
                                                    {
                                                        CourseId = courses.Key,
                                                        CourseCode = courseCode,
                                                        CourseName = courseName,
                                                        CourseGroupId = -3,
                                                        CourseGroupName = "Free Elective",
                                                        RegistrationCourseIds = courses.Select(x => x.Id)
                                                                                       .ToList()
                                                    });
                }
            }
            
            return model;
        }

        public GraduatingRequestViewModel GetCourseGroupingLogDetail(long courseGroupingLogId)
        {
            var model = new GraduatingRequestViewModel();
            model.CourseGroupingLog = _db.CourseGroupingLogs.SingleOrDefault(x => x.Id == courseGroupingLogId);
            model.CourseGroupingCreates = new List<CourseGroupingCreateViewModel>();
            var _courseGroupingDetails = _db.CourseGroupingDetails.Include(x => x.Course)
                                                                  .Where(x => x.CourseGroupingLogId == courseGroupingLogId)
                                                                  .OrderBy(x => x.Course.CodeAndName)
                                                                  .ToList();
            foreach (var details in _courseGroupingDetails.GroupBy(x => new { x.CourseGroupId, x.CourseId }))
            {
                var detail = details.FirstOrDefault();
                model.CourseGroupingCreates.Add(new CourseGroupingCreateViewModel
                                                {
                                                    CourseId = details.Key.CourseId,
                                                    CourseCode = detail.Course.Code,
                                                    CourseName = detail.Course.NameEnAndCredit,
                                                    CourseGroupId = details.Key.CourseGroupId ?? 0,
                                                    CourseGroupName = detail.CourseGroupName,
                                                    RegistrationCourseIds = details.Select(x => x.RegistrationCourseId ?? 0)
                                                                                   .ToList()
                                                });
            }

            model.GraduatingRequest = _db.GraduatingRequests.SingleOrDefault(x => x.Id == model.CourseGroupingLog.GraduatingRequestId);

            model.Student = _db.Students.Include(x => x.AcademicInformation)
                                            .ThenInclude(x => x.CurriculumVersion)
                                            .ThenInclude(x => x.Curriculum)
                                        .Include(x => x.AcademicInformation)
                                            .ThenInclude(x => x.Faculty)
                                        .Include(x => x.AcademicInformation)
                                            .ThenInclude(x => x.Department)
                                        .SingleOrDefault(x => x.Id == model.GraduatingRequest.StudentId);
            return model;
        }

        public GraduatingRequestViewModel GetCourseGroupingLogMoveGroup(long courseGroupingLogId)
        {
            var model = new GraduatingRequestViewModel();
            model.CourseGroupingLogId = courseGroupingLogId;
            model.CourseGroupingLog = _db.CourseGroupingLogs.SingleOrDefault(x => x.Id == courseGroupingLogId);
            var _courseGroupingDetails = _db.CourseGroupingDetails.Include(x => x.Course)
                                                                  .Include(x => x.EquivalentCourse)
                                                                  .Include(x => x.RegistrationCourse)
                                                                      .ThenInclude(x => x.Grade)
                                                                  .Include(x => x.RegistrationCourse)
                                                                      .ThenInclude(x => x.Term)
                                                                  .Where(x => x.CourseGroupingLogId == courseGroupingLogId)
                                                                  .ToList();

            model.GraduatingRequest = _db.GraduatingRequests.SingleOrDefault(x => x.Id == model.CourseGroupingLog.GraduatingRequestId);

            model.GraduatingRequestId = model.GraduatingRequest.Id;
            model.Student = _db.Students.Include(x => x.AcademicInformation)
                                            .ThenInclude(x => x.CurriculumVersion)
                                            .ThenInclude(x => x.Curriculum)
                                        .Include(x => x.AcademicInformation)
                                            .ThenInclude(x => x.Faculty)
                                        .Include(x => x.AcademicInformation)
                                            .ThenInclude(x => x.Department)
                                        .SingleOrDefault(x => x.Id == model.GraduatingRequest.StudentId);
                                                                
            model.CourseGroups = _db.CourseGroups.Where(x => x.CurriculumVersionId == model.Student.AcademicInformation.CurriculumVersionId
                                                             && _db.CurriculumCourses.Any(y => y.CourseGroupId == x.Id))
                                                 .Select(x => new CourseGroupingDetailViewModel
                                                              {
                                                                  CourseGroupId = x.Id,
                                                                  CourseGroupName = x.NameEnAndCredit
                                                              })
                                                 .ToList();

            var starRow = new CourseGroupingDetailViewModel();
            starRow.CourseGroupId = -1;
            starRow.CourseGroupName = "Star";
            model.CourseGroups.Add(starRow);

            var notCountRow = new CourseGroupingDetailViewModel();
            notCountRow.CourseGroupId = -2;
            notCountRow.CourseGroupName = "Not Count";
            model.CourseGroups.Add(notCountRow);

            var freeElectiveRow = new CourseGroupingDetailViewModel();
            freeElectiveRow.CourseGroupId = -3;
            freeElectiveRow.CourseGroupName = "Free Elective";
            model.CourseGroups.Add(freeElectiveRow);

            foreach (var courseGroup in model.CourseGroups)
            {
                courseGroup.Courses = new List<CourseGroupingCreateViewModel>();
                var _grouping = _courseGroupingDetails.Where(x => x.CourseGroupId == courseGroup.CourseGroupId)
                                                      .ToList();
                if (_grouping != null && _grouping.Any())
                {
                    foreach (var details in _grouping.GroupBy(x => x.CourseId))
                    {
                        var detail = details.FirstOrDefault();
                        var registrations = details.Select(x => (x.RegistrationCourse.IsTransferCourse 
                                                                && !x.RegistrationCourse.IsStarCourse) ? $"<span class=\"ks-label bg-info mx-3 w-150\">{ x.RegistrationCourse.GradeName } ({ x.RegistrationCourse.Term.TermPeriodText })</span><br/>"
                                                                                                      : (x.RegistrationCourse.IsTransferCourse 
                                                                                                        && x.RegistrationCourse.IsStarCourse) ? $"<span class=\"ks-label bg-info mx-3 w-150\">{ x.RegistrationCourse.GradeName }* ({ x.RegistrationCourse.Term.TermPeriodText })</span><br/>"
                                                                                                                                          : $"<span class=\"ks-label bg-success mx-3 w-150\">{ x.RegistrationCourse.GradeName } ({ x.RegistrationCourse.Term.TermPeriodText })</span><br/>")
                                                   .ToList();
                        var course = new CourseGroupingCreateViewModel
                                     {
                                         CourseId = details.Key,
                                         CourseCode = detail.Course.Code,
                                         CourseName = $"{ detail.Course.NameEnAndCredit }",
                                         CourseCodeAndName = detail.Course.CourseAndCredit,
                                         GradeName = $"{ string.Join("", registrations) }",
                                         CourseGroupId = courseGroup.CourseGroupId,
                                         EquivalentCourseId = detail.EquivalentCourseId,
                                         RegistrationCourseIds = details.Select(x => x.RegistrationCourseId ?? 0)
                                                                        .ToList()
                                     };

                        if (detail.EquivalentCourseId != null)
                        {
                            course.CourseName = $"{ course.CourseName } (Equivalent {detail.EquivalentCourse.NameEnAndCredit})";
                        }

                        courseGroup.Courses.Add(course);
                    }
                }
            }

            return model;
        }

        public long SaveEqualCourses(GraduatingRequestViewModel model)
        {
            if (model.CourseGroupingLog == null || model.CourseGroupingLog.Id == 0)
            {
                //Add
                var courseGroupingLog = new CourseGroupingLog();
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        var graduatingRequest = _db.GraduatingRequests.SingleOrDefault(x => x.Id == model.GraduatingRequestId );
                        var academicInformation = _db.AcademicInformations
                                                    .Include(x => x.CurriculumVersion)
                                                        .ThenInclude(x => x.Curriculum)
                                                    .Include(x => x.Faculty)
                                                    .Include(x => x.Department)
                                                    .SingleOrDefault(x => x.StudentId == graduatingRequest.StudentId);

                        courseGroupingLog.GraduatingRequestId = model.GraduatingRequestId;
                        courseGroupingLog.Remark = model.CourseGroupingLog.Remark;
                        courseGroupingLog.CourseGroupingDetails = new List<CourseGroupingDetail>();
                        _db.CourseGroupingLogs.Add(courseGroupingLog);

                        var _courseGroupingDetails = model.CourseGroupingCreates.SelectMany(x => x.RegistrationCourseIds
                                                                                                  .Select(y => new CourseGroupingDetail
                                                                                                               {
                                                                                                                   CourseGroupId = x.CourseGroupId,
                                                                                                                   CourseGroupName = x.CourseGroupName,
                                                                                                                   CourseId = x.CourseId,
                                                                                                                   RegistrationCourseId = y,
                                                                                                                   EquivalentCourseId = x.EquivalentCourseId
                                                                                                               }))
                                                                                .ToList();
                        courseGroupingLog.CourseGroupingDetails.AddRange(_courseGroupingDetails);
                        
                        _db.SaveChanges();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }

                return courseGroupingLog.Id;
            }
            else 
            {
                //Update
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        var courseGroupingLog = _db.CourseGroupingLogs.SingleOrDefault(x => x.Id == model.CourseGroupingLog.Id);
                        courseGroupingLog.Remark = model.CourseGroupingLog.Remark;
                        var currentCourseGroupingDetails = _db.CourseGroupingDetails.Where(x => x.CourseGroupingLogId == model.CourseGroupingLog.Id).ToList();

                        var _courseGroupingDetails = model.CourseGroupingCreates.SelectMany(x => x.RegistrationCourseIds
                                                                                                  .Select(y => new CourseGroupingDetail
                                                                                                               {
                                                                                                                   CourseGroupingLogId = model.CourseGroupingLog.Id,
                                                                                                                   CourseGroupId = x.CourseGroupId,
                                                                                                                   CourseGroupName = x.CourseGroupName,
                                                                                                                   CourseId = x.CourseId,
                                                                                                                   RegistrationCourseId = y,
                                                                                                                   EquivalentCourseId = x.EquivalentCourseId
                                                                                                               }))
                                                                                .ToList();

                        _db.CourseGroupingDetails.AddRange(_courseGroupingDetails);
                        
                        _db.CourseGroupingDetails.RemoveRange(currentCourseGroupingDetails);
                        _db.SaveChanges();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
                return model.CourseGroupingLog.Id;
            }
        }

        public void SaveGroupingCourseMoveGroup(GraduatingRequestViewModel model)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var courseGroupingLog = _db.CourseGroupingLogs.SingleOrDefault(x => x.Id == model.CourseGroupingLogId);
                    var currentCourseGroupingDetails = _db.CourseGroupingDetails.Where(x => x.CourseGroupingLogId == model.CourseGroupingLogId).ToList();

                    foreach (var courseGroup in model.CourseGroups)
                    {
                        if (courseGroup.Courses != null && courseGroup.Courses.Any())
                        {
                            var _courseGroupingDetails = courseGroup.Courses.SelectMany(x => x.RegistrationCourseIds
                                                                                              .Select(y => new CourseGroupingDetail
                                                                                                          {
                                                                                                              CourseGroupingLogId = courseGroupingLog.Id,
                                                                                                              CourseGroupId = courseGroup.CourseGroupId,
                                                                                                              CourseGroupName = courseGroup.CourseGroupName,
                                                                                                              CourseId = x.CourseId,
                                                                                                              RegistrationCourseId = y,
                                                                                                              EquivalentCourseId = x.EquivalentCourseId
                                                                                                          }))
                                                                            .ToList();
                            _db.CourseGroupingDetails.AddRange(_courseGroupingDetails);
                        }
                        
                    }

                    _db.CourseGroupingDetails.RemoveRange(currentCourseGroupingDetails);

                    _db.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }

            if (model.IsPublish) 
            {
                UpdateCourseGroupingLogTogglePublish(model.CourseGroupingLogId, true);
            }
        }

        public GraduatingRequestViewModel GetGroupingCourseMoveGroupForPrint(long courseGroupingLogId)
        {
            var model = GetCourseGroupingLogMoveGroup(courseGroupingLogId);
            return model;
        }

        public bool SaveGraduation(Guid studentId, string studentStatus, DateTime? graduatedAt, long? termId, long? honorId, string remark, List<GraduationHonor> honors)
        {
            // Have same code duplicate in StudentAPIController.cs Update Status

            var graduatedStatus = StudentStatus.G.ToString().ToLower();
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var student = _db.Students.SingleOrDefault(x => x.Id == studentId);
                    if (student.StudentStatus != studentStatus)
                    {
                        if (!_studentProvider.SaveStudentStatusLog(studentId
                                                                   , termId.Value
                                                                   , SaveStatusSouces.GRADUATION.GetDisplayName()
                                                                   , remark
                                                                   , studentStatus
                                                                   , graduatedAt))
                        {
                            return false;
                        }
                    }

                    student.StudentStatus = studentStatus;
                    student.IsActive = KeystoneLibrary.Providers.StudentProvider.IsActiveFromStudentStatus(student.StudentStatus);

                    var graduationInfo = _db.GraduationInformations.FirstOrDefault(x => x.StudentId == studentId);
                    var otherRemark = string.Empty;
                    if (honors != null && honors.Any(x => x.IsChecked == "on"))
                    {
                        otherRemark = string.Join(", ", honors.Where(x => x.IsChecked == "on")
                                                              .Select(x => x.Code)
                                                              .ToList());
                    }
                    
                    if (graduationInfo == null)
                    {
                        var curriculumInfo = _studentProvider.GetCurrentCurriculum(studentId);
                        GraduationInformation info = new GraduationInformation
                                                     {
                                                         StudentId = studentId,
                                                         CurriculumInformationId = curriculumInfo?.Id,
                                                         GraduatedAt = graduatedAt,
                                                         TermId = termId,
                                                         HonorId = honorId,
                                                         Remark = remark,
                                                         OtherRemark1 = otherRemark
                                                     };

                        
                        
                        _db.GraduationInformations.Add(info);
                    }
                    else
                    {
                        graduationInfo.GraduatedAt = graduatedAt;
                        graduationInfo.TermId = termId;
                        graduationInfo.HonorId = honorId;
                        graduationInfo.Remark = remark;
                        graduationInfo.OtherRemark1 = otherRemark;
                    }

                    _db.SaveChanges();
                    transaction.Commit();
                    
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    
                    return false;
                }
            }
        }

        public List<GraduationHonor> GetGraduationHonors(Guid studentId)
        {
            List<GraduationHonor> checkList = new List<GraduationHonor>();
            var _registrations = _db.RegistrationCourses.Where(x => x.StudentId == studentId
                                                                    && x.Status != "d")
                                                        .ToList();
            var _academicInfo = _db.AcademicInformations.FirstOrDefault(x => x.StudentId == studentId);
            var _curriculumInfo = _db.CurriculumInformations.Include(x => x.CurriculumVersion)
                                                            .FirstOrDefault(x => x.StudentId == studentId);
            checkList.Add(new GraduationHonor
                          {
                              Code = "1",
                              Remark = "นักศึกษาแลกเปลี่ยน",
                              IsPassed = _db.Students.SingleOrDefault(x => x.Id == studentId)?.StudentStatus == StudentStatus.EX.ToString().ToLower()
                          });
            checkList.Add(new GraduationHonor
                          {
                              Code = "2",
                              Remark = "โอนย้ายหน่วยกิต",
                              IsPassed = false
                          });
            checkList.Add(new GraduationHonor
                          {
                              Code = "3",
                              Remark = "สาขาวิชาโท",
                              IsPassed = false
                          });
            checkList.Add(new GraduationHonor
                          {
                              Code = "4",
                              Remark = "ลงทะเบียนเรียนซ้ำ",
                              IsPassed = _registrations.GroupBy(x => x.CourseId)
                                                       .Any(x => x.Count() > 1)
                          });
            checkList.Add(new GraduationHonor
                          {
                              Code = "5",
                              Remark = "ลาพักการศึกษา",
                              IsPassed = _db.MaintenanceStatuses.Any(x => x.StudentId == studentId)
                          });
            checkList.Add(new GraduationHonor
                          {
                              Code = "6",
                              Remark = "ย้ายสาขาวิชาเอก",
                              IsPassed = (_academicInfo?.ChangedMajorCount ?? 0) > 0
                          });
            checkList.Add(new GraduationHonor
                          {
                              Code = "7",
                              Remark = "ใช้ระยะจัดทำโครงงานหรือ การศึกษาในรายวิชาอิสระ",
                              IsPassed = false
                          });
            checkList.Add(new GraduationHonor
                          {
                              Code = "8",
                              Remark = "ใช้ระยะเวลาฝึกงาน",
                              IsPassed = false
                          });
            checkList.Add(new GraduationHonor
                          {
                              Code = "9",
                              Remark = "ได้เกรด F",
                              IsPassed = _registrations.Any(x => x.GradeName == "F")
                          });
            checkList.Add(new GraduationHonor
                          {
                              Code = "10",
                              Remark = "ทำเรื่องถอนรายวิชา \"W\"",
                              IsPassed = _registrations.Any(x => x.GradeName == "W")
                          });
            checkList.Add(new GraduationHonor
                          {
                              Code = "11",
                              Remark = "ประกาศนียบัตรขั้นสูง (half minor)",
                              IsPassed = false
                          });
            checkList.Add(new GraduationHonor
                          {
                              Code = "12",
                              Remark = "ลงทะเบียนเรียนตามเกณฑ์ขั้นต่ำ",
                              IsPassed = false
                          });
            checkList.Add(new GraduationHonor
                          {
                              Code = "13",
                              Remark = "ลงทะเบียนเรียนเกินจากหลักสูตร",
                              IsPassed = _academicInfo.CreditComp >= (_curriculumInfo?.CurriculumVersion?.TotalCredit ?? 0)
                          });
            checkList.Add(new GraduationHonor
                          {
                              Code = "14",
                              Remark = "รับราชการทหาร 2 ปี (สัญชาติเกาหลี)",
                              IsPassed = false
                          });

            return checkList;
        }

        public GraduatingRequest CreateGraduatingRequest(Guid studentId)
        {
            var student = _db.Students.Include(x => x.AcademicInformation)
                                      .SingleOrDefault(x => x.Id == studentId);
            var academicLevelId = student?.AcademicInformation?.AcademicLevelId;
            var graduatedTermId = _db.Terms.FirstOrDefault(x => x.AcademicLevelId == academicLevelId
                                                                && x.IsCurrent)?.Id ?? 0;
            var telephones = new List<string>() 
                             { 
                                 student?.TelephoneNumber1, 
                                 student?.TelephoneNumber2 
                             }
                             .Where(x => !string.IsNullOrEmpty(x))
                             .ToList();

            var request = new GraduatingRequest
                          {
                              StudentId = studentId,
                              Telephone = string.Join(", ", telephones),
                              Email = student?.Email,
                              Channel = "o",
                              RequestedDate = DateTime.UtcNow,
                              Status = "w"
                          };

            _db.GraduatingRequests.Add(request);
            _db.SaveChanges();

            return request;
        }

        public List<USparkGraduatingTerm> GetGraduatingTerms(long academicLevelId, int totalTerms, int termPerYear)
        {
            var terms = new List<USparkGraduatingTerm>();

            var current = _db.Terms.AsNoTracking()
                                   .IgnoreQueryFilters()
                                   .FirstOrDefault(x => x.IsCurrent
                                                        && x.AcademicLevelId == academicLevelId);
            if (current != null)
            {
                var year = current.AcademicYear;
                var term = current.AcademicTerm;

                var prevTerm = (term - 1) % termPerYear;
                prevTerm = prevTerm == 0 ? termPerYear : prevTerm;
                var prevYear = prevTerm > term ? year - 1 : year;

                var previousTerm = new USparkGraduatingTerm
                {
                    Term = prevTerm,
                    Year = prevYear,
                    IsCurrent = false,
                    IsSummer = prevTerm == termPerYear
                };

                terms.Add(previousTerm);

                var presentTerm = new USparkGraduatingTerm
                {
                    Year = year,
                    Term = term,
                    IsCurrent = true,
                    IsSummer = term == termPerYear
                };

                terms.Add(presentTerm);

                for (int i = 1; i <= totalTerms; i++)
                {
                    var currentTerm = (term + i) % termPerYear;
                    if (currentTerm == 1)
                        year++;

                    terms.Add(new USparkGraduatingTerm
                              {
                                  Year = year,
                                  Term = currentTerm == 0 ? termPerYear : currentTerm,
                                  IsCurrent = false,
                                  IsSummer = currentTerm == 0
                              });
                }
            }

            return terms.OrderBy(x => x.Year)
                        .ThenBy(x => x.Term)
                        .ToList();
        }
    }
}