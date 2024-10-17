using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class StudentStatisticByProvinceAndSchoolReportController : BaseController
    {
        protected readonly IAdmissionProvider _admissionProvider;

        public StudentStatisticByProvinceAndSchoolReportController(ApplicationDbContext db,
                                                                   ISelectListProvider selectListProvider,
                                                                   IFlashMessage flashMessage,
                                                                   IAdmissionProvider admissionProvider) : base(db, flashMessage, selectListProvider)
        {
            _admissionProvider = admissionProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList();
            if (criteria.Batches == null || string.IsNullOrEmpty(criteria.Type) || string.IsNullOrEmpty(criteria.Language))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var students = _db.Students.Include(x => x.AcademicInformation)
                                       .Include(x => x.AdmissionInformation)
                                           .ThenInclude(x => x.PreviousSchool)
                                               .ThenInclude(x => x.SchoolTerritory)
                                       .Include(x => x.AdmissionInformation)
                                           .ThenInclude(x => x.PreviousSchool)
                                               .ThenInclude(x => x.Province)
                                       .Include(x => x.AdmissionInformation)
                                           .ThenInclude(x => x.AdmissionTerm)
                                       .Include(x => x.RegistrationCourses)
                                           .ThenInclude(x => x.Course)
                                       .Include(x => x.RegistrationCourses)
                                           .ThenInclude(x => x.Term)
                                       .Where(x => (criteria.Batches == null
                                                    || criteria.Batches.Contains(x.AcademicInformation.Batch))
                                                   && (criteria.SchoolTerritoryId == 0
                                                       || criteria.SchoolTerritoryId == x.AdmissionInformation.PreviousSchool.SchoolTerritoryId)
                                                   && (criteria.ProvinceId == 0
                                                       || criteria.ProvinceId == x.AdmissionInformation.PreviousSchool.ProvinceId))
                                       .ToList();

            var studentGroup = _admissionProvider.GetStudentStatisticByProvinceAndSchoolReport(students, criteria);
            ViewBag.HeaderBatches = criteria.Batches.OrderBy(x => x)
                                                    .ToList();
            var studentPageResult = studentGroup.AsQueryable()
                                                .GetPaged(criteria, page, true);

            return View(studentPageResult);
        }

        public void CreateSelectList()
        {
            ViewBag.Types = _selectListProvider.GetStudentStatisticByProvinceAndSchoolReportType();
            ViewBag.Batches = _selectListProvider.GetBatches();
            ViewBag.Languages = _selectListProvider.GetLanguages();
            ViewBag.SchoolTerritories = _selectListProvider.GetSchoolTerritories();
            ViewBag.Provinces = _selectListProvider.GetProvinces();
        }
    }
}