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
    [PermissionAuthorize("ResignStatisticReports", "")]
    public class ResignStatisticReportController : BaseController
    {
        public ResignStatisticReportController(ApplicationDbContext db,
                                               IFlashMessage flashMessage,
                                               ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) {}
        
        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if ((criteria.StartStudentBatch ?? 0) == 0 || (criteria.EndStudentBatch ?? 0) == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var results = _db.ResignStudents.Include(x => x.Student)
                                                .ThenInclude(x => x.AcademicInformation)
                                                .ThenInclude(x => x.Department)
                                                .ThenInclude(x => x.Faculty)
                                            .Include(x => x.Term)
                                            .IgnoreQueryFilters()
                                            .Where(x => x.Student.AcademicInformation.Batch >= criteria.StartStudentBatch
                                                        && x.Student.AcademicInformation.Batch <= criteria.EndStudentBatch
                                                        && (criteria.ResignReasonId == 0
                                                            || x.ResignReasonId == criteria.ResignReasonId)
                                                        && (criteria.AcademicLevelId == 0
                                                            || x.Term.AcademicLevelId == criteria.AcademicLevelId)
                                                        && ((criteria.AcademicYear ?? 0) == 0
                                                            || x.Term.AcademicYear == criteria.AcademicYear)
                                                        && (criteria.FacultyId == 0
                                                            || x.Student.AcademicInformation.FacultyId == criteria.FacultyId)
                                                        && (criteria.DepartmentId == 0
                                                            || x.Student.AcademicInformation.DepartmentId == criteria.DepartmentId))
                                            .OrderBy(x => x.Student.AcademicInformation.Department.Faculty.Abbreviation)
                                            .ThenBy(x => x.Student.AcademicInformation.Department.Abbreviation)
                                            .Select(x => new ResignStatisticReport
                                                         {
                                                             FacultyAbbreviation = x.Student.AcademicInformation.Department.Faculty.Abbreviation,
                                                             DepartmentAbbreviation = x.Student.AcademicInformation.Department.Abbreviation,
                                                             DepartmentName = x.Student.AcademicInformation.Department.NameEn,
                                                             AcademicYear = x.Term.AcademicYear,
                                                             AcademicTerm = x.Term.AcademicTerm,
                                                             Batch = x.Student.AcademicInformation.Batch
                                                         })
                                            .ToList();

            var model = new ResignStatisticReportViewModel
                        {
                            Criteria = criteria,
                            Results = results
                        };

            var academicYears = new List<int>();
            if ((criteria.AcademicYear ?? 0) == 0)
            {
                academicYears = results.Select(x => x.AcademicYear)
                                       .Distinct()
                                       .ToList();
            }
            else
            {
                academicYears.Add(criteria.AcademicYear ?? 0);
            }

            model.Terms = _db.Terms.Where(x => academicYears.Any(y => y == x.AcademicYear)
                                               && (criteria.AcademicLevelId == 0
                                                   || x.AcademicLevelId == criteria.AcademicLevelId))
                                   .Select(x => x.AcademicTerm)
                                   .Distinct()
                                   .OrderBy(x => x)
                                   .ToList();
            
            return View(model);
        }

        private void CreateSelectList(long academicLevelId, long facultyId) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.ResignReasons = _selectListProvider.GetResignReasons();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }
            
            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartments(facultyId);
            }
        }
    }
}