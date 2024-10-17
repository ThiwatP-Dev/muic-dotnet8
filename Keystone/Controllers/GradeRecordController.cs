// using AutoMapper;
// using KeystoneLibrary.Data;
// using KeystoneLibrary.Interfaces;
// using KeystoneLibrary.Models;
// using KeystoneLibrary.Models.DataModels;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;
// using Newtonsoft.Json;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Vereyon.Web;

// namespace Keystone.Controllers
// {
//     public class GradeRecordController : BaseController
//     {
//         protected readonly IGradeProvider _gradeProvider;
//         private UserManager<ApplicationUser> _userManager { get; }

//         public GradeRecordController(ApplicationDbContext db, 
//                                      IFlashMessage flashMessage, 
//                                      IMapper mapper, 
//                                      ISelectListProvider selectListProvider,
//                                      IGradeProvider gradeProvider,
//                                      UserManager<ApplicationUser> user) : base(db, flashMessage, mapper, selectListProvider)
//         {
//             _gradeProvider = gradeProvider;
//             _userManager = user;
//         }

//         public IActionResult Index(GradeRecordViewModel model, string returnUrl)
//         {
//             var instructorId = GetInstructorId();
//             ViewBag.ReturnUrl = returnUrl;
//             if (!string.IsNullOrEmpty(returnUrl))
//             {
//                 ViewBag.DisplayQuestion = "Are you sure to approve this form?";
//             }

//             if (model.BarcodeId != 0)
//             {
//                 var barcode = _gradeProvider.GetGradeRecordViewModelByBarcode(model.BarcodeId);
//                 if (string.IsNullOrEmpty(barcode.BarcodeNumber))
//                 {
//                     _flashMessage.Danger(Message.RecordNotFound);
//                     return View(model);
//                 }

//                 if (!string.IsNullOrEmpty(barcode.BarcodeNumber) && !barcode.IsActive)
//                 {
//                     var active = _gradeProvider.GetActiveBarcodeByAllocationId(barcode.GradingAllocationId ?? 0);
//                     if (active != null)
//                     {
//                         _flashMessage.Danger($"This barcode is inactive, the updated barcode is ({ active.BarcodeNumber }).");
//                         return View(model);
//                     }

//                     _flashMessage.Danger("This barcode is inactive.");
//                     return View(model);
//                 }

//                 model.BarcodeInfromation = barcode;
//                 model.BarcodeNumber = barcode.BarcodeNumber;
//                 var gradingRanges = _gradeProvider.GetSummaryGradingRanges(barcode.Id, barcode.CourseId, barcode.Section.TermId, instructorId);
//                 var gradeTemplateIds = gradingRanges.Select(x => x.GradeTemplateId).ToList();
//                 var gradeTemplates = _db.GradeTemplates.Where(x => gradeTemplateIds.Contains(x.Id)).ToList();
//                 var letterIds = gradeTemplates.Where(x => x.Name.ToLower().Contains("letter"))
//                                               .Select(x => x.Id)
//                                               .ToList();

//                 var passFailIds = gradeTemplates.Where(x => x.Name.ToLower().Contains("pass") 
//                                                             || x.Name.ToLower().Contains("fail"))
//                                                 .Select(x => x.Id)
//                                                 .ToList();

//                 var letterGrades = gradingRanges.Where(x => letterIds.Contains(x.GradeTemplateId ?? 0)).ToList();
//                 var passFailGrades = gradingRanges.Where(x => passFailIds.Contains(x.GradeTemplateId ?? 0)).ToList();
//                 model.LetterGradingRanges.AddRange(letterGrades);
//                 model.PassFailGradingRanges.AddRange(passFailGrades);
//                 // model.Allocations = (string.IsNullOrEmpty(barcode.GradeAllocation?.Allocations)
//                 //                      ? new List<GradeRecordAllocation>() 
//                 //                      : JsonConvert.DeserializeObject<List<GradeRecordAllocation>>(barcode.GradeAllocation?.Allocations));
//                 model.StudentRecords = _gradeProvider.GetStudentScoresByBarcodeId(barcode.Id);
//             }

//             return View(model);
//         }

//         [HttpPost]
//         public ActionResult Save(GradeRecordViewModel model)
//         {
//             var barcode = _gradeProvider.GetBarcodeById(model.BarcodeId);
//             if (barcode == null)
//             {
//                 _flashMessage.Danger(Message.RecordNotFound);
//                 return RedirectToAction(nameof(Index), model);
//             }

//             using (var transaction = _db.Database.BeginTransaction())
//             {
//                 try
//                 {
//                     var userId = _userManager.GetUserId(User);
//                     barcode.IsPublished = true;
//                     barcode.PublishedAt = DateTime.UtcNow;

//                     var registrationCoursIds = model.StudentRecords.Select(x => x.RegistrationCourseId)
//                                                                    .ToList();
//                     var registrationCourses = _db.RegistrationCourses.Where(x => registrationCoursIds.Contains(x.Id))
//                                                                      .ToList();

//                     registrationCourses.Select(x => {
//                                                         x.GradeName = model.StudentRecords
//                                                                            .SingleOrDefault(y => y.RegistrationCourseId == x.Id)?.Grade;
//                                                         return x;
//                                                     }).ToList();

//                     var logs = new List<GradingLog>();
//                     foreach (var item in registrationCourses)
//                     {
//                         item.GradeName = model.StudentRecords.SingleOrDefault(x => x.RegistrationCourseId == item.Id)?.Grade;
//                         item.GradeId = _gradeProvider.GetGradeByName(item.GradeName).Id;
//                         var studentRecode = model.StudentRecords.SingleOrDefault(x => x.RegistrationCourseId == item.Id);

//                         logs.Add(_gradeProvider.SetGradingLog(studentRecode.StudentScoreId, item.Id, "", item.GradeName, userId, "Publish Grade", "a"));
//                     }

//                     _db.GradingLogs.AddRange(logs);

//                     _db.SaveChanges();
//                     transaction.Commit();
//                     _flashMessage.Confirmation(Message.SaveSucceed);
//                 }
//                 catch
//                 {
//                     transaction.Rollback();
//                     _flashMessage.Danger(Message.UnableToSave);
//                 }
//             }

//             return RedirectToAction(nameof(Index), model);
//         }

//         [HttpPost]
//         public ActionResult Approve(GradeRecordViewModel model, string returnUrl)
//         {
//             ViewBag.ReturnUrl = returnUrl;
//             var barcode = _gradeProvider.GetBarcodeById(model.BarcodeId);
//             if (barcode == null)
//             {
//                 _flashMessage.Danger(Message.RecordNotFound);
//                 return RedirectToAction(nameof(Index), new { BarcodeNumber = model.BarcodeNumber, returnUrl = returnUrl });
//             }

//             using (var transaction = _db.Database.BeginTransaction())
//             {
//                 try
//                 {
//                     var user = GetUser();
//                     barcode.ApprovedAt = DateTime.UtcNow;
//                     barcode.ApprovedBy = user.Id;

//                     _db.SaveChanges();
//                     transaction.Commit();
//                     _flashMessage.Confirmation(Message.SaveSucceed);
//                 }
//                 catch
//                 {
//                     transaction.Rollback();
//                     _flashMessage.Danger(Message.UnableToSave);
//                 }
//             }

//             return RedirectToAction(nameof(Index), new { BarcodeNumber = model.BarcodeNumber, SectionId = model.SectionId, returnUrl = returnUrl });
//         }

//         public ActionResult Cancel(long barcodeId, string returnUrl) 
//         {
//             ViewBag.ReturnUrl = returnUrl;
//             var model = _gradeProvider.GetGradeRecordViewModelByBarcode(barcodeId);
//             if (model == null)
//             {
//                 _flashMessage.Danger(Message.RecordNotFound);
//                 return RedirectToAction(nameof(Index), new { BarcodeNumber = model.BarcodeNumber, returnUrl = returnUrl });
//             }

//             if (!model.IsPublished)
//             {
//                 _flashMessage.Danger(Message.RecordUnpublished);
//                 return RedirectToAction(nameof(Index), new { BarcodeNumber = model.BarcodeNumber, returnUrl = returnUrl });
//             }

//             using (var transaction = _db.Database.BeginTransaction())
//             {
//                 var students = _gradeProvider.GetStudentScoresByAllocationId(model.GradingAllocationId ?? 0);
//                 var userId = _userManager.GetUserId(User);
//                 try
//                 {
//                     model.IsPublished = false;
//                     var logs = new List<GradingLog>();
//                     foreach (var item in students)
//                     {
//                         logs.Add(_gradeProvider.SetGradingLog(item.Id, item.RegistrationCourseId, item.RegistrationCourse.GradeName ?? "", "", userId, "Cancel Grade by barcode", "a"));          
//                         item.RegistrationCourse.Grade = null;
//                     }

//                     _db.GradingLogs.AddRange(logs);

//                     _db.SaveChanges();
//                     transaction.Commit();
//                     _flashMessage.Confirmation(Message.SaveSucceed);
//                 }
//                 catch
//                 {
//                     transaction.Rollback();
//                     _flashMessage.Danger(Message.UnableToCancel);
//                 }

//                 return RedirectToAction(nameof(Index), new { BarcodeNumber = model.BarcodeNumber, SectionId = model.SectionId, returnUrl = returnUrl });
//             }
//         }
//     }
// }