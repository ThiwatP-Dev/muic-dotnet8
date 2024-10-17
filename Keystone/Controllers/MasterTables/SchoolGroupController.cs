using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTable
{
    public class SchoolGroupController : BaseController
    {
        public SchoolGroupController(ApplicationDbContext db,
                                     IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            var models = _db.SchoolGroups.Where(x => (string.IsNullOrEmpty(criteria.CodeAndName)
                                                      || x.NameEn.StartsWith(criteria.CodeAndName)
                                                      || x.NameTh.StartsWith(criteria.CodeAndName)))
                                         .IgnoreQueryFilters()
                                         .GetPaged(criteria, page);
            return View(models);
        }

        public ActionResult Create()
        {
            return View(new SchoolGroup());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SchoolGroup model)
        {
            if (ModelState.IsValid)
            {
                _db.SchoolGroups.Add(model);
                try
                {
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new Criteria
                                                           {
                                                               CodeAndName = model.NameEn
                                                           });
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
            SchoolGroup model = Find(id);	
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<SchoolGroup>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new Criteria
                                                           {
                                                               CodeAndName = model.NameEn
                                                           });
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
            SchoolGroup model = Find(id);
            try
            {
                _db.SchoolGroups.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Details(long id, string returnUrl, int page)
        {
            ViewData["Title"] = Find(id).NameEn;
            ViewBag.ReturnUrl = returnUrl;
            var schools = _db.PreviousSchools.Include(x => x.SchoolGroup)
                                             .Include(x => x.SchoolTerritory)
                                             .Include(x => x.SchoolType)
                                             .Include(x => x.Country)
                                             .Include(x => x.Province)
                                             .Include(x => x.State)
                                             .Where(x => x.SchoolGroupId == id)
                                             .IgnoreQueryFilters()
                                             .GetPaged(page);
            return View(schools);
        }

        private SchoolGroup Find(long? id) 
        {
            var model = _db.SchoolGroups.IgnoreQueryFilters()
                                        .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}