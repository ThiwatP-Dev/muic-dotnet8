using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class SectionQuotaReportController : BaseController 
    {
        public SectionQuotaReportController(ApplicationDbContext db,
                                            IFlashMessage flashMessage,
                                            ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }
        
        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            var model = new SectionQuotaReportViewModel
                        {
                            Criteria = criteria
                        };

            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0 || criteria.TimeSlot == 0 || criteria.StartTime == null || criteria.EndTime == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var sectionDetails = _db.SectionDetails.Include(x => x.Section)
                                                       .ThenInclude(x => x.Course)
                                                       .ThenInclude(x => x.Faculty)
                                                   .Where(x => x.Section.Course.AcademicLevelId == criteria.AcademicLevelId
                                                               && x.Section.TermId == criteria.TermId
                                                               && x.StartTime >= criteria.StartTime
                                                               && criteria.EndTime <= x.EndTime
                                                               && (criteria.FacultyId == 0
                                                                   || x.Section.Course.FacultyId == criteria.FacultyId)
                                                               && (String.IsNullOrEmpty(criteria.Status)
                                                                   || (Convert.ToBoolean(criteria.Status) ? x.RoomId != null
                                                                                                          : x.RoomId == null)))
                                                   .ToList();

            var dayOfWeeks = new[] { "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" }.ToList();
            var timeIntervals = GetTimeIntervalList(criteria.StartTime, criteria.EndTime, criteria.TimeSlot);
            var sectionQuotaReports = new List<SectionQuotaReport>();
            foreach(var day in dayOfWeeks)
            {
                var sectionDetailInDay = sectionDetails.Where(x => x.Dayofweek == day);
                foreach(var time in timeIntervals)
                {
                    var sectionInTime = sectionDetailInDay.Where(x => x.StartTime >= time
                                                                      && time <= x.EndTime);
                    sectionQuotaReports.Add(new SectionQuotaReport
                                            {
                                                DayofWeek = day,
                                                Time = $"{ time.ToString(StringFormat.TimeSpan) } - { time.Add(new TimeSpan(0, criteria.TimeSlot, 59)).ToString(StringFormat.TimeSpan) }",
                                                FacultyNames = sectionInTime.Select(x => x.Section.Course.Faculty.Abbreviation)
                                                                            .Distinct()
                                                                            .OrderBy(x => x)
                                                                            .ToList()
                                            });
                }
            }

            model.SectionQuotaCounts = sectionQuotaReports.SelectMany(x => x.FacultyNames)
                                                          .GroupBy(x => x)
                                                          .Select(x => new SectionQuotaCount
                                                                       {
                                                                           Key = x.Key,
                                                                           Count = x.Count()
                                                                       })
                                                          .ToList();

            model.FacultySectionQuotaCounts = _db.SectionQuotas.Include(x => x.Faculty)
                                                               .Where(x => x.TermId == criteria.TermId)
                                                               .Select(x => new SectionQuotaCount
                                                                            {
                                                                                Key = x.Faculty.Abbreviation,
                                                                                Count = x.Quota
                                                                            })
                                                              .ToList();

            model.GrouppedSectionQuotaReports = sectionQuotaReports.GroupBy(x => x.Time)
                                                                   .ToList();

            return View(model);
        }

        private void CreateSelectList(long academicLevelId, long facultyId)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }
            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartments(facultyId);
            }
            ViewBag.AllYesNoAnswer = _selectListProvider.GetAllYesNoAnswer();
        }

        public List<TimeSpan> GetTimeIntervalList(TimeSpan startTime, TimeSpan endTime, int interval)
        {
            var timeIntervals = new List<TimeSpan>();
            var currentTime = startTime;
            while(currentTime < endTime)
            {
                timeIntervals.Add(currentTime);
                currentTime = currentTime.Add(new TimeSpan(0, interval, 0));
            }
            return timeIntervals;
        }
    }
}