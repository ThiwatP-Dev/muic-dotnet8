using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class QuestionnaireCourseGroupReportController : BaseController
    {
        public QuestionnaireCourseGroupReportController(ApplicationDbContext db,
                                                        ISelectListProvider selectListProvider,
                                                        IFlashMessage flashMessage) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(Criteria criteria, string returnUrl, string summary)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.CourseId);
            if (criteria.AcademicLevelId == 0 
                || criteria.TermId == 0
                || criteria.CourseGroupId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var model = new QuestionnaireCourseGroupReportViewModel();

            var courseIds = _db.QuestionnaireCourseGroupDetails.AsNoTracking()
                                                               .Where(x => x.QuestionnaireCourseGroupId == criteria.CourseGroupId)
                                                               .Select(x => x.CourseId)
                                                               .ToList();

            var responses = _db.Responses.AsNoTracking()
                                         .Include(x => x.Course)
                                         .Include(x => x.Section)
                                         .Include(x => x.Instructor)
                                         .Where(x => x.TermId == criteria.TermId
                                                     && courseIds.Contains(x.CourseId.Value))
                                         .ToList();

            if (responses.Any())
            {
                model.Details = responses.GroupBy(x => x.SectionId)
                                         .Select(x => new QuestionnaireCourseGroupDetail
                                                      {
                                                          SectionId = x.FirstOrDefault().Section?.Id ?? 0,
                                                          Course = x.FirstOrDefault().Course?.CourseAndCredit,
                                                          Section = x.FirstOrDefault().Section?.Number,
                                                          Instructor = x.FirstOrDefault().Instructor?.FullNameEn,
                                                          EvaluatedCount = x.Select(y => y.StudentId)
                                                                            .Distinct()
                                                                            .Count(),
                                                          TakenSeat = x.FirstOrDefault().Section.SeatUsed,
                                                          ParentSection = _db.Sections.SingleOrDefault(y => y.Id == x.FirstOrDefault().Section.ParentSectionId),
                                                          JointSections = x.FirstOrDefault().Section.ParentSectionId == null 
                                                                          ? _db.Sections.Where(y => y.ParentSectionId == x.FirstOrDefault().SectionId).ToList()
                                                                          : _db.Sections.Where(y => y.ParentSectionId == x.FirstOrDefault().Section.ParentSectionId).ToList()
                                                      })
                                         .ToList();
                List<QuestionAndAnswerViewModel> allRecords = new List<QuestionAndAnswerViewModel>();
                foreach (var item in model.Details)
                {
                    // var individualResponse = responses.Where(x => x.SectionId == item.SectionId)
                    //                                   .SelectMany(x => x.QuestionAndAnswers)
                    //                                   .ToList();
                    // if(individualResponse != null)
                    // {
                    //     item.IndividualTotalScore = individualResponse.Where(x => x != null)
                    //                                                   .Average(x => Decimal.TryParse(x.Answer, out decimal answer) ? answer : (decimal)0);
                    //     allRecords.AddRange(individualResponse.Where(x => x != null));
                    // }

                    // if (item.ParentSection == null)
                    // {
                    //     item.ParentSection = _db.Sections.SingleOrDefault(x => x.Id == item.SectionId);
                    // }


                    // // Parent Section
                    //  var parentResponses = responses.Where(x => item.ParentSection != null
                    //                                                && x.SectionId == item.ParentSection.Id)
                    //                                    .SelectMany(y => y.QuestionAndAnswers)
                    //                                    .ToList();
                    // var totalParent = (parentResponses != null && parentResponses.Count > 0)
                    //                                         ? parentResponses.Where(x => x != null)
                    //                                                          .Average(x => Decimal.TryParse(x.Answer, out decimal answer) ? answer : (decimal)0) : 0;
                    // allRecords.AddRange(parentResponses.Where(x => x != null));

                    // decimal totalJointSections = 0;
                    // if (item.JointSections != null && item.JointSections.Any())
                    // {
                    //     totalRecord += item.JointSections.Count();
                    //     var relatedResponses = responses.Where(x => item.JointSections.Any(y => y.Id == x.SectionId))
                    //                                     .SelectMany(x => x.QuestionAndAnswers)
                    //                                     .ToList();

                       

                    //     totalJointSections = (relatedResponses != null && relatedResponses.Count > 0)
                    //                           ? relatedResponses.Average(x => Decimal.TryParse(x.Answer, out decimal answer) ? answer : (decimal)0) : 0;

                    //     allRecords.AddRange(relatedResponses.Where(x => x != null));
                    // }
                    // item.RelatedSectionScore = totalParent + totalJointSections;
                    // totalScore += item.RelatedSectionScore;
                }
                model.TotalScore = allRecords.Average(x => Decimal.TryParse(x.Answer, out decimal answer) ? answer : 0).ToString(StringFormat.TwoDecimal);
            }

            return View(model);
        }

        private void CreateSelectList(long academicLevelId = 0, long termId = 0, long courseId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.QuestionnaireCourseGroups = _selectListProvider.GetQuestionnaireCourseGroups();
        }
    }
}