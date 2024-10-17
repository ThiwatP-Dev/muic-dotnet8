using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.DataModels.Configurations;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("RegistrationConfiguration", "")]
    public class RegistrationConfigurationController : BaseController
    {
        protected readonly IRegistrationProvider _registrationProvider;
        public RegistrationConfigurationController(ApplicationDbContext db,
                                                   IFlashMessage flashMessage,
                                                   ISelectListProvider selectListProvider,
                                                   IRegistrationProvider registrationProvider) : base(db, flashMessage, selectListProvider)
        {
            _registrationProvider = registrationProvider;
        }

        public IActionResult Index(int page, Criteria criteria, string returnUrl)
        {
            CreateSelectList(criteria.AcademicLevelId);
            ViewBag.ReturnUrl = returnUrl;
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var models = _db.RegistrationConfigurations.Include(x => x.FromTerm)
                                                          .ThenInclude(x => x.AcademicLevel)
                                                       .Include(x => x.ToTerm)
                                                       .Where(x => criteria.AcademicLevelId == 0
                                                                   || x.FromTerm.AcademicLevelId == criteria.AcademicLevelId)
                                                       .IgnoreQueryFilters()
                                                       .OrderByDescending(x => x.FromTerm.AcademicYear)
                                                           .ThenBy(x => x.FromTerm.AcademicTerm)
                                                       .GetPaged(criteria, page);
            return View(models);
        }

        [PermissionAuthorize("RegistrationConfiguration", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;

            return View(new RegistrationConfiguration());
        }

        [PermissionAuthorize("RegistrationConfiguration", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RegistrationConfiguration model, string returnUrl)
        {   
            ViewBag.ReturnUrl = returnUrl;
            model.FromTerm = _db.Terms.SingleOrDefault(x => x.Id == model.FromTermId);
            model.ToTerm = _db.Terms.SingleOrDefault(x => x.Id == model.ToTermId);
            var checkDuplicate = _db.RegistrationConfigurations.Where(x =>  x.FromTerm.AcademicLevelId == model.AcademicLevelId
                                                                            && (x.FromTerm.AcademicYear < model.FromTerm.AcademicYear
                                                                            || (x.FromTerm.AcademicYear == model.FromTerm.AcademicYear 
                                                                                && x.FromTerm.AcademicTerm <= model.FromTerm.AcademicTerm ))
                                                                            && (x.ToTermId == null
                                                                                || (x.ToTerm.AcademicYear > model.FromTerm.AcademicYear
                                                                                || (x.ToTerm.AcademicYear == model.FromTerm.AcademicYear 
                                                                                    && x.ToTerm.AcademicTerm >= model.FromTerm.AcademicTerm ))))
                                                               .Any();

            if(checkDuplicate)
            {
                CreateSelectList(model.AcademicLevelId);
                _flashMessage.Warning(Message.UnableToSaveDataDuplicate);
                return View(model);
            }
            if(model.ToTermId != null && model.ToTermId != 0)
            {
                var fromTerm = _db.Terms.SingleOrDefault(x => x.Id == model.FromTermId);
                var toTerm = _db.Terms.SingleOrDefault(x => x.Id == model.ToTermId);
                if(fromTerm.AcademicYear > toTerm.AcademicYear || ( fromTerm.AcademicYear == toTerm.AcademicYear && fromTerm.AcademicTerm > toTerm.AcademicTerm))
                {
                    CreateSelectList(model.AcademicLevelId);
                    _flashMessage.Warning(Message.InvalidTerm);
                    return View(model);
                }
            }

            try
            {
                _db.RegistrationConfigurations.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index), new Criteria { AcademicLevelId = model.AcademicLevelId });
            }
            catch
            {
                CreateSelectList(model.AcademicLevelId);
                _flashMessage.Danger(Message.UnableToCreate);
                return View(model);
            }
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = _db.RegistrationConfigurations.Include(x => x.FromTerm)
                                                      .SingleOrDefault(x => x.Id == id);

            model.AcademicLevelId = model.FromTerm.AcademicLevelId;
            CreateSelectList(model.AcademicLevelId);
            return View(model);
        }

        [PermissionAuthorize("RegistrationConfiguration", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(RegistrationConfiguration model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if(model.ToTermId != null && model.ToTermId != 0)
            {
                var fromTerm = _db.Terms.SingleOrDefault(x => x.Id == model.FromTermId);
                var toTerm = _db.Terms.SingleOrDefault(x => x.Id == model.ToTermId);
                if(fromTerm.AcademicYear > toTerm.AcademicYear || ( fromTerm.AcademicYear == toTerm.AcademicYear && fromTerm.AcademicTerm > toTerm.AcademicTerm))
                {
                    CreateSelectList(model.AcademicLevelId);
                    _flashMessage.Warning(Message.InvalidTerm);
                    return View(model);
                }
            }
            try
            {
                var registrationConfiguration = _db.RegistrationConfigurations.Find(model.Id);
                registrationConfiguration.FromTermId = model.FromTermId;
                registrationConfiguration.ToTermId = model.ToTermId;
                registrationConfiguration.MininumCreditToWithdraw = model.MininumCreditToWithdraw;
                registrationConfiguration.RegistrationTime = model.RegistrationTime;
                registrationConfiguration.IsRegistrationAllowed = model.IsRegistrationAllowed;
                registrationConfiguration.IsPaymentAllowed = model.IsPaymentAllowed;
                await _db.SaveChangesAsync();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index), new Criteria { AcademicLevelId = model.AcademicLevelId });
            }
            catch
            {
                CreateSelectList(model.AcademicLevelId);
                _flashMessage.Danger(Message.UnableToEdit);
                return View(model);
            }
        }

        [PermissionAuthorize("RegistrationConfiguration", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = _db.RegistrationConfigurations.Include(x => x.FromTerm)
                                                      .SingleOrDefault(x => x.Id == id);
            try
            {
                _db.RegistrationConfigurations.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index), new Criteria { AcademicLevelId = model.FromTerm.AcademicLevelId });
        }

        private void CreateSelectList(long academicLevelId = 0) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}