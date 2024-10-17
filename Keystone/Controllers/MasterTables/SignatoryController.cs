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
    [PermissionAuthorize("Signatory", "")]
    public class SignatoryController : BaseController
    {
        protected readonly IFileProvider _fileProvider;

        public SignatoryController(ApplicationDbContext db,
                                   IFlashMessage flashMessage,
                                   IFileProvider fileProvider) : base(db, flashMessage) 
        {
            _fileProvider = fileProvider; 
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            var models = _db.Signatories.Where(x => (string.IsNullOrEmpty(criteria.CodeAndName)
                                                     || x.FullNameEn.StartsWith(criteria.CodeAndName)
                                                     || x.FullNameTh.StartsWith(criteria.CodeAndName))
                                                     && (string.IsNullOrEmpty(criteria.Position)
                                                         || x.PositionEn.StartsWith(criteria.Position)
                                                         || x.PositionTh.StartsWith(criteria.Position)))
                                        .IgnoreQueryFilters()
                                        .OrderBy(x => x.FirstNameEn)
                                        .GetPaged(criteria, page);
            return View(models);
        }

        [PermissionAuthorize("Signatory", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new Signatory());
        }

        [PermissionAuthorize("Signatory", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Signatory model)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try 
                    {
                        _db.Signatories.Add(model);
                        _db.SaveChanges();

                        if (model.UploadFile != null && model.UploadFile.Length > 0)
                        {
                            model.SignImageURL = _fileProvider.UploadFile(UploadSubDirectory.SIGNATORY, model.UploadFile, model.FirstNameEn + "_" + model.LastNameEn);
                            if (string.IsNullOrEmpty(model.SignImageURL))
                            {
                                _flashMessage.Danger(Message.UnableToCreate);
                                transaction.Rollback();
                                return View(model);
                            }

                            _db.SaveChanges();
                        }

                        _flashMessage.Confirmation(Message.SaveSucceed);
                        transaction.Commit();
                        return RedirectToAction(nameof(Index));
                    }
                    catch
                    {
                        _flashMessage.Danger(Message.UnableToCreate);
                        transaction.Rollback();
                        return View(model);
                    }
                }
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            Signatory model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("Signatory", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<Signatory>(model))
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        await _db.SaveChangesAsync();
                        if (model.UploadFile != null && model.UploadFile.Length > 0)
                        {
                            model.SignImageURL = _fileProvider.UploadFile(UploadSubDirectory.SIGNATORY, model.UploadFile, model.FirstNameEn + "_" + model.LastNameEn);
                            if (string.IsNullOrEmpty(model.SignImageURL))
                            {
                                _flashMessage.Danger(Message.UnableToCreate);
                                transaction.Rollback();
                                return View(model);
                            }

                            _db.SaveChanges();
                        }

                        _flashMessage.Confirmation(Message.SaveSucceed);
                        transaction.Commit();
                        return RedirectToAction(nameof(Index));
                    }
                    catch 
                    { 
                        _flashMessage.Danger(Message.UnableToEdit);
                        transaction.Rollback();
                        return View(model);
                    }
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("Signatory", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Signatory model = Find(id);
            try
            {
                _db.Signatories.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private Signatory Find(long? id) 
        {
            var model = _db.Signatories.IgnoreQueryFilters()
                                       .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}