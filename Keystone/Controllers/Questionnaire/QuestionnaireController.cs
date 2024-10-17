using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Questionnaire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class QuestionnaireController : BaseController
    {
        private readonly ICacheProvider _cacheProvider;

        public QuestionnaireController(ApplicationDbContext db,
                                       IFlashMessage flashMessage,
                                       ISelectListProvider selectListProvider, 
                                       IMapper mapper,
                                       ICacheProvider cacheProvider) : base(db, flashMessage, mapper, selectListProvider) 
        { 
            _cacheProvider = cacheProvider;
        }
                                     
        public IActionResult Index(int page = 1)
        {
            var model = _db.Questionnaires.Include(x => x.Faculty)
                                          .Include(x => x.Department)
                                          .Include(x => x.Course)
                                          .IgnoreQueryFilters()
                                          .GetPaged(page);

            return View(model);
        }

        #region Create & Edit Questionnaire Header

        public IActionResult CreateHeader()
        {
            CreateSelectList();
            return View(new Questionnaire());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateHeader(Questionnaire model)
        {
            if (ModelState.IsValid)
            {
                try 
                {
                    _db.Questionnaires.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return View(nameof(CreateQuestionGroup), model);
                }
                catch
                {
                    CreateSelectList(model.FacultyId ?? 0);
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            CreateSelectList(model.FacultyId ?? 0);
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public IActionResult EditHeader(long id)
        {
            CreateSelectList();
            Questionnaire model = Find(id);	
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditHeader(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid && await TryUpdateModelAsync<Questionnaire>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch 
                {
                    CreateSelectList(model.FacultyId ?? 0);
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(model);
                }
            }

            CreateSelectList(model.FacultyId ?? 0);
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

         public ActionResult Delete(long id)
        {
            Questionnaire model = Find(id);
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var questionGroups = model.QuestionGroups;
                    var questions = questionGroups.SelectMany(x => x.Questions)
                                                  .ToList();
                    var answers = questions.SelectMany(x => x.Answers)
                                           .ToList();

                    _db.Answers.RemoveRange(answers);
                    _db.Questions.RemoveRange(questions);
                    _db.QuestionGroups.RemoveRange(questionGroups);
                    _db.Questionnaires.Remove(model);
                    _db.SaveChanges();

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToDelete);
                }
            }
            
            return RedirectToAction(nameof(Index));
        }


        #endregion

        #region Create & Edit Question Group

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateQuestionGroup(Questionnaire model)
        {
            model.QuestionGroup.QuestionnaireId = model.Id;
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.QuestionGroups.Add(model.QuestionGroup);
                    _db.SaveChanges();
                    transaction.Commit();
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                    transaction.Rollback();
                    return View(model);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditQuestionGroup(Questionnaire model)
        {
            return View(model);
        }

        #endregion

        public IActionResult Details()
        {
            var model = new Questionnaire();
            return View(model);
        }

        private Questionnaire Find(long? id) 
        {
            var model = _db.Questionnaires.Include(x => x.QuestionGroups)
                                             .ThenInclude(x => x.Questions)
                                             .ThenInclude(x => x.Answers)
                                          .IgnoreQueryFilters()
                                          .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList(long facultyId = 0)
        {
            ViewBag.Faculties = _selectListProvider.GetFaculties();
            if (facultyId != 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartments(facultyId);
            }

            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.RespondTypes = _selectListProvider.GetRespondType();
        }
    }
}