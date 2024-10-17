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
    public class GradePublishController : BaseController
    {
        protected readonly IRegistrationProvider _registrationProvider;
        private readonly UserManager<ApplicationUser> _userManager;

        public GradePublishController(ApplicationDbContext db, 
                                      IFlashMessage flashMessage, 
                                      IRegistrationProvider registrationProvider,
                                      ISelectListProvider selectListProvider,
                                      UserManager<ApplicationUser> user) : base(db, flashMessage, selectListProvider)
        {
            _registrationProvider = registrationProvider;
            _userManager = user;
        }
        
        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList();
            var user = GetUser();
            var gradeMember = _db.GradeMembers.SingleOrDefault(x => x.UserId == user.Id);
            var barcodes = _db.Barcodes.Where(x => gradeMember != null
                                                   && x.BarcodeNumber == criteria.Code 
                                                   && !x.IsPublished)
                                       .AsNoTracking()
                                       .Select(x => new GradeApprovalDetail
                                                    {
                                                        BarcodeId = x.Id,
                                                        SectionId = x.SectionId,
                                                        CourseId = x.CourseId,
                                                        TermId = x.Section.TermId,
                                                        SectionNumber = x.Section.Number,
                                                        Term = x.Section.Term.TermText,
                                                        BarcodeNumber = x.BarcodeNumber,
                                                        CourseCode = x.Course.Code,
                                                        JointSectionIds = x.SectionIds,
                                                        IsApproved = x.ApprovedAt == null ? false : true
                                                    })
                                       .ToList();

            var sectionIds = new List<long>();
            foreach (var barcode in barcodes)
            {
                // MASTER SECTION
                sectionIds.Add(barcode.SectionId);

                // JOINT SECTION
                var jointSectionIds = (string.IsNullOrEmpty(barcode.JointSectionIds) ? new List<long>() 
                                                                                     : JsonConvert.DeserializeObject<List<long>>(barcode.JointSectionIds));
                sectionIds.AddRange(jointSectionIds);
            }

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
            var studentRawScores = _db.StudentRawScores.Include(x => x.RegistrationCourse)
                                                           .ThenInclude(x => x.Grade)
                                                       .AsNoTracking()
                                                       .ToList();
            foreach (var barcode in barcodes)
            {
                var allSectionIds = new List<long>();
                allSectionIds.Add(barcode.SectionId);

                var jointSectionIds = (string.IsNullOrEmpty(barcode.JointSectionIds)
                                       ? new List<long>() : JsonConvert.DeserializeObject<List<long>>(barcode.JointSectionIds));
                var jointSections = sections.Where(x => jointSectionIds.Contains(x.SectionId)).ToList();
                var courseAndSections = new List<string>();

                foreach (var joint in jointSections)
                {
                    var courseAndSection = $"{ joint.CourseCode }({ joint.SectionNumber })";
                    courseAndSections.Add(courseAndSection);
                    allSectionIds.Add(joint.SectionId);
                }

                barcode.JointSection = courseAndSections.Any() ? string.Join(", ", courseAndSections)
                                                               : "N/A";

                barcode.GradeEnteredStudent = studentRawScores.Where(x => x.BarcodeId == barcode.BarcodeId
                                                                          && x.TotalScore != null                      
                                                                          && x.RegistrationCourse.Grade?.Name.ToLower() != "w")
                                                              .Select(x => x.StudentId)
                                                              .Distinct()
                                                              .Count();

                barcode.SpecifyGradeStudent = studentRawScores.Where(x => x.BarcodeId == barcode.BarcodeId
                                                                          && x.TotalScore == null
                                                                          && x.GradeId != null
                                                                          && x.RegistrationCourse.Grade?.Name.ToLower() != "w")
                                                              .Select(x => x.StudentId)
                                                              .Distinct()
                                                              .Count();

                barcode.NoScoreStudent = studentRawScores.Where(x => allSectionIds.Contains(x.SectionId)
                                                                     && x.TotalScore == null
                                                                     && x.GradeId == null
                                                                     && x.RegistrationCourse.Grade?.Name.ToLower() != "w")
                                                         .Select(x => x.StudentId)
                                                         .Distinct()
                                                         .Count();

                barcode.PublishedStudent = studentRawScores.Where(x => allSectionIds.Contains(x.SectionId)
                                                                       && x.RegistrationCourse.IsGradePublished
                                                                       && x.RegistrationCourse.Grade?.Name.ToLower() != "w")
                                                           .Select(x => x.StudentId)
                                                           .Distinct()
                                                           .Count();
                
                barcode.WithdrawnStudent = studentRawScores.Where(x => allSectionIds.Contains(x.SectionId)
                                                                       && x.RegistrationCourse.Grade?.Name.ToLower() == "w")
                                                           .Select(x => x.StudentId)
                                                           .Distinct()
                                                           .Count();

                barcode.SectionStudent = studentRawScores.Where(x => allSectionIds.Contains(x.SectionId))
                                                         .Select(x => x.StudentId)
                                                         .Distinct()
                                                         .Count();

                barcode.Status = (barcode.PublishedStudent > 0 && barcode.NoScoreStudent > 0) ? "pa" 
                                                                                              : (barcode.PublishedStudent + barcode.WithdrawnStudent) != barcode.SectionStudent
                                                                                              ? "pe" 
                                                                                              : (barcode.PublishedStudent + barcode.WithdrawnStudent) == barcode.SectionStudent
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

        public ActionResult Publishes(GradeApprovalViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var barcodeIds = model.Details.Where(x => x.IsChecked)
                                          .Select(x => x.BarcodeId)
                                          .ToList();
            var number = Update(barcodeIds);
            return RedirectToAction(nameof(Index), new Criteria { Code = number });
        }

        public ActionResult Publish(List<long> barcodeIds, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var number = Update(barcodeIds);
            return RedirectToAction(nameof(Index), new Criteria { Code = number });
        }

        private string Update(List<long> barcodeIds)
        {
            var user = GetUser();
            var gradeMember = _db.GradeMembers.SingleOrDefault(x => x.UserId == user.Id);
            var allJointSectionIds = new List<long>();
            var barcodes = _db.Barcodes.Where(x => gradeMember != null
                                                   && barcodeIds.Contains(x.Id))
                                       .ToList();
            try
            {
                foreach (var item in barcodes)
                {
                    var jointSectionIds = (string.IsNullOrEmpty(item.SectionIds)
                                           ? new List<long>() 
                                           : JsonConvert.DeserializeObject<List<long>>(item.SectionIds));
                                           
                    allJointSectionIds.Add(item.SectionId);
                    allJointSectionIds.AddRange(jointSectionIds);
                    item.PublishedAt = DateTime.UtcNow;
                    item.PublishedBy = user.UserName;
                    item.IsPublished = true;
                }
                
                var studentRawScores = _db.StudentRawScores.Where(x => barcodeIds.Contains(x.BarcodeId ?? 0)
                                                                       // && !x.IsSkipGrading
                                                                       && !x.RegistrationCourse.IsGradePublished
                                                                       && x.GradeId != null
                                                                       && x.RegistrationCourse.Grade.Name.ToLower() != "w")
                                                           .Select(x => new
                                                                        {
                                                                            RegistrationCourseId = x.RegistrationCourseId,
                                                                            GradeId = x.GradeId,
                                                                            GradeName = x.Grade.Name,
                                                                            StudentId = x.StudentId
                                                                        })
                                                           .ToList();

                var registrationCourseIds = studentRawScores.Select(x => x.RegistrationCourseId).ToList();
                var studentIds = studentRawScores.Select(x => x.StudentId)
                                                 .Distinct()
                                                 .ToList();
                var registrationCourses = _db.RegistrationCourses.Where(x => registrationCourseIds.Contains(x.Id)).ToList();
                foreach (var item in registrationCourses)
                {
                    var studentRawScore = studentRawScores.SingleOrDefault(x => x.RegistrationCourseId == item.Id);
                    item.GradeId = studentRawScore.GradeId;
                    item.GradeName = studentRawScore.GradeName;
                    item.GradePublishedAt = DateTime.UtcNow;
                    item.IsGradePublished = true;
                }

                _db.SaveChanges();
                var academicInformations = _db.AcademicInformations.Where(x => studentIds.Contains(x.StudentId)).ToList();
                foreach(var item in academicInformations)
                {
                    item.IsHasGradeUpdate = true;
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

        public ActionResult Revert(long barcodeId, string returnUrl)
        {
            var allJointSectionIds = new List<long>();
            var barcode = _db.Barcodes.SingleOrDefault(x => x.Id == barcodeId);
            try
            {
                var jointSectionIds = (string.IsNullOrEmpty(barcode.SectionIds)
                                       ? new List<long>() : JsonConvert.DeserializeObject<List<long>>(barcode.SectionIds));
                allJointSectionIds.Add(barcode.SectionId);
                allJointSectionIds.AddRange(jointSectionIds);
                barcode.IsPublished = false;
                
                var studentRawScores = _db.StudentRawScores.Where(x => (x.BarcodeId == barcodeId
                                                                        || allJointSectionIds.Contains(x.SectionId))
                                                                        // && !x.IsSkipGrading
                                                                        // && x.TotalScore != null
                                                                        && x.RegistrationCourse.IsGradePublished
                                                                        && x.RegistrationCourse.Grade.Name.ToLower() != "w")
                                                           .ToList();

                var registrationCourseIds = studentRawScores.Select(x => x.RegistrationCourseId).ToList();
                var studentIds = studentRawScores.Select(x => x.StudentId)
                                                 .Distinct()
                                                 .ToList();
                var registrationCourses = _db.RegistrationCourses.Where(x => registrationCourseIds.Contains(x.Id)).ToList();
                var grade = _db.Grades.SingleOrDefault(x => x.Name.ToLower() == "x");

                foreach (var item in registrationCourses)
                {
                    item.GradeId = grade.Id;
                    item.GradeName = grade.Name;
                    item.GradePublishedAt = null;
                    item.IsGradePublished = false;
                }

                _db.SaveChanges();

                var academicInformations = _db.AcademicInformations.Where(x => studentIds.Contains(x.StudentId)).ToList();
                foreach(var item in academicInformations)
                {
                    item.IsHasGradeUpdate = true;
                }
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
            }

            return RedirectToAction(nameof(Index), new Criteria { Code = barcode.BarcodeNumber });
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