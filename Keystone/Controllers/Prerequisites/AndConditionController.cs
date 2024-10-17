using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Prerequisites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("AndCondition", "")]
    public class AndConditionController : BaseController
    {
        protected readonly IPrerequisiteProvider _prerequisiteProvider;

        public AndConditionController(ApplicationDbContext db,
                                      IFlashMessage flashMessage,
                                      ISelectListProvider selectListProvider,
                                      IPrerequisiteProvider prerequisiteProvider) : base(db, flashMessage, selectListProvider)
        {
            _prerequisiteProvider = prerequisiteProvider;
        }

        public ActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.Type);
            if (string.IsNullOrEmpty(criteria.Type))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            var andCondition = _db.AndConditions.Where(x => (string.IsNullOrEmpty(criteria.Type)
                                                             || x.FirstConditionType == criteria.Type
                                                             || x.SecondConditionType == criteria.Type)
                                                             && (criteria.ConditionId == 0
                                                                 || x.FirstConditionId == criteria.ConditionId
                                                                 || x.SecondConditionId == criteria.ConditionId)
                                                             && (string.IsNullOrEmpty(criteria.Status)
                                                                 || x.IsActive == Convert.ToBoolean(criteria.Status)))
                                                .IgnoreQueryFilters()
                                                .ToList();

            andCondition = _prerequisiteProvider.GetAndConditionNames(andCondition);
            var andConditionPagedResult = andCondition.AsQueryable()
                                                      .GetPaged(criteria, page);
            
            return View(andConditionPagedResult);
        }

        [PermissionAuthorize("AndCondition", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            return View(new AndCondition());
        }

        [PermissionAuthorize("AndCondition", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AndCondition model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (_prerequisiteProvider.IsExistAndCondition(model))
                {
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    CreateSelectList(model.FirstConditionType, model.SecondConditionType);
                    return View();
                }

                try
                {
                    model.Description = _prerequisiteProvider.GetUpdateAndConditionDescription(model, true);
                    _db.AndConditions.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return Redirect(returnUrl);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToCreate);
                    CreateSelectList(model.FirstConditionType, model.SecondConditionType);
                    return View(model);
                }
            }

            _flashMessage.Danger(Message.UnableToCreate);
            CreateSelectList(model.FirstConditionType, model.SecondConditionType);
            return View(model);
        }

        [PermissionAuthorize("AndCondition", PolicyGenerator.Write)]
        public ActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = Find(id);
            CreateSelectList(model.FirstConditionType, model.SecondConditionType);
            return View(model);
        }

        [PermissionAuthorize("AndCondition", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<AndCondition>(model))
            {
                if (_prerequisiteProvider.IsExistAndCondition(model))
                {
                    _flashMessage.Danger(Message.DataAlreadyExist);
                    CreateSelectList(model.FirstConditionType, model.SecondConditionType);
                    return View(model);
                }
                
                try
                {
                    model.Description = _prerequisiteProvider.GetUpdateAndConditionDescription(model, true);
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return Redirect(returnUrl);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                    CreateSelectList(model.FirstConditionType, model.SecondConditionType);
                    return View(model);
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            CreateSelectList(model.FirstConditionType, model.SecondConditionType);
            return View(model);
        }

        [PermissionAuthorize("AndCondition", PolicyGenerator.Write)]
        public ActionResult Delete(long id, string returnUrl)
        {
            var model = Find(id);
            CreateSelectList(model.FirstConditionType, model.SecondConditionType);
            var check = _prerequisiteProvider.IsConditionAlreadyExits(model.Id, "and");
            if(check.IsAlreadyExist)
            {
                _flashMessage.Danger(check.Message);
                return Redirect(returnUrl);
            }
            try
            {
                _db.AndConditions.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return Redirect(returnUrl);
        }

        private AndCondition Find(long? id)
        {
            var model = _db.AndConditions.IgnoreQueryFilters()
                                         .SingleOrDefault(x => x.Id == id);

            return model;
        }

        private void CreateSelectList(string firstConditionType = null, string secondConditionType = null)
        {
            ViewBag.ConditionTypes = _selectListProvider.GetConditionTypes();
            ViewBag.Statuses = _selectListProvider.GetActiveStatuses();
            if (!string.IsNullOrEmpty(firstConditionType))
            {
                ViewBag.FirstConditions = _selectListProvider.GetConditionsByType(firstConditionType);
            }

            if (!string.IsNullOrEmpty(secondConditionType))
            {
                ViewBag.SecondConditions = _selectListProvider.GetConditionsByType(secondConditionType);
            }
        }
    }
}