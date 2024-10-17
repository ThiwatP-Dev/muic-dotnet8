using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Enumeration;
using KeystoneLibrary.Helpers;

namespace Keystone.Controllers.Report
{
    public class StudentStatusSummaryByDepartmentController : BaseController
    {
        public StudentStatusSummaryByDepartmentController(ApplicationDbContext db,
                                                          IFlashMessage flashMessage,
                                                          ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }
        
        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            StudentStatusSummaryViewModel model = new StudentStatusSummaryViewModel()
                                                  {
                                                      Criteria = criteria
                                                  };

            if (criteria.AcademicLevelId == 0 || criteria.FacultyId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            var students = _db.Students.Include(x => x.AcademicInformation)
                                            .ThenInclude(x => x.Department)
                                       .Where(x => x.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                   && (x.AcademicInformation.FacultyId == criteria.FacultyId)
                                                   && (criteria.DepartmentId == 0 || x.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                   && ((criteria.StartStudentBatch ?? 0) == 0
                                                       || x.AcademicInformation.Batch >= criteria.StartStudentBatch)
                                                   && ((criteria.EndStudentBatch ?? 0) == 0
                                                       || x.AcademicInformation.Batch <= criteria.EndStudentBatch)
                                                   && (criteria.StudentStatuses == null 
                                                       || criteria.StudentStatuses.Any(y => y == x.StudentStatus))
                                                   && (string.IsNullOrEmpty(criteria.Active) 
                                                        || x.IsActive == Convert.ToBoolean(criteria.Active)))
                                       .Select(x => new 
                                                    {
                                                        x.AcademicInformation.Department.Abbreviation,
                                                        x.AcademicInformation.DepartmentId,
                                                        x.StudentStatus
                                                    })
                                       .IgnoreQueryFilters()
                                       .ToList();

            foreach (var item in students.GroupBy(x => x.Abbreviation))
            {
                StudentStatusSummaryResult result = new StudentStatusSummaryResult()
                                                    {
                                                        DepartmentAbbreviation = item.Key,
                                                        DepartmentId = item.FirstOrDefault().DepartmentId
                                                    };

                foreach (var department in item.GroupBy(x => x.StudentStatus))
                {
                    var status = department.Key.ToUpper();
                    result.GetType().GetProperty(status).SetValue(result, department.Count());
                }

                model.Results.Add(result);
            }
            
            model.Results = model.Results.OrderBy(x => x.DepartmentAbbreviation)
                                         .ToList();
            return View(model);
        }

        public ActionResult Details(long academicLevelId, long facultyId, long departmentId, string active,
                                    string studentStatus, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var students = _db.Students.Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.Faculty)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.Department)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.Advisor)
                                           .ThenInclude(x => x.Title)
                                       .Include(x => x.StudentStatusLogs)
                                           .ThenInclude(x => x.Term)
                                       .Include(x => x.Title)
                                       .Include(x => x.CurriculumInformations)
                                       .Where(x => x.AcademicInformation.AcademicLevelId == academicLevelId
                                                   && x.StudentStatus == studentStatus.ToLower()
                                                   && ( x.AcademicInformation.FacultyId == facultyId)
                                                   && (x.AcademicInformation.DepartmentId == departmentId)
                                                   && (string.IsNullOrEmpty(active)
                                                        || x.IsActive == Convert.ToBoolean(active)))
                                       .IgnoreQueryFilters()
                                       .ToList();

            List<StudentStatusSummaryDetail> model = new List<StudentStatusSummaryDetail>();
            string status = "N/A";
            if (Enum.TryParse<StudentStatus>(studentStatus, true, out StudentStatus result))
            {
                status = result.GetDisplayName();
            }
            
            ViewBag.Title = $"Status : { status }";
            foreach (var item in students)
            {
                var student = new StudentStatusSummaryDetail
                              {
                                  Code = item.Code,
                                  Title = item.Title.NameEn,
                                  FullName = $"{ item.FirstNameEn } { item.MidNameEn } { item.LastNameEn }",
                                  FacultyName = item.AcademicInformation.Faculty.NameEn,
                                  DepartmentName = item.AcademicInformation.Department.Abbreviation,
                                  StudentStatusText = item.StudentStatusText,
                                  AdvisorName = item.AcademicInformation.Advisor?.FullNameEn
                              };
                              
                if (item.StudentStatusLogs != null && item.StudentStatusLogs.Any())
                {
                    var log = item.StudentStatusLogs.LastOrDefault();
                    student.Term = log.Term.TermText;
                    student.Date = log.UpdatedAt;
                }   

                model.Add(student);
            }

            return View("~/Views/StudentStatusSummary/Details.cshtml", model);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.ActiveStatuses = _selectListProvider.GetActiveStatuses();
            if (academicLevelId > 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                if (facultyId > 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }
        }
    }
}