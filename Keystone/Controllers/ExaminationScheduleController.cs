using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("ExaminationSchedule", "")]
    public class ExaminationScheduleController : BaseController
    {
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IScheduleProvider _scheduleProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;

        public ExaminationScheduleController(ApplicationDbContext db,
                                             ISelectListProvider selectListProvider,
                                             IScheduleProvider scheduleProvider,
                                             IAcademicProvider academicProvider,
                                             IFlashMessage flashMessage,
                                             IMapper mapper,
                                             IDateTimeProvider dateTimeProvider) : base(db, flashMessage, mapper, selectListProvider) 
        { 
            _academicProvider = academicProvider;
            _scheduleProvider = scheduleProvider;
            _dateTimeProvider = dateTimeProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.CampusId, criteria.BuildingId, criteria.AcademicLevelId);
            DateTime? asOfAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0 || String.IsNullOrEmpty(criteria.Type) || asOfAt == null)
            {
                 _flashMessage.Warning(Message.RequiredData);
                 return View();
            }

            // Get Room id follow criteria
            var roomIds = _db.Rooms.Include(x => x.RoomType)
                                 .Include(x => x.Building)
                                     .ThenInclude(x => x.Campus)
                                 .Where(x => (criteria.CampusId == 0 
                                              || x.Building.CampusId == criteria.CampusId)
                                                 && (criteria.BuildingId == 0 
                                                     || x.BuildingId == criteria.BuildingId) 
                                                 && (criteria.RoomtypeId == 0 
                                                     || x.RoomTypeId == criteria.RoomtypeId)
                                                 && (criteria.RoomIds == null
                                                     || criteria.RoomIds.Contains(x.Id)))
                                 .Select(x => x.Id)
                                 .ToList();

            var sections = _db.Sections.Include(x => x.MidtermRoom)
                                           .ThenInclude(x => x.Building)
                                       .Include(x => x.MidtermRoom)
                                           .ThenInclude(x => x.RoomType)
                                       .Include(x => x.FinalRoom)
                                           .ThenInclude(x => x.Building)
                                       .Include(x => x.FinalRoom)
                                           .ThenInclude(x => x.RoomType)
                                       .Where(x => criteria.TermId == x.TermId
                                                   && ((criteria.Type == "Midterm"
                                                        && roomIds.Contains(x.MidtermRoomId ?? 0)
                                                        && x.MidtermDateValue == asOfAt.Value.Date)
                                                       || (criteria.Type == "Final"
                                                           && roomIds.Contains(x.FinalRoomId ?? 0)
                                                           && x.FinalDateValue == asOfAt.Value.Date)))
                                       .ToList();

            var models = new List<ExaminationViewModel>();
            if (criteria.Type == "Midterm")
            {
                models = sections.GroupBy(x => new { x.MidtermDateValue, x.MidtermRoomId })
                                 .Select(x => new ExaminationViewModel
                                              {
                                                  Id = x.First().MidtermRoomId.HasValue ? x.First().MidtermRoomId.Value : 0,
                                                  NameEn = x.First().MidtermRoom.NameEn,
                                                  Floor = x.First().MidtermRoom.Floor,
                                                  Capacity = x.First().MidtermRoom.Capacity,
                                                  RoomTypeName = x.First().MidtermRoom.RoomType.Name,
                                                  BuildingName = x.First().MidtermRoom.Building.NameEn,
                                                  ExaminationDate = x.Key.MidtermDateValue.Date
                                              })
                                 .ToList();
            }
            else 
            {
                models = sections.GroupBy(x => new { x.FinalDateValue, x.FinalRoomId })
                                 .Select(x => new ExaminationViewModel
                                              {
                                                  Id = x.First().FinalRoomId.HasValue ? x.First().FinalRoomId.Value : 0,
                                                  NameEn = x.First().FinalRoom.NameEn,
                                                  Floor = x.First().FinalRoom.Floor,
                                                  Capacity = x.First().FinalRoom.Capacity,
                                                  RoomTypeName = x.First().FinalRoom.RoomType.Name,
                                                  BuildingName = x.First().FinalRoom.Building.NameEn,
                                                  ExaminationDate = x.Key.FinalDateValue.Date
                                              })
                                 .ToList();
            }
            
            var modelPageResult = models.OrderBy(x => x.NameEn)
                                        .AsQueryable()
                                        .GetPaged(criteria, page);
                                        
            return View(modelPageResult);
        }

        public ActionResult Details(long roomId, long termId, string date, string type, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            DateTime? asOfAt = _dateTimeProvider.ConvertStringToDateTime(date) ?? DateTime.Today;
            RoomScheduleViewModel model = new RoomScheduleViewModel();
            model = _db.Rooms.Include(x => x.Building)
                                 .ThenInclude(x => x.Campus)
                             .Where(x => x.Id == roomId)
                             .Select(x => _mapper.Map<Room, RoomScheduleViewModel>(x))
                             .SingleOrDefault();

            var sectionIds = _db.Sections.Include(x => x.Course)
                                         .Include(x => x.MidtermRoom)
                                         .Include(x => x.FinalRoom)
                                         .Where(x => !x.IsClosed
                                                      && x.TermId == termId
                                                      && ((type == "Midterm"
                                                            && x.MidtermRoomId == roomId
                                                            && x.MidtermDateValue == asOfAt.Value.Date)
                                                           || (type == "Final"
                                                               && x.FinalRoomId == roomId
                                                               && x.FinalDateValue == asOfAt.Value.Date)))
                                         .Select(x => x.Id)
                                         .ToList();

            model.Schedules = _scheduleProvider.GetExaminationSchedule(sectionIds, type);
            model.DateString = date;
            var termExam = _academicProvider.GetTerm(termId);
            model.Term = termExam == null ? "N/A" : termExam.TermText;
            ViewBag.CoursesJsonData = JsonConvert.SerializeObject(model.Schedules); 
             
            return View(model);
        }

        private void CreateSelectList(long campusId, long buildingId, long academicLevelId)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Campuses = _selectListProvider.GetCampuses();
            ViewBag.RoomTypes = _selectListProvider.GetRoomTypes();
            ViewBag.Buildings = _selectListProvider.GetBuildings(campusId);
            ViewBag.Rooms = _selectListProvider.GetRooms(buildingId);
            ViewBag.Types = _selectListProvider.GetMidtermAndFinal();
        }
    }
}