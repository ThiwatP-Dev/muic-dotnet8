using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Withdrawals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("Withdrawal", "")]
    public class WithdrawalController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly IWithdrawalProvider _withdrawProvider;

        public WithdrawalController(ApplicationDbContext db,
                                    IFlashMessage flashMessage,
                                    IMapper mapper,
                                    IStudentProvider studentProvider,
                                    IWithdrawalProvider withdrawProvider,
                                    ISelectListProvider selectListProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _studentProvider = studentProvider;
            _withdrawProvider = withdrawProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.FacultyId, criteria.CourseId);
            if (criteria.AcademicLevelId > 0 && criteria.TermId > 0)
            {
                var model = GetWithdrawal(criteria).GetPaged(criteria, page);

                return View(model);
            }

            return View();
        }

        public IActionResult Logs(long id)
        {
            var logs = from log in _db.WithdrawalLogs
                       where log.WithdrawalId == id
                       select log;

            foreach (var item in logs)
            {
                if (Guid.TryParse(item.CreatedBy, out Guid createdBy))
                {
                    if (_db.Users.Any(x => x.Id == createdBy.ToString()))
                    {
                        item.CreatedByFullNameEn = _db.Users.Single(x => x.Id == createdBy.ToString()).UserName;
                    }
                    else if (_db.Students.Any(x => x.Id == createdBy))
                    {
                        item.CreatedByFullNameEn = _db.Students.Include(x => x.Title)   
                                                               .Single(x => x.Id == createdBy).FullNameEn;
                    }
                }
                else if (item.CreatedBy?.Contains("instructorId") ?? false)
                {
                    try
                    {
                        if (Int64.TryParse(item.CreatedBy.Split(":")[1], out long instructorId)
                            && _db.Instructors.Any(x => x.Id == instructorId))
                        {
                            item.CreatedByFullNameEn = _db.Instructors.Include(x => x.Title)
                                                                      .Single(x => x.Id == instructorId).FullNameEn;
                        }
                    }
                    catch { }
                }
            }

            return PartialView("~/Views/Withdrawal/_Details.cshtml", logs);
        }

        private IOrderedQueryable<WithdrawalSearchResultViewModel> GetWithdrawal(Criteria criteria)
        {
            return (from withdrawal in _db.Withdrawals
                    join registrationCourse in _db.RegistrationCourses on withdrawal.RegistrationCourseId equals registrationCourse.Id
                    join academicInformation in _db.AcademicInformations on withdrawal.StudentId equals academicInformation.StudentId
                    join major in _db.Departments on academicInformation.DepartmentId equals major.Id
                    join course in _db.Courses on registrationCourse.CourseId equals course.Id
                    join section in _db.Sections on registrationCourse.SectionId equals section.Id into sections
                    from section in sections.DefaultIfEmpty()
                    join user in _db.Users on withdrawal.UpdatedBy equals user.Id into users
                    from user in users.DefaultIfEmpty()
                    where (criteria.TermId == 0
                           || registrationCourse.TermId == criteria.TermId)
                          && (criteria.AcademicLevelId == 0
                              || academicInformation.AcademicLevelId == criteria.AcademicLevelId)
                          && (criteria.FacultyId == 0
                              || academicInformation.FacultyId == criteria.FacultyId)
                          && (criteria.DepartmentId == 0
                              || academicInformation.DepartmentId == criteria.DepartmentId)
                          && (criteria.CourseId == 0
                              || course.Id == criteria.CourseId)
                          && (criteria.SectionId == 0
                              || section.Id == criteria.SectionId)
                          && (string.IsNullOrEmpty(criteria.CodeAndName)
                              || withdrawal.Student.Code.Contains(criteria.CodeAndName)
                              || withdrawal.Student.FirstNameEn.Contains(criteria.CodeAndName)
                              || withdrawal.Student.LastNameEn.Contains(criteria.CodeAndName))
                          && (string.IsNullOrEmpty(criteria.Status)
                              || withdrawal.Status == criteria.Status)
                          && (criteria.InstructorId == 0
                              || section.MainInstructorId == criteria.InstructorId)
                    select new WithdrawalSearchResultViewModel
                    {
                        Id = withdrawal.Id,
                        Code = withdrawal.Student.Code,
                        Name = (withdrawal.Student.Title != null ? withdrawal.Student.Title.NameEn + " " : "") + withdrawal.Student.FullNameEn,
                        Major = major.Abbreviation,
                        Course = course.CodeAndName,
                        Credit = course.CreditText,
                        Instructor = (section.MainInstructor != null ? (section.MainInstructor.Title != null ? section.MainInstructor.Title.NameEn + " " : "") + section.MainInstructor.FullNameEn 
                                                                     : ""),
                        Approver = (withdrawal.Instructor != null ? (withdrawal.Instructor.Title != null ? withdrawal.Instructor.Title.NameEn + " " : "") + withdrawal.Instructor.FullNameEn
                                                                  : (user != null ? user.UserName : "")),
                        Section = section.Number,
                        Type = withdrawal.Type,
                        TypeText = withdrawal.TypeText,
                        Status = withdrawal.Status,
                        StatusText = withdrawal.StatusText,
                        ApprovedDate = withdrawal.Status == "a" ? withdrawal.UpdatedAt.AddHours(7).ToString(StringFormat.ShortDateTime) : "",
                        Remark = withdrawal.Remark,
                        RequestedDate = withdrawal.RequestedAt.AddHours(7).ToString(StringFormat.ShortDateTime),
                    }).OrderByDescending(x => x.RequestedDate);
        }

        public IActionResult ExportExcel(Criteria criteria, string returnUrl)
        {
            List<WithdrawalSearchResultViewModel> list = new List<WithdrawalSearchResultViewModel>();
            if (criteria.AcademicLevelId > 0 && criteria.TermId > 0)
            {
                list = GetWithdrawal(criteria).ToList();
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(list);
        }

        [PermissionAuthorize("Withdrawal", PolicyGenerator.Write)]
        public IActionResult Create(WithdrawalViewModel model)
        {
            CreateSelectList(model.AcademicLevelId, model.TermId, 0, model.CourseId);
            ViewBag.ReturnUrl = model.ReturnUrl;
            if (model.CourseId != 0 && model.SectionId != 0)
            {
                model.WithdrawByCourse = GetRegistrationCourseByCourse(model.CourseId, model.SectionId);
            }
            else if (!string.IsNullOrEmpty(model.StudentCode))
            {
                if (!_studentProvider.IsExistStudent(model.StudentCode))
                {
                    _flashMessage.Danger(Message.StudentNotFound);
                    return View(model);
                }
                else
                {
                    model.WithdrawByStudent = GetRegistrationCourseByStudent(model.StudentCode, model.TermId);
                }
            }

            return View(model);
        }

        [PermissionAuthorize("Withdrawal", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveWithdrawalByCourse(WithdrawByCourseViewModel model, string returnUrl)
        {
            if (string.IsNullOrEmpty(model.Type) || (model.ApprovedBy == null && model.Type != "u") || model.RequestedAt == null
                || model.StudentWithdrawals == null || !model.StudentWithdrawals.Any())
            {
                _flashMessage.Warning(Message.RequiredData);
                return RedirectToAction(nameof(Create), new
                {
                    AcademicLevelId = model.AcademicLevelId,
                    TermId = model.TermId,
                    CourseId = model.CourseId,
                    SectionId = model.SectionId,
                    ReturnUrl = returnUrl,
                    tabIndex = "0"
                });
            }


            var withdrawalGrade = _db.Grades.SingleOrDefault(x => x.Name.ToUpper() == "W");
            if (withdrawalGrade == null)
            {
                _flashMessage.Warning(Message.UnableToSave + " - Cannot find withdraw grade in the system");
                return RedirectToAction(nameof(Create), new
                {
                    AcademicLevelId = model.AcademicLevelId,
                    TermId = model.TermId,
                    CourseId = model.CourseId,
                    SectionId = model.SectionId,
                    ReturnUrl = returnUrl,
                    tabIndex = "0"
                });
            }

            if (!_withdrawProvider.IsInWithdrawalPeriod(model.AcademicLevelId, model.Type, model.RequestedAt))
            {
                _flashMessage.Warning(Message.WithdrawOutsidePeriod);
            }
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var registrationCourseIds = model.StudentWithdrawals.Select(x => x.RegistrationCourseId)
                                                                        .ToList();
                    var withdrawals = _withdrawProvider.GetWithdrawalsByRegistrationCourses(registrationCourseIds, model.Type);

                    foreach (var item in model.StudentWithdrawals)
                    {
                        var withdrawal = withdrawals.FirstOrDefault(x => x.RegistrationCourseId == item.RegistrationCourseId);
                        if (withdrawal == null)
                        {
                            item.RequestedAt = model.RequestedAt;
                            item.Type = model.Type;
                            item.SignatoryId = model.Type == "p" ? model.ApprovedBy : null;
                            item.InstructorId = model.Type == "d" ? model.ApprovedBy : (model.Type == "u" ? GetUser().InstructorId : null);
                            item.Status = "a";
                            _db.Withdrawals.Add(item);

                            _db.WithdrawalLogs.Add(new WithdrawalLog
                            {
                                Status = "a",
                                WithdrawalId = item.Id
                            });
                        }
                    }

                    var registrationCourses = _db.RegistrationCourses.Include(x => x.Grade)
                                                                        .Where(x => registrationCourseIds.Contains(x.Id))
                                                                        .ToList();
                    if (registrationCourses.Any(x => x.IsGradePublished)) 
                    {
                        throw new Exception("Some Grade alread published");
                    }
                    registrationCourses.Select(x =>
                    {
                        x.GradeId = withdrawalGrade.Id;
                        x.GradeName = withdrawalGrade.Name;
                        x.IsGradePublished = false;
                        return x;
                    })
                                        .ToList();

                    foreach (var registrationCourse in registrationCourses)
                    {
                        var gradeLog = new GradingLog
                        {
                            RegistrationCourseId = registrationCourse.Id,
                            PreviousGrade = registrationCourse.Grade?.Name ?? "",
                            CurrentGrade = withdrawalGrade?.Name ?? "",
                            Type = "w"
                        };
                        _db.GradingLogs.Add(gradeLog);
                    }
                    _db.SaveChanges();

                    foreach (var registrationCourse in registrationCourses)
                    {
                        _studentProvider.UpdateTermGrade(registrationCourse.StudentId, registrationCourse.TermId);
                        _studentProvider.UpdateCGPA(registrationCourse.StudentId);
                    }
                        
                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new Criteria
                    {
                        AcademicLevelId = model.AcademicLevelId,
                        TermId = model.TermId,
                        CourseId = model.CourseId,
                        SectionId = model.SectionId
                    });
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToSave);
                }
            }
           
            return RedirectToAction(nameof(Create), new
            {
                AcademicLevelId = model.AcademicLevelId,
                TermId = model.TermId,
                CourseId = model.CourseId,
                SectionId = model.SectionId,
                ReturnUrl = returnUrl,
                tabIndex = "0"
            });
        }

        [PermissionAuthorize("Withdrawal", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveWithdrawalByStudent(WithdrawByStudentViewModel model, string returnUrl)
        {
            if (string.IsNullOrEmpty(model.Type) || (model.ApprovedBy == null && model.Type != "u") || model.RequestedAt == null
                || model.CourseIds == null || !model.CourseIds.Any())
            {
                _flashMessage.Warning(Message.RequiredData);
                return RedirectToAction(nameof(Create), new
                {
                    AcademicLevelId = model.AcademicLevelId,
                    TermId = model.TermId,
                    StudentCode = model.StudentCode,
                    ReturnUrl = returnUrl,
                    tabIndex = "1"
                });
            }

            var withdrawalGrade = _db.Grades.SingleOrDefault(x => x.Name.ToUpper() == "W");
            if (withdrawalGrade == null)
            {
                _flashMessage.Warning(Message.UnableToSave + " - Cannot find withdraw grade in the system");
                return RedirectToAction(nameof(Create), new
                {
                    AcademicLevelId = model.AcademicLevelId,
                    TermId = model.TermId,
                    StudentCode = model.StudentCode,
                    ReturnUrl = returnUrl,
                    tabIndex = "1"
                });
            }

            if (!_withdrawProvider.IsInWithdrawalPeriod(model.AcademicLevelId, model.Type, model.RequestedAt))
            {
                _flashMessage.Warning(Message.WithdrawOutsidePeriod);
            }
            var registrationCourses = _db.RegistrationCourses.Include(x => x.Student)
                                                                 .Include(x => x.Grade)
                                                                 .Where(x => model.CourseIds.Contains(x.Id))
                                                                 .ToList();

            Guid studentId = Guid.Empty;
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var registrationCourseIds = registrationCourses.Select(x => x.Id)
                                                                    .ToList();
                    var withdrawals = _withdrawProvider.GetWithdrawalsByRegistrationCourses(registrationCourseIds, model.Type);

                    foreach (var item in registrationCourses)
                    {
                        if (item.IsGradePublished)
                        {
                            throw new Exception("Grade alread publish");
                        }
                        studentId = item.StudentId;
                        if (!withdrawals.Any(x => x.RegistrationCourseId == item.Id))
                        {
                            Withdrawal withdrawal = new Withdrawal
                            {
                                StudentId = studentId,
                                RegistrationCourseId = item.Id,
                                Type = model.Type,
                                RequestedAt = model.RequestedAt,
                                SignatoryId = model.Type == "p" ? model.ApprovedBy : null,
                                InstructorId = model.Type == "d" ? model.ApprovedBy : (model.Type == "u" ? GetUser().InstructorId : null),
                                Status = "a",
                            };
                            _db.Withdrawals.Add(withdrawal);
                            _db.SaveChanges();

                            _db.WithdrawalLogs.Add(new WithdrawalLog
                            {
                                Status = "a",
                                WithdrawalId = withdrawal.Id
                            });
                            _db.SaveChanges();
                        }

                        item.GradeId = withdrawalGrade.Id;
                        item.GradeName = withdrawalGrade.Name;
                        item.IsGradePublished = false;

                        var gradeLog = new GradingLog
                                        {
                                            RegistrationCourseId = item.Id,
                                            PreviousGrade = item.Grade?.Name ?? "",
                                            CurrentGrade = "W",
                                            Type = "w"
                                        };

                        _db.GradingLogs.Add(gradeLog);
                        _db.SaveChanges();
                    }

                    if (studentId != Guid.Empty)
                    {
                        _studentProvider.UpdateTermGrade(studentId, model.TermId);
                        _studentProvider.UpdateCGPA(studentId);
                    }

                    transaction.Commit();

                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new Criteria
                    {
                        AcademicLevelId = model.AcademicLevelId,
                        TermId = model.TermId,
                        CodeAndName = model.StudentCode
                    });
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToSave);
                }
            }
 
            CreateSelectList(model.AcademicLevelId, model.TermId);
            return RedirectToAction(nameof(Create), new
            {
                AcademicLevelId = model.AcademicLevelId,
                TermId = model.TermId,
                StudentCode = model.StudentCode,
                ReturnUrl = returnUrl,
                tabIndex = "1"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetApprovedby(string type, long sectionId, string studentCode, long termId, string page)
        {
            var approvedby = new List<ApprovedByViewModel>();
            if (type == "p")
            {
                approvedby = _db.Signatories.Select(x => new ApprovedByViewModel()
                {
                    Id = x.Id,
                    CodeAndName = x.FullNameEn
                })
                                            .ToList();
            }
            else if (type == "d")
            {
                if (page == "1")
                {
                    approvedby = _db.InstructorSections.Include(x => x.Instructor)
                                                       .Include(x => x.SectionDetail)
                                                           .ThenInclude(x => x.Section)
                                                           .ThenInclude(x => x.RegistrationCourses)
                                                           .ThenInclude(x => x.Student)
                                                       .Where(x => x.SectionDetail.Section.RegistrationCourses.Any(y => y.Student.Code == studentCode
                                                                                                                        && y.TermId == termId))
                                                       .Select(x => x.Instructor)
                                                       .Distinct()
                                                       .Select(x => new ApprovedByViewModel
                                                       {
                                                           Id = x.Id,
                                                           CodeAndName = x.CodeAndName
                                                       })
                                                       .OrderBy(x => x.CodeAndName)
                                                       .ToList();
                }
                else
                {
                    approvedby = _db.Instructors.Include(x => x.InstructorSections)
                                                    .ThenInclude(x => x.SectionDetail)
                                                .Where(x => x.InstructorSections.Any(y => y.SectionDetail.SectionId == sectionId))
                                                .Select(x => new ApprovedByViewModel
                                                {
                                                    Id = x.Id,
                                                    CodeAndName = x.CodeAndName
                                                })
                                                .OrderBy(x => x.CodeAndName)
                                                .ToList();
                }
            }


            return (Json(approvedby));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetRegistrationCourseByType(string type, long sectionId)
        {
            var registrationCourses = _db.RegistrationCourses.Include(x => x.Student)
                                                             .Include(x => x.Withdrawals)
                                                             .Where(x => x.SectionId == sectionId
                                                                         && (type == "d"
                                                                             || !x.Withdrawals.Any()
                                                                             || !x.Withdrawals.Any(y => y.Type == type)))
                                                             .OrderBy(x => x.Student.Code)
                                                             .Select(x => new
                                                             {
                                                                 Id = x.Id,
                                                                 StudentId = x.Student.Id,
                                                                 CodeAndName = x.Student.CodeAndName,
                                                                 IsPaid = x.IsPaid
                                                             })
                                                             .ToList();
            return Json(registrationCourses);
        }

        public WithdrawByCourseViewModel GetRegistrationCourseByCourse(long courseId, long sectionId)
        {
            WithdrawByCourseViewModel model = new WithdrawByCourseViewModel
            {
                RegistrationCourses = _db.RegistrationCourses.Include(x => x.Student)
                                                                                                .Include(x => x.Withdrawals)
                                                                                                .Include(x => x.Course)
                                                                                                .Where(x => x.SectionId == sectionId
                                                                                                            && x.Status != "d")
                                                                                                .OrderBy(x => x.Student.Code)
                                                                                                .ToList(),
                StudentWithdrawals = new List<Withdrawal>(),
                CourseId = courseId,
                SectionId = sectionId
            };
            return model;
        }
        public WithdrawByStudentViewModel GetRegistrationCourseByStudent(string code, long termId)
        {
            WithdrawByStudentViewModel model = new WithdrawByStudentViewModel
            {
                RegistrationCourses = _db.RegistrationCourses.Include(x => x.Student)
                                                             .Include(x => x.Withdrawals)
                                                                 .ThenInclude(x => x.Instructor)
                                                                    .ThenInclude(x => x.Title)
                                                             .Include(x => x.Course)
                                                             .Include(x => x.Section)
                                                             .Where(x => x.Student.Code == code
                                                                         && x.TermId == termId
                                                                         && x.Status != "d")
                                                             .OrderBy(x => x.Course.Code)
                                                             .ToList(),
                CourseIds = new List<long>(),
                StudentCode = code
            };

            ViewBag.NonWithdrawals = model.RegistrationCourses.Where(x => x.Withdrawals == null || x.Withdrawals.Count() == 0).ToList();
            return model;
        }

        [PermissionAuthorize("Withdrawal", PolicyGenerator.Write)]
        public ActionResult Approve(long id, string returnUrl)
        {
            var withdrawalGrade = _db.Grades.SingleOrDefault(x => x.Name.ToUpper() == "W");
            if (withdrawalGrade == null)
            {
                _flashMessage.Warning(Message.UnableToSave + " - Cannot find withdraw grade in the system");
                return new RedirectResult(returnUrl);
            }

            var model = _withdrawProvider.GetWithdrawal(id);
            if (model == null)
            {
                _flashMessage.Warning(Message.DataNotFound);
                return new RedirectResult(returnUrl);
            }
            if (model.Status != "w")
            {
                _flashMessage.Warning(Message.UnableToSave);
                return new RedirectResult(returnUrl);
            }
            try
            {
                model.Status = "a";
                model.InstructorId = GetUser().InstructorId;

                _db.WithdrawalLogs.Add(new WithdrawalLog
                {
                    Status = model.Status,
                    WithdrawalId = model.Id
                });

                var registrationCourse = _db.RegistrationCourses
                                                .Include(x => x.Grade)
                                                .SingleOrDefault(x => x.Id == model.RegistrationCourseId);

                if (registrationCourse == null)
                {
                    _flashMessage.Danger(Message.DataNotFound);
                    return new RedirectResult(returnUrl);
                }

                if (registrationCourse.IsGradePublished)
                {
                    _flashMessage.Danger(Message.UnableToCancel + " Grade is Published");
                    return new RedirectResult(returnUrl);
                }

                registrationCourse.GradeId = withdrawalGrade.Id;
                registrationCourse.GradeName = withdrawalGrade.Name;
                registrationCourse.IsGradePublished = false;

                var gradeLog = new GradingLog
                {
                    RegistrationCourseId = registrationCourse.Id,
                    PreviousGrade = registrationCourse.Grade?.Name ?? "",
                    CurrentGrade = "W",
                    Type = "w"
                };
                _db.GradingLogs.Add(gradeLog);

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCancel);
            }

            return new RedirectResult(returnUrl);
        }

        [PermissionAuthorize("Withdrawal", PolicyGenerator.Write)]
        public ActionResult Reject(long id, string returnUrl)
        {
            var model = _withdrawProvider.GetWithdrawal(id);
            if (model == null)
            {
                _flashMessage.Warning(Message.DataNotFound);
                return new RedirectResult(returnUrl);
            }
            if (model.Status != "w")
            {
                _flashMessage.Warning(Message.UnableToSave);
                return new RedirectResult(returnUrl);
            }
            try
            {
                model.Status = "r";
                model.InstructorId = GetUser().InstructorId;

                _db.WithdrawalLogs.Add(new WithdrawalLog
                {
                    Status = model.Status,
                    WithdrawalId = model.Id
                });

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCancel);
            }

            return new RedirectResult(returnUrl);
        }

        [PermissionAuthorize("Withdrawal", PolicyGenerator.Write)]
        public ActionResult Cancel(long id, string returnUrl)
        {
            var model = _withdrawProvider.GetWithdrawal(id);
            try
            {
                model.Status = "c";
                model.IsActive = false;

                _db.WithdrawalLogs.Add(new WithdrawalLog
                {
                    Status = model.Status,
                    Remark = "Cancel Withdrawal",
                    WithdrawalId = model.Id
                });


                var gradingLog = _db.GradingLogs.Include(x => x.StudentRawScore)
                                                .LastOrDefault(x => x.RegistrationCourseId == model.RegistrationCourseId
                                                                    || x.StudentRawScore.RegistrationCourseId == model.RegistrationCourseId);
                if (gradingLog != null)
                {
                    var previousGrade = _db.Grades.SingleOrDefault(x => x.Name == gradingLog.PreviousGrade);
                    var registrationCourse = _db.RegistrationCourses.SingleOrDefault(x => x.Id == model.RegistrationCourseId);
                    registrationCourse.GradeId = previousGrade?.Id ?? null;
                    registrationCourse.GradeName = previousGrade?.Name ?? "x";
                    var newGradingLog = new GradingLog
                    {
                        RegistrationCourseId = registrationCourse.Id,
                        PreviousGrade = "W",
                        CurrentGrade = previousGrade?.Name ?? "",
                        Type = "w"
                    };
                    _db.GradingLogs.Add(newGradingLog);
                }

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCancel);
            }

            return new RedirectResult(returnUrl);
        }

        private void CreateSelectList(long academicLevelId, long termId, long facultyId = 0, long courseId = 0)
        {
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);
            ViewBag.WithdrawalTypes = _selectListProvider.GetWithdrawalTypes();
            ViewBag.WithdrawalStatuses = _selectListProvider.GetWithdrawalStatuses();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }

            if (courseId != 0)
            {
                ViewBag.Sections = _selectListProvider.GetSections(termId, courseId);
                ViewBag.Instructors = _selectListProvider.GetTermInstructorsByCourseId(termId, courseId);
            }
            else
            {
                ViewBag.Instructors = _selectListProvider.GetInstructors();
            }
        }
    }
}