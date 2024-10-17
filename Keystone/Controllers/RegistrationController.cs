using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Api;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Net;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("Registration", "")]
    public class RegistrationController : BaseController
    {
        protected readonly IUserProvider _userProvider;
        protected readonly IStudentProvider _studentProvider;
        protected readonly IScheduleProvider _scheduleProvider;
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IReceiptProvider _receiptProvider;
        protected readonly IScholarshipProvider _scholarshipProvider;
        protected readonly IPrerequisiteProvider _prerequisiteProvider;
        private readonly IFeeProvider _feeProvider;
        protected readonly IRegistrationCalculationProvider _registrationCalculationProvider;
        protected readonly UserManager<ApplicationUser> _userManager;
        
        public RegistrationController(ApplicationDbContext db,
                                      IFlashMessage flashMessage,
                                      IMapper mapper,
                                      IMemoryCache memoryCache,
                                      IUserProvider userProvider,
                                      IStudentProvider studentProvider,
                                      IScheduleProvider scheduleProvider,
                                      ICacheProvider cacheProvider,
                                      IRegistrationProvider registrationProvider,
                                      IReceiptProvider receiptProvider,
                                      IScholarshipProvider scholarshipProvider,
                                      ISelectListProvider selectListProvider,
                                      IPrerequisiteProvider prerequisiteProvider,
                                      UserManager<ApplicationUser> user,
                                      IFeeProvider feeProvider,
                                      IRegistrationCalculationProvider registrationCalculationProvider) : base(db, flashMessage, mapper, memoryCache, selectListProvider)
        {
            _userProvider = userProvider;
            _studentProvider = studentProvider;
            _scheduleProvider = scheduleProvider;
            _cacheProvider = cacheProvider;
            _registrationProvider = registrationProvider;
            _receiptProvider = receiptProvider;
            _scholarshipProvider = scholarshipProvider;
            _prerequisiteProvider = prerequisiteProvider;
            _userManager = user;
            _feeProvider = feeProvider;
            _registrationCalculationProvider = registrationCalculationProvider;
        }

        public async Task<ActionResult> Index(string code, long academicLevelId, long termId, string tabIndex, string returnUrl)
        {
            CreateSelectList(academicLevelId, termId);
            RegistrationViewModel model = new RegistrationViewModel();
            ViewBag.ReturnUrl = returnUrl;
            if (string.IsNullOrEmpty(code) || academicLevelId == 0 || termId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }
            
            var student = _studentProvider.GetStudentInformationByCode(code);

            if (student != null)
            {
                try
                {
                    var userId = GetUser()?.Id ?? "From KS";
                    await _registrationProvider.GetRegistrationCourseFromUspark(student.Id, termId, userId);
                }
                catch
                {
                    _flashMessage.Warning("Problem retreive data...");
                    return View(model);
                }

                student.RegistrationTermId = termId;
                model = _mapper.Map<Student, RegistrationViewModel>(student);
                model.Code = code;
                model.AcademicLevelId = academicLevelId;
                model.RegistrationTermId = termId;
                model.Receipts = _receiptProvider.GetReceiptByTerm(student.Id, termId);
                model.Invoices = _receiptProvider.GetInvoiceByTerm(student.Id, termId);
                _userProvider.FillUserTimeStampFullName(model.Invoices.ToList<UserTimeStamp>());
                model.Registrations = _registrationProvider.GetActiveRegistrationCourses(student.Id, termId);
                var sectionIds = model.Registrations.Where(x => x.SectionId != null)
                                                    .Select(x => x.Section.Id)
                                                    .ToList();

                model.Schedules = _scheduleProvider.GetSchedule(sectionIds);
                model.RegistrationScheduleJsonData = JsonConvert.SerializeObject(model.Schedules);
                model.AddingResults = model.Registrations.Select(x => _mapper.Map<RegistrationCourse, AddingViewModel>(x))
                                                         .ToList();

                var scholarships = _scholarshipProvider.GetScholarshipStudents(student.Id)
                                                       .Select(x => x.Scholarship.NameEn);
                model.ScholarshipProfile = scholarships.Any() ? string.Join(", ", scholarships) : null;

                foreach(var item in model.AddingResults) 
                {
                    item.Sections = _registrationProvider.GetSectionByCourseId(termId, item.CourseId);
                }
                
                model.TotalCredit = model.Registrations.Sum(x => x.Course.RegistrationCredit); 
                model.TermId = termId;
                model.TermText = _db.Terms.Where(x => x.Id == termId)
                                          .Select(x => x.TermText)
                                          .SingleOrDefault();

                var registrationSlots = _registrationProvider.GetRegistrationSlots(student, termId);
                model.RegistrationSlot = string.Join(", ", registrationSlots.Select(x => x.Description));
                model.StudentStatus = student.StudentStatusText;

                // INCIDENT 
                var incidents = _db.StudentIncidents.AsNoTracking()
                                                    .Where(x => x.StudentId == student.Id
                                                                && x.IsActive)
                                                    .ToList();

                model.IsAllowRegistration = !incidents.Any(x => x.LockedRegistration);
                model.IsAllowPayment = !incidents.Any(x => x.LockedPayment);
                model.IsAllowSignIn = !incidents.Any(x => x.LockedSignIn);

                // ADVISING
                model.IsAdvised = _db.AdvisingLogs.AsNoTracking()
                                                  .Any(x => x.StudentId == student.Id
                                                            && x.TermId == termId);

                model.AccumulativeCredit = _registrationProvider.GetAccumulativeCredit(student.Id);
                model.AccumulativeRegistrationCredit = _registrationProvider.GetAccumulativeRegistrationCredit(student.Id);
                model.Registrations.Where(x => x.Section != null)
                                   .Select(x => {
                                                    x.Section.SectionDetails = x.Section.SectionDetails.OrderBy(y => y.Day)
                                                                                                           .ThenBy(y => y.StartTime)
                                                                                                       .ToList();
                                                    return x;
                                                })
                                   .ToList();

                model.CreditLoadInformation = new CreditLoadInformationViewModel
                                              {
                                                  StudentId = student.Id,
                                                  MinimumCredit = student.AcademicInformation.MinimumCredit ?? 0,
                                                  MaximumCredit = student.AcademicInformation.MaximumCredit ?? 0,
                                                  ApprovedAt = DateTime.Now,
                                                  Code = code,
                                                  AcademicLevelId = academicLevelId,
                                                  TermId = termId,
                                                  TabIndex = tabIndex,
                                                  ReturnUrl = returnUrl
                                              };

                model.StudentRegistrationCourseViewModels = _studentProvider.GetStudentRegistrationCourseViewModel(student.Id, null);
                model.StudentRegistrationCoursesViewModels.TransferCourse = _studentProvider.GetStudentRegistrationCourseTranferViewModel(student.Id, null);
                model.StudentRegistrationCoursesViewModels.TransferCourseWithGrade = _studentProvider.GetStudentRegistrationCourseTranferWithGradeViewModel(student.Id, null);
                model.StudentRegistrationCoursesViewModels.TranscriptGrade = model.StudentRegistrationCourseViewModels; 

                CreateSelectList(academicLevelId, termId);
            }
            else
            {
                _flashMessage.Danger(Message.StudentNotFound);
            }

            return View(model);
        }

        [PermissionAuthorize("Registration", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromBody] RegistrationViewModel model)
        {
            var newResponse = new ApiResponse<long?>
            {
                StatusCode = HttpStatusCode.OK
            };

            var student = _db.Students.AsNoTracking().FirstOrDefault(x => x.Id == model.StudentId);
            if (student == null)
            {
                _flashMessage.Danger(Message.DataNotFound);
                return Ok(newResponse); // refactor later
            }

            //As of 9 May 2022. Ms.Anothai say registra office must be able to do this
            //var notAllowPaymentForStudentStatusConfig = _db.Configurations.AsNoTracking()
            //                                                        .SingleOrDefault(x => x.Key == "ExcludedRegistrationStatus");
            //var blockStudentStatus = notAllowPaymentForStudentStatusConfig == null
            //                         || string.IsNullOrEmpty(notAllowPaymentForStudentStatusConfig.Value)
            //                                ? Enumerable.Empty<string>()
            //                                : notAllowPaymentForStudentStatusConfig.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
            //                                                                             .Select(x => x.Trim())
            //                                                                             .ToList();
            //if (blockStudentStatus.Any() && blockStudentStatus.Contains(student.StudentStatus, StringComparer.InvariantCultureIgnoreCase))
            //{
            //    _flashMessage.Danger($"Current student status ({student.StudentStatus}) is not allow to perform fee calculation.");
            //    return Ok(newResponse);
            //}

            _registrationProvider.UpdateStudentStateToAdd(model.StudentId, model.RegistrationTermId);
            string round = _registrationProvider.GetStudentState(model.StudentId, model.RegistrationTermId);
            
            

            if (model.AddingResults.GroupBy(x => x.CourseId).Any(x => x.Count() > 1))
            {
                _flashMessage.Danger(Message.DuplicateCourses);
                return Ok(newResponse); // refactor later
            }

            var sectionIds = model.AddingResults.Select(x => (long?)x.SectionId).ToList();

            var jointSections = _db.Sections.Where(x => sectionIds.Contains(x.Id)
                                                        && x.ParentSectionId != null)
                                            .ToList();
                                            
            var parentSectionIds = _db.Sections.Where(x => sectionIds.Contains(x.Id)
                                                        && x.ParentSectionId == null)
                                               .Select(x => (long?)x.Id)
                                               .ToList();

            if (jointSections.Any(x => parentSectionIds.Contains(x.ParentSectionId)))
            {
                _flashMessage.Danger(Message.SectionCourseDuplicateJoinCourses);
                return Ok(newResponse); // refactor later
            }

            var user = await _userManager.GetUserAsync(User);

            var updateSectionIds = sectionIds == null ? Enumerable.Empty<long>()
                                                      : sectionIds.Where(x => x.HasValue).Select(x => x.Value).ToList();

            try
            {
                _registrationProvider.CallUSparkAPIUpdatePreregistrations(model.StudentId, user.Id, model.RegistrationTermId, updateSectionIds);
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch (Exception e)
            {
                _flashMessage.Danger(Message.UnableToModifyRegistraion + " " + e.Message);
            }
            
            return Ok(newResponse);
        }

        [PermissionAuthorize("Registration", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(string code, long academicLevelId, long termId)
        {
            var result = await _registrationCalculationProvider.ConfirmInvoice(code, academicLevelId, termId, GetUser()?.Id, true, true);

            if (!result.IsSuccess)
            {
                _flashMessage.Warning(result.Result);
                return RedirectToAction("Index", "Registration", new
                {
                    AcademicLevelId = academicLevelId,
                    Code = code,
                    TermId = termId,
                    tabIndex = 2
                });
            }
            _flashMessage.Confirmation(result.Result);
            // Redirect TO INDEX
            return RedirectToAction("Index", "Registration", new
            {
                AcademicLevelId = academicLevelId,
                Code = code,
                TermId = termId,
                tabIndex = 2
            });

        }


        public string CheckScheduleConflicts(RegistrationViewModel model)
        {
            var sectionIds = model.AddingResults.Select(x => x.SectionId)
                                                .ToList();
            string scheduleConflictMessage = _scheduleProvider.GetClassConflictMessage(sectionIds);
            return JsonConvert.SerializeObject(new 
                                               {
                                                   Message = scheduleConflictMessage
                                               });
        }

        public string CheckCredit(RegistrationViewModel model)
        {
            string creditMessage = _registrationProvider.GetCreditMessage(model);

            return JsonConvert.SerializeObject(new 
                                               {
                                                   Message = creditMessage
                                               });
        }

        public string CheckExamConflicts(RegistrationViewModel model)
        {
            var sectionIds = model.AddingResults.Select(x => x.SectionId)
                                                .ToList();
            string examConflictMessage = _scheduleProvider.GetExamConflictMessage(sectionIds);
            
            return JsonConvert.SerializeObject(new 
                                               {
                                                   Message = examConflictMessage
                                               });
        }

        public string CheckPrerequisite(RegistrationViewModel model)
        {
            System.Text.StringBuilder prerequisiteMessage = new System.Text.StringBuilder();
            var student = _prerequisiteProvider.GetStudentInfoForPrerequisite(model.StudentId);
            foreach (var courseId in model.AddingResults.Select(x => x.CourseId)
                                                        .Distinct())
            {
                string prerequisite = string.Empty;
                if (!_prerequisiteProvider.CheckPrerequisite(student, courseId, student.AcademicInformation.CurriculumVersionId ?? 0, out prerequisite))
                {
                    var course = _db.Courses.SingleOrDefault(x => x.Id == courseId);
                    prerequisiteMessage.Append($"{course.CodeAndName} requires {prerequisite}\n");
                }
            }

            return JsonConvert.SerializeObject(new 
                                               {
                                                   Message = prerequisiteMessage.ToString()
                                               });
        }

        public void Generate() 
        {
            var orConditions = _db.OrConditions.ToList();
            foreach (var item in orConditions)
            {
                var description = _prerequisiteProvider.GetConditionDescription("or", item.Id, false);
                item.Description = description;
                _db.SaveChanges();
            }

            var andConditions = _db.AndConditions.ToList();
            foreach (var item in andConditions)
            {
                var description = _prerequisiteProvider.GetConditionDescription("and", item.Id, false);
                item.Description = description;
            }
            _db.SaveChanges();
        }

        public string CheckCourseCorequisite(RegistrationViewModel model)
        {
            System.Text.StringBuilder corerequisiteMessage = new System.Text.StringBuilder();
            var student = _prerequisiteProvider.GetStudentInfoForPrerequisite(model.StudentId);
            foreach (var courseId in model.AddingResults.Select(x => x.CourseId)
                                                        .Distinct())
            {
                var course = _prerequisiteProvider.CheckCourseCorequisite(student, courseId);
                if (course != null)
                {
                    corerequisiteMessage.Append($"{course.Code}\n");
                }
            }

            return JsonConvert.SerializeObject(new 
                                               {
                                                   Message = corerequisiteMessage.ToString()
                                               });
        }

        public string ShowSchedule(RegistrationViewModel model)
        {
            var sectionIds = model.AddingResults == null ? new List<long>() : model.AddingResults.Where(x => !x.RefundItems.Any())
                                                                                                 .Select(x => x.SectionId)
                                                                                                 .ToList();

            var schedule = sectionIds.Any() ? JsonConvert.SerializeObject(_scheduleProvider.GetSchedule(sectionIds)) : "";
            return schedule;
        }

        public ActionResult ReturnSeat(Guid studentId, long termId, string channel)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    List<RegistrationCourse> courses = _studentProvider.GetRegistrationCourseByStudentId(studentId, termId);
                    _db.RegistrationCourses.RemoveRange(courses);

                    List<RegistrationCourse> results = _registrationProvider.GetActiveRegistrationCourses(studentId, termId);
                    results.Select(x => {
                                            x.Status = "d";
                                            _registrationProvider.ReturnSeat(x.Section, 1);
                                            return x;
                                        }).ToList();

                    _db.RegistrationLogs.Add(new RegistrationLog()
                                             {
                                                 Channel = channel,
                                                 StudentId = studentId,
                                                 TermId = termId,
                                                 Modifications = "Return All Seat",
                                                 RegistrationCourses = String.Join(", ", results.Select(x => x.CourseAndNumber)),
                                                 Type = "r",
                                                 Round = 0,
                                                 CreatedAt = DateTime.UtcNow,
                                                 CreatedBy = "km."
                                             });

                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToDelete);
                    transaction.Rollback();
                }
            }

            var code = _studentProvider.GetStudentById(studentId).Code;
            return RedirectToAction(nameof(Index), new { code = code, tabIndex = "0" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetAvailableSections(long termId, string courseCode, string courseName, string sectionNumber)
        {
            var sections = _registrationProvider.GetAvailableSections(courseCode, courseName, sectionNumber, termId);
            return PartialView("~/Views/Shared/_SelectCourses.cshtml", sections);
        }

        public Course GetCreditAndRegistrationCredit(long courseId)
        {
            var course = _db.Courses.Where(x => x.Id == courseId)
                                    .SingleOrDefault();
            return course;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public string GetMainInstructorAndCheckSeatAvailableBySectionId(long sectionId)
        {
            var result = _db.Sections.Where(x => x.Id == sectionId)
                                     .Select(x => new 
                                                  {
                                                      MainInstructorFullNameEn = x.MainInstructor.Title.NameEn + " " + x.MainInstructor.FirstNameEn + " " + x.MainInstructor.LastNameEn,
                                                      Message = x.SeatAvailable <= 0 ? x.Course.Code + ", Section Number " + x.Number : null
                                                  }) 
                                     .First();

            return JsonConvert.SerializeObject(new 
                                               {
                                                   MainInstructorFullNameEn = result.MainInstructorFullNameEn,
                                                   Message = result.Message
                                               });
        }

        [PermissionAuthorize("Registration", PolicyGenerator.Write)]
        public async Task<ActionResult> UpdateCreditLoad(CreditLoadInformationViewModel model)
        {
            var academicInfo = _db.AcademicInformations.Include(x => x.Student)
                                                       .FirstOrDefault(x => x.Student.Code == model.Code);
            if (academicInfo != null)
            {
                academicInfo.MaximumCredit = model.MaximumCredit;
                academicInfo.MinimumCredit = model.MinimumCredit;

                _db.SaveChanges();

                await _registrationProvider.UpdateCreditUspark(academicInfo.Student.Code,  model.MaximumCredit, model.MinimumCredit);
            }

            return RedirectToAction("Index", "Registration", new
                                                             {
                                                                 Code = model.Code,
                                                                 AcademicLevelId = model.AcademicLevelId,
                                                                 TermId = model.TermId,
                                                                 TabIndex = model.TabIndex,
                                                                 ReturnUrl = model.ReturnUrl
                                                             });
        }

        private void CreateSelectList(long academicLevelId, long termId) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.RefundPercentages = _selectListProvider.GetPercentages();
            ViewBag.OpenCourses = _registrationProvider.GetOpenedCourses(termId);
            ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);
            ViewBag.Signatories = _selectListProvider.GetSignatories();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}