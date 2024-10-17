using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Models.DataModels.Profile;
using Keystone.Permission;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("StudentStatusReport", "")]
    public class StudentStatusReportController : BaseController
    {
        public StudentStatusReportController(ApplicationDbContext db,
                                             IFlashMessage flashMessage,
                                             ISelectListProvider selectListProvider,
                                             IAcademicProvider academicProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyIds, criteria.CurriculumId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            if (criteria.StudentStatuses != null)
            {
                criteria.StudentStatuses = criteria.StudentStatuses.Where(x => x != null).ToList();
            }

            var students = new List<Student>();
            var thaiNationality = _db.Nationalities.FirstOrDefault(x => x.NameEn == "Thai");
            if (criteria.Type == "f")
            {
                students = _db.Students.AsNoTracking()
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.Advisor)
                                           .ThenInclude(x => x.Title)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.AcademicLevel)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.Faculty)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.Department)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.CurriculumVersion)
                                           .ThenInclude(x => x.Curriculum)
                                       .Include(x => x.AdmissionInformation)
                                           .ThenInclude(x => x.AdmissionTerm)
                                       .Include(x => x.AdmissionInformation)
                                           .ThenInclude(x => x.AdmissionType)
                                       .Include(x => x.ParentInformations)
                                           .ThenInclude(x => x.Occupation)
                                       .Include(x => x.ParentInformations)
                                           .ThenInclude(x => x.Relationship)
                                       .Include(x => x.ParentInformations)
                                           .ThenInclude(x => x.Country)
                                       .Include(x => x.ParentInformations)
                                           .ThenInclude(x => x.District)
                                       .Include(x => x.ParentInformations)
                                           .ThenInclude(x => x.Subdistrict)
                                       .Include(x => x.StudentAddresses)
                                           .ThenInclude(x => x.Country)
                                       .Include(x => x.StudentAddresses)
                                           .ThenInclude(x => x.Subdistrict)
                                       .Include(x => x.StudentAddresses)
                                           .ThenInclude(x => x.District)
                                       .Include(x => x.StudentAddresses)
                                           .ThenInclude(x => x.Province)
                                       .Include(x => x.MaintenanceStatuses)
                                           .ThenInclude(x => x.Term)
                                       .Include(x => x.StudentStatusLogs)
                                           .ThenInclude(x => x.Term)
                                       .Include(x => x.Title)
                                       .Include(x => x.Nationality)
                                       .Include(x => x.Race)
                                       .Include(x => x.Religion)
                                       .Include(x => x.StudentFeeType)
                                       .Include(x => x.ResidentType)
                                       .Include(x => x.CurriculumInformations)
                                           .ThenInclude(x => x.CurriculumVersion)
                                       .Include(x => x.CurriculumInformations)
                                           .ThenInclude(x => x.SpecializationGroupInformations)
                                       .Where(x => x.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                   && (criteria.TermIds == null
                                                       || !criteria.TermIds.Any()
                                                       || criteria.TermIds.Any(y => y == x.AdmissionInformation.AdmissionTermId))
                                                   && (criteria.FacultyIds == null
                                                       || !criteria.FacultyIds.Any()
                                                       || criteria.FacultyIds.Any(y => y == x.AcademicInformation.FacultyId))
                                                   && (criteria.DepartmentIds == null
                                                       || !criteria.DepartmentIds.Any()
                                                       || criteria.DepartmentIds.Any(y => y == x.AcademicInformation.DepartmentId))
                                                   && (criteria.CurriculumId == 0
                                                       || x.CurriculumInformations.Any(y => y.CurriculumVersion.CurriculumId == criteria.CurriculumId))
                                                   && (criteria.CurriculumVersionId == 0
                                                       || x.CurriculumInformations.Any(y => y.CurriculumVersionId == criteria.CurriculumVersionId))
                                                   && ((criteria.StudentCodeFrom ?? 0) == 0
                                                       || x.CodeInt >= criteria.StudentCodeFrom)
                                                   && ((criteria.StudentCodeTo ?? 0) == 0
                                                       || x.CodeInt <= criteria.StudentCodeTo)
                                                   && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                       || x.FirstNameEn.Contains(criteria.CodeAndName)
                                                       || (x.FirstNameTh ?? string.Empty).Contains(criteria.CodeAndName)
                                                       || x.LastNameEn.Contains(criteria.CodeAndName)
                                                       || (x.LastNameTh ?? string.Empty).Contains(criteria.CodeAndName))
                                                   && (criteria.StudentStatuses == null
                                                       || !criteria.StudentStatuses.Any()
                                                       || criteria.StudentStatuses.Any(y => y == x.StudentStatus))
                                                   && (criteria.StudentFeeTypeIds == null 
                                                       || !criteria.StudentFeeTypeIds.Any()
                                                       || criteria.StudentFeeTypeIds.Any(y => y == x.StudentFeeTypeId))
                                                   && (criteria.ResidentTypeIds == null 
                                                       || !criteria.ResidentTypeIds.Any()
                                                       || criteria.ResidentTypeIds.Any(y => y == x.ResidentTypeId))
                                                   && (criteria.IsThai == null 
                                                       || (criteria.IsThai.Value ? x.NationalityId == thaiNationality.Id : x.NationalityId != thaiNationality.Id))
                                                   && (criteria.NationalityIds == null 
                                                       || !criteria.NationalityIds.Any()
                                                       || criteria.NationalityIds.Any(y => y == x.NationalityId))
                                                   && ((criteria.StartStudentBatch ?? 0) == 0
                                                       || x.AcademicInformation.Batch >= criteria.StartStudentBatch)
                                                   && ((criteria.EndStudentBatch ?? 0) == 0
                                                       || x.AcademicInformation.Batch <= criteria.EndStudentBatch)
                                                   && (criteria.AdmissionTypeIds == null 
                                                       || !criteria.AdmissionTypeIds.Any()
                                                       || criteria.AdmissionTypeIds.Any(y => y == x.AdmissionInformation.AdmissionTypeId))
                                                   && ((criteria.BirthYear ?? 0) == 0
                                                       || x.BirthDate.Year == criteria.BirthYear)
                                                   && ((criteria.MinorId == 0) 
                                                       || x.CurriculumInformations.Any(y => y.SpecializationGroupInformations.Any(z => z.SpecializationGroupId == criteria.MinorId)))
                                                   )
                                       .ToList();
                
                students.Select(x => {
                                         var studentStatusLog = x.StudentStatusLogs.LastOrDefault();
                                         x.StatusLogTerm = studentStatusLog == null ? "" : studentStatusLog.Term?.TermText;

                                         var resignStudent = _db.ResignStudents.FirstOrDefault(y => y.StudentId == x.Id);
                                         if (x.StudentStatus == "rs" && resignStudent != null)
                                         {
                                             x.StatusLogDate = resignStudent.ApprovedAtText;
                                         }
                                         else
                                         {
                                             x.StatusLogDate = studentStatusLog == null ? "" : studentStatusLog.UpdatedDateTimeText;
                                         }
                                         var maintain = x.MaintenanceStatuses.LastOrDefault();
                                         x.StatusActivedTerm = maintain == null ? "" : maintain.Term?.TermText ?? "";
                                         x.StatusActivedDate = maintain == null ? "" : maintain.ApprovedAtText;

                                         x.CurrentAddress = x.StudentAddresses.Where(y => y.Type == "c")
                                                                              .LastOrDefault();

                                         x.PermanentAddress = x.StudentAddresses.Where(y => y.Type == "p")
                                                                                .FirstOrDefault();
                                         
                                         x.FatherInformation = x.ParentInformations.Where(y => y.Relationship.NameEn.ToLower().Contains("father"))
                                                                                   .FirstOrDefault();

                                         x.MotherInformation = x.ParentInformations.Where(y => y.Relationship.NameEn.ToLower().Contains("mother"))
                                                                                   .FirstOrDefault();

                                         x.MainParentInformation = x.ParentInformations.Where(y => y.IsMainParent)
                                                                                       .FirstOrDefault();
                                         
                                         x.EmergencyInformation = x.ParentInformations.Where(y => y.IsEmergencyContact)
                                                                                      .FirstOrDefault();
                                         
                                         return x;
                                     }).ToList();
            }
            else
            {
                students = _db.Students.AsNoTracking()
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.AcademicLevel)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.Faculty)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.Department)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.CurriculumVersion)
                                           .ThenInclude(x => x.Curriculum)
                                       .Include(x => x.AdmissionInformation)
                                           .ThenInclude(x => x.AdmissionTerm)
                                       .Include(x => x.AdmissionInformation)
                                           .ThenInclude(x => x.AdmissionType)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.Advisor)
                                           .ThenInclude(x => x.Title)
                                       .Include(x => x.Title)
                                       .Include(x => x.Nationality)
                                       .Include(x => x.Religion)
                                       .Include(x => x.StudentFeeType)
                                       .Include(x => x.ResidentType)
                                       .Include(x => x.CurriculumInformations)
                                           .ThenInclude(x => x.CurriculumVersion)
                                       .Include(x => x.CurriculumInformations)
                                           .ThenInclude(x => x.SpecializationGroupInformations)
                                       .Where(x => x.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                   && (criteria.TermIds == null
                                                       || !criteria.TermIds.Any()
                                                       || criteria.TermIds.Any(y => y == x.AdmissionInformation.AdmissionTermId))
                                                   && (criteria.FacultyIds == null
                                                       || !criteria.FacultyIds.Any()
                                                       || criteria.FacultyIds.Any(y => y == x.AcademicInformation.FacultyId))
                                                   && (criteria.DepartmentIds == null
                                                       || !criteria.DepartmentIds.Any()
                                                       || criteria.DepartmentIds.Any(y => y == x.AcademicInformation.DepartmentId))
                                                   && (criteria.CurriculumId == 0
                                                       || x.CurriculumInformations.Any(y => y.CurriculumVersion.CurriculumId == criteria.CurriculumId))
                                                   && (criteria.CurriculumVersionId == 0
                                                       || x.CurriculumInformations.Any(y => y.CurriculumVersionId == criteria.CurriculumVersionId))
                                                   && ((criteria.StudentCodeFrom ?? 0) == 0
                                                       || x.CodeInt >= criteria.StudentCodeFrom)
                                                   && ((criteria.StudentCodeTo ?? 0) == 0
                                                       || x.CodeInt <= criteria.StudentCodeTo)
                                                   && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                       || x.FirstNameEn.Contains(criteria.CodeAndName)
                                                       || (x.FirstNameTh ?? string.Empty).Contains(criteria.CodeAndName)
                                                       || x.LastNameEn.Contains(criteria.CodeAndName)
                                                       || (x.LastNameTh ?? string.Empty).Contains(criteria.CodeAndName))
                                                   && (criteria.StudentStatuses == null
                                                       || !criteria.StudentStatuses.Any()
                                                       || criteria.StudentStatuses.Any(y => y == x.StudentStatus))
                                                   && (criteria.StudentFeeTypeIds == null
                                                       || !criteria.StudentFeeTypeIds.Any()
                                                       || criteria.StudentFeeTypeIds.Any(y => y == x.StudentFeeTypeId))
                                                   && (criteria.ResidentTypeIds == null 
                                                       || !criteria.ResidentTypeIds.Any()
                                                       || criteria.ResidentTypeIds.Any(y => y == x.ResidentTypeId))
                                                   && (criteria.IsThai == null
                                                       || (criteria.IsThai.Value ? x.NationalityId == thaiNationality.Id : x.NationalityId != thaiNationality.Id))
                                                   && (criteria.NationalityIds == null 
                                                       || !criteria.NationalityIds.Any()
                                                       || criteria.NationalityIds.Any(y => y == x.NationalityId))
                                                   && ((criteria.StartStudentBatch ?? 0) == 0
                                                       || x.AcademicInformation.Batch >= criteria.StartStudentBatch)
                                                   && ((criteria.EndStudentBatch ?? 0) == 0
                                                       || x.AcademicInformation.Batch <= criteria.EndStudentBatch)
                                                   && (criteria.AdmissionTypeIds == null
                                                       || !criteria.AdmissionTypeIds.Any()
                                                       || criteria.AdmissionTypeIds.Any(y => y == x.AdmissionInformation.AdmissionTypeId))
                                                   && ((criteria.BirthYear ?? 0) == 0
                                                       || x.BirthDate.Year == criteria.BirthYear)
                                                   && ((criteria.MinorId == 0) 
                                                       || x.CurriculumInformations.Any(y => y.SpecializationGroupInformations.Any(z => z.SpecializationGroupId == criteria.MinorId)))
                                                   )
                                       .ToList();
            }
                
            var model = students.OrderBy(x => x.Code)
                                .AsQueryable()
                                .GetPaged(criteria, page, true);
            return View(model);
        }

        private void CreateSelectList(long academicLevelId, List<long> facultyIds, long curriculumId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();
            ViewBag.ResidentTypes = _selectListProvider.GetResidentTypes();
            ViewBag.ThaiStatuses = _selectListProvider.GetThaiStatuses();
            ViewBag.Nationalities = _selectListProvider.GetNationalities();
            ViewBag.ReportTypes = _selectListProvider.GetStudentStatusReportType();
            ViewBag.AdmissionTypes = _selectListProvider.GetAdmissionTypes();
            ViewBag.Minors = _selectListProvider.GetMinors();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);

                if (facultyIds != null && facultyIds.Any())
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByFacultyIds(academicLevelId, facultyIds);
                }

                if (curriculumId > 0)
                {
                    ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
                }
            }
        }
    }
}