using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("GraduatingStudentReport", "")]
    public class GraduatingStudentReportController : BaseController
    {
        protected readonly ICacheProvider _cacheProvider;
        public GraduatingStudentReportController(ApplicationDbContext db,
                                                 ISelectListProvider selectListProvider,
                                                 IFlashMessage flashMessage,
                                                 ICacheProvider cacheProvider) : base(db, flashMessage, selectListProvider)
        {
            _cacheProvider = cacheProvider;
        }

        public ActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.DepartmentId, criteria.CurriculumId);
            
            if (criteria.AcademicLevelId == 0 || criteria.StartStudentBatch == null || criteria.EndStudentBatch == null || criteria.Credit == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var currentTermId = _cacheProvider.GetCurrentTerm(criteria.AcademicLevelId)?.Id ?? 0;
            var studentQuery = (from student in _db.Students
                                join academic in _db.AcademicInformations on student.Id equals academic.StudentId
                                join graduation in _db.GraduationInformations on student.Id equals graduation.StudentId into graduations
                                from graduation in graduations.DefaultIfEmpty()
                                join faculty in _db.Faculties on academic.FacultyId equals faculty.Id
                                join department in _db.Departments on academic.DepartmentId equals department.Id
                                join version in _db.CurriculumVersions on academic.CurriculumVersionId equals version.Id
                                join curriculum in _db.Curriculums on version.CurriculumId equals curriculum.Id
                                join registration in _db.RegistrationCourses.Include(x => x.Course)
                                                                            .Where(x => x.TermId == currentTermId)
                                                  on student.Id equals registration.StudentId into registrations
                                from registration in registrations.DefaultIfEmpty()
                                join course in _db.Courses on registration.CourseId equals course.Id
                                where academic.AcademicLevelId == criteria.AcademicLevelId
                                      && student.StudentStatus == "s"
                                      && (graduation == null 
                                          || graduation.GraduatedAt == null)
                                      && criteria.StartStudentBatch <= academic.Batch
                                      && criteria.EndStudentBatch >= academic.Batch
                                      && (criteria.FacultyId == 0
                                          || academic.FacultyId == criteria.FacultyId)
                                      && (criteria.DepartmentId == 0
                                          || academic.DepartmentId == criteria.DepartmentId)
                                      && (criteria.CurriculumId == 0
                                          || version.CurriculumId == criteria.CurriculumId)
                                      && (criteria.CurriculumVersionId == 0
                                          || academic.CurriculumVersionId == criteria.CurriculumVersionId)
                                group new { student, academic, faculty, department, version, curriculum, registration, course }
                                by student.Id into x
                                select new GraduatingStudentReportViewModel
                                       {
                                           StudentId = x.Key,
                                           StudentCode = x.First().student.Code,
                                           StudentFullName = x.First().student.FullNameEn,
                                           Faculty = x.First().faculty.Abbreviation,
                                           Department = x.First().department.Abbreviation,
                                           Curriculum = x.First().curriculum.NameEn,
                                           CurriculumVersion = x.First().version.NameEn,
                                           TotalCreditCurriculum = x.First().version.TotalCredit,
                                           CreditEarned = x.First().academic.CreditEarned ?? 0,
                                           ExpectedCredit = criteria.Credit ?? 0,
                                           RegistrationCourses = x.Select(y => y.registration).ToList(),
                                           TotalRegistrationCredit = x.Sum(y => y.course.Credit)
                                       });

            var studentPageResult = studentQuery.Where(x => x.TotalCreditEarnWithExpected >= x.TotalCreditCurriculum)
                                                .OrderBy(x => x.StudentCode)
                                                .GetPaged(criteria, page, true);

            return View(studentPageResult);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long departmentId = 0, long curriculumId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            ViewBag.Curriculums = _selectListProvider.GetCurriculumByDepartment(academicLevelId, facultyId, departmentId);
            ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
        }
    }
}
