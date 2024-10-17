using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Withdrawals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("WithdrawalPeriod", "")]
    public class WithdrawalPeriodController : BaseController
    {
        protected readonly IWithdrawalProvider _withdrawProvider;
        public WithdrawalPeriodController(ApplicationDbContext db, 
                                          IFlashMessage flashMessage,
                                          IWithdrawalProvider withdrawProvider, 
                                          ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) 
        { 
            _withdrawProvider = withdrawProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var withdrawalPeriods = _db.WithdrawalPeriods.Include(x => x.Term)
                                                             .ThenInclude(x => x.AcademicLevel)
                                                         .Where(x => (x.AcademicLevelId == criteria.AcademicLevelId)
                                                                      && (criteria.TermId == 0
                                                                          || x.TermId == criteria.TermId))
                                                         .OrderBy(x => x.TermId)
                                                         .ThenBy(x => x.StartedAt)
                                                         .GetPaged(criteria, page);
            return View(withdrawalPeriods);
        }

        [PermissionAuthorize("WithdrawalPeriod", PolicyGenerator.Write)]
        public ActionResult Create(long academicLevelId, long termId)
        {
            CreateSelectList(academicLevelId);
            return View(new WithdrawalPeriod()
                        {
                            AcademicLevelId = academicLevelId,
                            TermId = termId,
                            StartedAt = DateTime.UtcNow.AddHours(7),
                            EndedAt = DateTime.UtcNow.AddHours(7)
                        });
        }

        [PermissionAuthorize("WithdrawalPeriod", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(WithdrawalPeriod model)
        {
            if (ModelState.IsValid)
            {
                model.StartedAt = model.StartedAt.AddHours(-7);
                model.EndedAt = model.EndedAt.AddHours(-7);
                if (_withdrawProvider.IsPeriodExisted(model))
                {
                    _flashMessage.Danger(Message.ExistedWithdrawalPeriod);
                    CreateSelectList(model.AcademicLevelId);
                    return View(model);
                }
                else
                {
                    _db.WithdrawalPeriods.Add(model);
                    try
                    {
                        _db.SaveChanges();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index), new 
                                                            { 
                                                                TermId = model.TermId,
                                                                AcademicLevelId = model.AcademicLevelId
                                                            });
                    }
                    catch
                    {
                        CreateSelectList(model.AcademicLevelId);
                        _flashMessage.Danger(Message.UnableToCreate);
                        return View(model);
                    }   
                }
            }

            CreateSelectList(model.AcademicLevelId);
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            var model = Find(id);
            model.StartedAt = model.StartedAt.AddHours(7);
            model.EndedAt = model.EndedAt.AddHours(7);
            CreateSelectList(model.AcademicLevelId);
            return View(model);
        }

        [PermissionAuthorize("WithdrawalPeriod", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<WithdrawalPeriod>(model))
            {
                model.StartedAt = model.StartedAt.AddHours(-7);
                model.EndedAt = model.EndedAt.AddHours(-7);
                if (_withdrawProvider.IsPeriodExisted(model))
                {
                    _flashMessage.Danger(Message.ExistedWithdrawalPeriod);
                    CreateSelectList(model.AcademicLevelId);
                    return View(model);
                }
                else
                {
                    try
                    {
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index), new 
                                                               { 
                                                                   TermId = model.TermId,
                                                                   AcademicLevelId = model.AcademicLevelId
                                                               });
                    }
                    catch 
                    { 
                        CreateSelectList(model.AcademicLevelId);
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
            }

            CreateSelectList(model.AcademicLevelId);
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("WithdrawalPeriod", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            WithdrawalPeriod model = Find(id);
            try
            {
                _db.WithdrawalPeriods.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private WithdrawalPeriod Find(long? id)
        {
            var model = _db.WithdrawalPeriods.Find(id);
            return model;
        }

        private void CreateSelectList(long academicLevelId = 0) 
        {
            ViewBag.WithdrawalPeriodTypes = _selectListProvider.GetWithdrawalPeriodTypes();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();

            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}