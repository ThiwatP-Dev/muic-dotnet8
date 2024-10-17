using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using Keystone.Permission;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("FinanceOtherFee", "")]
    public class FinanceOtherFeeController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IStudentProvider _studentProvider;
        protected readonly IMasterProvider _masterProvider;
        protected readonly IFeeProvider _feeProvider;
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IReceiptProvider _receiptProvider;

        public FinanceOtherFeeController(ApplicationDbContext db,
                                         IFlashMessage flashMessage,
                                         IMapper mapper,
                                         ISelectListProvider selectListProvider,
                                         IDateTimeProvider dateTimeProvider,
                                         IStudentProvider studentProvider,
                                         IMasterProvider masterProvider,
                                         IFeeProvider feeProvider,
                                         IAcademicProvider academicProvider,
                                         IReceiptProvider receiptProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _studentProvider = studentProvider;
            _masterProvider = masterProvider;
            _feeProvider = feeProvider;
            _academicProvider = academicProvider;
            _receiptProvider = receiptProvider;
        }
         
        public ActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId);
            DateTime? paidDateFrom = _dateTimeProvider.ConvertStringToDateTime(criteria.CreatedFrom);
            DateTime? paidDateTo = _dateTimeProvider.ConvertStringToDateTime(criteria.CreatedTo);

            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var invoices = (from invoiceItem in _db.InvoiceItems
                            join feeItem in _db.FeeItems on invoiceItem.FeeItemId equals feeItem.Id
                            join receiptItem in _db.ReceiptItems on invoiceItem.Id equals receiptItem.InvoiceItemId into receiptItems
                            from receiptItem in receiptItems.DefaultIfEmpty()
                            join receipt in _db.Receipts on receiptItem.ReceiptId equals receipt.Id into receipts
                            from receipt in receipts.DefaultIfEmpty()
                            join invoice in _db.Invoices on invoiceItem.InvoiceId equals invoice.Id
                            join student in _db.Students on invoice.StudentId equals student.Id
                            where invoice.TermId == criteria.TermId
                                  && (criteria.FeeItemId == 0 || feeItem.Id == criteria.FeeItemId)
                                  && (string.IsNullOrEmpty(criteria.Code)
                                      || student.Code == criteria.Code)
                                  && (paidDateFrom == null
                                      || invoice.CreatedAt >= paidDateFrom.Value.Date)
                                  && (paidDateTo == null
                                      || invoice.CreatedAt <= paidDateTo.Value.Date)
                            orderby student.Code
                            select new FinanceOtherFeeViewModel
                                   {
                                       Id = receipt != null? receipt.Id : 0,
                                       StudentCode = student.Code,
                                       StudentName = student.FullNameEn,
                                       FeeItem = feeItem.NameEn,
                                       TotalAmount = receiptItem != null ? receiptItem.TotalAmountText : invoiceItem.TotalAmountText,
                                       CreatedAt = receiptItem != null ? invoice.CreatedAtText : "",
                                       InvoiceNumber = invoice.Number,
                                       ReceiptNumber = receipt != null? receipt.Number : ""
                                   }).GetPaged(criteria, page);
                                       
            return View(invoices);
        }

        [PermissionAuthorize("FinanceOtherFee", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            return View();
        }

        [PermissionAuthorize("FinanceOtherFee", PolicyGenerator.Write)]
        [HttpPost]
        public ActionResult Create(FinanceOtherFeeFormModel model, string returnUrl)
        {
            if (model.FeeItems.Any(x => x.Quantity == 0))
            {
                _flashMessage.Danger(Message.QuantityZero);
                return RedirectToAction(nameof(Create), new { model = model, returnUrl = returnUrl });
            }
            model.UpdatedBy = GetUser()?.Id ?? "";
            var result = _feeProvider.SaveFinanceOtherFeeInvoiceAndReceipt(model);
            if (result)
            {
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            else
            {
                _flashMessage.Danger(Message.UnableToCreate);
                return RedirectToAction(nameof(Create), new { model = model, returnUrl = returnUrl });
            }

            var academicLevelId = _studentProvider.GetStudentAcademicLevelIdByCode(model.StudentCode);

            return RedirectToAction(nameof(Index), new
                                                   {
                                                       AcademicLevelId = academicLevelId,
                                                       TermId = model.TermId,
                                                       Code = model.StudentCode
                                                   });
        }

        public ActionResult Preview(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var report = new ReportViewModel();
            var model = _receiptProvider.FinanceOtherFeePreview(id);
            report.Body = model;
            return View(report);
        }

        private void CreateSelectList(long academicLevelId = 0, string studentCode = null)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.FeeItems = _selectListProvider.GetFeeItems();
            ViewBag.StudentTerms = _selectListProvider.GetTermsByStudentCode(studentCode);
            ViewBag.PaymentMethods = _selectListProvider.GetPaymentMethods();
        }
    }
}