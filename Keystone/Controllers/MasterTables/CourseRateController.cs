using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTable
{
    public class CourseRateController : BaseController
    {
        public CourseRateController(ApplicationDbContext db,
                                    IFlashMessage flashMessage) : base(db, flashMessage) { }
        
        public IActionResult Index()
        {
            var models = _db.CourseRates.IgnoreQueryFilters()
                                        .ToList();
            return View(models);
        }

        public ActionResult Create()
        {
            return View(new CourseRate());
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CourseRate model)
        {
            if (ModelState.IsValid)
            {
                _db.CourseRates.Add(model);
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
            CourseRate model = Find(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<CourseRate>(model))
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
            CourseRate model = Find(id);
            try
            {
                _db.CourseRates.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete); 
            }
            
            return RedirectToAction(nameof(Index));
        }

        private CourseRate Find(long? id) 
        {
            var model = _db.CourseRates.IgnoreQueryFilters()
                                       .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}