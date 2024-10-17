using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("RegistrationTerm", "")]
    public class RegistrationTermController : BaseController
    {
        public RegistrationTermController(ApplicationDbContext db,
                                          IFlashMessage flashMessage,
                                          ISelectListProvider selectListProvider,
                                          IMapper mapper) : base(db, flashMessage, mapper, selectListProvider) { }
        
        public ActionResult Index(long academicLevelId, long termId, string returnUrl)
        {
            CreateSelectList(academicLevelId);
            ViewBag.ReturnUrl = returnUrl;

            if (academicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var result = new RegistrationTermViewModel
                         {
                             AcademicLevelId = academicLevelId,
                             TermId = termId
                         };

            result.RegistrationTermDetails = _db.RegistrationTerms.Include(x => x.Term)
                                                                  .Where(x => x.Term.AcademicLevelId == academicLevelId
                                                                              && (x.TermId == termId || termId == 0))
                                                                  .Select(x => new RegistrationTermDetailViewModel
                                                                               {
                                                                                   Id = x.Id,
                                                                                   Name = x.Name,
                                                                                   AcademicLevelNameEn = x.Term.AcademicLevel.NameEn,
                                                                                   TermId = x.Term.Id,
                                                                                   TermText = x.Term.TermText,
                                                                                   StartedAt = x.StartedAt,
                                                                                   EndedAt = x.EndedAt,
                                                                                   ExpiredAt = x.ExpiredAt,
                                                                                   Type = x.TypeText,
                                                                                   IsActive = x.IsActive
                                                                               })
                                                                  .OrderBy(x => x.StartedAt)
                                                                    .ThenBy(x => x.EndedAt)
                                                                  .ToList();
            return View(result);
        }

        [PermissionAuthorize("RegistrationTerm", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();

            return View(new RegistrationTerm());
        }

        [PermissionAuthorize("RegistrationTerm", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RegistrationTerm model)
        {
            if(model.TermId == 0 || string.IsNullOrEmpty(model.NameNullAble) || string.IsNullOrEmpty(model.Type))
            {
                CreateSelectList(model.AcademicLevelId);
                _flashMessage.Warning(Message.RequiredData);

                return View(model);
            }

            if (model.StartedAt?.Date > model.EndedAt?.Date)
            {
                CreateSelectList(model.AcademicLevelId);
                _flashMessage.Danger(Message.InvalidStartedDate);

                return View(model);
            }

            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    model.Name = model.NameNullAble;
                    _db.RegistrationTerms.Add(model);
                    _db.SaveChanges();
                    transaction.Commit();

                    var registrationTerm = _db.RegistrationTerms.Where(x => x.Id == model.Id)
                                                               .Select(x => new
                                                                            {
                                                                                x.Term.AcademicLevelId, 
                                                                                x.TermId
                                                                            })
                                                               .First();

                    _flashMessage.Confirmation(Message.SaveSucceed);

                    return RedirectToAction(nameof(Index), new 
                                                           {
                                                               AcademicLevelId = registrationTerm.AcademicLevelId,
                                                               TermId = registrationTerm.TermId
                                                           });
                }
                catch
                {
                    transaction.Rollback();
                    CreateSelectList(model.AcademicLevelId);
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View (model);
                }
            }
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            var registrationTerm = _db.RegistrationTerms.Where(x => x.Id == id)
                                                        .Select(x => new RegistrationTerm
                                                                     {
                                                                             Id = x.Id,
                                                                             Name = x.Name,
                                                                             AcademicLevelId = x.Term.AcademicLevelId,
                                                                             TermId = x.Term.Id,
                                                                             StartedAt = x.StartedAt,
                                                                             EndedAt = x.EndedAt,
                                                                             ExpiredAt = x.ExpiredAt,
                                                                             Type = x.Type,
                                                                             IsActive = x.IsActive,
                                                                             NameNullAble = x.Name
                                                                     })
                                                        .First();

            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(registrationTerm.AcademicLevelId);
            return View(registrationTerm);
        }

        [PermissionAuthorize("RegistrationTerm", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RegistrationTerm model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            
            if(model.TermId == 0 || string.IsNullOrEmpty(model.NameNullAble) || string.IsNullOrEmpty(model.Type))
            {
                CreateSelectList(model.AcademicLevelId);
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            if (model.StartedAt?.Date > model.EndedAt?.Date)
            {
                CreateSelectList(model.AcademicLevelId);
                _flashMessage.Danger(Message.InvalidStartedDate);
                return View(model);
            }

            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var registrationTerm = _db.RegistrationTerms.SingleOrDefault(x => x.Id == model.Id);
                    registrationTerm.IsActive = model.IsActive;
                    registrationTerm.TermId = model.TermId;
                    registrationTerm.Name = model.Name;
                    registrationTerm.StartedAt = model.StartedAt;
                    registrationTerm.EndedAt = model.EndedAt;
                    registrationTerm.ExpiredAt = model.ExpiredAt;
                    registrationTerm.Type = model.Type;
                    registrationTerm.Name = model.NameNullAble;
                    _db.SaveChanges();
                    transaction.Commit();
                    
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    CreateSelectList(model.AcademicLevelId);

                    return RedirectToAction(nameof(Index), new 
                                                           { 
                                                               AcademicLevelId = model.AcademicLevelId,
                                                               TermId = model.TermId
                                                           });
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

        [PermissionAuthorize("RegistrationTerm", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = _db.RegistrationTerms.Include(x => x.Term)
                                             .SingleOrDefault(x => x.Id == id);

            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.RegistrationTerms.Remove(model);
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
                                                       AcademicLevelId = model.Term.AcademicLevelId,
                                                       TermId = model.TermId
                                                   });
        }

        private void CreateSelectList(long academicLevelId = 0) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Types = _selectListProvider.GetSlotType();
        }
    }
}