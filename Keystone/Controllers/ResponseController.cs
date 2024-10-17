using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class ResponseController : BaseController
    {
        protected readonly IRegistrationProvider _registrationProvider;
        public ResponseController(ApplicationDbContext db,
                                  ISelectListProvider selectListProvider,
                                  IRegistrationProvider registrationProvider,
                                  IFlashMessage flashMessage) : base (db, flashMessage, selectListProvider) 
        {
            _registrationProvider = registrationProvider;
        }
        
        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.SectionId);

            if (criteria.TermId == 0)
            {
                _flashMessage.Danger(Message.RequiredData);
                return View();
            }
            
            decimal studentCount = _registrationProvider.GetTotalRegistrationStudentByTermId(criteria.TermId);

            var models = _db.Responses.Include(x => x.Questionnaire)
                                      .Include(x => x.QuestionGroup)
                                      .Include(x => x.Student)
                                          .ThenInclude(x => x.AcademicInformation)
                                      .Include(x => x.Student)
                                          .ThenInclude(x => x.RegistrationCourses)
                                      .Where(x => (criteria.QuestionnaireId == 0
                                                   || x.QuestionnaireId == criteria.QuestionnaireId)
                                                   && (string.IsNullOrEmpty(criteria.ResponseType)
                                                       || x.Questionnaire.ResponseType == criteria.ResponseType)
                                                   && (criteria.AcademicLevelId == 0
                                                       || x.Student.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId)
                                                   && (criteria.TermId == 0
                                                       || x.TermId == criteria.TermId)
                                                   && (criteria.CourseId == 0
                                                       || x.CourseId == criteria.CourseId)
                                                   && (criteria.SectionId == 0
                                                       || x.SectionId == criteria.SectionId))
                                      .GroupBy(x => x.QuestionnaireId)
                                      .Select(x => new ResponseViewModel
                                                   {
                                                       NameEn = x.First().Questionnaire.NameEn,
                                                       NameTh = x.First().Questionnaire.NameTh,
                                                       QuestionGroupCount = x.Select(y => y.QuestionGroupId)
                                                                             .Distinct()
                                                                             .Count(),
                                                       QuestionCount = x.Select(y => y.QuestionGroup.Questions.Select(z => z.Id))
                                                                        .Distinct()
                                                                        .Count(),
                                                       Response = x.Count(),
                                                       StudentCount = studentCount
                                                   })
                                      .IgnoreQueryFilters()
                                      .GetPaged(criteria, page);

            return View(models);
        }

        private void CreateSelectList(long academicLevelId, long courseId)
        {
            ViewBag.Questionnaires = _selectListProvider.GetQuestionnaire();
            ViewBag.ResponseTypes = _selectListProvider.GetRespondType();
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }

            if (courseId != 0)
            {
                ViewBag.Sections = _selectListProvider.GetSections(courseId);
            }
        }
    }
}