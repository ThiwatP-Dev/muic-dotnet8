using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using KeystoneLibrary.Data;
using Keystone.Permission;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("RoomHistoryReport", "")]
    public class RoomHistoryReportController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IUserProvider _userProvider;

        public RoomHistoryReportController(ApplicationDbContext db,
                                           ISelectListProvider selectListProvider,
                                           IFlashMessage flashMessage,
                                           IUserProvider userProvider,
                                           IDateTimeProvider dateTimeProvider) : base (db, flashMessage, selectListProvider) 
        {
            _dateTimeProvider = dateTimeProvider;
            _userProvider = userProvider;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.CampusId);
            DateTime? starteAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);
            if (starteAt == null && endedAt == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var model = _db.RoomSlots.Where(x => x.Date.Date >= starteAt.Value.Date
                                                 && x.Date.Date <= endedAt.Value.Date
                                                 && (string.IsNullOrEmpty(criteria.Type)
                                                     || x.UsingType == criteria.Type)
                                                 && (criteria.CampusId == 0
                                                     || x.Room.Building.CampusId == criteria.CampusId)
                                                 && (criteria.BuildingId == 0
                                                     || x.Room.BuildingId == criteria.BuildingId)
                                                 && (criteria.Floor == null
                                                     || x.Room.Floor == criteria.Floor)
                                                 && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                     || x.Room.NameEn.Contains(criteria.CodeAndName))
                                                 && (string.IsNullOrEmpty(criteria.IsCancel)
                                                     || x.IsCancel == Convert.ToBoolean(criteria.IsCancel))
                                                 && (string.IsNullOrEmpty(criteria.IsMakeUp)
                                                     || (x.SectionSlotId != null 
                                                         && x.SectionSlot.IsMakeUpClass == Convert.ToBoolean(criteria.IsMakeUp))))
                                     .Select(x => new RoomHistoryReportViewModel
                                                  {
                                                      Name = x.Room.NameEn,
                                                      Campus = x.Room.Building.Campus.NameEn,
                                                      Building = x.Room.Building.NameEn,
                                                      Floor = x.Room.Floor,
                                                      StartTime = x.StartTime,
                                                      EndTime = x.EndTime,
                                                      UsingType = x.UsingTypeText,
                                                      Date = x.Date,
                                                      Cancel = x.IsCancel,
                                                      ExaminationCourseCode = x.ExaminationReservation.Section.Course.Code,
                                                      ExaminationCourseName = x.ExaminationReservation.Section.Course.NameEn,
                                                      ExaminationSectionNumber= x.ExaminationReservation.Section.Number,
                                                      ExaminationInstructor = x.ExaminationReservation.Section.MainInstructor == null ? "" : x.ExaminationReservation.Section.MainInstructor.Title.NameEn + " " + x.ExaminationReservation.Section.MainInstructor.FirstNameEn +  " " + x.ExaminationReservation.Section.MainInstructor.LastNameEn,
                                                      SectionCourseCode = x.SectionSlot.Section.Course.Code,
                                                      SectionCourseName = x.SectionSlot.Section.Course.NameEn,
                                                      SectionInstructor = x.SectionSlot.Section.MainInstructor == null ? "" : x.SectionSlot.Section.MainInstructor.Title.NameEn + " " + x.SectionSlot.Section.MainInstructor.FirstNameEn +  " " + x.SectionSlot.Section.MainInstructor.LastNameEn,
                                                      SectionNumber = x.SectionSlot.Section.Number,
                                                      MakeUp = x.SectionSlot.IsMakeUpClass,
                                                      RoomReservationRemark = x.RoomReservation.Remark,
                                                      CreatedAt = x.RoomReservation.CreatedAt,
                                                      CreatedBy = x.RoomReservation.CreatedBy
                                                  })
                                     .ToList();
            var createByIds = model.Where(x => !string.IsNullOrEmpty(x.CreatedBy)).Select(x => x.CreatedBy).Distinct().ToList();
            var createBys =  _userProvider.GetCreatedFullNameByIds(createByIds);
            if (criteria.OrderBy == "b")
            {
                model = model.Select(x => {
                                            if(!string.IsNullOrEmpty(x.CreatedBy))
                                            {
                                                var createBy = createBys.FirstOrDefault(y => x.CreatedBy == y.CreatedBy);

                                                if(createBy != null)
                                                {
                                                    x.CreatedBy = createBy.CreatedByFullNameEn;
                                                }                                                
                                                else
                                                {
                                                    x.CreatedBy = "-";
                                                }
                                            }
                                            else
                                            {
                                                x.CreatedBy = "-";
                                            }
                                            return x;
                                          })
                             .OrderBy(x => x.Building)
                                 .ThenBy(x => x.Floor)
                                 .ThenBy(x => x.Name)
                                 .ThenBy(x => x.Date)
                                 .ThenBy(x => x.StartTime)
                                 .ThenBy(x => x.EndTime)
                             .ToList();
            }
            else if (criteria.OrderBy == "r")
            {
                model = model.Select(x => {
                                            if(!string.IsNullOrEmpty(x.CreatedBy))
                                            {
                                                var createBy = createBys.FirstOrDefault(y => x.CreatedBy == y.CreatedBy);

                                                if(createBy != null)
                                                {
                                                    x.CreatedBy = createBy.CreatedByFullNameEn;
                                                }
                                                else
                                                {
                                                    x.CreatedBy = "-";
                                                }
                                            }
                                            else
                                            {
                                                x.CreatedBy = "-";
                                            }
                                            return x;
                                          })
                             .OrderBy(x => x.Name)
                                 .ThenBy(x => x.Date)
                                 .ThenBy(x => x.StartTime)
                                 .ThenBy(x => x.EndTime)
                             .ToList();
            }
            else
            {
                model = model.Select(x => {
                                            if(!string.IsNullOrEmpty(x.CreatedBy))
                                            {
                                                var createBy = createBys.FirstOrDefault(y => x.CreatedBy == y.CreatedBy);

                                                if(createBy != null)
                                                {
                                                    x.CreatedBy = createBy.CreatedByFullNameEn;
                                                }                                                
                                                else
                                                {
                                                    x.CreatedBy = "-";
                                                }
                                            }
                                            else
                                            {
                                                x.CreatedBy = "-";
                                            }
                                            return x;
                                          })
                            .OrderBy(x => x.Date)
                                 .ThenBy(x => x.StartTime)
                                 .ThenBy(x => x.Building)
                                 .ThenBy(x => x.Floor)
                                 .ThenBy(x => x.Name)
                             .ToList();
            }

            var modelPagedResult = model.AsQueryable()
                                        .GetPaged(criteria, page, true);

            ViewBag.TotalActive = model != null ? model.Where(x => !x.Cancel).Count() : 0;
            ViewBag.TotalCancel = model != null ? model.Where(x => x.Cancel).Count() : 0;
            return View(modelPagedResult);
        }

        private void CreateSelectList(long campusId = 0)
        {
            ViewBag.Campuses = _selectListProvider.GetCampuses();
            ViewBag.UsingTypes = _selectListProvider.GetUsingTypes();
            ViewBag.Answers = _selectListProvider.GetAllYesNoAnswer();
            ViewBag.OrderBy = _selectListProvider.GetRoomSlotOrderBy();
            if (campusId > 0)
            {
                ViewBag.Buildings = _selectListProvider.GetBuildings(campusId);
            }
        }
    }
}