using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("RoomSchedule", "")]
    public class RoomScheduleController : BaseController
    {
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IScheduleProvider _scheduleProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IRoomProvider _roomProvider;

        public RoomScheduleController(ApplicationDbContext db,
                                      IFlashMessage flashMessage,
                                      ISelectListProvider selectListProvider,
                                      IScheduleProvider scheduleProvider,
                                      IAcademicProvider academicProvider,
                                      IDateTimeProvider dateTimeProvider,
                                      IRoomProvider roomProvider,
                                      IMapper mapper) : base(db, flashMessage, mapper, selectListProvider) 
        { 
            _academicProvider = academicProvider;
            _scheduleProvider = scheduleProvider;
            _dateTimeProvider = dateTimeProvider;
            _roomProvider = roomProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.CampusId, criteria.BuildingId, criteria.AcademicLevelId);
            if (criteria.TermId == 0 || string.IsNullOrEmpty(criteria.Date))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var model = new RoomScheduleViewModel();
            DateTime? date = _dateTimeProvider.ConvertStringToDateTime(criteria.Date);
            var thisWeekStart = date.Value.AddDays(-(int)date.Value.DayOfWeek);
            var thisWeekEnd = thisWeekStart.AddDays(7);
            var roomIds = _db.RoomSlots.Where(x => x.Date >= thisWeekStart && x.Date < thisWeekEnd
                                                   && x.TermId == criteria.TermId
                                                   && (string.IsNullOrEmpty(criteria.Type) || x.UsingType == criteria.Type))
                                       .Select(x => x.RoomId)
                                       .Distinct();

            model.Rooms = _db.Rooms.Include(x => x.RoomType)
                                   .Include(x => x.Building)
                                       .ThenInclude(x => x.Campus)
                                   .Where(x => roomIds.Contains(x.Id)
                                               && (criteria.RoomIds == null
                                                   || criteria.RoomIds.Contains(x.Id))
                                               && (criteria.CampusId == 0 
                                                   || x.Building.CampusId == criteria.CampusId)
                                               && (criteria.BuildingId == 0 
                                                   || x.BuildingId == criteria.BuildingId) 
                                               && (criteria.RoomtypeId == 0 
                                                   || x.RoomTypeId == criteria.RoomtypeId)
                                               && (criteria.CapacityFrom == null
                                                   || x.Capacity >= criteria.CapacityFrom)
                                               && (criteria.CapacityTo == null
                                                   || x.Capacity <= criteria.CapacityTo)
                                               && (string.IsNullOrEmpty(criteria.Type)
                                                   || x.RoomSlots.Any(y => y.UsingType == criteria.Type))
                                               && (Convert.ToBoolean(criteria.IsMakeUp) ? x.RoomSlots.Any(y => y.SectionSlotId != null
                                                                                                               && y.SectionSlot.IsMakeUpClass
                                                                                                               && !y.IsCancel)
                                                                                        : true ))
                                   .OrderBy(x => x.NameEn)
                                   .ToList();
   
            model.Criteria = criteria;
            return View(model);
        }

        public ActionResult Preview(List<long> roomIds, long termId, string date, string isMakeUp, string type, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = new RoomScheduleViewModel();
            var report = new ReportViewModel();
            var previews = new List<RoomSchedulePreview>();
            var term = _academicProvider.GetTerm(termId);

            DateTime? dateDate = _dateTimeProvider.ConvertStringToDateTime(date);
            var thisWeekStart = dateDate.Value.AddDays(-(int)dateDate.Value.DayOfWeek);
            var thisWeekEnd = thisWeekStart.AddDays(6);

            foreach (var item in roomIds)
            {
                var room = _roomProvider.GetRoom(item);
                var roomSlots = _roomProvider.GetWeeklyRoomSlotsByRoomAndTermAndDate(item, termId, date, isMakeUp, type);
                var preview = new RoomSchedulePreview
                              {
                                  Name = room.NameEn,
                                  Term = term.TermText,
                                  BuildingName = room.Building?.NameEn,
                                  CampusName = room.Building?.Campus?.NameEn,
                                  DateString = $"{thisWeekStart.ToString(StringFormat.ShortDate)} - {thisWeekEnd.ToString(StringFormat.ShortDate)}",
                                  PrintDateString = DateTime.UtcNow.AddHours(7).ToString(StringFormat.ShortDateTime),
                                  Schedules = _scheduleProvider.GetRoomSchedulePreview(roomSlots)
                              };

                previews.Add(preview);
            }

            model.Preview = previews;
            report.Body = model;
            return View(report);
        }

        public ActionResult Details(long roomId, long termId, string date, string type, string isMakeUp, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            RoomScheduleViewModel model = new RoomScheduleViewModel();
            var roomSlots = _roomProvider.GetWeeklyRoomSlotsByRoomAndTermAndDate(roomId, termId, date, isMakeUp, type);
            var room = _roomProvider.GetRoom(roomId);
            var term = _academicProvider.GetTerm(termId);
            model.Name = room.NameEn;
            model.BuildingNameEn = room.Building?.NameEn;
            model.CampusNameEn = room.Building?.Campus?.NameEn;
            model.Term = term == null ? "" : term.TermText;
            model.Schedules = _scheduleProvider.GetRoomSchedulePreview(roomSlots, type);

            DateTime? dateDate = _dateTimeProvider.ConvertStringToDateTime(date);
            var thisWeekStart = dateDate.Value.AddDays(-(int)dateDate.Value.DayOfWeek);
            var thisWeekEnd = thisWeekStart.AddDays(6);
            model.DateString = $"{thisWeekStart.ToString(StringFormat.ShortDate)} - {thisWeekEnd.ToString(StringFormat.ShortDate)}";
            model.PrintDateString = DateTime.UtcNow.AddHours(7).ToString(StringFormat.ShortDateTime);
            model.RoomId = roomId;
            model.Date = date;
            model.TermId = termId;
            model.IsMakeUp = isMakeUp;

            ViewBag.CoursesJsonData = JsonConvert.SerializeObject(model.Schedules);

            return View(model);
        }

        private void CreateSelectList(long campusId, long buildingId, long academicLevelId)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Campuses = _selectListProvider.GetCampuses();
            ViewBag.RoomTypes = _selectListProvider.GetRoomTypes();
            ViewBag.UsingTypes = _selectListProvider.GetUsingTypes();
            ViewBag.Buildings = campusId == 0 ? _selectListProvider.GetBuildings()
                                              : _selectListProvider.GetBuildings(campusId);
            ViewBag.Rooms = buildingId == 0 ? _selectListProvider.GetRooms()
                                            : _selectListProvider.GetRooms(buildingId);
        }
    }
}