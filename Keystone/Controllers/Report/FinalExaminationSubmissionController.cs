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
    [PermissionAuthorize("FinalExaminationSubmission", "")]
    public class FinalExaminationSubmissionController : BaseController
    {
        public FinalExaminationSubmissionController(ApplicationDbContext db,
                                                    IFlashMessage flashMessage,
                                                    ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }
        
        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var barcodes = _db.Barcodes.AsNoTracking()
                                       .IgnoreQueryFilters()
                                       .Where(x => x.Section.TermId == criteria.TermId
                                                   && (string.IsNullOrEmpty(criteria.CourseName)
                                                      || x.Course.Code.Contains(criteria.CourseName)
                                                      || x.Course.NameEn.Contains(criteria.CourseName)))
                                       .Select(x => new 
                                                    {
                                                        BarcodeId = x.Id,
                                                        CourseId = x.CourseId,
                                                        SectionId = x.SectionId,
                                                        CreateAt = x.CreatedAt,
                                                        ApprovedAt = x.ApprovedAt,
                                                        x.IsActive
                                                    })
                                       .ToList();

            var activeBarcodes = barcodes.Where(x => x.IsActive).ToList();

            var barcodeIds = activeBarcodes.Select(x => (long?)x.BarcodeId).ToList();
            var barcodeSectionIds = activeBarcodes.Select(x => x.SectionId)
                                            .Distinct()
                                            .ToList();

            // GRADE SUBMISSION PERIOD
            var registrationTermEndDate = _db.RegistrationTerms.AsNoTracking()
                                                               .Where(x => x.TermId == criteria.TermId 
                                                                           && x.Type == "g")
                                                               .Max(y => y.EndedAt);

            var sections = _db.Sections.AsNoTracking()
                                       .Where(x => x.TermId == criteria.TermId
                                                   && (criteria.FacultyId == 0 
                                                       || x.Course.FacultyId == criteria.FacultyId)
                                                   && (criteria.DepartmentId == 0 
                                                       || x.Course.DepartmentId == criteria.DepartmentId)
                                                   && (criteria.InstructorId == 0
                                                       || x.MainInstructorId == criteria.InstructorId)
                                                   && (string.IsNullOrEmpty(criteria.CourseName)
                                                       || x.Course.Code.Contains(criteria.CourseName)
                                                       || x.Course.NameEn.Contains(criteria.CourseName))
                                                   && (criteria.IsWaitingToSubmit 
                                                       ? !barcodeSectionIds.Contains(x.Id) && !barcodeSectionIds.Contains(x.ParentSectionId ?? 0)
                                                       : barcodeSectionIds.Contains(x.Id)))
                                       .OrderBy(x => x.Course.Code)
                                            .ThenBy(x => x.Number)
                                       .Select(x => new FinalExaminationSubmission
                                                    {
                                                          SectionId = x.Id,
                                                          CourseId = x.CourseId,
                                                          FacultyNameEn = x.Course.Faculty.NameEn,
                                                          CourseCode = x.Course.Code,
                                                          CourseName = x.Course.NameEn,
                                                          CourseCredit = x.Course.Credit,
                                                          CourseLecture = x.Course.Lecture,
                                                          CourseOther = x.Course.Other,
                                                          CourseLab = x.Course.Lab,
                                                          SectionNumber = x.Number,
                                                          InstructorTitle = x.MainInstructor.Title.NameEn,
                                                          InstructorFirstName = x.MainInstructor.FirstNameEn,
                                                          InstructorLastName = x.MainInstructor.LastNameEn,
                                                          FinalDate = x.FinalDate,
                                                          DueAt = x.FinalDate.HasValue ? x.FinalDate.Value.AddDays(7) : registrationTermEndDate,
                                                          IsSpecialCase = x.IsSpecialCase,
                                                          IsOutbound = x.IsOutbound,
                                                          CourseRateId = x.Course.CourseRateId
                                                    })
                                       .ToList();

            var allSectionIds = new List<long>();
            var masterSectionIds = sections.Select(x => x.SectionId).ToList();
            allSectionIds.AddRange(masterSectionIds);

            var jointSections = _db.Sections.AsNoTracking()
                                            .Where(x => x.TermId == criteria.TermId
                                                        && x.ParentSectionId != null
                                                        && masterSectionIds.Contains(x.ParentSectionId.Value))
                                            .Select(x => new {
                                                                 SectionId = x.Id,
                                                                 ParentSectionId = x.ParentSectionId,
                                                                 CourseCode = x.Course.Code,
                                                                 SectionNumber = x.Number,
                                                                 CourseRateId = x.Course.CourseRateId
                                                             })
                                            .ToList();

            var jointSectionIds = jointSections.Select(x => x.SectionId).ToList();
            allSectionIds.AddRange(jointSectionIds);
            
            var registrationCourses = _db.RegistrationCourses.AsNoTracking()
                                                             .Where(x => allSectionIds.Contains(x.SectionId.Value)
                                                                         && x.TermId == criteria.TermId
                                                                         && x.Status != "d")
                                                             .Select(x => new {
                                                                                  SectionId = x.SectionId,
                                                                                  GradeName = x.Grade.Name,
                                                                                  ParentSectionId = x.Section.ParentSectionId,
                                                                                  IsGradePublished = x.IsGradePublished,
                                                                                  StudentId = x.StudentId
                                                                              })
                                                             .ToList();

            var studentRawScores = _db.StudentRawScores.Where(x => barcodeIds.Contains(x.BarcodeId))
                                                       .Select(x => new
                                                                    {
                                                                        CourseId = x.Barcode.CourseId,
                                                                        IsGradePublished = x.RegistrationCourse.IsGradePublished,
                                                                        GradeName = x.RegistrationCourse.Grade.Name,
                                                                        StudentId = x.StudentId,
                                                                        SectionId = x.SectionId,
                                                                        ParentSectionId = x.Section.ParentSectionId
                                                                    })
                                                       .ToList();
            foreach (var item in sections)
            {
                item.SubmissionAt = barcodeSectionIds.Contains(item.SectionId) 
                                    ? barcodes.Where(y => y.CourseId == item.CourseId
                                                          && y.SectionId == item.SectionId).Min(y => y.CreateAt)
                                    : null;
                item.ApprovedAt = barcodeSectionIds.Contains(item.SectionId) 
                                    ? barcodes.Where(y => y.CourseId == item.CourseId
                                                          && y.SectionId == item.SectionId
                                                          && y.ApprovedAt != null)
                                                    .OrderByDescending(y => y.CreateAt)
                                                    .Select(y => y.ApprovedAt).FirstOrDefault()
                                    : null;
                                    
                var joints = jointSections.Where(x => x.ParentSectionId == item.SectionId)
                                          .Select(x => $"{ x.CourseCode } ({ x.SectionNumber })")
                                          .ToList();

                item.JointSections = joints.Any() ? string.Join(", ", joints) : "";

                item.RegisteredStudent = registrationCourses.Count(x => (x.SectionId == item.SectionId
                                                                         || x.ParentSectionId == item.SectionId));

                item.PublishedStudent = registrationCourses.Count(x => (x.SectionId == item.SectionId
                                                                        || x.ParentSectionId == item.SectionId)
                                                                       && x.IsGradePublished
                                                                       && x.GradeName?.ToLower() != "w");

                item.WithdrawStudent = registrationCourses.Count(x => (x.SectionId == item.SectionId
                                                                       || x.ParentSectionId == item.SectionId
                                                                      && x.GradeName?.ToLower() == "w"));

                item.GradeEnteredStudent = studentRawScores.Where(x => (x.SectionId == item.SectionId
                                                                        || x.ParentSectionId == item.SectionId)
                                                                       && x.GradeName?.ToLower() != "w")
                                                           .Select(x => x.StudentId)
                                                           .Distinct()
                                                           .Count();
            }

            var model = new FinalExaminationSubmissionViewModel
                        {
                            Criteria = criteria,
                            Results = sections
                        };

            return View(model);
        }

        public void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Instructors = _selectListProvider.GetInstructors();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }

            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            }
        }
    }
}