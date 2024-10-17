using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class EGradingReportController : BaseController
    {
        protected readonly IRegistrationProvider _registrationProvider;
        public EGradingReportController(ApplicationDbContext db,
                                         ISelectListProvider selectListProvider,
                                         IRegistrationProvider registrationProvider,
                                         IFlashMessage flashMessage) : base(db, flashMessage, selectListProvider) 
        { 
            _registrationProvider = registrationProvider;
        }
        
        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0 || String.IsNullOrEmpty(criteria.TransferType))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }                                 

            var courseSection = (from course in _db.Courses.Include(x => x.Department)
                                 join section in _db.Sections
                                 on course.Id equals section.CourseId
                                 where criteria.AcademicLevelId == course.AcademicLevelId
                                       && course.TransferUniversityId == null
                                       && criteria.TermId == section.TermId
                                       && (criteria.FacultyId == course.Faculty.Id 
                                           || criteria.FacultyId == 0)
                                       && (!(criteria.DepartmentIds.Any())
                                           || criteria.DepartmentIds.Contains(course.Department.Id))
                                 select new 
                                        {
                                            courseId = course.Id,
                                            courseCode = course.Code,
                                            sectionId = section.Id,
                                            sectionNumber = section.Number
                                        })
                                 .ToList();
            
            var courseList = courseSection.Select(x => x.courseId)
                                          .ToList();
            
            var sectionList = courseSection.Select(x => x.sectionId)
                                           .ToList();

            var gradeBarcodes = _db.Barcodes.Include(x => x.Course)
                                            .Select(x => new
                                                         {
                                                             courseId = x.CourseId,
                                                             courseCode = x.Course.Code,
                                                             sectionId = x.SectionIds,
                                                             barcodeNumber = x.BarcodeNumber,
                                                             generatedAt = x.GeneratedAt,
                                                             publishedAt = x.PublishedAt
                                                         })
                                            .ToList();

            if (!String.IsNullOrEmpty(criteria.Code) && criteria.CourseNumberFrom != null && criteria.CourseNumberTo != null)
            {
                var sectionListCode = _registrationProvider.GetSectionIdsByCoursesRange(criteria.TermId,
                                                                                        criteria.Code, 
                                                                                        criteria.CourseNumberFrom ?? 0, 
                                                                                        criteria.CourseNumberTo ?? 0, 
                                                                                        criteria.SectionFrom ?? 0, 
                                                                                        criteria.SectionTo ?? 0);

                var modelListCode = courseSection.Join(gradeBarcodes, 
                                                       x => x.courseId, 
                                                       y => y.courseId, 
                                                       (x,y) => new 
                                                                {   
                                                                    CourseId = y.courseId, 
                                                                    CourseCode = y.courseCode, 
                                                                    SectionId_course = x.sectionId, 
                                                                    SectionId_grade = y.sectionId,
                                                                    SectionNumber = x.sectionNumber,
                                                                    BarcodeNumber = y.barcodeNumber,
                                                                    GeneratedAt = y.generatedAt,
                                                                    PublishedAt = y.publishedAt 
                                                                })
                                                 .Where (x => ((x.SectionId_course.ToString() == x.SectionId_grade) 
                                                                || String.IsNullOrEmpty(x.SectionId_grade))
                                                                &&((criteria.TransferType == "TRANSFERRED"
                                                                     && x.PublishedAt != null)
                                                                     || (criteria.TransferType == "NOTTRANSFERRED"
                                                                         && x.PublishedAt == null)
                                                                     || (criteria.TransferType == "NOTCOMPLETE"
                                                                         && String.IsNullOrEmpty(x.BarcodeNumber))
                                                                     || criteria.TransferType == "ALL"))
                                                 .GroupBy(x => x.SectionId_grade)
                                                 .Select (x => new FinalGradeDetail
                                                               {
                                                                   CourseCode = x.First().CourseCode,
                                                                   SectionNumber = String.IsNullOrEmpty(x.First().SectionId_grade) ? " " : x.First().SectionNumber,
                                                                   BarcodeNumber = x.First().BarcodeNumber,
                                                                   GeneratedAt = x.First().GeneratedAt,
                                                                   PublishedAt = x.First().PublishedAt
                                                               })
                                                 .ToList();

                var modelListCodes = new FinalGradeViewModel
                                     {
                                         Criteria = criteria,
                                         FinalGradeDetails = modelListCode
                                     };

                return View(modelListCodes);
            }  
        
            var model = gradeBarcodes.Join(courseSection, 
                                           x => x.courseId, 
                                           y => y.courseId, 
                                           (x,y) => new 
                                                    { 
                                                        CourseId = x.courseId, 
                                                        CourseCode = x.courseCode, 
                                                        SectionId_course = y.sectionId, 
                                                        SectionId_grade = x.sectionId,
                                                        SectionNumber = y.sectionNumber,
                                                        BarcodeNumber = x.barcodeNumber,
                                                        GeneratedAt = x.generatedAt,
                                                        PublishedAt = x.publishedAt 
                                                    })
                                     .Where (x =>((x.SectionId_course.ToString() == x.SectionId_grade) 
                                                   || String.IsNullOrEmpty(x.SectionId_grade))
                                                   &&((criteria.TransferType == "TRANSFERRED"
                                                        && x.PublishedAt != null)
                                                     || (criteria.TransferType == "NOTTRANSFERRED"
                                                         && x.PublishedAt == null)
                                                     || (criteria.TransferType == "NOTCOMPLETE"
                                                         && String.IsNullOrEmpty(x.BarcodeNumber))
                                                     || (criteria.TransferType == "ALL"
                                                  || String.IsNullOrEmpty(criteria.TransferType))))
                                     .GroupBy(x => x.SectionId_grade)
                                     .Select (x => new FinalGradeDetail
                                                   {
                                                       CourseCode = x.First().CourseCode,
                                                       SectionNumber = String.IsNullOrEmpty(x.First().SectionId_grade) ? " " : x.First().SectionNumber,
                                                       BarcodeNumber = x.First().BarcodeNumber,
                                                       GeneratedAt = x.First().GeneratedAt,
                                                       PublishedAt = x.First().PublishedAt
                                                   })
                                     .ToList();

            var models = new FinalGradeViewModel
                        {
                            Criteria = criteria,
                            FinalGradeDetails = model
                        };

            return View(models);
        }

        public void CreateSelectList(long academicLevelId = 0, long termId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.TransferType = _selectListProvider.GetTransferType();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            }
        }
    }
}