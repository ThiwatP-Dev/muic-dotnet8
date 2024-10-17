using Keystone.BackgroundTask;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Vereyon.Web;
using static KeystoneLibrary.Models.BatchRegistrationConfirmationJobCreationViewModel;

namespace Keystone.Controllers
{
    [PermissionAuthorize("BatchConfirmRegistration", "")]
    public class BatchConfirmRegistrationController : BaseController
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly BackgroundWorkerQueue _backgroundWorkerQueue;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRegistrationCalculationProvider _registrationCalculationProvider;
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly UserManager<ApplicationUser> _userManager;

     
        public BatchConfirmRegistrationController(ApplicationDbContext db,
                                                  ISelectListProvider selectListProvider,
                                                  IFlashMessage flashMessage,
                                                  IServiceScopeFactory serviceScopeFactory,
                                                  BackgroundWorkerQueue backgroundWorkerQueue,
                                                  IHttpContextAccessor httpContextAccessor,
                                                  IRegistrationCalculationProvider registrationCalculationProvider,
                                                  IAuthenticationProvider authenticationProvider,
                                                  UserManager<ApplicationUser> userManager) : base(db, flashMessage, selectListProvider)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _backgroundWorkerQueue = backgroundWorkerQueue;
            _httpContextAccessor = httpContextAccessor;
            _registrationCalculationProvider = registrationCalculationProvider;
            _authenticationProvider = authenticationProvider;
            _userManager = userManager;
        }

        public void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId);
            ViewBag.AdmissionTypes = _selectListProvider.GetAdmissionTypes();
        }

        public async Task<IActionResult> Index(Criteria criteria)
        {
            //var user = GetUser();
            //if (user == null)
            //{
            //    _flashMessage.Warning(Message.InvalidPermission);
            //    return View();
            //}
            //var userRoles = await _userManager.GetRolesAsync(user);
            //var roleIds = _db.Roles.AsNoTracking()
            //                       .Where(x => userRoles.Contains(x.Name))
            //                       .Select(x => x)
            //                       .ToList();
            //if (roleIds.Any(x => x.Name.Contains("Admin")))
            //{

            //}
            //else
            //{
            //    var rolesId = roleIds.Select(x => x.Id).ToList();
            //    var menu = _db.MenuPermissions.AsNoTracking()
            //                                  .Where(x => x.UserId == user.Id
            //                                            && x.Menu.TitleEn == "Batch Registration Confirmation"
            //                                        )
            //                                  .ToList();
            //    var role = _db.MenuPermissions.AsNoTracking()
            //                                  .Where(x => rolesId.Contains(x.RoleId)
            //                                            && x.Menu.TitleEn == "Batch Registration Confirmation"
            //                                        )
            //                                  .ToList();
            //    bool isReadable = (menu != null && menu.Any(x => x.IsReadable)) || (role != null && role.Any(x => x.IsReadable));
            //    if (!isReadable)
            //    {
            //        _flashMessage.Warning(Message.InvalidPermission);
            //        return View();
            //    }
            //}

            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }



            var jobLists = _db.BatchRegistrationConfirmJobs.AsNoTracking()
                                                           .Include(x => x.BatchRegistrationConfirmJobDetails)
                                                           .Where(x => x.AcademicLevelId == criteria.AcademicLevelId
                                                                        && (criteria.TermId == 0 || x.TermId == criteria.TermId)
                                                                  )
                                                           .OrderByDescending(x => x.StartProcessDateTimeUtc)
                                                           .ToList();

            FillReadableData(jobLists);

            var dataSyncInfo = _db.DataSyncLogs.AsNoTracking()
                                               .Where(x => x.SyncName == "Pregistration To Registration Course")
                                               .OrderByDescending(x => x.CreatedDateTimeUtc)
                                               .Take(3)
                                               .ToList();

            BatchRegistrationConfirmationViewModel view = new BatchRegistrationConfirmationViewModel
            {
                BatchRegistrationConfirmJobs = jobLists,
                Criteria = criteria,
                DataSyncLogs = dataSyncInfo,
                IsAbleToCreateNewJob = jobLists == null || !jobLists.Any() || !jobLists.Any(x => x.IsRunning)
            };

            return View(view);
        }

        private void FillReadableData(List<BatchRegistrationConfirmJob> jobLists)
        {
            var termIdLists = jobLists.Select(x => x.TermId).Distinct().ToList();

            var terms = _db.Terms.AsNoTracking()
                                 .IgnoreQueryFilters()
                                 .Where(x => termIdLists.Contains(x.Id))
                                 .ToList();

            var userIdLists = jobLists.Select(x => x.CreatedBy).Distinct().ToList();

            var users = _db.Users.AsNoTracking()
                                 .IgnoreQueryFilters()
                                 .Where(x => userIdLists.Contains(x.Id))
                                 .ToList();

            foreach (var job in jobLists)
            {
                job.Term = terms.FirstOrDefault(x => x.Id == job.TermId)?.TermText ?? "";
                var user = users.FirstOrDefault(x => x.Id == job.CreatedBy);
                if (user != null)
                {
                    job.CreatedByFullNameEn = string.IsNullOrEmpty(user.FirstnameEN) ? user.UserName : user.FullnameEN;
                }
                else
                {
                    job.CreatedByFullNameEn = job.CreatedBy;
                }
            }
        }

        [PermissionAuthorize("BatchConfirmRegistration", PolicyGenerator.Write)]
        public async Task<IActionResult> CreateJob(BatchRegistrationConfirmationCriteriaViewModel criteria)
        {
            BatchRegistrationConfirmationJobCreationViewModel job = new BatchRegistrationConfirmationJobCreationViewModel()
            {
                Criteria = criteria ?? new BatchRegistrationConfirmationCriteriaViewModel(),
            };

            //var user = GetUser();
            //if (user == null)
            //{
            //    _flashMessage.Warning(Message.InvalidPermission);
            //    return RedirectToAction(nameof(Index), new
            //    {
            //        criteria.AcademicLevelId,
            //        criteria.TermId,
            //    });
            //}
            //var userRoles = await _userManager.GetRolesAsync(user);
            //var roleIds = _db.Roles.AsNoTracking()
            //                       .Where(x => userRoles.Contains(x.Name))
            //                       .Select(x => x)
            //                       .ToList();
            //if (roleIds.Any(x => x.Name.Contains("Admin")))
            //{

            //}
            //else
            //{
            //    var rolesId = roleIds.Select(x => x.Id).ToList();
            //    var menu = _db.MenuPermissions.AsNoTracking()
            //                                  .Where(x => x.UserId == user.Id
            //                                            && x.Menu.TitleEn == "Batch Registration Confirmation"
            //                                        )
            //                                  .ToList();
            //    var role = _db.MenuPermissions.AsNoTracking()
            //                                  .Where(x => rolesId.Contains(x.RoleId)
            //                                            && x.Menu.TitleEn == "Batch Registration Confirmation"
            //                                        )
            //                                  .ToList();
            //    bool isReadable = (menu != null && menu.Any(x => x.IsReadable)) || (role != null && role.Any(x => x.IsReadable));
            //    bool isWriteable = (menu != null && menu.Any(x => x.IsWritable)) || (role != null && role.Any(x => x.IsWritable));
            //    if (!isReadable || !isWriteable)
            //    {
            //        _flashMessage.Warning(Message.InvalidPermission);
            //        return RedirectToAction(nameof(Index), new
            //        {
            //            criteria.AcademicLevelId,
            //            criteria.TermId,
            //        });
            //    }
            //}

            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(job);
            }

            //check if already job running? 
            if (CheckAlreadyHaveJobRunning(criteria))
            {
                _flashMessage.Warning("There is an already running job on same term. Please wait for the existing process to finish before start a new one.");
                return RedirectToAction(nameof(Index), new
                {
                    criteria.AcademicLevelId,
                    criteria.TermId,
                });
            }

            var term = _db.Terms.AsNoTracking().FirstOrDefault(x => x.Id == criteria.TermId);

            // Query student for selectable list
            var allActiveRegistrationCourseQuery = _db.RegistrationCourses.AsNoTracking()
                                                                          .Include(x => x.Student)
                                                                              .ThenInclude(x => x.Title)
                                                                          .Include(x => x.Student)
                                                                              .ThenInclude(x => x.AcademicInformation)
                                                                                  .ThenInclude(x => x.Department)
                                                                          .Include(x => x.Student)
                                                                              .ThenInclude(x => x.AcademicInformation)
                                                                                  .ThenInclude(x => x.Faculty)
                                                                          .Include(x => x.Student)
                                                                              .ThenInclude(x => x.AdmissionInformation)
                                                                                  .ThenInclude(x => x.AdmissionType)
                                                                          .Include(x => x.Student)
                                                                              .ThenInclude(x => x.ScholarshipStudents)
                                                                          .Include(x => x.Course)
                                                                          .Where(x => x.IsActive
                                                                                         && x.Status != "d"
                                                                                         && x.TermId == criteria.TermId
                                                                                         && !x.IsTransferCourse)
                                                                          .Select(x => new
                                                                          {
                                                                              AdmissionTypeId = x.Student.AdmissionInformation.AdmissionTypeId,
                                                                              AdmissionTypeName = x.Student.AdmissionInformation.AdmissionType.NameEn,
                                                                              x.Student.StudentStatus,
                                                                              ScholarshipCount = x.Student.ScholarshipStudents.Where(y => y.IsActive
                                                                                                                                            && (y.ExpiredTerm == null
                                                                                                                                                    || y.ExpiredTerm.AcademicYear > term.AcademicYear
                                                                                                                                                    || (y.ExpiredTerm.AcademicYear == term.AcademicYear && y.ExpiredTerm.AcademicTerm >= term.AcademicTerm)
                                                                                                                                                )
                                                                                                                              ).Count(),
                                                                              StudentCode = x.Student.Code,
                                                                              StudentName = x.Student.FullNameEn,
                                                                              DeprtmentId = x.Student.AcademicInformation.DepartmentId,
                                                                              DepartmentName = x.Student.AcademicInformation.Department.NameEn,
                                                                              FacultyName = x.Student.AcademicInformation.Faculty.NameEn,
                                                                              x.TermId,

                                                                              RegistrationCourseId = x.Id,
                                                                              x.Course.RegistrationCredit,
                                                                              x.StudentId
                                                                          }
                                                                          ).ToList();

            //Check more criteria 
            if (criteria.IsNotExchangeAdmissionType)
            {
                allActiveRegistrationCourseQuery = allActiveRegistrationCourseQuery.Where(x => x.AdmissionTypeName != "Exchange inbound").ToList();
            }
            if (criteria.IncludedAdmissionTypeIdList.Count > 0)
            {
                allActiveRegistrationCourseQuery = allActiveRegistrationCourseQuery.Where(x => criteria.IncludedAdmissionTypeIdList.Contains(x.AdmissionTypeId ?? 0)).ToList();
            }
            if (criteria.IsNotExchangeStatus)
            {
                allActiveRegistrationCourseQuery = allActiveRegistrationCourseQuery.Where(x => x.StudentStatus != "ex").ToList();
            }
            if (criteria.IsExcludeScholarshipStudent)
            {
                allActiveRegistrationCourseQuery = allActiveRegistrationCourseQuery.Where(x => x.ScholarshipCount == 0).ToList();
            }
            if (criteria.ExcludeStudentCodesList.Count > 0)
            {
                allActiveRegistrationCourseQuery = allActiveRegistrationCourseQuery.Where(x => !criteria.ExcludeStudentCodesList.Contains(x.StudentCode)).ToList();
            }
            if (criteria.ExcludeDepartmentIdList.Count > 0)
            {
                allActiveRegistrationCourseQuery = allActiveRegistrationCourseQuery.Where(x => !criteria.ExcludeDepartmentIdList.Contains(x.DeprtmentId ?? 0)).ToList();
            }
            if (criteria.IsOnlyUnconfirm)
            {
                var registrationCourseIdList = allActiveRegistrationCourseQuery.Select(x => (long?)x.RegistrationCourseId).ToList();

                var invoiceRegistrationCourseIdList = _db.InvoiceItems.AsNoTracking()
                                                                      .Where(x => x.RegistrationCourseId > 0
                                                                                    && registrationCourseIdList.Contains(x.RegistrationCourseId)
                                                                                    && !x.Invoice.IsCancel
                                                                                    && x.Invoice.IsActive)
                                                                      .Select(x => (x.RegistrationCourseId ?? 0))
                                                                      .Distinct()
                                                                      .ToList();

                allActiveRegistrationCourseQuery = allActiveRegistrationCourseQuery.Where(x => !invoiceRegistrationCourseIdList.Contains(x.RegistrationCourseId)).ToList();
            }

            job.BatchRegistrationConfirmJobDetailList = allActiveRegistrationCourseQuery.Select(x => new BatchRegistrationConfirmJobDetail
            {
                AcademicLevelId = criteria.AcademicLevelId,
                AdmissionTypeName = x.AdmissionTypeName,
                DepartmentName = x.DepartmentName,
                FacultyName = x.FacultyName,
                IsChecked = true,
                StudentCode = x.StudentCode,
                StudentId = x.StudentId,
                StudentFullName = x.StudentName,
                TermId = x.TermId,
                TotalRegistrationCredit = x.RegistrationCredit
            })
                                                                                        .GroupBy(x => new { x.TermId, x.AcademicLevelId, x.StudentId })
                                                                                        .Select(x => new BatchRegistrationConfirmJobDetail
                                                                                        {
                                                                                            AcademicLevelId = x.Key.AcademicLevelId,
                                                                                            AdmissionTypeName = x.FirstOrDefault()?.AdmissionTypeName ?? "",
                                                                                            DepartmentName = x.FirstOrDefault()?.DepartmentName ?? "",
                                                                                            FacultyName = x.FirstOrDefault()?.FacultyName ?? "",
                                                                                            IsChecked = true,
                                                                                            StudentCode = x.FirstOrDefault()?.StudentCode ?? "",
                                                                                            StudentId = x.Key.StudentId,
                                                                                            StudentFullName = x.FirstOrDefault()?.StudentFullName ?? "",
                                                                                            TermId = x.Key.TermId,
                                                                                            TotalRegistrationCredit = x.Sum(y => y.TotalRegistrationCredit)

                                                                                        })
                                                                                        .ToList();

            return View(job);
        }

        private bool CheckAlreadyHaveJobRunning(BatchRegistrationConfirmationCriteriaViewModel criteria)
        {
            return _db.BatchRegistrationConfirmJobs.Any(x => x.IsActive
                                                                        && x.IsRunning
                                                                        && x.AcademicLevelId == criteria.AcademicLevelId
                                                                        && x.TermId == criteria.TermId);
        }

        [PermissionAuthorize("BatchConfirmRegistration", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public async Task<IActionResult> CreateJob(BatchRegistrationConfirmationJobCreationViewModel viewModel)
        {
            var user = GetUser();

            if (user == null)
            {
                _flashMessage.Warning(Message.InvalidPermission);
                return RedirectToAction(nameof(Index), new
                {
                    viewModel.Criteria.AcademicLevelId,
                    viewModel.Criteria.TermId,
                });
            }
            //var userRoles = await _userManager.GetRolesAsync(user);
            //var roleIds = _db.Roles.AsNoTracking()
            //                       .Where(x => userRoles.Contains(x.Name))
            //                       .Select(x => x)
            //                       .ToList();
            //if (roleIds.Any(x => x.Name.Contains("Admin")))
            //{

            //}
            //else
            //{
            //    var rolesId = roleIds.Select(x => x.Id).ToList();
            //    var menu = _db.MenuPermissions.AsNoTracking()
            //                                  .Where(x => x.UserId == user.Id
            //                                            && x.Menu.TitleEn == "Batch Registration Confirmation"
            //                                        )
            //                                  .ToList();
            //    var role = _db.MenuPermissions.AsNoTracking()
            //                                  .Where(x => rolesId.Contains(x.RoleId)
            //                                            && x.Menu.TitleEn == "Batch Registration Confirmation"
            //                                        )
            //                                  .ToList();
            //    bool isReadable = (menu != null && menu.Any(x => x.IsReadable)) || (role != null && role.Any(x => x.IsReadable));
            //    bool isWriteable = (menu != null && menu.Any(x => x.IsWritable)) || (role != null && role.Any(x => x.IsWritable));
            //    if (!isReadable || !isWriteable)
            //    {
            //        _flashMessage.Warning(Message.InvalidPermission);
            //        return RedirectToAction(nameof(Index), new
            //        {
            //            viewModel.Criteria.AcademicLevelId,
            //            viewModel.Criteria.TermId,
            //        });
            //    }
            //}

            if (viewModel == null
                || viewModel.BatchRegistrationConfirmJobDetailList == null
                || viewModel.BatchRegistrationConfirmJobDetailList.Count == 0
                || viewModel.Criteria == null
                || viewModel.Criteria.AcademicLevelId == 0
                || viewModel.Criteria.TermId == 0
                || user.Id == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return RedirectToAction(nameof(CreateJob), viewModel.Criteria);
            }
            var selectedStudent = viewModel.BatchRegistrationConfirmJobDetailList.Where(x => x.IsChecked).ToList();
            if (selectedStudent == null || selectedStudent.Count == 0)
            {
                _flashMessage.Warning(Message.RequiredData + " please select at least one student");
                return RedirectToAction(nameof(CreateJob), viewModel.Criteria);
            }

            //check if already job running? 
            if (CheckAlreadyHaveJobRunning(viewModel.Criteria))
            {
                _flashMessage.Warning("There is an already running job on same term. Please wait for the existing process to finish before start a new one.");
                return RedirectToAction(nameof(Index), new
                {
                    viewModel.Criteria.AcademicLevelId,
                    viewModel.Criteria.TermId,
                });
            }

            //Create the job 
            BatchRegistrationConfirmJob job = new BatchRegistrationConfirmJob
            {
                AcademicLevelId = viewModel.Criteria.AcademicLevelId,
                TermId = viewModel.Criteria.TermId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.Id,
                CriteriaJson = JsonConvert.SerializeObject(viewModel.Criteria),
                FinishProcessDateTimeUtc = null,
                IsActive = true,
                IsCompleted = false,
                IsRunning = true,
                RunRemark = "Initiate",
                StartProcessDateTimeUtc = null,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = user.Id,

                BatchRegistrationConfirmJobDetails = selectedStudent.Select(x => new BatchRegistrationConfirmJobDetail
                {
                    AcademicLevelId = viewModel.Criteria.AcademicLevelId,
                    TermId = viewModel.Criteria.TermId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = user.Id,
                    DepartmentName = x.DepartmentName,
                    FacultyName = x.FacultyName,
                    FinishProcessDateTimeUtc = null,
                    IsActive = true,
                    FinishSyncWithUSparkDateTimeUtc = null,
                    IsChecked = true,
                    IsSuccess = false,
                    Result = "Initiate",
                    StartSyncWithUSparkDateTimeUtc = null,
                    StudentCode = x.StudentCode,
                    StudentId = x.StudentId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = user.Id,
                }).ToList(),
            };

            try
            {
                _db.BatchRegistrationConfirmJobs.Add(job);
                _db.SaveChanges();

                viewModel.BatchRegistrationConfirmJob = job;

                //Create Job 
                _backgroundWorkerQueue.QueueBackgroundWorkItem(async token =>
                {
                    await ProcessJob(viewModel);
                });

                _flashMessage.Confirmation("Job is successfully create");
                return RedirectToAction(nameof(Index), new
                {
                    viewModel.Criteria.AcademicLevelId,
                    viewModel.Criteria.TermId,
                });
            }
            catch (Exception e)
            {
                _flashMessage.Danger("Error: " + e.Message);
                return RedirectToAction(nameof(CreateJob), viewModel.Criteria);
            }

        }

        private async Task ProcessJob(BatchRegistrationConfirmationJobCreationViewModel viewModel)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var realDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var registrationCalculationProvider = scope.ServiceProvider.GetRequiredService<IRegistrationCalculationProvider>();
                    DateTime start = DateTime.UtcNow;

                    var taskViewModel = viewModel;
                    var realJobObj = realDb.BatchRegistrationConfirmJobs.Include(x => x.BatchRegistrationConfirmJobDetails)
                                                                        .FirstOrDefault(x => x.Id == taskViewModel.BatchRegistrationConfirmJob.Id);

                    if (realJobObj == null)
                    {
                        Console.WriteLine("Error");
                        return;
                    }

                    try
                    {
                        realJobObj.StartProcessDateTimeUtc = start;
                        realJobObj.RunRemark = "Start Process";
                        realDb.SaveChangesNoAutoUserIdAndTimestamps();

                        var tasks = new List<Task>();
                        // semaphore, allow to run 10 tasks in parallel
                        using (var semaphore = new SemaphoreSlim(10))
                        {
                            foreach (var student in realJobObj.BatchRegistrationConfirmJobDetails)
                            {
                                // await here until there is a room for this task
                                await semaphore.WaitAsync();
                                tasks.Add(ProcessStudent(semaphore, realDb, student, taskViewModel, registrationCalculationProvider, realJobObj.AcademicLevelId, realJobObj.TermId, realJobObj.CreatedBy));
                            }
                        }
                        // await for the rest of tasks to complete
                        await Task.WhenAll(tasks);

                        realJobObj.RunRemark = "Complete";
                        realJobObj.FinishProcessDateTimeUtc = DateTime.UtcNow;
                        realJobObj.IsCompleted = true;
                        realJobObj.IsRunning = false;
                        realDb.SaveChangesNoAutoUserIdAndTimestamps();
                    }
                    catch (Exception ex2)
                    {
                        realJobObj.RunRemark += " - Error: " + ex2.Message;
                        realJobObj.FinishProcessDateTimeUtc = DateTime.UtcNow;
                        realJobObj.IsCompleted = true;
                        realJobObj.IsRunning = false;
                        realDb.SaveChangesNoAutoUserIdAndTimestamps();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        private async Task ProcessStudent(SemaphoreSlim semaphore, ApplicationDbContext realDb,
            BatchRegistrationConfirmJobDetail student, BatchRegistrationConfirmationJobCreationViewModel taskViewModel,
            IRegistrationCalculationProvider registrationCalculationProvider, long academicLevelId, long termId, string createdBy)
        {
            try
            {
                //CheckCredit
                var registrationCourse = realDb.RegistrationCourses.AsNoTracking()
                                                                   .Include(x => x.Course)
                                                                   .Where(x => x.IsActive
                                                                   && x.Status != "d"
                                                                        && x.TermId == student.TermId
                                                                   && !x.IsTransferCourse
                                                                        && x.StudentId == student.StudentId)
                                                                   .Select(x => x)
                                                                   .ToList();
                var studentAcademicInfo = realDb.AcademicInformations.AsNoTracking()
                                                                     .FirstOrDefault(x => x.StudentId == student.StudentId);

                if (taskViewModel.Criteria.IsCheckCreditLimit)
                {
                    if (registrationCourse == null || registrationCourse.Count == 0)
                    {
                        student.FinishProcessDateTimeUtc = DateTime.UtcNow;
                        student.Result = $"No Registration Course To Check Credit";
                        realDb.SaveChangesNoAutoUserIdAndTimestamps();
                        return;
                    }
                    else if (studentAcademicInfo == null)
                    {
                        student.FinishProcessDateTimeUtc = DateTime.UtcNow;
                        student.Result = $"No Student Academic Info to check Min-Max Credit";
                        realDb.SaveChangesNoAutoUserIdAndTimestamps();
                        return;
                    }
                    var registrationCredit = registrationCourse.Sum(x => x.Course.RegistrationCredit);
                    if ((!studentAcademicInfo.MaximumCredit.HasValue || studentAcademicInfo.MaximumCredit >= registrationCredit)
                        && (!studentAcademicInfo.MinimumCredit.HasValue || studentAcademicInfo.MinimumCredit <= registrationCredit)
                        )
                    {

                    }
                    else
                    {
                        student.FinishProcessDateTimeUtc = DateTime.UtcNow;
                        student.Result = $"Credit Limit Problem : Reg Cr.: {registrationCredit} Min Cr.: {studentAcademicInfo.MinimumCredit} Max Cr.:{studentAcademicInfo.MaximumCredit}";
                        realDb.SaveChangesNoAutoUserIdAndTimestamps();
                        return;
                    }
                }
                var syncWithUSparkResult = await registrationCalculationProvider.ConfirmInvoice(student.StudentCode, academicLevelId, termId, createdBy, true, taskViewModel.Criteria.IsRecheckWithUSpark);

                if (syncWithUSparkResult != null)
                {
                    student.StartSyncWithUSparkDateTimeUtc = syncWithUSparkResult.StartSyncWithUSparkDateTimeUtc;
                    student.FinishSyncWithUSparkDateTimeUtc = syncWithUSparkResult.FinishSyncWithUSparkDateTimeUtc;
                    student.SyncWithUSparkRemark = syncWithUSparkResult.SyncWithUSparkRemark;
                    student.FinishProcessDateTimeUtc = syncWithUSparkResult.FinishProcessDateTimeUtc;
                    student.Result = syncWithUSparkResult.Result;
                    student.IsSuccess = syncWithUSparkResult.IsSuccess;
                    student.UpdatedAt = DateTime.UtcNow;
                    realDb.SaveChangesNoAutoUserIdAndTimestamps();
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        public IActionResult Detail(long id, string returnUrl)
        {
            var batchJob = _db.BatchRegistrationConfirmJobs.AsNoTracking()
                                                           .Include(x => x.BatchRegistrationConfirmJobDetails)
                                                           .FirstOrDefault(x => x.Id == id);

            if (batchJob == null)
            {
                _flashMessage.Warning(Message.DataNotFound);
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return Redirect(returnUrl);
                }
            }

            BatchRegistrationConfirmationJobCreationViewModel batchRegistrationConfirmation = new BatchRegistrationConfirmationJobCreationViewModel
            {
                BatchRegistrationConfirmJob = batchJob,
                BatchRegistrationConfirmJobDetailList = batchJob.BatchRegistrationConfirmJobDetails,
                Criteria = JsonConvert.DeserializeObject<BatchRegistrationConfirmationCriteriaViewModel>(batchJob.CriteriaJson)
            };

            FillReadableData(new List<BatchRegistrationConfirmJob> { batchJob });
            FillReadableData(batchJob.BatchRegistrationConfirmJobDetails);
            CreateSelectList(batchRegistrationConfirmation.Criteria.AcademicLevelId);

            ViewBag.ReturnUrl = returnUrl;

            return View(batchRegistrationConfirmation);
        }

        private void FillReadableData(List<BatchRegistrationConfirmJobDetail> batchRegistrationConfirmJobDetails)
        {
            var studentIdList = batchRegistrationConfirmJobDetails.Select(x => x.StudentId).Distinct().ToList();

            var studentList = _db.Students.AsNoTracking()
                                          .IgnoreQueryFilters()
                                          .Include(x => x.Title)
                                          .Include(x => x.AcademicInformation)
                                              .ThenInclude(x => x.Department)
                                          .Include(x => x.AcademicInformation)
                                              .ThenInclude(x => x.Faculty)
                                          .Include(x => x.AdmissionInformation)
                                              .ThenInclude(x => x.AdmissionType)
                                          .Where(x => studentIdList.Contains(x.Id))
                                          .Select(x => new
                                          {
                                              AdmissionTypeName = x.AdmissionInformation.AdmissionType.NameEn,
                                              x.StudentStatus,
                                              ScholarshipCount = x.ScholarshipStudents.Count,
                                              StudentCode = x.Code,
                                              StudentName = x.FullNameEn,
                                              DepartmentName = x.AcademicInformation.Department.NameEn,
                                              FacultyName = x.AcademicInformation.Faculty.NameEn,
                                              StudentId = x.Id
                                          })
                                          .ToList();

            foreach (var detail in batchRegistrationConfirmJobDetails)
            {
                var student = studentList.FirstOrDefault(x => x.StudentId == detail.StudentId);
                if (student != null)
                {
                    detail.StudentFullName = student.StudentName;
                    detail.FacultyName = student.FacultyName;
                    detail.DepartmentName = student.DepartmentName;
                    detail.AdmissionTypeName = student.AdmissionTypeName;
                }
            }

        }
    }
}
