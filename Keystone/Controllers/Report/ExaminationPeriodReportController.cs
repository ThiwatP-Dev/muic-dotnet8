using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("ExaminationPeriodReport", "")]
    public class ExaminationPeriodReportController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IRoomProvider _roomProvider;

        public ExaminationPeriodReportController(ApplicationDbContext db,
                                                 ISelectListProvider selectListProvider,
                                                 IFlashMessage flashMessage,
                                                 IDateTimeProvider dateTimeProvider,
                                                 IAcademicProvider academicProvider,
                                                 IRoomProvider roomProvider) : base(db, flashMessage, selectListProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _academicProvider = academicProvider;
            _roomProvider = roomProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);

            var model = new ExaminationPeriodReportViewModel
                        {
                            Criteria = criteria
                        };

            if (criteria.TermId == 0
                || string.IsNullOrEmpty(criteria.ExaminationType)
                || string.IsNullOrEmpty(criteria.StartedAt)
                || string.IsNullOrEmpty(criteria.EndedAt)
                || criteria.CampusId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            DateTime? startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);

            var examinationRoom = _db.Sections.AsNoTracking()
                                              .Include(x => x.MidtermRoom)
                                                  .ThenInclude(x => x.Building)
                                              .Include(x => x.FinalRoom)
                                                  .ThenInclude(x => x.Building)
                                              .Include(x => x.RegistrationCourses)
                                              .Where(x => x.TermId == criteria.TermId
                                                          && x.MidtermDate != null
                                                          && x.FinalDate != null);
            
            if (criteria.ExaminationType == "Midterm")
            {
                examinationRoom = examinationRoom.Where(x => x.MidtermRoom.Building.CampusId == criteria.CampusId
                                                             && (startedAt == null
                                                                 || x.MidtermDate.Value.Date >= startedAt.Value.Date)
                                                             && (endedAt == null
                                                                 || x.MidtermDate.Value.Date <= endedAt.Value.Date));
            }
            else
            {
                examinationRoom = examinationRoom.Where(x => x.FinalRoom.Building.CampusId == criteria.CampusId
                                                             && (startedAt == null
                                                                 || x.FinalDate.Value.Date >= startedAt.Value.Date)
                                                             && (endedAt == null
                                                                 || x.FinalDate.Value.Date <= endedAt.Value.Date));
            }

            var examinationRoomReport = examinationRoom.GroupBy(x => criteria.ExaminationType == "Midterm" ? x.MidtermDate : x.FinalDate)
                                                       .Select(x => new ExaminationPeriodByCourseReport
                                                                    {
                                                                        ExaminationAt = x.Key.Value,
                                                                        ExaminationPeriodByCourseDetails = x.Select(y => new ExaminationPeriodByCourseDetail
                                                                                                                         {
                                                                                                                             SectionId = y.Id,
                                                                                                                             ExamStartTime = criteria.ExaminationType == "Midterm" ? y.MidtermStart : y.FinalStart,
                                                                                                                             ExamEndTime = criteria.ExaminationType == "Midterm" ? y.MidtermEnd : y.FinalEnd,
                                                                                                                             CourseCode = y.Course.Code,
                                                                                                                             CourseName = y.Course.NameEn,
                                                                                                                             TotalStudent = y.RegistrationCourses.Count(z => z.GradeName != "W"
                                                                                                                                                                             && (z.Status == "a"
                                                                                                                                                                                 || z.Status == "r"))
                                                                                                                         })
                                                                                                            .OrderBy(y => y.ExamStartTime)
                                                                                                            .ThenBy(y => y.ExamEndTime)
                                                                                                            .ToList()
                                                                    })
                                                       .OrderBy(x => x.ExaminationAt)
                                                       .ToList();

            model.StartDate = startedAt.Value.ToString(StringFormat.DayOfWeekMonthYear);
            model.EndDate = endedAt.Value.ToString(StringFormat.DayOfWeekMonthYear);
            model.Term = _academicProvider.GetTerm(criteria.TermId)?.TermText ?? "N/A";
            model.CampusName = _roomProvider.GetCampus(criteria.CampusId)?.NameEn ?? "N/A";
            model.ExaminationPeriodByCourseReports = examinationRoomReport;

            if (criteria.IsShowByTime)
            {
                model.SummaryExaminationPeriodReportViewModels = model.ExaminationPeriodByCourseReports.Select(x => new SummaryExaminationPeriodReportViewModel
                                                                                                                    {
                                                                                                                        Date = x.ExaminationAt,
                                                                                                                        SummaryExaminationPeriodDetails = x.ExaminationPeriodByCourseDetails.GroupBy(y => new { y.BeginTime, y.EndTime })
                                                                                                                                                                                            .Select(y => new SummaryExaminationPeriodDetail
                                                                                                                                                                                                         {
                                                                                                                                                                                                             Time = $"{ y.Key.BeginTime } - { y.Key.EndTime }",
                                                                                                                                                                                                             Count =  y.Sum(z => z.TotalStudent)
                                                                                                                                                                                                         })
                                                                                                                                                                                            .ToList()
                                                                                                                    })
                                                                                                        .ToList();

                model.ExamTimes = model.SummaryExaminationPeriodReportViewModels.SelectMany(x => x.SummaryExaminationPeriodDetails.Select(y => y.Time))
                                                                                .Distinct()
                                                                                .OrderBy(x => DateTime.Parse(x.Split(" - ")[0]))
                                                                                .ToList();
            }
            
            return View(model);
        }
        
        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.ExaminationTypes = _selectListProvider.GetMidtermAndFinal();
            ViewBag.Campuses = _selectListProvider.GetCampuses();

            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}