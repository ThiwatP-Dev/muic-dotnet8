using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using KeystoneLibrary.Models.DataModels;
using Newtonsoft.Json;

namespace Keystone.Controllers.MasterTables
{
    public class TeachingScheduleController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IScheduleProvider _scheduleProvider;
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        
        public TeachingScheduleController(ApplicationDbContext db,
                                          IFlashMessage flashMessage,
                                          ISelectListProvider selectListProvider,
                                          IMapper mapper,
                                          IDateTimeProvider dateTimeProvider,
                                          IScheduleProvider scheduleProvider,
                                          IAcademicProvider academicProvider,
                                          IRegistrationProvider registrationProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _scheduleProvider = scheduleProvider;
            _academicProvider = academicProvider;
            _registrationProvider = registrationProvider;
        }

        public ActionResult Index(Criteria criteria)
        {
            TeachingScheduleViewModel model = new TeachingScheduleViewModel();
            model.Criteria = criteria;
            CreateSelectList(criteria.CampusId, criteria.BuildingId);
            DateTime? date = _dateTimeProvider.ConvertStringToDateTime(criteria.Date);
            if (criteria.RoomId == 0 || criteria.Date == null)
            {
                model.Criteria.Date = DateTime.Today.ToString(StringFormat.ShortDate);
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }
            
            var roomSlots = _db.RoomSlots.Include(x => x.Room)
                                             .ThenInclude(x => x.Building)
                                             .ThenInclude(x => x.Campus)
                                         .Include(x => x.SectionSlot)
                                             .ThenInclude(x => x.Section)
                                             .ThenInclude(x => x.Course)
                                         .Include(x => x.SectionSlot)
                                             .ThenInclude(x => x.Room)
                                         .Include(x => x.SectionSlot)
                                             .ThenInclude(x => x.Instructor)
                                         .Include(x => x.RoomReservation)
                                             .ThenInclude(x => x.Room)
                                         .Include(x => x.ExaminationReservation)
                                             .ThenInclude(x => x.Instructor)
                                         .Include(x => x.ExaminationReservation)
                                             .ThenInclude(x => x.Section)
                                             .ThenInclude(x => x.Course)
                                         .Include(x => x.ExaminationReservation)
                                             .ThenInclude(x => x.Room)
                                         .Include(x => x.Term)
                                         .Where(x => (criteria.CampusId == 0
                                                      || x.Room.Building.CampusId == criteria.CampusId)
                                                      && (criteria.BuildingId == 0
                                                          || x.Room.BuildingId == criteria.BuildingId)
                                                      && (criteria.RoomId == 0
                                                          || x.RoomId == criteria.RoomId)
                                                      && (criteria.Floor == null
                                                          || x.Room.Floor == criteria.Floor)
                                                      && x.Date == date
                                                      && x.UsingType == criteria.Type)
                                         .ToList();

            var teaching = roomSlots.Select(x => _mapper.Map<RoomSlot, TeachingScheduleDetail>(x))
                                    .ToList();

            var room = _db.Rooms.SingleOrDefault(x => x.Id == criteria.RoomId);
            model.RoomName = room.NameEn;
            model.BuildingName = room.Building?.NameEn;
            model.Campus = room.Building?.Campus?.NameEn;
            model.TeachingScheduleDetails = teaching;
            model.Schedules = _scheduleProvider.GetTeachingSchedule(roomSlots, criteria.Type);
            ViewBag.CoursesJsonData = JsonConvert.SerializeObject(model.Schedules);

            return View(model);
        }

        private void CreateSelectList(long campusId = 0, long buildingId = 0)
        {
            ViewBag.UsingTypes = _selectListProvider.GetUsingTypes();
            ViewBag.Campuses = _selectListProvider.GetCampuses();
            ViewBag.Buildings = _selectListProvider.GetBuildings(campusId);
            ViewBag.Rooms = _selectListProvider.GetRooms(buildingId);
        }
    }
}