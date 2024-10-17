using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.Report;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("ScholarshipBalanceReport", "")]
    public class ScholarshipBalanceReportController : BaseController
    {
        public ScholarshipBalanceReportController(ApplicationDbContext db,
                                                  IFlashMessage flashMessage,
                                                  ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.ScholarshipTypeId);
            if (criteria.AcademicYear == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var student = _db.ScholarshipStudents.Include(x => x.Student)
                                                     .ThenInclude(x => x.AcademicInformation)
                                                     .ThenInclude(x => x.Faculty)
                                                 .Include(x => x.Student)
                                                     .ThenInclude(x => x.AcademicInformation)
                                                     .ThenInclude(x => x.Department)
                                                 .Include(x => x.EffectivedTerm)
                                                 .Include(x => x.Scholarship)
                                                     .ThenInclude(x => x.ScholarshipType)
                                                 .IgnoreQueryFilters()
                                                 .Where(x => x.EffectivedTerm.AcademicYear == criteria.AcademicYear
                                                             && (criteria.ScholarshipTypeId == 0
                                                                 || x.Scholarship.ScholarshipTypeId == criteria.ScholarshipTypeId)
                                                             && (criteria.ScholarshipId == 0
                                                                 || x.ScholarshipId == criteria.ScholarshipId))
                                                 .GroupBy(x => x.ScholarshipId)
                                                 .Select(x => new ScholarshipReportDetail
                                                              {
                                                                  Year = criteria.AcademicYear.Value,
                                                                  Name = x.First().Scholarship.NameEn,
                                                                  Budget = x.First().Scholarship.LimitedAmount,
                                                                  Price = x.First().Scholarship.DefaultAmount,
                                                                  Total = x.Sum(y => y.LimitedAmount),
                                                                  TotalStudent = x.First().Scholarship.TotalStudent ?? 0,
                                                                  ScholarshipStudentDeatils = x.GroupBy(y => y.StudentId)
                                                                                               .Select(y => new ScholarshipStudentDeatil
                                                                                                            {
                                                                                                                FullName = y.First().Student.FullNameEn,
                                                                                                                Code = y.First().Student.Code,
                                                                                                                Department = y.First().Student.AcademicInformation.Department.NameEn,
                                                                                                                Price = y.Sum(z => z.LimitedAmount)
                                                                                                            })
                                                                                               .ToList()
                                                              })
                                                 .ToList();
            
            foreach (var item in student)
            {
                item.Balance = item.Budget != null ? item.Budget - item.Total : null;
            }

            var model = new ScholarshipBalanceReportViewModel
                        {
                            Criteria = criteria,
                            ScholarshipDetails = student
                        };

            return View(model);
        }

        private void CreateSelectList(long scholarshipTypeId = 0)
        {
            ViewBag.AcademicYears = _selectListProvider.GetAcademicYearByScholarshipStudents();
            ViewBag.ScholarshipTypes = _selectListProvider.GetScholarshipTypes();
            ViewBag.Scholarships = scholarshipTypeId > 0 ? _selectListProvider.GetScholarshipsByScholarshipTypeId(scholarshipTypeId)
                                                         : _selectListProvider.GetScholarships();
        }
    }
}