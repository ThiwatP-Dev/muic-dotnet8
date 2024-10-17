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
    [PermissionAuthorize("StudentGraduationReport", "")]
    public class StudentGraduationReportController : BaseController
    {
        public StudentGraduationReportController(ApplicationDbContext db,
                                                 IFlashMessage flashMessage,
                                                 ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.CurriculumId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var results = _db.GraduationInformations.AsNoTracking()
                                                    .Include(x => x.Student)
                                                        .ThenInclude(x => x.StudentAddresses)
                                                    .Where(x => x.Student.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                                && x.TermId == criteria.TermId
                                                                && (criteria.FacultyId == 0
                                                                    || x.Student.AcademicInformation.FacultyId == criteria.FacultyId)
                                                                && (criteria.DepartmentId == 0
                                                                    || x.Student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                                && (criteria.CurriculumId == 0
                                                                    || x.Student.CurriculumInformations.Any(y => y.CurriculumVersion != null 
                                                                                                         && y.CurriculumVersion.CurriculumId == criteria.CurriculumId))
                                                                && (criteria.CurriculumVersionId == 0
                                                                    || x.Student.CurriculumInformations.Any(y => y.CurriculumVersionId == criteria.CurriculumVersionId))
                                                                && (criteria.HonorId == 0
                                                                    || x.Student.GraduationInformations.Any(y => y.HonorId == criteria.HonorId))
                                                                && ((criteria.StartStudentBatch ?? 0) == 0
                                                                    || x.Student.AcademicInformation.Batch >= criteria.StartStudentBatch)
                                                                && ((criteria.EndStudentBatch ?? 0) == 0
                                                                    || x.Student.AcademicInformation.Batch <= criteria.EndStudentBatch)
                                                                && (criteria.StudentFeeTypeId == 0
                                                                    || x.Student.StudentFeeTypeId == criteria.StudentFeeTypeId)
                                                                && (criteria.ResidentTypeId == 0
                                                                    || x.Student.ResidentTypeId == criteria.ResidentTypeId)
                                                                && ((criteria.Gender ?? 0) == 0
                                                                    || x.Student.Gender == criteria.Gender)
                                                                && ((criteria.GPAFrom ?? 0) == 0
                                                                    || x.Student.AcademicInformation.GPA >= criteria.GPAFrom)
                                                                && ((criteria.GPATo ?? 0) == 0
                                                                    || x.Student.AcademicInformation.GPA <= criteria.GPATo)
                                                                && (string.IsNullOrEmpty(criteria.Code)
                                                                    || x.Student.Code.StartsWith(criteria.Code))
                                                                && (string.IsNullOrEmpty(criteria.FirstName)
                                                                    || x.Student.FirstNameEn.StartsWith(criteria.FirstName))
                                                                && (string.IsNullOrEmpty(criteria.StudentStatus)
                                                                    || x.Student.StudentStatus == criteria.StudentStatus)
                                                                && (string.IsNullOrEmpty(criteria.Status) 
                                                                     || x.IsActive == Convert.ToBoolean(criteria.Status)))
                                                    .Select(x => new StudentInformationViewModel
                                                                 {
                                                                    StudentCode = x.Student.Code,
                                                                    TitleEn = x.Student.Title.NameEn,
                                                                    TitleTh = x.Student.Title.NameTh,
                                                                    FirstNameEn = x.Student.FirstNameEn,
                                                                    FirstNameTh = x.Student.FirstNameTh,
                                                                    MidNameEn = x.Student.MidNameEn,
                                                                    MidNameTh = x.Student.MidNameTh,
                                                                    LastNameEn = x.Student.LastNameEn,
                                                                    LastNameTh = x.Student.LastNameTh,
                                                                    StudentFeeTypeEn = x.Student.StudentFeeType.NameEn,
                                                                    ResidentTypeEn = x.Student.ResidentType.NameEn,
                                                                    StudentStatus = x.Student.StudentStatus,
                                                                    GraduatedAt = x.GraduatedAt,
                                                                    AdmissionTermText = x.Student.AdmissionInformation.AdmissionTerm != null ?  x.Student.AdmissionInformation.AdmissionTerm.AcademicTerm + "/" + x.Student.AdmissionInformation.AdmissionTerm.AcademicYear : "",
                                                                    DepartmentCode = x.Student.AcademicInformation.Department.Code,
                                                                    GPA = x.Student.AcademicInformation.GPA,
                                                                    CreditComp = x.Student.AcademicInformation.CreditComp,
                                                                    TelephoneNumber = x.Student.TelephoneNumber1,
                                                                    StudentAddresses = x.Student.StudentAddresses
                                                                 })
                                                    .OrderBy(x => x.StudentCode)
                                                    .ToList();

            var model = new StudentGraduationReportViewModel
                        {
                            Criteria = criteria,
                            Results = results
                        };

            return View(model);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long curriculumId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Honors = _selectListProvider.GetAcademicHonors();
            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();
            ViewBag.ResidentTypes = _selectListProvider.GetResidentTypes();
            ViewBag.Genders = _selectListProvider.GetGender();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.Statuses = _selectListProvider.GetActiveStatuses();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);

                if (facultyId > 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }

                if (curriculumId > 0)
                {
                    ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
                }
            }
        }
    }
}