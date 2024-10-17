using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("RepeatStudentReport", "")]
    public class RepeatStudentReportController : BaseController
    {
        public RepeatStudentReportController(ApplicationDbContext db,
                                              IFlashMessage flashMessage,
                                              ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.CurriculumId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                criteria.Status = "s";
                criteria.AcademicLevelId = _db.AcademicLevels.SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;

                return View(new PagedResult<RepeatStudentReprotViewModel>()
                            {
                                Criteria = criteria
                            });
            }

            var students = _db.RegistrationCourses.AsNoTracking()
                                                  .Where(x => x.Status != "d"
                                                              //&& x.GradeId != null
                                                              &&(Convert.ToBoolean(criteria.IsGradeNotPass) ? (x.GradeId == null || x.Grade.Weight == 0 )
                                                                                                            : true)
                                                              && (string.IsNullOrEmpty(criteria.Code) 
                                                              || x.Student.Code.Contains(criteria.Code))
                                                              && (string.IsNullOrEmpty(criteria.FirstName)
                                                                  || x.Student.FirstNameEn.StartsWith(criteria.FirstName)
                                                                  || x.Student.FirstNameTh.StartsWith(criteria.FirstName))
                                                              && (string.IsNullOrEmpty(criteria.LastName)
                                                                  || x.Student.LastNameEn.StartsWith(criteria.LastName)
                                                                  || x.Student.LastNameTh.StartsWith(criteria.LastName))
                                                              && (criteria.NationalityId == 0 
                                                                  || x.Student.NationalityId == criteria.NationalityId)
                                                              && (criteria.AcademicLevelId == 0 
                                                                  || x.Student.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId)
                                                              && (criteria.FacultyId == 0 
                                                                  || x.Student.AcademicInformation.FacultyId == criteria.FacultyId)
                                                              && (criteria.DepartmentId == 0 
                                                                  || x.Student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                              && (string.IsNullOrEmpty(criteria.Status) 
                                                                  || x.Student.StudentStatus == criteria.Status)
                                                              && (criteria.StartStudentBatch == null 
                                                                  || x.Student.AcademicInformation.Batch >= criteria.StartStudentBatch)
                                                              && (criteria.EndStudentBatch == null 
                                                                  || x.Student.AcademicInformation.Batch <= criteria.EndStudentBatch))
                                                  .Select(x => new RepeatStudentDetailReprotViewModel
                                                               {
                                                                   Id = x.StudentId,
                                                                   Code = x.Student.Code,
                                                                   Title = x.Student.Title.NameEn,
                                                                   StudentFirstName = x.Student.FirstNameEn,
                                                                   StudentLastName = x.Student.LastNameEn,
                                                                   StudentMidName = x.Student.MidNameEn,                                                                   
                                                                   Department = x.Student.AcademicInformation.Department.Code,
                                                                   CourseId = x.Course.Id,
                                                                   CourseCode = x.Course.Code,
                                                                   CourseName = x.Course.NameEn,
                                                                   CourseRateId = x.Course.CourseRateId,
                                                                   Lab = x.Course.Lab,
                                                                   Other = x.Course.Other,
                                                                   Lecture = x.Course.Lecture,
                                                                   Credit = x.Course.Credit,
                                                                   TermId = x.TermId,
                                                                   AcademicTerm = x.Term.AcademicTerm,
                                                                   AcademicYear = x.Term.AcademicYear,
                                                                   GradeId = x.GradeId ?? 0,
                                                                   GradeName = x.GradeName,
                                                                   AdvisorTitleEn = x.Student.AcademicInformation.Advisor.Title.NameEn,
                                                                   AdvisorFirstNameEn = x.Student.AcademicInformation.Advisor.FirstNameEn,
                                                                   AdvisorLastNameEn = x.Student.AcademicInformation.Advisor.LastNameEn,
                                                                   GradePassing = x.Course.GradeTemplateId == 1 ? "D" : "S"
                                                               })
                                                   .GroupBy(x => new { x.Id, x.CourseId })
                                                   .Where(x => x.Count() > 1)
                                                   .ToList()
                                                   .Select(x => new RepeatStudentReprotViewModel
                                                                {
                                                                   StudentCode = x.FirstOrDefault().Code,
                                                                   FullName = x.FirstOrDefault().FullName,
                                                                   Title = x.FirstOrDefault().Title,
                                                                   StudentFirstName = x.FirstOrDefault().StudentFirstName,
                                                                   StudentLastName = x.FirstOrDefault().StudentLastName,
                                                                   StudentMidName = x.FirstOrDefault().StudentMidName,
                                                                   AdvisorTitleEn = x.FirstOrDefault().AdvisorTitleEn,
                                                                   AdvisorFirstNameEn = x.FirstOrDefault().AdvisorFirstNameEn,
                                                                   AdvisorLastNameEn = x.FirstOrDefault().AdvisorLastNameEn,
                                                                   Department = x.FirstOrDefault().Department,
                                                                   CourseAndCredit = x.FirstOrDefault().CourseAndCredit,
                                                                   AdvisorFullName = x.FirstOrDefault().AdvisorFullName,
                                                                   GradePassing = x.FirstOrDefault().GradePassing,
                                                                   GradeNameAndTerms = x.OrderBy(y => y.AcademicYear)
                                                                                            .ThenBy(y => y.AcademicTerm)
                                                                                        .Select(y => y.GradeName + "(" + y.TermText + ")")
                                                                                        .ToList()
                                                                })
                                                   .OrderBy(x => x.StudentCode)
                                                   .ToList();

            return View(students.GetAllPaged(criteria));
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long curriculumnId = 0)
        {
            ViewBag.Nationalities = _selectListProvider.GetNationalities();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Abilities = _selectListProvider.GetAbilities();
            ViewBag.Statuses = _selectListProvider.GetStudentStatuses();
            if (academicLevelId > 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);
                if (curriculumnId > 0)
                {
                    ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumnId);
                }
                
                if (facultyId > 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }
        }
    }
}