using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class TransactionFinalGradeReportController : BaseController
    {
        protected readonly IAcademicProvider _academicProvider;
        
        public TransactionFinalGradeReportController(ApplicationDbContext db,
                                                     ISelectListProvider selectListProvider,
                                                     IFlashMessage flashMessage,
                                                     IAcademicProvider academicProvider) : base(db, flashMessage, selectListProvider) 
        {
            _academicProvider = academicProvider;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId);
            if (criteria.AcademicLevelId == 0 || string.IsNullOrEmpty(criteria.Type))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var startTerm = _academicProvider.GetTerm(criteria.StartTermId);
            var endTerm = _academicProvider.GetTerm(criteria.EndTermId);
            var gradingLog = _db.GradingLogs.Include(x => x.RegistrationCourse)
                                                .ThenInclude(x => x.Term)
                                            .Include(x => x.RegistrationCourse)
                                                .ThenInclude(x => x.Course)
                                            .Include(x => x.RegistrationCourse)
                                                .ThenInclude(x => x.Section)
                                            .Include(x => x.RegistrationCourse)
                                                .ThenInclude(x => x.Student)
                                            .Where(x => x.Type == "m"
                                                        && x.RegistrationCourse.Term.AcademicLevelId == criteria.AcademicLevelId
                                                        && (startTerm == null
                                                            || endTerm == null
                                                            || ((x.RegistrationCourse.Term.AcademicYear == startTerm.AcademicYear
                                                                && x.RegistrationCourse.Term.AcademicTerm >= startTerm.AcademicTerm)
                                                                || x.RegistrationCourse.Term.AcademicYear > startTerm.AcademicYear)
                                                              && ((x.RegistrationCourse.Term.AcademicYear == endTerm.AcademicYear
                                                                   && x.RegistrationCourse.Term.AcademicTerm <= endTerm.AcademicTerm)
                                                                   || x.RegistrationCourse.Term.AcademicYear < endTerm.AcademicYear))
                                                        && (criteria.FacultyId == 0
                                                            || x.RegistrationCourse.Student.AcademicInformation.FacultyId == criteria.FacultyId)
                                                        && (criteria.DepartmentId == 0
                                                            || x.RegistrationCourse.Student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                        && (criteria.CurriculumId == 0
                                                            || x.RegistrationCourse.Student.AcademicInformation.CurriculumVersion.CurriculumId == criteria.CurriculumId)
                                                        && (criteria.CourseId == 0
                                                            || x.RegistrationCourse.CourseId == criteria.CourseId)
                                                        && (criteria.MaintenanceType == "all"
                                                            || (criteria.MaintenanceType == "change"
                                                                && !string.IsNullOrEmpty(x.PreviousGrade)
                                                                && !string.IsNullOrEmpty(x.CurrentGrade))
                                                            || (criteria.MaintenanceType == "add"
                                                                && string.IsNullOrEmpty(x.PreviousGrade)
                                                                && !string.IsNullOrEmpty(x.CurrentGrade))
                                                            || (criteria.MaintenanceType == "delete"
                                                                && !string.IsNullOrEmpty(x.PreviousGrade)
                                                                && string.IsNullOrEmpty(x.CurrentGrade)))
                                                        && (criteria.StudentCodeFrom == null
                                                            || (criteria.StudentCodeTo == null ? x.RegistrationCourse.Student.CodeInt == criteria.StudentCodeFrom : x.RegistrationCourse.Student.CodeInt >= criteria.StudentCodeFrom))
                                                        && (criteria.StudentCodeTo == null
                                                            || (criteria.StudentCodeFrom == null ? x.RegistrationCourse.Student.CodeInt == criteria.StudentCodeTo : x.RegistrationCourse.Student.CodeInt <= criteria.StudentCodeTo))
                                                        && (criteria.MonthFrom == null
                                                            || x.UpdatedAt.Month >= criteria.MonthFrom)
                                                        && (criteria.MonthTo == null
                                                            || x.UpdatedAt.Month <= criteria.MonthTo)
                                                        && (criteria.UpdatedFrom == null
                                                            || x.UpdatedAt.Date >= criteria.UpdatedFrom.Value.Date)
                                                        && (criteria.UpdatedTo == null
                                                            || x.UpdatedAt.Date <= criteria.UpdatedTo.Value.Date))
                                            .Select(x => new TransactionFinalGradeReportViewModel
                                                     {
                                                         StudentCode = x.RegistrationCourse.Student.Code,
                                                         Term = x.RegistrationCourse.Term.TermText,
                                                         Course = x.RegistrationCourse.Course.Code,
                                                         Section = x.RegistrationCourse.Section.Number,
                                                         PreviousGrade = x.PreviousGrade,
                                                         CurrentGrade = x.CurrentGrade,
                                                         UpdatedAt = x.UpdatedAtText,
                                                         UpdatedMonthAt = x.UpdatedAt.ToString(StringFormat.Month),
                                                         Remark = x.Remark
                                                     })
                                            .OrderBy(x => x.Term)
                                                .ThenBy(x => x.StudentCode)
                                                .ThenBy(x => x.Course)
                                                .ThenBy(x => x.UpdatedAt)
                                            .ToList();
            
            if (criteria.Type == "d")
            {
                criteria.UpdatedFromText = criteria.UpdatedFrom?.ToString(StringFormat.ShortDate);
                criteria.UpdatedToText = criteria.UpdatedTo?.ToString(StringFormat.ShortDate);
            }
            else if (criteria.Type == "m")
            {
                criteria.UpdatedFromText = _selectListProvider.GetMonth().SingleOrDefault(x => x.Value == (criteria.MonthFrom ?? 0).ToString())?.Text ?? "";
                criteria.UpdatedToText = _selectListProvider.GetMonth().SingleOrDefault(x => x.Value == (criteria.MonthTo ?? 0).ToString())?.Text ?? "";
            }
            else if (criteria.Type == "t")
            {
                criteria.UpdatedFromText = startTerm == null ? gradingLog.FirstOrDefault().Term : startTerm?.TermText;
                criteria.UpdatedToText = endTerm == null ? gradingLog.LastOrDefault().Term : endTerm?.TermText;
            }

            var transactionPageResult = gradingLog.AsQueryable()
                                                  .GetPaged(criteria, page, true);
            return View(transactionPageResult);
        }

        public void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Curriculums = _selectListProvider.GetCurriculum();
            ViewBag.MaintenanceTypes = _selectListProvider.GetMaintenanceTypes();
            ViewBag.ReportTypes = _selectListProvider.GetTransactionFinalGradeTypes();
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.Month = _selectListProvider.GetMonth();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);

                if (facultyId > 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }
        }
    }
}