using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    public class RegistrationCourseWithoutInvoiceReportController : BaseController
    {
        protected readonly ICacheProvider _cacheProvider;
        public RegistrationCourseWithoutInvoiceReportController(ApplicationDbContext db,
                                                                IFlashMessage flashMessage,
                                                                ISelectListProvider selectListProvider,
                                                                ICacheProvider cacheProvider) : base(db, flashMessage, selectListProvider)
        {
            _cacheProvider = cacheProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            var model = new RegistrationCourseWithoutInvoiceReportViewModel();
            model.Criteria = criteria;
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                criteria.AcademicLevelId = _db.AcademicLevels.SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
                criteria.TermId = _cacheProvider.GetCurrentTerm(criteria.AcademicLevelId).Id;
                criteria.Status = "false";
            }

            CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.FacultyId, criteria.DepartmentId, criteria.CurriculumId);
            var registrationCourses = (from x in _db.RegistrationCourses.AsNoTracking()
                                       join invoiceItem in _db.InvoiceItems.AsNoTracking() on 
                                       new
                                       {
                                           RegistrationCourseId = x.Id,
                                           IsActive = true,
                                           IsCancel = false,
                                       } equals 
                                       new
                                       {
                                           RegistrationCourseId = invoiceItem.RegistrationCourseId.Value,
                                           IsActive = invoiceItem.Invoice.IsActive,
                                           IsCancel = invoiceItem.Invoice.IsCancel
                                       } into invoiceItems
                                       from invoiceItem in invoiceItems.DefaultIfEmpty()
                                       where x.TermId == criteria.TermId
                                                                         && (criteria.FacultyId == 0
                                                                             || x.Student.AcademicInformation.FacultyId == criteria.FacultyId)
                                                                         && (criteria.DepartmentId == 0
                                                                             || x.Student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                                         && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                                             || x.Student.Code.StartsWith(criteria.CodeAndName)
                                                                             || x.Student.FirstNameEn.StartsWith(criteria.CodeAndName)
                                                                             || x.Student.MidNameEn.StartsWith(criteria.CodeAndName)
                                                                             || x.Student.LastNameEn.StartsWith(criteria.CodeAndName))
                                                                         && (criteria.StartStudentBatch == null
                                                                             || x.Student.AcademicInformation == null
                                                                             || x.Student.AcademicInformation.Batch >= criteria.StartStudentBatch)
                                                                         && (criteria.EndStudentBatch == null
                                                                             || x.Student.AcademicInformation == null
                                                                             || x.Student.AcademicInformation.Batch <= criteria.EndStudentBatch)
                                                                         && (criteria.CurriculumId == 0
                                                                             || x.Student.AcademicInformation.CurriculumVersion.CurriculumId == criteria.CurriculumId)
                                                                         && (criteria.CurriculumVersionId == 0
                                                                             || x.Student.AcademicInformation.CurriculumVersionId == criteria.CurriculumVersionId)
                                                                         && (string.IsNullOrEmpty(criteria.StudentStatus)
                                                                             || x.Student.StudentStatus == criteria.StudentStatus)
                                                                         && x.Status != "d"
                                                                         //Term 138 has a lot of problem related to data so will have special condition for it
                                                                         && (x.TermId > 138 || 
                                                                            ( (!x.Student.StudentFeeGroup.IsLumpsumPayment)
                                                                            )
                                                                         )

                                      select new RegistrationCourseWithoutInvoiceReportDetail
                                      {
                                          RegistrationCourseId = x.Id,
                                          CourseId = x.CourseId,
                                          StudentId = x.StudentId,
                                          StudentCode = x.Student.Code,
                                          StudentTitle = x.Student.Title.NameEn,
                                          StudentFirstName = x.Student.FirstNameEn,
                                          StudentMidName = x.Student.MidNameEn,
                                          StudentLastName = x.Student.LastNameEn,
                                          Department = x.Student.AcademicInformation.Department.NameEn,
                                          Email = x.Student.Email,
                                          PhoneNumber = x.Student.TelephoneNumber1,
                                          CourseCode = x.Course.Code,
                                          Credit = x.Course.Credit,
                                          Lecture = x.Course.Lecture,
                                          Lab = x.Course.Lab,
                                          Other = x.Course.Other,
                                          SectionNumber = x.Section.Number,
                                          HaveInvoice = invoiceItem != null
                                      }).ToList();

            //var registrationCourseIds = registrationCourses.Select(x => (long?)x.RegistrationCourseId);
            //var invoiceRegistrationIds = _db.InvoiceItems.AsNoTracking()
            //                                             .Where(x => registrationCourseIds.Contains(x.RegistrationCourseId)
            //                                                         && !x.Invoice.IsCancel)
            //                                             .Select(x => x.RegistrationCourseId)
            //                                             .ToList();

            //foreach (var item in registrationCourses)
            //{
            //    item.HaveInvoice = invoiceRegistrationIds.Contains(item.RegistrationCourseId);
            //}

            model.Details = registrationCourses.GroupBy(x => x.StudentId)
                                               .Select(x => new RegistrationCourseWithoutInvoiceReportDetail
                                               {
                                                   StudentId = x.FirstOrDefault().StudentId,
                                                   StudentCode = x.FirstOrDefault().StudentCode,
                                                   StudentTitle = x.FirstOrDefault().StudentTitle,
                                                   StudentFirstName = x.FirstOrDefault().StudentFirstName,
                                                   StudentMidName = x.FirstOrDefault().StudentMidName,
                                                   StudentLastName = x.FirstOrDefault().StudentLastName,
                                                   Department = x.FirstOrDefault().Department,
                                                   Email = x.FirstOrDefault().Email,
                                                   PhoneNumber = x.FirstOrDefault().PhoneNumber,
                                                   HaveInvoice = x.All(y => y.HaveInvoice),
                                                   CourseAndSectionText = string.Join(", ", x.Select(y => y.CourseAndSection).ToList()),
                                               })
                                               .ToList();

            model.Details = model.Details.Where(x => (string.IsNullOrEmpty(criteria.Status)
                                                      || x.HaveInvoice == Convert.ToBoolean(criteria.Status)))
                                         .OrderBy(x => x.StudentCode)
                                         .ToList();
            return View(model);
        }

        private void CreateSelectList(long academicLevelId, long termId, long facultyId, long departmentId, long curriculumId)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.Statuses = _selectListProvider.GetAllYesNoAnswer();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            ViewBag.Curriculums = _selectListProvider.GetCurriculumByDepartment(academicLevelId, facultyId, departmentId);
            ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
        }
    }
}