using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    public class CheatingStatisticReportController : BaseController
    {
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IStudentProvider _studentProvider;

        public CheatingStatisticReportController(ApplicationDbContext db,
                                                 ISelectListProvider selectListProvider,
                                                 IFlashMessage flashMessage,
                                                 IMapper mapper,
                                                 IAcademicProvider academicProvider,
                                                 IStudentProvider studentProvider) : base(db, flashMessage, mapper, selectListProvider) 
        {
            _academicProvider = academicProvider;
            _studentProvider = studentProvider;
        }

        public ActionResult Index(int page, Criteria criteria) 
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            var model = new List<CheatingStatisticReportViewModel>();
            if (criteria.AcademicLevelId == 0 || string.IsNullOrEmpty(criteria.Type) 
                || (criteria.Type == "b" && (criteria.StartStudentBatch == null || criteria.EndStudentBatch == null)))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var startTerm = _academicProvider.GetTerm(criteria.StartTermId);
            var endTerm = _academicProvider.GetTerm(criteria.EndTermId);
            var term = _db.CheatingStatuses.Where(x => ((startTerm == null
                                                        && endTerm == null)
                                                        || (startTerm == null
                                                            || (x.Term.AcademicYear == startTerm.AcademicYear
                                                          && x.Term.AcademicTerm >= startTerm.AcademicTerm)
                                                          || x.Term.AcademicYear > startTerm.AcademicYear)
                                                        && (endTerm == null
                                                            || (x.Term.AcademicYear == endTerm.AcademicYear
                                                                && x.Term.AcademicTerm <= endTerm.AcademicTerm)
                                                                || x.Term.AcademicYear < endTerm.AcademicYear)))
                                           .Select(x => x.Term)
                                           .Distinct()
                                           .OrderBy(x => x.AcademicYear)
                                               .ThenBy(x => x.AcademicTerm)
                                           .ToList();
            
            var termIds = term.Select(x => x.Id).ToList();
            startTerm = startTerm == null ? term.FirstOrDefault() : startTerm;
            endTerm = endTerm == null ? term.LastOrDefault() : endTerm;
            var cheating = _db.CheatingStatuses.Include(x => x.Incident)
                                               .Include(x => x.RegistrationCourse)
                                                   .ThenInclude(x => x.Section)
                                                       .ThenInclude(x => x.Course)
                                               .Include(x => x.Term)
                                               .Include(x => x.ExaminationType)
                                               .Include(x => x.Student)
                                                   .ThenInclude(x => x.AcademicInformation)
                                                       .ThenInclude(x => x.Faculty)
                                               .Where(x => termIds.Contains(x.TermId)
                                                           && (criteria.CourseId == 0
                                                               || x.RegistrationCourse.CourseId == criteria.CourseId)
                                                           && (string.IsNullOrEmpty(criteria.ExaminationType)
                                                               || x.ExaminationTypeId == criteria.ExaminationTypeId)
                                                           && (criteria.FacultyId == 0
                                                               || x.Student.AcademicInformation.FacultyId == criteria.FacultyId)
                                                           && (criteria.DepartmentId == 0
                                                               || x.Student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                           && (criteria.EndStudentBatch == null
                                                               || criteria.EndStudentBatch == 0
                                                               || criteria.EndStudentBatch >= x.Student.AcademicInformation.Batch)
                                                           && (criteria.StudentCodeFrom == null
                                                               || (criteria.StudentCodeTo == null ? x.Student.CodeInt == criteria.StudentCodeFrom : x.Student.CodeInt >= criteria.StudentCodeFrom))
                                                           && (criteria.StudentCodeTo == null
                                                                || (criteria.StudentCodeFrom == null ? x.Student.CodeInt == criteria.StudentCodeTo : x.Student.CodeInt <= criteria.StudentCodeTo)))
                                               .ToList();

            if (criteria.Type == "f")
            {
                var studentCount = cheating.GroupBy(x => x.Student.AcademicInformation.FacultyId)
                                           .Select(x => new CheatingStatisticReportViewModel
                                                        {
                                                            Faculty = x.FirstOrDefault().Student.AcademicInformation.Faculty.NameEn,
                                                            TermHeader = term,
                                                            TermText = startTerm?.TermText + " - " + endTerm?.TermText,
                                                            StatisticCheatingFaculties = x.GroupBy(y => y.Term.TermText)
                                                                                          .Select(y => new CheatingStatisticReportCount
                                                                                                       {
                                                                                                           Term = y.Key,
                                                                                                           StudentCount = y.Select(z => z.StudentId)
                                                                                                                           .Distinct()
                                                                                                                           .Count()
                                                                                                       })
                                                                                          .OrderBy(y => y.Term)
                                                                                          .ToList()
                                                        })
                                                        .ToList();

                model.AddRange(studentCount);
            }
            else if (criteria.Type == "b")
            {
                var beforeBatch = cheating.Where(x => x.Student.AcademicInformation.Batch < criteria.StartStudentBatch)
                                          .ToList();
                
                model.Add(new CheatingStatisticReportViewModel
                          {
                              Batch = "Before " + criteria.StartStudentBatch,
                              TermHeader = term,
                              TermText = startTerm?.TermText + " - " + endTerm?.TermText,
                              StatisticCheatingBatches = beforeBatch.GroupBy(x => x.Term.TermText)
                                                                    .Select(x => new CheatingStatisticReportCount
                                                                                 {
                                                                                     Term = x.Key,
                                                                                     StudentCount = x.Select(y => y.StudentId)
                                                                                                     .Distinct()
                                                                                                     .Count()
                                                                                 })
                                                                    .OrderBy(x => x.Term)
                                                                    .ToList()
                          });

                var studentCount = cheating.Where(x => x.Student.AcademicInformation.Batch >= criteria.StartStudentBatch)
                                           .GroupBy(x => x.Student.AcademicInformation.Batch)
                                           .Select(x => new CheatingStatisticReportViewModel
                                                        {
                                                            Batch = x.Key.ToString(),
                                                            TermHeader = term,
                                                            TermText = startTerm?.TermText + " - " + endTerm?.TermText,
                                                            StatisticCheatingBatches = x.GroupBy(y => y.Term.TermText)
                                                                                        .Select(y => new CheatingStatisticReportCount
                                                                                                     {
                                                                                                         Term = y.Key,
                                                                                                         StudentCount = y.Select(z => z.StudentId)
                                                                                                                         .Distinct()
                                                                                                                         .Count()
                                                                                                     })
                                                                                        .OrderBy(y => y.Term)
                                                                                        .ToList()
                                                        })
                                           .OrderBy(x => x.Batch)
                                           .ToList();

                model.AddRange(studentCount);
            }
            else if (criteria.Type == "t")
            {
                var studentCount = cheating.GroupBy(x => x.RegistrationCourse.Course)
                                           .Select(x => new CheatingStatisticReportViewModel
                                                        {
                                                            CourseCode = x.FirstOrDefault().RegistrationCourse.Course.Code,
                                                            CourseName = x.FirstOrDefault().RegistrationCourse.Course.NameEn,
                                                            TermHeader = term,
                                                            TermText = startTerm?.TermText + " - " + endTerm?.TermText,
                                                            StatisticCheatingTerms = x.GroupBy(y => y.Term.TermText)
                                                                                      .Select(y => new CheatingStatisticReportCount
                                                                                                   {
                                                                                                       Term = y.Key,
                                                                                                       StudentCount = y.Select(z => z.StudentId)
                                                                                                                       .Distinct()
                                                                                                                       .Count()
                                                                                                   })
                                                                                      .OrderBy(y => y.Term)
                                                                                      .ToList()
                                                        })
                                                        .ToList();

                model.AddRange(studentCount);
            }

            var cheatingPagedResult = model.AsQueryable()
                                           .GetPaged(criteria, page, true);

            return View(cheatingPagedResult);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.ExaminationTypes = _selectListProvider.GetExaminationTypes();
            ViewBag.ReportTypes = _selectListProvider.GetCheatingStatisticReportTypes();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }

            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            }
        }
    }
}