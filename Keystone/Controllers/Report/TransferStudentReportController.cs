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
    [PermissionAuthorize("TransferStudentReport", "")]
    public class TransferStudentReportController : BaseController
    {
        public TransferStudentReportController(ApplicationDbContext db,
                                               IFlashMessage flashMessage,
                                               ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var results = _db.StudentTransferLogs.Include(x => x.Term)
                                                 .Include(x => x.TransferUniversity)
                                                 .Include(x => x.Student)
                                                     .ThenInclude(x => x.AcademicInformation)
                                                         .ThenInclude(x => x.Department)
                                                 .Include(x => x.Student)
                                                     .ThenInclude(x => x.Nationality)
                                                 .Include(x => x.Student)
                                                     .ThenInclude(x => x.ResidentType)
                                                 .Include(x => x.StudentTransferLogDetails)
                                                     .ThenInclude(x => x.ExternalCourse)
                                                 .Include(x => x.StudentTransferLogDetails)
                                                     .ThenInclude(x => x.Course)
                                                 .Include(x => x.StudentTransferLogDetails)
                                                     .ThenInclude(x => x.Grade)
                                                 .Where(x => x.TermId == criteria.TermId
                                                             && x.TransferUniversityId != null
                                                             && (criteria.FacultyId == 0 
                                                                 || x.Student.AcademicInformation.FacultyId == criteria.FacultyId)
                                                             && (criteria.DepartmentId == 0 
                                                                 || x.Student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                             && (string.IsNullOrEmpty(criteria.Code) 
                                                                 || x.Student.Code.StartsWith(criteria.Code))
                                                             && (string.IsNullOrEmpty(criteria.FirstName) 
                                                                 || x.Student.FirstNameEn.StartsWith(criteria.FirstName))
                                                             && (criteria.TransferUniversityId == 0 
                                                                 || x.TransferUniversityId == criteria.TransferUniversityId))
                                               .ToList();
                                                           
            var model = new TransferStudentReportViewModel
                        {
                            Criteria = criteria,
                            Results = results
                        };

            return View(model);
        }

        public void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.TransferUniversities = _selectListProvider.GetTransferUniversities();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                if (facultyId != 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }
        }
    }
}