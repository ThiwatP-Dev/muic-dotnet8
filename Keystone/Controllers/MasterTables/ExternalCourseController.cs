using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.DataModels;

namespace Keystone.Controllers.MasterTables
{
    public class ExternalCourseController : BaseController
    {
        protected readonly IMasterProvider _masterProvider;

        public ExternalCourseController(ApplicationDbContext db,
                                        IFlashMessage flashMessage,
                                        ISelectListProvider selectListProvider,
                                        IMasterProvider masterProvider) : base(db, flashMessage, selectListProvider)
        {
            _masterProvider = masterProvider;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList();
            var models = _db.Courses.Include(x => x.AcademicLevel)
                                    .Include(x => x.TransferUniversity)
                                    .Where(x => x.TransferUniversityId != null
                                                && (criteria.TransferUniversityId == 0
                                                 || x.TransferUniversityId == criteria.TransferUniversityId)
                                                && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                    || x.CodeAndName.Contains(criteria.CodeAndName))
                                                && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                    || x.CodeAndNameTh.Contains(criteria.CodeAndName)))
                                    .IgnoreQueryFilters()
                                    .GetPaged(criteria, page);   
            return View(models);
        }

        public ActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new Course());
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Course model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.Courses.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new
                                                           {
                                                               TransferUniversityId = model.TransferUniversityId,
                                                               CodeAndName = model.Code
                                                           });
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

        public ActionResult Edit(long id, string returnUrl)
        {
            var model = _masterProvider.GetExternalCourse(id);
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = _masterProvider.GetExternalCourse(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<Course>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new
                                                           {
                                                               TransferUniversityId = model.TransferUniversityId,
                                                               CodeAndName = model.Code
                                                           });
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
            var model = _masterProvider.GetExternalCourse(id);
            try
            {
                _db.Courses.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete); 
            }
            
            return RedirectToAction(nameof(Index));
        }

        private void CreateSelectList()
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.TransferUniversities = _selectListProvider.GetTransferUniversities();
        }
    }
}