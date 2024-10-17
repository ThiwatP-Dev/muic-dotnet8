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
    [PermissionAuthorize("HolidayCalendar", "")]
    public class HolidayCalendarController : BaseController
    {
        public HolidayCalendarController(ApplicationDbContext db,
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
            //if (criteria.RoomId == 0 || criteria.DateCheck == null)
            //{
            //    _flashMessage.Warning(Message.RequiredData);
            //    return View();
            //}

            var model = new HolidayCalendarViewModel();
            model.Details = _db.MuicHolidays.IgnoreQueryFilters()
                                            .AsNoTracking()
                                            .Where(x => (string.IsNullOrEmpty(criteria.IsCancel)
                                                            || x.IsActive == !Convert.ToBoolean(criteria.IsCancel))
                                                            && (!criteria.FromDate.HasValue ||
                                                               x.EndedAt.Date >= criteria.FromDate.Value.Date)
                                                           && (!criteria.ToDate.HasValue ||
                                                               x.StartedAt.Date <= criteria.ToDate.Value.Date)
                                                           )
                                            .Select(x => new HolidayCalendarDetailViewModel
                                                         {
                                                             Start = x.StartedAt,
                                                             End = x.EndedAt,
                                                             Type = "MUIC",
                                                             IsCancel = !x.IsActive,
                                                             Title = x.Name,
                                                             Remark = x.Remark,

                                                             CreatedAt = x.CreatedAt,
                                                             CreatedBy = x.CreatedBy,
                                                             Id = x.Id,
                                                             MuicId = x.MuicId,
                                                             IsAllowMakeup = x.IsMakeUpAble,
                                                             UpdatedAt = x.UpdatedAt,
                                                             UpdatedBy = x.UpdatedBy,
                                            })
                                            .ToList();

            foreach (var item in model.Details)
            {
                if (item.End.Date < DateTime.Now.Date)
                {
                    item.Color = Grey;
                }
                else if (item.IsCancel)
                {
                    item.Color = Red;
                }
                else
                {
                    item.Color = Green;
                }
                if (item.End == item.End.Date && item.End != item.Start)
                {
                    item.End = item.End.AddDays(1);
                }
            }
            
            return View(model);
        }

        public ActionResult Details(long id)
        {
            HolidayCalendarDetailViewModel holidayDetail = new HolidayCalendarDetailViewModel();

            holidayDetail = _db.MuicHolidays.IgnoreQueryFilters()
                                           .AsNoTracking()
                                           .Where(x => x.Id == id)                                                          
                                           .Select(x => new HolidayCalendarDetailViewModel
                                           {
                                               Start = x.StartedAt,
                                               End = x.EndedAt,
                                               Type = "MUIC",
                                               IsCancel = !x.IsActive,
                                               Title = x.Name,
                                               Remark = x.Remark,

                                               CreatedAt = x.CreatedAt,
                                               CreatedBy = x.CreatedBy,
                                               Id = x.Id,
                                               MuicId = x.MuicId,
                                               IsAllowMakeup = x.IsMakeUpAble,
                                               UpdatedAt = x.UpdatedAt,
                                               UpdatedBy = x.UpdatedBy,
                                           })
                                           .FirstOrDefault();

            if (holidayDetail != null)
            {
                var sectionSlots = _db.SectionSlots.AsNoTracking()
                                                   .IgnoreQueryFilters()
                                                   .Include(x => x.Room)
                                                   .Include(x => x.TeachingType)
                                                   .Include(x => x.Section)
                                                      .ThenInclude(x => x.Course)
                                                   .Where(x => x.Date >= holidayDetail.Start
                                                                  && x.Date < holidayDetail.End.AddDays(1)
                                                                  && x.IsActive
                                                                  && x.Section.IsActive)
                                                   .OrderBy(x => x.Date)
                                                        .ThenBy(x => x.StartTime)
                                                   .AsNoTracking()
                                                   .ToList();
                holidayDetail.SectionSlots = sectionSlots;
            }

            return PartialView("_DetailInfo", holidayDetail);
        }

        private void CreateSelectList()
        {
            ViewBag.Answers = _selectListProvider.GetAllYesNoAnswer();
        }
    }
}