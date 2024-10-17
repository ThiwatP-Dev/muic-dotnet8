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
    [PermissionAuthorize("LatePaymentConfiguration", "")]
    public class LatePaymentConfigurationController : BaseController
    {
        public LatePaymentConfigurationController(ApplicationDbContext db,
                                                 IFlashMessage flashMessage,
                                                 ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider){ }

        public ActionResult Index(Criteria criteria, string returnUrl, int page)
        {
            CreateSelectList(criteria.AcademicLevelId);

            if(criteria.AcademicLevelId == 0)
            {
                CreateSelectList();
                _flashMessage.Warning(Message.RequiredData);

                return View();
            }
            
            var result = _db.LatePaymentConfigurations.Include(x => x.FromTerm)
                                                          .ThenInclude(x => x.AcademicLevel)
                                                      .Include(x => x.ToTerm)
                                                      .Where(x => x.FromTerm.AcademicLevelId == criteria.AcademicLevelId)
                                                      .IgnoreQueryFilters()
                                                      .GetPaged(page);

            return View(result);
        }

        [PermissionAuthorize("LatePaymentConfiguration", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();

            return View(new LatePaymentConfigurationViewModel { IsActive = true });
        }

        [PermissionAuthorize("LatePaymentConfiguration", PolicyGenerator.Write)]
        [HttpPost]
        public ActionResult Create(LatePaymentConfigurationViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if(model.FromTermId == 0 || model.AmountPerDay <= 0 || model.MaximumDays <= 0)
            {
                CreateSelectList(model.AcademicLevelId);
                _flashMessage.Warning(Message.RequiredData);

                return View(model);
            }

            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var result = new LatePaymentConfiguration
                                 {
                                     ToTermId = model.ToTermId,
                                     FromTermId = model.FromTermId,
                                     AmountPerDay = model.AmountPerDay,
                                     MaximumDays = model.MaximumDays,
                                     IsActive = model.IsActive
                                 };

                    _db.LatePaymentConfigurations.Add(result);
                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    CreateSelectList(model.AcademicLevelId);
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToSave);

                    return View(model);
                }
            }
        }

        public ActionResult Edit(long id, string returnUrl)
        {   
            ViewBag.ReturnUrl = returnUrl;

            var latePaymentConfiguration = _db.LatePaymentConfigurations.IgnoreQueryFilters()
                                                                        .Include(x => x.FromTerm)
                                                                        .SingleOrDefault(x => x.Id == id);

            CreateSelectList(latePaymentConfiguration.FromTerm.AcademicLevelId);

            var result = new LatePaymentConfigurationViewModel
                         {
                             Id = latePaymentConfiguration.Id,
                             AcademicLevelId = latePaymentConfiguration.FromTerm.AcademicLevelId,
                             FromTermId = latePaymentConfiguration.FromTermId,
                             ToTermId = latePaymentConfiguration.ToTermId,
                             IsActive = latePaymentConfiguration.IsActive,
                             AmountPerDay = latePaymentConfiguration.AmountPerDay,
                             MaximumDays = latePaymentConfiguration.MaximumDays
                         };

            return View(result);
        }

        [PermissionAuthorize("LatePaymentConfiguration", PolicyGenerator.Write)]
        [HttpPost]
        public ActionResult Edit(LatePaymentConfigurationViewModel model, string returnUrl)
        {   
            ViewBag.ReturnUrl = returnUrl;

            if(model.FromTermId == 0 || model.AmountPerDay <= 0 || model.MaximumDays <= 0)
            {
                CreateSelectList(model.AcademicLevelId);
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            var latePaymentConfiguration = _db.LatePaymentConfigurations.IgnoreQueryFilters()
                                                                        .Include(x => x.FromTerm)
                                                                        .SingleOrDefault(x => x.Id == model.Id);

            CreateSelectList(latePaymentConfiguration.FromTerm.AcademicLevelId);

            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    latePaymentConfiguration.FromTermId = model.FromTermId;
                    latePaymentConfiguration.ToTermId = model.ToTermId;
                    latePaymentConfiguration.AmountPerDay = model.AmountPerDay;
                    latePaymentConfiguration.MaximumDays = model.MaximumDays;
                    latePaymentConfiguration.IsActive = model.IsActive;

                    _db.SaveChanges();
                    transaction.Commit();

                    _flashMessage.Confirmation(Message.SaveSucceed);

                    return View(model);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToEdit);

                    return View(model);
                }
            }
        }

        [PermissionAuthorize("LatePaymentConfiguration", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var latePaymentConfiguration = _db.LatePaymentConfigurations.IgnoreQueryFilters()
                                                                        .SingleOrDefault(x => x.Id == id);

            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.LatePaymentConfigurations.Remove(latePaymentConfiguration);
                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToDelete);

                    return RedirectToAction(nameof(Index));
                }
            }
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.Students = _selectListProvider.GetStudents();
            if(academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}