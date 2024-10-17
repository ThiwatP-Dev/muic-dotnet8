using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("RegistrationStatisticReport", "")]
    public class RegistrationStatisticReportController : BaseController
    {
        public RegistrationStatisticReportController(ApplicationDbContext db,
                                                     IFlashMessage flashMessage,
                                                     ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }
        
        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var results = new List<RegistrationStatisticReport>();
            if (criteria.Type == "s")
            {
                var registrations = _db.RegistrationCourses.Where(x => x.TermId == criteria.TermId)
                                                           .Include(x => x.Student)
                                                           .Include(x => x.Section)
                                                           .Where(x => x.Status != "d")
                                                           .OrderBy(x => x.Student.Code)
                                                           .Select(x => new 
                                                                        {
                                                                            x.Student.Code,
                                                                            x.Student.FullNameEn,
                                                                            IsMasterSection = x.SectionId != null && x.Section.IsMasterSection,
                                                                            IsJoint = x.SectionId != null && x.Section.ParentSectionId != null,
                                                                            IsOther = x.SectionId != null && (x.Section.IsOutbound && x.Section.IsSpecialCase),
                                                                            x.Course.RegistrationCredit,
                                                                            x.Course.Credit
                                                                        })
                                                           .OrderBy(x => x.Code)
                                                           .ToList();
                
                foreach (var item in registrations.GroupBy(x => new { x.Code, x.FullNameEn }))
                {
                    results.Add(new RegistrationStatisticReport
                                {
                                    StudentCode = item.Key.Code,
                                    StudentFullNameEn = item.Key.FullNameEn,
                                    Master = item.Count(y => y.IsMasterSection),
                                    Joint = item.Count(y => y.IsJoint),
                                    Other = item.Count(y => y.IsOther),
                                    RegistrationCredit = item.Sum(y => y.RegistrationCredit),
                                    AcademicCredit = item.Sum(y => y.Credit)
                                });
                }
            }
            else
            {
                var sections = _db.Sections.Where(x => x.TermId == criteria.TermId)
                                           .Select(x => new 
                                                        {
                                                            x.Id,
                                                            x.Course.CourseAndCredit,
                                                            x.IsMasterSection,
                                                            IsJoint = x.ParentSectionId == null ? false : true,
                                                            IsOther = (x.IsOutbound && x.IsSpecialCase) ? true : false
                                                        })
                                           .ToList();

                foreach (var item in sections.GroupBy(x => x.CourseAndCredit))
                {
                    results.Add(new RegistrationStatisticReport
                                {
                                    Course = item.Key,
                                    Master = item.Count(y => y.IsMasterSection),
                                    Joint = item.Count(y => y.IsJoint),
                                    Other = item.Count(y => y.IsOther),
                                    TotalRegistration = _db.RegistrationCourses.Count(y => y.Status != "d" && y.SectionId == item.First().Id)
                                });
                }
            }

            var model = new RegistrationStatisticReportViewModel
                        {
                            Criteria = criteria,
                            RegistrationStatisticReportDetails = results
                        };

            return View(model);
        }

        private void CreateSelectList(long academicLevelId) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Types = _selectListProvider.GetRegistrationSummaryReportTypes();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}