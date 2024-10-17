using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Helpers;
using KeystoneLibrary.Enumeration;
using Keystone.Permission;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("StudentStatusSummary", "")]
    public class StudentStatusSummaryController : BaseController
    {
        public StudentStatusSummaryController(ApplicationDbContext db,
                                              IFlashMessage flashMessage,
                                              ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.CurriculumId);
            StudentStatusSummaryViewModel model = new StudentStatusSummaryViewModel()
                                                  {
                                                      Criteria = criteria
                                                  };
            
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            var students = _db.Students.Include(x => x.AcademicInformation)
                                       .Include(x => x.CurriculumInformations)
                                       .Where(x => x.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                   && (criteria.FacultyId == 0 || x.AcademicInformation.FacultyId == criteria.FacultyId)
                                                   && (criteria.DepartmentId == 0 || x.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                   && (criteria.StartStudentBatch == null
                                                       || criteria.StartStudentBatch == 0
                                                       || x.AcademicInformation.Batch >= criteria.StartStudentBatch)
                                                   && (criteria.EndStudentBatch == null
                                                       || criteria.EndStudentBatch == 0
                                                       || x.AcademicInformation.Batch <= criteria.EndStudentBatch)
                                                   && (criteria.StudentStatuses == null 
                                                       || criteria.StudentStatuses.Any(y => y == x.StudentStatus))
                                                   && (criteria.StudentFeeTypeIds == null
                                                       || criteria.StudentFeeTypeIds.Any(y => y == x.StudentFeeTypeId))
                                                   && (criteria.ResidentTypeIds == null
                                                       || criteria.ResidentTypeIds.Any(y => y == x.ResidentTypeId))
                                                   && (string.IsNullOrEmpty(criteria.Active) 
                                                        || x.IsActive == Convert.ToBoolean(criteria.Active))
                                                   && (criteria.CurriculumId == 0
                                                       || x.CurriculumInformations.Any(y => y.CurriculumVersion != null 
                                                                                            && y.CurriculumVersion.CurriculumId == criteria.CurriculumId))
                                                   && (criteria.CurriculumVersionId == 0
                                                       || x.CurriculumInformations.Any(y => y.CurriculumVersionId == criteria.CurriculumVersionId)))
                                       .Select(x => new 
                                                    {
                                                        x.AcademicInformation.Batch,
                                                        x.StudentStatus
                                                    })
                                       .IgnoreQueryFilters()
                                       .ToList();

            foreach (var item in students.GroupBy(x => x.Batch))
            {
                StudentStatusSummaryResult result = new StudentStatusSummaryResult()
                                                    {
                                                        Batch = item.Key
                                                    };

                foreach (var student in item)
                {
                    var status = student.StudentStatus.ToUpper();
                    var value = Convert.ToInt64(result.GetType().GetProperty(status).GetValue(result));
                    result.GetType().GetProperty(status).SetValue(result, value + 1);
                }

                model.Results.Add(result);
            }
            
            model.Results = model.Results.OrderBy(x => x.Batch)
                                         .ToList();
            return View(model);
        }

        public ActionResult Details(long academicLevelId, long facultyId, long departmentId, int batch, 
                                    List<long> studentFeeTypeIds, List<long> residentTypeIds, string active,
                                    long curriculumId, long curriculumVersionId,
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
                                                   && x.AcademicInformation.Batch == batch
                                                   && x.StudentStatus == studentStatus.ToLower()
                                                   && (facultyId == 0 || x.AcademicInformation.FacultyId == facultyId)
                                                   && (departmentId == 0 || x.AcademicInformation.DepartmentId == departmentId)
                                                   && (studentFeeTypeIds == null
                                                       || !studentFeeTypeIds.Any()
                                                       || studentFeeTypeIds.Any(y => y == x.StudentFeeTypeId))
                                                   && (residentTypeIds == null
                                                       || !residentTypeIds.Any()
                                                       || residentTypeIds.Any(y => y == x.ResidentTypeId))
                                                   && (string.IsNullOrEmpty(active) 
                                                        || x.IsActive == Convert.ToBoolean(active))
                                                   && (curriculumId == 0
                                                       || x.CurriculumInformations.Any(y => y.CurriculumVersion != null 
                                                                                            && y.CurriculumVersion.CurriculumId == curriculumId))
                                                   && (curriculumVersionId == 0
                                                       || x.CurriculumInformations.Any(y => y.CurriculumVersionId == curriculumVersionId)))
                                       .IgnoreQueryFilters()
                                       .ToList();

            List<StudentStatusSummaryDetail> model = new List<StudentStatusSummaryDetail>();
            string status = "N/A";
            if (Enum.TryParse<StudentStatus>(studentStatus, true, out StudentStatus result))
            {
                status = result.GetDisplayName();
            }
            
            ViewBag.Title = $"Status : { status } and Batch : { batch }";
            foreach (var item in students)
            {
                var student = new StudentStatusSummaryDetail
                              {
                                  Code = item.Code,
                                  Title = item.Title.NameEn,
                                  FullName = $"{ item.FirstNameEn } { item.MidNameEn } { item.LastNameEn }",
                                  FacultyName = item.AcademicInformation.Faculty.NameEn,
                                  DepartmentName = item.AcademicInformation.Department.NameEn,
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

            return View(model);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long curriculumId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses(KeystoneLibrary.Models.Enums.GetStudentStatusesEnum.DefaultWithoutAll);
            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();
            ViewBag.ResidentTypes = _selectListProvider.GetResidentTypes();
            ViewBag.ActiveStatuses = _selectListProvider.GetActiveStatuses();
            if (academicLevelId > 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);
                if (facultyId > 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }

                if (curriculumId > 0)
                {
                    ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
                }
            }
        }
    }
}