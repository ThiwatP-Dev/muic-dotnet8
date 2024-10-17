using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Questionnaire;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class QuestionnaireStudentController : BaseController
    {
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IRegistrationProvider _registrationCourse;
        protected readonly IInstructorProvider _instructorProvider;
        public QuestionnaireStudentController(ApplicationDbContext db,
                                              IFlashMessage flashMessage,
                                              ISelectListProvider selectListProvider, 
                                              IMapper mapper,
                                              IAcademicProvider academicProvider,
                                              IRegistrationProvider registrationProvider,
                                              IInstructorProvider instructorProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _academicProvider = academicProvider;
            _registrationCourse = registrationProvider;
            _instructorProvider = instructorProvider;
        }
                                     
        public IActionResult Index(Criteria criteria)
        {
            if (string.IsNullOrEmpty(criteria.Code))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var model = new QuestionnaireStudentViewModel();
            var registrations = _db.RegistrationCourses.Where(x => x.Student.Code == criteria.Code
                                                                   && x.Section.SectionSlots.Any(y => y.InstructorId != null))
                                                       .GroupBy(x => x.Term)
                                                       .Select(x => new QuestionnaireStudentCourse
                                                                    {
                                                                        Term = x.FirstOrDefault().Term.TermText,
                                                                        AcademicYear = x.FirstOrDefault().Term.AcademicYear,
                                                                        AcademicTerm = x.FirstOrDefault().Term.AcademicTerm,
                                                                        InPeriod = _db.QuestionnairePeriods.Any(y => y.TermId == x.FirstOrDefault().TermId
                                                                                                                     && (DateTime.Now >= y.StartedAt
                                                                                                                         && DateTime.Now <= y.EndedAt)),
                                                                        Details = x.Select(y => new QuestionnaireStudentCourseDetail
                                                                                                {
                                                                                                    RegistrationCourseId = y.Id,
                                                                                                    StudentId = y.StudentId,
                                                                                                    TermId = y.TermId,
                                                                                                    CourseCode = y.Course.Code,
                                                                                                    CourseName = y.Course.NameEnAndCredit,
                                                                                                    SectionNumber = y.Section.Number,
                                                                                                    Instructors = y.Section.SectionSlots
                                                                                                                   .GroupBy(z => z.InstructorId)
                                                                                                                   .Select(z => new QuestionnaireStudentInstructorDetail
                                                                                                                                                     {
                                                                                                                                                         InstructorId = z.Key,
                                                                                                                                                         InstructorName = z.FirstOrDefault().Instructor.FullNameEn,
                                                                                                                                                         Response = _db.Responses.Any(a => a.StudentId == y.StudentId
                                                                                                                                                                                           && a.CourseId == y.CourseId
                                                                                                                                                                                           && a.SectionId == y.SectionId
                                                                                                                                                                                           && a.TermId == y.TermId
                                                                                                                                                                                           && a.InstructorId == z.Key)
                                                                                                                                                     })
                                                                                                                                        .ToList()
                                                                                                })
                                                                                   .ToList()
                                                                    })
                                                       .OrderByDescending(x => x.AcademicYear)
                                                           .ThenByDescending(x => x.AcademicTerm)
                                                       .ToList();

            model.Criteria = criteria;
            model.Courses = registrations;
            return View(model);
        }

        public IActionResult Create(long id, long instructorId)
        {
            var registrationCourse = _registrationCourse.GetRegistrationCourse(id);
            var instructorName = _instructorProvider.GetInstructor(instructorId).FullNameEn;
            var model = new QuestionnaireStudentCourseDetail();
            var questionnaire = _db.Questionnaires.Include(x => x.QuestionGroups)
                                                      .ThenInclude(x => x.Questions)
                                                      .ThenInclude(x => x.Answers)
                                                  .SingleOrDefault(x => x.CourseId == registrationCourse.CourseId);
            if (questionnaire == null)
            {
                questionnaire = _db.Questionnaires.Include(x => x.QuestionGroups)
                                                      .ThenInclude(x => x.Questions)
                                                      .ThenInclude(x => x.Answers)
                                                  .SingleOrDefault(x => x.CourseId == null);
            }

            model.StudentId = registrationCourse.StudentId;
            model.StudentCode = registrationCourse.Student.Code;
            model.CourseId = registrationCourse.CourseId;
            model.SectionId = registrationCourse.SectionId;
            model.TermId = registrationCourse.TermId;
            model.InstructorId = instructorId;
            model.CourseCode = registrationCourse.Course.Code;
            model.CourseName = registrationCourse.Course.NameEnAndCredit;
            model.SectionNumber = registrationCourse.SectionNumber;
            model.InstructorName = instructorName;
            model.Questionnaire = questionnaire;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(QuestionnaireStudentCourseDetail model)
        {
            var response = new Response();
            try
            {
                foreach (var item in model.Questionnaire.QuestionGroups)
                {
                    var answerQuestionGroup = new List<QuestionAndAnswerViewModel>();
                    foreach (var question in item.Questions)
                    {
                        var answers = question.QuestionAndAnswers.Where(x => x.Answer != null)
                                                                 .Select(x => new QuestionAndAnswerViewModel
                                                                              {
                                                                                  Question = x.Question,
                                                                                  Answer = x.Answer
                                                                              })
                                                                 .FirstOrDefault();
                        answerQuestionGroup.Add(answers);                          
                    }

                    response = new Response
                               {
                                   StudentId = model.StudentId,
                                   TermId = model.TermId,
                                   CourseId = model.CourseId,
                                   SectionId = model.SectionId,
                                   InstructorId = model.InstructorId,
                                   QuestionnaireId = model.Questionnaire.Id,
                                   QuestionGroupId = item.Id,
                                //    Answer = JsonConvert.SerializeObject(answerQuestionGroup)
                               };

                    _db.Responses.Add(response);
                }
                
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index), new Criteria { Code = model.StudentCode });
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCreate);
                return View(model);
            }
        }
    }
}