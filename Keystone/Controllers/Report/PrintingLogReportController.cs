using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("PrintingLogReport", "")]
    public class PrintingLogReportController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IExceptionManager _exceptionManager;
        protected readonly IPrintingLogProvider _printingLogProvider;

        public PrintingLogReportController(ApplicationDbContext db,
                                           IMapper mapper,
                                           IFlashMessage flashMessage,
                                           ISelectListProvider selectListProvider,
                                           IDateTimeProvider dateTimeProvider,
                                           IExceptionManager exceptionManager,
                                           IPrintingLogProvider printingLogProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _exceptionManager = exceptionManager;
            _printingLogProvider = printingLogProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList();
            var query = _printingLogProvider.GetPrintingLogs(criteria);
                                          
            var model = new PrintingLogReportViewModel
                        {
                            PrintingLogReportStatistics = new PrintingLogReportStatistics
                                                          {
                                                              All = query.Count(),
                                                              Male = query.Count(x => x.Gender == 1),
                                                              Female = query.Count(x => x.Gender == 2),
                                                              Undefined = query.Count(x => x.Gender == 0),
                                                              Normal = query.Count(x => !x.IsUrgent),
                                                              Urgent = query.Count(x => x.IsUrgent),
                                                              English = query.Count(x => x.Language == "en"),
                                                              Thai = query.Count(x => x.Language == "th"),
                                                              Paid = query.Count(x => x.IsPaid),
                                                              Unpaid = query.Count(x => !x.IsPaid)
                                                          },
                            PrintingLogReports = query.OrderBy(x => x.PrintedAt)
                                                          .ThenBy(x => x.Gender)
                                                          .ThenBy(x => x.ReferenceNumber)
                                                      .ToList()
                        };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public bool SaveTrackingNumber([FromBody] UpdateTrackingNumberViewModel model)
        {
            try
            {
                var printingLog = _db.PrintingLogs.SingleOrDefault(x => x.Id == model.PrintingLogId);
                printingLog.TrackingNumber = model.TrackingNumber;
                _db.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void CreateSelectList()
        {
            ViewBag.Gender = _selectListProvider.GetGender();
            ViewBag.CertificateTypes = _selectListProvider.GetCertificateTypes();
            ViewBag.Languages = _selectListProvider.GetLanguages();
            ViewBag.UrgentStatuses = _selectListProvider.GetUrgentStatuses();
            ViewBag.PrintStatuses = _selectListProvider.GetPrintStatuses();
            ViewBag.PaidStatuses = _selectListProvider.GetPaidStatuses();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
        }
    }
}