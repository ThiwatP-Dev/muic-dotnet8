using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    public class FeeItemDairySummaryController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IAcademicProvider _academicProvider;

        public FeeItemDairySummaryController(ApplicationDbContext db,
                                             ISelectListProvider selectListProvider,
                                             IFlashMessage flashMessage,
                                             IMapper mapper,
                                             IDateTimeProvider dateTimeProvider,
                                             IAcademicProvider academicProvider) : base(db, flashMessage, mapper, selectListProvider) 
        {
            _dateTimeProvider = dateTimeProvider;
            _academicProvider = academicProvider;
        }

        public ActionResult Index(int page, Criteria criteria) 
        {
            CreateSelectList();
            var model = new FeeItemDairySummaryViewModel();
            if (string.IsNullOrEmpty(criteria.StartedAt) || string.IsNullOrEmpty(criteria.EndedAt) || criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            DateTime? startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);
            var academicLevelName = _academicProvider.GetAcademicLevelNameById(criteria.AcademicLevelId);

            var feeItemTerm = (from receiptItem in _db.ReceiptItems
                               join receipt in _db.Receipts on receiptItem.ReceiptId equals receipt.Id
                               join term in _db.Terms on receipt.TermId equals term.Id into termTmp
                               from term in termTmp.DefaultIfEmpty()
                               join academicLevel in _db.AcademicLevels on term.AcademicLevelId equals academicLevel.Id
                               join feeItem in _db.FeeItems on receiptItem.FeeItemId equals feeItem.Id
                               where receipt.CreatedAt.Date >= startedAt.Value.Date
                                     && receipt.CreatedAt.Date <= endedAt.Value.Date
                                     && receipt.Term.AcademicLevelId == criteria.AcademicLevelId
                                     && (criteria.FeeGroupId == 0
                                         || feeItem.FeeGroupId == criteria.FeeGroupId)
                                     && (criteria.FeeItemIds == null
                                         || !criteria.FeeItemIds.Any()
                                         || criteria.FeeItemIds.Contains(feeItem.Id))
                               group new { term, receipt, feeItem } by term.TermText into feeItemTermGroup
                               select new FeeItemDairyTerm
                                      {
                                          AcademicLevelHeader = academicLevelName,
                                          TotalBySemester = feeItemTermGroup.Key,
                                          AcademicTerm = feeItemTermGroup.FirstOrDefault().term.AcademicTerm,
                                          AcademicYear = feeItemTermGroup.FirstOrDefault().term.AcademicYear,
                                          FeeItemDetails = feeItemTermGroup.GroupBy(y => new { y.receipt.CreatedAtText, y.feeItem.NameEn })
                                                                           .Select(y => new FeeItemDairyDetail
                                                                                        {
                                                                                            Date = y.FirstOrDefault().receipt.CreatedAt,
                                                                                            DateText = y.FirstOrDefault().receipt.CreatedAtText,
                                                                                            FeeCode = y.FirstOrDefault().feeItem.Code,
                                                                                            FeeType = y.FirstOrDefault().feeItem.NameEn,
                                                                                            Amount = y.Sum(z => z.receipt.TotalAmount)
                                                                                        })
                                                                           .OrderBy(x => x.FeeCode)
                                                                           .ToList()
                                      }).OrderBy(x => x.AcademicYear)
                                            .ThenBy(x => x.AcademicTerm)
                                        .ToList();

            model.FeeItemTerms = feeItemTerm;
            model.Criteria = criteria;
            return View(model);
        }

        private void CreateSelectList()
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.FeeItems = _selectListProvider.GetFeeItems();
            ViewBag.FeeGroups = _selectListProvider.GetFeeGroups();
        }
    }
}