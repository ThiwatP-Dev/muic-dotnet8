using AutoMapper;
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
    [PermissionAuthorize("StudentStatusStatistic", "")]
    public class StudentStatusStatisticController : BaseController
    {
        public StudentStatusStatisticController(ApplicationDbContext db,
                                                IFlashMessage flashMessage,
                                                IMapper mapper,
                                                ISelectListProvider selectListProvider) : base(db, flashMessage, mapper, selectListProvider) { }
        
        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0 || string.IsNullOrEmpty(criteria.Status) || criteria.StartTermId == 0 || criteria.EndTermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var rawData = _db.Students.IgnoreQueryFilters()
                                      .Where(x => criteria.AcademicLevelId == x.AcademicInformation.AcademicLevelId
                                                  && (criteria.FacultyId == 0
                                                      || criteria.FacultyId == x.AcademicInformation.FacultyId)
                                                  && (criteria.DepartmentId == 0
                                                      || criteria.DepartmentId == x.AcademicInformation.DepartmentId)
                                                  && (criteria.StartTermId <= x.AdmissionInformation.AdmissionTermId)
                                                  && (criteria.EndTermId >= x.AdmissionInformation.AdmissionTermId)
                                                  && (string.IsNullOrEmpty(criteria.Active) 
                                                      || x.IsActive == Convert.ToBoolean(criteria.Active))
                                                  && criteria.Status == x.StudentStatus)
                                      .Select(x => new
                                                   {
                                                       x.AdmissionInformation.AdmissionTerm.AcademicYear,
                                                       x.AdmissionInformation.AdmissionTerm.AcademicTerm,
                                                       FacultyNameEn = x.AcademicInformation.Faculty.NameEn,
                                                       DepartmentCode = x.AcademicInformation.Department.Code,
                                                       DepartmentNameEn = x.AcademicInformation.Department.NameEn,
                                                       StudentId = x.Id
                                                   })
                                      .ToList();

            var model = new StudentStatusStatisticViewModel();

            var term = rawData.GroupBy(x => x.AcademicYear)
                              .Select(x => new TermDetails
                                           {
                                               Year = x.Key,
                                               Terms = x.GroupBy(y => y.AcademicTerm)
                                                        .Select(y => y.Key)
                                                        .OrderBy(y => y)
                                                        .ToList()
                                           })
                              .OrderBy(x => x.Year)
                              .ToList();
            
            model.TermHeader = term;

            var studentCount = rawData.GroupBy(x => x.FacultyNameEn)
                                      .Select(x => new StudentStatusStatisticDeatils
                                                   {
                                                       Faculty = x.Key,
                                                       NumberOfStudentInFaculty = x.Count(),
                                                       TotalStudentInTerm = x.GroupBy(y => new 
                                                                                           {
                                                                                               y.AcademicYear,
                                                                                               y.AcademicTerm
                                                                                           })
                                                                             .Select(y => new StudentStatusStatisticReportCount
                                                                                          {
                                                                                              Year = y.Key.AcademicYear,
                                                                                              Term = y.Key.AcademicTerm,
                                                                                              NumberOfStudent = y.Count()
                                                                                          })
                                                                             .OrderBy(y => y.Year)
                                                                                 .ThenBy(y => y.Term)
                                                                             .ToList(),
                                                       StudentStatusStatisticDepartments = x.GroupBy(y => new
                                                                                                          {
                                                                                                              y.DepartmentCode,
                                                                                                              y.DepartmentNameEn
                                                                                                          })
                                                                                            .Select(y => new StudentStatusStatisticDepartment
                                                                                                         {
                                                                                                              Code = y.Key.DepartmentCode,
                                                                                                              Department = y.Key.DepartmentNameEn,
                                                                                                              NumberOfStudentInDepartment = y.Count(),
                                                                                                              StudentStatusStatisticReportCounts = y.GroupBy(z => new 
                                                                                                                                                                  {
                                                                                                                                                                      z.AcademicYear,
                                                                                                                                                                      z.AcademicTerm
                                                                                                                                                                  })
                                                                                                                                                    .Select(z => new StudentStatusStatisticReportCount
                                                                                                                                                                 {
                                                                                                                                                                     Year = z.Key.AcademicYear,
                                                                                                                                                                     Term = z.Key.AcademicTerm,
                                                                                                                                                                     NumberOfStudent = z.Count()
                                                                                                                                                                 })
                                                                                                                                                    .OrderBy(z => z.Year)
                                                                                                                                                        .ThenBy(z => z.Term)
                                                                                                                                                    .ToList()
  
                                                                                                         })
                                                                                            .ToList()
                                                   })
                                      .ToList();

            model.Students = studentCount;

            return View(model);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.ActiveStatuses = _selectListProvider.GetActiveStatuses();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            if (academicLevelId > 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.AdmissionTerms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }

            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            }
        }
    }
}