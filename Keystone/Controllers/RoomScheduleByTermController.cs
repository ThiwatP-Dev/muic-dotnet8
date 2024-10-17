using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("RoomScheduleByTerm", "")]
    public class RoomScheduleByTermController : BaseController
    {
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IScheduleProvider _scheduleProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IRoomProvider _roomProvider;
        protected readonly ISectionProvider _sectionProvider;

        public RoomScheduleByTermController(ApplicationDbContext db,
                                            IFlashMessage flashMessage,
                                            ISelectListProvider selectListProvider,
                                            IScheduleProvider scheduleProvider,
                                            IAcademicProvider academicProvider,
                                            IDateTimeProvider dateTimeProvider,
                                            IRoomProvider roomProvider,
                                            ISectionProvider sectionProvider,
                                            IMapper mapper) : base(db, flashMessage, mapper, selectListProvider) 
        { 
            _academicProvider = academicProvider;
            _scheduleProvider = scheduleProvider;
            _dateTimeProvider = dateTimeProvider;
            _roomProvider = roomProvider;
            _sectionProvider = sectionProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.CampusId, criteria.BuildingId, criteria.AcademicLevelId);
            if (criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var model = new RoomScheduleByTermViewModel();
            var roomIds = criteria.RoomIds == null ? _db.SectionDetails.Where(x => x.Section.TermId == criteria.TermId
                                                                                   && (string.IsNullOrEmpty(criteria.Status)
                                                                                       || x.Section.Status == criteria.Status)
                                                                                   && x.RoomId != null
                                                                                   && !x.Section.IsClosed)
                                                                        .Select(x => (long)x.RoomId)
                                                                        .Distinct()
                                                                        .ToList()
                                                   : criteria.RoomIds;

            model.Rooms = _db.Rooms.Where(x => roomIds.Contains(x.Id)
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
                                               && !x.IsOnline)
                                    .Select(x => new RoomScheduleInfoByTermViewModel
                                                 {
                                                     Id = x.Id,
                                                     NameEn = x.NameEn,
                                                     Floor = x.Floor,
                                                     Capacity = x.Capacity,
                                                     RoomType = x.RoomType.Name,
                                                     BuildingName = x.Building.NameEn
                                                 })
                                   .OrderBy(x => x.NameEn)
                                   .ToList();

            model.Criteria = criteria;
            model.RoomIds = model.Rooms.Select(x => x.Id).ToList();
            return View(model);
        }

        public ActionResult Preview(List<long> roomIds, long termId, string sectionStatus, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = new RoomScheduleViewModel();
            var report = new ReportViewModel();
            var previews = new List<RoomSchedulePreview>();
            var term = _academicProvider.GetTerm(termId);

            foreach (var item in roomIds)
            {
                var room = _roomProvider.GetRoom(item);
                var sectionDetails = _sectionProvider.GetSectionDetailsByRoomAndTerm(item, termId, sectionStatus);
                var preview = new RoomSchedulePreview
                              {
                                  Name = room.NameEn,
                                  Term = term.TermText,
                                  BuildingName = room.Building?.NameEn,
                                  CampusName = room.Building?.Campus?.NameEn,
                                  PrintDateString = DateTime.UtcNow.AddHours(7).ToString(StringFormat.ShortDateTime),
                                  Schedules = _scheduleProvider.GetRoomSchedulePreview(sectionDetails)
                              };

                previews.Add(preview);
            }

            model.Preview = previews.OrderBy(x => x.Name).ToList();
            report.Body = model;
            return View(report);
        }

        public ActionResult Details(long roomId, long termId, string sectionStatus, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            RoomScheduleDetailByTermPreview model = new RoomScheduleDetailByTermPreview();
            var sectionDetails = _sectionProvider.GetSectionDetailsByRoomAndTerm(roomId, termId, sectionStatus);
            var room = _roomProvider.GetRoom(roomId);
            var term = _academicProvider.GetTerm(termId);
            model.Name = room.NameEn;
            model.BuildingName = room.Building?.NameEn;
            model.CampusName = room.Building?.Campus?.NameEn;
            model.Term = term == null ? "" : term.TermText;
            model.Schedules = _scheduleProvider.GetRoomSchedulePreview(sectionDetails);

            model.PrintDateString = DateTime.UtcNow.AddHours(7).ToString(StringFormat.ShortDateTime);
            model.RoomId = roomId;
            model.TermId = termId;
            model.SectionStatus = sectionStatus;

            ViewBag.CoursesJsonData = JsonConvert.SerializeObject(model.Schedules);

            return View(model);
        }

        private void CreateSelectList(long campusId, long buildingId, long academicLevelId)
        {
            ViewBag.SectionStatuses = _selectListProvider.GetSectionStatuses();
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