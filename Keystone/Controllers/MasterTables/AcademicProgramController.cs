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
    [PermissionAuthorize("AcademicProgram", "")]
    public class AcademicProgramController : BaseController
    {
        public AcademicProgramController(ApplicationDbContext db,
                                         ISelectListProvider selectListProvider,
                                         IFlashMessage flashMessage) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(int page)
        {
            var models = _db.AcademicPrograms.Include(x => x.AcademicLevel)
                                             .IgnoreQueryFilters()
                                             .GetPaged(page);
            return View(models);
        }

        [PermissionAuthorize("AcademicProgram", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View(new AcademicProgram());
        }

        [PermissionAuthorize("AcademicProgram", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AcademicProgram model)
        {
            if (ModelState.IsValid)
            {
                _db.AcademicPrograms.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction("Index");
            }

            CreateSelectList();
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            AcademicProgram model = Find(id);
            CreateSelectList();	
            return View(model);
        }

        [PermissionAuthorize("AcademicProgram", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<AcademicProgram>(model))
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
                    }
                }
            }

            CreateSelectList();
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("AcademicProgram", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            AcademicProgram model = Find(id);
            try
            {
                _db.AcademicPrograms.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private AcademicProgram Find(long? id) 
        {
            var model = _db.AcademicPrograms.IgnoreQueryFilters()
                                            .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList()
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
        }
    }
}