using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("StudyRoomSummaryReport", "")]
    public class StudyRoomSummaryReportController : BaseController
    {
        public StudyRoomSummaryReportController(ApplicationDbContext db,
                                                IFlashMessage flashMessage,
                                                ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.CampusId, criteria.BuildingId);

            var model = new StudyRoomSummaryReportViewModel
                        {
                            Criteria = criteria
                        };

            if (criteria.AcademicYear == 0 || criteria.AcademicYear == null
                || criteria.AcademicTerm == 0 || criteria.AcademicTerm == null
                || criteria.YearFrom == 0 || criteria.MonthFrom == 0 || criteria.MonthFrom == null
                || criteria.YearTo == 0 || criteria.MonthTo == 0 || criteria.MonthTo == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var startDate = new DateTime(criteria.YearFrom, criteria.MonthFrom ?? 0, 1, 0, 0, 0); 
            var endedDate = new DateTime(criteria.YearTo, criteria.MonthTo ?? 0, 1, 0, 0, 0); 
            endedDate = endedDate.AddMonths(1).AddDays(-1);
            model.MonthYearList = MonthYearList(startDate, endedDate);

            var termIds = _db.Terms.Where(x => x.AcademicYear == criteria.AcademicYear
                                               && x.AcademicTerm == criteria.AcademicTerm)
                                   .Select(x => x.Id)
                                   .ToList();

            model.StudyRoomSummaryReports = _db.RoomSlots.Include(x => x.Room)
                                                             .ThenInclude(x => x.Building)
                                                         .Where(x => termIds.Contains(x.TermId)
                                                                     && !x.IsCancel
                                                                     && (string.IsNullOrEmpty(criteria.Type)
                                                                         || x.UsingType == criteria.Type)
                                                                     && (startDate <= x.Date
                                                                         && x.Date <= endedDate)
                                                                     && (criteria.CampusId == 0
                                                                         || x.Room.Building.CampusId == criteria.CampusId)
                                                                     && (criteria.BuildingId == 0
                                                                         || x.Room.BuildingId == criteria.BuildingId)
                                                                     && (criteria.RoomId == 0
                                                                         || x.RoomId == criteria.RoomId)
                                                                     && (criteria.Floor == 0
                                                                         || criteria.Floor == null
                                                                         || x.Room.Floor == criteria.Floor))
                                                         .GroupBy(x => x.Room.Building.NameEn)
                                                         .Select(x => new StudyRoomSummaryReport
                                                                      {
                                                                          BuildingName = x.Key,
                                                                          StudyRoomSummaries = x.GroupBy(y => y.Date.ToString(StringFormat.MonthNameYear))
                                                                                                .Select(y => new StudyRoomSummary
                                                                                                             {
                                                                                                                 Month = y.Key,
                                                                                                                 TotalHours = y.Sum(z => z.TotalHours)
                                                                                                             })
                                                                                                .ToList()
                                                                      })
                                                         .ToList();
            return View(model);
        }

        private void CreateSelectList(long academicLevelId = 0, long campusId = 0, long buildingId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }

            ViewBag.UsingTypes = _selectListProvider.GetUsingTypes();
            ViewBag.Campuses = _selectListProvider.GetCampuses();
            ViewBag.Buildings = campusId == 0 ? _selectListProvider.GetBuildings()
                                              : _selectListProvider.GetBuildings(campusId);
            ViewBag.Rooms = buildingId == 0 ? _selectListProvider.GetRooms()
                                            : _selectListProvider.GetRooms(buildingId);
            ViewBag.Month = _selectListProvider.GetMonth();
        }

        private List<string> MonthYearList(DateTime startDate, DateTime endDate)
        {
            var dateMonths = new List<string>();
            for (DateTime date = startDate; date <= endDate; date = date.AddMonths(1))
            {
                dateMonths.Add(date.ToString(StringFormat.MonthNameYear));
            }
            return dateMonths;
        }
    }
}