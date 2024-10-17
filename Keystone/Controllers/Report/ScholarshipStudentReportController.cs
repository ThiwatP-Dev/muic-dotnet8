using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("ScholarshipStudentReport", "")]
    public class ScholarshipStudentReportController : BaseController
    {
        public ScholarshipStudentReportController(ApplicationDbContext db,
                                                  IFlashMessage flashMessage,
                                                  IMapper mapper,
                                                  ISelectListProvider selectListProvider) : base(db, flashMessage, mapper, selectListProvider) { }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            var scholarshipStudents = _db.ScholarshipStudents.Include(x => x.Student)
                                                                 .ThenInclude(x => x.AcademicInformation)
                                                             .Include(x => x.Scholarship)
                                                             .Include(x => x.EffectivedTerm)
                                                             .Include(x => x.ExpiredTerm)
                                                             .Where(x => (criteria.ScholarshipTypeId == 0
                                                                          || x.Scholarship.ScholarshipTypeId == criteria.ScholarshipTypeId)
                                                                          && (criteria.ScholarshipId == 0
                                                                              || x.ScholarshipId == criteria.ScholarshipId)
                                                                          && (criteria.StartTermId == 0
                                                                              || x.EffectivedTermId == criteria.StartTermId)
                                                                          && (criteria.EndTermId == 0
                                                                              || x.ExpiredTermId == criteria.EndTermId)
                                                                         && (string.IsNullOrEmpty(criteria.Status)
                                                                             || x.IsActive == Convert.ToBoolean(criteria.Status)))
                                                             .IgnoreQueryFilters()
                                                             .OrderBy(x => x.Scholarship.NameEn)
                                                                 .ThenBy(x => x.Student.Code)
                                                             .GetPaged(criteria, page, true);

            return View(scholarshipStudents);
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.ScholarShipTypes = _selectListProvider.GetScholarshipTypes();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Scholarships = _selectListProvider.GetScholarships();
            ViewBag.Statuses = _selectListProvider.GetActiveStatuses();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}