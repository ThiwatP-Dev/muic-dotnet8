using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Keystone.Controllers
{
    public class ScholarshipStudentController : BaseController
    {
        public ScholarshipStudentController(ApplicationDbContext db,
                                            ISelectListProvider selectListProvider) : base(db, selectListProvider) { }
        
        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.ScholarshipTypeId);
            var models = _db.ScholarshipStudents.Include(x => x.Student)
                                                    .ThenInclude(x => x.AcademicInformation)
                                                        .ThenInclude(x => x.Faculty)
                                                .Include(x => x.Student)
                                                    .ThenInclude(x => x.AcademicInformation)
                                                        .ThenInclude(x => x.Department)
                                                .Include(x => x.Scholarship)
                                                    .ThenInclude(x => x.ScholarshipType)
                                                .Where(x => (criteria.FacultyId == 0
                                                             || criteria.FacultyId == x.Student.AcademicInformation.FacultyId)
                                                             && (criteria.DepartmentId == 0
                                                                 || criteria.DepartmentId == x.Student.AcademicInformation.DepartmentId)
                                                             && (criteria.ScholarshipId == 0
                                                                 || criteria.ScholarshipId == x.ScholarshipId)
                                                             && (criteria.ScholarshipTypeId == 0
                                                                 || criteria.ScholarshipTypeId == x.Scholarship.ScholarshipTypeId)
                                                             && (string.IsNullOrEmpty(criteria.IsApproved)
                                                                 || Convert.ToBoolean(criteria.IsApproved) ? x.IsApproved : !x.IsApproved))
                                                .GetPaged(criteria, page);
            return View(models);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long scholarshipTypeId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.ScholarshipTypes = _selectListProvider.GetScholarshipTypes();
            ViewBag.Scholarships = _selectListProvider.GetScholarshipsByScholarshipTypeId(scholarshipTypeId);
            ViewBag.ApprovedStatus = _selectListProvider.GetApprovedStatuses();
            if (facultyId != 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            }
        }
    }
}