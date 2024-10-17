using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("ReservationCalendar", "")]
    public class ReservationCalendarController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;

        public ReservationCalendarController(ApplicationDbContext db,
                                             IFlashMessage flashMessage,
                                             IExceptionManager exceptionManager) : base(db, flashMessage) 
        { 
            _exceptionManager = exceptionManager;
        }

        public IActionResult Index()
        {
            var models = _db.ReservationCalendars.IgnoreQueryFilters()
                                                 .OrderBy(x => x.StartedAt)
                                                    .ThenBy(x => x.Name)
                                                 .ToList();
            return View(models);
        }

        public IActionResult Details(long id)
        {
            var reservationCalendar = _db.ReservationCalendars.SingleOrDefault(x => x.Id == id);

            return View(reservationCalendar);
        }

        [PermissionAuthorize("ReservationCalendar", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View(new ReservationCalendar { IsActive = true });
        }

        [PermissionAuthorize("ReservationCalendar", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ReservationCalendar model)
        {
            if(string.IsNullOrEmpty(model.Name) || model.StartedAt == null || model.EndedAt == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            if (model.StartedAt?.Date > model.EndedAt?.Date)
            {
                _flashMessage.Danger(Message.InvalidStartedDate);
                return View(model);
            }

            if(_db.ReservationCalendars.Any(x => x.StartedAt < model.EndedAt 
                                                 && x.EndedAt > model.StartedAt
                                                 && x.IsActive))
            {
                _flashMessage.Danger(Message.OverlapPeriod);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                _db.ReservationCalendars.Add(model);
                try
                {
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
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

                    return View(model);
                }
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            var model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("ReservationCalendar", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ReservationCalendar model)
        {
            if(string.IsNullOrEmpty(model.Name) || model.StartedAt == null || model.EndedAt == null)
            {
                _flashMessage.Danger(Message.RequiredData);
                return View(model);
            }

            if (model.StartedAt?.Date > model.EndedAt?.Date)
            {
                _flashMessage.Danger(Message.InvalidStartedDate);
                return View(model);
            }

            if(_db.ReservationCalendars.Any(x => x.StartedAt < model.EndedAt 
                                                 && x.EndedAt > model.StartedAt
                                                 && x.Id != model.Id
                                                 && x.IsActive))
            {
                _flashMessage.Danger(Message.OverlapPeriod);
                return View(model);
            }

            var result = Find(model.Id);
            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    result.Name = model.Name;
                    result.StartedAt = model.StartedAt;
                    result.EndedAt = model.EndedAt;
                    await _db.SaveChangesAsync();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception e)
                { 
                    if (_exceptionManager.IsDuplicatedEntityCode(e))
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.CodeUniqueConstraintError);
                    }
                    else
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.UnableToEdit);
                    }
                    return View(model);
                }
            }
        }

        [PermissionAuthorize("ReservationCalendar", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = Find(id);
            try
            {
                _db.ReservationCalendars.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private ReservationCalendar Find(long? id) 
        {
            var model = _db.ReservationCalendars.IgnoreQueryFilters()
                                                .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}