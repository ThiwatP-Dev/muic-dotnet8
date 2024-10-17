using AutoMapper;
using KeystoneLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Authorization;
using KeystoneLibrary.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vereyon.Web;
using Keystone.Permission;

namespace Keystone.Controllers
{
    public class InvoiceController : BaseController
    {
        protected readonly IReceiptProvider _receiptProvider;

        public InvoiceController(ApplicationDbContext db,
                                 IMapper mapper,
                                 IReceiptProvider receiptProvider,
                                 IHttpContextAccessor accessor,
                                 IFlashMessage flashMessage) : base(db, flashMessage, mapper)
        {
            _receiptProvider = receiptProvider;
        }

        public ActionResult InvoiceDetails(long id)
        {
            var model = _receiptProvider.GetInvoice(id);
            return PartialView("~/Views/Registration/Invoice/_Details.cshtml", model);
        }

        public ActionResult InvoicePartModalDetails(long id)
        {
            var model = _receiptProvider.GetInvoice(id);
            return PartialView("~/Views/Registration/Invoice/_ConfirmPaymentCompleteDetailModal.cshtml", model);
        }

        public ActionResult InvoicePreview(long id, string returnUrl)
        {
            System.Globalization.CultureInfo _cultureENInfo = new System.Globalization.CultureInfo("en-EN");
            ViewBag.ReturnUrl = returnUrl;
            var report = new ReportViewModel();
            var invoice = new InvoiceViewModel();
            var invoiceItems = new List<InvoiceItemDetail>();
            var model = _receiptProvider.GetInvoice(id);
            invoice = _mapper.Map<Invoice, InvoiceViewModel>(model);

            invoice.IsAddDrop = model.IsAddDrop;
            invoice.IsLateRegis = model.IsLateRegis;

            invoice.PrintedAt = DateTime.Now.ToString("d MMMM yyyy", _cultureENInfo);
            var registrationTerm = _db.RegistrationTerms.SingleOrDefault(x => x.TermId == model.TermId && x.Type == "p");
            if (registrationTerm != null)
            {
                invoice.StartedDate = registrationTerm.StartedAt?.ToString("d MMMM yyyy", _cultureENInfo) ?? "N/A";
                invoice.EndedDate = registrationTerm.EndedAt?.ToString("d MMMM yyyy", _cultureENInfo) ?? "N/A";
            }
            var term = _db.Terms.SingleOrDefault(x => x.Id == model.TermId);
            if (term != null)
            {
                if (string.IsNullOrEmpty(invoice.StartedDate) || "N/A".Equals(invoice.StartedDate))
                {
                    invoice.StartedDate = term.StartedAt?.ToString("d MMMM yyyy", _cultureENInfo) ?? "N/A";
                }
                if ((invoice.IsAddDrop.HasValue && invoice.IsAddDrop.Value) || (invoice.IsLateRegis.HasValue && invoice.IsLateRegis.Value))
                {
                    invoice.EndedDate = term.AddDropPaymentEndedAt?.ToString("d MMMM yyyy", _cultureENInfo) ?? "N/A";
                }
                else
                {
                    invoice.EndedDate = term.FirstRegistrationPaymentEndedAt?.ToString("d MMMM yyyy", _cultureENInfo) ?? "N/A";
                }
            }

            if (model.InvoiceItems != null)
            {
                //var groupInvoiceItem = from invoiceItem in model.InvoiceItems
                //                       group invoiceItem by new
                //                       {
                //                           invoiceItem.FeeItem.Code,
                //                           invoiceItem.FeeItem.NameEn,
                //                       } into invoiceItemGroup
                //                       select new InvoiceItemDetail
                //                       {
                //                           Code = invoiceItemGroup.Key.Code,
                //                           Name = invoiceItemGroup.Key.NameEn,
                //                           Amount = invoiceItemGroup.Sum(x => x.TotalAmount).ToString(StringFormat.Money)
                //                       };
                //invoiceItems.AddRange(groupInvoiceItem);

                foreach (var invoiceItem in model.InvoiceItems)
                {                    
                    invoiceItems.Add(new InvoiceItemDetail
                    {
                        FeeItemId = invoiceItem.FeeItemId,
                        Code = invoiceItem.FeeItem.Code,
                        Name = invoiceItem.FeeItemId == 1 && invoiceItem.Course != null ?
                                               $"{invoiceItem.FeeItemName} {invoiceItem.Course.CodeAndCredit}[{invoiceItem.Section.Number}] {(invoice.IsAddDrop.HasValue && invoice.IsAddDrop.Value ? (invoiceItem.TotalAmount < 0 ? "(Drop)" : "(Add)") : "")}"
                                               : invoiceItem.FeeItemName,
                        Amount = invoiceItem.TotalAmount.ToString(StringFormat.Money)
                    });
                }
                if (invoiceItems.Any(x => x.Name.Contains("Lump sum")))
                {
                    invoiceItems = invoiceItems.Where(x => x.FeeItemId != 1 && x.FeeItemId != 21 && x.FeeItemId != 25).ToList();
                }

                invoice.InvoiceItems = invoiceItems;
            }
            if (!string.IsNullOrEmpty(invoice.AllDiscountAmountText))
            {
                invoice.AllDiscountAmountText = $"({invoice.AllDiscountAmountText})";
            }

            report.Body = invoice;
            return View(report);
        }

        [PermissionAuthorize("Registration", PolicyGenerator.Write)]
        public ActionResult DeleteFromRegistration(long id, string remark, string returnUrl)
        {
            var invoice = _db.Invoices.AsNoTracking()
                                      .Include(x => x.Student)
                                      .Include(x => x.Term)
                                      .FirstOrDefault(x => x.Id == id);
            if (invoice == null)
            {
                _flashMessage.Danger(Message.DataNotFound);
                return Redirect(returnUrl);
            }
            try
            {
                _receiptProvider.DeleteInvoice(id, true, remark);
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch (Exception ex)
            {
                _flashMessage.Danger(ex.Message);
            }
            return RedirectToAction("Index", "Registration", new
            {
                code = invoice.Student.Code,
                academicLevelId = invoice.Term.AcademicLevelId,
                termId = invoice.TermId,
                tabIndex = 2
            });
        }

        [PermissionAuthorize("Registration", PolicyGenerator.Write)]
        public async Task<ActionResult> MarkInvoicePaidFromRegistration(long id, string returnUrl)
        {
            var invoice = _db.Invoices.AsNoTracking()
                                      .Include(x => x.Student)
                                      .Include(x => x.Term)
                                      .Include(x => x.InvoiceItems)
                                      .FirstOrDefault(x => x.Id == id);
            if (invoice == null)
            {
                _flashMessage.Danger(Message.DataNotFound);
                return Redirect(returnUrl);
            }
            if (!invoice.IsCancel && !invoice.IsPaid && invoice.TotalAmount != 0)
            {
                _flashMessage.Danger(Message.RequiredData);
                return RedirectToAction("Index", "Registration", new
                {
                    code = invoice.Student.Code,
                    academicLevelId = invoice.Term.AcademicLevelId,
                    termId = invoice.TermId,
                    tabIndex = 2
                });
            }

            //TODO: Payment Method config?
            var paymentMethod = _db.PaymentMethods.FirstOrDefault(x => x.NameEn == "Cash");
            if (paymentMethod == null)
            {
                _flashMessage.Danger("Not Have Payment Method");
                return RedirectToAction("Index", "Registration", new
                {
                    code = invoice.Student.Code,
                    academicLevelId = invoice.Term.AcademicLevelId,
                    termId = invoice.TermId,
                    tabIndex = 2
                });
            }

            FinanceRegistrationFeeViewModel financeRegistrationFeeViewModel = new FinanceRegistrationFeeViewModel();
            financeRegistrationFeeViewModel.Invoice = new RegistrationFeeInvoice();
            financeRegistrationFeeViewModel.Invoice.InvoiceItems = new List<RegistrationFeeInvoiceItem>();
            foreach (var item in invoice.InvoiceItems)
            {
                financeRegistrationFeeViewModel.Invoice.InvoiceItems.Add(new RegistrationFeeInvoiceItem
                {
                    Id = item.Id,
                    IsChecked = true,
                });
            }
            financeRegistrationFeeViewModel.InvoiceId = invoice.Id;
            financeRegistrationFeeViewModel.UpdatedBy = GetUser().Id;
            financeRegistrationFeeViewModel.PaymentMethods = new List<ReceiptPaymentMethod> { 
                new ReceiptPaymentMethod
                {
                    Amount = 0,
                    PaymentMethodId = paymentMethod.Id,
                } 
            }
            ;

            try
            {           
                var resultStr = await _receiptProvider.ProcessInvoicePaymentManualAsync(financeRegistrationFeeViewModel);
                if (string.IsNullOrEmpty(resultStr))
                {
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                else
                {
                    _flashMessage.Danger(Message.UnableToCreate + " " + resultStr);
                }
            }
            catch (Exception ex)
            {
                _flashMessage.Danger(ex.Message);
            }
            return RedirectToAction("Index", "Registration", new
            {
                code = invoice.Student.Code,
                academicLevelId = invoice.Term.AcademicLevelId,
                termId = invoice.TermId,
                tabIndex = 2
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveInvoicePreview(InvoiceViewModel model)
        {
            var invoice = _receiptProvider.GetInvoice(model.InvoiceId);
            var userId = _accessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                if (invoice.PrintedAt == null)
                {
                    invoice.PrintedAt = DateTime.Now;
                    invoice.PrintedBy = userId;
                    invoice.IsPrint = true;
                }
                else
                {
                    var printingLog = new InvoicePrintLog
                    {
                        InvoiceId = invoice.Id,
                        PrintedAt = DateTime.Now,
                        PrintedBy = userId
                    };
                    _db.InvoicePrintLogs.Add(printingLog);
                }

                _flashMessage.Confirmation(Message.SaveSucceed);
                _db.SaveChanges();
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCreate);
            }

            return RedirectToAction("Index", "Registration", new
            {
                code = model.StudentCode,
                academicLevelId = model.AcademicLevelId,
                termId = model.TermId,
                tabIndex = 2
            });
        }

        [AllowAnonymous]
        [Route("Invoice/Update")]
        [HttpPost]
        public async Task<ActionResult> UpdateStatusPayment([FromForm] InvoiceUpdateIsPaidViewModel model)
        {
            if (ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var invoice = await _db.Invoices.SingleOrDefaultAsync(x => x.Number == model.OrderId
                                                                               && x.Reference1 == model.Reference1
                                                                               && x.Amount == model.Amount
                                                                               && x.Reference2 == model.Reference2);

                    if (invoice == null)
                    {
                        return Error(InvoiceException.OrderNotFound());
                    }
                    invoice.IsPaid = model.IsPaymentSuccess;
                    _db.SaveChanges();
                    transaction.Commit();
                    return Success(null);
                }
                catch
                {
                    transaction.Rollback();
                    return Error(InvoiceException.UnableSaveUpdateIsPaid());
                }
            }
        }
    }
}