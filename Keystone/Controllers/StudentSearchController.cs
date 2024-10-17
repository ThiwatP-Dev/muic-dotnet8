using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("StudentSearch", "")]
    public class StudentSearchController : BaseController
    {
        protected readonly IStudentPhotoProvider _studentPhotoProvider;

        public StudentSearchController(ApplicationDbContext db,
                                       ISelectListProvider selectListProvider,
                                       IFlashMessage flashMessage,
                                       IStudentPhotoProvider studentPhotoProvider,
                                       IMapper mapper) : base(db, flashMessage, mapper, selectListProvider) 
        { 
            _studentPhotoProvider = studentPhotoProvider;
        }

        public async Task<IActionResult> Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.CurriculumId);
            if (string.IsNullOrEmpty(criteria.Code) && string.IsNullOrEmpty(criteria.PreviousCode) && string.IsNullOrEmpty(criteria.FirstName)
                && string.IsNullOrEmpty(criteria.LastName) && string.IsNullOrEmpty(criteria.CitizenAndPassport) && criteria.AcademicLevelId == 0
                && criteria.FacultyId == 0 && criteria.DepartmentId == 0 && criteria.CurriculumId == 0 && criteria.CurriculumVersionId == 0 && criteria.Gender == null
                && criteria.NationalityId == 0 && criteria.CreditFrom == null && criteria.CreditTo == null && string.IsNullOrEmpty(criteria.StudentStatus) && string.IsNullOrEmpty(criteria.Status))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var codes = new List<string>();
            if (!string.IsNullOrEmpty(criteria.Code))
            {
                codes = criteria.Code.Split(',')
                                     .Select(x => x?.Trim())
                                     .Where(x => !string.IsNullOrEmpty(x))
                                     .ToList();
                if (!codes.Any())
                {
                    _flashMessage.Warning(Message.RequiredData);
                    return View();
                }
            }

            var students = from student in _db.Students
                           join title in _db.Titles on student.TitleId equals title.Id
                           join academic in _db.AcademicInformations on student.Id equals academic.StudentId into academics
                           from academic in academics.DefaultIfEmpty()
                           join curriculumVersion in _db.CurriculumVersions on academic.CurriculumVersionId equals curriculumVersion.Id into curriculumVersions
                           from curriculumVersion in curriculumVersions.DefaultIfEmpty()
                           join faculty in _db.Faculties on academic.FacultyId equals faculty.Id into faculties
                           from faculty in faculties.DefaultIfEmpty()
                           join department in _db.Departments on academic.DepartmentId equals department.Id into departments
                           from department in departments.DefaultIfEmpty()
                           join nationality in _db.Nationalities on student.NationalityId equals nationality.Id into nationalities
                           from nationality in nationalities.DefaultIfEmpty()
                           join advisor in _db.Instructors on academic.AdvisorId equals advisor.Id into advisors
                           from advisor in advisors.DefaultIfEmpty()
                           join advisorTitle in _db.Titles on advisor.TitleId equals advisorTitle.Id into advisorTitles
                           from advisorTitle in advisorTitles.DefaultIfEmpty()
                           where student.StudentStatus != "a"
                                 && (!codes.Any() || codes.Contains(student.Code))
                                 && (string.IsNullOrEmpty(criteria.PreviousCode) || (academic != null && academic.OldStudentCode.StartsWith(criteria.PreviousCode)))
                                 && (string.IsNullOrEmpty(criteria.FirstName)
                                     || (student.FirstNameEn ?? string.Empty).StartsWith(criteria.FirstName)
                                     || (student.FirstNameTh ?? string.Empty).StartsWith(criteria.FirstName))
                                 && (string.IsNullOrEmpty(criteria.LastName)
                                     || (student.LastNameEn ?? string.Empty).StartsWith(criteria.LastName)
                                     || (student.LastNameTh ?? string.Empty).StartsWith(criteria.LastName))
                                 && (string.IsNullOrEmpty(criteria.CitizenAndPassport) 
                                     || student.CitizenNumber.Contains(criteria.CitizenAndPassport)
                                     || student.Passport.Contains(criteria.CitizenAndPassport))
                                 && (criteria.AcademicLevelId == 0
                                     || (academic != null && academic.AcademicLevelId == criteria.AcademicLevelId))
                                 && (criteria.FacultyId == 0
                                     || (academic != null && academic.FacultyId == criteria.FacultyId))
                                 && (criteria.DepartmentId == 0
                                     || (academic != null && academic.DepartmentId == criteria.DepartmentId))
                                 && (criteria.CurriculumId == 0
                                     || (curriculumVersion != null && curriculumVersion.CurriculumId == criteria.CurriculumId))
                                 && (criteria.CurriculumVersionId == 0
                                     || (academic != null && academic.CurriculumVersionId == criteria.CurriculumVersionId))
                                 && (criteria.Gender == null
                                     || student.Gender == criteria.Gender)
                                 && (criteria.NationalityId == 0
                                     || student.NationalityId == criteria.NationalityId)
                                 && (criteria.CreditFrom == null
                                     || (academic != null && academic.CreditComp >= criteria.CreditFrom))
                                 && (criteria.CreditTo == null
                                     || (academic != null && academic.CreditComp <= criteria.CreditTo))
                                 && (string.IsNullOrEmpty(criteria.StudentStatus)
                                     || student.StudentStatus == criteria.StudentStatus)
                                 && (string.IsNullOrEmpty(criteria.Status) 
                                     || student.IsActive == Convert.ToBoolean(criteria.Status))
                                 && ((criteria.MinorId == 0)
                                     || student.CurriculumInformations.Any(x => x.SpecializationGroupInformations.Any(y => y.SpecializationGroupId == criteria.MinorId)))
                           select new StudentSearchViewModel
                           {
                               Code = student.Code,
                               FullNameEn = (student.MidNameEn ?? string.Empty) == string.Empty 
                                            ? title.NameEn + " " + student.FirstNameEn + " " + student.LastNameEn
                                            : title.NameEn + " " + student.FirstNameEn + " " + student.MidNameEn + " " + student.LastNameEn,
                               FullNameTh = (student.MidNameTh ?? string.Empty) == string.Empty 
                                            ? title.NameTh + " " + student.FirstNameTh + " " + student.LastNameTh
                                            : title.NameTh + " " + student.FirstNameTh + " " + student.MidNameTh + " " + student.LastNameTh,
                               FacultyNameEn = faculty == null ? string.Empty : faculty.NameEn,
                               FacultyNameTh = faculty == null ? string.Empty : faculty.NameTh,
                               FacultyId = academic == null ? 0 : academic.FacultyId,
                               DepartmentCode = department == null ? string.Empty : department.Code,
                               DepartmentNameEn = department == null ? string.Empty : department.Code,
                               DepartmentNameTh = department == null ? string.Empty : department.Code,
                               DepartmentId = academic == null ? 0 : (academic.DepartmentId ?? 0),
                               CitizenNumber = student.CitizenNumber,
                               Passport = student.Passport,
                               NationalityNameEn = nationality == null ? string.Empty : nationality.NameEn,
                               NationalityNameTh = nationality == null ? string.Empty : nationality.NameTh,
                               ProfileImageURL = student.ProfileImageURL,
                               Credit = academic == null ? 0 : academic.CreditComp,
                               GPA = academic == null ? 0 : academic.GPA,
                               IsActive = student.IsActive,
                               Advisor = advisor == null ? string.Empty : (advisorTitle == null ? string.Empty : advisorTitle.NameEn) + " " + advisor.FirstNameEn + " " + advisor.LastNameEn
                           };
                           
            var model = students.IgnoreQueryFilters()
                                .GetPaged(criteria, page, true);

            foreach (var student in model.Results)
            {
                try
                {
                    student.ProfileImageURL = await _studentPhotoProvider.GetStudentImg(student.Code);
                }
                catch (Exception) { }
            }

            return View(model);
        }

        private void CreateSelectList(long academicLevelId, long facultyId, long curriculumId)
        {
            ViewBag.Statuses = _selectListProvider.GetActiveStatuses();
            ViewBag.Nationalities = _selectListProvider.GetNationalities();
            ViewBag.Genders = _selectListProvider.GetGender();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);
            ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
            ViewBag.Minors = _selectListProvider.GetMinors();
        }
    }
}