using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("ScholarshipSummaryExpenses", "")]
    public class ScholarshipSummaryExpensesController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;

        public ScholarshipSummaryExpensesController(ApplicationDbContext db,
                                                    IFlashMessage flashMessage,
                                                    IStudentProvider studentProvider,
                                                    ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider)
        {
            _studentProvider = studentProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList();
            if (criteria.StudentId == Guid.Empty || (criteria.AcademicYear ?? 0) == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var results = _db.FinancialTransactions.Include(x => x.Term)
                                                   .Include(x => x.ReceiptItem)
                                                        .ThenInclude(x => x.FeeItem)
                                                             .ThenInclude(x => x.FeeGroup)
                                                   .Where(x => x.StudentId == criteria.StudentId
                                                               && x.Term.AcademicYear == criteria.AcademicYear)
                                                   .IgnoreQueryFilters()
                                                   .OrderBy(x => x.Term.AcademicYear)
                                                   .ThenBy(x => x.Term.AcademicTerm)
                                                   .ThenBy(x => x.ReceiptItem.FeeItem.FeeGroup.NameEn)
                                                   .ThenBy(x => x.ReceiptItem.FeeItem.NameEn)
                                                   .Select(x => new ScholarshipSummaryExpenses
                                                   {
                                                       TermText = x.Term.TermText,
                                                       FeeGroupName = x.ReceiptItem.FeeItem.FeeGroup.NameEn,
                                                       FeeItemName = x.ReceiptItem.FeeItem.NameEn,
                                                       Amount = x.UsedScholarship
                                                   })
                                                   .ToList();

            var model = new ScholarshipSummaryExpensesViewModel
            {
                Criteria = criteria,
                Student = _studentProvider.GetStudentInformationById(criteria.StudentId),
                Results = results
            };

            return View(model);
        }

        private void CreateSelectList()
        {
            ViewBag.Students = _selectListProvider.GetScholarshipStudents();
        }
    }
}