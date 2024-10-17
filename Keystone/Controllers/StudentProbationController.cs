using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("StudentProbation", "")]
    public class StudentProbationController : BaseController
    {
        protected readonly IMasterProvider _masterProvider;
        protected readonly IStudentProvider _studentProvider;
        protected readonly IEmailProvider _emailProvider;
        protected readonly ICacheProvider _cacheProvider;

        public StudentProbationController(ApplicationDbContext db, 
                                          IFlashMessage flashMessage,
                                          IMapper mapper,
                                          ISelectListProvider selectListProvider,
                                          IMasterProvider masterProvider,
                                          IStudentProvider studentProvider,
                                          IEmailProvider emailProvider,
                                          ICacheProvider cacheProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _masterProvider = masterProvider;
            _studentProvider = studentProvider;
            _emailProvider = emailProvider;
            _cacheProvider = cacheProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var model = new StudentProbationViewModel();
            model.Students = _db.StudentProbations.Include(x => x.Probation)
                                                  .Include(x => x.Retire)
                                                  .Where(x => x.Student.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                              && (criteria.TermId == 0
                                                                  || x.TermId == criteria.TermId)
                                                              && ((criteria.ProbationId == 0
                                                                   && criteria.RetireId == 0
                                                                   && (x.RetireId != null || x.ProbationId != null))
                                                                  || ((criteria.RetireId != 0 || criteria.ProbationId != 0)
                                                                       && (criteria.RetireId == x.RetireId || criteria.ProbationId == x.ProbationId)))
                                                              && (string.IsNullOrEmpty(criteria.Status)
                                                                  || x.Student.StudentStatus == criteria.Status)
                                                              && (criteria.FacultyId == 0
                                                                  || x.Student.AcademicInformation.FacultyId == criteria.FacultyId)
                                                              && (criteria.DepartmentId == 0
                                                                  || x.Student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                              && (string.IsNullOrEmpty(criteria.Code)
                                                                  || x.Student.Code.StartsWith(criteria.Code))
                                                              && (string.IsNullOrEmpty(criteria.FirstName)
                                                                  || (x.Student.FirstNameEn ?? string.Empty).StartsWith(criteria.FirstName)
                                                                  || (x.Student.LastNameEn ?? string.Empty).StartsWith(criteria.FirstName)
                                                                  || (x.Student.FirstNameTh ?? string.Empty).StartsWith(criteria.FirstName)
                                                                  || (x.Student.LastNameTh ?? string.Empty).StartsWith(criteria.FirstName)))
                                                  .Select(x => new StudentProbationDetail
                                                               {
                                                                   StudentProbationId = x.Id,
                                                                   StudentCode = x.Student.Code,
                                                                   StudentTitle = x.Student.Title.NameEn,
                                                                   StudentFirstName = x.Student.FirstNameEn,
                                                                   StudentMidName = x.Student.MidNameEn,
                                                                   StudentLastName = x.Student.LastNameEn,
                                                                   FacultyName = x.Student.AcademicInformation.Faculty.NameEn,
                                                                   DepartmentName = x.Student.AcademicInformation.Department.Code,
                                                                   Credit = x.Student.AcademicInformation.CreditComp,
                                                                   StudentGPA = x.StudentGPA,
                                                                   AcademicTerm = x.Term.AcademicTerm,
                                                                   AcademicYear = x.Term.AcademicYear,
                                                                   ProbationName = x.Probation.Name,
                                                                   RetireName = x.Retire.Name,
                                                                   StudentStatus = x.Student.StudentStatusText
                                                               })
                                                  .OrderBy(x => x.StudentCode)
                                                  .AsNoTracking()
                                                  .IgnoreQueryFilters()
                                                  .ToList();
            return View(model);
        }

        [PermissionAuthorize("StudentProbation", PolicyGenerator.Write)]
        public IActionResult Create(Criteria criteria, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = new StudentProbationViewModel
                        {
                            Criteria = criteria
                        };

            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0 
                || criteria.StartTermId == 0 
                || criteria.EndTermId == 0 
                || criteria.GPAFrom == null 
                || criteria.GPATo == null)
            {
                criteria.Status = "s";
                _flashMessage.Warning(Message.RequiredData);
                return View(new StudentProbationViewModel
                            {
                                Criteria = criteria
                            });
            }

            model = _studentProvider.GetStudentProbation(criteria);
            return View(model);
        }

        [PermissionAuthorize("StudentProbation", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(List<StudentProbationDetail> model)
        {
            if (model == null || model.Count == 0)
            {
                CreateSelectList();
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            var studentId = model.First().StudentId;
            var student = _db.AcademicInformations.SingleOrDefault(x => x.StudentId == studentId);
            var probationId = model.First().ProbationId;
            var retireId = model.First().RetireId;
            var termId = model.First().ProbationTermId;
            
            var students = model.Where(x => x.IsCheck)
                                .Select(x => new StudentProbation
                                             {
                                                 StudentId = x.StudentId,
                                                 TermId = termId,
                                                 StudentGPA = x.StudentGPA,
                                                 ProbationId = x.ProbationId == 0 ? null : x.ProbationId,
                                                 RetireId = x.RetireId == 0 ? null : x.RetireId,
                                                 SendEmail = x.IsSendEmail
                                             })
                                .ToList();
                                
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in students)
                    {
                        if (item.RetireId > 0)
                        {
                            var _student = _db.Students.SingleOrDefault(x => x.Id == item.StudentId);
                            if (_student.StudentStatus != "rt")
                            {
                                _student.StudentStatus = "rt";
                                _student.IsActive = KeystoneLibrary.Providers.StudentProvider.IsActiveFromStudentStatus(_student.StudentStatus);
                                var success = _studentProvider.SaveStudentStatusLog(item.StudentId, termId, "Retire",
                                                                                    string.Empty, _student.StudentStatus);
                                if (!success)
                                {
                                    transaction.Rollback();
                                    CreateSelectList(student.AcademicLevelId);
                                    _flashMessage.Danger(Message.UnableToCreate);
                                    return View(model);
                                }
                            }
                        }

                        if (!_db.StudentProbations.Any(x => x.StudentId == item.StudentId
                                                            && x.TermId == model.First().ProbationTermId))
                        {
                            _db.StudentProbations.Add(item);
                        }
                    }

                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return Json(new { redirectToUrl = Url.Action("Index", "StudentProbation") });
                }
                catch
                {
                    transaction.Rollback();
                    CreateSelectList(student.AcademicLevelId);
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }
        }

        [HttpPost]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult ExportExcel(Criteria criteria, string returnUrl)
        {
            var model = _studentProvider.GetStudentProbation(criteria);
            if (model != null)
            {
                using (var wb = GenerateWorkBook(model))
                {
                    return wb.Deliver($"Student Probation Report.xlsx");
                }
            }

            return Redirect(returnUrl);
        }

        private XLWorkbook GenerateWorkBook(StudentProbationViewModel model)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "CODE";

            ws.Cell(row, column++).Value = "TITLE";
            ws.Cell(row, column++).Value = "NAME";
            ws.Cell(row, column++).Value = "MAJOR";
            ws.Cell(row, column++).Value = "CURRICULUM";
            ws.Cell(row, column++).Value = "ADVISOR TITLE";
            ws.Cell(row, column++).Value = "ADVISOR NAME";

            foreach(var item in model.Terms)
            {
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.TermText;
            }

            foreach (var item in model.Students) 
            {
                column = 1;
                row++;
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).SetValue<string>(item.StudentCode);

                ws.Cell(row, column++).Value = item.StudentTitle;
                ws.Cell(row, column++).Value = item.StudentFullName;

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.DepartmentCode;

                ws.Cell(row, column++).Value = item.CurriculumVersionNameEn;
                ws.Cell(row, column++).Value = item.AdvisorTitle;
                ws.Cell(row, column++).Value = item.AdvisorFullName;

                foreach(var gpa in item.TermGPAs)
                {
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column++).Value = gpa.GPA?.ToString(StringFormat.TwoDecimal);
                }
            }

            ws.Columns().AdjustToContents();
            ws.Rows().AdjustToContents();
            return wb;
        }

        [PermissionAuthorize("StudentProbation", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendEmail(List<Guid> studentIds)
        {
            _emailProvider.SendEmail("krich.mimacha@gmail.com", "KM", "MUIC", "Probation", "Test Test");
            return RedirectToAction(nameof(Create));
        }

        [PermissionAuthorize("StudentProbation", PolicyGenerator.Write)]
        public ActionResult Delete(long id, string returnUrl)
        {
            var model = _studentProvider.GetStudentProbationById(id);
            try
            {
                if (model.RetireId != null)
                {
                    model.Student.StudentStatus = "s";
                    model.Student.IsActive = KeystoneLibrary.Providers.StudentProvider.IsActiveFromStudentStatus(model.Student.StudentStatus);
                }

                _db.StudentProbations.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return Redirect(returnUrl);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Probations = _selectListProvider.GetProbations();
            ViewBag.Retires = _selectListProvider.GetRetires();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            }
            ViewBag.SortBy = _selectListProvider.GetSortByForStudentProbation();
        }
    }
}