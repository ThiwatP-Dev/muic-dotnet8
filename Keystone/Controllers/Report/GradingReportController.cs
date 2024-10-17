
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
    [PermissionAuthorize("GradingReport", "")]
    public class GradingReportController : BaseController
    {
        protected readonly IGradeProvider _gradeProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        public GradingReportController(ApplicationDbContext db,
                                       ISelectListProvider selectListProvider,
                                       IFlashMessage flashMessage,
                                       IGradeProvider gradeProvider,
                                       IRegistrationProvider registrationProvider) : base(db, flashMessage, selectListProvider)
        { 
            _gradeProvider = gradeProvider;
            _registrationProvider = registrationProvider;
        }
        
        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.FacultyId);
            if(criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var model = _db.RegistrationCourses.AsNoTracking()
                                               .Include(x => x.Student)
                                                   .ThenInclude(x => x.Title)
                                               .Include(x => x.Student)
                                                   .ThenInclude(x => x.AcademicInformation)
                                                   .ThenInclude(x => x.Department)
                                               .Include(x => x.Term)
                                               .Include(x => x.Course)
                                               .Include(x => x.Section)
                                                   .ThenInclude(x => x.MainInstructor)
                                                   .ThenInclude(x => x.Title)
                                               .Include(x => x.Grade)
                                               .Where(x => x.Status != "d" 
                                                           && criteria.AcademicLevelId == x.Term.AcademicLevelId
                                                           && criteria.TermId == x.TermId
                                                           && (criteria.CourseId == 0
                                                               || criteria.CourseId == x.CourseId)
                                                           && (criteria.FacultyId == 0
                                                               || criteria.FacultyId == x.Student.AcademicInformation.FacultyId)
                                                           && (criteria.DepartmentId == 0
                                                               || criteria.DepartmentId == x.Student.AcademicInformation.DepartmentId)
                                                           && (criteria.GradeIds == null 
                                                               || !criteria.GradeIds.Any()
                                                               || criteria.GradeIds.Contains(x.GradeId ?? 0)))
                                                .OrderBy(x => x.Course.Code)
                                                   .ThenBy(x => x.Section.Number)
                                                   .ThenBy(x => x.Student.Code)
                                               .GetPaged(criteria, page, true);
            return View(model);
        }

        public ActionResult Details(long id)
        {    
            var gradingLog = _gradeProvider.GetGradingLogsByRegistrationCourseId(id);
            var model = new GradingLogViewModel();

            if (!gradingLog.Any())
            {
                var student = _registrationProvider.GetRegistrationCourseById(id);
                model.StudentName = student.Student.FullNameEn;
                model.StudentCode = student.Student.Code;
                model.Course = student.Course.CourseAndCredit;
                model.Section = student.Section?.Number;
            }
            else
            {
                var Details = new List<GradingLogDetail>();
                foreach (var item in gradingLog)
                {
                    var log = new GradingLogDetail
                              {
                                  PreviousGrade = item.PreviousGrade,
                                  CurrentGrade = item.CurrentGrade,
                                  ApprovedAt = item.UpdatedAtText,
                                  ApprovedBy = item.UpdatedBy
                              };

                    Details.Add(log);
                }

                model.StudentCode = gradingLog.Select(x => x.RegistrationCourse.Student.Code).FirstOrDefault();
                model.StudentName = gradingLog.Select(x => x.RegistrationCourse.Student.FullNameEn).FirstOrDefault();
                model.Course = gradingLog.Select(x => x.RegistrationCourse.Course.CourseAndCredit).FirstOrDefault();
                model.Section = gradingLog.Select(x => x.RegistrationCourse.Section?.Number).FirstOrDefault();
                model.Details = Details;
            }

            return PartialView("_DetailsInfo", model);
        }  

        private void CreateSelectList(long academicLevelId = 0, long termId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Grades = _selectListProvider.GetGrades();
            ViewBag.Faculties = _selectListProvider.GetFaculties();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                if (facultyId != 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }

            if (termId != 0)
            {
                ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);
            }
        } 
    }
}