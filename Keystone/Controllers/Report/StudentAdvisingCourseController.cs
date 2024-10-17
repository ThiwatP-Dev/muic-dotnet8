using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    public class StudentAdvisingCourseController : BaseController
    {
        public StudentAdvisingCourseController(ApplicationDbContext db,
                                               IFlashMessage flashMessage,
                                               ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }
        
        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var results = _db.AdvisingCourses.Include(x => x.Course)
                                             .Include(x => x.Student)
                                                  .ThenInclude(x => x.AcademicInformation)
                                             .Where(x => x.TermId == criteria.TermId
                                                         && (criteria.InstructorId == 0 || x.InstructorId == criteria.InstructorId)
                                                         && (criteria.FacultyId == 0 || x.Student.AcademicInformation.FacultyId == criteria.FacultyId)
                                                         && (criteria.DepartmentId == 0 || x.Student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                         && (criteria.CourseId == 0 || x.CourseId == criteria.CourseId))
                                             .GroupBy(x => x.CourseId)
                                             .OrderBy(x => x.FirstOrDefault().Course.Code)
                                             .Select(x => new StudentAdvisingCourse
                                             {
                                                 CourseCode = x.FirstOrDefault().Course.Code,
                                                 CourseName = x.FirstOrDefault().Course.NameEnAndCredit,
                                                 Count = x.Select(y => y.StudentId).Distinct().Count()
                                             }).ToList();
            
            var model = new StudentAdvisingCourseViewModel
                        {
                            Criteria = criteria,
                            Results = results
                        };

            return View(model);
        }

        private void CreateSelectList(long academicLevelId, long facultyId)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.Advisors = _selectListProvider.GetInstructors();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);

                if (facultyId > 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }
        }
    }
}