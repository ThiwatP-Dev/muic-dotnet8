using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Admission;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTable
{
    [PermissionAuthorize("AdmissionRound", "")]
    public class AdmissionRoundController : BaseController
    {
        protected readonly IAdmissionProvider _admissionProvider;

        public AdmissionRoundController(ApplicationDbContext db,
                                        IFlashMessage flashMessage,
                                        ISelectListProvider selectListProvider,
                                        IAdmissionProvider admissionProvider) : base(db, flashMessage, selectListProvider)
        {
            _admissionProvider = admissionProvider;
        }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList(criteria.AcademicLevelId);
            var models = _db.AdmissionRounds.Include(x => x.AcademicLevel)
                                            .Include(x => x.AdmissionTerm)
                                            .Where(x => (criteria.AcademicLevelId == 0
                                                         || x.AcademicLevelId == criteria.AcademicLevelId)
                                                         && (criteria.TermId == 0
                                                             || x.AdmissionTermId == criteria.TermId))
                                            .OrderBy(x => x.AcademicLevel.NameEn)
                                                .ThenByDescending(x => x.AdmissionTerm.AcademicYear)
                                                .ThenByDescending(x => x.AdmissionTerm.AcademicTerm)
                                                .ThenByDescending(x => x.Round)
                                            .IgnoreQueryFilters()
                                            .GetPaged(criteria, page);
            return View(models);
        }

        [PermissionAuthorize("AdmissionRound", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View(new AdmissionRound());
        }

        [PermissionAuthorize("AdmissionRound", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdmissionRound model)
        {
            if (_admissionProvider.IsExistAdmissionRound(model.Id, model.AdmissionTermId, model.Round))
            {
                CreateSelectList(model.AcademicLevelId);
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }

            if (ModelState.IsValid && !string.IsNullOrEmpty(model.EndedAtText) && !string.IsNullOrEmpty(model.FirstClassAtText))
            {
                try
                {
                    _db.AdmissionRounds.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { AcademicLevelId = model.AcademicLevelId, TermId = model.AdmissionTermId });
                }
                catch
                {
                    CreateSelectList(model.AcademicLevelId);
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            CreateSelectList(model.AcademicLevelId);
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            AdmissionRound model = _admissionProvider.GetAdmissionRound(id);
            CreateSelectList(model.AcademicLevelId);
            return View(model);
        }

        [PermissionAuthorize("AdmissionRound", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = _admissionProvider.GetAdmissionRound(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<AdmissionRound>(model))
            {
                if (_admissionProvider.IsExistAdmissionRound(model.Id, model.AdmissionTermId, model.Round))
                {
                    CreateSelectList(model.AcademicLevelId);
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }

                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { AcademicLevelId = model.AcademicLevelId, TermId = model.AdmissionTermId });
                }
                catch 
                {
                    CreateSelectList(model.AcademicLevelId);
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(model);
                }
            }

            CreateSelectList(model.AcademicLevelId);
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("AdmissionRound", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            AdmissionRound model = _admissionProvider.GetAdmissionRound(id);
            try
            {
                _db.AdmissionRounds.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId > 0)
            {
                ViewBag.AdmissionTerms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}