using KeystoneLibrary.Data;
using AutoMapper;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    public class RegistrationReceiptDairySummaryController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;

        public RegistrationReceiptDairySummaryController(ApplicationDbContext db,
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
            var model = new RegistrationReceiptDairySummaryViewModel();
            if (string.IsNullOrEmpty(criteria.StartedAt) || string.IsNullOrEmpty(criteria.EndedAt) || criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            DateTime? startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);

            var termHeader = _db.Receipts.Where(x => x.CreatedAt.Date >= startedAt.Value.Date
                                                     && x.CreatedAt.Date <= endedAt.Value.Date
                                                     && x.Term.AcademicLevelId == criteria.AcademicLevelId)
                                         .Select(x => x.Term)
                                         .Distinct()
                                         .OrderBy(x => x.AcademicYear)
                                             .ThenBy(x => x.AcademicTerm)
                                         .ToList();

            model.Terms = (from receipt in _db.Receipts
                           join term in _db.Terms on receipt.TermId equals term.Id
                           join academicLevel in _db.AcademicLevels on term.AcademicLevelId equals academicLevel.Id
                           join student in _db.Students on receipt.StudentId equals student.Id
                           join academicInformation in _db.AcademicInformations on student.Id equals academicInformation.StudentId
                           where receipt.CreatedAt.Date >= startedAt.Value.Date
                                 && receipt.CreatedAt.Date <= endedAt.Value.Date
                                 && term.AcademicLevelId == criteria.AcademicLevelId
                                 && (criteria.StartStudentBatch == null
                                     || academicInformation.Batch >= criteria.StartStudentBatch)
                                 && (criteria.EndStudentBatch == null
                                     || academicInformation.Batch <= criteria.EndStudentBatch)
                           group new { receipt, student, term }
                           by receipt.CreatedAtText into receiptGroup
                           select new RegistrationReceiptDairyTerm
                                  {
                                      TermHeader = termHeader,
                                      TotalByDate = receiptGroup.FirstOrDefault().receipt.CreatedAtText,
                                      Students = receiptGroup.GroupBy(y => y.student.Id)
                                                             .Select(y => new RegistrationReceiptDairyStudent
                                                                          {
                                                                              StudentCode = y.FirstOrDefault().student.Code,
                                                                              StudentName = y.FirstOrDefault().student.FullNameEn,
                                                                              Amounts = y.GroupBy(z => z.term.TermText)
                                                                                         .Select(z => new RegistrationReceiptDairyAmount
                                                                                                      {
                                                                                                          Term = z.FirstOrDefault().term.TermText,
                                                                                                          Amount = z.FirstOrDefault().receipt.Amount,
                                                                                                          AmountText = z.FirstOrDefault().receipt.AmountText
                                                                                                      })
                                                                                         .ToList()
                                                                          })
                                                             .OrderBy(x => x.StudentCode)
                                                             .ToList()
                                  }).ToList();

            model.Criteria = criteria;
            return View(model);
        }

        private void CreateSelectList()
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
        }
    }
}