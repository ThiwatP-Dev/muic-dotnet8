using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("Registration", PolicyGenerator.Write)]
    public class WaiveController : BaseController
    {
        protected readonly IReceiptProvider _receiptProvider;
        protected readonly IRegistrationProvider _registrationProvider;

        public WaiveController(ApplicationDbContext db,
                               IFlashMessage flashMessage,
                               IReceiptProvider receiptProvider,
                               IRegistrationProvider registrationProvider) : base(db, flashMessage) 
        {
            _receiptProvider = receiptProvider;
            _registrationProvider = registrationProvider;
        }

        public IActionResult Index(long? id)
        {
            var model = _db.Invoices.Include(x => x.InvoiceItems)
                                        .ThenInclude(x => x.FeeItem)
                                    .Include(x => x.InvoiceItems)
                                        .ThenInclude(x => x.Course)
                                    .Include(x => x.InvoiceItems)
                                        .ThenInclude(x => x.Section)
                                    .Include(x => x.Student)
                                        .ThenInclude(x => x.Title)
                                    .Include(x => x.Student)
                                        .ThenInclude(x => x.StudentFeeGroup)
                                    .Include(x => x.Student)
                                        .ThenInclude(x => x.StudentFeeType)
                                    .Include(x => x.Student)
                                        .ThenInclude(x => x.AdmissionInformation)
                                        .ThenInclude(x => x.AdmissionType)
                                    .Include(x => x.Term)
                                    .SingleOrDefault(x => x.Id == id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(WaiveViewModel model)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var invoice = _db.Invoices.Include(x => x.Student)
                                              .Include(x => x.Term)
                                              .SingleOrDefault(x => x.Id == model.Id);
                    var originalTotalAmont = invoice.TotalAmount;
                    if (invoice.IsPaid)
                    {
                        _flashMessage.Danger("Cannot edit Invoice - is already paid");
                        throw new System.Exception();
                    }

                    invoice.DiscountRemark = model.DiscountRemark;
                    foreach (var item in model.InvoiceItems)
                    {
                        var invoiceItem = _db.InvoiceItems.SingleOrDefault(x => x.Id == item.Id);
                        invoiceItem.TotalAmount = invoiceItem.Amount - invoiceItem.ScholarshipAmount;
                        //if (invoiceItem.TotalAmount < item.DiscountAmount)
                        //{
                        //    _flashMessage.Danger("Item Waive exceed total amount");
                        //    throw new System.Exception();
                        //}
                        //if (invoiceItem.IsPaid && item.DiscountAmount != invoiceItem.DiscountAmount)
                        //{
                        //    _flashMessage.Danger("Cannot edit The Invoice Item that is already paid");
                        //    throw new System.Exception();
                        //}
                        invoiceItem.DiscountAmount = item.DiscountAmount;
                        invoiceItem.DiscountRemark = item.DiscountRemark;
                        invoiceItem.TotalAmount -= invoiceItem.DiscountAmount;
                        _db.SaveChanges();
                    }
                   
                    invoice.TotalItemsDiscount = _db.InvoiceItems.Where(x => x.InvoiceId == model.Id)
                                                                                .Sum(x => x.DiscountAmount);
                    var crDeductTran = _db.InvoiceDeductTransactions.Where(x => x.InvoiceId == model.Id).ToList();
                    if (crDeductTran != null && crDeductTran.Count > 0) 
                    {
                        //credit note deduct is already in invoice.TotalDeductAmount so don't include it as discount again
                        // [but yeah invoice item doesn't have field for it so they come and use that invoiceItem.DiscountAmount]
                        invoice.TotalItemsDiscount -= crDeductTran.Sum(x => x.Amount); 
                    }

                    if(invoice.Type == "cr")
                    {
                        invoice.TotalAmount = invoice.Amount - invoice.ScholarshipAmount - invoice.TotalItemsDiscount + invoice.TotalDeductAmount;
                    }
                    else
                    {
                        invoice.TotalAmount = invoice.Amount - invoice.ScholarshipAmount - invoice.TotalItemsDiscount - invoice.TotalDeductAmount;
                    }
                    //if (invoice.TotalAmount < model.TotalDiscount)
                    //{
                    //    _flashMessage.Danger("Invoice Discount exceed total amount");
                    //    throw new System.Exception();
                    //}
                    invoice.TotalDiscount = model.TotalDiscount;
                    invoice.TotalAmount -= invoice.TotalDiscount;
                    if (invoice.Type == "cr" && originalTotalAmont != invoice.TotalAmount)
                    {
                        invoice.IsConfirm = false;
                    }
                    _db.SaveChanges();
                    _receiptProvider.UpdateInvoiceReference(invoice, invoice.Student.Code);
                    //New Version USpark will pull invoice from us to no need to update them.
                    //_registrationProvider.CallUSparkServiceAPIWaiveInvoice(invoice.Id);

                    transaction.Commit();

                    return RedirectToAction(nameof(Index), "Registration", new
                    {
                        code = invoice.Student.Code,
                        academicLevelId = invoice.Term.AcademicLevelId,
                        termId = invoice.TermId
                    });
                }
                catch
                {
                    transaction.Rollback();
                    return RedirectToAction(nameof(Index), new { id = model.Id });
                }
            }
           
        }
    }
}