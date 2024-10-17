using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.DataModels.Scholarship;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("Voucher", "")]
    public class VoucherController : BaseController
    {
        protected readonly IVoucherProvider _voucherProvider;
        protected readonly IScholarshipProvider _scholarshipProvider;
        protected readonly IAcademicProvider _academicProvider;

        public VoucherController(ApplicationDbContext db,
                                 IFlashMessage flashMessage,
                                 IVoucherProvider voucherProvider,
                                 IScholarshipProvider scholarshipProvider,
                                 IAcademicProvider academicProvider,
                                 ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) 
        { 
            _voucherProvider = voucherProvider;
            _scholarshipProvider = scholarshipProvider;
            _academicProvider = academicProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var model = _db.Vouchers.Include(x => x.Student)
                                        .ThenInclude(x => x.AcademicInformation)
                                    .Include(x => x.Term)
                                    .Include(x => x.ScholarshipStudent)
                                        .ThenInclude(x => x.Scholarship)
                                    .Where(x => x.Student.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                && (criteria.TermId == 0 || x.TermId == criteria.TermId)
                                                && (string.IsNullOrEmpty(criteria.Code) 
                                                    || x.Student.Code.Contains(criteria.Code))
                                                && (criteria.RequestedFrom == null
                                                    || x.RequestedAt.Date >= criteria.RequestedFrom.Value.Date)
                                                && (criteria.RequestedTo == null
                                                    || x.RequestedAt.Date <= criteria.RequestedTo.Value.Date)
                                                && (string.IsNullOrEmpty(criteria.Status)
                                                    || x.Status == criteria.Status))
                                    .GetPaged(criteria, page);
            return View(model);
        }

        public ActionResult Details(long id)
        {    
            var voucherLogs = _voucherProvider.GetVoucherLogs(id);
            return PartialView("~/Views/ScholarshipProfile/_VoucherDetails.cshtml", voucherLogs);  
        }

        [PermissionAuthorize("Voucher", PolicyGenerator.Write)]
        public IActionResult Create(string code, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!string.IsNullOrEmpty(code))
            {
                Voucher model = new Voucher
                                {
                                    Code = code,
                                    RequestedAt = DateTime.UtcNow
                                };

                if (_voucherProvider.UpdateVoucherModel(model))
                {
                    CreateSelectList(model.Student.AcademicInformation.AcademicLevelId);
                    return View(model);
                }

                _flashMessage.Danger(Message.StudentNotFound);
                return View();
            }

            _flashMessage.Warning(Message.RequiredData);
            return View();
        }

        [PermissionAuthorize("Voucher", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Voucher model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (model.StudentId == Guid.Empty)
            {
                _flashMessage.Danger(Message.StudentNotFound);
                return View();
            }
            
            if (ModelState.IsValid)
            {
                using(var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        model.TotalAmount = model.VoucherLogs.Sum(x => x.Amount);
                        _db.Vouchers.Add(model);
                        _db.SaveChanges();

                        _scholarshipProvider.CreateTransactionFromVoucher(model);

                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);

                        var academicLevelId =_academicProvider.GetTerm(model.TermId)?.AcademicLevelId ?? 0;
                        return RedirectToAction(nameof(Index), new Criteria
                                                               {
                                                                   AcademicLevelId = academicLevelId,
                                                                   TermId = model.TermId
                                                               });
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }

            _voucherProvider.UpdateVoucherModel(model);
            CreateSelectList(model.Student.AcademicInformation.AcademicLevelId);
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public IActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = _voucherProvider.GetVoucherById(id);
            CreateSelectList(model.Student.AcademicInformation.AcademicLevelId);
            return View(model);
        }

        [PermissionAuthorize("Voucher", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Voucher model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (model.StudentId == Guid.Empty)
            {
                _flashMessage.Danger(Message.StudentNotFound);
                return Redirect(returnUrl);
            }

            var voucher = _db.Vouchers.Find(model.Id);
            using(var transaction = _db.Database.BeginTransaction())
            {
                // remove old fee item
                _db.VoucherLogs.RemoveRange(_db.VoucherLogs.Where(x => x.VoucherId == model.Id));
                _db.SaveChanges();

                if (await TryUpdateModelAsync<Voucher>(voucher))
                {
                    try
                    {
                        _scholarshipProvider.InactiveTransactionFromVoucher(voucher.Id);

                        voucher.TotalAmount = model.VoucherLogs.Sum(x => x.Amount);
                        _db.SaveChanges();

                        _scholarshipProvider.CreateTransactionFromVoucher(voucher);

                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        var academicLevelId =_academicProvider.GetTerm(model.TermId)?.AcademicLevelId ?? 0;
                        return RedirectToAction(nameof(Index), new Criteria
                                                               {
                                                                   AcademicLevelId = academicLevelId,
                                                                   TermId = model.TermId
                                                               });
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }

            _voucherProvider.UpdateVoucherModel(model);
            CreateSelectList(model.Student.AcademicInformation.AcademicLevelId);
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("Voucher", PolicyGenerator.Write)]
        public IActionResult Delete(long id, string returnUrl)
        {
            var voucher = _voucherProvider.GetVoucherById(id);
            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _scholarshipProvider.InactiveTransactionFromVoucher(id);
                    voucher.IsActive = false;
                    _db.SaveChanges();

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToDelete);
                }
            }

            return Redirect(returnUrl);
        }

        private void CreateSelectList(long academicLevelId) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Statuses = _selectListProvider.GetVoucherStatus();
            ViewBag.FeeItems = _selectListProvider.GetFeeItems();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}