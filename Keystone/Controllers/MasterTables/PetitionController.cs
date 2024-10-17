using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("Petition", "")]
    public class PetitionController : BaseController
    {
        protected readonly IMasterProvider _masterProvider;

        public PetitionController(ApplicationDbContext db,
                                  IFlashMessage flashMessage,
                                  ISelectListProvider selectListProvider,
                                  IMasterProvider masterProvider) : base(db, flashMessage, selectListProvider)
        {
            _masterProvider = masterProvider;
        }

        public ActionResult Index(int page)
        {
            var models = _db.Petitions.IgnoreQueryFilters()
                                      .GetPaged(page);
            return View(models);
        }

        [PermissionAuthorize("Petition", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new Petition());
        }

        [PermissionAuthorize("Petition", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Petition model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.Petitions.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            Petition model = _masterProvider.FindPetition(id);
            return View(model);
        }

        [PermissionAuthorize("Petition", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            Petition model = _masterProvider.FindPetition(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<Petition>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(model);
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("Petition", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Petition model = _masterProvider.FindPetition(id);
            try
            {
                _db.Petitions.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}