using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("RoomReservation", "")]
    public class RoomReservationController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IReservationProvider _reservationProvider;
        protected readonly IRoomProvider _roomProvider;
        protected readonly ICacheProvider _cacheProvider;

        public RoomReservationController(ApplicationDbContext db,
                                         ISelectListProvider selectListProvider,
                                         IFlashMessage flashMessage,
                                         IDateTimeProvider dateTimeProvider,
                                         IReservationProvider reservationProvider,
                                         ICacheProvider cacheProvider,
                                         IRoomProvider roomProvider) : base(db, flashMessage, selectListProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _reservationProvider = reservationProvider;
            _roomProvider = roomProvider;
            _cacheProvider = cacheProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.CampusId);
            DateTime? startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);

            if (startedAt == null || endedAt == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                if (string.IsNullOrEmpty(criteria.Status))
                {
                    criteria.Status = "w";
                }
                return View(new PagedResult<RoomReservationViewModel>()
                            {
                                Criteria = criteria
                            });
            }

            var user = GetUser();
            if(user != null)
            {
                var room = _db.RoomReservations.Where(x => (startedAt == null
                                                                || x.DateFrom.Date >= startedAt.Value.Date
                                                                || x.DateTo.Date >= startedAt.Value.Date
                                                                )
                                                           && (endedAt == null
                                                                || x.DateFrom.Date <= endedAt.Value.Date
                                                                || x.DateTo.Date <= endedAt.Value.Date
                                                                )
                                                           && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                               || x.Name.StartsWith(criteria.CodeAndName))
                                                           && (criteria.CampusId == 0
                                                               || x.Room.Building.CampusId == criteria.CampusId)
                                                           && (criteria.BuildingId == 0
                                                               || x.Room.BuildingId == criteria.BuildingId)
                                                           && (criteria.Floor == null
                                                               || x.Room.Floor == criteria.Floor)
                                                           && (string.IsNullOrEmpty(criteria.RoomName)
                                                               || x.Room.NameEn.Contains(criteria.RoomName))
                                                           && (criteria.Status == "all"
                                                               || x.Status == criteria.Status)
                                                           && (string.IsNullOrEmpty(criteria.SenderType)
                                                               || criteria.SenderType == x.SenderType)
                                                           && x.CreatedBy == user.Id)
                                               .IgnoreQueryFilters()
                                               .Select(x => new RoomReservationViewModel
                                               {
                                                   Id = x.Id,
                                                   Title = x.Name,
                                                   PhoneNumber = x.PhoneNumber,
                                                   Email = x.Email,
                                                   DateFromText = x.DateFromText,
                                                   DateToText = x.DateToText,
                                                   TimeDisplay = x.TimeDisplay,
                                                   SenderTypeText = x.SenderTypeText,
                                                   CreatedAtText = x.CreatedAtText,
                                                   CreatedBy = user.UserName,
                                                   Room = x.Room.NameEn,
                                                   Description = x.Description,
                                                   Remark = x.Remark,
                                                   Status = x.Status
                                               })
                                               .OrderByDescending(x => x.CreatedAtText)
                                               .ToList();

                if (startedAt != null || endedAt != null)
                {
                    var roomReservationId = room.Select(x => x.Id).ToList();
                    var validRoomResvationInRoomSlot = _db.RoomSlots.AsNoTracking()
                                                                    .Where(x => roomReservationId.Contains(x.RoomReservationId ?? 0))
                                                                    .ToList();
                    var existSlotRoomReservationId = validRoomResvationInRoomSlot.Select(x => x.RoomReservationId)
                                                                                 .ToList();

                    var forReservaerHasSlotRoomReserveId = validRoomResvationInRoomSlot.Where(x =>
                                                                                (startedAt == null
                                                                                    || x.Date >= startedAt.Value.Date
                                                                                    || x.Date >= startedAt.Value.Date
                                                                                    )
                                                                                && (endedAt == null
                                                                                    || x.Date <= endedAt.Value.Date
                                                                                    || x.Date <= endedAt.Value.Date
                                                                                    )
                                                                           )
                                                                    .Select(x => x.RoomReservationId)
                                                                    .ToList();
                    room = room.Where(x => !existSlotRoomReservationId.Contains(x.Id) || forReservaerHasSlotRoomReserveId.Contains(x.Id)).ToList();

                }

                var roomReturn = room.GetPaged(criteria, page);
                return View(roomReturn);
            }

            return View();
        }

        [PermissionAuthorize("RoomReservation", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var academicLevelId = _db.AcademicLevels.SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
            var termId = _cacheProvider.GetCurrentTerm(academicLevelId).Id;
            CreateSelectList(0, null, academicLevelId);
            var model = new RoomReservation();
            model.AcademicLevelId = academicLevelId;
            model.TermId = termId;
            model.DateFrom = model.DateTo = DateTime.Today;
            return View(model);
        }

        [PermissionAuthorize("RoomReservation", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RoomReservation model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(0, string.Empty, model.AcademicLevelId, model.DateFrom, model.StartTimeText, model.EndTimeText);
            ViewBag.Rooms = _selectListProvider.GetAvailableRoomByDates(model);
            if(!_reservationProvider.IsMeetingRoom(model.RoomId))
            {
                 if(_reservationProvider.IsPeriodReservationExisted(model))
                {
                    _flashMessage.Danger(Message.OutOfReservatoinPeriod);
                    return View(model);
                }
            }

            TimeSpan? startedAt = _dateTimeProvider.ConvertStringToTime(model.StartTimeText);
            TimeSpan? endedAt = _dateTimeProvider.ConvertStringToTime(model.EndTimeText);
            if(startedAt == null || endedAt == null || model.TermId == 0)
            {
                _flashMessage.Danger(Message.RequiredData);
                return View(model);
            }
            model.StartTime = startedAt.Value;
            model.EndTime = endedAt.Value;

            if (model.AcademicLevelId == 0
                || model.TermId == 0
                || model.RoomId == 0
                || string.IsNullOrEmpty(model.SenderType)
                || string.IsNullOrEmpty(model.UsingType)
                || model.DateFrom == null
                || model.DateTo == null
                || model.StartTimeText == null
                || model.EndTimeText == null)
            {
                _flashMessage.Danger(Message.UnableToCreate);
                return View(model);
            }

            if (model.DateFrom > model.DateTo || model.StartTime > model.EndTime)
            {
                _flashMessage.Danger(Message.InvalidStartedDate);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                if (_reservationProvider.IsRoomExisted(model))
                {
                    _flashMessage.Danger(Message.RoomIsNotAvailable);
                    return View(model);
                }
                else if (!_roomProvider.IsBuildingOpen(model.RoomId, model.StartTime, model.EndTime))
                {
                    _flashMessage.Danger(Message.BuildingIsNotAvailable);
                    return View(model);
                }

                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        model.Status = "w";
                        _db.RoomReservations.Add(model);
                        _db.SaveChanges();

                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return Redirect(returnUrl);
                    }
                    catch
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.UnableToCreate);
                        return View(model);
                    }
                }
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            var model = _reservationProvider.GetRoomReservation(id);
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(0, model.Status, model.AcademicLevelId, model.DateFrom, model.StartTimeText, model.EndTimeText);
            return View(model);
        }

        public ActionResult Details(long id)
        {
            var model = _db.RoomReservations.IgnoreQueryFilters()
                                            .Where(x => x.Id == id)
                                            .Select(x => new RoomReservationViewModel
                                                         {
                                                            Id = x.Id,
                                                            Title = x.Name,
                                                            AcademicLevelNameEn = x.Term.AcademicLevel.NameEn,
                                                            TermText = x.Term.TermText,
                                                            PhoneNumber = x.PhoneNumber,
                                                            Email = x.Email,
                                                            DateFromText =  x.DateFromText,
                                                            DateToText = x.DateToText,
                                                            StartedAtText = x.StartTimeString,
                                                            EndedAtText = x.EndTimeString,
                                                            TimeDisplay = x.TimeDisplay,
                                                            SenderTypeText = x.SenderTypeText,
                                                            UsingTypeText = x.UsingTypeText,
                                                            CreatedAtText = x.CreatedAtText,
                                                            Room = x.Room.NameEn,
                                                            Description = x.Description,
                                                            Remark = x.Remark,
                                                            Status = x.Status,
                                                            IsSunday = x.IsSunday,
                                                            IsMonday = x.IsMonday,
                                                            IsTuesday = x.IsTuesday,
                                                            IsWednesday = x.IsWednesday,
                                                            IsThursday = x.IsThursday,
                                                            IsFriday = x.IsFriday,
                                                            IsSaturday = x.IsSaturday
                                                         })
                                            .FirstOrDefault();

            if(model.Status != "w")
            {
                model.Results = _reservationProvider.GetRoomSlotByReservation(model.Id);
            }
            return PartialView("_Details", model);
        }

        [PermissionAuthorize("RoomReservation", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(RoomReservation model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(0, model.Status, model.AcademicLevelId, model.DateFrom, model.StartTimeText, model.EndTimeText);
            ViewBag.Rooms = _selectListProvider.GetAvailableRoomByDates(model);

            if(!_reservationProvider.IsMeetingRoom(model.RoomId))
            {
                 if(_reservationProvider.IsPeriodReservationExisted(model))
                {
                    _flashMessage.Danger(Message.OutOfReservatoinPeriod);
                    return View(model);
                }
            }

            TimeSpan? startedAt = _dateTimeProvider.ConvertStringToTime(model.StartTimeText);
            TimeSpan? endedAt = _dateTimeProvider.ConvertStringToTime(model.EndTimeText);
            if(startedAt == null || endedAt == null || model.TermId == 0)
            {
                _flashMessage.Danger(Message.RequiredData);
                return View(model);
            }

            model.StartTime = startedAt.Value;
            model.EndTime = endedAt.Value;

            if (model.AcademicLevelId == 0
                || model.TermId == 0
                || model.RoomId == 0
                || string.IsNullOrEmpty(model.SenderType)
                || string.IsNullOrEmpty(model.UsingType)
                || model.DateFrom == null
                || model.DateTo == null
                || model.StartTimeText == null
                || model.EndTimeText == null
                || string.IsNullOrEmpty(model.Status))
            {
                _flashMessage.Danger(Message.UnableToCreate);
                return View(model);
            }

            if (model.DateFrom > model.DateTo || model.StartTime > model.EndTime)
            {
                _flashMessage.Danger(Message.InvalidStartedDate);
                return View(model);
            }

            var roomReservation = _reservationProvider.GetRoomReservation(model.Id);
            roomReservation.StartTime = startedAt.Value;
            roomReservation.EndTime = endedAt.Value;

            if (_reservationProvider.IsRoomExisted(model))
            {
                _flashMessage.Danger(Message.RoomIsNotAvailable);
                return View(model);
            }
            else if (!_roomProvider.IsBuildingOpen(model.RoomId, model.StartTime, model.EndTime))
            {
                _flashMessage.Danger(Message.BuildingIsNotAvailable);
                return View(model);
            }

            if (ModelState.IsValid && await TryUpdateModelAsync<RoomReservation>(roomReservation))
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        _roomProvider.CancelRoomSlotByRoomReservationId(roomReservation.Id);
                        if (model.Status == "a")
                        {
                            var roomSlots = _reservationProvider.GenerateRoomSlotByRoomReservation(model);
                            _db.RoomSlots.AddRange(roomSlots);
                        }

                        await _db.SaveChangesAsync();
                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return Redirect(returnUrl);
                    }
                    catch
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("RoomReservation", PolicyGenerator.Write)]
        public ActionResult Delete(long id, string returnUrl)
        {
            var model = _reservationProvider.GetRoomReservation(id);
            try
            {
                if (_db.RoomSlots.Any(x => x.RoomReservationId == model.Id))
                {
                    var roomSlots = _db.RoomSlots.Where(x => x.RoomReservationId == model.Id).ToList();
                    _db.RoomSlots.RemoveRange(roomSlots);
                }
                _db.RoomReservations.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return Redirect(returnUrl);
        }

        [PermissionAuthorize("RoomReservation", PolicyGenerator.Write)]
        public ActionResult Cancel(long id, string returnUrl)
        {
            var model = _reservationProvider.GetRoomReservation(id);
            try
            {
                if (_db.RoomSlots.Any(x => x.RoomReservationId == model.Id))
                {
                    var roomSlots = _db.RoomSlots.Where(x => x.RoomReservationId == model.Id).ToList();
                    roomSlots = roomSlots.Select(x => { x.IsCancel = true; return x; }).ToList();
                    _db.SaveChanges();
                }
                model.Status = "c";
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return Redirect(returnUrl);
        }

        private void CreateSelectList(long campusId = 0, string currentStatus = "", long academicLevelId = 0, DateTime? date = null, string start = null, string end = null)
        {
            ViewBag.Campuses = _selectListProvider.GetCampuses();
            ViewBag.AllReservationStatues = _selectListProvider.GetAllReservationStatuses();
            ViewBag.ReservationStatuses = _selectListProvider.GetReservationStatuses(currentStatus);
            ViewBag.SenderTypes = _selectListProvider.GetSenderTypes();
            ViewBag.UsingTypes = _selectListProvider.GetUsingTypes();
            // ViewBag.Rooms = _selectListProvider.GetRooms();
            if (campusId > 0)
            {
                ViewBag.Buildings = _selectListProvider.GetBuildings(campusId);
            }

            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}