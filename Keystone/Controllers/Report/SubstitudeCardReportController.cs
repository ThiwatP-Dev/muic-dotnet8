using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("SubstitudeCardReport", "")]
    public class SubstitudeCardReportController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IMasterProvider _masterProvider;

        public SubstitudeCardReportController(ApplicationDbContext db,
                                              IFlashMessage flashMessage,
                                              ISelectListProvider selectListProvider,
                                              IDateTimeProvider dateTimeProvider,
                                              IMasterProvider masterProvider) : base(db, flashMessage, selectListProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _masterProvider = masterProvider;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var examinationTypeName = _masterProvider.FindExaminationType(criteria.ExaminationTypeId)?.NameEn ?? "";
            DateTime? startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);

            var model = _db.CardLogs.Where(x => x.CardType == "substitudecard")
                                    .Select(x => new
                                                 {
                                                     CardInfo = JsonConvert.DeserializeObject<StudentIdCardDetail>(x.Log),
                                                     PrintDate = x.PrintedAtText,
                                                     PrintBy = x.PrintedBy
                                                 })
                                    .Where(x => (criteria.FacultyId == 0
                                                 || x.CardInfo.FacultyId == criteria.FacultyId)
                                                && (criteria.DepartmentId == 0
                                                    || x.CardInfo.DepartmentId == criteria.DepartmentId)
                                                && (criteria.ExaminationTypeId == 0
                                                    || x.CardInfo.ExaminationTypeName == examinationTypeName)
                                                && (startedAt == null
                                                    || string.IsNullOrEmpty(x.CardInfo.ExaminationDate.Split()[1])
                                                    || _dateTimeProvider.ConvertStringToDateTime(x.CardInfo.ExaminationDate.Split()[1]).Value.Date >= startedAt.Value.Date)
                                                && (endedAt == null
                                                    || string.IsNullOrEmpty(x.CardInfo.ExaminationDate.Split()[1])
                                                    || _dateTimeProvider.ConvertStringToDateTime(x.CardInfo.ExaminationDate.Split()[1]).Value.Date <= endedAt.Value.Date)
                                                && (criteria.CourseId == 0
                                                    || x.CardInfo.CourseId == criteria.CourseId))
                                    .Select(x => new SubstitudeCardReportViewModel
                                                 {
                                                     ExamType = x.CardInfo.ExaminationTypeName,
                                                     ExamDate = x.CardInfo.ExaminationDate,
                                                     ExamTime = x.CardInfo.ExaminationTime,
                                                     Division = x.CardInfo.FacultyName,
                                                     Subject = x.CardInfo.CourseName,
                                                     Section = x.CardInfo.SectionNumber,
                                                     Instructor = x.CardInfo.InstructorName,
                                                     StudentCode = x.CardInfo.Code,
                                                     Program = x.CardInfo.DepartmentName,
                                                     StudentName = $"{ x.CardInfo.FirstName } { x.CardInfo.LastName }",
                                                     PrintDate = x.PrintDate,
                                                     PrintBy = x.PrintBy
                                                 })
                                    .GetPaged(criteria, page, true);
            return View(model);
        }

        public void CreateSelectList(long academicLevelId = 0, long termId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.ExaminationTypes = _selectListProvider.GetExaminationTypes();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                if (facultyId != 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }

            if (termId != 0)
            {
                ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);
            }
        }
    }
}