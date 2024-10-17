using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("WaitingPaymentWithAddressReport", "")]
    public class WaitingPaymentWithAddressReportController : BaseController
    {
        private IReceiptProvider _receiptProvider;
        public WaitingPaymentWithAddressReportController(ApplicationDbContext db,
                                                  IFlashMessage flashMessage,
                                                  ISelectListProvider selectListProvider,
                                                  IReceiptProvider receiptProvider) : base(db, flashMessage, selectListProvider)
        {
            _receiptProvider = receiptProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            var model = new WaitingPaymentWithAddressReportViewModel();
            model.Criteria = criteria;

            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            model = _receiptProvider.GetWaitingPaymentWithAddressReport(criteria);

            return View(model);
        }

        private void CreateSelectList(long academicLevelId, long facultyId)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.InvoiceType = _selectListProvider.GetInvoiceType();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }
            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartments(facultyId);
            }

            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();
            ViewBag.ResidentTypes = _selectListProvider.GetResidentTypes();
            ViewBag.AdmissionTypes = _selectListProvider.GetAdmissionTypes();
            ViewBag.StudentFeeGroups = _selectListProvider.GetStudentFeeGroups();
        }
    }
}