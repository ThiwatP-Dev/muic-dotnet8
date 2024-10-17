using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using KeystoneLibrary.Models.DataModels;

namespace Keystone.Controllers
{
    //[PermissionAuthorize("GradeApproval", "")]
    public class GradeApprovalController : BaseController
    {
        protected readonly IRegistrationProvider _registrationProvider;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICacheProvider _cacheProvider;
        private readonly IGradeProvider _gradeProvider;
        private readonly IAcademicProvider _academicProvider;

        public GradeApprovalController(ApplicationDbContext db, 
                                       IFlashMessage flashMessage, 
                                       IRegistrationProvider registrationProvider,
                                       ISelectListProvider selectListProvider,
                                       ICacheProvider cacheProvider,
                                       UserManager<ApplicationUser> user,
                                       IAcademicProvider academicProvider,
                                       IGradeProvider gradeProvider) : base(db, flashMessage, selectListProvider)
        {
            _registrationProvider = registrationProvider;
            _userManager = user;
            _cacheProvider = cacheProvider;
            _gradeProvider = gradeProvider;
            _academicProvider = academicProvider;
        }
        
        public IActionResult Index(Criteria criteria)
        {
            if (criteria.TermId == 0)
            {
                criteria.AcademicLevelId = _db.AcademicLevels.SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
                criteria.TermId = _academicProvider.GetCurrentTermByAcademicLevelId(criteria.AcademicLevelId).Id;
            }

            CreateSelectList(criteria.AcademicLevelId);
            var instructorId = GetInstructorId();
            var user = GetUser();
            var filterCourseGroupIds = _db.FacultyMembers.Where(x => x.InstructorId == instructorId
                                                                     && x.Type == "c")
                                                         .AsNoTracking()
                                                         .Select(x => x.FilterCourseGroupId)
                                                         .ToList();

            // GET COURSES
            var filterCourseIds = _db.FilterCourseGroupDetails.Where(x => filterCourseGroupIds.Contains(x.FilterCourseGroupId))
                                                              .AsNoTracking()
                                                              .Select(x => x.CourseId)
                                                              .ToList();

            var gradeMember = _db.GradeMembers.SingleOrDefault(x => x.UserId == user.Id);
            var barcodes = _db.Barcodes.Where(x => (gradeMember != null
                                                    || filterCourseIds.Contains(x.CourseId))
                                                    && (string.IsNullOrEmpty(criteria.CourseCode) 
                                                        || x.Course.Code.Contains(criteria.CourseCode))
                                                    && x.Section.TermId == criteria.TermId)
                                       .AsNoTracking()
                                       .Select(x => new GradeApprovalDetail
                                                    {
                                                        BarcodeId = x.Id,
                                                        SectionId = x.SectionId,
                                                        CourseId = x.CourseId,
                                                        SectionNumber = x.Section.Number,
                                                        Term = x.Section.Term.TermText,
                                                        BarcodeNumber = x.BarcodeNumber,
                                                        CourseCode = x.Course.Code,
                                                        JointSectionIds = x.SectionIds,
                                                        IsPublished = x.IsPublished,
                                                        IsApproved = x.ApprovedAt == null ? false : true
                                                    })
                                       .OrderBy(x => x.CourseCode)
                                           .ThenBy(x => x.NumberValue)
                                       .ToList();

            var barcodeSectionIds = barcodes.Select(x => x.SectionId).ToList();
            var sectionIds = new List<long>();
            foreach (var barcode in barcodes)
            {
                // MASTER SECTION
                sectionIds.Add(barcode.SectionId);
                // JOINT SECTION
                var jointSectionIds = (string.IsNullOrEmpty(barcode.JointSectionIds) ? new List<long>() 
                                                                                     : JsonConvert.DeserializeObject<List<long>>(barcode.JointSectionIds));
                barcode.JointSectionIdList = jointSectionIds;
                sectionIds.AddRange(jointSectionIds);
            }
            
            // GET ALL SECTION
            var sections = _db.Sections.Where(x => sectionIds.Contains(x.Id))
                                       .AsNoTracking()
                                       .Select(x => new
                                                    {
                                                        SectionId = x.Id,
                                                        SectionNumber = x.Number,
                                                        CourseCode = x.Course.Code
                                                    })
                                       .ToList();

            var barcodeIds = barcodes.Select(x => x.BarcodeId).ToList();
            // var studentRawScores = _db.StudentRawScores.Include(x => x.RegistrationCourse)
            //                                                .ThenInclude(x => x.Grade)
            //                                            .Include(x => x.StudentRawScoreDetails)
            //                                                .ThenInclude(x => x.MarkAllocation)
            //                                            .Where(x => sectionIds.Contains(x.SectionId))
            //                                            .AsNoTracking()
            //                                            .ToList();
            // var barcodeCourseIds = barcodes.Select(x => x.CourseId).ToList();
            // var barcodeMarkAllocations = _db.MarkAllocations.Where(x => barcodeCourseIds.Contains(x.CourseId)
            //                                                             && x.TermId == criteria.TermId)
            //                                                 .ToList();

            var gradeSkip = _db.Grades.Where(x => x.Name.ToUpper() == "I" 
                                                  || x.Name.ToUpper() == "AU" )
                                      .Select(x => (long?)x.Id)
                                      .ToList();
                                      
            var studentRawScores = _db.StudentRawScores.Where(x => sectionIds.Contains(x.SectionId))
                                                       .AsNoTracking()
                                                       .Select(x => new StudentRawScoreViewModel
                                                                {
                                                                    StudentId = x.StudentId,
                                                                    SectionId = x.SectionId,
                                                                    CourseCode = x.Course.CodeAndSpecialChar, 
                                                                    CourseCredit = x.Course.CreditText,
                                                                    CourseName = x.Course.NameEn,
                                                                    CourseId = x.CourseId,
                                                                    StudentCode = x.Student.Code,
                                                                    StudentTitle = x.Student.Title.NameEn,
                                                                    StudentFirstNameEn = x.Student.FirstNameEn,
                                                                    StudentLastNameEn = x.Student.LastNameEn,
                                                                    StudentMidNameEn = x.Student.MidNameEn,
                                                                    SectionNumber = x.Section.Number,
                                                                    SectionType = x.Section.ParentSectionId == null ? "Master" : "Joint",
                                                                    IsPaid = x.RegistrationCourse.IsPaid,
                                                                    IsWithdrawal = x.RegistrationCourse.GradeName == "W" ? true : false,
                                                                    IsGradePublish = x.RegistrationCourse.IsGradePublished,
                                                                    RegistrationCourseId = x.RegistrationCourseId,
                                                                    Id = x.Id,
                                                                    GradeId = x.GradeId,
                                                                    TotalScore = x.TotalScore,
                                                                    IsSkipGrading = x.IsSkipGrading,
                                                                    StudentRawScoreDetails = x.StudentRawScoreDetails,
                                                                    GradeName = x.Grade.Name,
                                                                    GradeWeight = x.Grade.Weight ?? 0,
                                                                    Percentage = x.TotalScore,
                                                                    IsCalcGrade = x.Grade == null ? false : x.Grade.IsCalculateGPA,
                                                                    Credit = x.Course.Credit,
                                                                    DepartmentCode = x.Student.AcademicInformation.Department.Code,
                                                                    GradeTemplateId = x.GradeTemplateId,
                                                                    BarcodeId = x.BarcodeId
                                                                })
                                                       .ToList();
            foreach (var barcode in barcodes)
            {
                var allSectionIds = new List<long?>();
                allSectionIds.Add(barcode.SectionId);

                // var jointSectionIds = (string.IsNullOrEmpty(barcode.JointSectionIds)
                //                        ? new List<long>() : JsonConvert.DeserializeObject<List<long>>(barcode.JointSectionIds));
                var jointSections = sections.Where(x => barcode.JointSectionIdList.Contains(x.SectionId)).ToList();
                var courseAndSections = new List<string>();

                foreach (var joint in jointSections)
                {
                    var courseAndSection = $"{ joint.CourseCode }({ joint.SectionNumber })";
                    courseAndSections.Add(courseAndSection);
                    allSectionIds.Add(joint.SectionId);
                }

                // var allocation = barcodeMarkAllocations.Where(x => x.CourseId == barcode.CourseId)
                //                                        .OrderBy(x => x.Sequence)
                //                                        .ToList();
                // var studentRawScoreDetails = allocation.Select(x => new StudentRawScoreDetail
                //                                                     {  
                //                                                         MarkAllocationId = x.Id,
                //                                                         MarkAllocation = x
                //                                                     })
                //                                        .ToList();

                var studentRawScoresByBarcode = studentRawScores.Where(x => x.BarcodeId == barcode.BarcodeId).ToList();
                var studentRawScoresBySectionIds = studentRawScores.Where(x => allSectionIds.Contains(x.SectionId)).ToList();
                
                var studentResultRawScore = studentRawScoresBySectionIds.Where(x => x.Id != 0
                                                                                      && !(x.IsSkipGrading 
                                                                                           || x.IsWithdrawal 
                                                                                           || x.TotalScore == null 
                                                                                           || gradeSkip.Contains(x.GradeId)))
                                                                        .ToList();

                var studentScoreAllocations = studentResultRawScore.Select(x => new StudentScoreAllocation
                                                                                {
                                                                                  CourseCredit = x.Credit,
                                                                                  GradeWeight = x.GradeWeight,
                                                                                  TotalScore = x.TotalScore
                                                                                })
                                                                   .ToList();

                var classStatistics = _gradeProvider.GetClassStatisticsGradeScoreSummary(studentScoreAllocations);
                barcode.Mean = classStatistics.Mean;
                barcode.Max = classStatistics.Max;
                barcode.Min = classStatistics.Min;
                barcode.SD = classStatistics.SD;

                barcode.JointSection = courseAndSections.Any() ? string.Join(", ", courseAndSections)
                                                               : "N/A";

                barcode.GradeEnteredStudent = studentRawScoresByBarcode.Distinct().Count(x => x.TotalScore != null                      
                                                                                              && !x.IsWithdrawal);

                barcode.SpecifyGradeStudent = studentRawScoresByBarcode.Distinct().Count(x => x.TotalScore == null
                                                                                              && x.GradeId != null
                                                                                              && !x.IsWithdrawal);

                barcode.NoScoreStudent = studentRawScoresBySectionIds.Distinct().Count(x => x.TotalScore == null
                                                                                            && x.GradeId == null
                                                                                            && !x.IsWithdrawal);

                barcode.PublishedStudent = studentRawScoresBySectionIds.Distinct().Count(x => x.IsGradePublish
                                                                                                   && !x.IsWithdrawal);
                
                barcode.WithdrawnStudent = studentRawScoresBySectionIds.Distinct().Count(x => x.IsWithdrawal);
                barcode.SectionStudent = studentRawScoresBySectionIds.Distinct().Count();
                var sectionStudentExcludeWithdraw = barcode.SectionStudent - barcode.WithdrawnStudent;

                barcode.Status = (barcode.PublishedStudent > 0 && barcode.NoScoreStudent > 0) ? "pa" 
                                                                                              : barcode.PublishedStudent != sectionStudentExcludeWithdraw
                                                                                              ? "pe" 
                                                                                              : barcode.PublishedStudent == sectionStudentExcludeWithdraw
                                                                                              ? "pu"
                                                                                              : "N/A";
            }

            var model = new GradeApprovalViewModel
                        {
                            Criteria = criteria,
                            Details = barcodes
                        };

            return View(model);
        }

        //[PermissionAuthorize("GradeApproval", PolicyGenerator.Write)]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public ActionResult Approves(GradeApprovalViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var barcodeIds = model.Details.Where(x => x.IsChecked && !x.IsPublished)
                                          .Select(x => x.BarcodeId)
                                          .ToList();
            var number = Update(barcodeIds);
            return RedirectToAction(nameof(Index), model.Criteria);
        }

        //[PermissionAuthorize("GradeApproval", PolicyGenerator.Write)]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public ActionResult Approve(List<long> barcodeIds, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var number = Update(barcodeIds);
            return RedirectToAction(nameof(Index));
        }

        private string Update(List<long> barcodeIds)
        {
            var instructorId = GetInstructorId();
            var user = GetUser();
            var gradeMember = _db.GradeMembers.SingleOrDefault(x => x.UserId == user.Id);
            var filterCourseGroupIds = _db.FacultyMembers.Where(x => x.InstructorId == instructorId
                                                                     && x.Type == "c")
                                                         .Select(x => x.FilterCourseGroupId)
                                                         .ToList();

            var filterCourseIds = _db.FilterCourseGroupDetails.Where(x => filterCourseGroupIds.Contains(x.FilterCourseGroupId))
                                                              .Select(x => x.CourseId)
                                                              .ToList();

            var barcodes = _db.Barcodes.Where(x => barcodeIds.Contains(x.Id)
                                                   && (filterCourseIds.Contains(x.CourseId)
                                                       || gradeMember != null))
                                       .ToList();
            try
            {
                foreach (var item in barcodes)
                {
                    item.ApprovedAt = DateTime.UtcNow;
                    item.ApprovedBy = user.ToString();
                }

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
            }

            return barcodes.FirstOrDefault().BarcodeNumber;
        }

        //[PermissionAuthorize("GradeApproval", PolicyGenerator.Write)]
        public ActionResult Revert(long barcodeId, string returnUrl)
        {
            var allJointSectionIds = new List<long>();
            var barcode = _db.Barcodes.SingleOrDefault(x => x.Id == barcodeId);
            try
            {
                barcode.ApprovedAt = null;
                barcode.ApprovedBy = null;
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
            }

            return RedirectToAction(nameof(Index));
        }

        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public ActionResult Report(long barcodeId, GradeApprovalViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var instructorId = GetInstructorId();
            var instructor = _db.Instructors.Where(x => x.Id == instructorId)
                                            .Select(x => new
                                                         {
                                                             Title = x.Title.NameEn,
                                                             FirstName = x.FirstNameEn,
                                                             LastName = x.LastNameEn
                                                         })
                                            .SingleOrDefault();
            var author = instructor != null ? $"{instructor.Title} {instructor.FirstName} {instructor.LastName}" : "";

            var barcodeIds = new List<long>();
            if (barcodeId > 0)
            {
                barcodeIds.Add(barcodeId);
            }
            else 
            {
                var checkedBarcodeIds = model.Details.Where(x => x.IsChecked && !x.IsPublished)
                                                     .Select(x => x.BarcodeId)
                                                     .ToList();
                barcodeIds.AddRange(checkedBarcodeIds);
            }

            var term = _db.Barcodes.Where(x => barcodeIds.Contains(x.Id))
                                   .Select(x => x.Section.Term)
                                   .FirstOrDefault();
            var body = _gradeProvider.GetGradeApprovalDetailForPreview(barcodeIds);
            var report = new ReportViewModel
            {
                TermId = term.Id,
                AcademicLevelId = term.AcademicLevelId,
                Title = "Report",
                Subject = "Grading Result Report",
                Creator = "Keystone V.xxxx",
                Author = author,
                Body = body
            };

            return View(report);
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}