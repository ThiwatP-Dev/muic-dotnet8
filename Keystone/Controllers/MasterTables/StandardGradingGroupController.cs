using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class StandardGradingGroupController : BaseController
    {
        protected readonly IMasterProvider _masterProvider;

        public StandardGradingGroupController(ApplicationDbContext db,
                                              IFlashMessage flashMessage,
                                              ISelectListProvider selectListProvider,
                                              IMasterProvider masterProvider) : base(db, flashMessage, selectListProvider)
        {
            _masterProvider = masterProvider;
        }

        public IActionResult Index(int page)
        {
            var model = _db.StandardGradingGroups.Include(x => x.GradeTemplate)
                                                 .IgnoreQueryFilters()
                                                 .GetPaged(page);
            return View(model);
        }

        public ActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new StandardGradingGroup());
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StandardGradingGroup model, string returnUrl)
        {
            if (ModelState.IsValid)
            { 
                try
                {
                    _db.StandardGradingGroups.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    CreateSelectList();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            var model = _masterProvider.GetStandardGradingGroup(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = _masterProvider.GetStandardGradingGroup(id ?? 0);

            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<StandardGradingGroup>(model))
                {
                    try
                    {
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index));
                    }
                    catch 
                    {
                        ViewBag.ReturnUrl = returnUrl;
                        CreateSelectList();
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        public ActionResult GetGradeTemplate(long id)
        {
            CreateSelectList();
            var model = _db.GradeTemplates.SingleOrDefault(x => x.Id == id);
            var gradeIds = _db.Grades.Where(x => model.GradeIdsLong.Contains(x.Id))
                                     .OrderBy(x => x.Weight)
                                     .Select(x => x.Id)
                                     .ToList();
     
            model.GradeIds = JsonConvert.SerializeObject(gradeIds) ?? "[]";
            return PartialView("~/Views/StandardGradingGroup/_GradeTemplateForm.cshtml", model);
        }

        public ActionResult Delete(long id)
        {
            var model = _masterProvider.GetStandardGradingGroup(id);
            try
            {
                _db.StandardGradingGroups.Remove(model);
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
            ViewBag.StandardGradingScoreTypes = _selectListProvider.GetStandardGradingScoreType();
            ViewBag.Grades = _selectListProvider.GetGrades();
            ViewBag.GradeTemplates = _selectListProvider.GetGradeTemplates();
        }
    }
}