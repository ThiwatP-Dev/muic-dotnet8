using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Enumeration;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    public class ReceiveDairySummaryController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;

        public ReceiveDairySummaryController(ApplicationDbContext db,
                                             ISelectListProvider selectListProvider,
                                             IFlashMessage flashMessage,
                                             IMapper mapper,
                                             IDateTimeProvider dateTimeProvider) : base(db, flashMessage, mapper, selectListProvider) 
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public ActionResult Index(int page, Criteria criteria) 
        {
            CreateSelectList();
            var model = new ReceiveDairySummaryViewModel();
            if (string.IsNullOrEmpty(criteria.StartedAt) || string.IsNullOrEmpty(criteria.EndedAt))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            DateTime? startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);

            var receiptItems = _db.ReceiptItems.Include(x => x.Receipt)
                                                   .ThenInclude(x => x.Term)
                                               .Include(x => x.Receipt)
                                                   .ThenInclude(x => x.ReceiptPaymentMethods)
                                                   .ThenInclude(x => x.PaymentMethod)
                                               .Include(x => x.FeeItem)
                                               .Where(x => x.Receipt.CreatedAt.Date >= startedAt.Value.Date
                                                           && x.Receipt.CreatedAt.Date <= endedAt.Value.Date
                                                           && (criteria.PaymentMethodIds == null
                                                               || !criteria.PaymentMethodIds.Any()
                                                               || x.Receipt.ReceiptPaymentMethods.Any(y => criteria.PaymentMethodIds.Contains(y.PaymentMethodId)))
                                                           && (criteria.FeeItemIds == null
                                                               || !criteria.FeeItemIds.Any()
                                                               || criteria.FeeItemIds.Contains(x.FeeItemId ?? 0))
                                                           && (string.IsNullOrEmpty(criteria.FeeType)
                                                               || criteria.FeeType == "o" && x.Receipt.Number.StartsWith(ReceiptNumberTypeExtension.ToStringValue(ReceiptNumberType.OTHER))
                                                               || criteria.FeeType == "r" && x.Receipt.Number.StartsWith(ReceiptNumberTypeExtension.ToStringValue(ReceiptNumberType.REGISTRATION))))
                                               .ToList();

            var t1 = receiptItems.Where(x => x.ReceiptId == 17858).ToList();

            var paymentMethodHeader = _db.ReceiptPaymentMethods.Where(x => receiptItems.Any(y => y.ReceiptId == x.ReceiptId))
                                                               .Select(x => x.PaymentMethod)
                                                               .Distinct()
                                                               .ToList();

            model.ReceiptItems = receiptItems.GroupBy(x => x.FeeItemName)
                                             .Select(x => new ReceiveReceiptItemViewModel
                                                          {
                                                              ReceiptId = x.FirstOrDefault().ReceiptId,
                                                              Name = x.Key,
                                                              PaymentMethods = paymentMethodHeader,
                                                              TotalStudent = x.Select(y => y.FeeItemName).Count(),
                                                              StudentPrice = x.FirstOrDefault().FeeItem.DefaultPriceText,
                                                              TotalPrice = x.Select(y => y.FeeItemName).Count() * x.FirstOrDefault().FeeItem.DefaultPrice,
                                                              PaymentAmounts = x.FirstOrDefault().Receipt.ReceiptPaymentMethods
                                                                                .Select(y => new PaymentMethodAmountViewModel
                                                                                             {
                                                                                                 PaymentName = y.PaymentMethod.NameEn,
                                                                                                 PaymentAmount = y.Amount
                                                                                             })
                                                                                .ToList()
                                                          })
                                             .ToList();

            return View(model);
        }

        private void CreateSelectList()
        {
            ViewBag.FeeItems = _selectListProvider.GetFeeItems();
            ViewBag.PaymentMethods = _selectListProvider.GetPaymentMethods();
            ViewBag.FeeTypes = _selectListProvider.GetFeeTypes();
        }
    }
}