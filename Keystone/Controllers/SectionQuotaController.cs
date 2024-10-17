using KeystoneLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.EntityFrameworkCore;
using KeystoneLibrary.Models.DataModels;

namespace Keystone.Controllers
{
    public class SectionQuotaController : BaseController
    {
        public SectionQuotaController(ApplicationDbContext db,
                                      ISelectListProvider selectListProvider,
                                      IFlashMessage flashMessage) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            var quotas = _db.SectionQuotas.Include(x => x.Term)
                                          .Include(x => x.Faculty)
                                          .Where(x => (criteria.AcademicLevelId == 0
                                                       || criteria.AcademicLevelId == x.Term.AcademicLevelId)
                                                      && (criteria.TermId == 0
                                                          || criteria.TermId == x.TermId)
                                                      && (criteria.FacultyId == 0
                                                          || criteria.FacultyId == x.FacultyId))
                                          .OrderBy(x => x.Term.TermText)
                                              .ThenBy(x => x.Faculty.NameEn)
                                          .IgnoreQueryFilters()
                                          .GetPaged(criteria, page);

            return View(quotas);
        }

        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            return View(new SectionQuota());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SectionQuota model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (IsExisted(model))
                {
                    CreateSelectList(model.AcademicLevelId);
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }

                try
                {
                    _db.SectionQuotas.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new 
                                                           { 
                                                               AcademicLevelId = model.AcademicLevelId,
                                                               TermId = model.TermId,
                                                               FacultyId = model.FacultyId
                                                           });
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    CreateSelectList(model.AcademicLevelId);
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.AcademicLevelId);
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = Find(id);
            CreateSelectList(model.AcademicLevelId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SectionQuota model, string returnUrl)
        {
            if (IsExisted(model))
            {
                CreateSelectList(model.AcademicLevelId);
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }
            else
            {
                var sectionQuota = Find(model.Id);
                if (ModelState.IsValid && await TryUpdateModelAsync<SectionQuota>(sectionQuota))
                {
                    try
                    {
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index), new 
                                                               { 
                                                                   AcademicLevelId = model.AcademicLevelId,
                                                                   TermId = model.TermId,
                                                                   FacultyId = model.FacultyId
                                                               });
                    }
                    catch
                    {
                        CreateSelectList(sectionQuota.AcademicLevelId);
                        ViewBag.ReturnUrl = returnUrl;
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.AcademicLevelId);
            return View(model);
        }

        public ActionResult Delete(long id)
        {
            var model = Find(id);
            try
            {
                _db.SectionQuotas.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        public bool IsExisted(SectionQuota model)
        {
            var isExist = _db.SectionQuotas.Any(x => x.TermId == model.TermId
                                                     && x.FacultyId == model.FacultyId
                                                     && x.Id != model.Id);
            return isExist;
        }

        private SectionQuota Find(long? id)
        {
            var model = _db.SectionQuotas.Include(x => x.Term)
                                         .IgnoreQueryFilters()
                                         .SingleOrDefault(x => x.Id == id);

            model.AcademicLevelId = model.Term.AcademicLevelId;
            return model;
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();

            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }
        }
    }
}