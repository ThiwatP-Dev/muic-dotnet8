using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.USpark;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("ModifyRegistrationCourse", PolicyGenerator.Write)]
    public class ModifyRegistrationCourseController : BaseController 
    {
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IScholarshipProvider _scholarshipProvider;

        public ModifyRegistrationCourseController(ApplicationDbContext db,
                                                  IFlashMessage flashMessage,
                                                  ISelectListProvider selectListProvider,
                                                  IScholarshipProvider scholarshipProvider,
                                                  IRegistrationProvider registrationProvider) : base(db, flashMessage, selectListProvider) 
        {
            _registrationProvider = registrationProvider;
            _scholarshipProvider = scholarshipProvider;
        }
            
        public ActionResult Index(Criteria criteria)
        {
            if (string.IsNullOrEmpty(criteria.Code))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            if (!_db.Students.Any(x => x.Code == criteria.Code))
            {
                _flashMessage.Warning(Message.InvalidStudentCode);
                return View();
            }

            var registrations = (from registration in _db.RegistrationCourses
                                 join student in _db.Students on registration.StudentId equals student.Id
                                 join course in _db.Courses on registration.CourseId equals course.Id
                                 join term in _db.Terms on registration.TermId equals term.Id
                                 join section in _db.Sections on registration.SectionId equals section.Id into sections
                                 from section in sections.DefaultIfEmpty()
                                 where student.Code == criteria.Code
                                       && registration.Status != "d"
                                 orderby term.AcademicYear, term.AcademicTerm, course.Code, section.Number
                                 select new
                                        {
                                            term.AcademicYear,
                                            term.AcademicTerm,
                                            RegistrationCourseId = registration.Id,
                                            course.CourseRateId,
                                            course.Code,
                                            course.NameEn,
                                            course.Credit,
                                            course.Lecture,
                                            course.Lab,
                                            course.Other,
                                            SectionNumber = section.Number,
                                            registration.GradeName,
                                            registration.IsGradePublished,
                                            registration.IsPaid,
                                            registration.IsStarCourse,
                                            registration.IsTransferCourse
                                        }).ToList();

            var models = registrations.GroupBy(x => new { x.AcademicYear, x.AcademicTerm })
                                      .Select(x => new ModifyRegistrationCourseDetailViewModel
                                                   {
                                                       AcademicYear = x.Key.AcademicYear,
                                                       AcademicTerm = x.Key.AcademicTerm,
                                                       Courses = x.Select(y => new ModifyRegistrationCourse
                                                                               {
                                                                                   RegistrationCourseId = y.RegistrationCourseId,
                                                                                   CourseRateId = y.CourseRateId,
                                                                                   Code = y.Code,
                                                                                   NameEn = y.NameEn,
                                                                                   Credit = y.Credit,
                                                                                   Lecture = y.Lecture,
                                                                                   Lab = y.Lab,
                                                                                   Other = y.Other,
                                                                                   SectionNumber = y.SectionNumber,
                                                                                   GradeName = y.GradeName,
                                                                                   IsGradePublished = y.IsGradePublished,
                                                                                   IsPaid = y.IsPaid,
                                                                                   IsStarCourse = y.IsStarCourse,
                                                                                   IsTransfer = y.IsTransferCourse
                                                                               })
                                                                  .ToList()
                                                   })
                                      .ToList();

            var studentInfo = _db.Students.Where(x => x.Code == criteria.Code)
                                      .Select(x => new StudentInformationViewModel
                                                   {
                                                      StudentId = x.Id,
                                                      StudentCode = x.Code,
                                                      StudentStatus = x.StudentStatus,
                                                      TitleEn = x.Title.NameEn,
                                                      FirstNameEn = x.FirstNameEn,
                                                      MidNameEn = x.MidNameEn,
                                                      LastNameEn = x.LastNameEn,
                                                      FacultyCode = x.AcademicInformation.Faculty.Code,
                                                      FacultyNameEn = x.AcademicInformation.Faculty.NameEn,
                                                      DepartmentCode = x.AcademicInformation.Department.NameEn,
                                                      DepartmentNameEn = x.AcademicInformation.Department.NameEn,
                                                      CurriculumVersionCode = x.AcademicInformation.CurriculumVersion.Code,
                                                      CurriculumVersionNameEn = x.AcademicInformation.CurriculumVersion.NameEn,
                                                      ProfileImageURL = x.ProfileImageURL,
                                                      AdvisorTitleEn = x.AcademicInformation.Advisor.Title.NameEn,
                                                      AdvisorFirstNameEn = x.AcademicInformation.Advisor.FirstNameEn,
                                                      AdvisorLastNameEn = x.AcademicInformation.Advisor.LastNameEn,
                                                      CreditComp = x.AcademicInformation.CreditComp,
                                                      CreditEarn = x.AcademicInformation.CreditEarned,
                                                      GPA = x.AcademicInformation.GPA,
                                                      IsActive = x.IsActive,
                                                      IsCurrentStudentProbation = x.IsCurrentStudentProbation,
                                                      IsStudentExtended = x.IsStudentExtended
                                                   })
                                      .FirstOrDefault();
            
            studentInfo.CurrentScholarshipNameEn = _scholarshipProvider.GetCurrentScholarshipStudent(studentInfo.StudentId)?.Scholarship?.NameEn ?? "N/A";

            var result = new ModifyRegistrationCourseViewModel
                         {
                            Criteria = criteria,
                            CouresByTerms = models,
                            Student = studentInfo
                         };
            return View(result);
        }

        public IActionResult Logs(long id)
        {
            var model = GetCourseChangedLogs(id);
            return PartialView("_Logs", model);
        }

        public IActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();

            var registrations = (from registration in _db.RegistrationCourses
                                 join student in _db.Students on registration.StudentId equals student.Id
                                 join course in _db.Courses on registration.CourseId equals course.Id
                                 where registration.Id == id
                                 select new ModifyRegistrationCourse
                                        {
                                            RegistrationCourseId = registration.Id,
                                            CourseRateId = course.CourseRateId,
                                            Code = course.Code,
                                            NameEn = course.NameEn,
                                            Credit = course.Credit,
                                            Lecture = course.Lecture,
                                            Lab = course.Lab,
                                            Other = course.Other,
                                            ToCourseId = registration.CourseId,
                                            IsStarCourse = registration.IsStarCourse,
                                            ToStarCourse = registration.IsStarCourse,
                                            IsTransfer = registration.IsTransferCourse,
                                            ToTransferCourse = registration.IsTransferCourse
                                        }).FirstOrDefault();
            if (registrations == null)
            {
                return Redirect(returnUrl);
            }

            registrations.Logs = GetCourseChangedLogs(id);
            return View(registrations);   
        }

        [HttpPost]
        public IActionResult Edit(ModifyRegistrationCourse model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            model.Logs = GetCourseChangedLogs(model.RegistrationCourseId);

            if (ModelState.IsValid)
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    var registration = _db.RegistrationCourses.SingleOrDefault(x => x.Id == model.RegistrationCourseId);

                    var changedLog = new RegistrationChangeCourseLog
                                     {
                                         StudentId = registration.StudentId,
                                         RegistrationCourseId = model.RegistrationCourseId,
                                         FromCourseId = registration.CourseId,
                                         ToCourseId = model.ToCourseId,
                                         FromGradeName = registration.GradeName,
                                         FromGradeId = registration.GradeId,
                                         FromStarCourse = registration.IsStarCourse,
                                         ToStarCourse = model.ToStarCourse,
                                         FromSectionId = registration.SectionId,
                                         FromTransferCourse = model.IsTransfer,
                                         ToTransferCourse = model.ToTransferCourse
                                     };
                    try
                    {
                        if (registration.USPreregistrationId.HasValue && registration.USPreregistrationId.Value != System.Guid.Empty)
                        {
                            var duplicateRegistrationCourse = new RegistrationCourse(registration)
                            {
                                IsActive = false,
                                Status = "d"
                            };

                            registration.USPreregistrationId = null;

                            _db.RegistrationCourses.Add(duplicateRegistrationCourse);
                        }

                        var student = _db.Students.SingleOrDefault(x => x.Id == registration.StudentId);

                        var transferCourseToUSpark = new KeystoneLibrary.Models.USpark.USparkTransferCourse
                        {
                            StudentCode = student.Code,
                            OldCourses = new List<USparkTransferCourse.TransferCourseDetail>(),
                            NewCourses = new List<USparkTransferCourse.TransferCourseDetail>()
                        };

                        if (registration != null)
                        {
                            bool isChanged = false;
                            if (registration.CourseId != model.ToCourseId)
                            {
                                isChanged = true;

                                transferCourseToUSpark.OldCourses.Add(new USparkTransferCourse.TransferCourseDetail
                                {
                                    KsCourseId = registration.CourseId,
                                    KsSectionId = registration.SectionId,
                                    KsTermId = registration.TermId,
                                    Grade = registration.GradeName
                                });

                                transferCourseToUSpark.NewCourses.Add(new USparkTransferCourse.TransferCourseDetail
                                {
                                    KsCourseId = model.ToCourseId,
                                    KsSectionId = null,
                                    KsTermId = registration.TermId,
                                    Grade = registration.GradeName
                                });

                                registration.CourseId = model.ToCourseId;
                                registration.SectionId = null;

                            }

                            if (registration.IsStarCourse != model.ToStarCourse)
                            {
                                isChanged = true;

                                registration.IsStarCourse = model.ToStarCourse;
                            }

                            if (registration.IsTransferCourse != model.ToTransferCourse)
                            {
                               isChanged = true;

                               registration.IsTransferCourse = model.ToTransferCourse;
                            }

                            if (isChanged)
                            {
                                _db.RegistrationChangeCourseLogs.Add(changedLog);
                            }

                            _db.SaveChanges();

                            _registrationProvider.TransferCourseToUspark(transferCourseToUSpark);

                            transaction.Commit();
                            _flashMessage.Confirmation(Message.SaveSucceed);
                            return Redirect(returnUrl);
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
            }
            
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        private List<RegistrationChangeCourseLogViewModel> GetCourseChangedLogs(long registrationCourseId)
        {
            var logs = (from log in _db.RegistrationChangeCourseLogs
                        join fromCourse in _db.Courses on log.FromCourseId equals fromCourse.Id
                        join toCourse in _db.Courses on log.ToCourseId equals toCourse.Id
                        where log.RegistrationCourseId == registrationCourseId
                        select new 
                               {
                                   FromCourseRateId = fromCourse.CourseRateId,
                                   FromCode = fromCourse.Code,
                                   FromNameEn = fromCourse.NameEn,
                                   FromCredit = fromCourse.Credit,
                                   FromLecture = fromCourse.Lecture,
                                   FromLab = fromCourse.Lab,
                                   FromOther = fromCourse.Other,
                                   FromStar = log.FromStarCourse,
                                   FromTransfer = log.FromTransferCourse,
       
                                   ToCourseRateId = toCourse.CourseRateId,
                                   ToCode = toCourse.Code,
                                   ToNameEn = toCourse.NameEn,
                                   ToCredit = toCourse.Credit,
                                   ToLecture = toCourse.Lecture,
                                   ToLab = toCourse.Lab,
                                   ToOther = toCourse.Other,
                                   ToStar = log.ToStarCourse,
                                   ToTransfer = log.ToTransferCourse
                               }).ToList();

            var results = logs.Select(x => new RegistrationChangeCourseLogViewModel
                                           {
                                               Previous = new ModifyRegistrationCourse
                                                          {
                                                              CourseRateId = x.FromCourseRateId,
                                                              Code = x.FromCode,
                                                              NameEn = x.FromNameEn,
                                                              Credit = x.FromCredit,
                                                              Lecture = x.FromLecture,
                                                              Lab = x.FromLab,
                                                              Other = x.FromOther,
                                                              IsStarCourse = x.FromStar,
                                                              IsTransfer = x.FromTransfer
                                                          },
                                               Changed = new ModifyRegistrationCourse
                                                         {
                                                             CourseRateId = x.ToCourseRateId,
                                                             Code = x.ToCode,
                                                             NameEn = x.ToNameEn,
                                                             Credit = x.ToCredit,
                                                             Lecture = x.ToLecture,
                                                             Lab = x.ToLab,
                                                             Other = x.ToOther,
                                                             IsStarCourse = x.ToStar,
                                                             IsTransfer = x.ToTransfer
                                                         }
                                           })
                              .ToList();
            return results;
        }

        public ActionResult Delete(long id, string returnUrl)
        {
            var model = _db.RegistrationCourses.SingleOrDefault(x => x.Id == id
                                                                     && x.IsTransferCourse);
            if (model != null)
            {
                try
                {
                    model.Status = "d";
                    model.IsActive = false;
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToDelete);
                }
            }
            else
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return Redirect(returnUrl);
        }

        private void CreateSelectList()
        {
            ViewBag.Courses = _selectListProvider.GetCourses(true);
        }
    }
}