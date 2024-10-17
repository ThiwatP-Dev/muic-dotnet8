using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Api;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("RegistrationStudentByCourse", "")]
    public class RegistrationStudentByCourseController : BaseController
    {
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IFeeProvider _feeProvider;

        public RegistrationStudentByCourseController(ApplicationDbContext db,
                                                     IFlashMessage flashMessage,
                                                     ISelectListProvider selectListProvider,
                                                     IRegistrationProvider registrationProvider,
                                                     IFeeProvider feeProvider,
                                                     IAcademicProvider academicProvider) : base(db, flashMessage, selectListProvider)
        {
            _academicProvider = academicProvider;
            _registrationProvider = registrationProvider;
            _feeProvider = feeProvider;
        }

        public async Task<IActionResult> Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            var userId = GetUser()?.Id ?? "From KS Regis by Course";

            var fromTerm = _academicProvider.GetTerm(criteria.StartTermId);
            var toTerm = _academicProvider.GetTerm(criteria.EndTermId);

            var allsectionInfo = (from section in _db.Sections
                                where criteria.StartTermId == 0 || string.Compare(section.Term.TermCompare, fromTerm.TermCompare) >= 0
                                     && (criteria.EndTermId == 0 || string.Compare(section.Term.TermCompare, toTerm.TermCompare) <= 0)
                                     && (!criteria.CourseIds.Any() || criteria.CourseIds.Contains(section.CourseId))
                                select new
                                {
                                    SectionId = section.Id,
                                    section.Course.CourseAndCredit,
                                    SectionNumber = section.Number
                                }).AsNoTracking().Distinct().ToList();

            //Add Joint 
            var allParentSectionId = allsectionInfo.Select(x => x.SectionId).ToList();
            allsectionInfo.AddRange(
                                  (from section in _db.Sections
                                   where criteria.StartTermId == 0 || string.Compare(section.Term.TermCompare, fromTerm.TermCompare) >= 0
                                        && (criteria.EndTermId == 0 || string.Compare(section.Term.TermCompare, toTerm.TermCompare) <= 0)
                                        && (allParentSectionId.Contains(section.ParentSectionId ?? 0))
                                   select new
                                   {
                                       SectionId = section.Id,
                                       section.Course.CourseAndCredit,
                                       SectionNumber = section.Number
                                   }).AsNoTracking().Distinct().ToList()
                                 );

            ////Add Parent [will mis match with attendance sheet, signature sheet]
            //allsectionInfo.AddRange(
            //                      (from section in _db.Sections
            //                       where criteria.StartTermId == 0 || string.Compare(section.Term.TermCompare, fromTerm.TermCompare) >= 0
            //                            && (criteria.EndTermId == 0 || string.Compare(section.Term.TermCompare, toTerm.TermCompare) <= 0)
            //                            && (!criteria.CourseIds.Any() || criteria.CourseIds.Contains(section.CourseId))
            //                            && section.ParentSectionId.HasValue 
            //                            && section.ParentSectionId > 0
            //                       select new
            //                       {
            //                           SectionId = section.ParentSectionId.Value,
            //                           section.ParentSection.Course.CourseAndCredit,
            //                           SectionNumber = section.ParentSection.Number
            //                       }).AsNoTracking().Distinct().ToList()
            //                     );

            var allSectionId = allsectionInfo.Select(x => x.SectionId).ToList();


            var invoiceItems = _db.InvoiceItems.Include(x => x.Invoice)
                                                   .ThenInclude(x => x.Term)
                                               .Include(x => x.Invoice)
                                                   .ThenInclude(x => x.Student)
                                                       .ThenInclude(x => x.ResidentType)
                                               .Include(x => x.Invoice)
                                                   .ThenInclude(x => x.Student)
                                                       .ThenInclude(x => x.Title)
                                               .Include(x => x.Invoice)
                                                   .ThenInclude(x => x.Student)
                                                       .ThenInclude(x => x.AcademicInformation)
                                                           .ThenInclude(x => x.Department)
                                               .Include(x => x.RegistrationCourse)
                                               .Include(x => x.Course)
                                               .Include(x => x.Section)
                                               .Where(x => !x.Invoice.IsCancel 
                                                           && x.CourseId != null
                                                           && x.RegistrationCourse.Status != "d"
                                                           && x.Invoice.Student.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                           && (criteria.FacultyId == 0 || x.Invoice.Student.AcademicInformation.FacultyId == criteria.FacultyId)
                                                           && (criteria.DepartmentId == 0 || x.Invoice.Student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                           && (criteria.StartTermId == 0 || string.Compare(x.Invoice.Term.TermCompare, fromTerm.TermCompare) >= 0)
                                                           && (criteria.EndTermId == 0 || string.Compare(x.Invoice.Term.TermCompare, toTerm.TermCompare) <= 0)
                                                           && (allSectionId.Contains(x.SectionId ?? 0))
                                                           && (string.IsNullOrEmpty(criteria.Code) || x.Invoice.Student.Code == criteria.Code))
                                               .OrderBy(x => x.Invoice.Student.Code)
                                               .ThenBy(x => x.Course.Code)
                                               .ThenBy(x => x.Section.Number)
                                               .ToList()
                                               .Select(x => new 
                                                            {
                                                                x.Invoice.StudentId,
                                                                x.CourseId,
                                                                x.SectionId,
                                                                StudentCode = x.Invoice.Student?.Code ?? "",
                                                                StudentFullNameEn = x.Invoice.Student?.FullNameEn ?? "",
                                                                DepartmentAbbreviation = x.Invoice.Student?.AcademicInformation?.Department?.Abbreviation ?? "",
                                                                ResidentTypeNameEn = x.Invoice.Student?.ResidentType?.NameEn ?? "",
                                                                CourseAndCredit = x.Course?.CourseAndCredit ?? "",
                                                                SectionNumber = x.Section?.Number ?? "",
                                                                //TotalAmount = x.TotalAmount,
                                                                Amount = x.Amount,
                                                                IsPaid = x.RegistrationCourse?.IsPaid ?? false,
                                                            })
                                               .ToList();

            var results = invoiceItems.GroupBy(x => new 
                                                    {
                                                        x.StudentId,
                                                        x.CourseId.Value,
                                                        SectionId = x.SectionId ?? 0
                                                    })
                                      .Select(x => {
                                                       var item = x.FirstOrDefault();
                                                       return new RegistrationStudentByCourse
                                                              {
                                                                  StudentCode = item.StudentCode,
                                                                  StudentFullNameEn = item.StudentFullNameEn,
                                                                  DepartmentAbbreviation = item.DepartmentAbbreviation,
                                                                  ResidentTypeNameEn = item.ResidentTypeNameEn,
                                                                  CourseAndCredit = item.CourseAndCredit,
                                                                  SectionNumber = item.SectionNumber,
                                                                  TotalAmount = x.Sum(y => y.Amount),
                                                                  IsPaid = x.All(y => y.IsPaid),
                                                                  SectionId = x.Key.SectionId,
                                                                  StudentId = x.Key.StudentId ?? System.Guid.Empty,
                                                                  IsConfirmInvoice = true
                                                       };
                                                   })
                                      .ToList();

            //Costly logic to sync data from US and calculate fee if they are not confirm
            if (criteria.IsIncludeUnConfirm && criteria.CourseIds.Any())
            {
                foreach (var sectionAndCourse in allsectionInfo)
                {
                    var section = _db.Sections.FirstOrDefault(x => x.Id == sectionAndCourse.SectionId);
                    List<StudentListViewModel> students = null;
                    try
                    {
                        students = _registrationProvider.CallUSparkAPIGetStudentsBySectionId(sectionAndCourse.SectionId);
                    }
                    catch
                    {
                        _flashMessage.Warning(" Problem retreive data...");
                        return View(new RegistrationStudentByCourseViewModel
                        {
                            Criteria = criteria,
                            Results = new System.Collections.Generic.List<RegistrationStudentByCourse>()
                        });
                    }
                    var notAlreadyInStudent = students.Where(x => !results.Any(y => y.SectionId == sectionAndCourse.SectionId && y.StudentCode == x.StudentCode)).ToList();

                    foreach (var student in notAlreadyInStudent)
                    {
                        var studentObj = _db.Students.IgnoreQueryFilters()
                                                     .AsNoTracking()
                                                     .Include(x => x.Title)
                                                     .Include(x => x.AcademicInformation)
                                                         .ThenInclude(x => x.Department)
                                                     .Include(x => x.ResidentType)
                                                     .FirstOrDefault(x => x.Code == student.StudentCode);
                        try
                        {
                            await _registrationProvider.GetRegistrationCourseFromUspark(studentObj.Id, section.TermId, userId);
                        }
                        catch
                        {
                            _flashMessage.Warning("No change made. Problem retreive data...");
                            return View (new RegistrationStudentByCourseViewModel
                            {
                                Criteria = criteria,
                                Results = new System.Collections.Generic.List<RegistrationStudentByCourse>()
                            });
                        }

                        var courseTuition = _feeProvider.CalculateTuitionFeeV3(student.StudentCode, section.TermId)
                                           .ToList();

                        var sectionTution = courseTuition.FirstOrDefault(x => x.SectionId == sectionAndCourse.SectionId);
                        if (sectionTution != null)
                        {
                            results.Add(new RegistrationStudentByCourse
                            {
                                StudentCode = student.StudentCode,
                                StudentFullNameEn = studentObj?.FullNameEn ?? "",
                                DepartmentAbbreviation = studentObj?.AcademicInformation?.Department?.Abbreviation ?? "",
                                ResidentTypeNameEn = studentObj?.ResidentType?.NameEn ?? "",
                                CourseAndCredit = sectionAndCourse.CourseAndCredit,
                                SectionNumber = sectionAndCourse.SectionNumber,
                                TotalAmount = sectionTution.TotalAmount,
                                IsPaid = false,
                                SectionId = sectionAndCourse.SectionId,
                                StudentId = studentObj.Id,
                                IsConfirmInvoice = false
                            });
                        }
                    }
                }
            }

            var model = new RegistrationStudentByCourseViewModel
                        {
                            Criteria = criteria,
                            Results = results
                        };

            return View(model);
        }

        private void CreateSelectList(long academicLevelId, long facultyId) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Courses = _selectListProvider.GetCourses();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }
            
            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartments(facultyId);
            }
        }
    }
}