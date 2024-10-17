using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("RoomCalendar", "")]
    public class RoomCalendarController : BaseController
    {
        public RoomCalendarController(ApplicationDbContext db,
                                      IFlashMessage flashMessage,
                                      ISelectListProvider selectListProvider,
                                      IMapper mapper) : base(db, flashMessage, mapper, selectListProvider) { }

        private const string Grey = "#c2c2c2";
        private const string Yellow = "#d1cc00";
        private const string Green = "#1ab394";
        private const string Sky = "#a3d2e3";
        private const string Blue = "#1c84c6";
        private const string Red = "#ff2d26";

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList();
            if (criteria.RoomId == 0 || criteria.DateCheck == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var model = new RoomCalendarViewModel();
            model.Details = _db.RoomSlots.IgnoreQueryFilters()
                                         .AsNoTracking()
                                         .Where(x => x.IsActive
                                                         && x.RoomId == criteria.RoomId
                                                    //  && x.Date.Month == criteria.DateCheck.Month 
                                                    //  && x.Date.Year == criteria.DateCheck.Year
                                                         && (string.IsNullOrEmpty(criteria.Type)
                                                             || x.UsingType == criteria.Type))
                                         .Select(x => new RoomCalendarDetail
                                                      {
                                                          SectionSlotId = x.SectionSlotId ?? 0,
                                                          ExaminationId = x.ExaminationReservationId ?? 0,
                                                          ReservationId = x.RoomReservationId ?? 0,
                                                          ReservationName = x.RoomReservation != null ? x.RoomReservation.Name : "",
                                                          SectionNumber = x.SectionSlotId != null ? x.SectionSlot.Section.Number : x.ExaminationReservationId != null ? x.ExaminationReservation.Section.Number : "N/A",
                                                          CourseRateId = x.SectionSlotId != null ? x.SectionSlot.Section.Course.CourseRateId : x.ExaminationReservationId != null ? x.ExaminationReservation.Section.Course.CourseRateId : null,
                                                          CourseCode = x.SectionSlotId != null ? x.SectionSlot.Section.Course.Code : x.ExaminationReservationId != null ? x.ExaminationReservation.Section.Course.Code : "N/A",
                                                          Credit = x.SectionSlotId != null ? x.SectionSlot.Section.Course.Credit : x.ExaminationReservationId != null ? x.ExaminationReservation.Section.Course.Credit : 0,
                                                          Lecture = x.SectionSlotId != null ? x.SectionSlot.Section.Course.Lecture : x.ExaminationReservationId != null ? x.ExaminationReservation.Section.Course.Lecture : 0,
                                                          Lab = x.SectionSlotId != null ? x.SectionSlot.Section.Course.Lab : x.ExaminationReservationId != null ? x.ExaminationReservation.Section.Course.Lab : 0,
                                                          Other = x.SectionSlotId != null ? x.SectionSlot.Section.Course.Other : x.ExaminationReservationId != null ? x.ExaminationReservation.Section.Course.Other : 0,
                                                          Start = new DateTime(x.Date.Year, x.Date.Month, x.Date.Day, x.StartTime.Hours, x.StartTime.Minutes, x.StartTime.Seconds),
                                                          End = new DateTime(x.Date.Year, x.Date.Month, x.Date.Day, x.EndTime.Hours, x.EndTime.Minutes, x.EndTime.Seconds),
                                                          Time = x.Time,
                                                          Type = x.SectionSlotId != null ? "s" : x.ExaminationReservationId != null ? "e" : "r",
                                                          IsCancel = x.IsCancel,
                                                          Date = x.Date,
                                                          IsWaiting = false
                                                      })
                                         .ToList();

            //Add Waiting Room Reservation to the list 
            var roomReservationIdList = model.Details.Where(x => x.ReservationId > 0).Select(x => x.ReservationId).ToList();
            var roomReservationWaiting = _db.RoomReservations.AsNoTracking()
                                                             .Where(x => //x.Status != "c"
                                                                            //&& 
                                                                            x.RoomId == criteria.RoomId
                                                                            && (string.IsNullOrEmpty(criteria.Type)
                                                                             || x.UsingType == criteria.Type)
                                                                            && !roomReservationIdList.Contains(x.Id)
                                                             )
                                                             .ToList();
            foreach (var roomReservation in roomReservationWaiting)
            {
                var startTime = roomReservation.DateFrom.Date;
                var endTime = roomReservation.DateTo.Date;
                for (DateTime current = startTime; current <= endTime; current = current.AddDays(1)) 
                {
                    if ((current.DayOfWeek == DayOfWeek.Sunday && roomReservation.IsSunday)
                            || (current.DayOfWeek == DayOfWeek.Monday && roomReservation.IsMonday)
                            || (current.DayOfWeek == DayOfWeek.Tuesday && roomReservation.IsTuesday)
                            || (current.DayOfWeek == DayOfWeek.Wednesday && roomReservation.IsWednesday)
                            || (current.DayOfWeek == DayOfWeek.Thursday && roomReservation.IsThursday)
                            || (current.DayOfWeek == DayOfWeek.Friday && roomReservation.IsFriday)
                            || (current.DayOfWeek == DayOfWeek.Saturday && roomReservation.IsSaturday)
                        )
                    {
                        model.Details.Add(new RoomCalendarDetail
                        {
                            SectionSlotId = 0,
                            ExaminationId = 0,
                            ReservationId = roomReservation.Id,
                            ReservationName = roomReservation.Name,
                            SectionNumber = "N/A",
                            CourseRateId = null,
                            CourseCode = "N/A",
                            Credit = 0,
                            Lecture = 0,
                            Lab = 0,
                            Other = 0,
                            Start = new DateTime(current.Year, current.Month, current.Day, roomReservation.StartTime.Hours, roomReservation.StartTime.Minutes, roomReservation.StartTime.Seconds),
                            End = new DateTime(current.Year, current.Month, current.Day, roomReservation.EndTime.Hours, roomReservation.EndTime.Minutes, roomReservation.EndTime.Seconds),
                            Time = roomReservation.TimeDisplay,
                            Type = "r",
                            IsCancel = roomReservation.Status == "c" || roomReservation.Status == "r",
                            Date = current.Date,
                            IsWaiting = roomReservation.Status != "a"
                        });
                    }
                }

            }


            foreach (var item in model.Details)
            {
                if (item.Date.Date < DateTime.Now.Date)
                {
                    item.Color = Grey;
                }
                else if (item.IsCancel)
                {
                    item.Color = Red;
                }
                else if (item.Type == "s")
                {
                    item.Color = Green;
                }
                else if (item.Type == "e")
                {
                    item.Color = Yellow;
                }
                else if (item.Type == "r" && item.IsWaiting)
                {
                    item.Color = Sky;
                }
                else if (item.Type == "r")
                {
                    item.Color = Blue;
                }
                else 
                {
                    item.Color = Grey;
                }
            }
            
            return View(model);
        }

        private void CreateSelectList()
        {
            ViewBag.Rooms = _selectListProvider.GetRooms();
            ViewBag.UsingTypes = _selectListProvider.GetUsingTypes();
        }
    }
}