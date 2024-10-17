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
    [PermissionAuthorize("NoTakenCourseFromMinorReport", "")]
    public class NoTakenCourseFromMinorReportController : BaseController
    {

        public NoTakenCourseFromMinorReportController(ApplicationDbContext db,
                                                         IFlashMessage flashMessage,
                                                         ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public ActionResult Index(int page, Criteria criteria) 
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.CurriculumId);
            if (criteria.AcademicLevelId == 0 || criteria.CourseId == 0 || criteria.MinorId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var studentCourses = _db.RegistrationCourses.AsNoTracking()
                                                        .Where(x => x.Student.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId 
                                                                    && x.CourseId == criteria.CourseId
                                                                    && (x.Status == "a"
                                                                        || x.Status == "r")
                                                                    && (criteria.CurriculumId == 0
                                                                        || x.Student.AcademicInformation.CurriculumVersion.CurriculumId == criteria.CurriculumId)
                                                                    && (criteria.CurriculumVersionId == 0
                                                                        || x.Student.AcademicInformation.CurriculumVersionId == criteria.CurriculumVersionId))
                                                        .Select(x => new 
                                                                     {
                                                                         x.StudentId,
                                                                         GradeWeight = x.Grade.Weight
                                                                     });

            var curriculumnCourses = (from student in _db.Students.AsNoTracking()
                                      join curriculumnInfo in _db.CurriculumInformations.AsNoTracking() on student.Id equals curriculumnInfo.StudentId
                                      join specializationGroupInfo in _db.SpecializationGroupInformations.AsNoTracking() on curriculumnInfo.Id equals specializationGroupInfo.CurriculumInformationId
                                      join courseGroup in _db.CourseGroups on specializationGroupInfo.SpecializationGroupId equals courseGroup.SpecializationGroupId
                                      join curriculumnCourse in _db.CurriculumCourses.Include(x => x.Grade) on 
                                         new
                                         {
                                             CourseGroupId = courseGroup.Id,
                                             criteria.CourseId
                                         } equals new
                                         {
                                             curriculumnCourse.CourseGroupId,
                                             curriculumnCourse.CourseId
                                         }
                                      where (criteria.IsTaken == null || (Convert.ToBoolean(criteria.IsTaken) ? studentCourses.Any(x => x.StudentId == student.Id
                                                                                                                                           && x.GradeWeight >= curriculumnCourse.Grade.Weight)
                                                                                                                  : !studentCourses.Any(x => x.StudentId == student.Id
                                                                                                                                             && x.GradeWeight >= curriculumnCourse.Grade.Weight)))
                                            && (specializationGroupInfo.SpecializationGroupId == criteria.MinorId)                                            
                                            && (criteria.StartStudentBatch == null || student.AcademicInformation.Batch >= criteria.StartStudentBatch)
                                            && (criteria.EndStudentBatch == null || student.AcademicInformation.Batch <= criteria.EndStudentBatch)
                                            && (string.IsNullOrEmpty(criteria.StudentStatus) || student.StudentStatus == criteria.StudentStatus)
                                            && (criteria.StudentTypeId == 0 || student.StudentFeeTypeId == criteria.StudentTypeId)
                                            && (criteria.CreditFrom == null || student.AcademicInformation.CreditEarned >= criteria.CreditFrom)
                                            && (criteria.CreditTo == null || student.AcademicInformation.CreditEarned <= criteria.CreditTo)
                                      select new NoTakenCourseReportViewModel
                                      {
                                          StudentCode = student.Code,
                                          StudentTitle = student.Title.NameEn,
                                          StudentFirstNameEn = student.FirstNameEn,
                                          StudentMiddleNameEn = student.MidNameEn,
                                          StudentLastNameEn = student.LastNameEn,
                                          StudentType = student.StudentFeeType.NameEn,
                                          StudentResidentType = student.ResidentType.NameEn,
                                          StudentEmail = student.Email,
                                          StudentStatusText = student.StudentStatusText,
                                          StudentContact = student.TelephoneNumber1,
                                          CurriculumnCode = student.AcademicInformation.CurriculumVersion.Curriculum.ReferenceCode,
                                          CurriculumnName = student.AcademicInformation.CurriculumVersion.Curriculum.NameEn,
                                          CurriculumnVerisonName = student.AcademicInformation.CurriculumVersion.NameEn,
                                          Nationality = student.Nationality.NameEn,
                                          CountryName = student.BirthCountry.NameEn
                                      }).GetPaged(criteria, page, true);
                                     
            return View(curriculumnCourses);
        }

        private void CreateSelectList(long academicLevelId = 0, long curriculumnId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();
            ViewBag.Concentrations = _selectListProvider.GetConcentrations();
            ViewBag.Minors = _selectListProvider.GetMinors();
            ViewBag.Takens = _selectListProvider.GetYesNoAnswer();
            if (academicLevelId > 0)
            {
                ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);
                ViewBag.Courses = _selectListProvider.GetCoursesByAcademicLevelId(academicLevelId);
            }

            if (curriculumnId > 0)
            {
                ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumnId);
            }
        }
    }
}