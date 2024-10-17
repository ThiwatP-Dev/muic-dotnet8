// using AutoMapper;
// using ClosedXML.Excel;
// using ExcelDataReader;
// using KeystoneLibrary.Data;
// using KeystoneLibrary.Interfaces;
// using KeystoneLibrary.Models;
// using KeystoneLibrary.Models.Api;
// using KeystoneLibrary.Models.DataModels;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Newtonsoft.Json;
// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Net;
// using System.Threading.Tasks;
// using Vereyon.Web;


// namespace Keystone.Controllers
// {
//     public class GradeManagementController : BaseController
//     {
//         protected readonly IAcademicProvider _academicProvider;
//         protected readonly IRegistrationProvider _registrationProvider;
//         protected readonly IGradeProvider _gradeProvider;
//         protected readonly IInstructorProvider _instructorProvider;
//         private UserManager<ApplicationUser> _userManager { get; }

//         public GradeManagementController(ApplicationDbContext db,
//                                          IFlashMessage flashMessage,
//                                          IMapper mapper,
//                                          ISelectListProvider selectListProvider,
//                                          IAcademicProvider academicProvider,
//                                          IRegistrationProvider registrationProvider,
//                                          IGradeProvider gradeProvider,
//                                          IInstructorProvider instructorProvider,
//                                          UserManager<ApplicationUser> user) : base(db, flashMessage, mapper, selectListProvider)
//         {
//             _registrationProvider = registrationProvider;
//             _academicProvider = academicProvider;
//             _gradeProvider = gradeProvider;
//             _userManager = user;
//             _instructorProvider = instructorProvider;
//         }

//         public async Task<IActionResult> Index(GradeManagementViewModel model)
//         {
//             var user = await _userManager.GetUserAsync(User);// implement after login complete
//             var instructor = _db.Instructors.FirstOrDefault(x => x.Code == "sunsern");
//             ViewBag.TermId = model.TermId;
//             // Mock user
//             // model.InstructorGuid = new Guid("4288e69d-7e9f-4224-8a0d-e5a18d3e44b4");
//             CreateSelectList(model.AcademicLevelId);

//             if (model.AcademicLevelId > 0 && model.TermId > 0)
//             {
//                 ViewBag.TermId = model.TermId;
//                 var coordinatorCourses = _gradeProvider.GetCoordinatorCourse(instructor.Id, model.TermId);
//                 if (coordinatorCourses != null && coordinatorCourses.Any())
//                 {
//                     var gradingAllocations =
//                         (
//                         from grade in _db.GradingAllocations
//                         join studentScore in _db.StudentScores on
//                             new
//                             {
//                                 grade.Id,
//                                 IsSkipGrading = false
//                             }
//                             equals
//                             new
//                             {
//                                 Id = studentScore.GradingAllocationId,
//                                 studentScore.IsSkipGrading
//                             } into studentScores
//                         from studentScore in studentScores.DefaultIfEmpty()
//                         join gradingScore in _db.GradingScores on grade.Id equals gradingScore.GradingAllocationId into gradingScores
//                         from gradingScore in gradingScores.DefaultIfEmpty()
//                         join barcode in _db.Barcodes on grade.Id equals barcode.GradingAllocationId into barcodes
//                         from barcode in barcodes.DefaultIfEmpty()
//                         where grade.TermId == model.TermId
//                             && (!coordinatorCourses.Any() || grade.CourseIdsLong.All(y => coordinatorCourses.Contains(y)))
//                         group new
//                         {
//                             barcode,
//                             studentScore,
//                             gradingScore
//                         } by new
//                         {
//                             grade.CourseIds,
//                             grade.SectionIds,
//                             grade.Id,
//                             grade.LastUpdate,
//                         } into grades
//                         select new
//                         {
//                             grades.Key.CourseIds,
//                             grades.Key.SectionIds,
//                             grades.Key.Id,
//                             grades.Key.LastUpdate,
//                             IsScored = grades.Any(x => x.studentScore != null),
//                             IsGraded = grades.Any(x => x.gradingScore != null),
//                             Barcode = grades.Where(x => x.barcode != null).Select(y => new
//                             {
//                                 y.barcode.BarcodeNumber,
//                                 y.barcode.IsPublished,
//                                 y.barcode.CreatedAt
//                             }).Distinct().ToList()
//                         }
//                         ).ToList();

//                     if (gradingAllocations.Any())
//                     {
//                         var allCourseIds = new List<long>();
//                         var allSectionIds = new List<long>();
//                         foreach (var allocation in gradingAllocations)
//                         {
//                             allCourseIds.AddRange(string.IsNullOrEmpty(allocation.CourseIds)
//                                             ? new List<long>() : JsonConvert.DeserializeObject<List<long>>(allocation.CourseIds));
//                             allSectionIds.AddRange(string.IsNullOrEmpty(allocation.SectionIds)
//                                              ? new List<long>() : JsonConvert.DeserializeObject<List<long>>(allocation.SectionIds));
//                         }
//                         var courses = _db.Courses.Where(x => allCourseIds.Contains(x.Id)).Select(x => new { x.Id, x.CourseAndCredit }).ToList();
//                         var sections = _db.Sections.Where(x => allSectionIds.Contains(x.Id)).Select(x => new { x.Id, x.Number }).ToList();

//                         foreach (var allocation in gradingAllocations)
//                         {
//                             var courseIds = (string.IsNullOrEmpty(allocation.CourseIds)
//                                            ? new List<long>() : JsonConvert.DeserializeObject<List<long>>(allocation.CourseIds));
//                             var sectionIds = (string.IsNullOrEmpty(allocation.SectionIds)
//                                              ? new List<long>() : JsonConvert.DeserializeObject<List<long>>(allocation.SectionIds));
//                             if (courseIds.Any())
//                             {
//                                 var courseNames = string.Join(", ", courses.Where(x => courseIds.Contains(x.Id)).Select(x => x.CourseAndCredit));
//                                 var barcodeIds = string.Join(", ", allocation.Barcode.OrderByDescending(x => x.CreatedAt).Select(x => x.BarcodeNumber).ToList());
//                                 var sectionNumbers = string.Join(", ", sections.Where(x => sectionIds.Contains(x.Id)).OrderBy(x => x.Number).Select(x => x.Number));
//                                 model.GradingStatuses.Add(new GradingStatus
//                                 {
//                                     AllocationId = allocation.Id,
//                                     CourseIds = allocation.CourseIds,
//                                     CourseNames = courseNames,
//                                     SectionNumbers = sectionNumbers,
//                                     IsAllocated = true,
//                                     IsScored = allocation.IsScored,
//                                     IsGraded = allocation.IsGraded,
//                                     IsBarcodeGenereated = allocation.Barcode?.Any() ?? false,
//                                     Barcode = barcodeIds,
//                                     IsPublished = allocation.Barcode?.Any(x => x.IsPublished) ?? false,
//                                     LastUpdate = allocation.LastUpdate
//                                 });

//                             }

//                         }

//                         if (model.GradingStatuses != null)
//                         {
//                             model.GradingStatuses = model.GradingStatuses.OrderBy(x => x.CourseNames)
//                                                                          .ToList();
//                         }
//                     }
//                 }
//             }

//             return View(model);
//         }

//         public async Task<IActionResult> Create(long termId)
//         {
//             //get grade template
//             ViewBag.Groups = _selectListProvider.GetStandardGradingGroups();
//             var user = await _userManager.GetUserAsync(User);
//             long instructorId = 0;
//             if (user != null)
//             {
//                 instructorId = _db.Instructors.FirstOrDefault(x => x.Code == user.UserName)?.Id ?? 0;
//             }

//             var term = _academicProvider.GetTerm(termId);
//             var model = new GradeManagementViewModel
//             {
//                 AcademicLevelId = term.AcademicLevelId,
//                 TermId = termId,
//                 InstructorId = instructorId
//             };

//             return Grading(model);
//         }

//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public IActionResult Create(GradeManagementViewModel model)
//         {
//             model.StandardGradingGroups = _db.StandardGradingGroups.Include(x => x.GradeTemplate)
//                                                                     .Include(x => x.StandardGradingScores)
//                                                                         .ThenInclude(x => x.Grade)
//                                                                     .ToList();
//             foreach (var item in model.StandardGradingGroups)
//             {
//                 item.StandardGradingScores = item.StandardGradingScores.OrderByDescending(x => x.Maximum).ToList();
//             }
//             return Grading(model);
//         }

//         public IActionResult Edit(long allocationId)
//         {
//             //get grade template
//             var allocation = _db.GradingAllocations.Find(allocationId);
//             var courseList = JsonConvert.DeserializeObject<List<long>>(allocation.CourseIds);
//             var students = StudentsByAllocationId(allocationId);
//             var studentCourseId = students.Select(x => x.RegistrationCourseId).ToList();
//             var studentRegistrationCourses = (from registrationCourse in _db.RegistrationCourses
//                                               where studentCourseId.Contains(registrationCourse.Id)
//                                               select registrationCourse).ToList();

//             var registrationCoursesGroupedByGradeTemplate = (from registrationCourse in studentRegistrationCourses
//                                                              join course in _db.Courses on registrationCourse.CourseId equals course.Id
//                                                              join gradeTemplate in _db.GradeTemplates on course.GradeTemplateId equals gradeTemplate.Id
//                                                              // Assuming that 1 GradeTemplate is used only by 1 StandardGradingGroup
//                                                              join standardGradingGroup in _db.StandardGradingGroups on gradeTemplate.Id equals standardGradingGroup.GradeTemplateId
//                                                              where studentCourseId.Contains(registrationCourse.Id)
//                                                              group new
//                                                              {
//                                                                  RegistrationCourse = registrationCourse,
//                                                                  GradeTemplate = gradeTemplate,
//                                                                  StandardGradingGroup = standardGradingGroup
//                                                              }
//                                                              by standardGradingGroup.Id into grp
//                                                              select new StudentBySectionViewModelGroupByGradeTemplate
//                                                              {
//                                                                  StandardGradingGroupId = grp.Key,
//                                                                  StandardGradingGroup = grp.FirstOrDefault().StandardGradingGroup,
//                                                                  StudentBySectionViewModels = students.Where(x => grp.Select(y => y.RegistrationCourse.Id).Contains(x.RegistrationCourseId)).ToList()
//                                                              })
//                                                             .OrderBy(x => x.StandardGradingGroup.Name)
//                                                             .ToList();
//             var model = new GradeManagementViewModel
//             {
//                 GradingAllocationId = allocation.Id,
//                 StandardGradingGroupId = allocation.StandardGradingGroupId,
//                 TermId = allocation.TermId,
//                 CourseIds = JsonConvert.DeserializeObject<List<long>>(allocation.CourseIds),
//                 JsonAllocations = allocation.Allocations,
//                 StudentScores = students,
//                 StudentScoresResult = students.Where(x => !x.IsSkipGrading).ToList(),
//                 Allocations = JsonConvert.DeserializeObject<List<Allocation>>(allocation.Allocations),
//                 StandardGradingGroups = _db.StandardGradingGroups.Include(x => x.GradeTemplate)
//                                                                              .Include(x => x.StandardGradingScores)
//                                                                                 .ThenInclude(x => x.Grade)
//                                                                              .ToList(),
//                 StudentScoresGroupByGradeTemplate = registrationCoursesGroupedByGradeTemplate,
//                 StudentScoresCount = studentRegistrationCourses.Count,
//                 WithdrawCount = students.Count(x => x.IsWithdrawal)
//             };
//             return Grading(model);
//         }

//         public ActionResult Delete(long id)
//         {
//             var model = _gradeProvider.GetGradingAllocation(id);
//             var barcodes = _gradeProvider.GetBarcodeByAllocationId(id);

//             using (var transaction = _db.Database.BeginTransaction())
//             {
//                 try
//                 {
//                     if (barcodes.Any())
//                     {
//                         barcodes.Select(x =>
//                         {
//                             x.IsActive = false;
//                             return x;
//                         })
//                                 .ToList();
//                     }

//                     model.GradingScores.Select(x =>
//                     {
//                         x.IsActive = false;
//                         return x;
//                     }).ToList();

//                     model.StudentScores.Select(x =>
//                     {
//                         x.IsActive = false;
//                         return x;
//                     }).ToList();

//                     model.IsActive = false;

//                     _db.SaveChanges();
//                     transaction.Commit();
//                     _flashMessage.Confirmation(Message.SaveSucceed);
//                 }
//                 catch
//                 {
//                     transaction.Rollback();
//                     _flashMessage.Danger(Message.UnableToDelete);
//                 }
//             }

//             var academicLevelId = _academicProvider.GetTerm(model.TermId).AcademicLevelId;
//             return RedirectToAction(nameof(Index), new
//             {
//                 AcademicLevelId = academicLevelId,
//                 TermId = model.TermId
//             });
//         }

//         public IActionResult Grading(GradeManagementViewModel model)
//         {
//             if (model.ResetTermId != 0)
//             {
//                 model.TermId = model.ResetTermId;
//                 var resetTerm = _academicProvider.GetTerm(model.TermId);
//                 model.AcademicLevelId = resetTerm.AcademicLevelId;
//                 return RedirectToAction(nameof(Index), model);
//             }

//             CreateGradingSelectList(model.TermId, model.InstructorId);
//             var term = _academicProvider.GetTerm(model.TermId);
//             model.AcademicLevelId = term.AcademicLevelId;
//             if (model.CourseIds.Any())
//             {
//                 if (model.GradingAllocationId > 0)
//                 {
//                     model.GradingScoresCurve = _gradeProvider.GetGradingScoreByAllocationId(model.GradingAllocationId);
//                 }
//                 else
//                 {
//                     model.GradingScoresCurve = _gradeProvider.GetGradingScoresByStandardGroupId(model.StandardGradingGroupId);
//                 }

//                 model.GradingScores = model.GradingScoresCurve;
//                 model.Courses = _registrationProvider.GetCourseByIds(model.CourseIds);
//                 model.GradingResult = _gradeProvider.GetStudentScoresByAllocationId(model.GradingAllocationId);

//                 model.ResetCourseIds = model.CourseIds;
//                 model.ResetStandardGradingGroupId = model.StandardGradingGroupId;
//                 model.ResetGradingAllocationId = model.GradingAllocationId;
//                 model.ResetTermId = model.TermId;
//                 model.ResetAcademicLevelId = model.AcademicLevelId;

//             }

//             return View("~/Views/GradeManagement/Grading.cshtml", model);
//         }

//         public List<GradingScore> GetGradingStatistics(long gradingAllocationId)
//         {
//             return _gradeProvider.GetGradingScoreByAllocationId(gradingAllocationId);
//         }

//         public List<GradingScore> GetGradingAllocation(long gradingAllocationId)
//         {
//             return _gradeProvider.GetGradingScoreByAllocationId(gradingAllocationId);
//         }

//         public GradingLog GetGradingLog(long studentScoreId, long allocationId)
//         {
//             var gradingLog = _gradeProvider.GetLatestGradingLog(studentScoreId);
//             if (gradingLog == null)
//             {
//                 gradingLog = new GradingLog();
//             }
//             gradingLog.Grades = _gradeProvider.GetGrades();
//             return gradingLog;
//         }

//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public PartialViewResult GradingResult(long gradingAllocationId) // Js call
//         {
//             var withdraw = _gradeProvider.GetGradeByName("W");
//             var studentScores = _gradeProvider.GetStudentScoresByAllocationId(gradingAllocationId);
//             var gradingScores = _gradeProvider.GetGradingScoreByAllocationId(gradingAllocationId);

//             foreach (var item in studentScores)
//             {
//                 var gradeTemplateId = (from registrationCourse in _db.RegistrationCourses
//                                        join section in _db.Sections on registrationCourse.SectionId equals section.Id
//                                        join academicInformation in _db.AcademicInformations on registrationCourse.StudentId equals academicInformation.StudentId
//                                        join curriculumVersion in _db.CurriculumVersions on academicInformation.CurriculumVersionId equals curriculumVersion.Id
//                                        join courseGroup in _db.CourseGroups on curriculumVersion.Id equals courseGroup.CurriculumVersionId
//                                        join curriculumCourse in _db.CurriculumCourses on new { CourseGroupId = courseGroup.Id, registrationCourse.CourseId } equals new { curriculumCourse.CourseGroupId, curriculumCourse.CourseId }
//                                        where registrationCourse.Id == item.RegistrationCourseId
//                                        select curriculumCourse.GradeTemplateId).SingleOrDefault();

//                 var grade = gradingScores.SingleOrDefault(x => x.GradeTemplateId == gradeTemplateId && x.Minimum <= Convert.ToInt32(item.Percentage) && x.Maximum >= Convert.ToInt32(item.Percentage));
//                 item.Grade = item.IsWithdrawal ? withdraw : item.Grade;

//                 if (item.Student.StudentScores != null)
//                 {
//                     item.Student.StudentScores.Clear();
//                 }

//                 if (item.RegistrationCourse.Withdrawals != null)
//                 {
//                     item.RegistrationCourse.Withdrawals.Clear();
//                 }

//                 if (item.Student.CheatingStatuses != null)
//                 {
//                     item.Student.CheatingStatuses.Clear();
//                 }
//             }

//             return PartialView("~/Views/GradeManagement/GradingSteps/_ResultTable.cshtml", studentScores.OrderBy(x => x.RegistrationCourse.Section.Number).ThenBy(x => x.Student.Code).ToList());
//         }

//         [HttpPost]
//         public void SaveStudentScore(long studentScoreId, long gradeId, string remark)
//         {
//             _gradeProvider.SaveStudentScore(studentScoreId, gradeId, remark);
//         }

//         [HttpPost]
//         [Consumes("application/x-www-form-urlencoded")]
//         [RequestFormLimits(ValueCountLimit = 50000)]
//         public string Save([FromForm] GradeManagementViewModel Model, long currentIndex, long AcademicLevelId, long TermId, List<long> CourseIds,
//             long GradingAllocationId, long StandardGradingGroupId, List<Allocation> Allocations, List<StandardGradingGroup> SelectedStandardGradingGroups)
//         {
//             GradeManagementViewModel viewModel = new GradeManagementViewModel();
//             viewModel.AcademicLevelId = AcademicLevelId;
//             viewModel.TermId = TermId;
//             viewModel.CourseIds = CourseIds;
//             viewModel.GradingAllocationId = GradingAllocationId;
//             viewModel.Allocations = Allocations;
//             viewModel.StandardGradingGroupId = StandardGradingGroupId;

//             long gradingAllocationId = 0;
//             var stringUrl = Url.Action(nameof(Index), new
//             {
//                 AcademicLevelId = AcademicLevelId,
//                 TermId = TermId
//             });

//             if (CourseIds == null)
//             {
//                 _flashMessage.Danger(Message.RequiredData);
//                 return stringUrl;
//             }

//             if (currentIndex == 0)// && nextIndex == 1) //Allocate Score view
//             {
//                 if (viewModel.GradingAllocationId == 0)
//                 {
//                     // new allocation
//                     gradingAllocationId = SetGradingAllocations(viewModel);
//                 }
//                 else
//                 {
//                     // update allocation in model & grading score
//                     gradingAllocationId = UpdateGradingAllocations(viewModel);
//                 }
//             }
//             else if (currentIndex == 1 && viewModel.GradingAllocationId != 0) // Scoring View
//             {
//                 _gradeProvider.UpdateStudentScores(viewModel.GradingAllocationId, Model.StudentScores);
//                 gradingAllocationId = viewModel.GradingAllocationId;
//             }
//             else if (currentIndex == 1 && GradingAllocationId != 0) // Scoring View
//             {
//                 _gradeProvider.UpdateStudentScores(GradingAllocationId, Model.StudentScores);
//                 gradingAllocationId = GradingAllocationId;
//             }
//             else if (currentIndex == 2)
//             {
//                 _gradeProvider.SaveStandardGradingGroups(GradingAllocationId, SelectedStandardGradingGroups);

//                 // Save to Student Score and Grading Log
//                 _gradeProvider.SaveStudentScoresAndGradingLog(GradingAllocationId);

//             }
//             else if (currentIndex == 3)
//             {
//                 var pdfUrl = Url.Action(nameof(Report), new
//                 {
//                     allocationId = GradingAllocationId,
//                     courseIds = JsonConvert.SerializeObject(viewModel.CourseIds)
//                 });
//                 return pdfUrl;
//             }

//             return stringUrl;
//         }

//         [HttpPost]
//         [Consumes("application/x-www-form-urlencoded")]
//         [RequestFormLimits(ValueCountLimit = 50000)]
//         public string Continue([FromForm] GradeManagementViewModel Model, long currentIndex, long AcademicLevelId, long TermId, List<long> CourseIds,
//             long GradingAllocationId, long StandardGradingGroupId, List<Allocation> Allocations, List<StandardGradingGroup> SelectedStandardGradingGroups, List<StudentScore> StudentScores)
//         //([FromForm] GradeManagementViewModel model, long currentIndex, long nextIndex, long TermId, long GradingAllocationId, List<StandardGradingGroup> SelectedStandardGradingGroups)
//         {
//             GradeManagementViewModel viewModel = new GradeManagementViewModel();
//             viewModel.AcademicLevelId = AcademicLevelId;
//             viewModel.TermId = TermId;
//             viewModel.CourseIds = CourseIds;
//             viewModel.GradingAllocationId = GradingAllocationId;
//             viewModel.Allocations = Allocations;
//             viewModel.StandardGradingGroupId = StandardGradingGroupId;
//             long gradingAllocationId = 0;
//             if (viewModel.CourseIds != null)
//             {
//                 if (currentIndex == 0)// && nextIndex == 1) //Allocate Score view
//                 {
//                     if (viewModel.GradingAllocationId == 0)
//                     {
//                         // new allocation
//                         gradingAllocationId = SetGradingAllocations(viewModel);
//                     }
//                     else
//                     {
//                         // update allocation in model & grading score
//                         gradingAllocationId = UpdateGradingAllocations(viewModel);
//                     }
//                 }
//                 else if (currentIndex == 1 && viewModel.GradingAllocationId != 0) // Scoring View
//                 {
//                     _gradeProvider.UpdateStudentScores(viewModel.GradingAllocationId, Model.StudentScores);
//                     gradingAllocationId = viewModel.GradingAllocationId;
//                 }
//                 else if (currentIndex == 1 && GradingAllocationId != 0) // Scoring View
//                 {
//                     _gradeProvider.UpdateStudentScores(GradingAllocationId, Model.StudentScores);
//                     gradingAllocationId = GradingAllocationId;
//                 }
//                 else if (currentIndex == 2)
//                 {
//                     // if (model.GradingAllocationId == 0)
//                     // {
//                     //     // new allocation
//                     //     gradingAllocationId = SetGradingAllocations(model);
//                     // }
//                     // else
//                     // {
//                     //     // update allocation in model & grading score
//                     //     gradingAllocationId = UpdateGradingAllocations(model, true);
//                     // }

//                     _gradeProvider.SaveStandardGradingGroups(GradingAllocationId, SelectedStandardGradingGroups);

//                     // Save to Student Score and Grading Log
//                     _gradeProvider.SaveStudentScoresAndGradingLog(GradingAllocationId);

//                 }
//                 else if (currentIndex == 3)
//                 {

//                 }
//             }

//             return gradingAllocationId.ToString();
//         }

//         [HttpPost]
//         [Consumes("application/x-www-form-urlencoded")]
//         [RequestFormLimits(ValueCountLimit = 50000)]
//         public string Finish([FromForm] GradeManagementViewModel model, long GradingAllocationId)
//         {
//             var courseIds = _gradeProvider.GetGradingAllocation(GradingAllocationId).CourseIds;
//             _gradeProvider.FinishGradingAllocation(GradingAllocationId);
//             var stringUrl = Url.Action(nameof(Report), new
//             {
//                 allocationId = GradingAllocationId,
//                 courseIds = JsonConvert.SerializeObject(courseIds)
//             });
//             return stringUrl;
//         }

//         public ActionResult Report(long allocationId, string courseIds)
//         {
//             var allocation = _gradeProvider.GetGradingAllocation(allocationId);
//             var term = _db.Terms.SingleOrDefault(x => x.Id == allocation.TermId);
//             if (string.IsNullOrEmpty(courseIds))
//             {
//                 _flashMessage.Danger(Message.RequiredData);
//                 return RedirectToAction(nameof(Index), new GradeManagementViewModel
//                 {
//                     AcademicLevelId = term.AcademicLevelId,
//                     TermId = term.Id
//                 });
//             }

//             var courseConvertIds = JsonConvert.DeserializeObject<List<long>>(courseIds.Replace("\"", ""));
//             var user = _userManager.GetUserName(User);
//             var report = new ReportViewModel();
//             foreach (var courseId in courseConvertIds)
//             {
//                 report = new ReportViewModel
//                 {
//                     Title = "Report",
//                     Subject = "Grading Result Report",
//                     Creator = "Keystone V.xxxx",
//                     Author = user,
//                     Body = _gradeProvider.GetGradeManagementViewModelByCourseId(allocationId, courseId)
//                 };
//             }

//             return View(report);
//         }

//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         [RequestFormLimits(ValueCountLimit = 100000)]
//         public PartialViewResult Import(GradeManagementViewModel model, int totalWithdraw, int totalStudentScores)
//         {
//             ModelState.Clear();
//             var extensions = new List<string> { ".xlsx", ".xls" };
//             if (string.IsNullOrEmpty(model.UploadFile?.FileName)
//                 || !extensions.Contains(Path.GetExtension(model.UploadFile.FileName)))
//             {
//                 _flashMessage.Danger("Invalid file.");
//             }
//             else
//             {
//                 try
//                 {
//                     System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
//                     using (var stream = model.UploadFile.OpenReadStream())
//                     {
//                         using (var reader = ExcelReaderFactory.CreateReader(stream))
//                         {
//                             var result = reader.AsDataSet(new ExcelDataSetConfiguration()
//                             {
//                                 ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
//                                 {
//                                     UseHeaderRow = false
//                                 }
//                             });

//                             for (int i = 0; i < result.Tables.Count; i++)
//                             {
//                                 for (int j = 1; j < result.Tables[i].Rows.Count; j++)
//                                 {
//                                     string courseCode = result.Tables[i].Rows[j][0]?.ToString();
//                                     string sectionNumber = result.Tables[i].Rows[j][1]?.ToString();
//                                     string studentCode = result.Tables[i].Rows[j][2]?.ToString();
//                                     var studentScoreTmp = model.StudentScoresGroupByGradeTemplate.SelectMany(x => x.StudentBySectionViewModels)
//                                                                                               .FirstOrDefault(x => x.CourseCode == courseCode
//                                                                                                                    && x.SectionNumber == sectionNumber
//                                                                                                                    && x.StudentCode == studentCode);
//                                     var studentScore = model.StudentScores.SingleOrDefault(x => x.RegistrationCourseId == studentScoreTmp.RegistrationCourseId);
//                                     if (studentCode != null)
//                                     {
//                                         for (int k = 4; k < result.Tables[i].Columns.Count; k++)
//                                         {
//                                             var header = result.Tables[i].Rows[0][k]?.ToString();
//                                             var score = studentScore.Scores.FirstOrDefault(x => x.Abbreviation == header);
//                                             if (score != null)
//                                             {
//                                                 score.FullScore = Convert.ToDecimal(result.Tables[i].Rows[j][k]);
//                                             }
//                                         }
//                                     }
//                                     studentScoreTmp.Scores = studentScore.Scores;
//                                 }
//                             }
//                         }
//                     }
//                 }
//                 catch
//                 {
//                     _flashMessage.Danger(Message.UnableToEdit);
//                 }
//             }

//             var returnModel = new GradeManagementViewModel
//             {
//                 Allocations = model.Allocations,
//                 StudentScoresGroupByGradeTemplate = model.StudentScoresGroupByGradeTemplate,
//                 StudentScoresCount = totalStudentScores,
//                 WithdrawCount = totalWithdraw
//             };

//             return PartialView("~/Views/GradeManagement/GradingSteps/_ScoringTable.cshtml", returnModel);
//         }

//         [HttpPost]
//         [Consumes("application/x-www-form-urlencoded")]
//         [RequestFormLimits(ValueCountLimit = 10000)]
//         public IActionResult Export([FromForm] GradeManagementViewModel Model)
//         {
//             string handle = string.Empty;
//             try
//             {
//                 using (var workbook = new XLWorkbook())
//                 {
//                     IXLWorksheet worksheet = workbook.Worksheets.Add("Scores");
//                     worksheet.Cell(1, 1).Value = "Course Code";
//                     worksheet.Cell(1, 2).Value = "Section";
//                     worksheet.Cell(1, 3).Value = "Student Code";
//                     worksheet.Cell(1, 4).Value = "Student Name";
//                     int column = 5;
//                     foreach (var item in Model.Allocations)
//                     {
//                         worksheet.Cell(1, column++).Value = item.Abbreviation;
//                     }

//                     int row = 2;
//                     foreach (var template in Model.StudentScoresGroupByGradeTemplate)
//                     {
//                         foreach (var item in template.StudentBySectionViewModels)
//                         {
//                             worksheet.Cell(row, 1).Value = item.CourseCode;
//                             worksheet.Cell(row, 2).Value = item.SectionNumber;
//                             worksheet.Cell(row, 3).Value = item.StudentCode;
//                             worksheet.Cell(row, 4).Value = item.StudentName;
//                             column = 5;
//                             var scores = Model.StudentScores.SingleOrDefault(x => x.StudentId == item.StudentId);
//                             if (scores != null)
//                             {
//                                 foreach (var score in scores.Scores)
//                                 {
//                                     worksheet.Cell(row, column++).Value = score.FullScore;
//                                 }
//                             }

//                             row++;
//                         }
//                     }

//                     using (var stream = new MemoryStream())
//                     {
//                         workbook.SaveAs(stream);
//                         handle = Guid.NewGuid().ToString();
//                         stream.Position = 0;
//                         TempData[handle] = stream.ToArray();
//                     }
//                 }
//             }
//             catch { }

//             var response = new ApiResponse<string>
//             {
//                 StatusCode = string.IsNullOrEmpty(handle) ? HttpStatusCode.Forbidden : HttpStatusCode.OK,
//                 Result = handle
//             };

//             return Ok(response);
//         }

//         [HttpGet]
//         public virtual ActionResult Download(string fileGuid)
//         {
//             string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
//             string fileName = "Student scoring.xlsx";
//             if (TempData[fileGuid] != null)
//             {
//                 byte[] content = TempData[fileGuid] as byte[];
//                 return File(content, contentType, fileName);
//             }
//             else
//             {
//                 return new EmptyResult();
//             }
//         }

//         private void CreateGradingSelectList(long termId, long instructorId)
//         {
//             ViewBag.ExaminationTypes = _gradeProvider.GetExaminationTypes();
//             ViewBag.Courses = instructorId == 0 ? _selectListProvider.GetCoursesByTerm(termId)
//                                                 : _selectListProvider.GetTeachingCourseByInstructorId(instructorId, termId);
//             ViewBag.Groups = _selectListProvider.GetStandardGradingGroups();
//         }

//         private void CreateSelectList(long academicLevelId = 0)
//         {
//             ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
//             if (academicLevelId != 0)
//             {
//                 ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
//             }
//         }

//         public async Task<List<GradeSection>> SectionsByCourseIds(long termId, List<long> courseIds) // Js call
//         {
//             var user = await _userManager.GetUserAsync(User);
//             var instructor = _db.Instructors.FirstOrDefault(x => x.Code == "sunsern");
//             if (instructor != null)
//             {
//                 var sections = new List<Section>();
//                 foreach (var courseId in courseIds)
//                 {
//                     // For MUIC
//                     var instructorSection = _gradeProvider.GetSectionByInstructorId(instructor.Id, courseId, termId);
//                     sections.AddRange(instructorSection);
//                 }

//                 var selectedSectionsIds = _gradeProvider.GetStudentScoresByCourseIds(termId, courseIds)
//                                                     .Select(x => x.RegistrationCourse.SectionId)
//                                                     .Distinct()
//                                                     .ToList();

//                 var gradeSections = sections.Select(x => _mapper.Map<Section, GradeSection>(x))
//                                             .Select(x =>
//                                             {
//                                                 x.IsSelected = selectedSectionsIds.Contains(x.SectionId);
//                                                 return x;
//                                             }).OrderBy(x => x.CourseCode)
//                                                         .ThenBy(x => x.SectionNumber)
//                                                         .ToList();

//                 return gradeSections;
//             }
//             return new List<GradeSection>();       
//         }

//         public List<StudentBySectionViewModel> StudentsByAllocationId(long allocationId)
//         {
//             var students = _gradeProvider.GetStudentScoresByAllocationId(allocationId);
//             var studentSections = students.Select(x => x.RegistrationCourse.SectionId)
//                                           .Distinct()
//                                           .ToList();

//             var relatedSections = _db.Sections.Where(x => studentSections.Contains(x.ParentSectionId)
//                                                           || studentSections.Contains(x.Id))
//                                               .Select(x => x.Id)
//                                               .ToList();

//             var studentScores = _gradeProvider.GetStudentScoresBySectionIds(relatedSections);
//             foreach (var student in studentScores)
//             {
//                 if (student.RegistrationCourse.Grade != null
//                     && (student.RegistrationCourse.Grade.Weight == null
//                         || student.RegistrationCourse.Grade.Weight == 0))
//                 {
//                     student.PercentageForGPACalculation = -1;
//                 }
//                 else
//                 {
//                     student.PercentageForGPACalculation = student.TotalScore;
//                 }
//             }

//             var result = studentScores.Select(x => _mapper.Map<StudentScore, StudentBySectionViewModel>(x))
//                                       .OrderBy(x => x.SectionNumber).ThenBy(x => x.StudentCode)
//                                       .ToList();
//             return result;
//         }

//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public PartialViewResult AllStudentBySectionIds(string ids, string childrenids, long gradingAllocationId) // Js call
//         {
//             var sectionIds = JsonConvert.DeserializeObject<List<long>>(ids);
//             var childrenSectionIds = JsonConvert.DeserializeObject<List<long>>(childrenids);
//             if (childrenSectionIds != null && childrenSectionIds.Any())
//             {
//                 sectionIds.AddRange(childrenSectionIds);
//             }
//             var studentBySectionViewModel = new List<StudentBySectionViewModel>();
//             // var existedStudents = _gradeProvider.GetStudentScoresBySectionIdsWithCurriculumVersionId(sectionIds)
//             //                                     .Select(x => _mapper.Map<StudentScore, StudentBySectionViewModel>(x))
//             //                                     .ToList();
//             var existedStudents = _gradeProvider.GetRegistrationCoursesBySectionIds(sectionIds, gradingAllocationId);
//             studentBySectionViewModel.AddRange(existedStudents);
//             var existedRegistrationCourseIds = existedStudents.Select(x => x.RegistrationCourseId)
//                                                    .Distinct()
//                                                    .ToList();
//             var full = new
//             {
//                 RegisteringCourse = new RegisteringCourse(),

//             };
//             // For the tested single case, this comment section run a bit faster than the used commit
//             //var registrationCoursesTmp = (from registrationCourse in _db.RegistrationCourses
//             //                            join student in _db.Students on registrationCourse.StudentId equals student.Id
//             //                            join title in _db.Titles on student.TitleId equals title.Id
//             //                            join section in _db.Sections on registrationCourse.SectionId equals section.Id
//             //                            join course in _db.Courses on registrationCourse.CourseId equals course.Id
//             //                            where registrationCourse.SectionId == sectionIds[0]
//             //                                && (registrationCourse.Status == "a" || registrationCourse.Status == "r")
//             //                                && !existedRegistrationCourseIds.Contains(registrationCourse.Id)
//             //                                && !section.IsClosed
//             //                            select new 
//             //                            {
//             //                                RegisteringCourse = registrationCourse,
//             //                                Student = student,
//             //                                Title = title,
//             //                                Section = section,
//             //                                Course = course
//             //                            }).ToList();
//             //for (int i = 1; i < sectionIds.Count; i++)
//             //{
//             //    registrationCoursesTmp.AddRange((from registrationCourse in _db.RegistrationCourses
//             //                                     join student in _db.Students on registrationCourse.StudentId equals student.Id
//             //                                     join title in _db.Titles on student.TitleId equals title.Id
//             //                                     join section in _db.Sections on registrationCourse.SectionId equals section.Id
//             //                                     join course in _db.Courses on registrationCourse.CourseId equals course.Id
//             //                                     where registrationCourse.SectionId == sectionIds[i]
//             //                                         && (registrationCourse.Status == "a" || registrationCourse.Status == "r")
//             //                                         && !existedRegistrationCourseIds.Contains(registrationCourse.Id)
//             //                                         && !section.IsClosed
//             //                                     select new
//             //                                     {
//             //                                         RegisteringCourse = registrationCourse,
//             //                                         Student = student,
//             //                                         Title = title,
//             //                                         Section = section,
//             //                                         Course = course
//             //                                     }));
//             //}


//             // This one is a bit slower than above but still prety fast compare to before
//             // now modify add more data
//             // var modSectionIds = sectionIds.Select(x => (long?)x).ToList();
//             // var registrationCoursesTmp = (from registrationCourse in _db.RegistrationCourses
//             //                               join student in _db.Students on registrationCourse.StudentId equals student.Id
//             //                               join title in _db.Titles on student.TitleId equals title.Id
//             //                               join section in _db.Sections on registrationCourse.SectionId equals section.Id
//             //                               join course in _db.Courses on registrationCourse.CourseId equals course.Id
//             //                               where modSectionIds.Contains(registrationCourse.SectionId)
//             //                                   && (registrationCourse.Status == "a" || registrationCourse.Status == "r")
//             //                                   && !existedRegistrationCourseIds.Contains(registrationCourse.Id)
//             //                                   && !section.IsClosed
//             //                               select new
//             //                               {
//             //                                   RegisteringCourse = registrationCourse,
//             //                                   Student = student,
//             //                                   Title = title,
//             //                                   Section = section,
//             //                                   Course = course,
//             //                                   CurVerId = student.AcademicInformation.CurriculumVersionId
//             //                               }).ToList();
//             // var registrationCourses = new List<RegistrationCourse>();
//             // var withdrawals = _db.Withdrawals.Where(x => registrationCoursesTmp.Select(y => y.RegisteringCourse.Id).Contains(x.RegistrationCourseId)).ToList();
//             // foreach (var item in registrationCoursesTmp)
//             // {
//             //     item.RegisteringCourse.Student = item.Student;
//             //     item.RegisteringCourse.Student.Title = item.Title;
//             //     item.RegisteringCourse.Section = item.Section;
//             //     item.RegisteringCourse.Course = item.Course;
//             //     item.RegisteringCourse.Withdrawals = withdrawals.Where(x => x.RegistrationCourseId == item.RegisteringCourse.Id).ToList();
//             //     registrationCourses.Add(item.RegisteringCourse);
//             // }

//             // if (registrationCourses.Any())
//             // {
//             //     var students = registrationCourses.Select(x => _mapper.Map<RegistrationCourse, StudentBySectionViewModel>(x))
//             //                                       .ToList();
//             //     studentBySectionViewModel.AddRange(students);
//             // }

//             // var studentRegistrationCourses = (from registrationCourse in _db.RegistrationCourses
//             //                                   where studentBySectionViewModel.Select(x => x.RegistrationCourseId).Contains(registrationCourse.Id)
//             //                                   select registrationCourse).ToList();
//             //var registrationCoursesGroupedByGradeTemplate2 = (from registrationCourse in studentBySectionViewModel
//             //                                                 join section in _db.Sections on registrationCourse.SectionId equals section.Id
//             //                                                 join academicInformation in _db.AcademicInformations on registrationCourse.StudentId equals academicInformation.StudentId
//             //                                                 join curriculumVersion in _db.CurriculumVersions on academicInformation.CurriculumVersionId equals curriculumVersion.Id
//             //                                                 join courseGroup in _db.CourseGroups on curriculumVersion.Id equals courseGroup.CurriculumVersionId
//             //                                                 join curriculumCourse in _db.CurriculumCourses on new { CourseGroupId = courseGroup.Id, registrationCourse.CourseId } equals new { curriculumCourse.CourseGroupId, curriculumCourse.CourseId }
//             //                                                 join gradeTemplate in _db.GradeTemplates on curriculumCourse.GradeTemplateId equals gradeTemplate.Id
//             //                                                 // Assuming that 1 GradeTemplate is used only by 1 StandardGradingGroup
//             //                                                 join standardGradingGroup in _db.StandardGradingGroups on gradeTemplate.Id equals standardGradingGroup.GradeTemplateId
//             //                                                 //where studentBySectionViewModel.Select(x => x.RegistrationCourseId).Contains(registrationCourse.Id)
//             //                                                 group new
//             //                                                 {
//             //                                                     RegistrationCourse = registrationCourse,
//             //                                                     AcademicInformation = academicInformation,
//             //                                                     GradeTemplate = gradeTemplate,
//             //                                                     StandardGradingGroup = standardGradingGroup
//             //                                                 }
//             //                                                 by standardGradingGroup.Id into grp
//             //                                                 select new StudentBySectionViewModelGroupByGradeTemplate
//             //                                                 {
//             //                                                     StandardGradingGroupId = grp.Key,
//             //                                                     StandardGradingGroup = grp.FirstOrDefault().StandardGradingGroup,
//             //                                                     StudentBySectionViewModels = studentBySectionViewModel.OrderBy(x => x.SectionNumber).ThenBy(x => x.StudentCode).ToList()
//             //                                                     //.Where(x => grp.Select(y => y.RegistrationCourse.Id).Contains(x.RegistrationCourseId)).ToList()
//             //                                                 })
//             //                                                .OrderBy(x => x.StandardGradingGroup.Name)
//             //                                                .ToList();

//             var standardGroup = studentBySectionViewModel.Select(x => new { x.CurriculumVersionId, x.CourseId }).ToList();
//             var registrationCoursesGroupedByGradeTemplate = (from sg in standardGroup
//                                                              join courseGroup in _db.CourseGroups on sg.CurriculumVersionId equals courseGroup.CurriculumVersionId
//                                                              join curriculumCourse in _db.CurriculumCourses on new { CourseGroupId = courseGroup.Id, sg.CourseId } equals new { curriculumCourse.CourseGroupId, curriculumCourse.CourseId }
//                                                              join gradeTemplate in _db.GradeTemplates on curriculumCourse.GradeTemplateId equals gradeTemplate.Id
//                                                              // Assuming that 1 GradeTemplate is used only by 1 StandardGradingGroup
//                                                              join standardGradingGroup in _db.StandardGradingGroups on gradeTemplate.Id equals standardGradingGroup.GradeTemplateId
//                                                              //where studentBySectionViewModel.Select(x => x.RegistrationCourseId).Contains(registrationCourse.Id)
//                                                              group new
//                                                              {
//                                                                  GradeTemplate = gradeTemplate,
//                                                                  StandardGradingGroup = standardGradingGroup
//                                                              }
//                                                              by standardGradingGroup.Id into grp
//                                                              select new StudentBySectionViewModelGroupByGradeTemplate
//                                                              {
//                                                                  StandardGradingGroupId = grp.Key,
//                                                                  StandardGradingGroup = grp.FirstOrDefault().StandardGradingGroup,
//                                                                  StudentBySectionViewModels = studentBySectionViewModel.OrderBy(x => x.SectionNumber).ThenBy(x => x.StudentCode).ToList()
//                                                                  //.Where(x => grp.Select(y => y.RegistrationCourse.Id).Contains(x.RegistrationCourseId)).ToList()
//                                                              })
//                                                           .OrderBy(x => x.StandardGradingGroup.Name)
//                                                           .ToList();

//             var model = new GradeManagementViewModel
//             {
//                 Allocations = _gradeProvider.GetAllocationScores(gradingAllocationId),
//                 StudentScoresGroupByGradeTemplate = registrationCoursesGroupedByGradeTemplate,
//                 StudentScoresCount = registrationCoursesGroupedByGradeTemplate.Sum(x => x.StudentBySectionViewModels.Count),
//                 WithdrawCount = studentBySectionViewModel.Count(x => x.IsWithdrawal)
//             };
//             return PartialView("~/Views/GradeManagement/GradingSteps/_ScoringTable.cshtml", model);
//         }

//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public PartialViewResult GetSelectedStandardGradingGroups(string ids) // Js call
//         {
//             var standardGradingGroupIds = JsonConvert.DeserializeObject<List<long>>(ids);
//             var model = new GradeManagementViewModel
//             {
//                 SelectedStandardGradingGroups = _gradeProvider.GetStandardGradingGroups(standardGradingGroupIds)
//             };
//             return PartialView("~/Views/GradeManagement/GradingSteps/_GradeTemplateGradingGroups.cshtml", model);
//         }

//         public long SetGradingAllocations(GradeManagementViewModel model)
//         {
//             long gradingAllocationId = 0;
//             using (var transaction = _db.Database.BeginTransaction())
//             {
//                 try
//                 {
//                     var standardGradingGroupId = (from course in _db.Courses
//                                                   join gradeTemplate in _db.GradeTemplates on course.GradeTemplateId equals gradeTemplate.Id
//                                                   join standardGradingGroup in _db.StandardGradingGroups on gradeTemplate.Id equals standardGradingGroup.GradeTemplateId
//                                                   where model.CourseIds.Contains(course.Id)
//                                                   select standardGradingGroup.Id).FirstOrDefault();
//                     GradingAllocation allocation = new GradingAllocation()
//                     {
//                         TermId = model.TermId,
//                         CourseIds = JsonConvert.SerializeObject(model.CourseIds),
//                         Allocations = JsonConvert.SerializeObject(model.Allocations),
//                         StandardGradingGroupId = standardGradingGroupId
//                     };

//                     _db.GradingAllocations.Add(allocation);
//                     _db.SaveChanges();

//                     // if (model.GradingScores != null && model.GradingScores.Any())
//                     // {
//                     //     model.GradingScores.Select(x => {
//                     //                                         x.GradingAllocationId = allocation.Id;
//                     //                                         return x;
//                     //                                     })
//                     //                        .ToList();

//                     //     _db.GradingScores.AddRange(model.GradingScores);
//                     //     _db.SaveChanges();
//                     // }

//                     _flashMessage.Confirmation(Message.SaveSucceed);
//                     transaction.Commit();
//                     gradingAllocationId = allocation.Id;
//                 }
//                 catch
//                 {
//                     CreateGradingSelectList(model.TermId, model.InstructorId);
//                     _flashMessage.Danger(Message.UnableToSave);
//                     transaction.Rollback();
//                 }
//             }

//             return gradingAllocationId;
//         }

//         public long UpdateGradingAllocations(GradeManagementViewModel model, bool gradeScoreCurve = false)
//         {
//             long gradingAllocationId = 0;
//             var allocation = _gradeProvider.GetGradingAllocation(model.GradingAllocationId);
//             //var gradingScores = _gradeProvider.GetGradingScoreByAllocationId(model.GradingAllocationId);
//             using (var transaction = _db.Database.BeginTransaction())
//             {
//                 try
//                 {
//                     allocation.Allocations = JsonConvert.SerializeObject(model.Allocations);
//                     allocation.StandardGradingGroupId = model.StandardGradingGroupId;
//                     // for (int i = 0; i < gradingScores.Count; ++i)
//                     // {
//                     //     gradingScores[i].Maximum = gradeScoreCurve ? model.GradingScoresCurve[i].Maximum : model.GradingScores[i].Maximum;
//                     //     gradingScores[i].Minimum = gradeScoreCurve ? model.GradingScoresCurve[i].Minimum : model.GradingScores[i].Minimum;
//                     // }

//                     _db.SaveChanges();
//                     _flashMessage.Confirmation(Message.SaveSucceed);
//                     transaction.Commit();
//                     gradingAllocationId = allocation.Id;
//                 }
//                 catch
//                 {
//                     CreateGradingSelectList(model.TermId, model.InstructorId);
//                     _flashMessage.Danger(Message.UnableToSave);
//                     transaction.Rollback();
//                 }

//             }

//             return gradingAllocationId;
//         }

//         public IActionResult Reset(GradeManagementViewModel model)
//         {
//             if (model.GradingAllocationId > 0)
//             {
//                 var allocation = _db.GradingAllocations.SingleOrDefault(x => x.Id == model.GradingAllocationId);
//                 if (allocation.StandardGradingGroupId != model.StandardGradingGroupId)
//                 {
//                     using (var transaction = _db.Database.BeginTransaction())
//                     {
//                         try
//                         {
//                             var withdraw = _gradeProvider.GetGradeByName("W");
//                             // Standard grading group id
//                             allocation.StandardGradingGroupId = model.StandardGradingGroupId;

//                             // Score
//                             var scores = _db.StudentScores.Where(x => x.GradingAllocationId == model.GradingAllocationId)
//                                                           .ToList();
//                             foreach (var item in scores)
//                             {
//                                 if (item.GradeId != withdraw.Id)
//                                 {
//                                     item.GradeId = null;
//                                 }

//                                 // Registration course
//                                 var registrationCourse = _db.RegistrationCourses.SingleOrDefault(x => x.Id == item.RegistrationCourseId);
//                                 if (registrationCourse != null && registrationCourse.GradeId != withdraw.Id)
//                                 {
//                                     registrationCourse.GradeId = null;
//                                     registrationCourse.GradeName = null;
//                                 }
//                             }

//                             // Barcode
//                             var barcodes = _db.Barcodes.Where(x => x.GradingAllocationId == model.GradingAllocationId)
//                                                        .ToList();
//                             barcodes.ForEach(x => x.IsActive = false);

//                             // Grading score
//                             _db.GradingScores.RemoveRange(_db.GradingScores.Where(x => x.GradingAllocationId == model.GradingAllocationId));
//                             _db.SaveChanges();

//                             var gradingScores = _gradeProvider.GetGradingScoresByStandardGroupId(model.StandardGradingGroupId);
//                             gradingScores.ForEach(x => x.GradingAllocationId = model.GradingAllocationId);
//                             _db.GradingScores.AddRange(gradingScores);

//                             _db.SaveChanges();
//                             transaction.Commit();
//                         }
//                         catch
//                         {
//                             transaction.Rollback();
//                         }
//                     }
//                 }

//                 return Edit(model.GradingAllocationId);
//             }
//             else
//             {
//                 return Grading(model);
//             }
//         }
//     }
// }