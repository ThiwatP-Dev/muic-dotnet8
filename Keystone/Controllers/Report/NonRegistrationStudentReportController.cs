using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.Enums;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("NonRegistrationStudentReport", "")]
    public class NonRegistrationStudentReportController : BaseController
    {
        protected ICacheProvider _cacheProvider;
        public NonRegistrationStudentReportController(ApplicationDbContext db,
                                                      ISelectListProvider selectListProvider,
                                                      ICacheProvider cacheProvider,
                                                      IFlashMessage flashMessage) : base(db, flashMessage, selectListProvider) 
        { 
            _cacheProvider = cacheProvider;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {

                criteria.AcademicLevelId = _db.AcademicLevels.SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
                criteria.TermId = _cacheProvider.GetCurrentTerm(criteria.AcademicLevelId).Id;
                CreateSelectList(criteria.AcademicLevelId);
                return View(new PagedResult<StudentInformationViewModel>()
                            {
                                Criteria = criteria
                            });
            }

            var students = _db.Students.AsNoTracking()
                                       .Where(x => !x.RegistrationCourses.Any(y => y.TermId == criteria.TermId && y.Status != "d")
                                                   && (criteria.FacultyId == 0
                                                       || x.AcademicInformation.FacultyId == criteria.FacultyId)
                                                   && (criteria.DepartmentId == 0
                                                       || x.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                   && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                       || x.Code.Contains(criteria.CodeAndName)
                                                       || x.FirstNameEn.Contains(criteria.CodeAndName)
                                                       || x.MidNameEn.Contains(criteria.CodeAndName)
                                                       || x.LastNameEn.Contains(criteria.CodeAndName)
                                                       || x.FirstNameTh.Contains(criteria.CodeAndName)
                                                       || x.MidNameTh.Contains(criteria.CodeAndName)
                                                       || x.LastNameTh.Contains(criteria.CodeAndName))
                                                   && (criteria.StudentTypeId == 0
                                                       || x.StudentFeeTypeId == criteria.StudentTypeId)
                                                   && (criteria.ResidentTypeId == 0
                                                       || x.ResidentTypeId == criteria.ResidentTypeId)
                                                   && (string.IsNullOrEmpty(criteria.IsLocked)
                                                       || Convert.ToBoolean(criteria.IsLocked) == x.StudentIncidents.Any(y => y.LockedRegistration))
                                                   && (string.IsNullOrEmpty(criteria.StudentStatus)
                                                       || x.StudentStatus == criteria.StudentStatus))
                                       .Select(x => new StudentInformationViewModel
                                                    {
                                                      StudentId = x.Id,
                                                      StudentCode = x.Code,
                                                      StudentStatus = x.StudentStatus,
                                                      TitleEn = x.Title.NameEn,
                                                      FirstNameEn = x.FirstNameEn,
                                                      MidNameEn = x.MidNameEn,
                                                      LastNameEn = x.LastNameEn,
                                                      DepartmentCode = x.AcademicInformation.Department.Code,
                                                      AdvisorTitleEn = x.AcademicInformation.Advisor.Title.NameEn,
                                                      AdvisorFirstNameEn = x.AcademicInformation.Advisor.FirstNameEn,
                                                      AdvisorLastNameEn = x.AcademicInformation.Advisor.LastNameEn,
                                                      StudentFeeTypeEn = x.StudentFeeType.NameEn,
                                                      ResidentTypeEn = x.ResidentType.NameEn,
                                                      IsActive = x.IsActive,
                                                      IsLocked = x.StudentIncidents.Any(y => y.LockedRegistration)
                                                    })
                                       .OrderBy(x => x.StudentCode)
                                       .GetPaged(criteria, page, true);

            return View(students);
        }

        public void CreateSelectList(long academicLevelId)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();
            ViewBag.ResidentTypes = _selectListProvider.GetResidentTypes();
            ViewBag.LockedRegistrationStatuses = _selectListProvider.GetLockedRegistrationStatuses();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses(GetStudentStatusesEnum.DefaultStudying);
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}