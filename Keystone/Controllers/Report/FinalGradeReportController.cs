using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class FinalGradeReportController : BaseController
    {
        protected readonly IRegistrationProvider _registrationProvider;

        public FinalGradeReportController(ApplicationDbContext db,
                                          ISelectListProvider selectListProvider,
                                          IRegistrationProvider registrationProvider,
                                          IFlashMessage flashMessage) : base(db, flashMessage, selectListProvider) 
        {
            _registrationProvider = registrationProvider;
        }
        
        public IActionResult Index(int page, Criteria criteria, string returnUrl)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0 || String.IsNullOrEmpty(criteria.Code) || criteria.CourseNumberFrom == 0 || criteria.CourseNumberTo == 0)
            {
                 _flashMessage.Warning(Message.RequiredData);
                 return View();
            }

            var sectionIds = _registrationProvider.GetSectionIdsByCoursesRange(criteria.TermId, criteria.Code, 
                                                                               criteria.CourseNumberFrom ?? 0, criteria.CourseNumberTo ?? 0, 
                                                                               criteria.SectionFrom ?? 0, criteria.SectionTo ?? 0);

            var registrationCourses = _db.RegistrationCourses.Include(x => x.Student)
                                                             .Include(x => x.Term)
                                                             .Include(x => x.Section)
                                                             .Include(x => x.Course)
                                                             .Include(x => x.Grade)
                                                             .Include(x => x.Section)
                                                             .Where(x => sectionIds.Contains(x.Section.Id)
                                                                         && (x.Status == "a" || x.Status == "r"))
                                                             .GroupBy(x => new { x.Course.Code, x.Section.Number })
                                                             .Select(x => new FinalGradeDetail
                                                                          {
                                                                              Term = x.FirstOrDefault().Term.TermText,
                                                                              CourseCode = x.Key.Code,
                                                                              SectionNumber = x.Key.Number,
                                                                              Students = x.Select(y => new FinalGradeStudentDetail
                                                                                                       {
                                                                                                           Code = y.Student.Code,
                                                                                                           Grade = y.GradeName,
                                                                                                           FullName = y.Student.FullNameEn
                                                                                                       })
                                                                                          .OrderBy(y => y.Code)
                                                                                          .ToList()
                                                                          })
                                                             .OrderBy(x => x.CourseCode)
                                                                 .ThenBy(x => x.SectionNumber)
                                                             .ToList();

            var model = new FinalGradeViewModel
                        {
                            Criteria = criteria,
                            FinalGradeDetails = registrationCourses
                        };

            return View(model);
        }

        public void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}