using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Data;
using Vereyon.Web;
using Microsoft.AspNetCore.Mvc;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("SectionSlotRoom", "")]
    public class SectionSlotRoomController : BaseController
    {
        protected readonly ISectionProvider _sectionProvider;
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IRoomProvider _roomProvider;

        public SectionSlotRoomController(ApplicationDbContext db,
                                         IFlashMessage flashMessage,
                                         ISelectListProvider selectListProvider,
                                         IAcademicProvider academicProvider,
                                         IRoomProvider roomProvider,
                                         ISectionProvider sectionProvider) : base(db, flashMessage, selectListProvider) 
        {
            _sectionProvider = sectionProvider;
            _academicProvider = academicProvider;
            _roomProvider = roomProvider;
        }

        public ActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.CourseId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            var IsSectionDetail = Convert.ToBoolean(criteria.SectionDetail);
            var IsSectionSlot = Convert.ToBoolean(criteria.SectionSlot);
            var model = _db.Sections.Include(x => x.Term)
                                    .Include(x => x.Course)
                                    .Include(x => x.MainInstructor)
                                        .ThenInclude(x => x.Title)
                                    .Include(x => x.SectionDetails)
                                    .Include(x => x.SectionSlots)
                                    .AsNoTracking()
                                    .Where(x => x.TermId == criteria.TermId
                                                && (x.ParentSectionId == null || x.ParentSectionId == 0)
                                                && (x.Status == "a" || x.Status == "c")
                                                && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                    || x.Course.NameEn.Contains(criteria.CodeAndName)
                                                    || x.Course.Code.Contains(criteria.CodeAndName))
                                                && (criteria.FacultyId == 0
                                                    || x.Course.FacultyId == criteria.FacultyId)
                                                && (string.IsNullOrEmpty(criteria.SectionNumber)
                                                    || x.Number.Contains(criteria.SectionNumber))
                                                && (criteria.InstructorId == 0
                                                    || criteria.InstructorId == x.MainInstructorId)
                                                &&(string.IsNullOrEmpty(criteria.SectionSlot) 
                                                    ||(IsSectionSlot ? x.SectionSlots.Count > 0
                                                                     : x.SectionSlots.Count == 0 ))
                                                &&(string.IsNullOrEmpty(criteria.SectionDetail) 
                                                    ||(IsSectionDetail ? x.SectionDetails.Count > 0
                                                                       : x.SectionDetails.Count == 0 )))
                                    .GetPaged(criteria, page);

            // model.Results.Select(x => {
            //                               x.SectionDetails = x.SectionDetails.OrderBy(y => y.Day)
            //                                                                  .ThenBy(y => y.StartTime)
            //                                                                  .ToList();
  
            //                               return x;
            //                           })
            //              .ToList();

            var sectionIdsNullable = model.Results.Select(x => (long?)x.Id).ToList();

            var jointSections = _db.Sections.AsNoTracking()
                                            .Where(x => sectionIdsNullable.Contains(x.ParentSectionId))
                                            .Select(x => new JointSectionCourseToBeOfferedViewModel
                                                         {
                                                             Id = x.Id,
                                                             ParentSectionId = x.ParentSectionId,
                                                             Number = x.Number,
                                                             CourseRateId = x.Course.CourseRateId,
                                                             CourseCode = x.Course.Code,
                                                             SeatUsed = x.SeatUsed
                                                         })
                                            .ToList();
            foreach(var item in model.Results)
            {
                item.SectionDetails = item.SectionDetails.OrderBy(y => y.Day)
                                          .ThenBy(y => y.StartTime)
                                          .ToList();

                item.TotalSeatUsed = item.SeatUsed + jointSections.Where(x => x.ParentSectionId == item.Id)
                                                                  .Sum(x => x.SeatUsed);
            }
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            // CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            var model = _sectionProvider.GetSection(id);
            model.SectionDetails = model.SectionDetails.OrderBy(x => x.Day)
                                                           .ThenBy(x => x.StartTime) 
                                                       .ToList();
            if (model != null)
            {
                model.SectionSlots = model.SectionSlots.OrderBy(x => x.Date)
                                                        .ThenBy(x => x.StartTime)
                                                    .ToList();
                var roomsSelectList = new List<SelectList>();
                foreach (var detail in model.SectionDetails)
                {

                    var sectionSlotIds =  _db.SectionSlots.Include(x => x.Section)
                                                          .Where(x => x.SectionId == detail.SectionId
                                                                      && x.Day == detail.Day
                                                                      && x.StartTime == detail.StartTime
                                                                      && x.EndTime == detail.EndTime
                                                                      && x.Status == "w")
                                                          .Select(x => x.Id)
                                                          .ToList();

                    roomsSelectList.Add(sectionSlotIds == null || !sectionSlotIds.Any() ? _selectListProvider.GetRooms()
                                                                                        : _selectListProvider.GetAvailableRoomBySectionSlotIdsAndInSectionSlot(sectionSlotIds));
                }
                ViewBag.RoomList = roomsSelectList;
            }

            return View(model);
        }

        [PermissionAuthorize("SectionSlotRoom", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Section section, string returnUrl)
        {
            var errorMessage = "";
            var today = DateTime.Today;
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var jointSectionDetails = _db.Sections.Include(x => x.SectionDetails)
                                                          .Where(x => x.ParentSectionId == section.Id)
                                                          .SelectMany(x => x.SectionDetails)
                                                          .ToList();
                    
                    foreach (var detail in section.SectionDetails)
                    {
                        var oldSlot = _db.SectionSlots.Include(x => x.Section)
                                                      .Where(x => x.SectionId == detail.SectionId
                                                                  && x.Day == detail.Day
                                                                  && x.Date.Date >= today.Date
                                                                  && x.StartTime == detail.StartTime
                                                                  && x.EndTime == detail.EndTime
                                                                  && x.Status == "w"
                                                                  && !x.IsMakeUpClass)
                                                      .ToList();

                        foreach (var slot in oldSlot)
                        {
                            var reservedSlot = _db.RoomSlots.Include(x => x.Room)
                                                            .FirstOrDefault(x => !x.IsCancel
                                                                                 && x.SectionSlotId != slot.Id
                                                                                 && x.RoomId == detail.RoomId
                                                                                 && x.Date.Date == slot.Date
                                                                                 && x.StartTime < slot.EndTime
                                                                                 && x.EndTime > slot.StartTime);
                            if (reservedSlot == null)
                            {
                                _roomProvider.CancelAndInActiveRoomSlotBySectionSlotId(slot.Id);
                                
                                slot.RoomId = detail.RoomId;

                                _roomProvider.CreateRoomSlotBySectionSlot(slot);
                            }
                            else
                            {
                                if(reservedSlot.SectionSlotId != null)
                                {
                                    var detailSlotDuplicate = _db.SectionSlots.Include(x => x.Section)
                                                                                  .ThenInclude(x => x.Course)
                                                                              .Where(x => x.Id == reservedSlot.SectionSlotId)
                                                                              .Select(x => new
                                                                                           {
                                                                                               CourseCodeAndCredit = x.Section.Course.CodeAndCredit,
                                                                                               SectionNumber = x.Section.Number
                                                                                           })
                                                                              .First();
                                                            
                                    errorMessage += $"{ detailSlotDuplicate.CourseCodeAndCredit } SectionNumber:{ detailSlotDuplicate.SectionNumber } { reservedSlot.UsingTypeText } { reservedSlot.Room?.NameEn ?? "" } { reservedSlot.DateAndTime },";
                                }
                                else
                                {
                                    errorMessage += $"{ reservedSlot.UsingTypeText } { reservedSlot.Room?.NameEn ?? "" } { reservedSlot.DateAndTime },";
                                }
                                // overlap slot
                            }
                        }
                        var sectionDetail = _db.SectionDetails.FirstOrDefault(x => x.Id == detail.Id);
                        if (sectionDetail != null)
                        {
                            sectionDetail.RoomId = detail.RoomId;

                            var sameDetailInJoint = jointSectionDetails.Where(x => x.InstructorIds == sectionDetail.InstructorIds
                                                                                       && x.InstructorId == sectionDetail.InstructorId
                                                                                       && x.TeachingTypeId == sectionDetail.TeachingTypeId
                                                                                       && x.Day == sectionDetail.Day
                                                                                       && x.StartTime == sectionDetail.StartTime
                                                                                       && x.EndTime == sectionDetail.EndTime
                                                                                       && x.IsActive == sectionDetail.IsActive
                                                                                       && x.Remark == sectionDetail.Remark
                                                                              )
                                                                       .ToList();
                            sameDetailInJoint.ForEach(x => x.RoomId = detail.RoomId);
                        }
                       
                    }

                    section = _sectionProvider.GetSection(section.Id);

                    if (string.IsNullOrEmpty(errorMessage))
                    {

                        _db.SaveChanges();
                        transaction.Commit();

                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index), new  
                                                               {
                                                                   CodeAndName = section.Course.Code,
                                                                   AcademicLevelId = section.Term.AcademicLevelId, 
                                                                   TermId = section.TermId,
                                                                   SectionNumber = section.Number
                                                               });
                    }
                    else
                    {
                        transaction.Rollback();
                        _flashMessage.Warning("Duplicate Room " + errorMessage);
                        CreateSelectList();
                        return RedirectToAction(nameof(Edit), new 
                                                              {
                                                                  id = section.Id,
                                                                  returnUrl = returnUrl
                                                              });
                    }
                }
                catch
                {
                    transaction.Rollback();
                    CreateSelectList();
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(section);
                }
            }
        }

        [PermissionAuthorize("SectionSlotRoom", PolicyGenerator.Write)]
        public ActionResult ClearRoom(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var section = _sectionProvider.GetSection(id);
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {   var today = DateTime.Today;
                    var sectionDetails = _sectionProvider.GetSectionDetailsBySectionId(section.Id);
                    var allSectionSlot = _db.SectionSlots.Where(x => x.SectionId == section.Id)
                                                        .ToList();
                    bool adjustSomeSlot = false;
                    foreach (var detail in section.SectionDetails)
                    {
                        sectionDetails.Where(x => x.Id == detail.Id)
                                      .Select(x => 
                                                   {
                                                       x.RoomId = null;
                                                       return x;
                                                   })
                                      .ToList();
                        var oldSlot = allSectionSlot.Where(x => x.Day == detail.Day
                                                                 && x.Date.Date >= today.Date
                                                                 && x.Status == "w")
                                                     .ToList();

                        if (oldSlot.Any())
                        {
                            adjustSomeSlot = true;
                            oldSlot.Select(x =>
                            {
                                _roomProvider.CancelAndInActiveRoomSlotBySectionSlotId(x.Id);
                                x.RoomId = null;
                                return x;
                            })
                                   .ToList();
                        }
                    }

                    if (allSectionSlot.Any() && !adjustSomeSlot)
                    {
                        // case where cannot adjust anything == no slot that are waiting.
                        // check for case where all slot was cancel [from closed section, then was re open again therefore can delete room] 
                        if (allSectionSlot.All(x => x.Status == "c"))
                        {
                            allSectionSlot.Select(x =>
                            {
                                _roomProvider.CancelAndInActiveRoomSlotBySectionSlotId(x.Id);
                                x.RoomId = null;
                                return x;
                            })
                                .ToList();
                        }
                    }

                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);

                    return RedirectToAction(nameof(Index), new  
                                                        {
                                                            CodeAndName = section.Course.Code,
                                                            AcademicLevelId = section.Term.AcademicLevelId, 
                                                            TermId = section.TermId,
                                                            SectionNumber = section.Number
                                                        });
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToEdit);
                    return RedirectToAction(nameof(Index), new  
                                                        {
                                                            CodeAndName = section.Course.Code,
                                                            AcademicLevelId = section.Term.AcademicLevelId, 
                                                            TermId = section.TermId,
                                                            SectionNumber = section.Number
                                                        });
                }
            }
        }
        public ActionResult EditModal(long id) 
        {
            CreateSelectList();
            var sectionSlot = _db.SectionSlots.SingleOrDefault(x => x.Id == id);
            ViewBag.Rooms = _selectListProvider.GetAvailableRoom(sectionSlot.Date, sectionSlot.StartTime, sectionSlot.EndTime, string.Empty);
            return PartialView("_FormModal", sectionSlot);
        }

        [PermissionAuthorize("SectionSlotRoom", PolicyGenerator.Write)]
        [HttpPost]
        public async Task<ActionResult> EditSectionDetail(long id)
        {
            var model = _db.SectionSlots.SingleOrDefault(x => x.Id == id);
            var section = _sectionProvider.GetSection(model.SectionId);
            var errorMessage = "";
            if (ModelState.IsValid && await TryUpdateModelAsync<SectionSlot>(model))
            {
                var existedRoomSlot = _roomProvider.IsExistedRoomSlot(model.RoomId ?? 0, model.Date, model.StartTime, model.EndTime);
                if (model.StartTime >= model.EndTime)
                {
                    _flashMessage.Danger(Message.InvalidStartedTime);
                    return RedirectToAction(nameof(Index), new
                                                           {
                                                               AcademicLevelId = section.Term.AcademicLevelId,
                                                               TermId = section.TermId,
                                                               CodeAndName = section.Course.Code,
                                                               SectionNumber = section.Number
                                                           });
                }

                if (existedRoomSlot != null && existedRoomSlot?.SectionSlotId != model.Id)
                {
                    if(existedRoomSlot.SectionSlotId != null || existedRoomSlot.SectionSlotId != 0)
                    {
                        var sectionSlot = _db.SectionSlots.Include(x => x.Section)
                                                              .ThenInclude(x => x.Course)
                                                          .Where(x => x.Id == existedRoomSlot.SectionSlotId)
                                                          .Select(x => new 
                                                                       {
                                                                           SectionNumber = x.Section.Number,
                                                                           CourseCodeAndCredit = x.Section.Course.CodeAndCredit
                                                                       })
                                                          .First();
                        errorMessage += $"{ sectionSlot.CourseCodeAndCredit } Section Number: { sectionSlot.SectionNumber } { existedRoomSlot.UsingTypeText } { existedRoomSlot.Room?.NameEn ?? "" } { existedRoomSlot.DateAndTime }";
                    }
                    else
                    {
                        errorMessage += $"{ existedRoomSlot.UsingTypeText } { existedRoomSlot.Room?.NameEn ?? "" } { existedRoomSlot.DateAndTime }";
                    }

                    _flashMessage.Warning("Duplicate Room " + errorMessage);
                    return RedirectToAction(nameof(Edit), new
                                                           {
                                                               Id = section.Id
                                                           });
                }



                try
                {
                    model.Day = (int)model.Date.DayOfWeek;
                    await _db.SaveChangesAsync();
                    _roomProvider.CancelRoomSlotBySectionSlotId(model.Id);
                    _roomProvider.CreateRoomSlotBySectionSlot(model);
                    await _db.SaveChangesAsync();

                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch 
                { 
                    _flashMessage.Danger(Message.UnableToEdit);
                }  
            }

            return RedirectToAction(nameof(Edit), new
                                                   {
                                                        Id = section.Id
                                                   });
        }
        
        private void CreateSelectList(long academicLevelId = 0, long termId = 0, long courseId = 0) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId); 
            ViewBag.Sections = _selectListProvider.GetSections(termId, courseId);
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Instructors = _selectListProvider.GetInstructors();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            ViewBag.Dayofweeks = _selectListProvider.GetDayOfWeek(); 
            ViewBag.TeachingTypes = _selectListProvider.GetTeachingTypes();
            ViewBag.Status = _selectListProvider.GetSectionSlotStatus();
        }

        //Thu 6 Jan 2022 : Script to loop save UpdateRoom to assign room to the created slot
        //public ActionResult FixRoom()
        //{
        //    var sectionDetailList = (from sd in _db.SectionDetails
        //                             join s in _db.Sections on sd.SectionId equals s.Id
        //                             join ss in _db.SectionSlots on new
        //                             {
        //                                 sd.SectionId,
        //                                 sd.Day
        //                             } equals
        //                             new
        //                             {
        //                                 ss.SectionId,
        //                                 ss.Day
        //                             } into sss
        //                             from ss in sss.DefaultIfEmpty()
        //                             join rs in _db.RoomSlots on ss.Id equals rs.SectionSlotId into rss
        //                             from rs in rss.DefaultIfEmpty()
        //                             where s.TermId == 138
        //                                 //&& s.Id == 44039
        //                                 && (s.ParentSectionId == null || s.ParentSectionId == 0)
        //                                 && (ss.RoomId == null || rs == null)
        //                                 && sd.RoomId != null
        //                                 && ss.Status != "c"
        //                             select sd).Distinct().ToList();
        //    var log = "";
        //    var errorMessage = "";
        //    var today = DateTime.Today;
        //    foreach (var detail in sectionDetailList)
        //    {
        //        var innterError = "";
        //        using (var transaction = _db.Database.BeginTransaction())
        //        {
        //            try
        //            {

        //                var oldSlot = _db.SectionSlots.Include(x => x.Section)
        //                                              .Where(x => x.SectionId == detail.SectionId
        //                                                          && x.Day == detail.Day
        //                                                          && x.Date.Date >= today.Date
        //                                                          && x.StartTime == detail.StartTime
        //                                                          && x.EndTime == detail.EndTime
        //                                                          && x.Status == "w")
        //                                              .ToList();

        //                foreach (var slot in oldSlot)
        //                {
        //                    var reservedSlot = _db.RoomSlots.Include(x => x.Room)
        //                                                    .FirstOrDefault(x => !x.IsCancel
        //                                                                         && x.SectionSlotId != slot.Id
        //                                                                         && x.RoomId == detail.RoomId
        //                                                                         && x.Date.Date == slot.Date
        //                                                                         && x.StartTime < slot.EndTime
        //                                                                         && x.EndTime > slot.StartTime);
        //                    if (reservedSlot == null)
        //                    {
        //                        _roomProvider.CancelRoomSlotBySectionSlotId(slot.Id);

        //                        slot.RoomId = detail.RoomId;

        //                        _roomProvider.CreateRoomSlotBySectionSlot(slot);

        //                        log += $"{detail.RoomId},{slot.Id};";
        //                    }
        //                    else
        //                    {
        //                        if (reservedSlot.SectionSlotId != null)
        //                        {
        //                            var detailSlotDuplicate = _db.SectionSlots.Include(x => x.Section)
        //                                                                          .ThenInclude(x => x.Course)
        //                                                                      .Where(x => x.Id == reservedSlot.SectionSlotId)
        //                                                                      .Select(x => new
        //                                                                      {
        //                                                                          CourseCodeAndCredit = x.Section.Course.CodeAndCredit,
        //                                                                          SectionNumber = x.Section.Number
        //                                                                      })
        //                                                                      .First();

        //                            innterError += $"{ detailSlotDuplicate.CourseCodeAndCredit } SectionNumber:{ detailSlotDuplicate.SectionNumber } { reservedSlot.UsingTypeText } { reservedSlot.Room?.NameEn ?? "" } { reservedSlot.DateAndTime },";
        //                        }
        //                        else
        //                        {
        //                            innterError += $"{ reservedSlot.UsingTypeText } { reservedSlot.Room?.NameEn ?? "" } { reservedSlot.DateAndTime },";
        //                        }
        //                        // overlap slot
        //                    }
        //                }
        //                var sectionDetail = _db.SectionDetails.FirstOrDefault(x => x.Id == detail.Id);
        //                if (sectionDetail != null)
        //                {
        //                    sectionDetail.RoomId = detail.RoomId;
        //                }


        //                if (string.IsNullOrEmpty(innterError))
        //                {

        //                    _db.SaveChanges();
        //                    transaction.Commit();
        //                }
        //                else
        //                {
        //                    transaction.Rollback();
        //                    errorMessage += detail.Id + " " + innterError;
        //                }
        //            }
        //            catch
        //            {
        //                transaction.Rollback();
        //                errorMessage += detail.Id + " " + innterError;
        //            }
        //        }
        //    }
        //    if (string.IsNullOrEmpty(errorMessage))
        //    {
        //        _flashMessage.Confirmation(Message.SaveSucceed + log);
        //        return RedirectToAction(nameof(Index), new Criteria
        //        {

        //        });
        //    }
        //    else
        //    {
        //        _flashMessage.Confirmation(Message.SaveSucceed + log);
        //        _flashMessage.Warning("Duplicate Room " + errorMessage);
        //        return RedirectToAction(nameof(Index), new
        //        {

        //        });
        //    }
        //}
    }
}