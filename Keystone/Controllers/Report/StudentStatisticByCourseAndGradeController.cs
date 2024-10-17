using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.Report;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("StudentStatisticByCourseAndGrade", "")]
    public class StudentStatisticByCourseAndGradeController : BaseController
    {
        public StudentStatisticByCourseAndGradeController(ApplicationDbContext db,
                                                          ISelectListProvider selectListProvider,
                                                          IFlashMessage flashMessage) : base(db, flashMessage, selectListProvider){ }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }


            var model = new StudentStatisticByCourseAndGradeViewModel();
            var registrationCourses = _db.RegistrationCourses.Include(x => x.Course)
                                                             .Include(x => x.Student)
                                                             .Include(x => x.Grade)
                                                             .Where(x => x.Term.AcademicLevelId == criteria.AcademicLevelId
                                                                         && x.TermId == criteria.TermId
                                                                         && (criteria.FacultyId == 0
                                                                             || x.Course.FacultyId == criteria.FacultyId)
                                                                         && (criteria.DepartmentId == 0
                                                                             || x.Course.DepartmentId == criteria.DepartmentId)
                                                                         && (criteria.CourseIds == null
                                                                             || !criteria.CourseIds.Any()
                                                                             || criteria.CourseIds.Contains(x.CourseId)))
                                                             .ToList();
                                                 
            var gradeHeader = registrationCourses.Select(x => x.GradeName)
                                                 .Distinct()
                                                 .OrderBy(x => x)
                                                 .ToList();

            model.Grades = registrationCourses.GroupBy(x => x.Grade)
                                              .Select(x => new StudentStatisticByGrade
                                                           {
                                                                GradeName = x.FirstOrDefault().GradeName,
                                                                StudentCount = x.Select(y => y.Student)
                                                                                .Count()
                                                           })
                                              .ToList();

            model.Courses = registrationCourses.GroupBy(x => x.Course)
                                               .Select(x => new StudentStatisticByCourse
                                                            {
                                                                CourseName = x.FirstOrDefault().Course.CourseAndCredit,
                                                                Grades = x.GroupBy(y => y.Grade)
                                                                          .Select(y => new StudentStatisticByGrade
                                                                                       {
                                                                                           GradeName = y.FirstOrDefault().GradeName,
                                                                                           StudentCount = y.Select(z => z.Student)
                                                                                                           .Distinct()
                                                                                                           .Count()
                                                                                       })
                                                                          .ToList()
                                                            })
                                               .ToList();
                                   
            model.Criteria = criteria;
            model.GradeHeader = gradeHeader;
            return View(model);
        }

        public void CreateSelectList(long academicLevelId = 0, long termId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);
        }
    }
}