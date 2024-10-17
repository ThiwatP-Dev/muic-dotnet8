using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("WaiveGroup", "")]
    public class WaiveGroupController : BaseController
    {
        protected readonly IReceiptProvider _receiptProvider;

        public WaiveGroupController(ApplicationDbContext db,
                                    ISelectListProvider selectListProvider,
                                    IFlashMessage flashMessage,
                                    IReceiptProvider receiptProvider) : base(db, flashMessage, selectListProvider) 
        {
            _receiptProvider = receiptProvider;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var model = _db.InvoiceItems.Include(x => x.Invoice)
                                            .ThenInclude(x => x.Student)
                                        .Include(x => x.FeeItem)
                                        .Include(x => x.Course)
                                        .Include(x => x.Section)
                                        .Where(x => x.DiscountAmount > 0
                                                    && x.Invoice.TermId == criteria.TermId
                                                    && !x.Invoice.IsCancel
                                                    && x.Invoice.IsActive
                                                    && (criteria.FeeItemId == 0 
                                                        || x.FeeItemId == criteria.FeeItemId)
                                                    && (string.IsNullOrEmpty(criteria.Code)
                                                        || x.Invoice.Student.Code.StartsWith(criteria.Code)))
                                        .Select(x => new WaiveGroupViewModel
                                                     {
                                                         StudentCode = x.Invoice.Student.Code,
                                                         StudentFullNameEn = x.Invoice.Student.FullNameEn,
                                                         InvoiceNumber = x.Invoice.Number,
                                                         ReceiptNumber = _db.Receipts.FirstOrDefault(y => y.InvoiceId == x.InvoiceId).Number,
                                                         FeeItemNameEn = x.FeeItem.NameEn,
                                                         CourseNameEn = x.Course.NameEn,
                                                         SectionNumber = x.Section.Number,
                                                         Amount = x.Amount,
                                                         DiscountAmount = x.DiscountAmount
                                                     })
                                        .OrderBy(x => x.StudentCode)
                                        .ThenBy(x => x.InvoiceNumber)
                                        .GetPaged(criteria, page);
            return View(model);
        }

        [PermissionAuthorize("WaiveGroup", PolicyGenerator.Write)]
        public IActionResult Create(Criteria criteria, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.CourseId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0 || criteria.FeeItemId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var results = _db.InvoiceItems.Include(x => x.Invoice)
                                              .ThenInclude(x => x.Student)
                                          .Include(x => x.Course)
                                          .Include(x => x.Section)
                                          .Where(x => x.TotalAmount > 0
                                                      && !x.Invoice.IsPaid
                                                      && !x.Invoice.IsCancel
                                                      && x.Invoice.IsActive
                                                      && x.Invoice.TermId == criteria.TermId
                                                      && x.FeeItemId == criteria.FeeItemId
                                                      && (criteria.CourseId == 0
                                                          || x.CourseId == criteria.CourseId)
                                                      && (criteria.SectionId == 0
                                                          || x.SectionId == criteria.SectionId))
                                          .ToList();

            var model = new WaiveStudentViewModel
                        {
                            Criteria = criteria,
                            Results = results
                        };

            return View(model);
        }

        [PermissionAuthorize("WaiveGroup", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(WaiveStudentViewModel model, decimal waiveAmount, string waiveRemark, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.Criteria.AcademicLevelId, model.Criteria.TermId, model.Criteria.CourseId);
            var obj = new 
                      { 
                          AcademicLevelId = model.Criteria.AcademicLevelId,
                          TermId = model.Criteria.TermId,
                          FeeItemId = model.Criteria.FeeItemId,
                          CourseId = model.Criteria.CourseId,
                          SectionId = model.Criteria.SectionId,
                          ReturnUrl = returnUrl
                      };

            if (waiveAmount > 0)
            {
                var invoiceItems = model.Results.Where(x => x.IsSelected == "on")
                                                .ToList();
                if (invoiceItems.Any(x => waiveAmount > x.TotalAmount + x.DiscountAmount))
                {
                    _flashMessage.Danger("Waive amount exceed possible amount.");
                    return RedirectToAction(nameof(Create), obj);
                }

                if (invoiceItems.Any())
                {
                    using (var transaction = _db.Database.BeginTransaction())
                    {
                        try
                        {
                            var selectedInvoiceItemId = invoiceItems.Select(x => x.Id).ToList();
                            var allInvoiceItems = _db.InvoiceItems.Include(x => x.Invoice)
                                                                      .ThenInclude(x => x.Student)
                                                                  .Where(x => x.TotalAmount > 0
                                                                              && !x.Invoice.IsPaid
                                                                              && !x.Invoice.IsCancel
                                                                              && x.Invoice.IsActive
                                                                              && selectedInvoiceItemId.Contains(x.Id))
                                                                  .ToList();
                            if (selectedInvoiceItemId.Count != allInvoiceItems.Count())
                            {
                                transaction.Rollback();
                                _flashMessage.Danger("Problem data not update, please restart the process to retreive latest data.");
                                return RedirectToAction(nameof(Create), obj);
                            }

                            foreach (var invoiceItem in allInvoiceItems)
                            {
                                invoiceItem.TotalAmount = invoiceItem.Amount - invoiceItem.ScholarshipAmount;
                                invoiceItem.DiscountAmount = waiveAmount;
                                invoiceItem.DiscountRemark = waiveRemark;
                                invoiceItem.TotalAmount -= invoiceItem.DiscountAmount;
                                _db.SaveChanges();

                                var originalTotalAmont = invoiceItem.Invoice.TotalAmount;

                                invoiceItem.Invoice.TotalItemsDiscount = _db.InvoiceItems.Where(x => x.InvoiceId == invoiceItem.InvoiceId)
                                                                                         .Sum(x => x.DiscountAmount);
                                var crDeductTran = _db.InvoiceDeductTransactions.Where(x => x.InvoiceId == invoiceItem.InvoiceId).ToList();
                                if (crDeductTran != null && crDeductTran.Count > 0)
                                {
                                    //credit note deduct is already in invoice.TotalDeductAmount so don't include it as discount again
                                    // [but yeah invoice item doesn't have field for it so they come and use that invoiceItem.DiscountAmount]
                                    invoiceItem.Invoice.TotalItemsDiscount -= crDeductTran.Sum(x => x.Amount);
                                }

                                if (invoiceItem.Invoice.Type == "cr")
                                {
                                    invoiceItem.Invoice.TotalAmount = invoiceItem.Invoice.Amount 
                                                                            - invoiceItem.Invoice.ScholarshipAmount 
                                                                            - invoiceItem.Invoice.TotalItemsDiscount 
                                                                            + invoiceItem.Invoice.TotalDeductAmount;
                                }
                                else
                                {
                                    invoiceItem.Invoice.TotalAmount = invoiceItem.Invoice.Amount 
                                                                            - invoiceItem.Invoice.ScholarshipAmount 
                                                                            - invoiceItem.Invoice.TotalItemsDiscount 
                                                                            - invoiceItem.Invoice.TotalDeductAmount;
                                }

                                if (invoiceItem.Invoice.Type == "cr" && originalTotalAmont != invoiceItem.Invoice.TotalAmount)
                                {
                                    invoiceItem.Invoice.IsConfirm = false;
                                }
                                _db.SaveChanges();
                                _receiptProvider.UpdateInvoiceReference(invoiceItem.Invoice, invoiceItem.Invoice.Student.Code);
                            }

                            transaction.Commit();
                            _flashMessage.Confirmation(Message.SaveSucceed);
                            return Redirect(returnUrl);
                        }
                        catch
                        {
                            transaction.Rollback();
                            _flashMessage.Danger(Message.UnableToEdit);
                            return RedirectToAction(nameof(Create), obj);
                        }
                    }
                }
                
                _flashMessage.Danger(Message.AtLeastOneInvoiceItem);
                return RedirectToAction(nameof(Create), obj);
            }

            _flashMessage.Warning(Message.RequiredData);
            return RedirectToAction(nameof(Create), obj);
        }

        private void CreateSelectList(long academicLevelId = 0, long termId = 0, long courseId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.FeeItems = _selectListProvider.GetFeeItems();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                if (termId > 0)
                {
                    ViewBag.Courses = _selectListProvider.GetCoursesByAcademicLevelAndTerm(academicLevelId, termId);
                    if (courseId > 0)
                    {
                        ViewBag.Sections = _selectListProvider.GetSections(termId, courseId);
                    }
                }
            }
        }
    }
}