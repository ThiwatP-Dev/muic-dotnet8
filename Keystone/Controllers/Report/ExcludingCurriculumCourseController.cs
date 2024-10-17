using Microsoft.AspNetCore.Mvc;
using KeystoneLibrary.Models;
using KeystoneLibrary.Data;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.Report;
using Keystone.Permission;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("ExcludingCurriculumCourse", "")]
    public class ExcludingCurriculumCourseController : BaseController
    {
        public ExcludingCurriculumCourseController(ApplicationDbContext db,
                                                   IFlashMessage flashMessage,
                                                   ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.DepartmentId, criteria.CurriculumId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var curriculumnCourses = from academicInfo in _db.AcademicInformations
                                     join courseGroup in _db.CourseGroups on academicInfo.CurriculumVersionId equals courseGroup.CurriculumVersionId
                                     join curriculumnCourse in _db.CurriculumCourses on courseGroup.Id equals curriculumnCourse.CourseGroupId
                                     where (academicInfo.AcademicLevel.Id == criteria.AcademicLevelId)
                                     select new 
                                            { 
                                                academicInfo.StudentId, 
                                                curriculumnCourse.CourseId 
                                            };
            
            var studentCources = (from registrationCourse in _db.RegistrationCourses.Include(x => x.Course)
                                                                                    .Include(x => x.Term)
                                  join student in _db.Students.Include(x => x.StudentFeeType)
                                                              .Include(x => x.ResidentType) 
                                  on registrationCourse.StudentId equals student.Id
                                  join academicInfo in _db.AcademicInformations.Include(x => x.Faculty)
                                                                               .Include(x => x.Department)
                                                                               .Include(x => x.Advisor)
                                                                                   .ThenInclude(x => x.Title)
                                                                               .Include(x => x.AcademicLevel)
                                                                               .Include(x => x.CurriculumVersion)
                                  on student.Id equals academicInfo.StudentId
                                  where !curriculumnCourses.Any(x => x.StudentId == registrationCourse.StudentId
                                                                     && x.CourseId == registrationCourse.CourseId)
                                        && (academicInfo.AcademicLevel.Id == criteria.AcademicLevelId)
                                        && (registrationCourse.TermId == criteria.TermId)
                                        && (criteria.FacultyId == 0 || academicInfo.FacultyId == criteria.FacultyId)
                                        && (criteria.DepartmentId == 0 || academicInfo.DepartmentId == criteria.DepartmentId)
                                        && (criteria.CurriculumId == 0 || academicInfo.CurriculumVersion.CurriculumId == criteria.CurriculumId)
                                        && (criteria.CurriculumVersionId == 0 || academicInfo.CurriculumVersionId == criteria.CurriculumVersionId)
                                        && (criteria.StudentTypeId == 0 || student.StudentFeeTypeId == criteria.StudentTypeId)
                                        && (criteria.ResidentTypeId == 0 || student.ResidentTypeId == criteria.ResidentTypeId)
                                        && (string.IsNullOrEmpty(criteria.Status) || student.StudentStatus == criteria.Status)
                                        && (criteria.StartStudentBatch == null
                                            || academicInfo.Batch >= criteria.StartStudentBatch)
                                        && (criteria.EndStudentBatch == null
                                            || academicInfo.Batch <= criteria.EndStudentBatch)
                                        && (string.IsNullOrEmpty(criteria.Code)
                                            || student.Code.Contains(criteria.Code))
                                  select new ExcludingCurriculumnCourseViewModel
                                         {
                                             Major = academicInfo.Department.NameEn,
                                             StudentCode = student.Code,
                                             StudentName = student.FullNameEn,
                                             StudentType = student.StudentFeeType.NameEn,
                                             StudentResidentType = student.ResidentType.NameEn,
                                             AdvisorName = academicInfo.Advisor.FullNameEn,
                                             StudentStatusText = student.StudentStatusText,
                                             BlockedStatus = string.Empty,
                                             CourseCode = registrationCourse.Course.Code,
                                             CourseName = registrationCourse.Course.NameEn,
                                             CreditText = registrationCourse.Course.CreditText,
                                             Grade = registrationCourse.GradeName,
                                             TermText = registrationCourse.Term.TermText
                                         }).GetPaged(criteria, page, true);

            return View(studentCources);
        }

        public void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long departmentId = 0, long curriculumId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();
            ViewBag.ResidentTypes = _selectListProvider.GetResidentTypes();

            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);

                if (facultyId > 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);

                    if (departmentId > 0)
                    {
                        ViewBag.Curriculums = _selectListProvider.GetCurriculumByDepartment(academicLevelId, facultyId, departmentId);
                    }
                }
            }

            if (curriculumId > 0)
            {
                ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
            }
        }
    }
}