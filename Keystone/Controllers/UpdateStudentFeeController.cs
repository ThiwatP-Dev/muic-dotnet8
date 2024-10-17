using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("UpdateStudentFee", "")]
    public class UpdateStudentFeeController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly IRegistrationProvider _registrationProvider;
        public UpdateStudentFeeController(ApplicationDbContext db,
                                          IFlashMessage flashMessage,
                                          ISelectListProvider selectListProvider,
                                          IRegistrationProvider registrationProvider,
                                          IConfiguration config) : base(db, flashMessage, selectListProvider) 
        { 
            _config = config;
            _registrationProvider = registrationProvider;
        }

        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult Index(Criteria criteria, string returnUrl)
        {   
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.DepartmentId, criteria.CurriculumId);
            ViewBag.ReturnUrl = returnUrl;
            if (criteria.AcademicLevelId == 0 
                && criteria.FacultyId == 0 
                && criteria.DepartmentId == 0 
                && criteria.CurriculumId == 0 
                && criteria.CurriculumVersionId == 0
                && criteria.AdmissionTypeId == 0 
                && string.IsNullOrEmpty(criteria.FirstName) 
                && string.IsNullOrEmpty(criteria.LastName) 
                && string.IsNullOrEmpty(criteria.StudentStatus)
                && criteria.StudentCodeFrom == null 
                && criteria.StudentCodeTo == null 
                && criteria.IsThai == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                criteria.StudentStatus = "s";
                return View(new UpdateStudentFeeViewModel
                            { 
                                Criteria = criteria
                            });
            }

            var details = _db.Students.Where(x => (criteria.StudentCodeFrom == null
                                                   || (criteria.StudentCodeTo == null ? Convert.ToInt32(x.Code) == criteria.StudentCodeFrom
                                                                                      : Convert.ToInt32(x.Code) >= criteria.StudentCodeFrom))
                                                   && (criteria.StudentCodeTo == null
                                                       || (criteria.StudentCodeFrom == null ? Convert.ToInt32(x.Code) == criteria.StudentCodeTo
                                                                                            : Convert.ToInt32(x.Code) <= criteria.StudentCodeTo))
                                                   && (string.IsNullOrEmpty(criteria.FirstName)
                                                       || x.FirstNameEn.Contains(criteria.FirstName)
                                                       || x.FirstNameTh.Contains(criteria.FirstName))
                                                   && (string.IsNullOrEmpty(criteria.LastName)
                                                       || x.LastNameEn.Contains(criteria.LastName)
                                                       || x.LastNameTh.Contains(criteria.LastName))
                                                   && (criteria.AcademicLevelId == 0
                                                       || x.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId)
                                                   && (criteria.FacultyId == 0
                                                       || x.AcademicInformation.FacultyId == criteria.FacultyId)
                                                   && (criteria.DepartmentId == 0
                                                       || x.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                   && (criteria.AdmissionTypeId == 0
                                                       || x.AdmissionInformation.AdmissionTypeId == criteria.AdmissionTypeId)
                                                   && (criteria.CurriculumId == 0
                                                       || x.AcademicInformation.CurriculumVersion.CurriculumId == criteria.CurriculumId)
                                                   && (criteria.CurriculumVersionId == 0
                                                       || x.AcademicInformation.CurriculumVersionId == criteria.CurriculumVersionId)
                                                   && (string.IsNullOrEmpty(criteria.StudentStatus)
                                                       || x.StudentStatus == criteria.StudentStatus)
                                                   && (criteria.IsThai == null 
                                                       || (criteria.IsThai.Value ? x.Nationality.NameEn.ToLower() == "thai" 
                                                                                 : x.Nationality.NameEn.ToLower() != "thai"))
                                                   && (criteria.StudentFeeGroupId == 0
                                                       || x.StudentFeeGroupId == criteria.StudentFeeGroupId))
                                      .Select(x => new UpdateStudentFeeDetail
                                                   {
                                                       StudentId = x.Id,
                                                       Code = x.Code,
                                                       Title = x.Title.NameEn,
                                                       FirstName = x.FirstNameEn,
                                                       MidName = x.MidNameEn,
                                                       LastName = x.LastNameEn,
                                                       Major = x.AcademicInformation.Department.Code,
                                                       Nationality = x.Nationality.NameEn,
                                                       ResidentType = x.ResidentType.NameEn,
                                                       StudentFeeType = x.StudentFeeType.NameEn,
                                                       StudentFeeGroup = x.StudentFeeGroup.Name,
                                                       AdmissionType = x.AdmissionInformation.AdmissionType.NameEn
                                                   })
                                      .OrderBy(x => x.Code)
                                      .AsNoTracking()
                                      .ToList();

            var model = new UpdateStudentFeeViewModel
                        {
                            Criteria = criteria,
                            Details = details
                        };

            return View(model);
        }

        [PermissionAuthorize("UpdateStudentFee", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public ActionResult Update(UpdateStudentFeeViewModel model, string returnUrl)
        {
            try
            {
                var studentIds = model.Details.Where(x => x.IsChecked == "on")
                                              .Select(x => x.StudentId)
                                              .ToList();

                var students = _db.Students.Where(x => studentIds.Contains(x.Id))
                                           .ToList();

                foreach(var item in students)
                {
                    if (model.Criteria.ResidentTypeId != 0)
                    {
                        item.ResidentTypeId = model.Criteria.ResidentTypeId;
                    }

                    if (model.Criteria.StudentFeeTypeId != 0)
                    {
                        item.StudentFeeTypeId = model.Criteria.StudentFeeTypeId;
                    }

                    if (model.Criteria.StudentFeeGroupId != 0)
                    {
                        item.StudentFeeGroupId = model.Criteria.StudentFeeGroupId;
                    }
                }

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                CreateSelectList();
                return Redirect(returnUrl);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCreate);
                CreateSelectList();
                ViewBag.ReturnUrl = returnUrl;
                return Redirect(returnUrl);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult ExportExcel(UpdateStudentFeeViewModel model, string returnUrl)
        {
            if (model.Details != null && model.Details.Any())
            {
                using (var wb = GenerateWorkBook(model.Details))
                {
                    return wb.Deliver($"Update Student Fee.xlsx");
                }
            }

            return Redirect(returnUrl);
        }

        private XLWorkbook GenerateWorkBook(List<UpdateStudentFeeDetail> details)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;
            ws.Cell(row, column++).Value = "Code";
            ws.Cell(row, column++).Value = "Name";
            ws.Cell(row, column++).Value = "Major";
            ws.Cell(row, column++).Value = "Nationality";
            ws.Cell(row, column++).Value = "Admission Type";
            ws.Cell(row, column++).Value = "Resident Type";
            ws.Cell(row, column++).Value = "Student Fee Type";
            ws.Cell(row++, column).Value = "Student Fee Group";

            foreach (var item in details)
            {
                var predefind = string.Empty;
                column = 1;
                ws.Cell(row, column).SetValue<string>(item.Code);
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.FullName;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.Major;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.Nationality;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.AdmissionType;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.ResidentType;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.StudentFeeType;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;

                ws.Cell(row, column).Value = item.StudentFeeGroup;
                ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                column++;
                row += 1;
            }
            
            ws.Columns().AdjustToContents();
            ws.Rows().AdjustToContents();
            return wb;
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long department = 0, long curriculumId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.AdmissionTypes = _selectListProvider.GetAdmissionTypes();
            ViewBag.ThaiStatuses = _selectListProvider.GetThaiStatuses();
            ViewBag.ResidentTypes = _selectListProvider.GetResidentTypes();
            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();
            ViewBag.StudentFeeGroups = _selectListProvider.GetStudentFeeGroups();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            ViewBag.Curriculums = _selectListProvider.GetCurriculumByDepartment(academicLevelId, facultyId, department);
            ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
            ViewBag.StudentFeeGroups = _selectListProvider.GetStudentFeeGroups();
        }
    }
}