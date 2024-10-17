using AutoMapper;
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
    [PermissionAuthorize("AcademicCalendar", "")]
    public class AcademicCalendarController : BaseController
    {
        public AcademicCalendarController(ApplicationDbContext db,
                                          IFlashMessage flashMessage,
                                          ISelectListProvider selectListProvider,
                                          IMapper mapper) : base(db, flashMessage, mapper, selectListProvider) { }

        private const string Grey = "#c2c2c2";
        private const string Orange = "#F36648";
        private const string Blue = "#1ab394";
        private const string Green = "#1c84c6";

        public IActionResult Index()
        {
            var academicCalendars = _db.AcademicCalendars.Include(x => x.Event)
                                                             .ThenInclude(x => x.EventCategory)
                                                         .Include(x => x.AcademicLevel)
                                                         .Select(x => _mapper.Map<AcademicCalendar, AcademicCalendarViewModel>(x))
                                                         .ToList();

            foreach (var item in academicCalendars)
            {
                if (item.AcademicLevel.ToLower().Contains("bachelor"))
                {
                    item.Color = Orange;
                }
                else if (item.AcademicLevel.ToLower().Contains("master"))
                {
                    item.Color = Blue;
                }
                else if (item.AcademicLevel.ToLower().Contains("doctor"))
                {
                    item.Color = Green;
                }
                else 
                {
                    item.Color = Grey;
                }
            }
            
            return View(academicCalendars);
        }

        [PermissionAuthorize("AcademicCalendar", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            CreateSelectList();
            return View(new AcademicCalendar());
        }

        [PermissionAuthorize("AcademicCalendar", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AcademicCalendar model)
        {
            if (ModelState.IsValid)
            { 
                if (model.StartedAt.Date > model.EndedAt.Date)
                {
                    CreateSelectList(model.EventCategoryId ?? 0);
                    _flashMessage.Danger(Message.InvalidStartedDate);
                    return View(model);
                }

                _db.AcademicCalendars.Add(model);
                try
                {
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    CreateSelectList(model.EventCategoryId ?? 0);
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            CreateSelectList(model.EventCategoryId ?? 0);
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            AcademicCalendar model = Find(id);
            CreateSelectList(model.EventCategoryId ?? 0);
            return View(model);
        }

        [PermissionAuthorize("AcademicCalendar", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);
            if (ModelState.IsValid && await TryUpdateModelAsync<AcademicCalendar>(model))
            {
                if (model.StartedAt.Date > model.EndedAt.Date)
                {
                    CreateSelectList(model.EventCategoryId ?? 0);
                    _flashMessage.Danger(Message.InvalidStartedDate);
                    return View(model);
                }
                
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch 
                { 
                    CreateSelectList(model.EventCategoryId ?? 0);
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(model);
                }  
            }

            CreateSelectList(model.EventCategoryId ?? 0);
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("AcademicCalendar", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            AcademicCalendar model = Find(id);
            try
            {
                _db.AcademicCalendars.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private AcademicCalendar Find(long? id) 
        {
            var academicCalendar = _db.AcademicCalendars.Include(x => x.Event)
                                                        .Include(x => x.AcademicLevel)
                                                        .SingleOrDefault(x => x.Id == id);
            academicCalendar.EventCategoryId = academicCalendar.Event.EventCategoryId;
            return academicCalendar;
        }

        private void CreateSelectList(long eventCategorieId = 0) 
        {
            ViewBag.EventCategories = _selectListProvider.GetEventCategories();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.ViewLevels = _selectListProvider.GetCalendarViewLevel();

            if (eventCategorieId != 0)
            {
                ViewBag.Events = _selectListProvider.GetEvents();
            }
        }
    }
}