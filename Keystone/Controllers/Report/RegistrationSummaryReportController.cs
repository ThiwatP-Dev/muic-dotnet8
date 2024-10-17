using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    public class RegistrationSummaryReportController : BaseController
    {
        public RegistrationSummaryReportController(ApplicationDbContext db,
                                                   IFlashMessage flashMessage,
                                                   ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }
        
        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.CourseId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var results = new List<RegistrationSummaryReport>();
            if (criteria.Type == "s")
            {
                var registrations = _db.RegistrationCourses.Include(x => x.Section)
                                                           .Include(x => x.Student)
                                                           .Where(x => x.TermId == criteria.TermId
                                                                       && x.Status != "d"
                                                                       && (criteria.CourseId == 0 
                                                                           || x.CourseId == criteria.CourseId)
                                                                       && (criteria.SectionId == 0 
                                                                           || x.SectionId == criteria.SectionId)
                                                                       && (string.IsNullOrEmpty(criteria.IsJointSection)
                                                                           || (x.SectionId != null 
                                                                               && x.Section.IsMasterSection == !Convert.ToBoolean(criteria.IsJointSection))))
                                                           .OrderBy(x => x.Student.Code)
                                                           .Select(x => new 
                                                                        {
                                                                            x.Student.Code,
                                                                            x.Student.FullNameEn,
                                                                            IsMasterSection = x.SectionId == null ? false : x.Section.IsMasterSection,
                                                                            x.Course.RegistrationCredit,
                                                                            x.Course.Credit
                                                                        })
                                                           .ToList();
                foreach (var item in registrations.GroupBy(x => new { x.Code, x.FullNameEn }))
                {
                    results.Add(new RegistrationSummaryReport
                                {
                                    StudentCode = item.Key.Code,
                                    StuentFullNameEn = item.Key.FullNameEn,
                                    Master = item.Count(x => x.IsMasterSection),
                                    Joint = item.Count(x => !x.IsMasterSection),
                                    RegistrationCredit = item.Sum(x => x.RegistrationCredit),
                                    AcademicCredit = item.Sum(x => x.Credit)
                                });
                }
            }
            else
            {
                var sections = _db.Sections.Include(x => x.Course)
                                           .Where(x => x.TermId == criteria.TermId
                                                       && (criteria.CourseId == 0 
                                                           || x.CourseId == criteria.CourseId)
                                                       && (criteria.SectionId == 0 
                                                           || x.Id == criteria.SectionId)
                                                       && (string.IsNullOrEmpty(criteria.IsJointSection)
                                                           || x.IsMasterSection == !Convert.ToBoolean(criteria.IsJointSection)))
                                           .Select(x => new 
                                                        {
                                                            x.Course.CourseAndCredit,
                                                            x.IsMasterSection,
                                                            TotalRegistration = _db.RegistrationCourses.Count(y => y.SectionId == x.Id
                                                                                                                   && x.Status != "d")
                                                        })
                                           .ToList();

                foreach (var item in sections.GroupBy(x => x.CourseAndCredit))
                {
                    results.Add(new RegistrationSummaryReport
                                {
                                    Course = item.Key,
                                    Section = item.Count(),
                                    Master = item.Count(x => x.IsMasterSection),
                                    Joint = item.Count(x => !x.IsMasterSection),
                                    TotalRegistration = item.Sum(x => x.TotalRegistration)
                                });
                }
            }

            var model = new RegistrationSummaryReportViewModel
                        {
                            Criteria = criteria,
                            Results = results
                        };

            return View(model);
        }

        private void CreateSelectList(long academicLevelId, long termId, long courseId) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.AllYesNoAnswer = _selectListProvider.GetAllYesNoAnswer();
            ViewBag.Types = _selectListProvider.GetRegistrationSummaryReportTypes();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }

            if (termId > 0)
            {
                ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);
            }

            if (courseId > 0)
            {
                ViewBag.Sections = _selectListProvider.GetSections(termId, courseId);
            }
        }
    }
}