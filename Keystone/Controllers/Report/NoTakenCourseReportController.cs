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
    [PermissionAuthorize("NoTakenCourseReport", "")]
    public class NoTakenCourseReportController : BaseController
    {

        public NoTakenCourseReportController(ApplicationDbContext db,
                                             IFlashMessage flashMessage,
                                             ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public ActionResult Index(int page, Criteria criteria) 
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.CurriculumId);
            if (criteria.AcademicLevelId == 0 || criteria.CourseId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var studentCourses = _db.RegistrationCourses.AsNoTracking()
                                                        .Where(x => x.Student.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId 
                                                                    && x.CourseId == criteria.CourseId
                                                                    && (x.Status != "d")
                                                                    && (criteria.CurriculumId == 0
                                                                        || x.Student.AcademicInformation.CurriculumVersion.CurriculumId == criteria.CurriculumId)
                                                                    && (criteria.CurriculumVersionId == 0
                                                                        || x.Student.AcademicInformation.CurriculumVersionId == criteria.CurriculumVersionId))
                                                        .Select(x => new 
                                                                     {
                                                                         x.StudentId,
                                                                         GradeWeight = x.Grade.Weight
                                                                     });

            var curriculumnCourses = (from academicInfo in _db.AcademicInformations.Include(x => x.CurriculumVersion)
                                                                                       .ThenInclude(x => x.Curriculum)
                                      join student in _db.Students.Include(x => x.Title)
                                                                  .Include(x => x.StudentFeeType)
                                                                  .Include(x => x.ResidentType)
                                                                  .Include(x => x.Nationality)
                                                                  .Include(x => x.BirthCountry)
                                      on academicInfo.StudentId equals student.Id
                                      join courseGroup in _db.CourseGroups on academicInfo.CurriculumVersionId equals courseGroup.CurriculumVersionId
                                      join curriculumnCourse in _db.CurriculumCourses.Include(x => x.Grade) on courseGroup.Id equals curriculumnCourse.CourseGroupId

                                      let concentrations = _db.CurriculumInformations.Include(x => x.SpecializationGroupInformations)
                                                                                     .Where(x => x.StudentId == student.Id
                                                                                                 && x.IsActive
                                                                                                 && x.SpecializationGroupInformations.Any(y => y.SpecializationGroupId == criteria.ConcentrationId))

                                      where academicInfo.AcademicLevel.Id == criteria.AcademicLevelId
                                            && curriculumnCourse.CourseId == criteria.CourseId
                                            && (criteria.CurriculumId == 0 || academicInfo.CurriculumVersion.CurriculumId == criteria.CurriculumId)
                                            && (criteria.CurriculumVersionId == 0 || academicInfo.CurriculumVersionId == criteria.CurriculumVersionId)
                                            && (criteria.IsTaken == null || (Convert.ToBoolean(criteria.IsTaken) ? studentCourses.Any(x => x.StudentId == academicInfo.StudentId
                                                                                                                                           && x.GradeWeight >= curriculumnCourse.Grade.Weight)
                                                                                                                  : !studentCourses.Any(x => x.StudentId == academicInfo.StudentId
                                                                                                                                             && x.GradeWeight >= curriculumnCourse.Grade.Weight)))
                                            && (criteria.StartStudentBatch == null || academicInfo.Batch >= criteria.StartStudentBatch)
                                            && (criteria.EndStudentBatch == null || academicInfo.Batch <= criteria.EndStudentBatch)
                                            && (string.IsNullOrEmpty(criteria.StudentStatus) || student.StudentStatus == criteria.StudentStatus)
                                            && (criteria.StudentTypeId == 0 || student.StudentFeeTypeId == criteria.StudentTypeId)
                                            && (criteria.CreditFrom == null || academicInfo.CreditEarned >= criteria.CreditFrom)
                                            && (criteria.CreditTo == null || academicInfo.CreditEarned <= criteria.CreditTo)
                                            && (criteria.ConcentrationId == 0 || (concentrations != null && concentrations.Any()))
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
                                          CurriculumnCode = academicInfo.CurriculumVersion.Curriculum.ReferenceCode,
                                          CurriculumnName = academicInfo.CurriculumVersion.Curriculum.NameEn,
                                          CurriculumnVerisonName = academicInfo.CurriculumVersion.NameEn,
                                          Nationality = student.Nationality.NameEn,
                                          CountryName = student.BirthCountry.NameEn
                                      }).ToList().GroupBy(x => x.StudentCode).Select(x => x.FirstOrDefault()).ToList().GetPaged(criteria, page, true);
                                     
            return View(curriculumnCourses);
        }

        private void CreateSelectList(long academicLevelId = 0, long curriculumnId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();
            ViewBag.Concentrations = _selectListProvider.GetConcentrations();
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