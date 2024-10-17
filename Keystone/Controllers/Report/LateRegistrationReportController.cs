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
    [PermissionAuthorize("LateRegistrationReport", "")]
    public class LateRegistrationReportController : BaseController
    {
        public LateRegistrationReportController(ApplicationDbContext db,
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

            var registrations = (from registration in _db.RegistrationCourses
                                 join term in _db.Terms on registration.TermId equals term.Id
                                 join course in _db.Courses on registration.CourseId equals course.Id
                                 join student in _db.Students on registration.StudentId equals student.Id
                                 join sTitle in _db.Titles on student.TitleId equals sTitle.Id
                                 join academicInfo in _db.AcademicInformations on student.Id equals academicInfo.StudentId
                                 join department in _db.Departments on academicInfo.DepartmentId equals department.Id into departments
                                 from department in departments.DefaultIfEmpty()
                                 join faculty in _db.Faculties on academicInfo.FacultyId equals faculty.Id into faculties
                                 from faculty in faculties.DefaultIfEmpty()
                                 join instructor in _db.Instructors on academicInfo.AdvisorId equals instructor.Id into instructors
                                 from instructor in instructors.DefaultIfEmpty()
                                 join aTitle in _db.Titles on instructor.TitleId equals aTitle.Id into aTitles
                                 from aTitle in aTitles.DefaultIfEmpty()
                                 let registrationAt = (from log in _db.RegistrationLogs
                                                       where log.StudentId == registration.StudentId
                                                             && log.TermId == registration.TermId
                                                       orderby log.CreatedAt
                                                       select log.CreatedAt).FirstOrDefault()
                                 where registration.TermId == criteria.TermId
                                       && registration.Status != "d"
                                       && registration.IsTransferCourse == false
                                       && registrationAt != null 
                                       && term.FirstRegistrationEndedAt != null 
                                       && term.FirstRegistrationEndedAt < registrationAt
                                       && (criteria.FacultyId == 0 || academicInfo.FacultyId == criteria.FacultyId)
                                       && (criteria.DepartmentId == 0 || academicInfo.DepartmentId == criteria.DepartmentId)
                                       && ((criteria.StartStudentBatch ?? 0) == 0 || academicInfo.Batch >= criteria.StartStudentBatch)
                                       && ((criteria.EndStudentBatch ?? 0) == 0 || academicInfo.Batch <= criteria.EndStudentBatch)
                                       && (string.IsNullOrEmpty(criteria.StudentCode) || student.Code.StartsWith(criteria.StudentCode))
                                       && (string.IsNullOrEmpty(criteria.StudentStatus) || student.StudentStatus == criteria.StudentStatus)
                                 select new 
                                        {
                                            term.AcademicYear,
                                            term.AcademicTerm,
                                            student.Code,
                                            TitleNameEn = sTitle.NameEn,
                                            student.FirstNameEn,
                                            student.MidNameEn,
                                            student.LastNameEn,
                                            student.StudentStatus,
                                            DepartmentCode = department.Code,
                                            FacultyCode = faculty.Code,
                                            FacultyNameEn = faculty.NameEn,
                                            AdvisorTitleNameEn = aTitle.NameEn,
                                            AdvisorFirstNameEn = instructor.FirstNameEn,
                                            AdvisorLastNameEn = instructor.LastNameEn,
                                            course.RegistrationCredit,
                                            registration.IsPaid
                                        }).AsNoTracking()
                                   .ToList();
            
            var results = registrations.GroupBy(x => new 
                                                     {
                                                         x.AcademicYear,
                                                         x.AcademicTerm,
                                                         x.Code,
                                                         x.TitleNameEn,
                                                         x.FirstNameEn,
                                                         x.MidNameEn,
                                                         x.LastNameEn,
                                                         x.StudentStatus,
                                                         x.DepartmentCode,
                                                         x.FacultyCode,
                                                         x.FacultyNameEn,
                                                         x.AdvisorTitleNameEn,
                                                         x.AdvisorFirstNameEn,
                                                         x.AdvisorLastNameEn
                                                     })
                                       .Where(x => ((criteria.Credit ?? 0) == 0 || x.Sum(y => y.RegistrationCredit) == criteria.Credit)
                                                   && (string.IsNullOrEmpty(criteria.IsPaidAdmissionFee) || x.All(y => y.IsPaid) == Convert.ToBoolean(criteria.IsPaidAdmissionFee)))
                                       .Select(x => new LateRegistrationReportViewModel
                                                    {
                                                        AcademicYear = x.Key.AcademicYear,
                                                        AcademicTerm = x.Key.AcademicTerm,
                                                        Code = x.Key.Code,
                                                        TitleNameEn = x.Key.TitleNameEn,
                                                        FirstNameEn = x.Key.FirstNameEn,
                                                        MidNameEn = x.Key.MidNameEn,
                                                        LastNameEn = x.Key.LastNameEn,
                                                        StudentStatus = x.Key.StudentStatus,
                                                        DepartmentCode = x.Key.DepartmentCode,
                                                        FacultyCode = x.Key.FacultyCode,
                                                        FacultyNameEn = x.Key.FacultyNameEn,
                                                        AdvisorTitleNameEn = x.Key.AdvisorTitleNameEn,
                                                        AdvisorFirstNameEn = x.Key.AdvisorFirstNameEn,
                                                        AdvisorLastNameEn = x.Key.AdvisorLastNameEn,
                                                        Credit = x.Sum(y => y.RegistrationCredit),
                                                        IsPaid = x.All(y => y.IsPaid)
                                                    })
                                       .OrderBy(x => x.TermText).ThenBy(x => x.Code)
                                       .GetAllPaged(criteria);
            return View(results);
        }

        private void CreateSelectList(long academicLevelId, long facultyId)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }
            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartments(facultyId);
            }

            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses().Where(x => x.Value == "s"
                                                                                          || x.Value == "ex"
                                                                                          || x.Value == "la");

            ViewBag.PaidStatuses = _selectListProvider.GetPaidStatuses();
        }
    }
}