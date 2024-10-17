using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Scholarship;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("ScholarshipFinancialTransactionReport", "")]
    public class ScholarshipFinancialTransactionReportController : BaseController
    {
        protected readonly IScholarshipProvider _scholarshipProvider;
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IReportProvider _reportProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        public ScholarshipFinancialTransactionReportController(ApplicationDbContext db,
                                                               IFlashMessage flashMessage,
                                                               IMapper mapper,
                                                               ISelectListProvider selectListProvider,
                                                               IScholarshipProvider scholarshipProvider,
                                                               ICacheProvider cacheProvider,
                                                               IReportProvider reportProvider,
                                                               IRegistrationProvider registrationProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _scholarshipProvider = scholarshipProvider;
            _cacheProvider = cacheProvider;
            _reportProvider = reportProvider;
            _registrationProvider = registrationProvider;
        }

        public IActionResult Index(ScholarshipFinancialTransactionReportViewModel viewModel)
        {
            CreateSelectList();
            var model = new ScholarshipFinancialTransactionReportViewModel();
            var scholarships = _db.ScholarshipStudents.Include(x => x.Student)
                                                      .Include(x => x.Scholarship)
                                                      .Include(x => x.FinancialTransactions)
                                                      .Where(x => x.Student.Code == viewModel.StudentCode
                                                                  && (viewModel.ScholarshipId == 0
                                                                      || x.ScholarshipId == viewModel.ScholarshipId))
                                                      .Select(x => new ScholarshipFinancialTransactionDetail
                                                                   {
                                                                       Id = x.Id,
                                                                       StudentCode = x.Student.Code,
                                                                       StudentName = x.Student.FullNameEn,
                                                                       Division = x.Student.AcademicInformation.Faculty.NameEn,
                                                                       Major = x.Student.AcademicInformation.Department.NameEn ?? "",
                                                                       Scholarship = x.Scholarship.NameEn
                                                                   })
                                                      .ToList();

            model.SignatoryId1 = viewModel.SignatoryId1;
            model.SignatoryId2 = viewModel.SignatoryId2;
            model.Details = scholarships;
            return View(model);
        }

        public ActionResult Preview(long id, long signatoryId1, long signatoryId2)
        {
            var report = new ReportViewModel();
            var model = new ScholarshipFinancialTransactionDetail();
            var scholarship = _scholarshipProvider.GetScholarshipStudentById(id);
            model = _mapper.Map<ScholarshipStudent, ScholarshipFinancialTransactionDetail>(scholarship);
            model.CurrentTerm = _cacheProvider.GetCurrentTerm(scholarship.Student.AcademicInformation.AcademicLevelId).TermText;
            model.SignatoryName1 = _reportProvider.GetSignatoryNameById(signatoryId1, "th");
            model.SignatoryName2 = _reportProvider.GetSignatoryNameById(signatoryId2, "th");
            model.SignatoryPosition1 = _reportProvider.GetSignatoryPositionById(signatoryId1, "th");
            model.SignatoryPosition2 = _reportProvider.GetSignatoryPositionById(signatoryId2, "th");
            model.ApprovedDate = scholarship.ApprovedAt;
            model.TotalYear = scholarship.Scholarship.TotalYear ?? 0;
            model.ExpiredDate = scholarship.ApprovedAt?.AddYears(model.TotalYear);

            if (scholarship.FinancialTransactions != null)
            {
                var financialTransactions = scholarship.FinancialTransactions.GroupBy(x => new { x.Term, x.ReceiptItem?.Type })
                                                                             .Select(x => new FinancialTransactionDetail
                                                                                          {
                                                                                              Term = x.FirstOrDefault().Term.TermText,
                                                                                              Amount = x.Sum(y => y.UsedScholarship),
                                                                                              Type = x.FirstOrDefault().ReceiptItem == null ? ""
                                                                                                     : x.FirstOrDefault().ReceiptItem.TypeText
                                                                                          })
                                                                             .ToList();
                                                                             
                model.FinancialTransactions = financialTransactions;
            }

            report.Body = model;
            return View(report);
        }

        private void CreateSelectList() 
        {
            ViewBag.Scholarships = _selectListProvider.GetScholarships();
            ViewBag.Signatories = _selectListProvider.GetSignatories();
        }
    }
}