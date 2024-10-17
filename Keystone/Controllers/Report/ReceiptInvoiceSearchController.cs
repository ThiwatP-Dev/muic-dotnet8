using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("ReceiptInvoiceSearch", "")]
    public class ReceiptInvoiceSearchController : BaseController
    {
        public ReceiptInvoiceSearchController(ApplicationDbContext db,
                                              ISelectListProvider selectListProvider,
                                              IFlashMessage flashMessage) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0
                || criteria.TermId == 0
                || string.IsNullOrEmpty(criteria.Type))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var results = new List<ReceiptInvoiceSearchResultViewModel>();

            switch (criteria.Type)
            {
                case "r":
                    results = _db.Receipts.Include(x => x.Student)
                                          .Include(x => x.ReceiptItems)
                                          .Where(x => x.TermId == criteria.TermId
                                                      && !x.IsCancel
                                                      && (string.IsNullOrEmpty(criteria.CodeAndName) || x.Number.Contains(criteria.CodeAndName))
                                                      && (string.IsNullOrEmpty(criteria.Code) || x.Student.Code.Contains(criteria.Code))
                                                      && (string.IsNullOrEmpty(criteria.FirstName)
                                                          || x.Student.FirstNameEn.ToLower().StartsWith(criteria.FirstName.ToLower())
                                                          || (x.Student.FirstNameTh ?? string.Empty).StartsWith(criteria.FirstName)
                                                          || (!string.IsNullOrEmpty(x.Student.MidNameEn) && x.Student.MidNameEn.ToLower().StartsWith(criteria.FirstName.ToLower()))
                                                          || (x.Student.MidNameTh ?? string.Empty).StartsWith(criteria.FirstName)
                                                          || x.Student.LastNameEn.ToLower().StartsWith(criteria.FirstName.ToLower())
                                                          || (x.Student.LastNameTh ?? string.Empty).StartsWith(criteria.FirstName))
                                                      && (string.IsNullOrEmpty(criteria.ReceiptInvoiceType) || x.Round == criteria.ReceiptInvoiceType)
                                                      && (string.IsNullOrEmpty(criteria.PrintStatus)
                                                          || x.IsPrint == Convert.ToBoolean(criteria.PrintStatus)))
                                          .Select(x => new ReceiptInvoiceSearchResultViewModel
                                                       {
                                                           Id = x.Id,
                                                           Number = x.Number,
                                                           StudentCode = x.Student.Code,
                                                           StudentFullName = x.Student.FullNameEn,
                                                           TotalAmount = x.TotalAmount,
                                                           ScholarshipPayAmount = x.TotalScholarshipAmount,
                                                           Amount = x.Amount,
                                                           TypeText = x.RoundText,
                                                           IsCancel = x.IsCancel,
                                                           CreatedAt = x.CreatedAt
                                                       })
                                          .OrderBy(x => x.CreatedAt)
                                               .ThenBy(x => x.Number)
                                          .ToList();
                    break;
                case "i":
                    results = _db.Invoices.Include(x => x.Student)
                                          .Include(x => x.InvoiceItems)
                                          .Where(x => x.TermId == criteria.TermId
                                                      && !x.IsCancel
                                                      && (string.IsNullOrEmpty(criteria.CodeAndName) || x.Number.Contains(criteria.CodeAndName))
                                                      && (string.IsNullOrEmpty(criteria.Code) || x.Student.Code.Contains(criteria.Code))
                                                      && (string.IsNullOrEmpty(criteria.FirstName)
                                                          || x.Student.FirstNameEn.ToLower().StartsWith(criteria.FirstName.ToLower())
                                                          || (x.Student.FirstNameTh ?? string.Empty).StartsWith(criteria.FirstName)
                                                          || (!string.IsNullOrEmpty(x.Student.MidNameEn) && x.Student.MidNameEn.ToLower().StartsWith(criteria.FirstName.ToLower()))
                                                          || (x.Student.MidNameTh ?? string.Empty).StartsWith(criteria.FirstName)
                                                          || x.Student.LastNameEn.ToLower().StartsWith(criteria.FirstName.ToLower())
                                                          || (x.Student.LastNameTh ?? string.Empty).StartsWith(criteria.FirstName))
                                                      && (string.IsNullOrEmpty(criteria.ReceiptInvoiceType) || x.Type == criteria.ReceiptInvoiceType)
                                                      && (string.IsNullOrEmpty(criteria.PaidStatus)
                                                          || x.IsPaid == Convert.ToBoolean(criteria.PaidStatus)))
                                          .Select(x => new ReceiptInvoiceSearchResultViewModel
                                                       {
                                                           Id = x.Id,
                                                           Number = x.Number,
                                                           StudentCode = x.Student.Code,
                                                           StudentFullName = x.Student.FullNameEn,
                                                           TotalAmount = x.TotalAmount,
                                                           ScholarshipPayAmount = x.ScholarshipAmount,
                                                           Amount = x.Amount,
                                                           TypeText = x.TypeText,
                                                           IsPaid = x.IsPaid,
                                                           IsCancel = x.IsCancel,
                                                           CreatedAt = x.CreatedAt
                                                       })
                                          .OrderBy(x => x.CreatedAt)
                                               .ThenBy(x => x.Number)
                                          .ToList();
                    break;
            }
            
            ReceiptInvoiceSearchViewModel model = new ReceiptInvoiceSearchViewModel
                                                  {
                                                      Criteria = criteria,
                                                      Results = results
                                                  };

            return View(model);
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.Types = _selectListProvider.GetReceiptInvoice();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.ReceiptInvoiceTypes = _selectListProvider.GetReceiptInvoiceType();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
        }
    }
}