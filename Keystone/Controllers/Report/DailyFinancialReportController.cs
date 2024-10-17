using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class DailyFinancialReportController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IStudentProvider _studentProvider;

        public DailyFinancialReportController(ApplicationDbContext db, 
                                              IFlashMessage flashMessage,
                                              IDateTimeProvider dateTimeProvider,
                                              IStudentProvider studentProvider,
                                              ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) 
        { 
            _dateTimeProvider = dateTimeProvider;
            _studentProvider = studentProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList();
            var model = new DailyFinancialReportViewModel
                        {
                            Criteria = criteria
                        };

            if (criteria.StartedAt == null || criteria.EndedAt == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            DateTime? startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);

            var receipts = _db.Receipts.Include(x => x.Student)
                                           .ThenInclude(x => x.AcademicInformation)
                                       .Include(x => x.ReceiptPaymentMethods)
                                           .ThenInclude(x => x.PaymentMethod)
                                       .Include(x => x.ReceiptItems)
                                       .Where(x => (string.IsNullOrEmpty(criteria.CodeAndName)
                                                    || x.CreatedBy == criteria.CodeAndName)
                                                   && (startedAt == null
                                                       || x.CreatedAt.Date >= startedAt.Value.Date)
                                                   && (endedAt == null
                                                       || x.CreatedAt.Date <= endedAt.Value.Date)
                                                   && (criteria.AcademicLevelId == 0
                                                       || x.Student.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId)
                                                   && (criteria.StartStudentBatch == null
                                                       || criteria.StartStudentBatch == 0
                                                       || criteria.StartStudentBatch >= x.Student.AcademicInformation.Batch)
                                                   && (criteria.EndStudentBatch == null
                                                       || criteria.EndStudentBatch == 0
                                                       || criteria.EndStudentBatch <= x.Student.AcademicInformation.Batch)
                                                   && (string.IsNullOrEmpty(criteria.ReferenceNumber)
                                                       || x.Number.Contains(criteria.ReferenceNumber))
                                                   && (string.IsNullOrEmpty(criteria.Code)
                                                       || x.Student.Code.StartsWith(criteria.Code))
                                                   && (string.IsNullOrEmpty(criteria.FirstName)
                                                       || x.Student.FirstNameEn.StartsWith(criteria.FirstName)
                                                       || x.Student.FirstNameTh.StartsWith(criteria.FirstName)
                                                       || x.Student.MidNameEn.StartsWith(criteria.FirstName)
                                                       || x.Student.MidNameTh.StartsWith(criteria.FirstName)
                                                       || x.Student.LastNameEn.StartsWith(criteria.FirstName)
                                                       || x.Student.LastNameTh.StartsWith(criteria.FirstName)))
                                       .ToList();
            
            receipts = receipts.Where(x => (!criteria.FeeItemIds.Any()
                                            || criteria.FeeItemIds.Intersect(x.ReceiptItems.Select(y => y.NonNullableFeeItemId)).Any())
                                            && (criteria.PaymentMethodIds == null
                                                || criteria.PaymentMethodIds.Intersect(x.ReceiptPaymentMethods.Select(y => y.PaymentMethodId)).Any()))
                               .ToList();

            model.DailyFinancialReports = receipts.GroupBy(x => $"{ x.Number} { string.Join(", ", x.ReceiptItems.Select(y => y.FeeItemName).Distinct()) }")
                                                  .Select(x => new DailyFinancialReport
                                                               {
                                                                   ReceiptNumber = x.FirstOrDefault().Number,
                                                                   StudentCode = x.FirstOrDefault().Student.Code,
                                                                   StudentName = x.FirstOrDefault().Student.FullNameEn,
                                                                   FeeNames = x.FirstOrDefault().ReceiptItems.FirstOrDefault()?.FeeItemName ?? "",
                                                                   PaymentMethods = string.Join(", ", x.SelectMany(y => y.ReceiptPaymentMethods.Select(z => z.PaymentMethod.NameEn))),
                                                                   TotalTransaction = x.FirstOrDefault().ReceiptItems.Count(),
                                                                   Amount = x.Sum(y => y.ReceiptItems.Sum(z => z.Amount)),
                                                                   CreatedAt = x.FirstOrDefault().CreatedAt,
                                                                   CreatedAtText = x.FirstOrDefault().CreatedAtText
                                                               })
                                                  .OrderBy(x => x.CreatedAt)
                                                      .ThenBy(x => x.ReceiptNumber)  
                                                  .ToList();

            model.DailyFinancialSummaries = receipts.SelectMany(x => x.ReceiptPaymentMethods)
                                                    .GroupBy(x => x.PaymentMethod.NameEn)
                                                    .Select(x => new DailyFinancialSummary
                                                                    {
                                                                        PaymentMethod = x.Key,
                                                                        TotalTransaction = x.Count(),
                                                                        Amount = x.Sum(y => y.Amount)
                                                                    })
                                                    .ToList();

            model.DailyFinancialReportTotal = new DailyFinancialSummary
                                              {
                                                  TotalTransaction = model.DailyFinancialReports.Sum(x => x.TotalTransaction),
                                                  Amount = model.DailyFinancialReports.Sum(x => x.Amount)
                                              };

            model.DailyFinancialSummaryTotal = new DailyFinancialSummary
                                               {
                                                   TotalTransaction = model.DailyFinancialSummaries.Sum(x => x.TotalTransaction),
                                                   Amount = model.DailyFinancialSummaries.Sum(x => x.Amount)
                                               };

            return View(model);
        }

        private void CreateSelectList()
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.FeeItems = _selectListProvider.GetFeeItems();
            ViewBag.PaymentMethods = _selectListProvider.GetPaymentMethods();
            ViewBag.CreatedBy = _selectListProvider.GetReceiptCreatedBy();
        }
    }
}