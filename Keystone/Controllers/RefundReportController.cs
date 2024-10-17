using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Keystone.Controllers
{
    [PermissionAuthorize("RefundReport", "")]
    public class RefundReportController : BaseController
    {
        protected readonly ICacheProvider _cacheProvider;

        public RefundReportController(ApplicationDbContext db,
                                      ISelectListProvider selectListProvider,
                                      IMapper mapper,
                                      ICacheProvider cacheProvider) : base(db, mapper, selectListProvider) 
        {
            _cacheProvider = cacheProvider;
        }

        public ActionResult Index(Criteria criteria)
        {
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                criteria.AcademicLevelId = _db.AcademicLevels.SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
                criteria.TermId = _cacheProvider.GetCurrentTerm(criteria.AcademicLevelId).Id;
                CreateSelectList(criteria.AcademicLevelId);
            }

            CreateSelectList(criteria.AcademicLevelId);
            var model = new RefundReportViewModel();
            var invoices = _db.Invoices.AsNoTracking()
                                       .Where(x => !x.IsCancel
                                                    && x.TotalAmount <= 0
                                                    && (string.IsNullOrEmpty(criteria.IsExcludeBalanceInvoice)
                                                        || (!Convert.ToBoolean(criteria.IsExcludeBalanceInvoice) && x.TotalAmount == 0)
                                                        || (Convert.ToBoolean(criteria.IsExcludeBalanceInvoice) && x.TotalAmount < 0)
                                                       )
                                                    && (string.IsNullOrEmpty(criteria.PaidStatus)
                                                        || x.IsPaid == Convert.ToBoolean(criteria.PaidStatus))
                                                    && (criteria.TermId == 0
                                                        || x.TermId == criteria.TermId)
                                                    && (criteria.FacultyId == 0 || x.Student.AcademicInformation.FacultyId == criteria.FacultyId)
                                                    && (criteria.DepartmentId == 0 || x.Student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                    && (string.IsNullOrEmpty(criteria.Code)
                                                        || x.Student.Code.StartsWith(criteria.Code))
                                                    && (string.IsNullOrEmpty(criteria.Name)
                                                        || x.Student.FirstNameEn.StartsWith(criteria.Name)
                                                        || x.Student.FirstNameTh.StartsWith(criteria.Name)
                                                        || x.Student.MidNameEn.StartsWith(criteria.Name)
                                                        || x.Student.MidNameTh.StartsWith(criteria.Name)
                                                        || x.Student.LastNameEn.StartsWith(criteria.Name)
                                                        || x.Student.LastNameTh.StartsWith(criteria.Name))
                                                    && (!criteria.ReceiptDateFrom.HasValue ||
                                                            x.CreatedAt.Date >= criteria.ReceiptDateFrom.Value.Date
                                                        )
                                                    && (!criteria.ReceiptDateTo.HasValue ||
                                                            x.CreatedAt.Date <= criteria.ReceiptDateTo.Value
                                                        )
                                                    //&& (string.IsNullOrEmpty(criteria.InvoiceType)
                                                    //    || ("All".Equals(criteria.InvoiceType))
                                                    //    || (KeystoneLibrary.Models.DataModels.Invoice.TYPE_REGISTRATION.Equals(criteria.InvoiceType) 
                                                    //            && x.Type == "r"
                                                    //       )
                                                    //    || (KeystoneLibrary.Models.DataModels.Invoice.TYPE_ADD_DROP.Equals(criteria.InvoiceType)
                                                    //            && (x.Type == "a" || x.Type == "cr")
                                                    //       )
                                                    //    )
                                                    )

                                       .Select(x => new RefundReportDetail
                                                 {
                                                     Term = x.Term.TermText,
                                                     InvoiceId = x.Id,
                                                     InvoiceNumber = x.Number,
                                                     InvoiceType = x.TypeText,
                                                     StudentCode = x.Student.Code,
                                                     StudentTitle = x.Student.Title.NameEn,
                                                     StudentFirstName = x.Student.FirstNameEn,
                                                     StudentMidName = x.Student.MidNameEn,
                                                     StudentLastName = x.Student.LastNameEn,
                                                     Major = x.Student.AcademicInformation.Department.Abbreviation,
                                                     PaidStatus = x.IsPaid,
                                                     TotalAmount = x.TotalAmountText,
                                                     CreateDate = x.CreatedAtText
                                                 })
                                       .ToList();

            var invoiceIds = invoices.Select(x => x.InvoiceId).ToList();
            var receipts = _db.Receipts.Where(x => invoiceIds.Contains(x.InvoiceId ?? 0))
                                       .Select(x => new
                                                    {
                                                        x.CreatedAtText,
                                                        x.InvoiceId
                                                    })
                                       .ToList();

            foreach (var item in invoices)
            {
                var paidDate = receipts.Where(x => x.InvoiceId == item.InvoiceId)
                                       .Select(x => x.CreatedAtText)
                                       .ToList();
                                       
                item.PaidDate = string.Join(", ", paidDate);
            }

            model.Criteria = criteria;
            model.Details = invoices;
            return View(model);
        }

        private void CreateSelectList(long academicLevelId = 0) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.PaidStatuses = _selectListProvider.GetPaidStatuses();
            ViewBag.ExcludeBalanceInvoices = _selectListProvider.GetExcludeBalanceInvoice();
            //ViewBag.InvoiceType = _selectListProvider.GetInvoiceType();
        }
    }
}