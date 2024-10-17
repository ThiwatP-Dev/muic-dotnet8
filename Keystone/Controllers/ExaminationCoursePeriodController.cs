using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class ExaminationCoursePeriodController : BaseController 
    {
        protected readonly IExaminationProvider _examinationProvider;

        public ExaminationCoursePeriodController(ApplicationDbContext db,
                                                 IFlashMessage flashMessage,
                                                 ISelectListProvider selectListProvider,
                                                 IExaminationProvider examinationProvider) : base(db, flashMessage, selectListProvider)
        {
            _examinationProvider = examinationProvider;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList();
            var model = _db.ExaminationCoursePeriods.Include(x => x.Course)
                                                    .Where(x => (criteria.CourseId == 0 || x.CourseId == criteria.CourseId)
                                                                 && (criteria.Period == null || x.Period == criteria.Period)
                                                                 && (criteria.IsEvening == "All"
                                                                     || string.IsNullOrEmpty(criteria.IsEvening)
                                                                     || x.IsEvening == Convert.ToBoolean(criteria.IsEvening))
                                                                 && (criteria.IsSpeacialCase == "All"
                                                                     || string.IsNullOrEmpty(criteria.IsSpeacialCase)
                                                                     || x.IsSpeacialCase == Convert.ToBoolean(criteria.IsSpeacialCase)))
                                                    .GetPaged(criteria, page);
            return View(model);
        }

        public IActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new ExaminationCoursePeriod());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ExaminationCoursePeriod model, string returnUrl)
        {
            if (_examinationProvider.IsCoursePeriodExists(model.Id, model.CourseId, model.IsEvening))
            {
                CreateSelectList();
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.ExaminationCoursePeriods.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return Redirect(returnUrl);
                }
                catch
                {
                    CreateSelectList();
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public IActionResult Edit(long id, string returnUrl)
        {
            var model = _examinationProvider.GetExaminationCoursePeriod(id);
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ExaminationCoursePeriod model, string returnUrl)
        {
            if (_examinationProvider.IsCoursePeriodExists(model.Id, model.CourseId, model.IsEvening))
            {
                CreateSelectList();
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }

            var modelToUpdate = _examinationProvider.GetExaminationCoursePeriod(model.Id);
            if (ModelState.IsValid && await TryUpdateModelAsync<ExaminationCoursePeriod>(modelToUpdate))
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
            var model = _db.ExaminationCoursePeriods.SingleOrDefault(x => x.Id == id);
            if (model == null)
            {
                _flashMessage.Danger(Message.UnableToDelete);
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _db.ExaminationCoursePeriods.Remove(model);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetExaminationCoursePeriodCondition(long courseId)
        {
            var courseCondition = _examinationProvider.GetExaminationCoursePeriodCondition(courseId);
            return Json(courseCondition);
        }

        private void CreateSelectList() 
        {
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.AllYesNoAnswer = _selectListProvider.GetAllYesNoAnswer();
        }
    }
}