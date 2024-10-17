using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class ExaminationCourseConditionController : BaseController
    {
        protected readonly IExaminationProvider _examinationProvider;
        private UserManager<ApplicationUser> _userManager { get; }
        
        public ExaminationCourseConditionController(ApplicationDbContext db,
                                                    IFlashMessage flashMessage,
                                                    ISelectListProvider selectListProvider,
                                                    IExaminationProvider examinationProvider,
                                                    UserManager<ApplicationUser> user) : base(db, flashMessage, selectListProvider)
        {
            _examinationProvider = examinationProvider;
            _userManager = user;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList();

            var model = _db.ExaminationCourseConditions.Where(x => criteria.Condition == "All"
                                                                   || string.IsNullOrEmpty(criteria.Condition)
                                                                   || x.Condition == criteria.Condition)
                                                       .ToList();

            model = model.Where(x => criteria.CourseId == 0 
                                     || JsonConvert.DeserializeObject<List<long>>(x.CourseIds).Contains(criteria.CourseId))
                         .ToList();

            model = model.Select(x => 
                                     {
                                         x.Courses = JsonConvert.DeserializeObject<List<long>>(x.CourseIds);
                                         x.CourseList = _db.Courses.Where(y => x.Courses.Contains(y.Id))
                                                                   .OrderBy(y => y.Code)
                                                                   .ToList();
                                         return x;
                                     })
                         .ToList();

            return View(model.AsQueryable().GetPaged(criteria, page));
        }

        public IActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new ExaminationCourseCondition());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ExaminationCourseCondition model, string returnUrl)
        {
            try
            {
                model.CourseIds = JsonConvert.SerializeObject(model.Courses) ?? "[]";
                _db.ExaminationCourseConditions.Add(model);
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

        public IActionResult Edit(long id, string returnUrl)
        {
            var model = _examinationProvider.GetExaminationCourseCondition(id);
            model.Courses = JsonConvert.DeserializeObject<List<long>>(model.CourseIds);
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ExaminationCourseCondition model, string returnUrl)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                var condition = _examinationProvider.GetExaminationCourseCondition(model.Id);
                condition.CourseIds = JsonConvert.SerializeObject(model.Courses) ?? "[]";
                condition.Condition = model.Condition;
                condition.UpdatedAt = DateTime.UtcNow;
                condition.UpdatedBy = userId;

                _db.SaveChanges();

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

        public ActionResult Delete(long id)
        {
            var model = _db.ExaminationCourseConditions.SingleOrDefault(x => x.Id == id);
            if (model == null)
            {
                _flashMessage.Danger(Message.UnableToDelete);
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _db.ExaminationCourseConditions.Remove(model);
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

        private void CreateSelectList() 
        {
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.Conditions = _selectListProvider.GetExaminationCourseConditions();
        }
    }
}