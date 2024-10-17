using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTable
{
    public class ExemptedAdmissionExaminationController : BaseController
    {
        public ExemptedAdmissionExaminationController(ApplicationDbContext db,
                                                      IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index(int page = 1)
        {
            var models = _db.ExemptedAdmissionExaminations.OrderBy(x => x.NameEn)
                                                          .IgnoreQueryFilters()
                                                          .GetPaged(page);
            return View(models);
        }

        public ActionResult Create()
        {
            return View(new ExemptedAdmissionExamination());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ExemptedAdmissionExamination model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.ExemptedAdmissionExaminations.Add(model);
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
            ExemptedAdmissionExamination model = Find(id);	
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<ExemptedAdmissionExamination>(model))
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

        public ActionResult Delete(long id)
        {
            ExemptedAdmissionExamination model = Find(id);
            try
            {
                _db.ExemptedAdmissionExaminations.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private ExemptedAdmissionExamination Find(long? id)
        {
            var model = _db.ExemptedAdmissionExaminations.IgnoreQueryFilters()
                                                         .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}