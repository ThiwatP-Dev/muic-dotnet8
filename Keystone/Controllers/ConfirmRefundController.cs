using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("ConfirmRefund", "")]
    public class ConfirmRefundController : BaseController
    {
        public ConfirmRefundController(ApplicationDbContext db,
                                       IFlashMessage flashMessage,
                                       ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }
        
        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var invoices = from invoice in _db.Invoices
                           join student in _db.Students on invoice.StudentId equals student.Id
                           join title in _db.Titles on student.TitleId equals title.Id
                           let firstNameEn = string.IsNullOrEmpty(student.MidNameEn) ? student.FirstNameEn : student.FirstNameEn + " " + student.MidNameEn
                           where invoice.TotalAmount <= 0
                                 && (string.IsNullOrEmpty(criteria.IsExcludeBalanceInvoice)
                                                              || (!Convert.ToBoolean(criteria.IsExcludeBalanceInvoice) && invoice.TotalAmount == 0)
                                                              || (Convert.ToBoolean(criteria.IsExcludeBalanceInvoice) && invoice.TotalAmount < 0)
                                                             )
                                 && invoice.TermId == criteria.TermId
                                 && !invoice.IsPaid
                                 && !invoice.IsCancel
                           orderby invoice.Number
                           select new ConfirmRefundViewModel
                                  {
                                      InvoiceId = invoice.Id,
                                      InvoiceNumber = invoice.Number,
                                      StudentCode = student.Code,
                                      StudentName = title.NameEn + " " + firstNameEn + " " + student.LastNameEn,
                                      Amount = invoice.Amount
                                  };

            return View(invoices.GetAllPaged(criteria));
        }

        [PermissionAuthorize("ConfirmRefund", PolicyGenerator.Write)]
        [HttpPost]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult Edit(PagedResult<ConfirmRefundViewModel> model)
        {
            if (model != null && model.Results != null && model.Results.Any(x => x.IsChecked))
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    foreach (var item in model.Results.Where(x => x.IsChecked))
                    {
                        var invoice = _db.Invoices.SingleOrDefault(x => x.Id == item.InvoiceId);
                        if (invoice != null)
                        {
                            invoice.IsPaid = true;
                            _db.InvoiceItems.Where(x => x.InvoiceId == item.InvoiceId
                                                        && x.TotalAmount <= 0
                                                        && !x.IsPaid)
                                            .ToList()
                                            .ForEach(x => x.IsPaid = true);
                        }
                    }

                    try
                    {
                        _db.SaveChanges();
                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index), new { academicLevelId = model.Criteria?.AcademicLevelId, termId = model.Criteria?.TermId });
                    }
                    catch
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.UnableToEdit);
                        return RedirectToAction(nameof(Index), new { academicLevelId = model.Criteria?.AcademicLevelId, termId = model.Criteria?.TermId });
                    }
                }
            }

            _flashMessage.Danger(Message.AtLeastOneInvoice);
            return RedirectToAction(nameof(Index), new { academicLevelId = model?.Criteria?.AcademicLevelId, termId = model?.Criteria?.TermId });
        }

        private void CreateSelectList(long academicLevelId)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);   
            }
            ViewBag.ExcludeBalanceInvoices = _selectListProvider.GetExcludeBalanceInvoice();
        }
    }
}