using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class WithdrawalReportController : BaseController
    {
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IWithdrawalProvider _withdrawalProvider;

        public WithdrawalReportController(ApplicationDbContext db,
                                          IFlashMessage flashMessage,
                                          ISelectListProvider selectListProvider,
                                          IWithdrawalProvider withdrawalProvider,
                                          IAcademicProvider academicProvider) : base(db, flashMessage, selectListProvider)
        {
            _academicProvider = academicProvider;
            _withdrawalProvider = withdrawalProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId);
            var model = new WithdrawalReportViewModel();
            if (criteria.TermId != 0)
            {
                model = _withdrawalProvider.GetWithdrawalReport(criteria);
            }

            return View(model);
        }
        
        [HttpPost]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult ExportExcel(Criteria criteria, string returnUrl)
        {
            var results = _withdrawalProvider.GetWithdrawalReport(criteria);
            if (results != null)
            {
                using (var wb = GenerateWorkBook(results))
                {
                    var group = criteria.GroupWithdrawBy.Contains("s") ? "Student" : "Course";
                    return wb.Deliver($"Withdrawal Student List Group By { group }.xlsx");
                }
            }

            return Redirect(returnUrl);
        }

        private XLWorkbook GenerateWorkBook(WithdrawalReportViewModel result)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;

            ws.Cell(row, column).Value = "Trimester: " + result.Term;
            ws.Range(ws.Cell(row, column), ws.Cell(row, column + 8)).Merge();
            row++;
            column = 1;

            ws.Cell(row, column).Value = "*** D = Debarment, A = Application, P = Petition" + "; Printed At: " + DateTime.UtcNow.AddHours(7).ToString(StringFormat.ShortDateTime);
            ws.Range(ws.Cell(row, column), ws.Cell(row, column + 8)).Merge();
            row++;
            column = 1;

            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Value = "#";
            ws.Range(ws.Cell(row, column), ws.Cell(row + 1, column)).Merge();
            column++;

            if (result.Criteria.GroupWithdrawBy.Contains("s"))
            {
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column).Value = "Student ID";
                ws.Range(ws.Cell(row, column), ws.Cell(row + 1, column)).Merge();
                column++;
            }
            else
            {
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column).Value = "Course Code";
                ws.Range(ws.Cell(row, column), ws.Cell(row + 1, column)).Merge();
                column++;
            }

            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell(row, column).Value = "Name";
            ws.Range(ws.Cell(row, column), ws.Cell(row + 1, column)).Merge();
            column++;

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell(row, column).Value = "Section";
            ws.Range(ws.Cell(row, column), ws.Cell(row + 1, column)).Merge();
            column++;

            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell(row, column).Value = "Instructor";
            ws.Range(ws.Cell(row, column), ws.Cell(row + 1, column)).Merge();
            column++;

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Value = "Type";
            ws.Range(ws.Cell(row, column), ws.Cell(row, column + 2)).Merge();
            column+=3;

            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell(row, column).Value = "Remark";
            ws.Range(ws.Cell(row, column), ws.Cell(row + 1, column)).Merge();

            row++;
            column = 6;

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "D";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "A";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "P";
            row++;
            if (result.Criteria.GroupWithdrawBy.Contains("s"))
            {
                foreach(var item in result.WithdrawalReportByStudents)
                {
                    column = 1;
                    ws.Cell(row, column).Value = $"{ item.DepartmentName } { item.StudentCodeAndName }";
                    ws.Range(ws.Cell(row, column), ws.Cell(row, column + 8)).Merge();
                    row++;
                    var index = 1;
                    foreach(var withdrawal in item.Withdrawals)
                    {
                        column = 1;
                        ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(row, column++).Value = index++;

                        ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(row, column++).Value = withdrawal.CourseCode;

                        ws.Cell(row, column++).Value = withdrawal.CourseName;

                        ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(row, column++).Value = withdrawal.SectionNumber;
                        ws.Cell(row, column++).Value = withdrawal.InstructorFullName;
                        if (withdrawal.Type == "d")
                        {
                            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(row, column++).Value ="W";
                            column++;
                            ws.Cell(row, ++column).Value = string.IsNullOrEmpty(withdrawal.Remark) ? $"{ withdrawal.InstructorCode }: { withdrawal.InstructorFullName }"
                                                                                                   : $"{ withdrawal.InstructorCode }: { withdrawal.InstructorFullName }, { withdrawal.Remark }";


                        }
                        else if (withdrawal.Type == "u")
                        {
                            ws.Cell(row, ++column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(row, column++).Value ="W";
                            ws.Cell(row, ++column).Value = $"{ withdrawal.Remark }";
                        }
                        else if (withdrawal.Type == "p")
                        {
                            column++;
                            column++;
                            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(row, column++).Value ="W";
                            ws.Cell(row, column).Value = $"{ withdrawal.Remark }";
                        }
                        row++;
                    }
                }
            }
            else
            {
                foreach(var item in result.WithdrawalReportByCourses)
                {
                    column = 1;
                    ws.Cell(row, column).Value = $"{ item.CourseCodeAndName }";
                    ws.Range(ws.Cell(row, column), ws.Cell(row, column + 8)).Merge();
                    row++;
                    var index = 1;
                    foreach(var withdrawal in item.Withdrawals)
                    {
                        column = 1;
                            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(row, column++).Value = index++;

                            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(row, column++).Value = withdrawal.StudentCode;

                        ws.Cell(row, column++).Value = withdrawal.StudentFullName;

                        ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(row, column++).Value = withdrawal.SectionNumber;

                        ws.Cell(row, column++).Value = withdrawal.InstructorFullName;
                        if (withdrawal.Type == "d")
                        {
                            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(row, column++).Value ="W";
                            column++;
                            ws.Cell(row, ++column).Value = string.IsNullOrEmpty(withdrawal.Remark) ? $"{ withdrawal.InstructorCode }: { withdrawal.InstructorFullName }"
                                                                                                   : $"{ withdrawal.InstructorCode }: { withdrawal.InstructorFullName }, { withdrawal.Remark }";


                        }
                        else if (withdrawal.Type == "u")
                        {
                            ws.Cell(row, ++column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(row, column++).Value ="W";
                            ws.Cell(row, ++column).Value = $"{ withdrawal.Remark }";
                        }
                        else if (withdrawal.Type == "p")
                        {
                            column++;
                            column++;
                            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(row, column++).Value ="W";
                            ws.Cell(row, column).Value = $"{ withdrawal.Remark }";
                        }
                        row++;
                    }
                }
            }

            return wb;
        }

        public void CreateSelectList(long academicLevelId = 0, long termId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }

            if (termId != 0)
            {
                ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);
            }
        }
    }
}