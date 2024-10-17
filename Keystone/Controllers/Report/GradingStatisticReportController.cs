using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("GradingStatisticReport", "")]
    public class GradingStatisticReportController : BaseController
    {
        protected readonly ICalculationProvider _calculationProvider;
        protected readonly IGradeProvider _gradeProvider;

        public GradingStatisticReportController(ApplicationDbContext db,
                                                ISelectListProvider selectListProvider,
                                                IFlashMessage flashMessage,
                                                ICalculationProvider calculationProvider,
                                                IGradeProvider gradeProvider) : base(db, flashMessage, selectListProvider) 
        {
            _calculationProvider = calculationProvider;
            _gradeProvider = gradeProvider;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            var model = new GradingStatisticReportViewModel();
            model.Criteria = criteria;
            model.TotalRecord = 0;

            if (criteria.AcademicLevelId == 0 
                || criteria.TermId == 0 
                || criteria.CurriculumVersionId == 0 
                || string.IsNullOrEmpty(criteria.Type))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            var registrationCourses = _db.RegistrationCourses.AsNoTracking()
                                                             .Include(x => x.Course)
                                                                 .ThenInclude(x => x.Faculty)
                                                             .Include(x => x.Course)
                                                                 .ThenInclude(x => x.Department)
                                                             .Include(x => x.Grade)
                                                             .Where(x => x.TermId == criteria.TermId
                                                                         && x.Status != "d"
                                                                         && (x.Student.AcademicInformation.CurriculumVersionId == criteria.CurriculumVersionId)
                                                                         && (criteria.AdmissionTypeIds == null 
                                                                             || criteria.AdmissionTypeIds.Contains(x.Student.AdmissionInformation.AdmissionTypeId.Value)))
                                                             .ToList();

            var grades = _db.Grades.AsNoTracking()
                                   .Select(x => x.Name)
                                   .OrderBy(x => x)
                                   .ToList();

            var totalRegistrationCourses = registrationCourses.Count();
            model.GradeHeaders = grades;
            
            var studentCount = registrationCourses.GroupBy(x => x.CourseId)
                                                  .Select(x => new GradingStatisticByCourseViewModel
                                                              {
                                                                  Code = x.FirstOrDefault().Course?.Code,
                                                                  Name = x.FirstOrDefault().Course?.NameEn,
                                                                  Credit = x.FirstOrDefault().Course?.CreditText,
                                                                  TotalStudentRegister = x.Count(),
                                                                  TotalStudentPass = x.Count(y => y.Grade?.Weight > 0),
                                                                  Grades = x.GroupBy(y => y.GradeName)
                                                                            .Select(y => new GradingStatisticReportCountViewModel
                                                                                         {
                                                                                            Grade = y.Key,
                                                                                            StudentCount = y.Count(),
                                                                                            Percentage = _calculationProvider.GetPercentage(y.Count(),
                                                                                                                                            x.Count())
                                                                                         })
                                                                            .ToList()
                                                              })
                                                  .OrderBy(x => x.Code)
                                                  .ToList();

            model.Footer = registrationCourses.GroupBy(x => x.TermId)
                                              .Select(x => new GradingStatisticByCourseViewModel
                                                           {
                                                               TotalStudentRegister = x.Count(),
                                                               TotalStudentPass = x.Count(y => y.Grade?.Weight > 0),
                                                               Grades = x.GroupBy(y => y.GradeName)  
                                                                         .Select(y => new GradingStatisticReportCountViewModel
                                                                                      {
                                                                                          Grade = y.Key,
                                                                                          StudentCount = y.Count(),
                                                                                          Percentage = _calculationProvider.GetPercentage(y.Count(),
                                                                                                                                          x.Count())
                                                                                      }) 
                                                                         .ToList()
                                                           })
                                              .FirstOrDefault();
                                                

            model.GradeByCourses = studentCount;
            model.TotalRecord = studentCount.Count;
            return View(model);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            // ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.ReportTypes = _selectListProvider.GetGradingStatisticReport();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersions();
            ViewBag.AdmissionTypes = _selectListProvider.GetAdmissionTypes();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);

                if (facultyId != 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }
        }
    }
}