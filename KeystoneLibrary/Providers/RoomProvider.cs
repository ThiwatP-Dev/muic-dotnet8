using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.EntityFrameworkCore;

namespace KeystoneLibrary.Providers
{
    public class RoomProvider : IRoomProvider
    {
        protected readonly ApplicationDbContext _db;
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IReservationProvider _reservationProvider;

        public RoomProvider(ApplicationDbContext db,
                            IReservationProvider reservationProvider,
                            IDateTimeProvider dateTimeProvider)
        {
            _db = db;
            _dateTimeProvider = dateTimeProvider;
            _reservationProvider = reservationProvider;
        }

        public Room GetRoom(long roomId)
        {
            var room = _db.Rooms.Include(x => x.Building)
                                .ThenInclude(x => x.Campus)
                                .SingleOrDefault(x => x.Id == roomId);
            return room;
        }

        public List<Building> GetBuildingByCampusId(long campusId)
        {
            var buildings = _db.Buildings.Where(x => x.CampusId == campusId)
                                         .ToList();
            return buildings;
        }

        public List<Room> GetRoomByBuildingId(long buildingId)
        {
            var rooms = _db.Rooms.Where(x => x.BuildingId == buildingId)
                                 .ToList();
            return rooms;
        }

        public Campus GetCampus(long id)
        {
            var campus = _db.Campuses.SingleOrDefault(x => x.Id == id);
            return campus;
        }

        public List<Room> GetAvailableRoom(DateTime date, TimeSpan start, TimeSpan end, string type)
        {
            var rooms = from room in _db.Rooms
                        where (!_db.RoomSlots.Any(x => room.Id == x.RoomId
                                                       && x.Date.Date == date.Date
                                                       && x.StartTime < end
                                                       && x.EndTime > start
                                                       && !x.IsCancel))
                              || room.IsOnline
                        select room;

            return rooms.ToList();
        }

        public List<Room> GetAvailableRoomByDates(List<DateTime> dates, TimeSpan? start, TimeSpan? end, long id = 0)
        {
            var roomIds = new List<long>();
            foreach(var item in dates)
            {
                // EXISTS RoomSlot
                var result = _db.RoomSlots.AsNoTracking()
                                          .Where(x => item.Date == x.Date.Date
                                                      && x.StartTime < end
                                                      && x.EndTime > start
                                                      && !x.IsCancel)
                                          .Select(x => x.RoomId)
                                          .ToList();

                if(result.Any())
                {
                    roomIds.AddRange(result);
                }

                // EXISTS RESERVATION
                var existsReservations = _db.RoomReservations.AsNoTracking()
                                                             .Where(x => x.Id != id
                                                                        && x.DateFrom <= item.Date
                                                                        && x.DateTo >= item.Date
                                                                        && x.StartTime < end
                                                                        && x.EndTime > start
                                                                        && x.Status == "w")
                                                             .ToList();

                if (existsReservations != null && existsReservations.Any())
                {
                    foreach (var existsReservation in existsReservations)
                    {
                        var compareDates = _reservationProvider.GenerateSelectedDate(existsReservation);
                        if (compareDates.Any(x => x.Date == item.Date
                                                  && existsReservation.StartTime < end
                                                  && existsReservation.EndTime > start))
                        {
                            roomIds.Add(existsReservation.RoomId);
                        }
                    }
                } 
            }
            roomIds = roomIds.Distinct().ToList();
            var rooms = _db.Rooms.Where(x => !roomIds.Contains(x.Id) || x.IsOnline)
                                 .ToList();

            return rooms;
        }

        public RoomSlot IsExistedRoomSlot(long roomId, DateTime date, TimeSpan start, TimeSpan end)
        {
            var roomSlot = _db.RoomSlots.FirstOrDefault(x => x.RoomId == roomId
                                                             && x.Date.Date == date.Date
                                                             && x.StartTime < end
                                                             && x.EndTime > start
                                                             && !x.Room.IsOnline
                                                             && !x.IsCancel);
            return roomSlot;
        }

        public RoomSlot IsExistedRoomSlot(long roomId, DateTime date, TimeSpan start, TimeSpan end, long examReservationId = 0)
        {
            var roomSlot = _db.RoomSlots.FirstOrDefault(x => x.ExaminationReservationId != examReservationId
                                                             && x.RoomId == roomId
                                                             && x.Date.Date == date.Date
                                                             && x.StartTime < end
                                                             && x.EndTime > start
                                                             && !x.Room.IsOnline
                                                             && !x.IsCancel);
            return roomSlot;
        }

        public List<Room> GetAvailableRoomBySectionSlotIds(List<long> sectionSlotIds)
        {
            List<long> unavailableRoomIds = GetUnavailableRoomIdsBySectionSlotIds(sectionSlotIds);
            var rooms = _db.Rooms.Where(x => x.IsActive
                                             && !unavailableRoomIds.Contains(x.Id))
                                 .ToList();
            return rooms;
        }

        public List<Room> GetAvailableRoomAndRoomInSectionSlot(List<long> sectionSlotIds)
        {
            List<long> unavailableRoomIds = GetUnavailableRoomIdsBySectionSlotIds(sectionSlotIds);
            List<long> roomInSectionSlot = _db.SectionSlots.Where(x => sectionSlotIds.Contains(x.Id) && x.RoomId != null)
                                                            .Select(x => x.RoomId??0)
                                                            .ToList();

            var rooms = _db.Rooms.Where(x => x.IsActive
                                             && (!unavailableRoomIds.Contains(x.Id)
                                                 || x.IsOnline)
                                             || roomInSectionSlot.Contains(x.Id))
                                 .ToList();
            return rooms;
        }

        public string GetRoomSlotDetail(DateTime startDate, DateTime endDate)
        {
            var roomSlots = _db.RoomSlots.Include(x => x.Room)
                                         .Include(x => x.SectionSlot)
                                            .ThenInclude(x=>x.TeachingType)
                                         .Include(x => x.SectionSlot)
                                            .ThenInclude(x=>x.Instructor)
                                         .Where(x => x.IsActive 
                                                     && x.Room.IsActive
                                                     && x.Date >= startDate
                                                     && x.Date <= endDate)
                                         .Select(x => new
                                                      {
                                                          Room = x.Room.NameEn,
                                                          Instructor = x.SectionSlot.Instructor.CodeAndNameTh,
                                                          x.Date,
                                                          x.StartTime,
                                                          x.EndTime,
                                                          x.UsingTypeText,
                                                          TeachingType = x.SectionSlot.TeachingType.NameEn,
                                                          x.IsCancel,
                                                          x.SectionSlot.IsMakeUpClass,
                                                          x.UpdatedAt,
                                                          x.UpdatedBy
                                                      })
                                         .ToList();

            var json = JsonConvert.SerializeObject(roomSlots);
            return json;
        }

        private List<long> GetUnavailableRoomIdsBySectionSlotIds(List<long> sectionSlotIds)
        {
            List<long> roomIds = new List<long>();
            foreach (var sectionSlotId in sectionSlotIds)
            {
                var sectionSlot = _db.SectionSlots.SingleOrDefault(x => x.Id == sectionSlotId);
                if (_db.RoomSlots.Any(x => x.Date == sectionSlot.Date
                                           && x.StartTime < sectionSlot.EndTime
                                           && x.EndTime > sectionSlot.StartTime
                                           && !x.IsCancel))
                {
                    var roomId = _db.RoomSlots.Where(x => x.Date == sectionSlot.Date
                                                          && x.StartTime < sectionSlot.EndTime
                                                          && x.EndTime > sectionSlot.StartTime
                                                          && !x.IsCancel)
                                              .Select(x => x.RoomId)
                                              .FirstOrDefault();
                    roomIds.Add(roomId);
                }
            }

            return roomIds.Distinct().ToList();
        }

        public List<RoomSlot> GetRoomSlotsBySectionSlotIds(List<long> sectionSlotIds)
        {
            var sectionSlotIdsNullable = sectionSlotIds.Select(x => (long?)x).ToList();
            var roomSlots = _db.RoomSlots.Where(x => sectionSlotIdsNullable.Contains(x.SectionSlotId)
                                                     && !x.IsCancel)
                                         .Distinct()
                                         .ToList();
            return roomSlots;
        }

        public bool CreateRoomSlotBySectionSlot(SectionSlot sectionsSlot)
        {
            try
            {
                _db.RoomSlots.Add(new RoomSlot()
                {
                    TermId = sectionsSlot.Section.TermId,
                    RoomId = sectionsSlot.RoomId.Value,
                    Day = sectionsSlot.Day,
                    Date = sectionsSlot.Date,
                    StartTime = sectionsSlot.StartTime,
                    EndTime = sectionsSlot.EndTime,
                    UsingType = "s",
                    SectionSlotId = sectionsSlot.Id,
                    IsCancel = false
                });

                _db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CancelRoomSlotBySectionSlotId(long sectionsSlotId)
        {
            if (sectionsSlotId <= 0) return false;

            try
            {
                var roomSlot = _db.RoomSlots.Where(x => x.SectionSlotId == sectionsSlotId)
                                            .OrderByDescending(x => x.CreatedAt)
                                            .First();
                roomSlot.IsCancel = true;
                _db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CancelAndInActiveRoomSlotBySectionSlotId(long sectionsSlotId)
        {
            if (sectionsSlotId <= 0) return false;

            try
            {
                var roomSlot = _db.RoomSlots.Where(x => x.SectionSlotId == sectionsSlotId)
                                            .OrderByDescending(x => x.CreatedAt)
                                            .First();
                roomSlot.IsCancel = true;
                roomSlot.IsActive = false;
                _db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsBuildingOpen(long roomId, TimeSpan start, TimeSpan end)
        {
            var room = _db.Rooms.Include(x => x.Building)
                                .SingleOrDefault(x => x.Id == roomId);

            if (room == null || room.Building == null)
            {
                return false;
            }
            else
            {
                return (room.Building.OpenedTime == null || room.Building.OpenedTime <= start) 
                       && (room.Building.ClosedTime == null || room.Building.ClosedTime >= end); 
            }
        }

        public bool CancelRoomSlotByRoomReservationId(long roomReservationId)
        {
            if (roomReservationId <= 0) return false;

            try
            {
                var roomSlots = _db.RoomSlots.Where(x => x.RoomReservationId == roomReservationId)
                                             .ToList();
                                             
                roomSlots.ForEach(x => x.IsCancel = true);
                _db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<RoomSlot> GetRoomSlotsByRoomAndTermAndDate(long roomId, long termId, string date)
        {
            DateTime? dateTime = _dateTimeProvider.ConvertStringToDateTime(date);
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
                                             .ThenInclude(x => x.Title)
                                         .Include(x => x.RoomReservation)
                                             .ThenInclude(x => x.Room)
                                         .Include(x => x.ExaminationReservation)
                                             .ThenInclude(x => x.Instructor)
                                             .ThenInclude(x => x.Title)
                                         .Include(x => x.ExaminationReservation)
                                             .ThenInclude(x => x.Section)
                                             .ThenInclude(x => x.Course)
                                         .Include(x => x.ExaminationReservation)
                                             .ThenInclude(x => x.Room)
                                         .Include(x => x.Term)
                                         .Where(x => x.RoomId == roomId
                                                     && x.TermId == termId
                                                     && x.Date.Date == dateTime
                                                     && !x.IsCancel)
                                         .ToList();

            return roomSlots;
        }

        public List<RoomSlot> GetWeeklyRoomSlotsByRoomAndTermAndDate(long roomId, long termId, string date, string isMakeUp, string type = "")
        {
            DateTime? dateTime = _dateTimeProvider.ConvertStringToDateTime(date);
            var thisWeekStart = dateTime.Value.AddDays(-(int)dateTime.Value.DayOfWeek);
            var thisWeekEnd = thisWeekStart.AddDays(7);
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
                                             .ThenInclude(x => x.Title)
                                         .Include(x => x.RoomReservation)
                                             .ThenInclude(x => x.Room)
                                         .Include(x => x.ExaminationReservation)
                                             .ThenInclude(x => x.Instructor)
                                             .ThenInclude(x => x.Title)
                                         .Include(x => x.ExaminationReservation)
                                             .ThenInclude(x => x.Section)
                                             .ThenInclude(x => x.Course)
                                         .Include(x => x.ExaminationReservation)
                                             .ThenInclude(x => x.Room)
                                         .Include(x => x.ExaminationReservation)
                                             .ThenInclude(x => x.ExaminationType)
                                         .Include(x => x.Term)
                                         .Where(x => x.RoomId == roomId
                                                     && x.TermId == termId
                                                     && (string.IsNullOrEmpty(type) || x.UsingType == type)
                                                     && x.Date.Date >= thisWeekStart && x.Date.Date < thisWeekEnd
                                                     && !x.IsCancel
                                                     && (Convert.ToBoolean(isMakeUp) ? x.SectionSlotId != null && 
                                                                                       x.SectionSlot.IsMakeUpClass
                                                                                     : true))
                                         .ToList();

            return roomSlots;
        }
        public bool IsHaveRoomInSectionSlot(long sectionId)
        {
            return _db.SectionSlots.Any(x => x.SectionId == sectionId && x.RoomId != null);
        }
    }
}