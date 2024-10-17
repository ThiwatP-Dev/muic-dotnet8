using Microsoft.AspNetCore.Mvc;
using KeystoneLibrary.Models;
using KeystoneLibrary.Data;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.Report;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace Keystone.Controllers.Report
{
    public class PaymentLogReportController : BaseController
    {
        public PaymentLogReportController(ApplicationDbContext db,
                                          IFlashMessage flashMessage,
                                          ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public ActionResult Index(Criteria criteria) 
        {
            CreateSelectList();
            if (criteria.UpdatedFrom == null || criteria.UpdatedTo == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var responses = (from response in _db.BankPaymentResponses
                             join invoice in _db.Invoices.IgnoreQueryFilters() on response.Number equals invoice.Number into invoices 
                             from invoice in invoices.DefaultIfEmpty()
                             where response.TransactionDateTime >= criteria.UpdatedFrom.Value.Date
                                   && response.TransactionDateTime < criteria.UpdatedTo.Value.AddDays(1).Date
                                   && (string.IsNullOrEmpty(criteria.ReferenceNumber) || response.Reference1.Contains(criteria.ReferenceNumber))
                                   && (string.IsNullOrEmpty(criteria.InvoiceNumber) || (response.InvoiceId > 0 && invoice.Number.Contains(criteria.InvoiceNumber)))
                                   && (string.IsNullOrEmpty(criteria.IsPaymentSucceeded) || response.IsPaymentSuccess == Convert.ToBoolean(criteria.IsPaymentSucceeded))
                             select new PaymentLogReportViewModel
                                    {
                                        Id = response.Id,
                                        Number = response.Number,
                                        InvoiceIsCancel = invoice != null ? invoice.IsCancel : null,
                                        InvoiceIsPaid = invoice != null ? invoice.IsPaid : null,
                                        Reference1 = response.Reference1,
                                        Reference2 = response.Reference2,
                                        TotalAmount = response.TotalAmount,
                                        PaidAmount = response.PaidAmount,
                                        IsPaymentSuccess = response.IsPaymentSuccess,
                                        TransactionAt = response.TransactionDateTime
                                    }).GetAllPaged(criteria);

            return View(responses);
        }

        public ActionResult Details(long id)
        {
            var responses = from response in _db.BankPaymentResponses
                            join invoice in _db.Invoices.IgnoreQueryFilters() on response.Number equals invoice.Number into invoices 
                            from invoice in invoices.DefaultIfEmpty()
                            where response.Id == id
                            select new
                                   {
                                       InvoiceNumber = invoice.Number,
                                       InvoiceIsCancel = invoice != null ? invoice.IsCancel : (bool?)null,
                                       InvoiceIsPaid = invoice != null ? invoice.IsPaid : (bool?)null,
                                       response.Reference1,
                                       response.Reference2,
                                       response.TotalAmount,
                                       response.PaidAmount,
                                       response.TransactionDateTime,
                                       response.RawResponse
                                   };

            if (responses == null)
                return PartialView("_Details", null);                             

            var result = responses.FirstOrDefault();
            var model = new PaymentLogReportViewModel
            {
                InvoiceNumber = result.InvoiceNumber,
                InvoiceIsCancel = result.InvoiceIsCancel,
                InvoiceIsPaid = result.InvoiceIsPaid,
                Reference1 = result.Reference1,
                Reference2 = result.Reference2,
                TotalAmount = result.TotalAmount,
                PaidAmount = result.PaidAmount,
                TransactionAt = result.TransactionDateTime
            };  

            if (!string.IsNullOrEmpty(result.RawResponse))
            {
                if (result.RawResponse.StartsWith("Manual Update By"))
                {
                    var guid = result.RawResponse.Replace("Manual Update By ", string.Empty);
                    if (Guid.TryParse(guid, out Guid userId))
                    {
                        var user = _db.Users.SingleOrDefault(x => x.Id == userId.ToString());
                        model.Response = $"Manual Update By { user.FullnameEN }";
                    }
                }
                else if (result.RawResponse.StartsWith("MANUAL"))
                {
                    model.Response = result.RawResponse;
                }
                else
                {
                    model.ResponseObject = JsonConvert.DeserializeObject(result.RawResponse);
                }
            }

            return PartialView("_Details", model);
        }

        private void CreateSelectList()
        {
            ViewBag.AllYesNoAnswer = _selectListProvider.GetAllYesNoAnswer();
        }
    }
}