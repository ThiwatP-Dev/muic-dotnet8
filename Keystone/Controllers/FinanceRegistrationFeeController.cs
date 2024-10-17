using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("FinanceRegistrationFee", PolicyGenerator.Write)]
    public class FinanceRegistrationFeeController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IMasterProvider _masterProvider;
        protected readonly IFeeProvider _feeProvider;
        protected readonly IReceiptProvider _receiptProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IScholarshipProvider _scholarshipProvider;

        public FinanceRegistrationFeeController(ApplicationDbContext db,
                                                IFlashMessage flashMessage,
                                                IMapper mapper,
                                                ISelectListProvider selectListProvider,
                                                IStudentProvider studentProvider,
                                                IDateTimeProvider dateTimeProvider,
                                                IMasterProvider masterProvider,
                                                IFeeProvider feeProvider,
                                                IReceiptProvider receiptProvider,
                                                IRegistrationProvider registrationProvider,
                                                IScholarshipProvider scholarshipProvider) : base(db, flashMessage, mapper, selectListProvider) 
        {
            _studentProvider = studentProvider;
            _dateTimeProvider = dateTimeProvider;
            _masterProvider = masterProvider;
            _feeProvider = feeProvider;
            _receiptProvider = receiptProvider;
            _registrationProvider = registrationProvider;
            _scholarshipProvider = scholarshipProvider;
        }

        public ActionResult Index(string code, long termId, long invoiceId)
        {
            var model = new FinanceRegistrationFeeViewModel
                        {
                            Code = code,
                            TermId = termId,
                            InvoiceId = invoiceId
                        };

            if (string.IsNullOrEmpty(code) || termId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var student = _db.Students.FirstOrDefault(x => x.Code == code);
            
            if (termId != 0 && invoiceId != 0)
            {
                model.Invoice = _feeProvider.GetRegistrationFeeInvoice(invoiceId);

                if (model.Invoice == null)
                {
                    _flashMessage.Warning(Message.DataNotFound);
                    return View();
                }

                //// Amount for paid (not included scholarship)
                //var amount = model.Invoice.InvoiceItems.Where(x => x.Type != "Delete")
                //                                                  .Sum(x => x.TotalAmount);

                model.TotalDiscountAmountText = model.Invoice.TotalDiscount.ToString(StringFormat.Money);
                model.TotalAmountText = (model.Invoice.TotalAmount).ToString(StringFormat.Money);
            }
            //else
            //{
            //    if (student != null)
            //    {
            //        _registrationProvider.GetRegistrationCourseFromUspark(student.Id, termId, out newCourses, out deleteUnpaidCourses, out deletePaidCourses);
            //        _receiptProvider.Checkout(student.Id, termId, newCourses, deletePaidCourses);
            //    }
            //}
            CreateSelectList(code, termId);

            return View(model);
        }

        public async Task<ActionResult> Create(FinanceRegistrationFeeViewModel model)
        {
            bool isError = false;
            decimal selectedAmount = 0;
            //if(model.Invoice.InvoiceItems.Any(x => x.IsChecked))
            //{
            //    selectedAmount  = model.Invoice.InvoiceItems.Where(x => x.IsChecked).Sum(x => x.TotalAmount);
            //}
            foreach (var item in model.Invoice.InvoiceItems)
            {
                item.IsChecked = true;
            }
            var invoice =  _feeProvider.GetRegistrationFeeInvoice(model.InvoiceId);
            if (invoice == null)
            {
                _flashMessage.Danger(Message.DataNotFound);
                isError = true;
                return RedirectToAction(nameof(Index), new { TermId = model.TermId, InvoiceId = model.InvoiceId, Code = model.Code });
            }

            selectedAmount = invoice.TotalAmount;
            //there can be waive till 0 total amount so this must be supported
            //if (selectedAmount == 0)
            //{
            //    _flashMessage.Danger(Message.InvalidAmount);
            //    isError = true;
            //}

            decimal paidAmount = 0;
            paidAmount += model.PaymentMethods.Any() ? model.PaymentMethods.Sum(x => x.Amount) : 0;
            paidAmount += model.FinancialTransactions.Any() ? model.FinancialTransactions.Sum(x => x.UsedScholarship) : 0;
            if(selectedAmount != paidAmount)
            {
                _flashMessage.Danger(Message.InvalidAmount);
                isError = true;
            }
            
            if(model.FinancialTransactions.Any())
            {
                foreach (var scholarship in model.FinancialTransactions)
                {
                   decimal balance = _scholarshipProvider.GetScholarshipBalance(scholarship.StudentScholarshipId);
                   if(balance < scholarship.UsedScholarship)
                   {
                       _flashMessage.Danger(Message.InvalidScholarshipAmount);
                       isError = true;
                       break;
                   }
                }
            }

            if(!isError)
            {
                model.UpdatedBy = GetUser().Id;

                var resultStr = await _receiptProvider.ProcessInvoicePaymentManualAsync(model);
                if (string.IsNullOrEmpty(resultStr))
                {
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                else
                {
                    _flashMessage.Danger(Message.UnableToCreate + " " + resultStr);
                }
            }
            return RedirectToAction(nameof(Index), new { TermId = model.TermId, InvoiceId = model.InvoiceId, Code = model.Code });
        }

        private void CreateSelectList(string studentCode = null, long termId = 0)
        {
            ViewBag.Terms = _selectListProvider.GetStudentRegistrationTerms(studentCode);
            ViewBag.InvoiceDates = _selectListProvider.GetInvoicesByStudentCodeAndTermId(studentCode, termId);
            ViewBag.PaymentMethods = _selectListProvider.GetPaymentMethods();
            ViewBag.ScholarshipStudents = _selectListProvider.GetScholarshipStudentsByStudentCode(studentCode);
        }
    }
}