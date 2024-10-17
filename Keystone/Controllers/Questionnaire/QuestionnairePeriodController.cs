using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Questionnaire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("QuestionnairePeriod", "")]
    public class QuestionnairePeriodController : BaseController
    {
        protected readonly IQuestionnaireProvider _questionnaireProvider;

        public QuestionnairePeriodController(ApplicationDbContext db,
                                             IFlashMessage flashMessage,
                                             IQuestionnaireProvider questionnaireProvider,
                                             ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider)
        {
            _questionnaireProvider = questionnaireProvider;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId);
            var model = _db.QuestionnairePeriods.Include(x => x.Term)
                                                .Where(x => (criteria.AcademicLevelId == 0 || x.Term.AcademicLevelId == criteria.AcademicLevelId)
                                                            && (criteria.TermId == 0 || x.TermId == criteria.TermId))
                                                .OrderBy(x => x.Term.AcademicYear)
                                                .ThenBy(x => x.Term.AcademicTerm)
                                                .GetPaged(criteria, page);
            return View(model);
        }

        [PermissionAuthorize("QuestionnairePeriod", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();

            return View(new QuestionnairePeriod());
        }

        [PermissionAuthorize("QuestionnairePeriod", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(QuestionnairePeriod model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                if (_questionnaireProvider.IsDuplicatedTerm(model.Id, model.TermId))
                {
                    CreateSelectList(model.AcademicLevelId);
                    _flashMessage.Danger(Message.DuplicateQuestionnairePeriod);
                    return View(model);
                }

                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        _db.QuestionnairePeriods.Add(model);
                        _db.SaveChanges();

                        //call api 
                        _questionnaireProvider.SyncQuestionnairePeriodWithUSpark(model);

                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return Redirect(returnUrl);
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }

            CreateSelectList(model.AcademicLevelId);
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            QuestionnairePeriod model = Find(id);
            model.AcademicLevelId = model.Term.AcademicLevelId;
            CreateSelectList(model.AcademicLevelId);
            return View(model);
        }

        [PermissionAuthorize("QuestionnairePeriod", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(QuestionnairePeriod model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var period = Find(model.Id);
            if (ModelState.IsValid)
            {
                if (_questionnaireProvider.IsDuplicatedTerm(model.Id, model.TermId))
                {
                    CreateSelectList(model.AcademicLevelId);
                    _flashMessage.Danger(Message.DuplicateQuestionnairePeriod);
                    return View(model);
                }

                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        if (await TryUpdateModelAsync<QuestionnairePeriod>(period))
                        {
                            await _db.SaveChangesAsync();
                            _questionnaireProvider.SyncQuestionnairePeriodWithUSpark(period);

                            transaction.Commit();

                            _flashMessage.Confirmation(Message.SaveSucceed);
                            return Redirect(returnUrl);
                        }
                    }
                    catch {
                        transaction.Rollback();
                    }
                }
            }

            CreateSelectList(model.AcademicLevelId);
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("QuestionnairePeriod", PolicyGenerator.Write)]
        public ActionResult Delete(long id, string returnUrl)
        {
            QuestionnairePeriod model = Find(id);
            try
            {
                _db.QuestionnairePeriods.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return Redirect(returnUrl);
        }

        private QuestionnairePeriod Find(long? id)
        {
            var model = _db.QuestionnairePeriods.Include(x => x.Term)
                                                .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}