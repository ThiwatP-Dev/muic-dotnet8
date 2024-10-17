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
    [PermissionAuthorize("CumulativeGPAReport", "")]
    public class CumulativeGPAReportController : BaseController
    {
        protected readonly ICacheProvider _cacheProvider;

        public CumulativeGPAReportController(ApplicationDbContext db,
                                             ISelectListProvider selectListProvider,
                                             IFlashMessage flashMessage,
                                             ICacheProvider cacheProvider) : base(db, flashMessage, selectListProvider)
        { 
            _cacheProvider = cacheProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyIds);
            var model = new PagedResult<CumulativeGPAReportViewModel>();
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            model = _db.Students.IgnoreQueryFilters()
                                .Where(x => x.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                            && (criteria.FacultyIds == null
                                                || !criteria.FacultyIds.Any()
                                                || criteria.FacultyIds.Contains(x.AcademicInformation.FacultyId))
                                            && (criteria.DepartmentIds == null
                                                || !criteria.DepartmentIds.Any()
                                                || criteria.DepartmentIds.Contains(x.AcademicInformation.DepartmentId ?? 0))
                                            && (criteria.Batches == null
                                                || !criteria.Batches.Any()
                                                || criteria.Batches.Contains(x.AcademicInformation.Batch))
                                            && (criteria.StudentTypeIds == null
                                                || criteria.StudentTypeIds.Contains(x.StudentFeeTypeId))
                                            && (criteria.ResidentTypeIds == null
                                                || criteria.ResidentTypeIds.Contains(x.ResidentTypeId))
                                            && (criteria.GPAFrom == null
                                                || x.AcademicInformation.GPA >= criteria.GPAFrom)
                                            && (criteria.GPATo == null
                                                || x.AcademicInformation.GPA <= criteria.GPATo)
                                            && (criteria.StudentStatuses == null
                                                || !criteria.StudentStatuses.Any()
                                                || criteria.StudentStatuses.Contains(x.StudentStatus)))
                                .Select(x => new CumulativeGPAReportViewModel
                                             {
                                                 StudentId = x.Id,
                                                 StudentCode = x.Code,
                                                 Title = x.Title.NameEn,
                                                 FirstName = x.FirstNameEn,
                                                 MidName = x.MidNameEn,
                                                 LastName = x.LastNameEn,
                                                 Major = x.AcademicInformation.Department.Code,
                                                 CGPA = x.AcademicInformation.GPA,
                                                 CreditEarned = x.AcademicInformation.CreditEarned ?? 0,
                                                 CompleteCredit = x.AcademicInformation.CreditComp,
                                                 StudentType = x.StudentFeeType.NameEn,
                                                 ResidentType = x.ResidentType.NameEn,
                                                 AdmissionType = x.AdmissionInformation.AdmissionType.NameEn,
                                                 StudentStatus = x.StudentStatusText,
                                                 RegistrationTerm = (from registraion in _db.RegistrationCourses
                                                                     join term in _db.Terms on registraion.TermId equals term.Id
                                                                     where registraion.StudentId == x.Id
                                                                     orderby term.TermText descending
                                                                     select term.TermText).FirstOrDefault()
                                             })
                                .OrderBy(x => x.StudentId)
                                .GetPaged(criteria, page, true);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult ExportExcel(List<CumulativeGPAReportViewModel> model, string returnUrl)
        {
            if (model != null && model.Any())
            {
                using (var wb = GenerateWorkBook(model))
                {
                    return wb.Deliver($"Cumulative GPA Report.xlsx");
                }
            }

            return Redirect(returnUrl);
        }

        private XLWorkbook GenerateWorkBook(List<CumulativeGPAReportViewModel> model)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;
            ws.Cell(row, column++).Value = "Code";
            ws.Cell(row, column++).Value = "Name";
            ws.Cell(row, column++).Value = "Major";
            ws.Cell(row, column++).Value = "CGPA";
            ws.Cell(row, column++).Value = "Registration Credit";
            ws.Cell(row, column++).Value = "Completed Credit";
            ws.Cell(row, column++).Value = "Student Fee Type";
            ws.Cell(row, column++).Value = "Resident Type";
            ws.Cell(row, column++).Value = "Admission Type";
            ws.Cell(row, column++).Value = "Student Status";
            ws.Cell(row++, column).Value = "Registration Term";

            foreach (var item in model)
            {
                var fullName = string.IsNullOrEmpty(item.MidName) ? $"{ item.Title } { item.FirstName } { item.LastName }"
                                                                  : $"{ item.Title } { item.FirstName } { item.MidName } { item.LastName }";
                var predefind = string.Empty;
                column = 1;
                ws.Cell(row, column).SetValue<string>(item.StudentCode);
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = fullName;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.Major;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.CGPA;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.CreditEarned;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.CompleteCredit;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.StudentType;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.ResidentType;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.AdmissionType;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.StudentStatus;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.RegistrationTerm;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;
                row += 1;
            }
            
            ws.Columns().AdjustToContents();
            ws.Rows().AdjustToContents();
            return wb;
        }

        public void CreateSelectList(long academicLevelId = 0, List<long> facultyIds = null)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();
            ViewBag.ResidentTypes = _selectListProvider.GetResidentTypes();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.Batches = _selectListProvider.GetBatches();
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByFacultyIds(academicLevelId, facultyIds);
        }
    }
}