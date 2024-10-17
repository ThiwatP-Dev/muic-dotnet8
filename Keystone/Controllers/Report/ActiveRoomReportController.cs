using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Data;

namespace Keystone.Controllers.Report
{
    public class ActiveRoomReportController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;

        public ActiveRoomReportController(ApplicationDbContext db,
                                          ISelectListProvider selectListProvider,
                                          IFlashMessage flashMessage,
                                          IDateTimeProvider dateTimeProvider) : base (db, flashMessage, selectListProvider) 
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.CampusId);
            DateTime? date = _dateTimeProvider.ConvertStringToDateTime(criteria.Date);
            if (date == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var model = new ActiveRoomReportViewModel
                        {
                            Criteria = criteria
                        };

            model.Details = _db.RoomSlots.Include(x => x.Room)
                                             .ThenInclude(x => x.Building)
                                             .ThenInclude(x => x.Campus)
                                         .Include(x => x.RoomReservation)
                                         .Include(x => x.ExaminationReservation).ThenInclude(x => x.Section).ThenInclude(x => x.Course)
                                         .Include(x => x.SectionSlot).ThenInclude(x => x.Section).ThenInclude(x => x.Course)
                                         .Where(x => x.Date.Date == date.Value.Date
                                                     && ( (x.SectionSlot != null && x.SectionSlot.Status != "c" && !x.IsCancel )
                                                        || (x.ExaminationReservation != null && x.ExaminationReservation.Status != "r" && !x.IsCancel)
                                                        || (x.RoomReservation != null && x.RoomReservation.Status != "r" && !x.IsCancel)
                                                     )
                                                     && (string.IsNullOrEmpty(criteria.Type)
                                                         || x.UsingType == criteria.Type)
                                                     && (criteria.CampusId == 0
                                                         || x.Room.Building.CampusId == criteria.CampusId)
                                                     && (criteria.BuildingId == 0
                                                         || x.Room.BuildingId == criteria.BuildingId)
                                                     && (criteria.Floor == null
                                                         || x.Room.Floor == criteria.Floor)
                                                     && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                         || x.Room.NameEn.Contains(criteria.CodeAndName)))
                                         //.GroupBy(x => x.RoomId)
                                         .Select(x => new ActiveRoomReportDetail
                                                      {
                                             //Name = x.FirstOrDefault().Room.NameEn,
                                             //Campus = x.FirstOrDefault().Room.Building.Campus.NameEn,
                                             //Building = x.FirstOrDefault().Room.Building.NameEn,
                                             //Floor = x.FirstOrDefault().Room.Floor,
                                             //RoomSlots = x.OrderBy(y => y.StartTime)
                                             //                 .ThenBy(y => y.EndTime)
                                             //             .ToList()
                                             Name = x.Room.NameEn,
                                             Campus = x.Room.Building.Campus.NameEn,
                                             Building = x.Room.Building.NameEn,
                                             Floor = x.Room.Floor,
                                             RoomSlot =  x 
                                         })
                                         .OrderBy(x => x.Campus)
                                             .ThenBy(x => x.Building)
                                             .ThenBy(x => x.Floor)
                                             .ThenBy(x => x.Name)
                                         .ToList();

            return View(model);
        }

        private void CreateSelectList(long campusId = 0)
        {
            ViewBag.Campuses = _selectListProvider.GetCampuses();
            ViewBag.UsingTypes = _selectListProvider.GetUsingTypes();
            if (campusId > 0)
            {
                ViewBag.Buildings = _selectListProvider.GetBuildings(campusId);
            }
        }
    }
}