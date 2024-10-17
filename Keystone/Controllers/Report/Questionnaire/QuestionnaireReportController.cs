using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class QuestionnaireReportController : BaseController
    {
        protected readonly IQuestionnaireProvider _questionnaireProvider;
        public QuestionnaireReportController(ApplicationDbContext db,
                                             ISelectListProvider selectListProvider,
                                             IFlashMessage flashMessage,
                                             IQuestionnaireProvider questionnaireProvider) : base(db, flashMessage, selectListProvider) 
        { 
            _questionnaireProvider = questionnaireProvider;
        }

        public IActionResult Index(Criteria criteria, string returnUrl, string summary)
        {
            ViewBag.ReturnUrl = returnUrl;

            var userInstructorId = GetInstructorId();
            bool IsAllow = false;
            bool IsInstructor = false;
            
            // QUESTIONNAIRE MEMBER
            var IsQuestionnaireMember = _db.QuestionnaireMembers.Any(x => x.InstructorId == userInstructorId);
            IsAllow = IsQuestionnaireMember;

            // PROGRAM DIRECTOR
            if (!IsAllow)
            {
                var questionnaireMemberApproved = _db.QuestionnaireApprovals.AsNoTracking()
                                                                            .Where(x => x.SectionId == criteria.SectionId
                                                                                      && x.InstructorId == criteria.InstructorId
                                                                                      && x.TermId == criteria.TermId)
                                                                            .All(x => x.Status == "s" || x.Status == "p");
                var facultyMember = _db.FacultyMembers.AsNoTracking()
                                                      .Where(x => x.InstructorId == userInstructorId && x.Type == "pd")
                                                      .ToList();

                if (facultyMember != null && facultyMember.Any())
                {
                    var filterCourseGroupIds = facultyMember.Select(x => x.FilterCourseGroupId);
                    var filterCourseIds = _db.FilterCourseGroupDetails.AsNoTracking()
                                                                      .Where(x => filterCourseGroupIds.Contains(x.FilterCourseGroupId))
                                                                      .Select(x => x.CourseId)
                                                                      .ToList();

                    IsAllow = filterCourseIds.Contains(criteria.CourseId) && questionnaireMemberApproved;
                }
            }

            // INSTRUCTOR
            if (!IsAllow)
            {
                var pdApproved = _db.QuestionnaireApprovals.Where(x => x.SectionId == criteria.SectionId
                                                                       && x.InstructorId == criteria.InstructorId
                                                                       && x.TermId == criteria.TermId)
                                                           .All(x => x.Status == "p");

                var IsAllowInstructor = _db.Sections.AsNoTracking()
                                                    .Any(x => x.Id == criteria.SectionId
                                                            && (x.MainInstructorId == userInstructorId
                                                                || x.SectionSlots.Any(y => y.InstructorId == userInstructorId)));

                IsAllow = (IsAllowInstructor && pdApproved); 
                IsInstructor = true;
            }

            if (criteria.AcademicLevelId == 0)
            {
                criteria.AcademicLevelId = _db.AcademicLevels.AsNoTracking()
                                                             .SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
            }
            if (criteria.TermId == 0)
            {
                criteria.TermId = _questionnaireProvider.GetQuestionnaireTermByAcademicLevelId(criteria.AcademicLevelId).Id;
            }
                
            if (IsInstructor)
            {
                criteria.InstructorId = userInstructorId;
            }

            CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.CourseId, criteria.InstructorId);
            if (criteria.AcademicLevelId == 0 
                && criteria.TermId == 0
                && criteria.CourseId == 0
                && criteria.SectionId == 0
                && criteria.InstructorId == 0)
            {        
                _flashMessage.Warning(Message.RequiredData);
                return View(new QuestionnaireReportViewModel
                            {
                                Criteria = criteria
                            });
            }

            if (!IsAllow)
            {
                return View(new QuestionnaireReportViewModel
                            {
                                Criteria = criteria
                            });
            }

            var section = _db.Sections.IgnoreQueryFilters()
                                      .Include(x => x.MainInstructor)
                                          .ThenInclude(x => x.Title)
                                      .SingleOrDefault(x => x.IsActive && x.Id == criteria.SectionId);

            //var instructors = _db.SectionSlots.Include(x => x.Instructor)
            //                                      .ThenInclude(x => x.Title)
            //                                  .Where(x => x.SectionId == criteria.SectionId)
            //                                  .Select(x => x.Instructor.FullNameEn)
            //                                  .Distinct()
            //                                  .ToList();
            //if( section.MainInstructor != null
            //     && !_db.SectionSlots.Any(x => x.SectionId == criteria.SectionId 
            //                                   && section.MainInstructorId == x.InstructorId))
            //{
            //    instructors.Add(section.MainInstructor.FullNameEn);
            //    instructors.Distinct().ToList();
            //}

            //var instructorNames = string.Join(", ", instructors);
            var instructorNames = _db.Instructors.IgnoreQueryFilters()
                                                 .FirstOrDefault(x => x.Id == criteria.InstructorId)?.FullNameEn;

            var questionnaireApproval = _db.QuestionnaireApprovals.IgnoreQueryFilters().FirstOrDefault(x => x.IsActive && x.SectionId == criteria.SectionId && x.InstructorId == criteria.InstructorId);
            var responses = _db.Responses.Where(x => x.TermId == criteria.TermId
                                                     && x.CourseId == criteria.CourseId
                                                     && x.SectionId == criteria.SectionId
                                                     && x.InstructorId == criteria.InstructorId)
                                         .ToList();

            QuestionnaireReportViewModel model = new QuestionnaireReportViewModel
                                                 {
                                                     Criteria = criteria,
                                                     Summary = summary,
                                                     TermText = _db.Terms.SingleOrDefault(x => x.Id == criteria.TermId)?.TermText,
                                                     CourseCodeAndName = _db.Courses.SingleOrDefault(x => x.Id == criteria.CourseId)?.CodeAndName,
                                                     SectionNumber = section?.Number,
                                                     LecturerNames = instructorNames,
                                                     TotalScore = 0,
                                                     EvaluatedCount = responses?.Select(x => x.StudentId)
                                                                               ?.Distinct()
                                                                               ?.Count() ?? 0,
                                                     TakenSeat = section?.SeatUsed ?? 0,
                                                     TotalEnrolled = questionnaireApproval?.TotalEnrolled ?? 0
                                                 };

            var questionnaireId = responses.FirstOrDefault()?.QuestionnaireId;
            if (questionnaireId > 0)
            {
                var questionGroups = _db.QuestionGroups.Include(x => x.Questions)
                                                           .ThenInclude(x => x.Answers)
                                                       .Where(x => x.QuestionnaireId == questionnaireId
                                                                   && (criteria.QuestionGroupIds == null
                                                                       || criteria.QuestionGroupIds.Any(y => y == x.Id)))
                                                       .ToList();

                var questionAndAnswers = responses.Where(x => x != null)
                                                  .ToList();
                                                  
                questionGroups.ForEach(x => x.Questions
                                             .ForEach(y => y.Responses = questionAndAnswers.Where(z => z.Answer.QuestionId == y.Id)
                                                                                           .ToList()));
                model.Groups = questionGroups;
                model.TotalScore = questionAndAnswers == null || questionAndAnswers.Count == 0
                                   ? 0 : questionAndAnswers.Where(x => x.Answer.Question.IsCalculate
                                                                       && x.Answer.Value != "0")
                                                           .Average(x => Decimal.TryParse(x.Answer.Value, out decimal answer) ? answer : 0);
            }

            return View(model);
        }

        private void CreateSelectList(long academicLevelId = 0, long termId = 0, long courseId = 0, long instructorId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);
            ViewBag.Sections = _selectListProvider.GetSectionByCourseId(termId, courseId);
            ViewBag.QuestionGroups = _selectListProvider.GetQuestionGroups();
            ViewBag.Instructors = _selectListProvider.GetInstructorById(instructorId);
        }
    }
}