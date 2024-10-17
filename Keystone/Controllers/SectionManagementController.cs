using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using System.Security.Claims;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("SectionManagement", "")]
    public class SectionManagementController : BaseController
    {
        protected readonly ISectionProvider _sectionProvider;
        protected readonly IRegistrationProvider _registrationProvider;

        public SectionManagementController(ApplicationDbContext db,
                                           IFlashMessage flashMessage,
                                           ISelectListProvider selectListProvider,
                                           IHttpContextAccessor accessor,
                                           ISectionProvider sectionProvider,
                                           IRegistrationProvider registrationProvider) : base(db, flashMessage, selectListProvider, accessor)
        {
            _sectionProvider = sectionProvider;
            _registrationProvider = registrationProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.CourseId, criteria.Status);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                 _flashMessage.Warning(Message.RequiredData);
                 return View();
            }
            
            var sections = _db.Sections.Where(x => x.TermId == criteria.TermId
                                                   && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                       || x.Course.Code.Contains(criteria.CodeAndName)
                                                       || x.Course.NameEn.Contains(criteria.CodeAndName))
                                                   && (string.IsNullOrEmpty(criteria.SectionNumber)
                                                     || x.Number.Contains(criteria.SectionNumber))
                                                   && (string.IsNullOrEmpty(criteria.SeatAvailable)
                                                       || (Convert.ToBoolean(criteria.SeatAvailable) ? x.SeatAvailable > 0
                                                                                                     : x.SeatAvailable == 0))
                                                   && (String.IsNullOrEmpty(criteria.Status)
                                                       || x.IsActive == Convert.ToBoolean(criteria.Status))
                                                   && (string.IsNullOrEmpty(criteria.Type)
                                                       || x.Status == criteria.Type)
                                                   && (criteria.InstructorId == 0
                                                       || x.MainInstructorId == criteria.InstructorId)
                                                   && (string.IsNullOrEmpty(criteria.SectionType)
                                                       || (criteria.SectionType == "o" ? x.IsOutbound
                                                                                       : criteria.SectionType == "g"
                                                                                       ? x.IsSpecialCase
                                                                                       : criteria.SectionType == "j"
                                                                                       ? x.ParentSectionId != null
                                                                                       : x.ParentSectionId == null))
                                             )
                                       .Select(x => new SectionManagementViewModel
                                                    {
                                                        Id = x.Id,
                                                        CourseCode = x.Course.Code,
                                                        CourseRateId = x.Course.CourseRateId,
                                                        CourseCredit = x.Course.Credit,
                                                        CourseName = x.Course.NameEn,
                                                        CourseLab = x.Course.Lab,
                                                        CourseLecture = x.Course.Lecture,
                                                        CourseOther = x.Course.Other,
                                                        Title = x.MainInstructor.Title.NameEn,
                                                        FirstNameEn = x.MainInstructor.FirstNameEn,
                                                        LastNameEn = x.MainInstructor.LastNameEn,
                                                        Number = x.Number,
                                                        MidtermDate = x.MidtermDate,
                                                        MidtermStart = x.MidtermStart,
                                                        MidtermEnd = x.MidtermEnd,
                                                        FinalDate = x.FinalDate,
                                                        FinalStart = x.FinalStart,
                                                        FinalEnd = x.FinalEnd,
                                                        SeatLimit = x.SeatLimit,
                                                        SeatUsed = x.SeatUsed,
                                                        PlanningSeat = x.PlanningSeat,
                                                        Status = x.Status,
                                                        IsClosed = x.IsClosed,
                                                        IsOutbound = x.IsOutbound,
                                                        IsSpecialCase = x.IsSpecialCase,
                                                        ApprovedBy = x.ApprovedBy,
                                                        ApprovedAtText = x.ApprovedAtText,
                                                        ParentSectionId = x.ParentSectionId,
                                                        ParentSeatUsed = x.ParentSection == null ? 0 : x.ParentSection.SeatUsed,
                                                        ParentSectionCourseCode = x.ParentSection.Course.Code,
                                                        ParentSectionNumber = x.ParentSection.Number,
                                                        ParentCourseRateId = x.ParentSection.Course.CourseRateId,
                                                        Remark = x.Remark
                                                    })
                                       .OrderBy(x => x.CourseCode)
                                       .GetPaged(criteria, page);

            var sectionIds = sections.Results.Select(x => x.Id).ToList();
            var sectionIdsNullable = sections.Results.Select(x => (long?)x.Id).ToList();
            var parentSectionIds = sections.Results.Where(x => x.ParentSectionId != null).Select(x => x.ParentSectionId);
            var jointTotalSeatUsed = _db.Sections.Where(x => parentSectionIds.Contains(x.ParentSectionId));
            var jointSections = _db.Sections.Where(x => sectionIdsNullable.Contains(x.ParentSectionId))
                                            .Select(x => new JointSectionManagementViewModel
                                                         {
                                                             Id = x.Id,
                                                             ParentSectionId = x.ParentSectionId,
                                                             Number = x.Number,
                                                             CourseCode = x.Course.Code,
                                                             SeatUsed = x.SeatUsed,
                                                             CourseRateId = x.Course.CourseRateId
                                                         })
                                            .ToList();

            var sectionDetails = _db.SectionDetails.Where(x => sectionIds.Contains(x.SectionId))
                                                   .Select(x => new SectionDetailManagementViewModel
                                                                {
                                                                    SectionId = x.SectionId,
                                                                    Day = x.Day,
                                                                    StartTime = x.StartTime,
                                                                    EndTime = x.EndTime
                                                                })
                                                   .OrderBy(x => x.Day)
                                                      .ThenBy(x => x.StartTime)
                                                   .ToList();
                                                   
            foreach (var item in sections.Results)
            {
                item.JointSections = jointSections.Where(x => x.ParentSectionId == item.Id)
                                                  .ToList();

                if (item.ParentSectionId == null)
                {
                    item.TotalSeatUsed = item.SeatUsed + item.JointSections.Sum(x => x.SeatUsed);
                }
                else
                {
                    item.TotalSeatUsed = item.ParentSeatUsed + jointTotalSeatUsed.Where(x => x.ParentSectionId == item.ParentSectionId)
                                                                                 .Sum(x => x.SeatUsed);
                }

                item.SectionDetails = sectionDetails.Where(x => x.SectionId == item.Id)
                                                    .OrderBy(x => x.Day)
                                                       .ThenBy(x => x.StartTime)
                                                    .ToList();
            }

            return View(sections);
        }

        public IActionResult Details(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var section = _sectionProvider.GetSection(id);
            var jointSections = _db.Sections.Where(x => x.ParentSectionId == section.Id)
                                            .Select(x => new JointSectionManagementViewModel
                                                         {
                                                             SeatUsed = x.SeatUsed
                                                         })
                                            .ToList();
            section.SeatUsed = section.SeatUsed + jointSections.Sum(x => x.SeatUsed);
            return View(section);
        }

        [PermissionAuthorize("SectionManagement", PolicyGenerator.Write)]
        public IActionResult AddSlot(long id)
        {
            var model = _sectionProvider.GetSection(id);  
            var jointSections = _sectionProvider.GetJointSections(model.Id);
            var userId = _accessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            // var roomSlots = new List<RoomSlot>();
            // foreach (var item in model.SectionSlots)
            // {
            //     if (item.RoomId != null)
            //     {
            //         roomSlots.Add(new RoomSlot
            //                       {
            //                           TermId = model.TermId,
            //                           RoomId = item.RoomId ?? 0,
            //                           Day = (int)item.Day,
            //                           Date = item.Date,
            //                           StartTime = item.StartTime,
            //                           EndTime = item.EndTime,
            //                           UsingType = "s",
            //                           SectionSlotId = item.Id,
            //                           IsCancel = false
            //                       });
            //     }
            // }

            DateTime updatedAt = DateTime.UtcNow;

            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {           
                    model.Status = "a";
                    model.ApprovedAt = updatedAt;
                    model.ApprovedBy = userId;
                    // _db.RoomSlots.AddRange(roomSlots);
                    
                    foreach (var jointSection in jointSections)
                    {
                        jointSection.Status = "a";
                        jointSection.ApprovedAt = updatedAt;
                        jointSection.ApprovedBy = userId;
                    }
                    
                    _db.SaveChanges();

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToCreate);
                    transaction.Rollback();
                }
            }

            // Call USPARK API to add section
            // if (_registrationProvider.IsRegistrationPeriod(updatedAt, model.TermId))
            // {
                try
                {
                    _registrationProvider.CallUSparkAPIAddSection(model.Id);

                    // Joint Section
                    foreach (var jointSection in jointSections)
                    {
                        _registrationProvider.CallUSparkAPIAddSection(jointSection.Id);
                    }
                }
                catch (Exception e)
                {
                    _flashMessage.Danger(e.Message);
                }
            // }

            return RedirectToAction(nameof(Index), new 
                                                   {
                                                       AcademicLevelId = model.Term.AcademicLevelId,
                                                       TermId = model.TermId,
                                                       CodeAndName = model.Course.Code,
                                                       Status = model.IsActive.ToString().ToLower()
                                                   });
        }

        // public IActionResult WaitingSlot(long id)
        // {
        //     var model = _sectionProvider.GetSection(id);
        //     var removeRoomSlot = _db.RoomSlots.Where(x => x.SectionSlotId == id)
        //                                       .ToList();

        //     var jointSections = _sectionProvider.GetJointSections(model.Id);
        //     try
        //     {
        //         model.Status = "c";
        //         model.ApprovedAt = null;
        //         model.ApprovedBy = null;
        //         model.FinalDate = null;
        //         model.FinalStart = null;
        //         model.FinalEnd = null;
        //         model.FinalRoomId = null;
        //         if (removeRoomSlot != null)
        //         {
        //             removeRoomSlot.Select(x => {
        //                                            x.IsCancel = true;
        //                                            return x; 
        //                                        })
        //                           .ToList();
        //         }

        //         jointSections.Select(x => {
        //                                      x.Status = "c";
        //                                      x.ApprovedAt = null;
        //                                      x.ApprovedBy = null;
        //                                      x.FinalDate = null;
        //                                      x.FinalStart = null;
        //                                      x.FinalEnd = null;
        //                                      x.FinalRoomId = null;
        //                                      return x;
        //                                   }).ToList();

        //         _db.SaveChanges();
        //         _flashMessage.Confirmation(Message.SaveSucceed);
        //     }
        //     catch
        //     {
        //         _flashMessage.Danger(Message.UnableToDelete);
        //     }

        //     return RedirectToAction(nameof(Index), new Criteria
        //                                            {
        //                                                AcademicLevelId = model.Term.AcademicLevelId,
        //                                                TermId = model.TermId,
        //                                                CourseId = model.CourseId,
        //                                                Status = model.IsActive.ToString().ToLower()
        //                                            });
        // }

        [PermissionAuthorize("SectionManagement", PolicyGenerator.Write)]
        public IActionResult RejectSlot(long id)
        {
            var model = _sectionProvider.GetSection(id);
            if(_db.RegistrationCourses.Any(x => x.SectionId == id
                                                && x.Status != "d"))
            {
                _flashMessage.Danger(Message.StudentsInSectionNotChangeStatus);
            }
            else
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    var removeRoomSlot = _db.RoomSlots.Where(x => x.SectionSlot.SectionId == id
                                                            )
                                                      .ToList();

                    var jointSections = _sectionProvider.GetJointSections(model.Id);
                    try
                    {
                        model.Status = "c";
                        model.ApprovedAt = null;
                        model.ApprovedBy = null;
                        model.FinalDate = null;
                        model.FinalStart = null;
                        model.FinalEnd = null;
                        model.FinalRoomId = null;
                        if (removeRoomSlot != null)
                        {
                            removeRoomSlot.Select(x =>
                            {
                                x.IsCancel = true;
                                x.IsActive = false;
                                return x;
                            })
                                          .ToList();
                        }

                        jointSections.Select(x =>
                        {
                            x.Status = "c";
                            x.ApprovedAt = null;
                            x.ApprovedBy = null;
                            x.FinalDate = null;
                            x.FinalStart = null;
                            x.FinalEnd = null;
                            x.FinalRoomId = null;
                            return x;
                        }).ToList();

                        var finalId = _db.ExaminationTypes.SingleOrDefault(x => x.NameEn.ToLower() == "final").Id;
                        var examinationReservation = _db.ExaminationReservations.SingleOrDefault(x => x.SectionId == model.Id
                                                                                                      && x.ExaminationTypeId == finalId);
                        if (examinationReservation != null)
                        {
                            examinationReservation.IsActive = false;
                            examinationReservation.Status = "r";

                            var roomSlots = _db.RoomSlots.Where(x => x.ExaminationReservationId == examinationReservation.Id
                                                                             && !x.IsCancel
                                                                             && x.IsActive)
                                                         .ToList();
                            if (roomSlots != null)
                            {
                                roomSlots.Select(x =>
                                {
                                    x.IsActive = false;
                                    x.IsCancel = true;
                                    return x;
                                }).ToList();
                            }
                        }

                        _db.SaveChanges();
                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                    }
                    catch
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.UnableToDelete);
                    }
                }
            }

            return RedirectToAction(nameof(Index), new 
                                                   {
                                                       AcademicLevelId = model.Term.AcademicLevelId,
                                                       TermId = model.TermId,
                                                       CodeAndName = model.Course.Code,
                                                       Status = model.IsActive.ToString().ToLower()
                                                   });
        }

        private void CreateSelectList(long academicLevelId = 0, long termId = 0, long courseId = 0, string sectionStatus = null) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Courses = _selectListProvider.GetCoursesBySectionStatus(termId, sectionStatus);
            }

            if (courseId > 0)
            {
                ViewBag.Sections = _selectListProvider.GetSections(termId, courseId);
            }

            ViewBag.Statuses = _selectListProvider.GetActiveStatuses();
            ViewBag.SeatAvailableStatuses = _selectListProvider.GetSeatAvailableStatuses();
            ViewBag.SectionStatuses = _selectListProvider.GetAllYesNoAnswer();
            ViewBag.SectionStatusTypes = _selectListProvider.GetSectionStatuses();
            ViewBag.SectionTypes = _selectListProvider.GetSectionTypes();
            ViewBag.Instructors = _selectListProvider.GetInstructors();
        }
    }
}