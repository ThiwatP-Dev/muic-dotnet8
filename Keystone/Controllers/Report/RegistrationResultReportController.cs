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
    [PermissionAuthorize("RegistrationResultReport", "")]
    public class RegistrationResultReportController : BaseController
    {
        public RegistrationResultReportController(ApplicationDbContext db,
                                                  IFlashMessage flashMessage,
                                                  ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) {}

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.TermId, criteria.CourseId);
            var model = new RegistrationResultReportViewModel();
            model.Criteria = criteria;

            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var result = (from student in _db.Students.IgnoreQueryFilters()
                                                      .Include(x => x.AcademicInformation)
                                                          .ThenInclude(x => x.Department)
                                                              .ThenInclude(x => x.Faculty)
                          join instructor in _db.Instructors.IgnoreQueryFilters() on student.AcademicInformation.AdvisorId equals instructor.Id into instructors
                          from instructor in instructors.DefaultIfEmpty()
                          join aTitle in _db.Titles.IgnoreQueryFilters() on instructor.TitleId equals aTitle.Id into aTitles
                          from aTitle in aTitles.DefaultIfEmpty()
                          join registedCourse in (from registration in _db.RegistrationCourses.IgnoreQueryFilters()
                                                  join section in _db.Sections.IgnoreQueryFilters() on registration.SectionId equals section.Id
                                                  join course in _db.Courses.IgnoreQueryFilters() on registration.CourseId equals course.Id
                                                  where registration.TermId == criteria.TermId
                                                        && (registration.Status == "a"
                                                            || registration.Status == "r")
                                                        && (criteria.CourseId == 0
                                                            || registration.CourseId == criteria.CourseId)
                                                        && (criteria.SectionId == 0
                                                            || registration.SectionId == criteria.SectionId)
                                                  select new
                                                         {
                                                             registration.StudentId,
                                                             CourseCode = course.Code,
                                                             RegistrationCredit = course.RegistrationCredit,
                                                             AcademicCredit = course.Credit,
                                                             SectionNumber = section.Number,
                                                             registration.IsPaid
                                                         })
                          on student.Id equals registedCourse.StudentId into registedCourses
                          where 
                          //( string.IsNullOrEmpty(criteria.StudentStatus) ? student.StudentStatus == "s" 
                          //                                                         || student.StudentStatus == "ex"
                          //                                                         || student.StudentStatus == "la"
                          //                                                     : student.StudentStatus == criteria.StudentStatus )
                          //      && 
                                (string.IsNullOrEmpty(criteria.Code)
                                 || student.Code.StartsWith(criteria.Code))
                                && (criteria.FacultyId == 0
                                    || student.AcademicInformation.FacultyId == criteria.FacultyId)
                                && (criteria.DepartmentId == 0
                                    || student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                && (criteria.StartStudentBatch == null
                                    || student.AcademicInformation.Batch >= criteria.StartStudentBatch)
                                && (criteria.EndStudentBatch == null
                                    || student.AcademicInformation.Batch <= criteria.EndStudentBatch)
                                && (criteria.StudentStatuses == null || !criteria.StudentStatuses.Any()
                                    || criteria.StudentStatuses.Contains(student.StudentStatus) )
                                && (criteria.StudentFeeTypeId == 0
                                    || student.StudentFeeTypeId == criteria.StudentFeeTypeId )
                                && (criteria.StudentCodeFrom == null
                                    || student.CodeInt >= criteria.StudentCodeFrom)
                                && (criteria.StudentCodeTo == null
                                    || student.CodeInt <= criteria.StudentCodeTo)
                                && (criteria.ResidentTypeId == 0
                                   || student.ResidentTypeId == criteria.ResidentTypeId)
                                && (criteria.AdmissionTypeId == 0
                                   || student.AdmissionInformation.AdmissionTypeId == criteria.AdmissionTypeId)
                                && (criteria.StudentFeeGroupId == 0
                                   || student.StudentFeeGroupId == criteria.StudentFeeGroupId)
                          orderby student.Code
                          select new RegistrationResultReport
                                 {
                                     StudentCode = student.Code,
                                     StudentTitle = student.Title.NameEn,
                                     StudentName = student.FullNameEnNoTitle,
                                     StudentStatus = student.StudentStatusText,
                                     AdvisorTitle = aTitle.NameEn,
                                     AdvisorName = instructor.FullNameEn,
                                     Faculty = student.AcademicInformation.Faculty.ShortNameEn,
                                     Department = student.AcademicInformation.Department.ShortNameEn,
                                     RegisteredCourses = registedCourses == null ? new List<RegisteredCourse>()
                                                                                 : registedCourses.Select(x => new RegisteredCourse
                                                                                                               {
                                                                                                                   CourseAndSection = $"{ x.CourseCode }({ x.SectionNumber })",
                                                                                                                   IsPaid = x.IsPaid,
                                                                                                                   RegistrationCredit = x.RegistrationCredit,
                                                                                                                   AcademicCredit = x.RegistrationCredit
                                                                                                               }).ToList()
                                 }).ToList();
            
            if (criteria.IsFinishedRegistration)
            {
                model.RegistrationResultReports = result.Where(x => x.RegisteredCourses.Any()).ToList();
                if (criteria.PaymentStatus == "p")
                {
                    model.RegistrationResultReports = model.RegistrationResultReports.Where(x => x.RegisteredCourses.Any(y => y.IsPaid))
                                                                                     .ToList();
                }
                else if (criteria.PaymentStatus == "u")
                {
                    model.RegistrationResultReports = model.RegistrationResultReports.Where(x => x.RegisteredCourses.Any(y => !y.IsPaid))
                                                                                     .ToList();
                }
            }
            else
            {
                model.RegistrationResultReports = result.Where(x => !x.RegisteredCourses.Any()).ToList();
            }

            if (criteria.CreditFrom != null && criteria.CreditFrom > 0)
            {
                model.RegistrationResultReports = model.RegistrationResultReports.Where(x => x.TotalCredit >= criteria.CreditFrom).ToList();
            }
            if (criteria.CreditTo != null && criteria.CreditTo > 0)
            {
                model.RegistrationResultReports = model.RegistrationResultReports.Where(x => x.TotalCredit <= criteria.CreditTo).ToList();
            }

            return View(model);
        }

        private void CreateSelectList(long academicLevelId, long facultyId, long termId, long courseId) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.PaymentStatuses = _selectListProvider.GetPaymentStatuses();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }
            
            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartments(facultyId);
            }

            if (termId > 0)
            {
                ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);
            }

            if (courseId > 0)
            {
                ViewBag.Sections = _selectListProvider.GetSections(termId, courseId);
            }

            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();
            ViewBag.ResidentTypes = _selectListProvider.GetResidentTypes();
            ViewBag.AdmissionTypes = _selectListProvider.GetAdmissionTypes();
            ViewBag.StudentFeeGroups = _selectListProvider.GetStudentFeeGroups();
        }
    }
}