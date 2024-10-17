using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    public class InstructorTypeController : BaseController
    {
        protected readonly IMasterProvider _masterProvider;

        public InstructorTypeController(ApplicationDbContext db,
                                        IFlashMessage flashMessage,
                                        IMasterProvider masterProvider) : base(db, flashMessage) 
        { 
            _masterProvider = masterProvider;
        }

        public IActionResult Index()
        {
            var models = _db.InstructorTypes.IgnoreQueryFilters()
                                            .ToList();
            return View(models);
        }

        public ActionResult Create()
        {
            return View(new InstructorType());
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(InstructorType model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.InstructorTypes.Add(model);
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
            InstructorType model = _masterProvider.FindInstructorType(id);	
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            InstructorType model = _masterProvider.FindInstructorType(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<InstructorType>(model))
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
            InstructorType model = _masterProvider.FindInstructorType(id);
            try
            {
                _db.InstructorTypes.Remove(model);
                _flashMessage.Confirmation(Message.SaveSucceed);
                _db.SaveChanges();
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}