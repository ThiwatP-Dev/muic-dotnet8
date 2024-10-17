using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("TotalStudentByFeeCodeReport", "")]
    public class TotalStudentByFeeCodeReportController : BaseController
    {
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IFeeProvider _feeProvider;

        public TotalStudentByFeeCodeReportController(ApplicationDbContext db,
                                                     IFlashMessage flashMessage,
                                                     ISelectListProvider selectListProvider,
                                                     IAcademicProvider academicProvider,
                                                     IFeeProvider feeProvider) : base(db, flashMessage, selectListProvider)
        {
            _academicProvider = academicProvider;
            _feeProvider = feeProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0 || criteria.StartTermId == 0 || criteria.EndTermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var startTerm = _academicProvider.GetTerm(criteria.StartTermId);
            var endTerm = _academicProvider.GetTerm(criteria.EndTermId);
            var invoiceItemList = (from invoiceItem in _db.InvoiceItems
                                   join invoice in _db.Invoices on invoiceItem.InvoiceId equals invoice.Id
                                   join student in _db.Students on invoice.StudentId equals student.Id
                                   join term in _db.Terms on invoice.TermId equals term.Id
                                   join academicLevel in _db.AcademicLevels on term.AcademicLevelId equals academicLevel.Id
                                   join feeItem in _db.FeeItems on invoiceItem.FeeItemId equals feeItem.Id
                                   where term.AcademicLevelId == criteria.AcademicLevelId
                                         && (((term.AcademicYear == startTerm.AcademicYear
                                               && term.AcademicTerm >= startTerm.AcademicTerm)
                                              || term.AcademicYear > startTerm.AcademicYear)
                                             && ((term.AcademicYear == endTerm.AcademicYear
                                                 && term.AcademicTerm <= endTerm.AcademicTerm)
                                                || term.AcademicYear < endTerm.AcademicYear))
                                         && (!criteria.FeeItemIds.Any()|| criteria.FeeItemIds.Contains(invoiceItem.FeeItemId))
                                         && (invoice.Type == "a" || invoice.Type == "r")
                                         && (invoiceItem.Type == "a" || invoiceItem.Type == "r")
                                         && !invoice.IsCancel
                                   group new { invoiceItem, invoice, student, term, academicLevel, feeItem }
                                   by new { invoiceItem.FeeItemId, term.Id } into grp
                                   select new TotalStudentByFeeCodeReportViewModel
                                          {
                                              FeeItemId = grp.Key.FeeItemId,
                                              AcademicLevelId = grp.First().term.AcademicLevelId,
                                              TermId = grp.Key.Id,
                                              Year = grp.First().term.AcademicYear,
                                              Term = grp.First().term.AcademicTerm,
                                              AcademicLevel = grp.First().academicLevel.NameEn,
                                              FeeCode = grp.First().feeItem.Code,
                                              FeeNameEn = grp.First().feeItem.NameEn,
                                              FeeNameTh = grp.First().feeItem.NameTh,
                                              TotalStudent = grp.Select(y => y.invoice.StudentId)
                                                                .Distinct()
                                                                .Count(),
                                              TotalAmount = grp.Select(y => y.invoiceItem.TotalAmount)
                                                               .Sum()
                                          })
                                   .OrderBy(x => x.Year)
                                       .ThenBy(x => x.Term)
                                       .ThenBy(x => x.FeeCode);
                                       
            var invoiceItemPageResult = invoiceItemList.GetPaged(criteria, page, true);
            return View(invoiceItemPageResult);
        }

        public IActionResult Details(long feeItemId, long academicLevelId, long termId, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.FeeItem = _feeProvider.GetFeeItem(feeItemId)?.NameEn ?? "N/A";
            ViewBag.Term = _academicProvider.GetTerm(termId)?.TermText ?? "N/A";

            var invoices = (from invoiceItem in _db.InvoiceItems
                            join invoice in _db.Invoices on invoiceItem.InvoiceId equals invoice.Id
                            join student in _db.Students on invoice.StudentId equals student.Id into students
                            from student in students.DefaultIfEmpty()
                            join academicInformation in _db.AcademicInformations on student.Id equals academicInformation.StudentId into academicInformations
                            from academicInformation in academicInformations.DefaultIfEmpty()
                            join faculty in _db.Faculties on academicInformation.FacultyId equals faculty.Id into faculties
                            from faculty in faculties.DefaultIfEmpty()
                            join department in _db.Departments on academicInformation.DepartmentId equals department.Id into departments
                            from department in departments.DefaultIfEmpty()
                            join title in _db.Titles on student.TitleId equals title.Id into titles
                            from title in titles.DefaultIfEmpty()
                            join term in _db.Terms on invoice.TermId equals term.Id into terms
                            from term in terms.DefaultIfEmpty()
                            join nationality in _db.Nationalities on student.NationalityId equals nationality.Id into nationalities
                            from nationality in nationalities.DefaultIfEmpty()
                            join receiptItem in _db.ReceiptItems on invoiceItem.Id equals receiptItem.InvoiceId into receiptItems
                            from receiptItem in receiptItems.DefaultIfEmpty()
                            join receipt in _db.Receipts on receiptItem.ReceiptId equals receipt.Id into receipts
                            from receipt in receipts.DefaultIfEmpty()
                            where invoice.TermId == termId
                                  && invoiceItem.FeeItemId == feeItemId
                                  && (invoice.Type == "a" || invoice.Type == "r")
                                  && (invoiceItem.Type == "a" || invoiceItem.Type == "r")
                                  && !invoice.IsCancel
                                  && invoiceItem.IsPaid
                            select new TotalStudentByFeeCodeDetail
                                   {
                                       CreatedDate = receipt.CreatedAtText,
                                       Amount = invoiceItem.Amount.ToString(StringFormat.Money),
                                       StudentCode = student.Code,
                                       Title = title.NameEn,
                                       StudentName = student.FullNameEn,
                                       FacultyCode = faculty.Code,
                                       DepartmentCode = department.Code,
                                       Faculty = faculty.NameEn,
                                       Department = department.NameEn,
                                       Passport = student.Passport,
                                       CitizenNumber = student.CitizenNumber,
                                       BirthDate = student.BirthDateText,
                                       Gender = student.GenderText,
                                       Phone = student.TelephoneNumber1,
                                       Email = student.Email,
                                       Nationality = nationality.NameEn
                                  }).ToList();

            return View(invoices);
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.FeeItems = _selectListProvider.GetFeeItems();
        }
    }
}