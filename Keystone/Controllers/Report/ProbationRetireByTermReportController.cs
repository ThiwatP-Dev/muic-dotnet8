using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("ProbationRetireByTermReport", "")]
    public class ProbationRetireByTermReportController : BaseController
    {
        protected readonly IMasterProvider _masterProvider;
        protected readonly IStudentProvider _studentProvider;
        protected readonly ICacheProvider _cacheProvider;

        public ProbationRetireByTermReportController(ApplicationDbContext db, 
                                                     IFlashMessage flashMessage,
                                                     IMapper mapper,
                                                     ISelectListProvider selectListProvider,
                                                     IMasterProvider masterProvider,
                                                     IStudentProvider studentProvider,
                                                     ICacheProvider cacheProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _masterProvider = masterProvider;
            _studentProvider = studentProvider;
            _cacheProvider = cacheProvider;
        }

        public IActionResult Index(Criteria criteria, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = new StudentProbationViewModel
                        {
                            Criteria = criteria
                        };

            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            model = _studentProvider.GetStudentProbation(criteria);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult ExportExcel(StudentProbationViewModel model, string returnUrl)
        {
            if (model.Students != null && model.Students.Any())
            {
                using (var wb = GenerateWorkBook(model))
                {
                    return wb.Deliver($"Probation Retire By Term Report.xlsx");
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
            ws.Cell(row, column++).Value = "Code";
            ws.Cell(row, column++).Value = "Name";
            ws.Cell(row, column++).Value = "Major";
            ws.Cell(row, column++).Value = "Curriculum";
            ws.Cell(row, column++).Value = "Advisor";
            ws.Cell(row, column++).Value = "CGPA";
            for (var i = 0; i < model.Terms.Count; i++)
            {
                var termText = $"{ model.Terms[i].AcademicTerm }/{ model.Terms[i].AcademicYear }";
                if (i < model.Terms.Count - 1)
                {
                    ws.Cell(row, column++).Value = termText;
                }
                else
                {
                    ws.Cell(row++, column++).Value = termText;
                }
            }

            foreach (var item in model.Students)
            {
                var fullName = string.IsNullOrEmpty(item.StudentMidName) ? $"{ item.StudentTitle } { item.StudentFirstName } { item.StudentLastName }"
                                                                         : $"{ item.StudentTitle } { item.StudentFirstName } { item.StudentMidName } { item.StudentLastName }";

                var studentGPA = String.Format("{0:N2}",(Math.Truncate(item.StudentGPA * 100) / 100));
                column = 1;

                ws.Cell(row, column).SetValue<string>(item.StudentCode);
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = fullName;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.DepartmentCode;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.CurriculumVersionNameEn;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.AdvisorName;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = studentGPA;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                foreach (var probation in item.TermGPAs)
                {
                    ws.Cell(row, column).Value = probation.DisplayText;
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    column++;
                }
                
                row += 1;
            }

            return wb;
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