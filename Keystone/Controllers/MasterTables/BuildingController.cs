using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTable
{
    [PermissionAuthorize("Building", "")]
    public class BuildingController : BaseController
    {
        public BuildingController(ApplicationDbContext db,
                                  IFlashMessage flashMessage,
                                  ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList();
            var models = _db.Buildings.Include(x => x.Campus)
                                      .IgnoreQueryFilters()
                                      .Where(x => (criteria.CampusId == 0
                                                   || x.CampusId == criteria.CampusId)
                                                  && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                      || x.NameEn.Contains(criteria.CodeAndName)
                                                      || x.NameTh.Contains(criteria.CodeAndName))
                                                  && (string.IsNullOrEmpty(criteria.Status)
                                                      || x.IsActive == Convert.ToBoolean(criteria.Status)))
                                      .GetPaged(criteria, page);
            return View(models);
        }

        [PermissionAuthorize("Building", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View(new Building());
        }

        [PermissionAuthorize("Building", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Building model)
        {
            if (ModelState.IsValid)
            {
                _db.Buildings.Add(model);
                try
                {
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    CreateSelectList();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            CreateSelectList();
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            CreateSelectList();
            Building model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("Building", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<Building>(model))
                {
                    try
                    {
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index));
                    }
                    catch 
                    { 
                        CreateSelectList();
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
            }

            CreateSelectList();
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("Building", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Building model = Find(id);
            try
            {
                _db.Buildings.Remove(model);
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
            var rooms = _db.Rooms.Include(x => x.Building)
                                 .Where(x => x.BuildingId == id)
                                 .IgnoreQueryFilters()
                                 .GetPaged(page);
            return View(rooms);
        }

        private Building Find(long? id) 
        {
            var model = _db.Buildings.IgnoreQueryFilters()
                                     .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList() 
        {
            ViewBag.Campuses = _selectListProvider.GetCampuses();
            ViewBag.Statuses = _selectListProvider.GetActiveStatuses();
        }
    }
}