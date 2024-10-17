using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("Room", "")]
    public class RoomController : BaseController
    {
        public RoomController(ApplicationDbContext db,
                              IFlashMessage flashMessage,
                              ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList();
            var models = _db.Rooms.Include(x => x.Building)
                                  .Include(x => x.RoomType)
                                  .Where(x => (string.IsNullOrEmpty(criteria.CodeAndName)
                                               || x.NameEn.Contains(criteria.CodeAndName)
                                               || x.NameTh.Contains(criteria.CodeAndName))
                                               && (criteria.Floor == 0
                                                   ||criteria.Floor == null
                                                   || x.Floor == criteria.Floor)
                                               && (criteria.CapacityFrom == null
                                                   || x.Capacity >= criteria.CapacityFrom)
                                               && (criteria.CapacityTo == null
                                                   || x.Capacity <= criteria.CapacityTo)
                                               && (criteria.BuildingId == 0
                                                   || x.BuildingId == criteria.BuildingId)
                                               && (criteria.RoomtypeId == 0
                                                   || x.RoomTypeId == criteria.RoomtypeId)
                                               && (criteria.FacilityIds == null
                                                   || !criteria.FacilityIds.Any()
                                                   || x.RoomFacilities.Any(y => criteria.FacilityIds.Contains(y.FacilityId)))
                                               && (string.IsNullOrEmpty(criteria.Status)
                                                   || x.IsActive == Convert.ToBoolean(criteria.Status))
                                               && (Convert.ToBoolean(criteria.IsOnline) ? x.IsOnline
                                                                                        : x.IsOnline || !x.IsOnline)
                                               && (Convert.ToBoolean(criteria.IsAllowLecture) ? x.IsAllowLecture
                                                                                              : x.IsAllowLecture || !x.IsAllowLecture))
                                  .OrderBy(x => x.NameEn)
                                  .IgnoreQueryFilters()
                                  .GetPaged(criteria, page);
            return View(models);
        }

        [PermissionAuthorize("Room", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View(new Room());
        }

        [PermissionAuthorize("Room", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Room model)
        {
            if (ModelState.IsValid)
            {
                if (model.ExaminationCapacity > model.Capacity)
                {
                    CreateSelectList();
                    _flashMessage.Danger(Message.InvalidExaminationCapacity);
                    return View(model);
                }

                if (model.BuildingId == 0 || model.RoomTypeId == 0 || string.IsNullOrEmpty(model.NameEn))
                {
                    CreateSelectList();
                    _flashMessage.Warning(Message.RequiredData);
                    return View(model);
                }
                model.RoomFacilities = model.RoomFacilities.Where(x => x.FacilityId != 0 && x.Amount != 0).ToList();
                _db.Rooms.Add(model);

                try
                {
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new Criteria
                                                           {
                                                               CodeAndName = model.NameEn,
                                                               Floor = model.Floor,
                                                               BuildingId = model.BuildingId,
                                                               RoomtypeId = model.RoomTypeId
                                                           });
                }
                catch
                {
                    CreateSelectList();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            CreateSelectList();
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            CreateSelectList();
            Room model = Find(id);
            return View(model);
        }

        [PermissionAuthorize("Room", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Room model)
        {
            var modelToUpdate = Find(model.Id);
            if (model.ExaminationCapacity > model.Capacity)
            {
                CreateSelectList();
                _flashMessage.Danger(Message.InvalidExaminationCapacity);
                return View(model);
            }
            if (model.BuildingId == 0 || model.RoomTypeId == 0 || string.IsNullOrEmpty(model.NameEn))
            {
                CreateSelectList();
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }
            using(var transaction = _db.Database.BeginTransaction())
            {
                model.RoomFacilities = model.RoomFacilities.Where(x => x.FacilityId != 0).ToList();
                var roomFacilities = _db.RoomFacilities.Where(x => x.RoomId == model.Id);
                _db.RoomFacilities.RemoveRange(roomFacilities);

                if (model.RoomFacilities.Count == 0)
                {
                    model.RoomFacilities = null;
                    modelToUpdate.NameEn = model.NameEn;
                    modelToUpdate.NameTh = model.NameTh;
                    modelToUpdate.Floor = model.Floor;
                    modelToUpdate.Capacity = model.Capacity;
                    modelToUpdate.ExaminationCapacity = model.ExaminationCapacity;
                    modelToUpdate.BuildingId = model.BuildingId;
                    modelToUpdate.RoomTypeId = model.RoomTypeId;
                    modelToUpdate.IsAllowLecture = model.IsAllowLecture;
                    modelToUpdate.IsAllowSearch = model.IsAllowSearch;
                    modelToUpdate.AllowStudent = model.AllowStudent;
                    modelToUpdate.AllowInstructor = model.AllowInstructor;
                    modelToUpdate.AllowStaff = model.AllowStaff;
                    modelToUpdate.IsActive = model.IsActive;
                    modelToUpdate.IsOnline = model.IsOnline;
                }
                else
                {
                    await TryUpdateModelAsync<Room>(modelToUpdate);
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        await _db.SaveChangesAsync();
                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index), new Criteria
                                                            {
                                                                CodeAndName = modelToUpdate.NameEn,
                                                                Floor = modelToUpdate.Floor,
                                                                BuildingId = modelToUpdate.BuildingId,
                                                                RoomtypeId = modelToUpdate.RoomTypeId
                                                            });
                    }
                    catch
                    { 
                        transaction.Rollback();
                        CreateSelectList();
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }

                CreateSelectList();
                transaction.Rollback();
                _flashMessage.Danger(Message.UnableToEdit);
                return View(model);
            }
        }

        public ActionResult Details(long id)
        {    
            var room = Find(id);
            return PartialView("_Details", room);  
        }

        [PermissionAuthorize("Room", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Room model = Find(id);
            try
            {
                _db.Rooms.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private Room Find(long? id) 
        {
            var model = _db.Rooms.Include(x => x.Building)
                                 .Include(x => x.RoomFacilities)
                                     .ThenInclude(x => x.Facility)
                                 .IgnoreQueryFilters()
                                 .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList() 
        {
            ViewBag.Campuses = _selectListProvider.GetCampuses();
            ViewBag.Buildings = _selectListProvider.GetBuildings();
            ViewBag.RoomTypes = _selectListProvider.GetRoomTypes();
            ViewBag.Facilities = _selectListProvider.GetFacilities();
            ViewBag.ActiveStatuses = _selectListProvider.GetYesNoAnswer();
            ViewBag.Statuses = _selectListProvider.GetActiveStatuses();
        }
    }
}