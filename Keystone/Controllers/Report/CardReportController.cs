using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class CardReportController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IStudentProvider _studentProvider;

        public CardReportController(ApplicationDbContext db, 
                                    IFlashMessage flashMessage,
                                    IDateTimeProvider dateTimeProvider,
                                    IStudentProvider studentProvider,
                                    ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) 
        { 
            _dateTimeProvider = dateTimeProvider;
            _studentProvider = studentProvider;
        }

        public IActionResult Index( Criteria criteria, int page = 1)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            DateTime? start = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? end = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);
            var models = _db.Students.Include(x => x.AcademicInformation)
                                         .ThenInclude(x => x.Faculty)
                                     .Include(x => x.AcademicInformation)
                                         .ThenInclude(x => x.Department)
                                     .Include(x => x.AdmissionInformation)
                                     .Where(x => x.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                 && (criteria.FacultyId == 0
                                                     || x.AcademicInformation.FacultyId == criteria.FacultyId)
                                                 && (criteria.DepartmentId == 0
                                                     || x.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                 && (criteria.AdmissionRoundId == 0 
                                                     || x.AdmissionInformation.AdmissionRoundId == criteria.AdmissionRoundId)
                                                 && (string.IsNullOrEmpty(criteria.StartedAt)
                                                     || x.IdCardCreatedDate >= start)
                                                 && (string.IsNullOrEmpty(criteria.EndedAt)
                                                     || x.IdCardCreatedDate <= end)
                                                 && (criteria.StartStudentBatch == null
                                                     || x.AcademicInformation.Batch == criteria.StartStudentBatch)
                                                 && (string.IsNullOrEmpty(criteria.CardStatus)
                                                     || (Convert.ToBoolean(criteria.CardStatus) ? x.IdCardCreatedDate != null
                                                                                                : x.IdCardCreatedDate == null))
                                                 && (string.IsNullOrEmpty(criteria.ImageStatus)
                                                     || (Convert.ToBoolean(criteria.ImageStatus) ? !string.IsNullOrEmpty(x.ProfileImageURL)
                                                                                                 : string.IsNullOrEmpty(x.ProfileImageURL)))
                                                 && string.IsNullOrEmpty(criteria.Status)
                                                    || criteria.Status == x.StudentStatus)
                                     .OrderBy(x => x.Code)
                                     .AsQueryable()
                                     .GetPaged(criteria, page, true);
                                     
            return View(models);
        }

        public IActionResult Details(Guid id)
        {
            return RedirectToAction(nameof(Details), "Student", new { id = id, tabIndex = "0" });
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.AllYesNoAnswer = _selectListProvider.GetAllYesNoAnswer();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            if (academicLevelId != 0)
            {
                ViewBag.AdmissionRounds = _selectListProvider.GetAdmissionRoundByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                if (facultyId != 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }
        }
    }
}