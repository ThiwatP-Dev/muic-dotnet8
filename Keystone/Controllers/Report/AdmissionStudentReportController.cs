using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    public class AdmissionStudentReportController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IMasterProvider _masterProvider;

        public AdmissionStudentReportController(ApplicationDbContext db,
                                                IFlashMessage flashMessage,
                                                ISelectListProvider selectListProvider,
                                                IDateTimeProvider dateTimeProvider,
                                                IMasterProvider masterProvider) : base(db, flashMessage, selectListProvider) 
        { 
            _dateTimeProvider = dateTimeProvider;
            _masterProvider = masterProvider;
        }

        public ActionResult Index(int page, Criteria criteria) 
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.DepartmentId, criteria.CurriculumId);
            ViewBag.ExemptedExamination = _masterProvider.GetExemptedAdmissionExaminations();
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            DateTime? startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);

            var students = _db.AdmissionInformations.Include(x => x.CurriculumVersion)
                                                    .Include(x => x.Faculty)
                                                    .Include(x => x.Department)
                                                    .Include(x => x.Student)
                                                        .ThenInclude(x => x.AcademicInformation)
                                                        .ThenInclude(x => x.AcademicLevel)
                                                .Include(x => x.Student)
                                                        .ThenInclude(x => x.StudentExemptedExamScores)
                                                    .Include(x => x.AdmissionRound)
                                                    .Where(x => criteria.AcademicLevelId == x.Student.AdmissionInformation.AcademicLevelId
                                                                && (criteria.AdmissionRoundId == 0
                                                                    || criteria.AdmissionRoundId == x.AdmissionRoundId)
                                                                && (criteria.FacultyId == 0
                                                                    || criteria.FacultyId == x.FacultyId)
                                                                && (criteria.DepartmentId == 0
                                                                    || criteria.DepartmentId == x.DepartmentId)
                                                                && (criteria.CurriculumVersionId == 0
                                                                    || criteria.CurriculumVersionId == x.CurriculumVersionId)
                                                                && (String.IsNullOrEmpty(criteria.Status)
                                                                    || criteria.Status == x.Student.StudentStatus)
                                                                && (startedAt == null
                                                                    || (x.AppliedAt != null
                                                                        && x.AppliedAt.Value.Date >= startedAt))
                                                                && (endedAt == null
                                                                    || (x.AppliedAt != null
                                                                        && x.AppliedAt.Value.Date <= endedAt))
                                                                && x.Student.StudentStatus == "a")
                                                    .OrderBy(x => x.Student.Code)
                                                    .ToList();
                                                    
            var studentPageResult = students.AsQueryable()
                                            .GetPaged(criteria, page, true);

            return View(studentPageResult);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long department = 0, long curriculumId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Statuses = _selectListProvider.GetStudentStatuses();

            if (academicLevelId != 0)
            {
                ViewBag.AdmissionRounds = _selectListProvider.GetAdmissionRoundByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);

                if (facultyId != 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);

                    if (department != 0)
                    {
                        ViewBag.Curriculums = _selectListProvider.GetCurriculumByDepartment(academicLevelId, facultyId, department);

                        if (curriculumId != 0)
                        {
                            ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
                        }
                    }
                }
            }
        }
    }
}