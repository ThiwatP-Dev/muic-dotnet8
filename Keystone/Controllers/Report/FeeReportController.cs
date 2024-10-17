using Microsoft.AspNetCore.Mvc;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using KeystoneLibrary.Data;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using Keystone.Permission;

namespace Keystone.Controllers.Report
{
    public class FeeReportController : BaseController
    {
        private readonly IFeeProvider _feeProvider;
        private readonly IReceiptProvider _receiptProvider;
        public FeeReportController(ApplicationDbContext db,
                                   IFlashMessage flashMessage,
                                   ISelectListProvider selectListProvider,
                                   IFeeProvider feeProvider,
                                   IReceiptProvider receiptProvider) : base(db, flashMessage, selectListProvider)
        {
            _feeProvider = feeProvider;
            _receiptProvider = receiptProvider;
        }

        [PermissionAuthorize("FeeReportPivotByStudent", "")]
        public IActionResult PivotByStudent()
        {
            CreateSelectList();
            var model = new RegistrationResultWithAmountAndCreditReportViewModel();
            model.Criteria = new Criteria();
            return View(model);
        }

        [PermissionAuthorize("FeeReportPivotByStudent", "")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PivotByStudent(Criteria criteria, string actionType)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            var model = new RegistrationResultWithAmountAndCreditReportViewModel();
            model.Criteria = criteria;

            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Danger(Message.RequiredData);
                return View(model);
            }
            criteria.IsQueryPaymentMethod = true;
            criteria.IsConvertScholarAsAnotherFeeType = true;
            
            model = _receiptProvider.GetRegistrationResultWithAmountAndCreditReport(criteria);

            if (!string.IsNullOrEmpty(actionType) && actionType == "Export")
            {
                if (model != null && model.ReportItems != null)
                {
                    using (var wb = GenerateStudentExcel(model))
                    {
                        return wb.Deliver(GetFeeReportPivotByStudentReportFileName());
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

        [PermissionAuthorize("FeeReportPivotByStudentFacultyDepartment", "")]
        public IActionResult PivotByStudentFacultyDepartment()
        {
            CreateSelectList();
            var model = new RegistrationResultWithAmountAndCreditReportViewModel();
            model.Criteria = new Criteria();
            return View(model);
        }

        [PermissionAuthorize("FeeReportPivotByStudentFacultyDepartment", "")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PivotByStudentFacultyDepartment(Criteria criteria, string actionType)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            var model = new RegistrationResultWithAmountAndCreditReportViewModel();
            model.Criteria = criteria;

            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Danger(Message.RequiredData);
                return View(model);
            }

            criteria.IsQueryPaymentMethod = true;
            criteria.IsQueryReceipt = true;
            criteria.IsConvertScholarAsAnotherFeeType = true;
            model = _receiptProvider.GetRegistrationResultWithAmountAndCreditReport(criteria);

            if (!string.IsNullOrEmpty(actionType) && actionType == "Export")
            {
                if (model != null && model.ReportItems != null)
                {
                    using (var wb = GenerateFacultyDepartmentExcel(model))
                    {
                        return wb.Deliver(GetFeeReportPivotByDivisionAndMajorReportFileName());
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

        [PermissionAuthorize("FeeReportPivotByDepartment", "")]
        public IActionResult PivotByDepartment()
        {
            CreateSelectList();
            var model = new RegistrationResultWithAmountAndCreditReportViewModel();
            model.Criteria = new Criteria() ;
            return View(model);
        }

        [PermissionAuthorize("FeeReportPivotByDepartment", "")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PivotByDepartment(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            var model = new RegistrationResultWithAmountAndCreditReportViewModel();
            model.Criteria = criteria;

            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Danger(Message.RequiredData);
                return View(model);
            }
            criteria.IsConvertScholarAsAnotherFeeType = true;
            model = _receiptProvider.GetRegistrationResultWithAmountAndCreditReport(criteria);
            model.ReportItems = model.ReportItems
                .GroupBy(x => new
                {
                    x.Faculty,
                    x.Department,
                })
                .Select(x => new RegistrationResultWithAmountAndCreditReportItemViewModel
                {
                    Faculty = x.Key.Faculty,
                    Department = x.Key.Department,
                    Items = x.SelectMany(y => y.Items).ToList(),
                    TotalAmount = x.Sum(y => y.TotalAmount)
                })
                .OrderBy(x => x.Faculty)
                .ThenBy(x => x.Department)
                .ToList();

            return View(model);
        }

        [PermissionAuthorize("FeeReportPivotByStudentBatch", "")]
        public IActionResult PivotByStudentBatch(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            var model = new RegistrationResultWithAmountAndCreditReportViewModel();
            model.Criteria = criteria;

            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            criteria.IsConvertScholarAsAnotherFeeType = true;
            model = _receiptProvider.GetRegistrationResultWithAmountAndCreditReport(criteria);
            model.Batches = model.ReportItems.GroupBy(x => x.StudentBatch)
                                             .Select(x => new RegistrationResultWithAmountAndCreditReportBatch
                                                          {
                                                              StudentBatch = x.Key,
                                                              ReportItems = x.GroupBy(y => y.Faculty)
                                                                             .Select(y => new RegistrationResultWithAmountAndCreditReportItemViewModel
                                                                                          {
                                                                                              StudentCount = y.Select(z => z.StudentCode).Count(),
                                                                                              Faculty = y.FirstOrDefault().Faculty,
                                                                                              Items = y.SelectMany(z => z.Items).ToList(),
                                                                                              TotalAmount = y.Sum(z => z.TotalAmount)
                                                                                          })
                                                                             .OrderBy(y => y.Faculty)
                                                                             .ToList()
                                                          })
                                             .OrderBy(x => x.StudentBatch)
                                             .ToList();

            model.ReportItems = model.ReportItems.GroupBy(x => x.Faculty)
                                                 .Select(x => new RegistrationResultWithAmountAndCreditReportItemViewModel
                                                              {
                                                                  Faculty = x.Key,
                                                                  StudentCount = x.Select(y => y.StudentCode).Count(),
                                                                  Items = x.SelectMany(y => y.Items).ToList(),
                                                                  TotalAmount = x.Sum(y => y.TotalAmount)
                                                              })
                                                 .OrderBy(x => x.Faculty)
                                                 .ToList();
            return View(model);
        }

        [PermissionAuthorize("FeeReportPivotByStudentBatch", "")]
        public IActionResult PivotByStudentBatchDetail(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            var model = new RegistrationResultWithAmountAndCreditReportViewModel();
            model.Criteria = criteria;

            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            criteria.IsConvertScholarAsAnotherFeeType = true;
            model = _receiptProvider.GetRegistrationResultWithAmountAndCreditReport(criteria);
            var departments = _db.Departments.AsNoTracking()
                                             .Where(x => !x.Code.Contains("No Dep"))
                                             .Select(x => new RegistrationResultWithAmountAndCreditReportItemViewModel
                                                          {
                                                              Department = x.Code,
                                                              Faculty = x.Faculty.ShortNameEn,
                                                              StudentCount = 0,
                                                              Items = new List<RegistrationResultWithAmountAndCreditReportItemViewModel.Item>(),
                                                              TotalAmount = 0
                                                          })
                                             .ToList();
            model.Batches = model.ReportItems.GroupBy(x => x.StudentBatch)
                                             .Select(x => new RegistrationResultWithAmountAndCreditReportBatch
                                                          {
                                                              StudentBatch = x.Key,
                                                              ReportItems = x.GroupBy(y => y.Department)
                                                                             .Select(y => new RegistrationResultWithAmountAndCreditReportItemViewModel
                                                                                          {
                                                                                              StudentCount = y.Select(z => z.StudentCode).Count(),
                                                                                              Faculty = y.FirstOrDefault().Faculty,
                                                                                              Department = y.FirstOrDefault().Department,
                                                                                              Items = y.SelectMany(z => z.Items).ToList(),
                                                                                              TotalAmount = y.Sum(z => z.TotalAmount)
                                                                                          })
                                                                             .ToList()
                                                          })
                                             .OrderBy(x => x.StudentBatch)
                                             .ToList();
            foreach(var item in model.Batches)
            {
                var departmentBatches = item.ReportItems.Select(x => x.Department).ToList();
                var departmentAdds = departments.Where(x => !departmentBatches.Contains(x.Department)).ToList();
                item.ReportItems.AddRange(departmentAdds);
                item.ReportItems = item.ReportItems.ToList();

                item.Faculties = item.ReportItems.GroupBy(x => x.Faculty)
                                                 .Select(x => new RegistrationResultWithAmountAndCreditReportBatchDetail
                                                              {
                                                                  Faculty = x.Key,
                                                                  ReportItems = x.Select(y => y).OrderBy(y => y.Department).ToList()
                                                              })
                                                 .OrderBy(x => x.Faculty)
                                                 .ToList();
            }

            model.ReportItems = model.ReportItems.GroupBy(x => x.Department)
                                                 .Select(x => new RegistrationResultWithAmountAndCreditReportItemViewModel
                                                              {
                                                                  Department = x.Key,
                                                                  Faculty = x.FirstOrDefault().Faculty,
                                                                  StudentCount = x.Select(y => y.StudentCode).Count(),
                                                                  Items = x.SelectMany(y => y.Items).ToList(),
                                                                  TotalAmount = x.Sum(y => y.TotalAmount)
                                                              })
                                                 .ToList();
            if(model.ReportItems.Any())
            {
                var departmentBatches = model.ReportItems.Select(x => x.Department).ToList();
                var departmentAdds = departments.Where(x => !departmentBatches.Contains(x.Department)).ToList();
                model.ReportItems.AddRange(departmentAdds);
                model.ReportItems = model.ReportItems.OrderBy(x => x.Faculty)
                                                         .ThenBy(x => x.Department)
                                                     .ToList();

                model.Faculties = model.ReportItems.GroupBy(x => x.Faculty)
                                                   .Select(x => new RegistrationResultWithAmountAndCreditReportBatchDetail
                                                                {
                                                                    Faculty = x.Key,
                                                                    ReportItems = x.Select(y => y).OrderBy(y => y.Department).ToList()
                                                                })
                                                   .OrderBy(x => x.Faculty)
                                                   .ToList();
            }

            return View(model);
        }

        private string GetFeeReportPivotByStudentReportFileName()
        {
            string fileName = $"Fee Report Pivot By Student Report As Of {DateTime.Now.ToShortDateString()}.xlsx";

            foreach (var c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        private string GetFeeReportPivotByDivisionAndMajorReportFileName()
        {
            var fileName = $"Fee Report by Division and Major Report As Of {DateTime.Now.ToShortDateString()}.xlsx";

            foreach (var c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult ExportExcel(RegistrationResultWithAmountAndCreditReportViewModel model, string page, string returnUrl)
        {
            if ((model.ReportItems != null && model.ReportItems.Any())
                || (model.Batches != null && model.Batches.Any()))
            {

                if (page == "b")
                {
                    using (var wb = GenerateStudentBatchExcel(model))
                    {
                        return wb.Deliver($"Fee Report Pivot By Student Batch.xlsx");
                    }
                }
                else if (page == "bd")
                {
                    using (var wb = GenerateStudentBatchExcelDetail(model))
                    {
                        return wb.Deliver($"Fee Report Pivot By Student Batch Detail.xlsx");
                    }
                }
                else if (page == "d")
                {
                    using (var wb = GenerateDepartmentExcel(model))
                    {
                        return wb.Deliver($"Fee Report Pivot By Department.xlsx");
                    }
                }
                else if (page == "s")
                {
                    using (var wb = GenerateStudentExcel(model))
                    {
                        var fileName = GetFeeReportPivotByStudentReportFileName();
                        return wb.Deliver(fileName);
                    }
                }
                else if (page == "fd")
                {
                    using (var wb = GenerateFacultyDepartmentExcel(model))
                    {
                        return wb.Deliver(GetFeeReportPivotByDivisionAndMajorReportFileName());
                    }
                }
            }

            return Redirect(returnUrl);

        }

        private XLWorkbook GenerateStudentBatchExcel(RegistrationResultWithAmountAndCreditReportViewModel model)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;
            ws.Cell(row, column++).Value = "Division";
            ws.Cell(row, column++).Value = "Student Count";
            foreach (var item in model.Fees)
            {
                ws.Cell(row, column++).Value = item.FeeName;
            }

            ws.Cell(row, column++).Value = "Others";
            ws.Cell(row++, column).Value = "Total Amount";
            var mergeCells = model.Fees.Count + 3;
            foreach (var batch in model.Batches)
            {
                column = 1;
                ws.Cell(row, column).SetValue<string>($"Batch : { batch.StudentBatch}");
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Range(ws.Cell(row, column), ws.Cell(row, mergeCells)).Merge();
                row++;

                foreach (var item in batch.ReportItems)
                {
                    column = 1;
                    ws.Cell(row, column).Value = item.Faculty;
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    column++;

                    ws.Cell(row, column).Value = item.StudentCount;
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    column++;

                    foreach (var fee in item.Items)
                    {
                        ws.Cell(row, column).Value = fee.Amount.ToString(StringFormat.Money);
                        ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        column++;
                    }

                    ws.Cell(row, column).Value = item.OtherTotalAmount.ToString(StringFormat.Money);
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    column++;

                    ws.Cell(row, column).Value = item.TotalAmount.ToString(StringFormat.Money);
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    column++;
                    row += 1;
                }

                column = 1;
                ws.Cell(row, column).Value = $"Batch : {batch.StudentBatch} Total";
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = batch.StudentCount;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                foreach (var fee in batch.Fees)
                {
                    ws.Cell(row, column).Value = fee.Amount.ToString(StringFormat.Money);
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    column++;
                }

                ws.Cell(row, column).Value = batch.OtherTotalAmount.ToString(StringFormat.Money);
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = batch.TotalAmount.ToString(StringFormat.Money);
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;
                row += 1;
            }

            column = 1;
            ws.Cell(row, column).SetValue<string>($"Batch : All");
            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(ws.Cell(row, column), ws.Cell(row, mergeCells)).Merge();
            row++;

            foreach (var item in model.ReportItems)
            {
                column = 1;
                ws.Cell(row, column).Value = item.Faculty;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.StudentCount;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                foreach (var fee in item.Items)
                {
                    ws.Cell(row, column).Value = fee.Amount.ToString(StringFormat.Money);
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    column++;
                }

                ws.Cell(row, column).Value = item.OtherTotalAmount.ToString(StringFormat.Money);
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.TotalAmount.ToString(StringFormat.Money);
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;
                row += 1;
            }

            column = 1;
            ws.Cell(row, column).Value = $"All Total";
            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            column++;

            ws.Cell(row, column).Value = model.StudentCount;
            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            column++;

            foreach (var item in model.Fees)
            {
                ws.Cell(row, column).Value = item.Amount.ToString(StringFormat.Money);
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;
            }

            ws.Cell(row, column).Value = model.OtherTotalAmount.ToString(StringFormat.Money);
            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            column++;

            ws.Cell(row, column).Value = model.TotalAmount.ToString(StringFormat.Money);
            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            column++;

            ws.Columns().AdjustToContents();            
            ws.Rows().AdjustToContents();
            return wb;
        }

        private XLWorkbook GenerateStudentBatchExcelDetail(RegistrationResultWithAmountAndCreditReportViewModel model)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;
            ws.Cell(row, column++).Value = "Division";
            ws.Cell(row, column++).Value = "Major";
            ws.Cell(row, column++).Value = "Student Count";
            foreach (var item in model.Fees)
            {
                ws.Cell(row, column++).Value = item.FeeName;
            }

            ws.Cell(row, column++).Value = "Others";
            ws.Cell(row++, column).Value = "Total Amount";
            var mergeCells = model.Fees.Count + 4;
            foreach (var batch in model.Batches)
            {
                column = 1;
                ws.Cell(row, column).SetValue<string>($"Batch : { batch.StudentBatch}");
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Range(ws.Cell(row, column), ws.Cell(row, mergeCells)).Merge();
                row++;

                foreach (var faculty in batch.Faculties)
                {
                    foreach (var item in faculty.ReportItems)
                    {
                        column = 1;
                        ws.Cell(row, column).Value = item.Faculty;
                        ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        column++;

                        ws.Cell(row, column).Value = item.Department;
                        ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        column++;

                        ws.Cell(row, column).Value = item.StudentCount;
                        ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        column++;

                        foreach (var fee in item.Items)
                        {
                            ws.Cell(row, column).Value = fee.Amount.ToString(StringFormat.Money);
                            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            column++;
                        }

                        ws.Cell(row, column).Value = item.OtherTotalAmount.ToString(StringFormat.Money);
                        ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        column++;

                        ws.Cell(row, column).Value = item.TotalAmount.ToString(StringFormat.Money);
                        ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        column++;
                        row += 1;
                    }
                    column = 1;
                    ws.Cell(row, column).Value = $"Total : {faculty.Faculty} / {batch.StudentBatch}";
                    ws.Cell(row, column++).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Range(ws.Cell(row, column - 1), ws.Cell(row, column)).Merge();
                    column++;

                    ws.Cell(row, column).Value = faculty.StudentCount;
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    column++;


                    foreach (var fee in faculty.Fees)
                    {
                        ws.Cell(row, column).Value = fee.Amount.ToString(StringFormat.Money);
                        ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        column++;
                    }

                    ws.Cell(row, column).Value = faculty.OtherTotalAmount.ToString(StringFormat.Money);
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    column++;

                    ws.Cell(row, column).Value = faculty.TotalAmount.ToString(StringFormat.Money);
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    column++;
                    row += 1;
                }

                column = 1;
                ws.Cell(row, column).Value = $"Total Batch : {batch.StudentBatch}";
                ws.Cell(row, column++).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Range(ws.Cell(row, column - 1), ws.Cell(row, column)).Merge();
                column++;

                ws.Cell(row, column).Value = batch.StudentCount;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                foreach (var fee in batch.Fees)
                {
                    ws.Cell(row, column).Value = fee.Amount.ToString(StringFormat.Money);
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    column++;
                }

                ws.Cell(row, column).Value = batch.OtherTotalAmount.ToString(StringFormat.Money);
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                column++;

                ws.Cell(row, column).Value = batch.TotalAmount.ToString(StringFormat.Money);
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                column++;
                row += 1;
            }

            column = 1;
            ws.Cell(row, column).SetValue<string>($"Batch : All");
            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(ws.Cell(row, column), ws.Cell(row, mergeCells)).Merge();
            row++;

            foreach (var faculty in model.Faculties)
            {
                foreach (var item in faculty.ReportItems)
                {
                    column = 1;
                    ws.Cell(row, column).Value = item.Faculty;
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    column++;

                    ws.Cell(row, column).Value = item.Department;
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    column++;

                    ws.Cell(row, column).Value = item.StudentCount;
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    column++;

                    foreach (var fee in item.Items)
                    {
                        ws.Cell(row, column).Value = fee.Amount.ToString(StringFormat.Money);
                        ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        column++;
                    }

                    ws.Cell(row, column).Value = item.OtherTotalAmount.ToString(StringFormat.Money);
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    column++;

                    ws.Cell(row, column).Value = item.TotalAmount.ToString(StringFormat.Money);
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    column++;
                    row += 1;
                }
                column = 1;
                ws.Cell(row, column).Value = $"Total Faculty : { faculty.Faculty }";
                ws.Cell(row, column++).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Range(ws.Cell(row, column-1), ws.Cell(row, column)).Merge();
                column++;

                ws.Cell(row, column).Value = faculty.StudentCount;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                foreach (var item in faculty.Fees)
                {
                    ws.Cell(row, column).Value = item.Amount.ToString(StringFormat.Money);
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    column++;
                }

                ws.Cell(row, column).Value = faculty.OtherTotalAmount.ToString(StringFormat.Money);
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                column++;

                ws.Cell(row, column).Value = faculty.TotalAmount.ToString(StringFormat.Money);
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                column++;
                row += 1;
            }

            column = 1;
            ws.Cell(row, column).Value = $"All Total";
            ws.Cell(row, column++).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(ws.Cell(row, column-1), ws.Cell(row, column)).Merge();
            column++;

            ws.Cell(row, column).Value = model.StudentCount;
            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            column++;

            foreach (var item in model.Fees)
            {
                ws.Cell(row, column).Value = item.Amount.ToString(StringFormat.Money);
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                column++;
            }

            ws.Cell(row, column).Value = model.OtherTotalAmount.ToString(StringFormat.Money);
            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            column++;

            ws.Cell(row, column).Value = model.TotalAmount.ToString(StringFormat.Money);
            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            column++;

            ws.Columns().AdjustToContents();            
            ws.Rows().AdjustToContents();
            return wb;
        }

        private XLWorkbook GenerateDepartmentExcel(RegistrationResultWithAmountAndCreditReportViewModel model)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;
            ws.Cell(row, column++).Value = "Division";
            ws.Cell(row, column++).Value = "Major";
            foreach (var item in model.Fees)
            {
                ws.Cell(row, column++).Value = item.FeeName;
            }

            ws.Cell(row, column++).Value = "Others";
            ws.Cell(row++, column).Value = "Total Amount";
            foreach (var item in model.ReportItems)
            {
                column = 1;
                ws.Cell(row, column).Value = item.Faculty;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.Department;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                foreach (var fee in item.Items)
                {
                    ws.Cell(row, column).Value = fee.AmountText;
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    column++;
                }

                ws.Cell(row, column).Value = item.OtherTotalAmount.ToString(StringFormat.Money);
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.TotalAmount.ToString(StringFormat.Money);
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;
                row += 1;
            }

            column = 1;
            ws.Cell(row, column).Value = $"Total { model.ReportItems.Count } records";
            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(ws.Cell(row, column), ws.Cell(row, 2)).Merge();
            column += 2;

            foreach (var item in model.Fees)
            {
                ws.Cell(row, column).Value = item.Amount.ToString(StringFormat.Money);
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;
            }

            ws.Cell(row, column).Value = model.OtherTotalAmount.ToString(StringFormat.Money);
            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            column++;

            ws.Cell(row, column).Value = model.TotalAmount.ToString(StringFormat.Money);
            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            column++;

            ws.Columns().AdjustToContents();            
            ws.Rows().AdjustToContents();
            return wb;
        }

        private XLWorkbook GenerateStudentExcel(RegistrationResultWithAmountAndCreditReportViewModel model)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;
            ws.Cell(row, column++).Value = "Term";
            ws.Cell(row, column++).Value = "Code";
            ws.Cell(row, column++).Value = "Name";
            ws.Cell(row, column++).Value = "Division";
            ws.Cell(row, column++).Value = "Major";
            ws.Cell(row, column++).Value = "Invoice Number";
            ws.Cell(row, column++).Value = "Invoice Date";
            foreach (var item in model.FeeColumns)
            {
                ws.Cell(row, column++).Value = item.Key;
            }

            ws.Cell(row, column++).Value = "Others";
            ws.Cell(row, column++).Value = "Total Amount";
            ws.Cell(row, column++).Value = "Paid Method";
            ws.Cell(row, column++).Value = "Paid Status";
            ws.Cell(row++, column).Value = "Payment Date";
            
            foreach (var item in model.ReportItems)
            {
                item.OtherTotalAmount = ((item.Items.Where(x => !model.FeeColumns.SelectMany(y => y.Value).Contains(x.FeeItemId)).Sum(x => x.Amount) - item.Items.Sum(x => x.DiscountAmount)));

                var fullName = $"{ item.StudentTitleEn } { item.StudentFirstNameEn } { item.StudentLastNameEn }";

                column = 1;
                ws.Cell(row, column).Value = item.Term;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.StudentCode;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = fullName;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.Faculty;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.Department;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.InvoiceNumber;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.InvoiceDateText;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                foreach (var key in model.FeeColumns)
                {
                    ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(row, column++).Value = item.Items.Where(x => key.Value.Contains(x.FeeItemId)).Sum(x => x.Amount);
                }

                ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, column).Value = item.OtherTotalAmount;
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                column++;

                ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, column).Value = item.TotalAmount;
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                column++;

                ws.Cell(row, column).Value = item.InvoicePaymentMethod;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.IsPaid ? "Paid" : "Unpaid";
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.LastPaymentDateText;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;
                row += 1;
            }

            column = 1;
            ws.Cell(row, column).Value = $"Total { model.ReportItems.Count } records";
            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(ws.Cell(row, column), ws.Cell(row, 5)).Merge();
            column += 7;

            foreach (var key in model.FeeColumns)
            {
                ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, column++).Value = model.ReportItems.SelectMany(x => x.Items).Where(x => key.Value.Contains(x.FeeItemId)).Sum(x => x.Amount);
            }

            model.OtherTotalAmount = model.ReportItems.Sum(x => x.OtherTotalAmount);
            model.TotalAmount = model.ReportItems.Sum(x => x.TotalAmount);

            ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, column).Value = model.OtherTotalAmount;
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            column++;

            ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, column).Value = model.TotalAmount;
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            column++;

            ws.Columns().AdjustToContents();            
            ws.Rows().AdjustToContents();
            return wb;
        }

        private XLWorkbook GenerateFacultyDepartmentExcel(RegistrationResultWithAmountAndCreditReportViewModel model)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;
            ws.Cell(row, column++).Value = "Term";
            ws.Cell(row, column++).Value = "Code";
            ws.Cell(row, column++).Value = "Name";
            ws.Cell(row, column++).Value = "Division";
            ws.Cell(row, column++).Value = "Major";
            foreach (var item in model.FeeColumns)
            {
                ws.Cell(row, column++).Value = item.Key;
            }

            ws.Cell(row, column++).Value = "Others";
            ws.Cell(row, column++).Value = "Total Amount";
            ws.Cell(row, column++).Value = "Invoice Number";
            ws.Cell(row, column++).Value = "Receipt Number";
            ws.Cell(row, column++).Value = "Receipt Date";
            ws.Cell(row, column++).Value = "Type";
            ws.Cell(row, column++).Value = "Paid Method";
            ws.Cell(row, column++).Value = "Paid Status";
            ws.Cell(row++, column).Value = "Payment Date";

            foreach (var item in model.ReportItems)
            {
                item.OtherTotalAmount = ((item.Items.Where(x => !model.FeeColumns.SelectMany(y => y.Value).Contains(x.FeeItemId)).Sum(x => x.Amount) - item.Items.Sum(x => x.DiscountAmount)));

                var fullName = $"{ item.StudentTitleEn } { item.StudentFirstNameEn } { item.StudentLastNameEn }";

                column = 1;
                ws.Cell(row, column).Value = item.Term;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.StudentCode;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = fullName;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.Faculty;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.Department;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                foreach (var key in model.FeeColumns)
                {
                    ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(row, column++).Value = item.Items.Where(x => key.Value.Contains(x.FeeItemId)).Sum(x => x.Amount);
                }

                ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, column).Value = item.OtherTotalAmount;
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                column++;

                ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, column).Value = item.TotalAmount;
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                column++;

                ws.Cell(row, column).Value = item.InvoiceNumber;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.ReceiptNumber;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.ReceiptDateText;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.InvoiceType;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.InvoicePaymentMethod;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.IsPaid ? "Paid" : "Unpaid";
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.LastPaymentDateText;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;
                row += 1;
            }

            column = 1;
            ws.Cell(row, column).Value = $"Total { model.ReportItems.Count } records";
            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(ws.Cell(row, column), ws.Cell(row, 5)).Merge();
            column += 5;

            foreach (var key in model.FeeColumns)
            {
                ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, column++).Value = model.ReportItems.SelectMany(x => x.Items).Where(x => key.Value.Contains(x.FeeItemId)).Sum(x => x.Amount);
            }

            model.OtherTotalAmount = model.ReportItems.Sum(x => x.OtherTotalAmount);
            model.TotalAmount = model.ReportItems.Sum(x => x.TotalAmount);

            ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, column).Value = model.OtherTotalAmount;
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            column++;

            ws.Cell(row, column).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, column).Value = model.TotalAmount;
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            column++;

            ws.Columns().AdjustToContents();            
            ws.Rows().AdjustToContents();
            return wb;
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            }
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.FeeGroups = _selectListProvider.GetFeeGroups();
            ViewBag.SlotTypes = _selectListProvider.GetSlotType();
            ViewBag.PaymentStatuses = _selectListProvider.GetPaymentStatusesWithOutAll();

            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            ViewBag.PaidStatuses = _selectListProvider.GetPaidStatuses();
            ViewBag.InvoiceType = _selectListProvider.GetInvoiceType();
            ViewBag.ResidentTypes = _selectListProvider.GetResidentTypes();
            ViewBag.AdmissionTypes = _selectListProvider.GetAdmissionTypes();
            ViewBag.StudentFeeGroups = _selectListProvider.GetStudentFeeGroups();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();
        }
    }
}