using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Keystone.Controllers
{
    [PermissionAuthorize("TotalWithdrawalReport", "")]
    public class TotalWithdrawalReportController : BaseController
    {
        public TotalWithdrawalReportController(ApplicationDbContext db,
                                               ISelectListProvider selectListProvider) : base(db, selectListProvider) { }

        public ActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId);
            if (criteria.AcademicLevelId > 0 && criteria.TermId > 0)
            {
                var students = from regisCourse in _db.RegistrationCourses.AsNoTracking().IgnoreQueryFilters()
                                                                          .Include(x => x.Course)
                                                                          .Include(x => x.Section)
                                                                              .ThenInclude(x => x.MainInstructor)
                                                                              .ThenInclude(x => x.Title)
                               where regisCourse.TermId == criteria.TermId
                                     && regisCourse.IsActive
                                     && regisCourse.Status != "d"
                                     && !regisCourse.IsTransferCourse
                                     && !(regisCourse.SectionId == 0 || regisCourse.SectionId == null)
                                     && (criteria.CourseIds == null || criteria.CourseIds.Count == 0 || criteria.CourseIds.Contains(regisCourse.CourseId))
                               group regisCourse by new
                               {
                                   regisCourse.CourseId,
                                   regisCourse.Course,
                                   regisCourse.Section.Number,
                                   regisCourse.Section.MainInstructor
                               } into regisCourses
                               select new TotalWithdrawalReportViewModel
                               {
                                   CourseCode = regisCourses.Key.Course.Code,
                                   CourseNameEn = regisCourses.Key.Course.NameEn,
                                   CourseNameTh = regisCourses.Key.Course.NameTh,
                                   CourseCredit = regisCourses.Key.Course.CreditText,
                                   SectionNumber = regisCourses.Key.Number,
                                   Instructor = regisCourses.Key.MainInstructor == null ? "" : regisCourses.Key.MainInstructor.FullNameEn,
                                   EnrollmentStudent = regisCourses.Count(),
                                   //WithdrawalStudent = regisCourses.SelectMany(x => x.Withdrawals).Count(x => x.Status == "a"),
                               }                              
                               ;

                students = students.OrderBy(x => x.CourseCode).ThenBy(x => x.SectionNumber);

                var totalEnroll = ViewBag.TotalEnrollmentStudent = _db.RegistrationCourses.Count(x => x.TermId == criteria.TermId
                                     && x.IsActive
                                     && x.Status != "d"
                                     && (criteria.CourseIds == null || criteria.CourseIds.Count == 0 || criteria.CourseIds.Contains(x.CourseId)));

                var qWithdrawal = (from withdrawal in _db.Withdrawals.AsNoTracking().IgnoreQueryFilters()
                                   join regisCourse in _db.RegistrationCourses.AsNoTracking().IgnoreQueryFilters() on withdrawal.RegistrationCourseId equals regisCourse.Id
                                   where withdrawal.Status == "a"
                                        && regisCourse.TermId == criteria.TermId
                                        && regisCourse.IsActive
                                        && regisCourse.Status != "d"
                                        && !regisCourse.IsTransferCourse
                                        && !(regisCourse.SectionId == 0 || regisCourse.SectionId == null)
                                        && (criteria.CourseIds == null || criteria.CourseIds.Count == 0 || criteria.CourseIds.Contains(regisCourse.CourseId))
                                   group withdrawal by new
                                   {
                                       regisCourse.CourseId,
                                       regisCourse.Course.Code,
                                       regisCourse.Course.NameEn,
                                       regisCourse.Course.NameTh,
                                       regisCourse.Section.Number,
                                       //regisCourse.Section.MainInstructor
                                   } into regisCourses
                                   select new TotalWithdrawalReportViewModel
                                   {
                                       CourseCode = regisCourses.Key.Code,
                                       CourseNameEn = regisCourses.Key.NameEn,
                                       CourseNameTh = regisCourses.Key.NameTh,
                                       SectionNumber = regisCourses.Key.Number,
                                       WithdrawalStudent = regisCourses.Count(),
                                   }).ToList();

                var totalWithdraw = ViewBag.TotalWithdrawalStudent = qWithdrawal.Sum(x => x.WithdrawalStudent);
                ViewBag.TotalLeftOverStudent = totalEnroll - totalWithdraw;

                var model = students.GetPaged(criteria, page, true);
                foreach (var item in model.Results)
                {
                    var withdrawalCount = qWithdrawal.FirstOrDefault(x => x.CourseCode == item.CourseCode 
                                                                          && x.SectionNumber == item.SectionNumber);
                    if (withdrawalCount != null)
                    {
                        item.WithdrawalStudent = withdrawalCount.WithdrawalStudent;
                    }
                }
                return View(model);
            }
            
            return View();
        }

        public void CreateSelectList(long academicLevelId, long termId)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId); 
            }

            if (termId != 0)
            {
                ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);
            }
        }
    }
}