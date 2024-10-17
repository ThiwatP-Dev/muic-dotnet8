using KeystoneLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.EntityFrameworkCore;
using KeystoneLibrary.Models.DataModels;

namespace Keystone.Controllers
{
    public class SectionPeriodController : BaseController
    {
        public SectionPeriodController(ApplicationDbContext db,
                                       ISelectListProvider selectListProvider,
                                       IFlashMessage flashMessage) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            var sectionPeriod = _db.SectionPeriods.Include(x => x.Term)
                                                  .Where(x => (criteria.AcademicLevelId == 0
                                                               || criteria.AcademicLevelId == x.Term.AcademicLevelId)
                                                              && (criteria.TermId == 0
                                                                  || criteria.TermId == x.TermId))
                                                  .OrderBy(x => x.Term.TermText)
                                                  .IgnoreQueryFilters()
                                                  .GetPaged(criteria, page);
            return View(sectionPeriod);
        }

        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            return View(new SectionPeriod());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SectionPeriod model, string returnUrl)
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
                    _db.SectionPeriods.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new 
                                                           { 
                                                               AcademicLevelId = model.AcademicLevelId,
                                                               TermId = model.TermId,
                                                               ReturnUrl = returnUrl
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
        public async Task<ActionResult> Edit(SectionPeriod model, string returnUrl)
        {
            if (IsExisted(model))
            {
                CreateSelectList(model.AcademicLevelId);
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }
            else
            {
                var sectionPeriod = Find(model.Id);
                if (ModelState.IsValid && await TryUpdateModelAsync<SectionPeriod>(sectionPeriod))
                {
                    try
                    {
                        await _db.SaveChangesAsync();
                        ViewBag.ReturnUrl = returnUrl;
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index), new 
                                                               { 
                                                                   AcademicLevelId = model.AcademicLevelId,
                                                                   TermId = model.TermId,
                                                               });
                    }
                    catch
                    {
                        CreateSelectList(sectionPeriod.AcademicLevelId);
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
                _db.SectionPeriods.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        public bool IsExisted(SectionPeriod model)
        {
            var isExisted = _db.SectionPeriods.Any(x => x.TermId == model.TermId
                                                        && x.Id != model.Id);
            return isExisted;
        }

        private SectionPeriod Find(long? id)
        {
            var model = _db.SectionPeriods.IgnoreQueryFilters()
                                          .SingleOrDefault(x => x.Id == id);

            return model;
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}