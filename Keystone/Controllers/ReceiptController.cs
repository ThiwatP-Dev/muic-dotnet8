using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Vereyon.Web;
using Microsoft.AspNetCore.Identity;
using Keystone.Permission;

namespace Keystone.Controllers
{
    public class ReceiptController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IReceiptProvider _receiptProvider;
        protected readonly IAcademicProvider _academicProvider;
        private UserManager<ApplicationUser> _userManager { get; }
        protected readonly IScholarshipProvider _scholarshipProvider;

        public ReceiptController(ApplicationDbContext db,
                                 IFlashMessage flashMessage,
                                 ISelectListProvider selectListProvider,
                                 IMapper mapper,
                                 IStudentProvider studentProvider,
                                 IRegistrationProvider registrationProvider,
                                 ICacheProvider cacheProvider,
                                 IReceiptProvider receiptProvider,
                                 IScholarshipProvider scholarshipProvider,
                                 IAcademicProvider academicProvider,
                                 UserManager<ApplicationUser> user,
                                 IHttpContextAccessor accessor) : base(db, flashMessage, mapper, selectListProvider)
        {
            _studentProvider = studentProvider;
            _registrationProvider = registrationProvider;
            _cacheProvider = cacheProvider;
            _receiptProvider = receiptProvider;
            _academicProvider = academicProvider;
            _userManager = user;
            _scholarshipProvider = scholarshipProvider;
        }

        [PermissionAuthorize("Receipt", "")]
        public ActionResult Index(ReceiptViewModel model)
        {
            CreateSelectList(model.AcademicLevelId);
            if (string.IsNullOrEmpty(model.StudentCode))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View("~/Views/Registration/Receipt/Index.cshtml", model);
            }

            // get all receipt by student code
            var student = _studentProvider.GetStudentByCode(model.StudentCode);
            if (student != null)
            {
                model.Receipts = _db.Receipts.Include(x => x.ReceiptItems)
                                                 .ThenInclude(x => x.InvoiceItem)
                                                 .ThenInclude(x => x.FeeItem)
                                             .Include(x => x.ReceiptItems)
                                                 .ThenInclude(x => x.InvoiceItem)
                                                 .ThenInclude(x => x.Section)
                                                 .ThenInclude(x => x.Course)
                                             .Include(x => x.ReceiptItems)
                                                 .ThenInclude(x => x.Invoice)
                                             .Include(x => x.Term)
                                             .Where(x => x.StudentId == student.Id
                                                         && (model.TermId == 0
                                                             || x.TermId == model.TermId))
                                             .OrderBy(x => x.Term.AcademicYear)
                                                 .ThenBy(x => x.Term.AcademicTerm)
                                             .ToList();
                
                if (model.Receipts != null && model.Receipts.Any())
                {
                    model.Receipts.ForEach(x => {
                        var user = _db.Users.SingleOrDefault(y => y.Id == x.PrintedBy);
                        var name = user?.FullnameEN?.Trim();
                        if (string.IsNullOrEmpty(name))
                        {
                            name = user?.UserName;
                        }

                        x.PrintedByFullName = name;
                    });
                }   
            }

            return View("~/Views/Registration/Receipt/Index.cshtml", model);
        }

        public ActionResult Details(long id)
        {
            var model = _receiptProvider.GetReceiptById(id);
            return PartialView("~/Views/Registration/Receipt/_Details.cshtml", model);
        }

        public ActionResult Preview()
        {
            return PartialView("_PreviewModal");
        }

        public ActionResult Transaction(long id)
        {
            List<ReceiptPrintLog> receiptPrintLog = _db.ReceiptPrintLogs.Include(x => x.Receipt)
                                                                        .Where(x => x.ReceiptId == id)
                                                                        .OrderBy(x => x.PrintedAt)
                                                                        .ToList();

            ViewBag.ReceiptNumber = receiptPrintLog.Any() ? receiptPrintLog.FirstOrDefault().Receipt?.Number ?? "" : "";
            return PartialView("~/Views/Registration/Receipt/_Transaction.cshtml", receiptPrintLog);
        }

        [PermissionAuthorize("Registration", PolicyGenerator.Write)]
        public ActionResult Cancel(long id, string returnUrl)
        {
            Receipt model = Find(id);
            // if (model == null || model.CreatedAt.Date != DateTime.Now.Date)
            if (model == null)
            {
                _flashMessage.Danger(Message.UnableToCancelReceipt);
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction(nameof(Details), "Registration");
                }
                else
                {
                    return Redirect(returnUrl);
                }
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var invoice = _db.Invoices.SingleOrDefault(x => x.Id == model.InvoiceId);
                    if (invoice != null)
                    {
                        invoice.IsPaid = false;
                    }

                    foreach (var item in model.ReceiptItems)
                    {
                        item.Type = "d";
                        var invoiceItem = _db.InvoiceItems.SingleOrDefault(x => x.Id == item.InvoiceItemId);
                        if (invoiceItem != null)
                        {
                            invoiceItem.IsPaid = false;
                            var registrationCourse = _db.RegistrationCourses.SingleOrDefault(x => x.Id == invoiceItem.RegistrationCourseId);
                            if (registrationCourse != null)
                            {
                                registrationCourse.IsPaid = false;
                            }
                        }
                    }

                    model.IsCancel = true;
                    model.IsActive = false;
                    _db.SaveChanges();

                    _scholarshipProvider.CancelTransactionFromReceipt(model.Id);
                    
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCancelReceipt);
                }
            }

            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction(nameof(Index), "Registration", new
                {
                    code = model.Student.Code,
                    tabIndex = "2"
                });
            }
            else
            {
                return Redirect(returnUrl);
            }
        }

        public ActionResult ReceiptPreview(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var userId = _userManager.GetUserId(User);
            var report = new ReportViewModel();
            var model = new ReceiptPreviewViewModel();
            var receipt = _receiptProvider.GetReceiptById(id);
            var receiptItems = new List<ReceiptItemDetail>();
            var user = GetUser();
            model = _mapper.Map<Receipt, ReceiptPreviewViewModel>(receipt);
            model.PrintedAt = DateTime.Now.ToString(StringFormat.ShortDate);
            model.PrintedBy = user == null ? "" : !string.IsNullOrEmpty(user.FirstnameTH) 
                                           ? $"{ user.FirstnameTH } { user.LastnameTH }"
                                           : !string.IsNullOrEmpty(user.FirstnameEN)
                                           ? $"{ user.FirstnameEN } { user.LastnameEN }"
                                           : user.UserName;
                                           
            model.TotalAmountTextTh = _receiptProvider.GetTotalAmountTh(model.TotalAmount);
            model.TotalAmountTextEn = _receiptProvider.GetTotalAmountEn(model.TotalAmount);
            model.InvoiceType = receipt.Invoice?.Type;

            if (receipt.ReceiptItems != null)
            {
                receipt.ReceiptItems = receipt.ReceiptItems.Where(x => x.TotalAmount != 0).ToList();

                var groupReceiptItem = from receiptItem in receipt.ReceiptItems
                                       group receiptItem by new
                                       {
                                           receiptItem.FeeItemId,
                                           receiptItem.FeeItem.NameTh,
                                           receiptItem.FeeItem.NameEn,
                                       } into invoiceItemGroup
                                       select new ReceiptItemDetail
                                       {
                                           FeeItemId = invoiceItemGroup.Key.FeeItemId ?? 0,
                                           NameTh = invoiceItemGroup.Key.NameTh,
                                           NameEn = invoiceItemGroup.Key.NameEn,
                                           Amount = invoiceItemGroup.Sum(x => x.TotalAmount).ToString(StringFormat.Money)
                                       };

                if (groupReceiptItem.Any(x => x.NameEn.Contains("Lump sum")))
                {
                    groupReceiptItem = groupReceiptItem.Where(x => x.FeeItemId != 1 && x.FeeItemId != 21 && x.FeeItemId != 25).ToList();
                }

                receiptItems.AddRange(groupReceiptItem.OrderBy(x => x.FeeItemId).ToList());

                model.ReceiptItemDetails = receiptItems;
            }

            try
            {
                var receiptForSave = _db.Receipts.FirstOrDefault(x => x.Id == receipt.Id);
                // Original receipt
                if (receiptForSave != null && receiptForSave.PrintedAt == null)
                {
                    receiptForSave.PrintedAt = DateTime.UtcNow;
                    receiptForSave.PrintedBy = userId;
                    receiptForSave.IsPrint = true;
                }
                else
                {
                    // Copy receipt
                    var receiptTransaction = new ReceiptPrintLog
                                            {
                                                ReceiptId = receipt.Id,
                                                PrintedAt = DateTime.UtcNow,
                                                PrintedBy = userId
                                            };

                    _db.ReceiptPrintLogs.Add(receiptTransaction);
                }

                _db.SaveChanges();
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCreate);
            }

            report.Body = model;

            return View(report);
        }

        public ActionResult Print(long id)
        {
            Receipt model = Find(id);
            if (model.IsPrint)
            {
                _flashMessage.Danger(Message.AlreadyPrinted);
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                
                model.IsPrint = true;
                model.PrintedBy = userId;
                model.PrintedAt = DateTime.UtcNow;
                try
                {
                    _db.SaveChanges();
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToDelete);
                }
            }

            return PartialView("~/Views/Registration/_print.cshtml", model);
        }

        public ActionResult PrintCopyReceipt(long id, string remark)
        {
            Receipt model = Find(id);
            try
            {
                ReceiptPrintLog transaction = new ReceiptPrintLog()
                {
                    ReceiptId = id,
                    Remark = remark,
                    PrintedAt = DateTime.UtcNow,
                    PrintedBy = "Officer"
                };
                _db.SaveChanges();
                return PartialView("~/Views/Registration/_printcopy.cshtml", model);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
                return RedirectToAction(nameof(Details), "Registration");
            }
        }

        private Receipt Find(long? id)
        {
            var receipt = _db.Receipts.Include(x => x.ReceiptItems)
                                          .ThenInclude(x => x.Invoice)
                                      .Include(x => x.ReceiptItems)
                                          .ThenInclude(x => x.InvoiceItem)
                                      .Include(x => x.Student)
                                      .SingleOrDefault(x => x.Id == id);
            receipt.ReceiptItems = receipt.ReceiptItems.OrderBy(x => x.FeeItemName)
                                                       .ToList();
            return receipt;
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public PartialViewResult RenderReceiptModal([FromBody] CourseRegistration courseRegistration)
        {
            var registeringCourses = MapRegistrationCouse(courseRegistration);
            var receiptItems = _receiptProvider.GetInvoiceItemsForPreview(registeringCourses, courseRegistration.RegistrationRound);
            var model = new ReceiptModalViewModel();
            if (receiptItems.Any())
            {
                model.ReceiptDetailViewModels = receiptItems.GroupBy(x => x.FeeItemName)
                                                            .Select(x => new ReceiptDetailViewModel
                                                                         {
                                                                             ItemId = x.First().FeeItemId,
                                                                             ItemTitle = x.Key,
                                                                             IsTermFee = string.IsNullOrEmpty(x.First().CourseCode),
                                                                             ReceiptModalItems = x.Select(y => new ReceiptModalItem
                                                                                                               {
                                                                                                                   CourseId = y.CourseId ?? 0,
                                                                                                                   CourseName = y.CourseName,
                                                                                                                   CourseAndCredit = y.CourseAndCredit,
                                                                                                                   FeeItemId = y.FeeItemId,
                                                                                                                   FeeItemName = y.FeeItemName,
                                                                                                                   Amount = y.Amount,
                                                                                                                   IsTermFee = string.IsNullOrEmpty(y.CourseCode),
                                                                                                                   ItemJsonString = JsonConvert.SerializeObject(y),
                                                                                                                   TotalAmount = y.TotalAmount,
                                                                                                                   ScholarshipAmount = y.ScholarshipAmount
                                                                                                               })
                                                                                                  .ToList()})
                                                            .ToList();

                var feeItems = model.ReceiptDetailViewModels.Select(x => new FeeItemViewModel
                                                                         {
                                                                             FeeItemId = x.ItemId,
                                                                             FeeItemName = x.ItemTitle
                                                                         })
                                                            .ToList();

                ViewBag.FeeItems = _selectListProvider.GetFeeItemFromReceiptPreview(feeItems);
            }
            
            return PartialView("~/Views/Registration/Receipt/_PreviewModalDetails.cshtml", model);
        }

        public ActionResult WaiveFee(ReceiptModalViewModel model)
        {
            return RedirectToAction(nameof(Index), "Registration");
        }

        private List<RegistrationCourse> MapRegistrationCouse(CourseRegistration courseRegistration)
        {
            var registrationCourses = courseRegistration.RegisteringCourses
                                                        .Select(x => new RegistrationCourse
                                                                     {
                                                                         StudentId = courseRegistration.StudentId,
                                                                         TermId = courseRegistration.RegistrationTermId,
                                                                         CourseId = x.CourseId,
                                                                         SectionId = x.SectionId,
                                                                         IsPaid = x.IsPaid
                                                                     })
                                                        .ToList();
            return registrationCourses;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public List<ReceiptModalItem> GetCourseByFeeItemId(long feeItemId, List<ReceiptModalItem> receiptItems)
        {
            var courses = receiptItems.Where(x => x.FeeItemId == feeItemId)
                                      .ToList();
            return courses;
        }
    }
}