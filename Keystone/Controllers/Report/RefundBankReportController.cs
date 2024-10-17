using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    public class RefundBankReportController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;

        public RefundBankReportController(ApplicationDbContext db,
                                          IFlashMessage flashMessage,
                                          IMapper mapper,
                                          ISelectListProvider selectListProvider,
                                          IDateTimeProvider dateTimeProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            var model = new RefundBankReportViewModel();
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            DateTime? startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);
            DateTime? createdFrom = _dateTimeProvider.ConvertStringToDateTime(criteria.CreatedFrom);
            DateTime? createdTo = _dateTimeProvider.ConvertStringToDateTime(criteria.CreatedTo);

            var refundDetail = (from refund in _db.Refunds
                                join receiptItem in _db.ReceiptItems on refund.ReceiptItemId equals receiptItem.Id
                                join receipt in _db.Receipts on refund.ReceiptId equals receipt.Id
                                join student in _db.Students on receipt.StudentId equals student.Id
                                join bankBranch in _db.BankBranches on student.BankBranchId equals bankBranch.Id into bankBranches
                                from bankBranch in bankBranches.DefaultIfEmpty()
                                where receipt.TermId == criteria.TermId
                                      && (createdFrom == null
                                          || refund.CreatedAt.Date >= createdFrom.Value.Date)
                                      && (createdTo == null
                                          || refund.CreatedAt.Date <= createdTo.Value.Date)
                                      && (startedAt == null
                                          || refund.RefundedAt >= startedAt.Value.Date)
                                      && (endedAt == null
                                          || refund.RefundedAt <= endedAt.Value.Date)
                                      && (Convert.ToBoolean(criteria.HasAccount) ? !string.IsNullOrEmpty(student.BankAccount)
                                                                                 : string.IsNullOrEmpty(student.BankAccount))
                                      && (Convert.ToBoolean(criteria.Status) ? refund.RefundedAt != null
                                                                             : refund.RefundedAt == null)
                                group new { receipt, refund, student, bankBranch }
                                by receipt.StudentId into studentGroup
                                select new RefundBankReportDetail
                                       {
                                           StudentId = studentGroup.Key ?? new Guid(),
                                           TermId = studentGroup.First().receipt.TermId ?? 0,
                                           AccountName = studentGroup.First().student.FullNameEn ?? "",
                                           AccountNumber = studentGroup.First().student.BankAccount ?? "",
                                           Amount = studentGroup.Sum(y => y.refund.Amount),
                                           BankBranch = studentGroup.First().bankBranch == null
                                                        ? "" : studentGroup.First().bankBranch.Abbreviation ?? "",
                                           StudentCode = studentGroup.First().student.Code,
                                           RefundedAt = studentGroup.First().refund.RefundedAtText ?? "",
                                           RefundIds = studentGroup.Select(y => y.refund.Id).ToList(),
                                           RefundIdString = string.Join("|", studentGroup.Select(y => y.refund.Id).ToList()),
                                           CreatedDate = studentGroup.First().refund.CreatedAtText,
                                           CreatedBy = studentGroup.First().refund.CreatedBy
                                       }).ToList();

            model.RefundBankReportDetails = refundDetail;
            return View(model);
        }

        public IActionResult Details(Guid stundentId, long termId)
        {
            var refunds = _db.Refunds.Include(x => x.ReceiptItem)
                                         .ThenInclude(x => x.InvoiceItem)
                                     .Include(x => x.Receipt)
                                         .ThenInclude(x => x.Student)
                                     .Include(x => x.Receipt)
                                         .ThenInclude(x => x.Term)
                                     .Where(x => x.Receipt.StudentId == stundentId
                                                 && x.Receipt.TermId == termId)
                                     .ToList();

            return View(refunds);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public string SaveRefundedAt(DateTime refundedAt, List<string> refundIds, string redirectPath)
        {
            try
            {
                var ids = new List<long>();
                foreach(var id in refundIds)
                {
                    var idLong = id.Split('|').Select(Int64.Parse).ToList();
                    ids.AddRange(idLong);
                }

                var refunds = _db.Refunds.Where(x => ids.Contains(x.Id))
                                         .ToList();

                if (refunds.Any())
                {
                    refunds.Select(x => {
                                            x.RefundedAt = refundedAt;
                                            return x;
                                        })
                           .ToList();

                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                else
                {
                    _flashMessage.Danger(Message.UnableToSave);
                }
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToSave);
            }

            return redirectPath;
        }

        private void CreateSelectList(long academicLevelId)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}