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
    [PermissionAuthorize("StudentExpectedGraduateReport", "")]
    public class StudentExpectedGraduateReportController : BaseController
    {
        public StudentExpectedGraduateReportController(ApplicationDbContext db,
                                                       IFlashMessage flashMessage,
                                                       ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }
        
        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.CurriculumId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var results = (from student in _db.Students.AsNoTracking()
                                                       .Include(x => x.AcademicInformation)
                                                           .ThenInclude(x => x.Faculty)
                                                       .Include(x => x.AcademicInformation)
                                                           .ThenInclude(x => x.Department)
                                                       .Include(x => x.CurriculumInformations)
                                                           .ThenInclude(x => x.CurriculumVersion)
                                                       .Include(x => x.Title)
                                                       .Include(x => x.GraduationInformations)
                                                       .Include(x => x.GraduatingRequest)
                           //let requests = _db.GraduatingRequests.Where(x => x.StudentId == student.Id)
                           where student.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                 && (student.GraduationInformations == null
                                     || student.GraduationInformations.All(y => y.TermId == null))
                                 && (student.CurriculumInformations.All(x => x.CurriculumVersion != null
                                                                             && student.AcademicInformation.CreditComp >= (x.CurriculumVersion.ExpectCredit ?? 0)))
                                 && (criteria.FacultyId == 0 
                                     || student.AcademicInformation.FacultyId == criteria.FacultyId)
                                 && (criteria.DepartmentId == 0 
                                     || student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                 && (criteria.CurriculumId == 0
                                     || student.CurriculumInformations.Any(x => x.CurriculumVersion != null 
                                                                                && x.CurriculumVersion.CurriculumId == criteria.CurriculumId))
                                 && (criteria.CurriculumVersionId == 0
                                     || student.CurriculumInformations.Any(y => y.CurriculumVersionId == criteria.CurriculumVersionId))
                                //  && (criteria.ExpectedGraduationTermId == 0
                                //      || _db.GraduatingRequests.Any(x => x.StudentId == student.Id 
                                //                                         && x.GraduatedTermId == criteria.ExpectedGraduationTermId))
                                 && ((criteria.StartStudentBatch ?? 0) == 0
                                     || student.AcademicInformation.Batch == criteria.StartStudentBatch)
                                 && (string.IsNullOrEmpty(criteria.Code)
                                     || student.Code.StartsWith(criteria.Code))
                                 && (criteria.ExpectedGraduationYear == null
                                     || student.GraduatingRequest.ExpectedAcademicYear == criteria.ExpectedGraduationYear)
                                 && (criteria.ExpectedGraduationTerm == null
                                     || student.GraduatingRequest.ExpectedAcademicTerm == criteria.ExpectedGraduationTerm)
                           orderby student.Code
                           select new StudentExpectedGraduateReport
                                  {
                                      StudentId = student.Id,
                                      Code = student.Code,
                                      TitleEn = student.Title.NameEn,
                                      TitleTh = student.Title.NameTh,
                                      FirstNameEn = student.FirstNameEn,
                                      LastNameEn = student.LastNameEn,
                                      MidNameEn = student.MidNameEn,
                                      FirstNameTh = student.FirstNameTh,
                                      LastNameTh = student.LastNameTh,
                                      MidNameTh = student.MidNameTh,
                                      FacultyName = student.AcademicInformation.Faculty.NameEn,
                                      DepartmentName = student.AcademicInformation.Department.NameEn,
                                      FacultyNameTh = student.AcademicInformation.Faculty.NameTh,
                                      DepartmentNameTh = student.AcademicInformation.Department.NameTh,
                                      CurriculumVersionName = student.CurriculumInformations.FirstOrDefault().CurriculumVersion.NameEn,
                                      Credit = student.AcademicInformation.CreditComp,
                                      GPA = student.AcademicInformation.GPA,
                                      StudentStatusText = student.StudentStatusText,
                                      IsRequested = false,
                                      RequestedAt = null,
                                      //IsRequested = requests != null && requests.Any(),
                                      //RequestedAt = requests == null || !requests.Any() ? (DateTime?)null : requests.Max(x => x.CreatedAt)
                                  }).ToList();

            var studentId = results.Select(x => x.StudentId).ToList();

            var requestList = (from request in _db.GraduatingRequests.AsNoTracking()
                               where studentId.Contains(request.StudentId)
                               group request by request.StudentId into g
                               select new
                               {
                                   StudentId = g.Key,
                                   RequestedAt = g.Max(x => x.CreatedAt),
                               }
                              ).ToList();
            foreach (var item in results)
            {
                var request = requestList.FirstOrDefault(x => x.StudentId == item.StudentId);
                if (request != null)
                {
                    item.IsRequested = true;
                    item.RequestedAt = request.RequestedAt;
                }
            }
            if (!string.IsNullOrEmpty(criteria.IsRequested))
            {
                results = results.Where(x => Convert.ToBoolean(criteria.IsRequested) == x.IsRequested).ToList();
            }

            var model = new StudentExpectedGraduateReportViewModel
                        {
                            Criteria = criteria,
                            Results = results
                        };

            return View(model);
        }

        private void CreateSelectList(long academicLevelId, long facultyId, long curriculumId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
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

                ViewBag.ExpectedGraduationTerms = _selectListProvider.GetExpectedGraduationTerms(academicLevelId);
                ViewBag.ExpectedGraduationYears = _selectListProvider.GetExpectedGraduationYears(academicLevelId);
            }
        }
    }
}