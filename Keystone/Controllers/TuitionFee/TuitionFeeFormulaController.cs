using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Fee;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("TuitionFeeFormula", "")]
    public class TuitionFeeFormulaController : BaseController
    {
        private readonly IFeeProvider _iFeeProvider;
        
        public TuitionFeeFormulaController(ApplicationDbContext db,
                                           IFlashMessage flashMessage,
                                           ISelectListProvider selectListProvider,
                                           IFeeProvider iFeeProvider) : base(db, flashMessage, selectListProvider)
        {
            _iFeeProvider = iFeeProvider;
        }

        public IActionResult Index(int page)
        {
            var models = _db.TuitionFeeFormulas.Include(x => x.FirstTuitionFeeType)
                                               .Include(x => x.SecondTuitionFeeType)
                                               .IgnoreQueryFilters()
                                               .GetPaged(page);
            return View(models);
        }

        [PermissionAuthorize("TuitionFeeFormula", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View(new TuitionFeeFormula());
        }

        [PermissionAuthorize("TuitionFeeFormula", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TuitionFeeFormula model)
        {
            CreateSelectList();
            if (ModelState.IsValid)
            {
                try
                {
                    _db.TuitionFeeFormulas.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
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
            CreateSelectList();
            var model = _iFeeProvider.GetTuitionFeeFormula(id);
            return View(model);
        }

        [PermissionAuthorize("TuitionFeeFormula", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            CreateSelectList();
            var model = _iFeeProvider.GetTuitionFeeFormula(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<TuitionFeeFormula>(model))
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

        [PermissionAuthorize("TuitionFeeFormula", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = _iFeeProvider.GetTuitionFeeFormula(id);
            try
            {
                _db.TuitionFeeFormulas.Remove(model);
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
            ViewBag.TuitionFeeTypes = _selectListProvider.GetTuitionFeeTypes();
        }
    }
}