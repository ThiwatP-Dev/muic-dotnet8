using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class GradeMaintenanceStatisticController : BaseController
    {
        public GradeMaintenanceStatisticController(ApplicationDbContext db,
                                                   ISelectListProvider selectListProvider,
                                                   IFlashMessage flashMessage) : base(db, flashMessage, selectListProvider) { }
        
        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0 || criteria.StartTermId == 0 || criteria.EndTermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                 return View();
            }

            var fromTerm = _db.Terms.SingleOrDefault(x => x.Id == criteria.StartTermId);
            var toTerm = _db.Terms.SingleOrDefault(x => x.Id == criteria.EndTermId);
            var grades = (from grade in _db.GradingLogs
                          join registration in _db.RegistrationCourses on grade.RegistrationCourseId equals registration.Id
                          join term in _db.Terms on registration.TermId equals term.Id
                          join course in _db.Courses on registration.CourseId equals course.Id
                          join academic in _db.AcademicInformations on registration.StudentId equals academic.StudentId
                          where grade.Type == "m"
                                && term.AcademicLevelId == criteria.AcademicLevelId
                                && term.AcademicYear >= fromTerm.AcademicYear
                                && term.AcademicTerm >= fromTerm.AcademicTerm
                                && term.AcademicYear <= toTerm.AcademicYear
                                && term.AcademicTerm <= toTerm.AcademicTerm
                                && (string.IsNullOrEmpty(criteria.MaintenanceType)
                                    || criteria.MaintenanceType == "all"
                                    || (criteria.MaintenanceType == "change"
                                        && !string.IsNullOrEmpty(grade.PreviousGrade)
                                        && !string.IsNullOrEmpty(grade.CurrentGrade))
                                    || (criteria.MaintenanceType == "add"
                                        && string.IsNullOrEmpty(grade.PreviousGrade)
                                        && !string.IsNullOrEmpty(grade.CurrentGrade))
                                    || (criteria.MaintenanceType == "delete"
                                        && !string.IsNullOrEmpty(grade.PreviousGrade)
                                        && string.IsNullOrEmpty(grade.CurrentGrade)))
                                && (criteria.StartStudentBatch == null
                                    || criteria.StartStudentBatch == 0
                                    || academic.Batch >= criteria.StartStudentBatch)
                                && (criteria.EndStudentBatch == null
                                    || criteria.EndStudentBatch == 0
                                    || academic.Batch <= criteria.EndStudentBatch)
                                && (criteria.FacultyId == 0
                                    || academic.FacultyId == criteria.FacultyId)
                                && (criteria.DepartmentId == 0
                                    || academic.DepartmentId == criteria.DepartmentId)
                          select new 
                          {
                              CourseCode = course.Code,
                              CourseName = course.NameEn,
                              term.TermText
                          }).ToList();

            var model = grades.GroupBy(x => x.CourseCode)
                              .Select(x => new GradeMaintenanceStatisticViewModel
                                           {
                                               CourseCode = x.Key,
                                               CourseName = x.FirstOrDefault().CourseName,
                                               Terms = x.Select(y => y.TermText)
                                                        .ToList()
                                           })
                              .OrderBy(x => x.CourseCode)
                              .AsQueryable()
                              .GetPaged(criteria, page, true);

            ViewBag.Header = _db.Terms.Where(x => x.AcademicLevelId == criteria.AcademicLevelId
                                                  && x.AcademicYear >= fromTerm.AcademicYear
                                                  && x.AcademicTerm >= fromTerm.AcademicTerm
                                                  && x.AcademicYear <= toTerm.AcademicYear
                                                  && x.AcademicTerm <= toTerm.AcademicTerm)
                                      .Select(x => x.TermText)
                                      .OrderBy(x => x)
                                      .ToList();

            return View(model);
        }

        public void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.MaintenanceTypes = _selectListProvider.GetMaintenanceTypes();
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