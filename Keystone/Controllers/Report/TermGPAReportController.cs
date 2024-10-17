using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using KeystoneLibrary.Models.Report;
using Keystone.Permission;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("TermGPAReport", "")]
    public class TermGPAReportController : BaseController
    {
         public TermGPAReportController(ApplicationDbContext db,
                                        IFlashMessage flashMessage,
                                        ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            var term = _db.Terms.SingleOrDefault(x => x.Id == criteria.TermId);
            var courses = (from registrationCourse in _db.RegistrationCourses
                            join grade in _db.Grades on registrationCourse.GradeId equals grade.Id
                            join course in _db.Courses on registrationCourse.CourseId equals course.Id
                            where registrationCourse.TermId == criteria.TermId
                            && (registrationCourse.Status == "a" || registrationCourse.Status == "r")
                            && course.IsCalculateCredit
                            && grade.IsCalculateGPA
                            group new { registrationCourse, grade, course } by registrationCourse.StudentId into registrationCourseTmp
                            select new 
                            {
                                StudentId = registrationCourseTmp.Key,
                                GPA = registrationCourseTmp.Sum(y => y.course.Credit) == 0
                                    ? 0 : registrationCourseTmp.Sum(y => y.course.Credit * (y.grade.Weight ?? 0)) / registrationCourseTmp.Sum(y => y.course.Credit)
                            }).ToList();

            var model = (from course in courses
                         join student in _db.Students on course.StudentId equals student.Id
                         join title in _db.Titles on student.TitleId equals title.Id
                         join studentFeeType in _db.StudentFeeTypes on student.StudentFeeTypeId equals studentFeeType.Id
                         join residentType in _db.ResidentTypes on student.ResidentTypeId equals residentType.Id
                         join academicInformation in _db.AcademicInformations on student.Id equals academicInformation.StudentId
                         join department in _db.Departments on academicInformation.DepartmentId equals department.Id
                         where (criteria.FacultyId == 0 || student.AcademicInformation.FacultyId == criteria.FacultyId)
                               && (criteria.DepartmentId == 0 || student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                               && (string.IsNullOrEmpty(criteria.StudentStatus) || student.StudentStatus == criteria.StudentStatus)
                               && (criteria.StudentTypeId == 0 || student.StudentFeeTypeId == criteria.StudentTypeId)
                               && (criteria.ResidentTypeId == 0 || student.ResidentTypeId == criteria.ResidentTypeId)
                               && (criteria.StartStudentBatch == null
                                   || student.AcademicInformation.Batch >= criteria.StartStudentBatch)
                               && (criteria.EndStudentBatch == null
                                   || student.AcademicInformation.Batch <= criteria.EndStudentBatch)
                         select new TermGPAReportViewModel
                                {
                                    StudentCode = student.Code,
                                    Title = title.NameEn,
                                    FirstName = student.FirstNameEn,
                                    MidName = student.MidNameEn,
                                    LastName = student.LastNameEn,
                                    DepartmentCode = department.Code,
                                    GPA = course.GPA,
                                    CummulativeGPA = academicInformation.GPA,
                                    TotalCreditEarned = academicInformation.CreditComp,
                                    StudentTypeName = studentFeeType.NameEn,
                                    ResidentTypeName = residentType.NameEn,
                                    StudentStatus = student.StudentStatusText,
                                    Detail = term.TermText
                                }).OrderBy(x => x.StudentCode).AsQueryable()
                         .GetPaged(criteria, page, true);

            return View(model);
        }

        public void CreateSelectList(long academicLevelId = 0, long termId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();
            ViewBag.ResidentTypes = _selectListProvider.GetResidentTypes();
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