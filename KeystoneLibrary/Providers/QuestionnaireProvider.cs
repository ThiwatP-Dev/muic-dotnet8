using System.Net;
using System.Text;
using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Questionnaire;
using KeystoneLibrary.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace KeystoneLibrary.Providers
{
    public class QuestionnaireProvider : BaseProvider, IQuestionnaireProvider
    {
        private readonly IHttpClientProxy _httpClientProxy;

        public QuestionnaireProvider(ApplicationDbContext db, 
                                        IHttpClientProxy httpClientProxy, 
                                        IMapper mapper,
                                        IConfiguration config) : base(config, db, mapper)
        {
            _httpClientProxy = httpClientProxy;
        }

        public bool IsDuplicatedTerm(long id, long termId)
        {
            return _db.QuestionnairePeriods.Any(x => x.Id != id
                                                     && x.TermId == termId);
        }

        public Questionnaire GetQuestionnaireById(long id)
        {
            var questionnaire = _db.Questionnaires.Include(x => x.QuestionGroups)
                                                      .ThenInclude(x => x.Questions)
                                                      .ThenInclude(x => x.Answers)
                                                  .SingleOrDefault(x => x.Id == id);
            
            return questionnaire;
        }

        public Term GetQuestionnaireTermByAcademicLevelId(long academicLevelId)
        {
            var questionnaireTerm = _db.Terms.AsNoTracking()
                                             .FirstOrDefault(x => x.AcademicLevelId == academicLevelId
                                                                  && x.IsQuestionnaire);
            
            if (questionnaireTerm == null)
            {
                var currentTerm = _db.Terms.AsNoTracking()
                                           .FirstOrDefault(x => x.AcademicLevelId == academicLevelId
                                                                && x.IsCurrent);
                return currentTerm;
            }
            return questionnaireTerm;
        }

        public QuestionnaireByInstructorAndSectionViewModel GetQuestionnaireByInstructorAndSectionReport(List<QuestionnaireByInstructorAndSectionStudent> studentList)
        {
            var model = new QuestionnaireByInstructorAndSectionViewModel();
            var students = studentList.GroupBy(x => new { x.StudentId, x.SectionId, x.InstructorId })
                                      .Select(x => new QuestionnaireByInstructorAndSectionStudent
                                                   {
                                                    //    StudentId = x.Key,
                                                       TermId = x.FirstOrDefault().TermId,
                                                    //    StudentCode = x.FirstOrDefault().StudentCode,
                                                    //    Title = x.FirstOrDefault().Title,
                                                    //    FirstName = x.FirstOrDefault().FirstName,
                                                    //    MidName = x.FirstOrDefault().MidName,
                                                    //    LastName = x.FirstOrDefault().LastName,
                                                       QuestionGroupId = x.FirstOrDefault().QuestionGroupId,
                                                       QuestionId = x.FirstOrDefault().QuestionId,
                                                       Value = x.FirstOrDefault().Value,
                                                       Questions = x.Select(z => new QuestionnaireByInstructorAndSectionQuestion
                                                                                 {
                                                                                     QuestionId = z.QuestionId,
                                                                                     Answer = z.Value,
                                                                                     AnswerText = z.ValueText,
                                                                                     Type = z.Type,
                                                                                     IsCalculate = z.IsCalulate
                                                                                 })
                                                                    .ToList()
                                                   })
                                      .ToList();

            var questionGroups = _db.QuestionGroups.Include(x => x.Questions)
                                                   .Select(x => new QuestionnaireByInstructorAndSectionGroup
                                                                {
                                                                    QuestionGroupId = x.Id,
                                                                    GroupName = x.NameEn,
                                                                    Questions = x.Questions.Select(y => new QuestionnaireByInstructorAndSectionQuestion
                                                                                                        {
                                                                                                            QuestionId = y.Id,
                                                                                                            Order = y.Order,
                                                                                                            Type = y.QuestionType,
                                                                                                            IsCalculate = y.IsCalculate
                                                                                                        })
                                                                                           .ToList()
                                                                })
                                                   .ToList();

            model.Students = students;
            model.Header = questionGroups;
            return model;
        }

        public void BuildScore(long termId)
        {
            var sections = _db.Sections.AsNoTracking()
                                       .Include(x => x.Course)
                                       .Include(x => x.MainInstructor)
                                       .Include(x => x.SectionSlots)
                                            .ThenInclude(x => x.Instructor)
                                       .Where(x => x.TermId == termId
                                                   && !x.IsClosed
                                                   && x.Status == "a")
                                       .ToList();

            var sectionIdWithParentSectionIds = sections.Select(x => new { SectionId = x.Id, x.ParentSectionId })
                                                        .ToList();

            var registrationCourses = _db.RegistrationCourses.AsNoTracking()
                                                             .Where(x => x.SectionId != null
                                                                         && x.TermId == termId
                                                                         && x.Status != "d")
                                                             .ToList();

            var responses = _db.Responses.Include(x => x.Answer)
                                         .AsNoTracking()
                                         .Where(x => x.TermId == termId
                                                    && x.Answer.Question.IsCalculate
                                                    && x.Answer.Value != "0")
                                         .ToList();

            var questionnaireApprovals = _db.QuestionnaireApprovals.Where(x => x.TermId == termId)
                                                                   .ToList();

                                             
            foreach (var section in sections)
            {
                var regisStudents = registrationCourses.Where(x => x.SectionId == section.Id).Count();

                var instructors = section.SectionSlots.Where(x => x.InstructorId.HasValue)
                                                        .Select(x => new { x.InstructorId.Value, x.Instructor.FullNameEn })
                                                        .Distinct()
                                                        .ToList();

                instructors = instructors.Distinct().ToList();

                // JOINT SECTION
                if (section.ParentSectionId.HasValue)
                {
                    var masterSection = sections.FirstOrDefault(x => x.Id == section.ParentSectionId.Value);
                    if (masterSection != null)
                    {
                        var jointInstructors = masterSection.SectionSlots.Where(x => x.InstructorId.HasValue)
                                                                            .Select(x => new { x.InstructorId.Value, x.Instructor.FullNameEn })
                                                                            .Distinct()
                                                                            .ToList();
                        instructors = jointInstructors.Distinct().ToList(); 
                    }                                           
                }
                
                if (section.MainInstructorId.HasValue)
                {
                    if (!instructors.Any(x => x.Value == section.MainInstructorId.Value))
                    {
                        instructors.Add( new { section.MainInstructorId.Value, section.MainInstructor.FullNameEn });
                    }
                }           
          
                foreach (var instructor in instructors)
                {
                    // RESPONSE RELATED SECTION
                    long parentSectionId = section.ParentSectionId.HasValue ? section.ParentSectionId.Value : section.Id;
                    var relatedSectionIds = sectionIdWithParentSectionIds.Where(x => x.SectionId == section.Id
                                                                                        || x.SectionId == parentSectionId
                                                                                        || x.ParentSectionId == parentSectionId)
                                                                            .Select(x => x.SectionId)
                                                                            .ToList();

                    var responseRelatedSection = responses.Any(x => relatedSectionIds.Contains(x.SectionId.Value)
                                                                    && x.InstructorId == instructor.Value) 
                                                    ? Math.Round(responses.Where(x => relatedSectionIds.Contains(x.SectionId.Value)
                                                                                        && x.InstructorId == instructor.Value)
                                                                .Select(x => Convert.ToDecimal(x.Answer.Value)).Average(), 2) : 0;

                    var responseRelatedSectionSD = (responses.Any(x => relatedSectionIds.Contains(x.SectionId.Value)
                                                                       && x.InstructorId == instructor.Value) 
                                                    && responses.Count(x => relatedSectionIds.Contains(x.SectionId.Value)
                                                                            && x.InstructorId == instructor.Value) > 1)
                                                    ? FindSD(responses.Where(x => relatedSectionIds.Contains(x.SectionId.Value)
                                                                                        && x.InstructorId == instructor.Value)
                                                                .Select(x => Convert.ToDecimal(x.Answer.Value)).ToList()) : 0;

                    // RESPONSE EACH SECTION
                    var response = responses.Where(x => x.SectionId == section.Id
                                                        && x.InstructorId == instructor.Value) 
                                            .Select(x => x.Answer.Value);

                    decimal totalSectionAnswerValue = 0;
                    decimal totalSectionQuestion = 0;
                    decimal totalSectionScore = 0;
                    decimal totalSectionSD = 0;
                    if (response != null && response.Any())
                    {
                        totalSectionAnswerValue = response.Select(x => Convert.ToDecimal(x)).Sum();
                        totalSectionQuestion = response.Count();
                        totalSectionScore = Math.Round(response.Select(x => Convert.ToDecimal(x)).Average(), 2);
                        if(response.Count() > 1)
                        {
                            totalSectionSD = FindSD(response.Select(x => Convert.ToDecimal(x)).ToList());
                        }
                    }
                    
                    var totalSurveys = responses.Any(x => x.SectionId == section.Id
                                                            && x.InstructorId == instructor.Value) 
                                        ? responses.Where(x => x.SectionId == section.Id
                                                                            && x.InstructorId == instructor.Value)
                                                    .Select(x => x.StudentId).Distinct().Count()
                                        : 0;

                    var approval = questionnaireApprovals.SingleOrDefault(x => x.SectionId == section.Id
                                                                               && x.InstructorId == instructor.Value
                                                                               && x.TermId == termId);
                    if (approval == null)
                    {
                        var newApproval = new QuestionnaireApproval()
                                          {
                                                CourseId = section.CourseId,
                                                SectionId = section.Id,
                                                InstructorId = instructor.Value,
                                                TermId = termId,
                                                TotalSurvey = totalSurveys,
                                                TotalEnrolled = regisStudents,
                                                TotalAnswerValue = totalSectionAnswerValue,
                                                TotalQuestion = totalSectionQuestion,
                                                TotalSectionScore = totalSectionScore,
                                                TotalSectionMJScore = responseRelatedSection,
                                                TotalSectionMJSD = responseRelatedSectionSD,
                                                TotalSectionSD = totalSectionSD,
                                                Status = "w"
                                          };
                                            
                        _db.QuestionnaireApprovals.Add(newApproval);
                    } 
                    else 
                    {
                        approval.TotalSurvey = totalSurveys;
                        approval.TotalEnrolled = regisStudents;
                        approval.TotalAnswerValue = totalSectionAnswerValue;
                        approval.TotalQuestion = totalSectionQuestion;
                        approval.TotalSectionScore = totalSectionScore;
                        approval.TotalSectionMJScore = responseRelatedSection;
                        approval.TotalSectionMJSD = responseRelatedSectionSD;
                        approval.TotalSectionSD = totalSectionSD;
                    }
                }
            }

            _db.SaveChanges();
        }

        private decimal FindSD(List<decimal> raws)
        {
            double sd = 0;
            if(raws.Any())
            {
                var values = raws.Select(x => Convert.ToDouble(x)).ToList();
                //FIND AVERAGE
                double avg = Math.Round(values.Average(), 2);

                //SUM POWER(Xi - X bar) 
                double sumPower = values.Sum(x => Math.Pow(x - avg, 2));

                sd = Math.Sqrt((sumPower / (values.Count() - 1)));

            }

            return Math.Round(Convert.ToDecimal(sd), 2);
        }

        public void SyncQuestionnairePeriodWithUSpark(QuestionnairePeriod model)
        {
            var body = JsonConvert.SerializeObject(new
            {
                ksTermId = model.TermId,
                periodId = model.Id,
                startedAt = model.StartedAt,
                endedAt = model.EndedAt,
                isActive = model.IsActive
            });
            var endpoint = $"{_USparkAPIURL}/semesters/questionairePeriod";
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var header = new Dictionary<string, string>
            {
                {"x-api-key", _USparkAPIKey }
            };

            var response = _httpClientProxy.PostAsync(endpoint, header, content).Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return;
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ArgumentException(responseContent);
            }
            else if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception(responseContent);
            }
            else
            {
                throw new Exception(responseContent);
            }
        }
    }
}