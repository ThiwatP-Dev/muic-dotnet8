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
    [PermissionAuthorize("Probation", "")]
    public class ProbationController : BaseController
    {
        protected readonly IMasterProvider _masterProvider;

        public ProbationController(ApplicationDbContext db,
                                   IFlashMessage flashMessage,
                                   IMasterProvider masterProvider) : base(db, flashMessage)
        {
            _masterProvider = masterProvider;
        }

        public ActionResult Index(Criteria criteria, int page)
        {
            var probations = _db.Probations.IgnoreQueryFilters()
                                           .GetPaged(criteria ,page);
            return View(probations);
        }

        [PermissionAuthorize("Probation", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new Probation());
        }

        [PermissionAuthorize("Probation", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Probation model)
        {
            if (ModelState.IsValid)
            {
                _db.Probations.Add(model);
                try
                {
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
            var model = _masterProvider.FindProbation(id);
            return View(model);
        }

        [PermissionAuthorize("Probation", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = _masterProvider.FindProbation(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<Probation>(model))
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

        [PermissionAuthorize("Probation", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = _masterProvider.FindProbation(id);

            try
            {
                _db.Probations.Remove(model);
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