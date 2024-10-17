using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    public class CreditLoadController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly ICacheProvider _cacheProvider;

        public CreditLoadController(ApplicationDbContext db,
                                    IFlashMessage flashMessage, 
                                    ISelectListProvider selectListProvider,
                                    IStudentProvider studentProvider,
                                    ICacheProvider cacheProvider) : base(db, flashMessage, selectListProvider)
        {
            _studentProvider = studentProvider;
            _cacheProvider = cacheProvider;
        }

        public IActionResult Index()
        {
            var creditLoads = _db.CreditLoads.Include(x => x.AcademicLevel) 
                                             .Include(x => x.TermType)   
                                             .Include(x => x.Faculty)
                                             .Include(x => x.Department)
                                             .Include(x => x.TermType)
                                             .IgnoreQueryFilters()
                                             .ToList();

            return View(creditLoads);
        }

        public ActionResult Create()
        {
            CreateSelectList();
            return View(new CreditLoad());
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreditLoad model)
        {
            if (ModelState.IsValid)
            {
                _db.CreditLoads.Add(model);
                try
                {
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    CreateSelectList(model.AcademicLevelId, model.FacultyId ?? 0);
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            CreateSelectList(model.AcademicLevelId, model.FacultyId ?? 0);
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            CreditLoad model = Find(id);
            CreateSelectList(model.AcademicLevelId, model.FacultyId ?? 0);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<CreditLoad>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch
                { 
                    CreateSelectList(model.AcademicLevelId, model.FacultyId ?? 0);
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(model);
                }
            }

            CreateSelectList(model.AcademicLevelId, model.FacultyId ?? 0);
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        public ActionResult Delete(long id)
        {
            var model = Find(id);
            try
            {
                _db.CreditLoads.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public ActionResult UpdateCreditLoad(CreditLoadInformationViewModel model)
        {
            try
            {
                var academicInformation = _studentProvider.GetAcademicInformationByStudentId(model.StudentId);
                
                model.PreviousMinimumCredit = academicInformation.MinimumCredit ?? 0;
                model.PreviousMaximumCredit = academicInformation.MaximumCredit ?? 0;

                var currentTerm = _cacheProvider.GetCurrentTerm(academicInformation.AcademicLevelId);
                var studentLog = new StudentLog
                                 {
                                     StudentId = model.StudentId,
                                     TermId = currentTerm.Id,
                                     Source = "Update Credit",
                                     Log = JsonConvert.SerializeObject(model)
                                 };

                academicInformation.MinimumCredit = model.MinimumCredit;
                academicInformation.MaximumCredit = model.MaximumCredit;

                _db.StudentLogs.Add(studentLog);

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToSave);
            }

            return RedirectToAction("Index", "Registration", new
                                                             {
                                                                 Code = model.Code,
                                                                 AcademicLevelId = model.AcademicLevelId,
                                                                 TermId = model.TermId,
                                                                 TabIndex = model.TabIndex,
                                                                 ReturnUrl = model.ReturnUrl
                                                             });
        }

        private CreditLoad Find(long? id) 
        {
            return _db.CreditLoads.Include(x => x.AcademicLevel)
                                  .IgnoreQueryFilters()
                                  .SingleOrDefault(x => x.Id == id);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.TermTypes = _selectListProvider.GetTermTypes();
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
        }
    }
}