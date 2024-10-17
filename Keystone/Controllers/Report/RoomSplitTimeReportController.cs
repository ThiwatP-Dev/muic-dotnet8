using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using KeystoneLibrary.Data;

namespace Keystone.Controllers.Report
{
    public class RoomSplitTimeReportController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;

        public RoomSplitTimeReportController(ApplicationDbContext db,
                                             ISelectListProvider selectListProvider,
                                             IFlashMessage flashMessage,
                                             IDateTimeProvider dateTimeProvider) : base(db, flashMessage, selectListProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.CampusId);
            DateTime? starteAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);
            if (starteAt == null && endedAt == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var rooms = (from roomSlot in _db.RoomSlots
                         join room in _db.Rooms on roomSlot.RoomId equals room.Id
                         join building in _db.Buildings on room.BuildingId equals building.Id
                         join term in _db.Terms on roomSlot.TermId equals term.Id
                         join termType in _db.TermTypes on term.TermTypeId equals termType.Id
                         join sectionSlot in _db.SectionSlots on roomSlot.SectionSlotId equals sectionSlot.Id into g1
                         from sectionSlot in g1.DefaultIfEmpty()
                         where !roomSlot.IsCancel
                               && roomSlot.Date.Date >= starteAt.Value.Date
                               && roomSlot.Date.Date <= endedAt.Value.Date
                               && (string.IsNullOrEmpty(criteria.Type)
                                   || roomSlot.UsingType == criteria.Type)
                               && (criteria.CampusId == 0
                                   || building.CampusId == criteria.CampusId)
                               && (criteria.BuildingId == 0
                                   || room.BuildingId == criteria.BuildingId)
                               && (criteria.Floor == null
                                   || room.Floor == criteria.Floor)
                               && (string.IsNullOrEmpty(criteria.CodeAndName)
                                   || room.NameEn.Contains(criteria.CodeAndName))
                               && (string.IsNullOrEmpty(criteria.IsMakeUp)
                                   || (roomSlot.SectionSlotId != null 
                                       && sectionSlot.IsMakeUpClass == Convert.ToBoolean(criteria.IsMakeUp)))
                         select new RoomSplitTimeReportViewModel
                                {
                                    Building = building.NameEn,
                                    Room = room.NameEn,
                                    TermType = termType.NameEn,
                                    UsingType = roomSlot.UsingTypeText,
                                    Term = term.AcademicTerm.ToString() + '/' + term.AcademicYear.ToString(),
                                    DateText = roomSlot.DateText,
                                    DayOfWeek = roomSlot.Date.DayOfWeek.ToString(),
                                    IsMakeUp = sectionSlot != null ? sectionSlot.IsMakeUpClass : false,
                                    TimeText = roomSlot.StartTimeText + "-" + roomSlot.EndTimeText,
                                    Eight = roomSlot.StartTime.Hours <= 8 && roomSlot.EndTime.Hours >= 8 ? 1 : 0,
                                    Nine = roomSlot.StartTime.Hours <= 9 && roomSlot.EndTime.Hours >= 9 ? 1 : 0,
                                    Ten = roomSlot.StartTime.Hours <= 10 && roomSlot.EndTime.Hours >= 10 ? 1 : 0,
                                    Eleven = roomSlot.StartTime.Hours <= 11 && roomSlot.EndTime.Hours >= 11 ? 1 : 0,
                                    Twelve = roomSlot.StartTime.Hours <= 12 && roomSlot.EndTime.Hours >= 12 ? 1 : 0,
                                    Thirteen = roomSlot.StartTime.Hours <= 13 && roomSlot.EndTime.Hours >= 13 ? 1 : 0,
                                    Fourteen = roomSlot.StartTime.Hours <= 14 && roomSlot.EndTime.Hours >= 14 ? 1 : 0,
                                    Fifteen = roomSlot.StartTime.Hours <= 15 && roomSlot.EndTime.Hours >= 15 ? 1 : 0,
                                    Sixteen = roomSlot.StartTime.Hours <= 16 && roomSlot.EndTime.Hours >= 16 ? 1 : 0,
                                    Seventeen = roomSlot.StartTime.Hours <= 17 && roomSlot.EndTime.Hours >= 17 ? 1 : 0,
                                    Eighteen = roomSlot.StartTime.Hours <= 18 && roomSlot.EndTime.Hours >= 18 ? 1 : 0,
                                    Nineteen = roomSlot.StartTime.Hours <= 19 && roomSlot.EndTime.Hours >= 19 ? 1 : 0,
                                    Twenty = roomSlot.StartTime.Hours <= 20 && roomSlot.EndTime.Hours >= 20 ? 1 : 0
                                }).AsQueryable()
                                .GetAllPaged(criteria);

            return View(rooms);
        }

        private void CreateSelectList(long campusId = 0)
        {
            ViewBag.Campuses = _selectListProvider.GetCampuses();
            ViewBag.UsingTypes = _selectListProvider.GetUsingTypes();
            ViewBag.Answers = _selectListProvider.GetAllYesNoAnswer();
            if (campusId > 0)
            {
                ViewBag.Buildings = _selectListProvider.GetBuildings(campusId);
            }
        }
    }
}