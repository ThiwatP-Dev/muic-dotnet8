using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("AllSectionTimeSlotReport", "")]
    public class AllSectionTimeSlotReportController : BaseController
    {
        protected readonly IReservationProvider _reservationProvider;
        public AllSectionTimeSlotReportController(ApplicationDbContext db,
                                                  IFlashMessage flashMessage,
                                                  ISelectListProvider selectListProvider,
                                                  IReservationProvider reservationProvider) : base(db, flashMessage, selectListProvider)
        {
            _reservationProvider = reservationProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.CampusId);
            var model = new AllSectionTimeSlotReportViewModel
            {
                Criteria = criteria
            };

            if (string.IsNullOrEmpty(criteria.StartedAt) || string.IsNullOrEmpty(criteria.EndedAt))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            try
            {
                model = _reservationProvider.GetAllSectionTimeSlotReport(criteria);
                if (model.Items != null && model.Items.Any())
                {
                    var allModel = new AllSectionTimeSlotReportViewModel.AllSectionTimeSlotByUsingTypeReportItem
                    {
                        UsingType = "All",
                        UsingTypeText = "All"
                    };
                    foreach (var header in model.Items[0].Header)
                    {
                        allModel.Header.Add(header);
                    }
                    var allRowData = model.Items.SelectMany(x => x.Rows).ToList();
                    for (int rowIndex = 0; rowIndex < model.Items[0].Rows.Count; rowIndex++)
                    {
                        var dummyRow = model.Items[0].Rows[rowIndex];

                        var allSameRow = allRowData.Where(x => x.DayOfWeekText == dummyRow.DayOfWeekText);
                        var newRow = new AllSectionTimeSlotReportViewModel.AllSectionTimeSlotByUsingTypeReportItem.RowData
                        {
                            DayOfWeekText = dummyRow.DayOfWeekText,
                        };
                        for (int colIndex = 0; colIndex < dummyRow.Values.Count; colIndex++)
                        {
                            var sumAllCol = allSameRow.Sum(x => x.Values[colIndex]);
                            newRow.Values.Add(sumAllCol);
                        }
                        allModel.Rows.Add(newRow);
                    }
                    model.Items.Add(allModel);
                }
            }
            catch (Exception ex)
            {
                _flashMessage.Warning(Message.UnableToSearch + " " + ex.Message);
            }

            return View(model);
        }

        private void CreateSelectList(long campusId = 0)
        {
            ViewBag.Campuses = _selectListProvider.GetCampuses();
            ViewBag.UsingTypes = _selectListProvider.GetUsingTypes();
            ViewBag.Rooms = _selectListProvider.GetRooms();
            if (campusId > 0)
            {
                ViewBag.Buildings = _selectListProvider.GetBuildings(campusId);
            }

        }
    }
}