using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("Term", "")]
    public class TermController : BaseController
    {
        protected readonly IRegistrationProvider _registrationProvider;
        public TermController(ApplicationDbContext db,
                              IFlashMessage flashMessage,
                              IRegistrationProvider registrationProvider,
                              ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) 
        {
            _registrationProvider = registrationProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList();
            var models = _db.Terms.Include(x => x.AcademicLevel)
                                  .Include(x => x.TermType)
                                  .OrderByDescending(x => x.AcademicYear)
                                      .ThenBy(x => x.AcademicLevel.NameEn)
                                          .ThenBy(x => x.AcademicTerm)
                                  .Where(x => (criteria.AcademicLevelId == 0 
                                               || x.AcademicLevelId == criteria.AcademicLevelId)
                                               && (criteria.AcademicTerm == 0
                                                   || criteria.AcademicTerm == null
                                                   || x.AcademicTerm == criteria.AcademicTerm)
                                               && (criteria.AcademicYear == 0
                                                   || criteria.AcademicYear == null
                                                   || x.AcademicYear == criteria.AcademicYear)
                                               && (string.IsNullOrEmpty(criteria.Status)
                                                   || x.IsActive == Convert.ToBoolean(criteria.Status)))
                                  .IgnoreQueryFilters()
                                  .GetPaged(criteria, page);
            return View(models);
        }

        [PermissionAuthorize("Term", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            var model = new Term()
                        {
                            AcademicYear = DateTime.Now.Year,
                            TotalWeeksCount = 12
                        };

            return View(model);
        }

        [PermissionAuthorize("Term", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Term model, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;

            var existMessage = IsExistTerm(model);
            if (!string.IsNullOrEmpty(existMessage))
            {
                _flashMessage.Danger(existMessage);
                return View(model);
            }

            if (ModelState.IsValid)
            { 
                _db.Terms.Add(model);
                try
                {
                    _db.SaveChanges();

                    if (model.IsRegistration)
                    {
                        _registrationProvider.UpdateStudentCreditLoadByRegistrationTerm(model.AcademicLevelId);
                    }

                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new
                                                           {
                                                               AcademicLevelId = model.AcademicLevelId,
                                                               AcademicYear = model.AcademicYear
                                                           });
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            Term model = Find(id);
            return View(model);
        }

        [PermissionAuthorize("Term", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Term model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();

            if (ModelState.IsValid)
            {
                var existMessage = IsExistTerm(model);
                if (!string.IsNullOrEmpty(existMessage))
                {
                    _flashMessage.Danger(existMessage);
                    return View(model);
                }

                try
                {
                    var term = Find(model.Id);
                    bool isRegistrationTermChanged = term.IsRegistration != model.IsRegistration;

                    term.AcademicLevelId = model.AcademicLevelId;
                    term.AcademicYear = model.AcademicYear;
                    term.AcademicTerm = model.AcademicTerm;
                    term.StartedAt = model.StartedAt;
                    term.EndedAt = model.EndedAt;
                    term.TotalWeeksCount = model.TotalWeeksCount;
                    term.FirstRegistrationEndedAt = model.FirstRegistrationEndedAt;
                    term.FirstRegistrationPaymentEndedAt = model.FirstRegistrationPaymentEndedAt;
                    term.AddDropPaymentEndedAt = model.AddDropPaymentEndedAt;
                    term.LastPaymentEndedAt = model.LastPaymentEndedAt;
                    term.MinimumCredit = model.MinimumCredit;
                    term.MaximumCredit = model.MaximumCredit;
                    term.IsCurrent = model.IsCurrent;
                    term.IsAdvising = model.IsAdvising;
                    term.IsRegistration = model.IsRegistration;
                    term.IsAdmission = model.IsAdmission;
                    term.IsQuestionnaire = model.IsQuestionnaire;
                    term.IsActive = model.IsActive;

                    await _db.SaveChangesAsync();

                    if (isRegistrationTermChanged)
                    {
                        _registrationProvider.UpdateStudentCreditLoadByRegistrationTerm(model.AcademicLevelId);
                    }

                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new
                                                            {
                                                                AcademicLevelId = model.AcademicLevelId,
                                                                AcademicYear = model.AcademicYear
                                                            });
                }
                catch 
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(model);
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("Term", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Term model = Find(id);
            try
            {
                _db.Terms.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index), new
                                                   {
                                                       AcademicLevelId = model.AcademicLevelId,
                                                       AcademicYear = model.AcademicYear
                                                   });
        }

        private Term Find(long? id) 
        {
            var model = _db.Terms.IgnoreQueryFilters()
                                 .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList()
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.TermTypes = _selectListProvider.GetTermTypes();
            ViewBag.Statuses = _selectListProvider.GetActiveStatuses();
        }

        private string IsExistTerm(Term model)
        {
            var message = "term already exist in database.";
            var terms = _db.Terms.Where(x => x.Id != model.Id
                                             && x.AcademicLevelId == model.AcademicLevelId);

            if (terms.Any(x => model.AcademicYear == x.AcademicYear
                               && model.AcademicTerm == x.AcademicTerm))
            {
                return Message.ExistedTerm;
            }
            else if (model.IsCurrent && terms.Any(x => x.IsCurrent))
            {
                return $"Current { message }";
            }
            else if (model.IsAdvising && terms.Any(x => x.IsAdvising))
            {
                return $"Advising { message }";
            }
            else if (model.IsRegistration && terms.Any(x => x.IsRegistration))
            {
                return $"Registration { message }";
            }
            else if (model.IsAdmission && terms.Any(x => x.IsAdmission))
            {
                return $"Admission { message }";
            }
            else if (model.IsQuestionnaire && terms.Any(x => x.IsQuestionnaire))
            {
                return $"Questionnaire { message }";
            }
            else
            {
                return "";
            }
        }
    }
}