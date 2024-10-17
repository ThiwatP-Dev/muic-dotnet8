using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class SchoolRevenueSummaryController : BaseController
    {
        public SchoolRevenueSummaryController(ApplicationDbContext db,
                                              IFlashMessage flashMessage,
                                              ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }
        
        public IActionResult Index(Criteria criteria, int page = 1)
        {
            SchoolRevenueSummaryViewModel model = new SchoolRevenueSummaryViewModel
            {
                Criteria = criteria,
                Results = new List<SchoolRevenueSummaryResult>()
            };

            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0 || criteria.FacultyId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            var results = (from receiptItem in _db.ReceiptItems
                           join receipt in _db.Receipts on receiptItem.ReceiptId equals receipt.Id
                           join invoiceItem in _db.InvoiceItems on receiptItem.InvoiceItemId equals invoiceItem.Id
                           join invoice in _db.Invoices on receiptItem.InvoiceId equals invoice.Id
                           join student in _db.Students on invoice.StudentId equals student.Id
                           join academicInfo in _db.AcademicInformations on student.Id equals academicInfo.StudentId
                           join faculty in _db.Faculties on academicInfo.FacultyId equals faculty.Id
                           join term in _db.Terms on invoice.TermId equals term.Id
                           join course in _db.Courses on invoiceItem.CourseId equals course.Id into courses
                           from course in courses.DefaultIfEmpty()
                           where term.AcademicLevelId == criteria.AcademicLevelId
                                 && invoice.TermId == criteria.TermId
                                 && academicInfo.FacultyId == criteria.FacultyId
                                 && (criteria.UpdatedFrom == null
                                     || receipt.CreatedAt.Date >= criteria.UpdatedFrom.Value.Date)
                                 && (criteria.UpdatedTo == null
                                     || receipt.CreatedAt.Date <= criteria.UpdatedTo.Value.Date)
                                 && (criteria.DepartmentId == 0
                                     || academicInfo.DepartmentId == criteria.DepartmentId)
                                 && (criteria.CourseFacultyId == 0
                                     || course.FacultyId == criteria.CourseFacultyId)
                                 && (criteria.CourseIds == null
                                     || !criteria.CourseIds.Any()
                                     || criteria.CourseIds.Contains(invoiceItem.CourseId ?? 0))
                                 && (criteria.FeeItemIds == null
                                     || !criteria.FeeItemIds.Any()
                                     || criteria.FeeItemIds.Contains(receiptItem.FeeItemId ?? 0))
                           select new SchoolRevenueSummaryResult
                           {
                               FacultyName = faculty.NameEn,
                               FeeName = receiptItem.FeeItemName,
                               CourseCode = course == null ? string.Empty : course.Code,
                               CourseName = course == null ? string.Empty : course.NameEn,
                               StudentCode = student.Code,
                               StudentName = student.FullNameEn,
                               Amount = receiptItem.TotalAmount
                           }).ToList();
            
            model.Results = results;
            return View(model);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.FeeItems = _selectListProvider.GetFeeItems();
            if (academicLevelId != 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);

                if (facultyId != 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }
        }
    }
}