using AutoMapper;
using KeystoneLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using Microsoft.AspNetCore.Authorization;
using KeystoneLibrary.Models.USpark;
using KeystoneLibrary.Exceptions;
using KeystoneLibrary.Models.DataModels.Questionnaire;

namespace Keystone.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class USparkQuestionnaireController : BaseController
    {
        public USparkQuestionnaireController(ApplicationDbContext db, 
                                             IFlashMessage flashMessage, 
                                             IMapper mapper) : base(db, flashMessage, mapper){ }

        [HttpGet("Questionnaire/{id}")]
        public IActionResult Get(long id)
        {
            if(_db.Sections.Any(x => x.Id == id))
            {
                var mainInstructorId = _db.Sections.Single(x => x.Id == id).MainInstructorId;
                var instructorIds = _db.SectionSlots.Where(x => x.SectionId == id
                                                                && x.InstructorId.HasValue)
                                                    .Select(x => x.InstructorId.Value)
                                                    .Distinct()
                                                    .ToList();
                if (mainInstructorId > 0)
                {
                    instructorIds.Add(mainInstructorId ?? 0);
                }

                var questionnaire = _db.Questionnaires.Include(x => x.QuestionGroups)
                                                          .ThenInclude(x => x.Questions)
                                                              .ThenInclude(x => x.Answers)
                                                      .FirstOrDefault();

                var result = new USparkQuestionnaireViewModel();
                if (questionnaire != null && questionnaire.QuestionGroups != null && questionnaire.QuestionGroups.Any())
                {
                    result.Id = questionnaire.Id;
                    result.InstructorIds = instructorIds.Distinct()
                                                        .ToList();
                    result.KSSectionId = id;
                    result.QuestionGroups = questionnaire.QuestionGroups
                                                         .Select(x => new USparkQuestionGroupViewModel
                                                                      {
                                                                          Id = x.Id,
                                                                          NameEn = x.NameEn,
                                                                          Questions = x.Questions.Select(y => new USparkQuestionViewModel
                                                                                                              {
                                                                                                                  Order = y.Order,
                                                                                                                  NameEn = y.QuestionEn,
                                                                                                                  QuestionType = y.QuestionType,
                                                                                                                  Answers = y.Answers.Select(z => new USparkAnswerViewModel
                                                                                                                                                  {
                                                                                                                                                      Id = z.Id,
                                                                                                                                                      Value = z.Value,
                                                                                                                                                      AnswerEn = z.AnswerEn
                                                                                                                                                  })
                                                                                                                                     .ToList()
                                                                                                              })
                                                                                                 .ToList()
                                                                      })
                                                         .ToList();
                }

                return Success(result);
            }
            else
            {
                return Error(QuestionnaireException.SectionNotFound());
            }
        }

        [HttpPost("Submit")]
        public async Task<IActionResult> Submit(USparkQuestionnaireResponseViewModel model)
        {
            if (model != null && model.Responses != null && model.Responses.Any())
            {
                var student = _db.Students.FirstOrDefault(x => x.Code == model.StudentCode);
                if (student != null)
                {
                    var section = _db.Sections.SingleOrDefault(x => x.Id == model.KSSectionId);
                    if (section != null)
                    {
                        bool IsSurveyedAlready = _db.RegistrationCourses.Any(x => x.StudentId == student.Id
                                                                                  && x.SectionId == section.Id
                                                                                  && x.Status != "d"
                                                                                  && x.IsSurveyed);
                        
                        if (!IsSurveyedAlready)
                        {
                            await _db.RegistrationCourses.Where(x => x.StudentId == student.Id
                                                                    && x.SectionId == section.Id
                                                                    && x.Status != "d")
                                                         .ForEachAsync(x => x.IsSurveyed = true);

                            foreach (var response in model.Responses)
                            {
                                _db.Responses.Add(new Response
                                                {
                                                    StudentId = student.Id,
                                                    TermId = section.TermId,
                                                    CourseId = section.CourseId,
                                                    SectionId = model.KSSectionId,
                                                    InstructorId = response.InstructorId,
                                                    QuestionnaireId = model.QuestionnaireId,
                                                    QuestionGroupId = response.QuestionGroupId,
                                                    AnswerId = response.AnswerId,
                                                    AnswerRemark = response.AnswerRemark
                                                });
                            }

                            _db.SaveChanges();
                            return Success(0);
                        }
                        else
                        {
                            return Error(QuestionnaireException.SurveyedAlreadyExist());
                        }
                    }
                    else
                    {
                        return Error(QuestionnaireException.SectionNotFound());                    
                    }
                }
                else
                {
                    return Error(QuestionnaireException.StudentNotFound());
                }
            }
            return Error(QuestionnaireException.ObjectIsNull());
        }
    }
}