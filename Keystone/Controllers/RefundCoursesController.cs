using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Scholarship;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("RefundCourses", PolicyGenerator.Write)]
    public class RefundCoursesController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly IReceiptProvider _receiptProvider;
        protected readonly IScholarshipProvider _scholarshipProvider;
        
        public RefundCoursesController(ApplicationDbContext db,
                                       IFlashMessage flashMessage,
                                       ISelectListProvider selectListProvider,
                                       IStudentProvider studentProvider,
                                       IReceiptProvider receiptProvider,
                                       IScholarshipProvider scholarshipProvider) : base(db, flashMessage, selectListProvider)
        {
            _studentProvider = studentProvider;
            _receiptProvider = receiptProvider;
            _scholarshipProvider = scholarshipProvider;
        }

        public IActionResult Index(long academicLevelId, long termId, string studentCode)
        {
            CreateSelectList(academicLevelId);
            var model = new RefundCoursesViewModel();
            if (termId != 0 && !String.IsNullOrWhiteSpace(studentCode))
            {
                var student = _studentProvider.GetStudentByCode(studentCode);
                if (student != null)
                {
                    ViewBag.TermFeeItems = _selectListProvider.GetTermFeeItemsByStudentAndTermId(student.Id, termId);
                    ViewBag.Courses = _selectListProvider.GetReceiptsCourses(student.Id, termId);
                    model.AcademicLevelId = academicLevelId;
                    model.TermId = termId;
                    model.StudentId = student.Id;
                    model.StudentCode = studentCode;
                    model.StudentName = student.FullNameEn;
                    var scholarships = _scholarshipProvider.GetScholarshipStudents(student.Id);
                    model.ScholarshipName = scholarships == null ? "N/A"
                                                                 : string.Join(",", scholarships.Select(x => x.Scholarship.NameEn)
                                                                                                .Distinct()
                                                                                                .ToList());
                    
                    var receiptItems = _db.ReceiptItems.Include(x => x.Receipt)
                                                         .Include(x => x.InvoiceItem)
                                                             .ThenInclude(x => x.Section)
                                                         .Include(x => x.InvoiceItem)
                                                             .ThenInclude(x => x.Course)
                                                         .Include(x => x.FeeItem)
                                                         .Where(x => x.Receipt.StudentId == student.Id
                                                                     && x.Receipt.TermId == termId)
                                                         .ToList();
                    
                    model.ReceiptItems = receiptItems.Where(x => x.RemainingAmount > 0)
                                                     .ToList();

                    model.NoRemainningReceiptItems = receiptItems.Where(x => x.RemainingAmount <= 0)
                                                                 .ToList();
                    
                    // re-calculare remaining personal pay and scholarship pay
                    model.ReceiptItems.ForEach(x => 
                    {
                        var refund = _db.Refunds.Where(y => y.ReceiptItemId == x.Id);
                        x.RemainingPersonalPayAmount = x.TotalAmount - refund.Sum(y => y.Amount - y.ScholarshipAmount);
                        x.RemainingScholarshipAmount = x.ScholarshipAmount - refund.Sum(y => y.ScholarshipAmount);
                    });

                    return View(model);
                }
                else
                {
                    _flashMessage.Danger(Message.StudentNotFound);
                    return View(model);
                }
            }
            
            return View(model);
        }
        
        [HttpPost]
        public IActionResult RefundCourses(RefundCoursesViewModel model)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    List<FinancialTransaction> transactions = new List<FinancialTransaction>();
                    foreach (var item in model.ReceiptItems.Where(x => x.IsRefund))
                    {
                        var invoice = _db.Invoices.SingleOrDefault(x => x.Id == item.InvoiceId);
                        var invoiceItem = _db.InvoiceItems.SingleOrDefault(x => x.Id == item.InvoiceItemId);
                        invoiceItem.Type = "rf";

                        var receiptItem = _db.ReceiptItems.SingleOrDefault(x => x.Id == item.Id);
                        var refundAmount = Math.Min(receiptItem.RemainingAmount, item.RefundAmount ?? 0);
                        receiptItem.RemainingAmount -= refundAmount;

                        var refund = new Refund
                        {
                            RegistrationCourseId = invoiceItem.RegistrationCourseId,
                            ReceiptId = receiptItem.ReceiptId,
                            ReceiptItemId = receiptItem.Id,
                            RefundPercentage = item.RefundPercentage,
                            Amount = item.RefundAmount ?? 0,
                            ScholarshipAmount = item.RefundScholarshipAmount,
                            RefundedAt = item.RefundedAt
                        };

                        _db.Refunds.Add(refund);

                        if (item.IsReturnBudget && invoiceItem.ScholarshipStudentId > 0)
                        {
                            transactions.Add(new FinancialTransaction
                                             {
                                                 StudentId = invoice.StudentId ?? Guid.Empty,
                                                 StudentScholarshipId = invoiceItem.ScholarshipStudentId ?? 0,
                                                 TermId = invoice.TermId ?? 0,
                                                 ReceiptId = receiptItem.ReceiptId,
                                                 ReceiptItemId = receiptItem.Id,
                                                 PaidAt = item.RefundedAt ?? DateTime.UtcNow,
                                                 PersonalPay = -item.RefundPersonalPayAmount,
                                                 UsedScholarship = -item.RefundScholarshipAmount,
                                                 Type = "r"
                                             });
                        }
                    }

                    _db.SaveChanges();

                    if (transactions.Any())
                    {
                        _scholarshipProvider.CreateTransactions(transactions);
                    }

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToSave);
                }
            }

            return RedirectToAction(nameof(Index), new
                                                   {
                                                        StudentCode = model.StudentCode,
                                                        AcademicLevelId = model.AcademicLevelId, 
                                                        TermId = model.TermId
                                                   });
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.Percentages = _selectListProvider.GetPercentages();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }

        public List<RefundDetail> GetInvoiceCourseFeeItem(Guid studentId, long termId, long courseId, long registrationId)
        {
            var items = _receiptProvider.GetInvoiceCourseFeeItem(studentId, termId, courseId, registrationId);
            return items;
        }
    }
}