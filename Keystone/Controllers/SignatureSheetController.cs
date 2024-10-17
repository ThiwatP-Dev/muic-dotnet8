using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Models;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("SignatureSheet", "")]
    public class SignatureSheetController : BaseController
    {
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        public SignatureSheetController(ApplicationDbContext db,
                                        ISelectListProvider selectListProvider,
                                        IFlashMessage flashMessage,
                                        IAcademicProvider academicProvider,
                                        IRegistrationProvider registrationProvider) : base(db, flashMessage, selectListProvider)
        {
            _academicProvider = academicProvider;
            _registrationProvider = registrationProvider;
        }

        public IActionResult Index(Criteria criteria, string actionType)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.CourseId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0 || criteria.CourseId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            List<SignatureSheetDetail> models = GetStudentListForSignatureSheet(criteria);

            var result = new SignatureSheetViewModel
            {
                Criteria = criteria,
                SignatureSheetDetails = models
            };

            if (!string.IsNullOrEmpty(actionType) && actionType == "Export")
            {
                if (result != null && result.SignatureSheetDetails != null && result.SignatureSheetDetails.Any())
                {
                    var fileName = $"Signature Sheet Of {result.SignatureSheetDetails[0].CourseCode} Of {result.SignatureSheetDetails[0].AcademicYear}-{(result.SignatureSheetDetails[0].AcademicYear + 1)} Trimester {result.SignatureSheetDetails[0].AcademicTerm}.xlsx";
                    foreach (var c in System.IO.Path.GetInvalidFileNameChars())
                    {
                        fileName = fileName.Replace(c, '_');
                    }
                    ViewData["fileName"] = fileName;

                    using (var wb = GenerateWorkBook(result))
                    {
                        return wb.Deliver(fileName);
                    }
                }
                else
                {
                    _flashMessage.Warning("Problem export Excel");
                    return View(result);
                }
            }
            else
            {
                return View(result);
            }
        }

        private List<SignatureSheetDetail> GetStudentListForSignatureSheet(Criteria criteria)
        {
            var models = _db.Sections.AsNoTracking()
                                     .Include(x => x.Course)
                                     .Where(x => x.TermId == criteria.TermId
                                                && criteria.CourseId == x.CourseId
                                                && (!criteria.SectionIds.Any()
                                                    || criteria.SectionIds == null
                                                    || criteria.SectionIds.Contains(x.Id)))
                                     .Select(x => new SignatureSheetDetail
                                     {
                                         SectionId = x.Id,
                                         CourseCode = x.Course.Code,
                                         CourseName = x.Course.NameEn,
                                         AcademicYear = x.Term.AcademicYear,
                                         AcademicTerm = x.Term.AcademicTerm,
                                         SubjectCodeAndName = x.Course.CourseAndCredit,
                                         Credit = x.Course.CreditText,
                                         SectionNumber = x.Number,
                                         OpenedDate = x.OpenedAtText,
                                         IntructorFullNameEn = x.MainInstructor.Title.NameEn + " " + x.MainInstructor.FirstNameEn + " " + x.MainInstructor.LastNameEn
                                     })
                                     .OrderBy(x => x.SectionNumber)
                                     .ToList();
            var sectionIds = models.Select(x => (long?)x.SectionId).ToList();

            var jointSections = _db.Sections.Where(x => sectionIds.Contains(x.ParentSectionId))
                                            .Select(x => new
                                            {
                                                SectionId = x.Id,
                                                ParentSectionId = (long)x.ParentSectionId
                                            })
                                            .ToList();

            foreach (var item in models)
            {
                var students = _registrationProvider.CallUSparkAPIGetStudentsBySectionId(item.SectionId);
                var studentList = _registrationProvider.GetSignatureSheetStudentListsByStudents(students, item.SectionId);
                item.Students.AddRange(studentList);
                var jointSectionIds = jointSections.Where(x => x.ParentSectionId == item.SectionId).Select(x => x.SectionId).ToList();
                foreach (var jointId in jointSectionIds)
                {
                    students = _registrationProvider.CallUSparkAPIGetStudentsBySectionId(jointId);
                    studentList = _registrationProvider.GetSignatureSheetStudentListsByStudents(students, jointId);
                    if (students.Any())
                    {
                        item.Students.AddRange(studentList);
                    }
                }

                if (!criteria.IsShowWithdrawStudent)
                {
                    item.Students = item.Students.Where(x => string.IsNullOrEmpty(x.WithdrawnStatus)).ToList();
                }
                if (!criteria.IsShowUnpaidStudent)
                {
                    item.Students = item.Students.Where(x => "Paid".Equals(x.PaidStatus)).ToList();
                }
            }

            if (criteria.SortBy == "n")
            {
                models.Select(x =>
                {
                    x.Students = x.Students.OrderBy(y => y.FirstName).ToList();
                    return x;
                })
                     .ToList();
            }
            else
            {
                models.Select(x =>
                {
                    x.Students = x.Students.OrderBy(y => y.Code).ToList();
                    return x;
                })
                     .ToList();
            }

            return models;
        }

        private XLWorkbook GenerateWorkBook(SignatureSheetViewModel result)
        {
            var wb = new XLWorkbook();

            wb.PageOptions.PaperSize = XLPaperSize.A4Paper;
            wb.PageOptions.FitToPages(1, 0);
            double rowHeight = 23.5;

            var ws = wb.AddWorksheet();
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.FitToPages(1, 0);
            int row = 1;
            var column = 1;



            foreach (var item in result.SignatureSheetDetails)
            {
                var studentCount = 1;
                for (var i = 0; i < item.PageCount; i++)
                {
                    //Header 
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell(row, column).Style.Font.Bold = true;
                    ws.Cell(row, column).Style.Alignment.WrapText = false;
                    ws.Cell(row, column).Value = "International College, Mahidol University";
                    ws.Cell(row, column).Style.Font.FontSize += 2;
                    ws.Range(ws.Cell(row, 1), ws.Cell(row, 5)).Merge();

                    column = 6;
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(row, column).Style.Font.Bold = true;
                    ws.Cell(row, column).Style.Alignment.WrapText = false;
                    ws.Cell(row, column).Style.Font.FontSize += 2;
                    ws.Cell(row, column).Value = $"Generated: {item.GeneratedDate}";

                    ws.Row(row).Height = rowHeight;
                    row++;
                    column = 1;

                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell(row, column).Style.Font.Bold = true;
                    ws.Cell(row, column).Style.Alignment.WrapText = false;
                    ws.Cell(row, column).Style.Font.FontSize += 2;
                    ws.Cell(row, column).Value = $"Class List for Instructor : {item.IntructorFullNameEn}";
                    ws.Range(ws.Cell(row, 1), ws.Cell(row, 5)).Merge();

                    column = 6;
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(row, column).Style.Font.Bold = true;
                    ws.Cell(row, column).Style.Alignment.WrapText = false;
                    ws.Cell(row, column).Style.Font.FontSize += 2;
                    ws.Cell(row, column).Value = $"(SIGNATURE SHEET)";

                    ws.Row(row).Height = rowHeight;
                    row++;
                    column = 1;

                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell(row, column).Style.Font.Bold = true;
                    ws.Cell(row, column).Style.Alignment.WrapText = false;
                    ws.Cell(row, column).Value = $"{item.AcademicYear} - {(item.AcademicYear + 1)} : Trimester {item.AcademicTerm}";
                    ws.Cell(row, column).Style.Font.FontSize += 2;
                    ws.Range(ws.Cell(row, 1), ws.Cell(row, 5)).Merge();

                    column = 6;
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(row, column).Style.Font.Bold = true;
                    ws.Cell(row, column).Style.Alignment.WrapText = false;
                    ws.Cell(row, column).Style.Font.FontSize += 2;
                    ws.Cell(row, column).Value = $"Credit {item.Credit}";

                    ws.Row(row).Height = rowHeight;
                    row++;
                    column = 1;

                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell(row, column).Style.Font.Bold = true;
                    ws.Cell(row, column).Style.Alignment.WrapText = false;
                    ws.Cell(row, column).Value = $"{item.SubjectCodeAndName}, Section {item.SectionNumber}";
                    ws.Cell(row, column).Style.Font.FontSize += 2;
                    ws.Range(ws.Cell(row, 1), ws.Cell(row, 5)).Merge();

                    ws.Row(row).Height = rowHeight;
                    row++;
                    column = 1;

                    row++;

                    //Body table Header
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column).Style.Font.Bold = true;
                    ws.Cell(row, column).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                    ws.Cell(row, column++).Value = $"#";

                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column).Style.Font.Bold = true;
                    ws.Cell(row, column).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                    ws.Cell(row, column++).Value = $"Student ID";

                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column).Style.Font.Bold = true;
                    ws.Cell(row, column).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                    ws.Cell(row, column++).Value = $"Major";

                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell(row, column).Style.Font.Bold = true;
                    ws.Cell(row, column).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                    ws.Cell(row, column++).Value = $"Student Name";

                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column).Style.Font.Bold = true;
                    ws.Cell(row, column).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                    ws.Cell(row, column++).Value = $"Status";

                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column).Style.Font.Bold = true;
                    ws.Cell(row, column).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                    ws.Cell(row, column++).Value = $"Signature";

                    ws.Row(row).Height = rowHeight;
                    row++;
                    column = 1;

                    //Body Data
                    foreach (var data in item.Students.Skip(i * 25).Take(25).ToList())
                    {
                        ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell(row, column).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                        ws.Cell(row, column++).Value = $"{studentCount}";

                        ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell(row, column).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                        ws.Cell(row, column++).Value = $"{data.Code}";

                        ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell(row, column).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                        ws.Cell(row, column++).Value = $"{data.Department} | {data.CourseCode}";

                        ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell(row, column).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                        ws.Cell(row, column++).Value = $"{data.Name}";

                        ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell(row, column).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                        ws.Cell(row, column++).Value = $"{( string.IsNullOrEmpty(data.WithdrawnStatus) ? (result.Criteria.IsShowPaymentStatus ? data.PaidStatus : "") : data.WithdrawnStatus)}";

                        ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell(row, column).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                        ws.Cell(row, column++).Value = $"";

                        studentCount++;

                        ws.Row(row).Height = rowHeight;
                        row++;
                        column = 1;
                    }

                    if(result.Criteria.IsShowPaymentStatus)
                    {
                        ws.Row(row).Height = rowHeight;
                        row++;
                        column = 1;

                        ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell(row, column).Style.Font.Bold = true;
                        ws.Cell(row, column).Value = $"Status";
                        ws.Range(ws.Cell(row, 1), ws.Cell(row, 5)).Merge();
                        ws.Row(row).Height = rowHeight;
                        row++;

                        ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell(row, column).Value = $"Paid - Students have paid their tuition fees.";
                        ws.Range(ws.Cell(row, 1), ws.Cell(row, 5)).Merge();
                        ws.Row(row).Height = rowHeight;
                        row++;

                        ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell(row, column).Value = $"Unpaid - Students have NOT paid their fees.";
                        ws.Range(ws.Cell(row, 1), ws.Cell(row, 5)).Merge();
                        ws.Row(row).Height = rowHeight;
                        row++;
                    }

                    ws.Row(row).Height = rowHeight;
                    ws.Row(row++).AddHorizontalPageBreak();
                }
            }

            ws.Column(1).Width = 3.57;
            ws.Column(2).Width = 9.57;
            ws.Column(3).Width = 14.3;
            ws.Column(4).Width = 35;
            ws.Column(5).Width = 10.71;
            ws.Column(6).Width = 27.86;


            return wb;
        }

        public void CreateSelectList(long academicLevelId = 0, long termId = 0, long courseId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.SortBy = _selectListProvider.GetSortByForSignatureSheet();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            }

            if (termId > 0)
            {
                ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);
                ViewBag.Sections = _selectListProvider.GetSections(termId, courseId);
            }
        }
    }
}