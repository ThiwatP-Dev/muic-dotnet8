using KeystoneLibrary.Data;
using KeystoneLibrary.Enumeration;
using KeystoneLibrary.Exceptions;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.USpark;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Keystone.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class USparkRoomReservationController : BaseController
    {
        private readonly IReservationProvider _reservationProvider;
        protected readonly IRoomProvider _roomProvider;

        public USparkRoomReservationController(ApplicationDbContext db,
                                               IReservationProvider reservationProvider,
                                               IRoomProvider roomProvider) : base(db)
        {
            _reservationProvider = reservationProvider;
            _roomProvider = roomProvider;
        }

        [HttpGet("Student/RoomReservations")]
        public IActionResult StudentRoomReservations(string studentCode)
        {
            var reservations = from reservation in _db.RoomReservations
                               join student in _db.Students on reservation.CreatedBy equals student.Id.ToString()
                               join room in _db.Rooms on reservation.RoomId equals room.Id
                               where student.Code == studentCode
                                     && reservation.Status != "c"
                               select new 
                                      {
                                          reservation.PhoneNumber,
                                          reservation.Email,
                                          reservation.Id,
                                          reservation.Name,
                                          room.NameEn,
                                          room.NameTh,
                                          reservation.UsingType,
                                          reservation.DateFrom,
                                          reservation.StartTime,
                                          reservation.EndTime,
                                          reservation.Description,
                                          reservation.Remark,
                                          reservation.Status,
                                          reservation.CreatedAt
                                      };

            if (reservations != null && reservations.Any())
            {
                var _reservations = new List<UsparkRoomReservationViewModel>();
                foreach (var item in reservations)
                {
                    _reservations.Add(new UsparkRoomReservationViewModel
                                      {
                                          RoomReservationId = item.Id,
                                          PhoneNumber = item.PhoneNumber,
                                          Email = item.Email,
                                          Title = item.Name,
                                          NameEn = item.NameEn,
                                          NameTh = item.NameTh,
                                          Date = item.DateFrom,
                                          Purpose = GetUsingTypeText(item.UsingType),
                                          StartedTime = item.StartTime,
                                          EndedTime = item.EndTime,
                                          Description = item.Description,
                                          Remark = item.Remark,
                                          Status = GetStatus(item.Status),
                                          CreatedAt = item.CreatedAt.ToUniversalTime()
                                      });
                }

                var now = DateTime.UtcNow.Date;
                var result = new 
                             {
                                 Upcoming = _reservations.Where(x => x.Date >= now)
                                                         .OrderBy(x => x.Date)
                                                         .ThenBy(x => x.StartedTime)
                                                         .ThenBy(x => x.NameEn),
                                 History = _reservations.Where(x => x.Date < now)
                                                        .OrderByDescending(x => x.Date)
                                                        .ThenByDescending(x => x.StartedTime)
                                                        .ThenBy(x => x.NameEn)
                             };
                
                return Success(result);
            }

            return NoContent();
        }

        [HttpGet("Times")]
        public IActionResult Times()
        {
            var today = DateTime.Today;
            var start = today.AddHours(8); 
            var end = today.AddHours(20);
            var result = new 
                         {
                             Start = start.ToString(StringFormat.TimeSpanFullHour),
                             End = end.ToString(StringFormat.TimeSpanFullHour),
                             Interval = 30
                         };

            return Success(result);
        }

        [HttpGet("UsingTypes")]
        public IActionResult UsingTypes()
        {
            var results = Enum.GetNames(typeof(RoomReservationUsingTypes));
            return Success(results);
        }

        [HttpPost("Rooms")]
        public IActionResult Rooms(ReservationDateTime model)
        {
            if (model == null)
                return Error(ApiException.Forbidden());

            var rooms = _roomProvider.GetAvailableRoomByDates(new List<DateTime> { model.Date }, model.StartedTime, model.EndedTime);

            if (rooms != null && rooms.Any())
            {
                var roomForStudents = rooms.Where(x => x.AllowStudent
                                                            //&& x.IsAllowSearch
                                                 )
                                           .ToList();

                if (roomForStudents != null && roomForStudents.Any())
                {
                    var results = roomForStudents.Select(x => new
                    {
                        RoomId = x.Id,
                        x.NameEn,
                        x.NameTh
                    });

                    return Success(results);
                }
            }

            return Error(RoomReservationAPIException.AvailableRoomNotFound());
        }

        [HttpPost("Student/Reserve")]
        public IActionResult Create(UsparkRoomReservation model)
        {
            if (model == null || !ModelState.IsValid)
                return Error(ApiException.Forbidden());

            if (_db.Students.Any(x => x.Code == model.StudentCode))
            {
                var studentId = _db.Students.First(x => x.Code == model.StudentCode).Id.ToString();
                var date = model.Date.ToLocalTime().Date;
                var now = DateTime.Now;
                var usingType = GetUsingType(model.Purpose);
                if (string.IsNullOrEmpty(usingType))
                    return Error(RoomReservationAPIException.InvalidUsingType());

                var room = _db.Rooms.FirstOrDefault(x => x.Id == model.RoomId);
                if (room == null)
                {
                    return Error(ApiException.InvalidKey());
                }
                if (!room.AllowStudent)
                {
                    return Error(RoomReservationAPIException.RoomNotAvailable());
                }

                var roomReservation = new RoomReservation
                                      {
                                          TermId = model.TermId,
                                          Name = model.Title,
                                          PhoneNumber = model.PhoneNumber,
                                          Email = model.Email,
                                          RoomId = model.RoomId,
                                          DateFrom = date,
                                          DateTo = date,
                                          StartTime = model.StartedTime,
                                          EndTime = model.EndedTime,
                                          Description = model.Description,
                                          Remark = model.Remark,
                                          Status = "w",
                                          SenderType = "s",
                                          CreatedAt = now,
                                          CreatedBy = studentId,
                                          UpdatedAt = now,
                                          UpdatedBy = studentId,
                                          UsingType = usingType
                                      };
                
                if (!_reservationProvider.IsMeetingRoom(model.RoomId))
                {
                    if (_reservationProvider.IsPeriodReservationExisted(roomReservation))
                    {
                        return Error(RoomReservationAPIException.OutOfReservationPeriod());
                    }
                }

                if (_reservationProvider.IsRoomExisted(roomReservation))
                {
                    return Error(RoomReservationAPIException.RoomNotAvailable());
                }
                else if (!_roomProvider.IsBuildingOpen(roomReservation.RoomId, roomReservation.StartTime, roomReservation.EndTime))
                {
                    return Error(RoomReservationAPIException.BuildingNotAvailable());
                }

                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        _db.RoomReservations.Add(roomReservation);
                        _db.SaveChangesNoAutoUserIdAndTimestamps();

                        transaction.Commit();
                        
                        return Success(0);
                    }
                    catch
                    {
                        transaction.Rollback();
                        return Error(RoomReservationAPIException.UnableToReserve());
                    }
                }
            }

            return Error(RoomReservationAPIException.StudentNotFound());
        }

        [HttpPost("Student/DeleteRoomReservation")]
        public IActionResult Delete(StudentRoomReservationDelete model)
        {
            if (_db.Students.Any(x => x.Code == model.StudentCode))
            {
                var studentId = _db.Students.First(x => x.Code == model.StudentCode).Id.ToString();
                var reservation = _db.RoomReservations.SingleOrDefault(x => x.Id == model.Id
                                                                            && x.CreatedBy == studentId);
                if (reservation != null)
                {
                    using (var transaction = _db.Database.BeginTransaction())
                    {
                        try
                        {
                            if (_db.RoomSlots.Any(x => x.RoomReservationId == reservation.Id))
                            {
                                var roomSlots = _db.RoomSlots.Where(x => x.RoomReservationId == reservation.Id);
                                _db.RoomSlots.RemoveRange(roomSlots);
                            }

                            _db.RoomReservations.Remove(reservation);
                            _db.SaveChanges();

                            transaction.Commit();
                            return Success(0);
                        }
                        catch 
                        {
                            transaction.Rollback();
                            return Error(RoomReservationAPIException.UnableToDelete());
                        }
                    }
                }

                return Error(RoomReservationAPIException.ReservationNotFound());
            }
            
            return Error(RoomReservationAPIException.StudentNotFound());
        }

        private string GetUsingType(string type)
        {
            switch (type)
            {
                case "Studying":
                    return "s";
                case "Activity":
                    return "a";
                case "Examination":
                    return "e";
                case "Meeting":
                    return "m";
                default:
                    return string.Empty;
            }
        }

        private string GetUsingTypeText(string type)
        {
            switch (type)
            {
                case "s":
                    return "Studying";
                case "a":
                    return "Activity";
                case "e":
                    return "Examination";
                case "m":
                    return "Meeting";
                default:
                    return string.Empty;
            }
        }

        public string GetStatus(string status)
        {
            switch (status)
            {
                case "w":
                    return "Waiting";
                case "a":
                    return "Approved";
                case "r":
                    return "Rejected";
                default:
                    return string.Empty;
            }
        }
    }
}