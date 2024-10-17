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
    [PermissionAuthorize("Topic", "")]
    public class TopicController : BaseController
    {
        public TopicController(ApplicationDbContext db,
                               IFlashMessage flashMessage,
                               ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index()
        {
            var models = _db.Topics.Include(x => x.Faculty)
                                   .Include(x => x.Department)
                                   .Include(x => x.Channel)
                                   .IgnoreQueryFilters()
                                   .ToList();
            return View(models);
        }

        [PermissionAuthorize("Topic", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View(new Topic());
        }

        [PermissionAuthorize("Topic", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Topic model)
        {
            if (ModelState.IsValid)
            {
                _db.Topics.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }

            CreateSelectList(model.FacultyId ?? 0, model.AcademicLevelId);
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            CreateSelectList();
            Topic model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("Topic", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<Topic>(model))
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
                    }
                }
            }

            CreateSelectList(model.FacultyId ?? 0);
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("Topic", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Topic model = Find(id);
            try
            {
                _db.Topics.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        private Topic Find(long? id) 
        {
            var model = _db.Topics.IgnoreQueryFilters()
                                  .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList(long facultyId = 0, long academiclevelId = 0)
        {
            ViewBag.Channels = _selectListProvider.GetChannels();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Faculties = academiclevelId == 0 ? _selectListProvider.GetFaculties() : _selectListProvider.GetFacultiesByAcademicLevelId(academiclevelId);
            ViewBag.Departments = academiclevelId == 0 && facultyId == 0
                                  ? _selectListProvider.GetDepartments(facultyId)
                                  : _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academiclevelId, facultyId);
        }
    }
}