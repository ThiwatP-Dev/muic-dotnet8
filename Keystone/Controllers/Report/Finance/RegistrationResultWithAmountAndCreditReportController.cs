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
    [PermissionAuthorize("RegistrationResultWithAmountAndCreditReport", "")]
    public class RegistrationResultWithAmountAndCreditReportController : BaseController
    {
        private IReceiptProvider _receiptProvider;
        public RegistrationResultWithAmountAndCreditReportController(ApplicationDbContext db,
                                                  IFlashMessage flashMessage,
                                                  ISelectListProvider selectListProvider,
                                                  IReceiptProvider receiptProvider) : base(db, flashMessage, selectListProvider)
        {
            _receiptProvider = receiptProvider;
        }

        public IActionResult Index(Criteria criteria, string actionType)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            var model = new RegistrationResultWithAmountAndCreditReportViewModel();
            model.Criteria = criteria;
            model.Criteria.IsConvertScholarAsAnotherFeeType = true;

            var fileName = $"Registration Result of Amount and Credit Report As Of {DateTime.Now.ToShortDateString()}.xlsx";
            foreach (var c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            ViewData["fileName"] = fileName;

            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            model = _receiptProvider.GetRegistrationResultWithAmountAndCreditReport(criteria);

            var scholarshipGroup = new List<long> { KeystoneLibrary.Providers.ReceiptProvider.SPECIAL_FEE_TYPE_ID_SCHOLARSHIP };
            List<KeyValuePair<string, List<long>>> feeColumn = new List<KeyValuePair<string, List<long>>>()
            {
                //From FeeItem
                new KeyValuePair<string, List<long>> ("Tuition" , new List<long>{1}),
                new KeyValuePair<string, List<long>> ("Education Fee" , new List<long>{13}),
                new KeyValuePair<string, List<long>> ("Late Registration Fee" , new List<long>{24}),
                new KeyValuePair<string, List<long>> ("Late Payment Fee" , new List<long>{17}),
                new KeyValuePair<string, List<long>> ("Insurance Fee" , new List<long>{15}),
                new KeyValuePair<string, List<long>> ("Trimester Lump Sum Tution Fees" , new List<long>{7,9,10,11,33}),
                new KeyValuePair<string, List<long>> ("Add/Drop Fee" , new List<long>{21}),
                new KeyValuePair<string, List<long>> ("Scholarship" , scholarshipGroup),
            };

            ViewData["feeColumn"] = feeColumn;

            if (!string.IsNullOrEmpty(actionType) && actionType == "Export")
            {
                if (model != null && model.ReportItems != null)
                {
                    
                    using (var wb = GenerateWorkBook(model, feeColumn, scholarshipGroup))
                    {
                        return wb.Deliver(fileName);
                    }
                }
                else
                {
                    _flashMessage.Warning("Problem export Excel");
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

        private XLWorkbook GenerateWorkBook(RegistrationResultWithAmountAndCreditReportViewModel model, List<KeyValuePair<string, List<long>>> feeColumn, List<long> scholarshipGroup)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;

            //ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //ws.Cell(row, column).Style.Font.Bold = true;
            //ws.Cell(row, column).Style.Font.FontSize = 14;
            //ws.Cell(row, column).Value = "Mahidol University International College";
            //ws.Range(ws.Cell(row, column), ws.Cell(row, column + 20)).Merge();
            //row++;

            //real table header
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "#";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Term";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Division";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Citizen ID";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Code";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Major";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Title";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "F-Name";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "L-Name";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Title (Thai)";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "F-Name (Thai)";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "L-Name (Thai)";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Student Fee Type";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Nationality";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Telephone No.";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Status";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Advisor Title";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Advisor F-Name";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Advisor L-Name";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Type";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Total Subjects";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Credit";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Invoice Number";
            if (model.Criteria?.ReceiptDateFrom.HasValue ?? false)
            {
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(row, column++).Value = "Invoice Date"; 
            }
            foreach(var key in feeColumn)
            {
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, column++).Value = key.Key;
            }
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, column++).Value = "Others";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, column++).Value = "Total Amount";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "First Regist At";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Paid";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Last Payment At";
            row++;

            var index = 1;
            foreach (var item in model.ReportItems)
            {
                column = 1;
                ws.Cell(row, column++).Value = index++;
                ws.Cell(row, column++).Value = "'" + item.Term;
                ws.Cell(row, column++).Value = item.Faculty;
                ws.Cell(row, column).Value = item.StudentCitizenNumber;
                ws.Cell(row, column++).DataType = XLDataType.Text;
                ws.Cell(row, column).Value = item.StudentCode;
                ws.Cell(row, column++).DataType = XLDataType.Text;
                ws.Cell(row, column++).Value = item.Department;
                ws.Cell(row, column++).Value = item.StudentTitleEn;
                ws.Cell(row, column++).Value = item.StudentFirstNameEn;
                ws.Cell(row, column++).Value = item.StudentLastNameEn;
                ws.Cell(row, column++).Value = item.StudentTitleTh;
                ws.Cell(row, column++).Value = item.StudentFirstNameTh;
                ws.Cell(row, column++).Value = item.StudentLastNameTh;
                ws.Cell(row, column++).Value = item.StudentFeeType;
                ws.Cell(row, column++).Value = item.StudentNationality;
                ws.Cell(row, column).Value = item.StudentTelephoneNumber;
                ws.Cell(row, column++).DataType = XLDataType.Text;
                ws.Cell(row, column++).Value = item.StudentStatus;
                ws.Cell(row, column++).Value = item.AdvisorTitle;
                ws.Cell(row, column++).Value = item.AdvisorFirstName;
                ws.Cell(row, column++).Value = item.AdvisorLastName;
                ws.Cell(row, column++).Value = item.InvoiceTypeText;
                ws.Cell(row, column).Style.NumberFormat.Format = "#,##0";
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, column++).Value = item.TotalRelatedCourse;
                ws.Cell(row, column).Style.NumberFormat.Format = "#,##0";
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, column++).Value = item.TotalRelatedCredit;
                ws.Cell(row, column).Value = item.InvoiceNumber;
                ws.Cell(row, column++).DataType = XLDataType.Text;
                if (model.Criteria?.ReceiptDateFrom.HasValue ?? false)
                {
                    ws.Cell(row, column).Style.DateFormat.Format = "dd/mm/yyyy";
                    ws.Cell(row, column++).Value = item.InvoiceDateTime;
                }
                foreach (var key in feeColumn)
                {
                    ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(row, column++).Value = item.Items.Where(x => key.Value.Contains(x.FeeItemId)).Sum(x => x.Amount);
                }
                ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, column++).Value = (item.Items.Where(x => !feeColumn.SelectMany(y => y.Value).Contains(x.FeeItemId)).Sum(x => x.Amount)
                                                    - item.Items.Sum(x => x.DiscountAmount)
                                                    );
                ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, column++).Value = (item.TotalAmount 
                                                    - (item.Items.Where(x => scholarshipGroup.Contains(x.FeeItemId)).Sum(x => x.Amount)));


                ws.Cell(row, column).Style.DateFormat.Format = "dd/mm/yyyy";
                ws.Cell(row, column++).Value = item.FirstRegistrationDate;
                ws.Cell(row, column++).Value = item.IsPaid ? "Paid" : "Unpaid";
                ws.Cell(row, column).Style.DateFormat.Format = "dd/mm/yyyy";
                ws.Cell(row, column++).Value = item.LastPaymentDate;

                row++;
            }

            //Summary Row
            column = 1;
            ws.Cell(row, column).Value = $"Total {model.ReportItems.Count:N0} records";
            ws.Range(ws.Cell(row, column), ws.Cell(row, column + 19)).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            column += 20;

            ws.Cell(row, column).Style.NumberFormat.Format = "#,##0";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, column++).Value = model.ReportItems.Sum(x => x.TotalRelatedCourse);
            ws.Cell(row, column).Style.NumberFormat.Format = "#,##0";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, column++).Value = model.ReportItems.Sum(x => x.TotalRelatedCredit);
            ws.Cell(row, column++).Value = "";
            if (model.Criteria?.ReceiptDateFrom.HasValue ?? false)
            {
                ws.Cell(row, column++).Value = "";
            }
            foreach (var key in feeColumn)
            {
                ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, column++).Value = model.ReportItems.SelectMany(x => x.Items).Where(x => key.Value.Contains(x.FeeItemId)).Sum(x => x.Amount);
            }
            ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, column++).Value = (model.ReportItems.SelectMany(x => x.Items).Where(x => !feeColumn.SelectMany(y => y.Value).Contains(x.FeeItemId)).Sum(x => x.Amount)
                                                            - model.ReportItems.SelectMany(x => x.Items).Sum(x => x.DiscountAmount)
                                                        );
            ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, column++).Value = model.ReportItems.Sum(x => x.TotalAmount
                                                                         - (x.Items.Where(y => scholarshipGroup.Contains(y.FeeItemId)).Sum(y => y.Amount))
                                                                );

            ws.Cell(row, column++).Value = "";
            ws.Cell(row, column++).Value = "";
            ws.Cell(row, column++).Value = "";
            row++;

            ws.Columns(1, 7).AdjustToContents();
            ws.Columns(8, 9).Width = 23;
            ws.Columns(10, 10).AdjustToContents();
            ws.Columns(11, 12).Width = 23;
            ws.Columns(13, 17).AdjustToContents();
            ws.Columns(18, 19).Width = 23;
            ws.Columns(20, 37).AdjustToContents();

            return wb;
        }

        private void CreateSelectList(long academicLevelId, long facultyId)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.PaidStatuses = _selectListProvider.GetPaidStatuses();

            ViewBag.InvoiceType = _selectListProvider.GetInvoiceType();
            ViewBag.InvoiceRefundType = _selectListProvider.GetInvoiceRefundType();
            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();

            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }
            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartments(facultyId);
            }

            ViewBag.ResidentTypes = _selectListProvider.GetResidentTypes();
            ViewBag.AdmissionTypes = _selectListProvider.GetAdmissionTypes();
            ViewBag.StudentFeeGroups = _selectListProvider.GetStudentFeeGroups();
        }
    }
}