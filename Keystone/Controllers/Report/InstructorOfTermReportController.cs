using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("InstructorOfTermReport", "")]
    public class InstructorOfTermReportController : BaseController
    {
        protected readonly IInstructorProvider _instructorProvider;
        protected readonly ICacheProvider _cacheProvider;

        public InstructorOfTermReportController(ApplicationDbContext db,
                                    IFlashMessage flashMessage,
                                    ISelectListProvider selectListProvider,
                                    IInstructorProvider instructorProvider,
                                    ICacheProvider cacheProvider,
                                    IMapper mapper) : base(db, flashMessage, mapper, selectListProvider)
        {
            _instructorProvider = instructorProvider;
            _cacheProvider = cacheProvider;
        }

        public ActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria);

            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                criteria.AcademicLevelId = _db.AcademicLevels.SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
                criteria.TermId = _cacheProvider.GetCurrentTerm(criteria.AcademicLevelId).Id;
                CreateSelectList(criteria);
                return View(new PagedResult<InstructorViewModel>()
                {
                    Criteria = criteria
                });
            }

            var instructors = _instructorProvider.GetInstructors(criteria)
                                                 .OrderBy(x => x.Code)
                                                 .Select(x => new InstructorViewModel
                                                 {
                                                     Code = x.Code,
                                                     DepartmentCode = x.DepartmentCode,
                                                     DepartmentNameEn = x.Department,
                                                     DepartmentNameTh = x.Department,
                                                     Email = x.Email,
                                                     FacultyNameEn = x.Faculty,
                                                     FacultyNameTh = x.Faculty,
                                                     FullNameEn = x.FullNameEn,
                                                     FullNameTh = x.FullNameTh,
                                                     Id = x.Id,
                                                     IsActive = x.IsActive,
                                                     ProfileImageURL = x.ProfileImageURL,
                                                     Type = x.Type
                                                 })
                                                 .ToList();
            var displayInstructor = instructors.GetPaged(criteria, page);
            if(displayInstructor.Results != null && displayInstructor.Results.Any())
            {
                var emailInstructors  = instructors.Select(x => x.Email).ToList();
                displayInstructor.Criteria.InstructorEmails = string.Join(";", emailInstructors);
            }

            return View(displayInstructor);
        }

        [HttpPost]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult ExportExcel(Criteria criteria, string returnUrl)
        {
            var results = _instructorProvider.GetInstructors(criteria);
            if (results != null && results.Any())
            {
                using (var wb = GenerateWorkBook(results))
                {
                    return wb.Deliver($"Instructor Report.xlsx");
                }
            }

            return Redirect(returnUrl);
        }

        private XLWorkbook GenerateWorkBook(List<InstructorInfoViewModel> results)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "INSTRUCTOR ID";

            ws.Cell(row, column++).Value = "NAME EN";
            ws.Cell(row, column++).Value = "DIVISION";
            ws.Cell(row, column++).Value = "MAJOR";
            ws.Cell(row, column++).Value = "EMAIL";
            ws.Cell(row, column++).Value = "TYPE";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "STATUS";

            foreach (var item in results)
            {
                column = 1;
                row++;
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).SetValue<string>(item.Code);

                ws.Cell(row, column++).Value = item.FullNameEn;
                ws.Cell(row, column++).Value = item.Faculty;
                ws.Cell(row, column++).Value = item.Department;
                ws.Cell(row, column++).Value = item.Email;

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.Type;

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.IsActive ? "Active" : "Inactive";
            }

            return wb;
        }
       
        private void CreateSelectList(Criteria criteria)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (criteria.AcademicLevelId > 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(criteria.AcademicLevelId);
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(criteria.AcademicLevelId);
            }
            ViewBag.Statuses = _selectListProvider.GetActiveStatuses();
            ViewBag.Faculties = _selectListProvider.GetFaculties();
            ViewBag.InstructorTypes = _selectListProvider.GetInstructorTypes();
            ViewBag.InstructorRankings = _selectListProvider.GetInstructorRankings();
            if (criteria.FacultyId != 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartments(criteria.FacultyId);
            }
        }

    }
}