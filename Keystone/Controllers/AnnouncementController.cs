using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    public class AnnouncementController : BaseController
    {
        public AnnouncementController(ApplicationDbContext db,
                                      IFlashMessage flashMessage,
                                      ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(int page = 1)
        {
            var announcements = _db.Announcements.Include(x => x.AnnouncementTopics)
                                                     .ThenInclude(x => x.Topic)
                                                 .GetPaged(page);
            return View(announcements);
        }

        public ActionResult Create()
        {
            CreateSelectList();
            return View(new Announcement());
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Announcement model)
        {
            if (model.StartedAt.Date > model.ExpiredAt.Date)
            {
                CreateSelectList();
                _flashMessage.Danger(Message.InvalidStartedDate);
                return View(model);
            }

            try
            {
                if(model.Topics == null)
                {
                    _flashMessage.Danger(Message.EmptyTopic);
                    return View(model);
                }

                _db.Announcements.Add(model);
                _db.SaveChanges();

                foreach(var item in model.Topics)
                {
                    _db.AnnouncementTopics.Add(new AnnouncementTopic
                                               {
                                                   AnnouncementId = model.Id,
                                                   TopicId = item
                                               });
                }

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                CreateSelectList();
                _flashMessage.Danger(Message.UnableToCreate);
            }
            
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            Announcement model = Find(id);
            model.Topics = _db.AnnouncementTopics.Where(x => x.AnnouncementId == model.Id)
                                                 .Select(x => x.TopicId)
                                                 .ToList();

            CreateSelectList(model.Topics);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Announcement model)
        {
            if (ModelState.IsValid)
            {
                if (model.StartedAt.Date > model.ExpiredAt.Date)
                {
                    CreateSelectList();
                    _flashMessage.Danger(Message.InvalidStartedDate);
                    return View(model);
                }

                if(model.Topics == null)
                {
                    CreateSelectList();
                    _flashMessage.Danger(Message.EmptyTopic);
                    return View(model);
                }

                _db.Entry(model).State = EntityState.Modified;
                DeleteTopics(model.Id);

                foreach(var item in model.Topics)
                {
                    _db.AnnouncementTopics.Add(new AnnouncementTopic
                                               {
                                                   AnnouncementId = model.Id,
                                                   TopicId = item
                                               });
                }

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }

            CreateSelectList();
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        public ActionResult Delete(long id)
        {
            Announcement model = Find(id);
            DeleteTopics(id);
            if (model == null)
            {
                _flashMessage.Danger(Message.UnableToDelete);
                return RedirectToAction(nameof(Index));
            }

            _db.Announcements.Remove(model);
            _db.SaveChanges();
            _flashMessage.Confirmation(Message.SaveSucceed);
            return RedirectToAction(nameof(Index));
        }

        private Announcement Find(long id) 
        {
            var announcement = _db.Announcements.Include(x => x.AnnouncementTopics)
                                                    .ThenInclude(x => x.Topic)
                                                .SingleOrDefault(x => x.Id == id);
            return announcement;
        }

        private void DeleteTopics(long announcementId)
        {
            var topics = _db.AnnouncementTopics.Where(x => x.AnnouncementId == announcementId);
            _db.RemoveRange(topics);
        }

        private void CreateSelectList() 
        {
            ViewBag.Channels = _selectListProvider.GetChannels();
            ViewBag.Topics = _selectListProvider.GetTopics();
        }

        private void CreateSelectList(List<long> selectedTopic) 
        {
            ViewBag.Channels = _selectListProvider.GetChannels();
            ViewBag.Topics = _selectListProvider.GetTopics(selectedTopic);
        }
    }
}