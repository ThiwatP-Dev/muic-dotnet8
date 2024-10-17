using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("LateRegistrationConfiguration", "")]
    public class LateRegistrationConfigurationController : BaseController
    {
        public LateRegistrationConfigurationController(ApplicationDbContext db,
                                     IFlashMessage flashMessage,
                                     ISelectListProvider selectListProvider,
                                     IMapper mapper) : base(db, flashMessage, mapper, selectListProvider) { }
        
        public ActionResult Index(long academicLevelId, string returnUrl)
        {
            CreateSelectList(academicLevelId);
            ViewBag.ReturnUrl = returnUrl;
            if (academicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            var result = new LateRegistrationConfigurationViewModel
                         {
                             AcademicLevelId = academicLevelId
                         };

            result.LateRegistrations = _db.LateRegistrationConfigurations.Where(x => x.FromTerm.AcademicLevelId == academicLevelId)
                                                                         .Select(x => new LateRegistrationViewModel
                                                                                      {
                                                                                          Id = x.Id,
                                                                                          AcademicLevelNameEn = x.FromTerm.AcademicLevel.NameEn,
                                                                                          FromTermId = x.FromTermId,
                                                                                          FromTermText = x.FromTerm.TermText,
                                                                                          ToTermId = x.ToTermId,
                                                                                          ToTermText = x.ToTerm.TermText,
                                                                                          Amount = x.Amount,
                                                                                          IsActive = x.IsActive
                                                                                      })
                                                                         .ToList();
            return View(result);
        }

        [PermissionAuthorize("LateRegistrationConfiguration", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            return View(new LateRegistrationConfiguration());
        }

        [PermissionAuthorize("LateRegistrationConfiguration", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LateRegistrationConfiguration model)
        {
            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.LateRegistrationConfigurations.Add(model);
                    _db.SaveChanges();
                    transaction.Commit();

                    var academicLevelId = _db.LateRegistrationConfigurations.Where(x => x.Id == model.Id)
                                                                            .Select(x => x.FromTerm.AcademicLevelId)
                                                                            .First();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new 
                                                           {
                                                               AcademicLevelId = academicLevelId
                                                           });
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View (model);
                }
            }
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            var lateRegistration = _db.LateRegistrationConfigurations.Where(x => x.Id == id)
                                                                     .Select(x => new LateRegistrationViewModel
                                                                                  {
                                                                                      Id = x.Id,
                                                                                      AcademicLevelId = x.FromTerm.AcademicLevelId,
                                                                                      AcademicLevelNameEn = x.FromTerm.AcademicLevel.NameEn,
                                                                                      FromTermId = x.FromTermId,
                                                                                      FromTermText = x.FromTerm.TermText,
                                                                                      ToTermId = x.ToTermId,
                                                                                      ToTermText = x.ToTerm.TermText,
                                                                                      Amount = x.Amount,
                                                                                      IsActive = x.IsActive
                                                                                  })
                                                                     .First();

            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(lateRegistration.AcademicLevelId);
            return View(lateRegistration);
        }

        public ActionResult Details(long id)
        {
            var lateRegistration = _db.LateRegistrationConfigurations.Where(x => x.Id == id)
                                                                     .Select(x => new LateRegistrationViewModel
                                                                                  {
                                                                                      Id = x.Id,
                                                                                      AcademicLevelId = x.FromTerm.AcademicLevelId,
                                                                                      AcademicLevelNameEn = x.FromTerm.AcademicLevel.NameEn,
                                                                                      FromTermId = x.FromTermId,
                                                                                      FromTermText = x.FromTerm.TermText,
                                                                                      ToTermId = x.ToTermId,
                                                                                      ToTermText = x.ToTerm.TermText,
                                                                                      Amount = x.Amount,
                                                                                      IsActive = x.IsActive
                                                                                  })
                                                                     .First();

            return PartialView("_Details", lateRegistration);
        }

        [PermissionAuthorize("LateRegistrationConfiguration", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LateRegistrationViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var lateRegistration = _db.LateRegistrationConfigurations.SingleOrDefault(x => x.Id == model.Id);
                    lateRegistration.IsActive = model.IsActive;
                    lateRegistration.FromTermId = model.FromTermId;
                    lateRegistration.ToTermId = model.ToTermId;
                    lateRegistration.Amount = model.Amount;
                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    CreateSelectList(model.AcademicLevelId);
                    return RedirectToAction(nameof(Index), new { AcademicLevelId = model.AcademicLevelId});
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToEdit);
                    CreateSelectList(model.AcademicLevelId);
                    return View(model);
                }
            }            
        }

        [PermissionAuthorize("LateRegistrationConfiguration", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = _db.LateRegistrationConfigurations.Include(x => x.FromTerm)
                                                          .SingleOrDefault(x => x.Id == id);

            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.LateRegistrationConfigurations.Remove(model);
                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToDelete);
                    transaction.Rollback();
                }
            }

            return RedirectToAction(nameof(Index), new 
                                                   {
                                                       AcademicLevelId = model.FromTerm.AcademicLevelId
                                                   });
        }

        private void CreateSelectList(long academicLevelId = 0) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
        }
    }
}