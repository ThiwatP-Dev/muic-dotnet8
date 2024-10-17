using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Prerequisites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class BatchConditionController : BaseController
    {
        protected readonly IPrerequisiteProvider _prerequisiteProvider;

        public BatchConditionController(ApplicationDbContext db,
                                        IFlashMessage flashMessage,
                                        ISelectListProvider selectListProvider,
                                        IPrerequisiteProvider prerequisiteProvider) : base(db, flashMessage, selectListProvider)
        {
            _prerequisiteProvider = prerequisiteProvider;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            var models = _db.BatchConditions.IgnoreQueryFilters()
                                            .Where(x => criteria.StartStudentBatch == null
                                                        || x.Batch == criteria.StartStudentBatch)
                                            .OrderBy(x => x.Batch)
                                            .GetPaged(criteria, page, true);
            return View(models);
        }

        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new BatchCondition());
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BatchCondition model, string returnUrl)
        {
            if (_prerequisiteProvider.IsExistBatchCondition(model))
            {
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    model.Description = $"Batch {model.Batch}";
                    _db.BatchConditions.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { Batch = model.Batch });
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            BatchCondition model = _prerequisiteProvider.GetBatchConditionById(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = _prerequisiteProvider.GetBatchConditionById(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<BatchCondition>(model))
            {
                if (_prerequisiteProvider.IsExistBatchCondition(model))
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    return View(model);
                }
                else
                {
                    try
                    {
                        model.Description = $"Batch {model.Batch}";
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index), new { Batch = model.Batch });
                    }
                    catch
                    {
                        ViewBag.ReturnUrl = returnUrl;
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        public ActionResult Delete(long id, string returnUrl)
        {
            BatchCondition model = _prerequisiteProvider.GetBatchConditionById(id);
            var check = _prerequisiteProvider.IsConditionAlreadyExits(model.Id, "batch");
            if(check.IsAlreadyExist)
            {
                _flashMessage.Danger(check.Message);
                return Redirect(returnUrl);
            }
            try
            {
                _db.BatchConditions.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return Redirect(returnUrl);
        }
    }
}