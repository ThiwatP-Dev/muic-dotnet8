using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{

    [PermissionAuthorize("Slot", "")]
    public class SlotController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;
        protected readonly IAcademicProvider _academicProvider;
        
        public SlotController(ApplicationDbContext db,
                              IAcademicProvider academicProvider,
                              IExceptionManager exceptionManager,
                              IFlashMessage flashMessage,
                              ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) 
        {
            _academicProvider = academicProvider;
            _exceptionManager = exceptionManager;
        }

        public IActionResult Index(int page, Criteria criteria)
        {          
            if(criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                CreateSelectList();
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            CreateSelectList(criteria.AcademicLevelId);
            var slots = _db.Slots.Include(x => x.RegistrationTerm)
                                 .Where(x => x.RegistrationTerm.TermId == criteria.TermId
                                             && (criteria.RegistrationTermId <= 0 
                                                 || x.RegistrationTerm.Id == criteria.RegistrationTermId))
                                 .OrderBy(x => x.StartedAt)
                                 .GetPaged(criteria, page, true);

            for(var i = 0 ; i < slots.Results.Count ; i++)
            {
                slots.Results[i].RegistrationSlotConditions = _db.RegistrationSlotConditions.Include(x => x.RegistrationCondition)
                                                                                               .ThenInclude(x => x.Department)
                                                                                            .Include(x => x.RegistrationCondition)
                                                                                                .ThenInclude(x => x.Faculty)
                                                                                            .Include(x => x.RegistrationCondition)
                                                                                                .ThenInclude(x => x.AcademicLevel)
                                                                                            .Include(x => x.RegistrationCondition)
                                                                                                .ThenInclude(x => x.AcademicProgram)
                                                                                            .Where(x => x.SlotId == slots.Results[i].Id)    
                                                                                            .ToList();
            }

            return View(slots);
        }

        [PermissionAuthorize("Slot", PolicyGenerator.Write)]
        public ActionResult Create(Criteria criteria, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            return View(new SlotViewModel());
        }

        [PermissionAuthorize("Slot", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SlotViewModel model, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            if(string.IsNullOrEmpty(model.Name))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            if(model.StartedAt > model.EndedAt)
            {
                _flashMessage.Warning(Message.InvalidStartedDate);
                return View(model);
            }
            
            if (ModelState.IsValid)
            { 
                using(var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        _db.Slots.Add(model);
                        _db.SaveChanges();

                        if(model.RegistrationSlotConditions != null)
                        {
                            foreach (var item in model.RegistrationSlotConditions)
                            {
                                if(!_db.RegistrationSlotConditions.Any(x => x.SlotId == model.Id && x.RegistrationConditionId == item.RegistrationConditionId))
                                {
                                    var registrationSlot = new RegistrationSlotCondition
                                                           {
                                                               SlotId = model.Id,
                                                               RegistrationConditionId = item.RegistrationConditionId
                                                           };

                                    _db.RegistrationSlotConditions.Add(registrationSlot);
                                    _db.SaveChanges();
                                }
                            }
                        }

                        _flashMessage.Confirmation(Message.SaveSucceed);
                        transaction.Commit();

                        var criteria = _db.RegistrationTerms.Where(x => x.Id ==  model.RegistrationTermId)
                                                            .Select(x => new Criteria
                                                                         {
                                                                             AcademicLevelId = x.Term.AcademicLevelId,
                                                                             TermId = x.TermId,
                                                                             RegistrationTermId = model.RegistrationTermId
                                                                         })
                                                            .First();

                        return RedirectToAction(nameof(Index), criteria);
                    }
                    catch (Exception e)
                    {
                        if (_exceptionManager.IsDuplicatedEntityCode(e))
                        {
                            _flashMessage.Danger(Message.CodeUniqueConstraintError);
                        }
                        else
                        {
                            _flashMessage.Danger(Message.UnableToCreate);
                        }
                        transaction.Rollback();
                        CreateSelectList();
                        return View(model);
                    }
                }
            }

            CreateSelectList();
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var slot = Find(id);
            CreateSelectList();
            return View(slot);
        }

        [PermissionAuthorize("Slot", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, SlotViewModel result, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if(string.IsNullOrEmpty(result.Name))
            {
                _flashMessage.Warning(Message.RequiredData);
                CreateSelectList();
                return View(result);
            }

            if(result.StartedAt > result.EndedAt)
            {
                _flashMessage.Warning(Message.InvalidStartedDate);
                CreateSelectList();
                return View(result);
            }

            var model = _db.Slots.Include(x => x.RegistrationSlotConditions).SingleOrDefault(x => x.Id == id);

            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<Slot>(model))
                {
                    try
                    {
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        var criteria = _db.RegistrationTerms.Where(x => x.Id ==  model.RegistrationTermId)
                                                            .Select(x => new Criteria
                                                                         {
                                                                             AcademicLevelId = x.Term.AcademicLevelId,
                                                                             TermId = x.TermId,
                                                                             RegistrationTermId = model.RegistrationTermId
                                                                         })
                                                            .First();
                        return RedirectToAction(nameof(Index), criteria);
                    }
                    catch (Exception e)
                    { 
                        if (_exceptionManager.IsDuplicatedEntityCode(e))
                        {
                            _flashMessage.Danger(Message.CodeUniqueConstraintError);
                        }
                        else
                        {
                            _flashMessage.Danger(Message.UnableToEdit);
                        }

                        CreateSelectList();
                        return View(model);
                    }
                }
            }

            CreateSelectList();
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("Slot", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Slot model = _db.Slots.Include(x => x.RegistrationSlotConditions).SingleOrDefault(x => x.Id == id);;
            try
            {
                _db.Slots.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ExportExcel(List<long> ids, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var slots = _db.Slots.AsNoTracking()
                                 .Include(x => x.RegistrationTerm)
                                 .Where(x => ids.Contains(x.Id))
                                 .OrderBy(x => x.StartedAt)
                                 .ToList();

            for(var i = 0 ; i < slots.Count ; i++)
            {
                slots[i].RegistrationSlotConditions = _db.RegistrationSlotConditions.AsNoTracking()
                                                                                    .Include(x => x.RegistrationCondition)
                                                                                       .ThenInclude(x => x.Department)
                                                                                    .Include(x => x.RegistrationCondition)
                                                                                        .ThenInclude(x => x.Faculty)
                                                                                    .Include(x => x.RegistrationCondition)
                                                                                        .ThenInclude(x => x.AcademicLevel)
                                                                                    .Include(x => x.RegistrationCondition)
                                                                                        .ThenInclude(x => x.AcademicProgram)
                                                                                    .Where(x => x.SlotId == slots[i].Id)    
                                                                                    .ToList();
            }
            return View(slots);
        }

        private SlotViewModel Find(long? id)
        {
            var slot = _db.Slots.Include(x => x.RegistrationSlotConditions)
                                .Where(x => x.Id == id)
                                .Select(x => new SlotViewModel
                                             {
                                                 Id = x.Id,
                                                 Name = x.Name,
                                                 Description = x.Description,
                                                 StartedAt = x.StartedAt,
                                                 EndedAt = x.EndedAt,
                                                 RegistrationTermId = x.RegistrationTermId,
                                                 RegistrationSlotConditions = x.RegistrationSlotConditions,
                                                 IsActive = x.IsActive,
                                                 IsSpecialSlot = x.IsSpecialSlot,
                                                 Type = x.Type
                                             })
                                .First();
            return slot;
        }

        private long GetAcademicLevelId(long id)
        {
            var level = _db.Terms.SingleOrDefault(x => x.Id == id).AcademicLevelId;
            return level;
        }

        private void CreateSelectList(long academicLevelId = 0, long termId = 0) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Types = _selectListProvider.GetSlotType();
            ViewBag.RegistrationTerms = _selectListProvider.GetRegistrationTerms();
            ViewBag.RegistrationConditions = _selectListProvider.GetRegistrationConditions();

            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                if(termId > 0)
                {
                    ViewBag.RegistrationTerms = _selectListProvider.GetRegistrationTermsByTerm(termId);
                }
            }
        }
    }
}