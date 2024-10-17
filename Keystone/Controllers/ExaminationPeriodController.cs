using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class ExaminationPeriodController : BaseController 
    {
        protected readonly IExaminationProvider _examinationProvider;

        public ExaminationPeriodController(ApplicationDbContext db,
                                           IFlashMessage flashMessage,
                                           ISelectListProvider selectListProvider,
                                           IExaminationProvider examinationProvider) : base(db, flashMessage, selectListProvider)
        {
            _examinationProvider = examinationProvider;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var model = _db.ExaminationPeriods.Include(x => x.Term)
                                              .Where(x => x.Term.AcademicLevelId == criteria.AcademicLevelId
                                                          && x.TermId == criteria.TermId
                                                          && (criteria.MidtermDate == null || x.MidtermDate == criteria.MidtermDate)
                                                          && (criteria.FinalDate == null || x.FinalDate == criteria.FinalDate)
                                                          && (criteria.Period == null || x.Period == criteria.Period))
                                              .OrderBy(x => x.Period)
                                              .GetPaged(criteria, page);
            return View(model);
        }

        public IActionResult Details(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = _db.ExaminationPeriods.Include(x => x.Term)
                                              .SingleOrDefault(x => x.Id == id);
                                              
            model.Courses = _db.ExaminationCoursePeriods.Include(x => x.Course)
                                                        .Where(x => x.Period == model.Period)
                                                        .ToList();  
            return View(model);
        }

        public IActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new ExaminationPeriod());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ExaminationPeriod model, string returnUrl)
        {
            if (_examinationProvider.IsPeriodExists(model.Id, model.TermId, model.Period))
            {
                CreateSelectList(model.AcademicLevelId);
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    _db.ExaminationPeriods.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return Redirect(returnUrl);
                }
                catch
                {
                    CreateSelectList(model.AcademicLevelId);
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            CreateSelectList(model.AcademicLevelId);
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public IActionResult Edit(long id, string returnUrl)
        {
            var model = _examinationProvider.GetExaminationPeriod(id);
            model.AcademicLevelId = _db.Terms.SingleOrDefault(x => x.Id == model.TermId).AcademicLevelId;
            CreateSelectList(model.AcademicLevelId);
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ExaminationPeriod model, string returnUrl)
        {
            if (_examinationProvider.IsPeriodExists(model.Id, model.TermId, model.Period))
            {
                CreateSelectList();
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }

            var modelToUpdate = _examinationProvider.GetExaminationPeriod(model.Id);
            if (ModelState.IsValid && await TryUpdateModelAsync<ExaminationPeriod>(modelToUpdate))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return Redirect(returnUrl);
                }
                catch
                {
                    CreateSelectList();
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(model);
                }
            }

            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        public ActionResult Delete(long id)
        {
            var model = _db.ExaminationPeriods.SingleOrDefault(x => x.Id == id);
            if (model == null)
            {
                _flashMessage.Danger(Message.UnableToDelete);
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _db.ExaminationPeriods.Remove(model);
                _db.SaveChanges();

                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
                return RedirectToAction(nameof(Index));
            }
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