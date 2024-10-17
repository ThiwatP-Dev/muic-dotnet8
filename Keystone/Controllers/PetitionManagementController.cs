using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    public class PetitionManagementController : BaseController
    {
        public PetitionManagementController(ApplicationDbContext db,
                                            IFlashMessage flashMessage,
                                            ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public ActionResult Index(Criteria criteria, int page, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var petition = _db.StudentPetitions.Include(x => x.Term)
                                                   .ThenInclude(x => x.AcademicLevel)
                                               .Include(x => x.Petition)
                                               .Include(x => x.Student)
                                                   .ThenInclude(x => x.AcademicInformation)
                                                   .ThenInclude(x => x.Faculty)
                                               .Include(x => x.Student)
                                                   .ThenInclude(x => x.AcademicInformation)
                                                   .ThenInclude(x => x.Department)
                                               .Where(x => (x.Term.AcademicLevelId == criteria.AcademicLevelId)
                                                            && (criteria.PetitionId == 0
                                                                || x.PetitionId == criteria.PetitionId)
                                                            && (criteria.TermId == 0
                                                                || x.TermId == criteria.TermId)
                                                            && (string.IsNullOrEmpty(criteria.Code)
                                                                || x.Student.Code.StartsWith(criteria.Code))
                                                            && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                                || x.Student.FirstNameEn.StartsWith(criteria.CodeAndName)
                                                                || x.Student.FirstNameTh.StartsWith(criteria.CodeAndName)
                                                                || x.Student.MidNameEn.StartsWith(criteria.CodeAndName)
                                                                || x.Student.MidNameTh.StartsWith(criteria.CodeAndName)
                                                                || x.Student.LastNameEn.StartsWith(criteria.CodeAndName)
                                                                || x.Student.LastNameTh.StartsWith(criteria.CodeAndName)))
                                               .IgnoreQueryFilters()
                                               .GetPaged(criteria ,page);
            return View(petition);
        }

        public ActionResult AddLogs(long id)
        {
            var model = new PetitionLog
                        {
                            StudentPetitionId = id
                        };

            return PartialView("_AddLogs", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddLogs(PetitionLog model)
        {
            var petitionId = _db.StudentPetitions.SingleOrDefault(x => x.Id == model.StudentPetitionId)
                                                 .PetitionId;
            if (ModelState.IsValid)
            {
                try
                {
                    _db.PetitionLogs.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { PetitionId = petitionId });
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToCreate);
                    CreateSelectList();
                    return RedirectToAction(nameof(Index), new { PetitionId = petitionId });
                }
            }

            _flashMessage.Danger(Message.UnableToCreate);
            CreateSelectList();
            return RedirectToAction(nameof(Index), new { PetitionId = petitionId });
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = Find(id);
            CreateSelectList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<StudentPetition>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { PetitionId = model.PetitionId });
                }         
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToEdit);
                    CreateSelectList();
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            CreateSelectList();
            return View(model);
        }
        
        public ActionResult Details(long id)
        {
            var petition = Find(id);
            return PartialView("_Details", petition);
        }

        private StudentPetition Find(long? id)
        {
            var model = _db.StudentPetitions.Include(x => x.Student)
                                                .ThenInclude(x => x.AcademicInformation)
                                                .ThenInclude(x => x.AcademicLevel)
                                            .Include(x => x.Student)
                                                .ThenInclude(x => x.Title)
                                            .Include(x => x.Student)
                                                .ThenInclude(x => x.AcademicInformation)
                                                .ThenInclude(x => x.Faculty)  
                                            .Include(x => x.Student)
                                                .ThenInclude(x => x.AcademicInformation)
                                                .ThenInclude(x => x.Department)
                                            .Include(x => x.Student)
                                                .ThenInclude(x => x.StudentFeeType)
                                            .Include(x => x.Student)
                                                .ThenInclude(x => x.ResidentType) 
                                            .Include(x => x.Petition)
                                            .Include(x => x.PetitionLogs)
                                            .IgnoreQueryFilters()
                                            .SingleOrDefault(x => x.Id == id);

            return model;
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Petitions = _selectListProvider.GetPetitions();
            ViewBag.Statuses = _selectListProvider.GetPetitionStatuses();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}