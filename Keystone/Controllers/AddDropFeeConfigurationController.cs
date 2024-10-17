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
    [PermissionAuthorize("AddDropFeeConfiguration", "")]
    public class AddDropFeeConfigurationController : BaseController
    {
        public AddDropFeeConfigurationController(ApplicationDbContext db,
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
            
            var result = _db.AddDropFeeConfigurations.Include(x => x.FromTerm)
                                                         .ThenInclude(x => x.AcademicLevel)
                                                     .Include(x => x.ToTerm)
                                                     .Where(x => x.FromTerm.AcademicLevelId == criteria.AcademicLevelId)
                                                     .IgnoreQueryFilters()
                                                     .GetPaged(page);

            return View(result);
        }

        [PermissionAuthorize("AddDropFeeConfiguration", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();

            return View(new AddDropFeeConfigurationViewModel { IsActive = true });
        }

        [PermissionAuthorize("AddDropFeeConfiguration", PolicyGenerator.Write)]
        [HttpPost]
        public ActionResult Create(AddDropFeeConfigurationViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if(model.FromTermId == 0 || model.Amount <= 0 || model.FreeAddDropCount <= 0)
            {
                CreateSelectList(model.AcademicLevelId);
                _flashMessage.Warning(Message.RequiredData);

                return View(model);
            }

            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var result = new AddDropFeeConfiguration
                                 {
                                     ToTermId = model.ToTermId,
                                     FromTermId = model.FromTermId,
                                     Amount = model.Amount,
                                     FreeAddDropCount = model.FreeAddDropCount,
                                     IsActive = model.IsActive
                                 };

                    _db.AddDropFeeConfigurations.Add(result);
                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);

                    return RedirectToAction(nameof(Index), new { AcademicLevelId =  model.AcademicLevelId});
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

            var addDropFeeconfiguration = _db.AddDropFeeConfigurations.IgnoreQueryFilters()
                                                                      .Include(x => x.FromTerm)
                                                                      .SingleOrDefault(x => x.Id == id);

            CreateSelectList(addDropFeeconfiguration.FromTerm.AcademicLevelId);

            var result = new AddDropFeeConfigurationViewModel
                         {
                             Id = addDropFeeconfiguration.Id,
                             AcademicLevelId = addDropFeeconfiguration.FromTerm.AcademicLevelId,
                             FromTermId = addDropFeeconfiguration.FromTermId,
                             ToTermId = addDropFeeconfiguration.ToTermId,
                             IsActive = addDropFeeconfiguration.IsActive,
                             Amount = addDropFeeconfiguration.Amount,
                             FreeAddDropCount = addDropFeeconfiguration.FreeAddDropCount
                         };

            return View(result);
        }

        [PermissionAuthorize("AddDropFeeConfiguration", PolicyGenerator.Write)]
        [HttpPost]
        public ActionResult Edit(AddDropFeeConfigurationViewModel model, string returnUrl)
        {   
            ViewBag.ReturnUrl = returnUrl;

            if(model.FromTermId == 0 || model.Amount <= 0 || model.FreeAddDropCount <= 0)
            {
                CreateSelectList(model.AcademicLevelId);
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            var addDropFeeConfiguration = _db.AddDropFeeConfigurations.IgnoreQueryFilters()
                                                                      .Include(x => x.FromTerm)
                                                                      .SingleOrDefault(x => x.Id == model.Id);

            CreateSelectList(addDropFeeConfiguration.FromTerm.AcademicLevelId);

            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    addDropFeeConfiguration.FromTermId = model.FromTermId;
                    addDropFeeConfiguration.ToTermId = model.ToTermId;
                    addDropFeeConfiguration.Amount = model.Amount;
                    addDropFeeConfiguration.FreeAddDropCount = model.FreeAddDropCount;
                    addDropFeeConfiguration.IsActive = model.IsActive;

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

        [PermissionAuthorize("AddDropFeeConfiguration", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var addDropFeeconfiguration = _db.AddDropFeeConfigurations.IgnoreQueryFilters()
                                                                      .SingleOrDefault(x => x.Id == id);

            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.AddDropFeeConfigurations.Remove(addDropFeeconfiguration);
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