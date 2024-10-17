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
    [PermissionAuthorize("RoomReservationManagement", "")]
    public class RoomReservationManagementController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IReservationProvider _reservationProvider;
        protected readonly IRoomProvider _roomProvider;
        protected readonly IUserProvider _userProvider;

        public RoomReservationManagementController(ApplicationDbContext db,
                                                   ISelectListProvider selectListProvider,
                                                   IFlashMessage flashMessage,
                                                   IDateTimeProvider dateTimeProvider,
                                                   IReservationProvider reservationProvider,
                                                   IUserProvider userProvider,
                                                   IRoomProvider roomProvider) : base(db, flashMessage, selectListProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _reservationProvider = reservationProvider;
            _roomProvider = roomProvider;
            _userProvider = userProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.CampusId);
            DateTime? startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);

            DateTime? createdStartedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.CreatedFrom);
            DateTime? createdEndedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.CreatedTo);

            if (string.IsNullOrEmpty(criteria.Status))
            {
                criteria.Status = "w";
            }

            var room = _db.RoomReservations.Include(x => x.Room)
                                               .ThenInclude(x => x.Building)
                                           .Where(x => (startedAt == null
                                                        || x.DateFrom.Date >= startedAt.Value.Date
                                                        || x.DateTo.Date >= startedAt.Value.Date
                                                        )
                                                        && (endedAt == null
                                                            || x.DateFrom.Date <= endedAt.Value.Date
                                                            || x.DateTo.Date <= endedAt.Value.Date
                                                            )
                                                        && (createdStartedAt == null
                                                            || x.CreatedAt.Date >= createdStartedAt.Value.Date
                                                            || x.CreatedAt.Date >= createdStartedAt.Value.Date
                                                            )
                                                        && (createdEndedAt == null
                                                            || x.CreatedAt.Date <= createdEndedAt.Value.Date
                                                            || x.CreatedAt.Date <= createdEndedAt.Value.Date
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
                                                        && (string.IsNullOrEmpty(criteria.CreatedBy)
                                                            || x.CreatedBy == criteria.CreatedBy)
                                                        && (string.IsNullOrEmpty(criteria.SenderType)
                                                            || criteria.SenderType == x.SenderType))
                                           .IgnoreQueryFilters()
                                           .OrderBy(x => x.CreatedAt)
                                           .ToList();

            if (startedAt != null || endedAt != null)
            {
                var roomReservationId = room.Select(x => x.Id).ToList();
                var validRoomResvationInRoomSlot = _db.RoomSlots.AsNoTracking()
                                                                .Where(x => roomReservationId.Contains(x.RoomReservationId ?? 0))
                                                                .ToList();
                var existSlotRoomReservationId = validRoomResvationInRoomSlot.Select(x => x.RoomReservationId)
                                                                             .ToList();

                var forReservaerHasSlotRoomReserveId = validRoomResvationInRoomSlot.Where( x =>
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
                room = room.Where(x => !existSlotRoomReservationId.Contains(x.Id) || forReservaerHasSlotRoomReserveId.Contains(x.Id)).ToList() ;
                    
            }

            var roomReturn = room.GetPaged(criteria, page);
            foreach (var item in roomReturn.Results)
            {
                item.ApprovedByFullNameEn = _userProvider.GetUserFullNameById(item.ApprovedBy);
                item.ApprovedAtText = item.ApprovedAt?.AddHours(7).ToString(StringFormat.ShortDateTime);
            }

            _userProvider.FillUserTimeStampFullNameWithOptionalStudent(roomReturn.Results.ToList<UserTimeStamp>());
            
            return View(roomReturn);
        }

        [PermissionAuthorize("RoomReservationManagement", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            var model = new RoomReservation();
            model.DateFrom = model.DateTo = DateTime.Today;
            return View(model);
        }

        [PermissionAuthorize("RoomReservationManagement", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RoomReservation model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            TimeSpan? startedAt = _dateTimeProvider.ConvertStringToTime(model.StartTimeText);
            TimeSpan? endedAt = _dateTimeProvider.ConvertStringToTime(model.EndTimeText);
            model.StartTime = startedAt.Value;
            model.EndTime = endedAt.Value;
            var user = GetUser();

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
                CreateSelectList(0, string.Empty, model.AcademicLevelId, model.DateFrom, model.StartTimeText, model.EndTimeText);
                _flashMessage.Danger(Message.UnableToCreate);
                return View(model);
            }

            if (model.DateFrom > model.DateTo || model.StartTime > model.EndTime)
            {
                CreateSelectList(0, string.Empty, model.AcademicLevelId, model.DateFrom, model.StartTimeText, model.EndTimeText);
                _flashMessage.Danger(Message.InvalidStartedDate);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                if (_reservationProvider.IsRoomExisted(model))
                {
                    CreateSelectList(0, string.Empty, model.AcademicLevelId, model.DateFrom, model.StartTimeText, model.EndTimeText);
                    _flashMessage.Danger(Message.RoomIsNotAvailable);
                    return View(model);
                }
                else if (!_roomProvider.IsBuildingOpen(model.RoomId, model.StartTime, model.EndTime))
                {
                    CreateSelectList(0, string.Empty, model.AcademicLevelId, model.DateFrom, model.StartTimeText, model.EndTimeText);
                    _flashMessage.Danger(Message.BuildingIsNotAvailable);
                    return View(model);
                }

                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        if (model.Status == "a")
                        {
                            model.ApprovedBy = user.Id;
                            model.ApprovedAt = DateTime.UtcNow;
                        }
                        else 
                        {
                            model.ApprovedBy = null;
                            model.ApprovedAt = null;
                        }

                        _db.RoomReservations.Add(model);
                        _db.SaveChanges();
                        if (model.Status == "a")
                        {
                            var roomSlots = _reservationProvider.GenerateRoomSlotByRoomReservation(model);
                            _db.RoomSlots.AddRange(roomSlots);
                        }

                        _db.SaveChanges();
                        transaction.Commit();
                        CreateSelectList(0, model.Status, model.AcademicLevelId, model.DateFrom, model.StartTimeText, model.EndTimeText);
                        _flashMessage.Confirmation(Message.SaveSucceed);
                                 return Redirect(returnUrl);
                    }
                    catch
                    {
                        transaction.Rollback();
                        CreateSelectList(0, model.Status, model.AcademicLevelId, model.DateFrom, model.StartTimeText, model.EndTimeText);
                        _flashMessage.Danger(Message.UnableToCreate);
                        return View(model);
                    }
                }
            }

            CreateSelectList(0, string.Empty, model.AcademicLevelId, model.DateFrom, model.StartTimeText, model.EndTimeText);
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

        [PermissionAuthorize("RoomReservationManagement", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(RoomReservation model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            TimeSpan? startedAt = _dateTimeProvider.ConvertStringToTime(model.StartTimeText);
            TimeSpan? endedAt = _dateTimeProvider.ConvertStringToTime(model.EndTimeText);
            model.StartTime = startedAt.Value;
            model.EndTime = endedAt.Value;
            var user = GetUser();

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
                CreateSelectList(0, model.Status, model.AcademicLevelId, model.DateFrom, model.StartTimeText, model.EndTimeText);
                _flashMessage.Danger(Message.UnableToCreate);
                return View(model);
            }

            if (model.DateFrom > model.DateTo || model.StartTime > model.EndTime)
            {
                CreateSelectList(0, model.Status, model.AcademicLevelId, model.DateFrom, model.StartTimeText, model.EndTimeText);
                _flashMessage.Danger(Message.InvalidStartedDate);
                return View(model);
            }

            var roomReservation = _reservationProvider.GetRoomReservation(model.Id);
            roomReservation.StartTime = startedAt.Value;
            roomReservation.EndTime = endedAt.Value;

            if (_reservationProvider.IsRoomExisted(model))
            {
                CreateSelectList(0, model.Status, model.AcademicLevelId, model.DateFrom, model.StartTimeText, model.EndTimeText);
                _flashMessage.Danger(Message.RoomIsNotAvailable);
                return View(model);
            }
            else if (!_roomProvider.IsBuildingOpen(model.RoomId, model.StartTime, model.EndTime))
            {
                CreateSelectList(0, model.Status, model.AcademicLevelId, model.DateFrom, model.StartTimeText, model.EndTimeText);
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
                        roomReservation.Status = model.Status;
                        if (model.Status == "a")
                        {
                            roomReservation.ApprovedBy = user.Id;
                            roomReservation.ApprovedAt = DateTime.UtcNow;
                            var roomSlots = _reservationProvider.GenerateRoomSlotByRoomReservation(model);
                            _db.RoomSlots.AddRange(roomSlots);
                        }
                        else 
                        {
                            model.ApprovedBy = null;
                            model.ApprovedAt = null;
                        }

                        await _db.SaveChangesAsync();
                        transaction.Commit();
                        CreateSelectList(0, model.Status, model.AcademicLevelId, model.DateFrom, model.StartTimeText, model.EndTimeText);
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return Redirect(returnUrl);
                    }
                    catch
                    {
                        transaction.Rollback();
                        CreateSelectList(0, model.Status, model.AcademicLevelId, model.DateFrom, model.StartTimeText, model.EndTimeText);
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
            }

            CreateSelectList(0, model.Status, model.AcademicLevelId, model.DateFrom, model.StartTimeText, model.EndTimeText);
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        public ActionResult EditStatus(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = _reservationProvider.GetRoomReservation(id);
            CreateSelectList(0, model.Status);
            return PartialView("_EditReservation", model);
        }

        public ActionResult CancelRoomSlot(long id, string returnUrl)
        {
            var model = _reservationProvider.GetRoomSlotByReservation(id);
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_EditReservationRoomSlot", model);
        }

        [PermissionAuthorize("RoomReservationManagement", PolicyGenerator.Write)]
        [HttpPost]
        public ActionResult CancelRoomSlot(List<RoomReservationSlotViewModel> models, long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if(models.Any())
            {
                var model = _reservationProvider.GetRoomReservation(models.FirstOrDefault().RoomReservationId);
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        var allRoomSlotInReservation = _db.RoomSlots.Where(x => x.RoomReservationId == model.Id);

                        foreach (var item in models)
                        {
                            if (item.IsChecked == "on")
                            {
                                var roomslot = allRoomSlotInReservation.SingleOrDefault(x => x.Id == item.Id);
                                roomslot.IsCancel = true;
                                _db.SaveChanges();
                            }
                        }

                        if (allRoomSlotInReservation.All(x => x.IsCancel))
                        {
                            model.Remark = model.Remark + " - cancel all room slot auto cancel reservation";
                            model.Status = "c";
                            model.ApprovedBy = null;
                            model.ApprovedAt = null;
                            _db.SaveChanges();
                        }
                        
                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return Redirect(returnUrl);
                    }
                    catch
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.UnableToCancel);
                        return Redirect(returnUrl);
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [PermissionAuthorize("RoomReservationManagement", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditStatus(RoomReservation model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var roomReservation = _reservationProvider.GetRoomReservation(model.Id);
            var user = GetUser();
            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    if (model.Status == "a")
                    {
                        if (_reservationProvider.IsRoomExisted(model))
                        {
                            CreateSelectList(0, roomReservation.Status, roomReservation.AcademicLevelId, roomReservation.DateFrom, roomReservation.StartTimeText, roomReservation.EndTimeText);
                            _flashMessage.Danger(Message.RoomIsNotAvailable);
                            transaction.Rollback();
                            return Redirect(returnUrl);
                        }

                        roomReservation.Status = model.Status;
                        roomReservation.ApprovedBy = user.Id;
                        roomReservation.ApprovedAt = DateTime.UtcNow;
                        var roomSlots = _reservationProvider.GenerateRoomSlotByRoomReservation(roomReservation);
                        _db.RoomSlots.AddRange(roomSlots);
                    }
                    else 
                    {
                        roomReservation.Remark = model.Remark;
                        roomReservation.Status = model.Status;
                        roomReservation.ApprovedBy = null;
                        roomReservation.ApprovedAt = null;
                        _roomProvider.CancelRoomSlotByRoomReservationId(model.Id);
                    }

                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return Redirect(returnUrl);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToEdit);
                    CreateSelectList(0, model.Status);
                    return Redirect(returnUrl);
                }
            }
        }

        [PermissionAuthorize("RoomReservationManagement", PolicyGenerator.Write)]
        public async Task<ActionResult> Approve(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = _reservationProvider.GetRoomReservation(id);
            var user = GetUser();
            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    model.Status = "a";
                    if (_reservationProvider.IsRoomExisted(model))
                    {
                        CreateSelectList(0, model.Status, model.AcademicLevelId, model.DateFrom, model.StartTimeText, model.EndTimeText);
                        _flashMessage.Danger(Message.RoomIsNotAvailable);
                        transaction.Rollback();
                        return Redirect(returnUrl);
                    }

                    var roomSlots = _reservationProvider.GenerateRoomSlotByRoomReservation(model);
                    model.ApprovedBy = user.Id;
                    model.ApprovedAt = DateTime.UtcNow;
                    _db.RoomSlots.AddRange(roomSlots);

                    await _db.SaveChangesAsync();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return Redirect(returnUrl);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToEdit);
                    CreateSelectList(0, model.Status);
                    return Redirect(returnUrl);
                }
            }
        }

        [PermissionAuthorize("RoomReservationManagement", PolicyGenerator.Write)]
        public ActionResult Delete(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = _reservationProvider.GetRoomReservation(id);
            if (model == null)
            {
                _flashMessage.Warning(Message.DataNotFound);
                return Redirect(returnUrl);
            }
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

        private void CreateSelectList(long campusId = 0, string currentStatus = "", long academicLevelId = 0, DateTime? date = null, string start = null, string end = null)
        {
            ViewBag.Campuses = _selectListProvider.GetCampuses();
            ViewBag.AllReservationStatues = _selectListProvider.GetAllReservationStatuses();
            ViewBag.ReservationStatuses = _selectListProvider.GetReservationStatuses(currentStatus);
            ViewBag.SenderTypes = _selectListProvider.GetSenderTypes();
            ViewBag.UsingTypes = _selectListProvider.GetUsingTypes();
            ViewBag.Rooms = _selectListProvider.GetRooms();
            ViewBag.Users = _selectListProvider.GetUsersFullNameEn();
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