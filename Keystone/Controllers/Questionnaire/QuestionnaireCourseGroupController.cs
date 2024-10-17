using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    public class QuestionnaireCourseGroupController : BaseController
    {
        protected readonly IMasterProvider _masterProvider;
        protected readonly IRegistrationProvider _registrationProvider;

        public QuestionnaireCourseGroupController(ApplicationDbContext db,
                                                  IFlashMessage flashMessage,
                                                  ISelectListProvider selectListProvider,
                                                  IMasterProvider masterProvider,
                                                  IRegistrationProvider registrationProvider) : base(db, flashMessage, selectListProvider)
        {
            _masterProvider = masterProvider;
            _registrationProvider = registrationProvider;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList();
            var model = _db.QuestionnaireCourseGroups.AsNoTracking()
                                                     .Select(x => new QuestionnaireCourseGroupViewModel
                                                                  {
                                                                      QuestionnaireCourseGroupId = x.Id,
                                                                      Name = x.Name,
                                                                      Description = x.Description,
                                                                      Courses = x.QuestionnaireCourseGroupDetails.Select(y => new QuestionnaireCourseGroupCourse
                                                                                                                              {
                                                                                                                                  CourseRateId = y.Course.CourseRateId ?? 0,
                                                                                                                                  CourseCode = y.Course.Code,
                                                                                                                                  CodeAndCredit = y.Course.CodeAndCredit + "[" + y.Course.MUICId + "]" ,
                                                                                                                                  CourseName = y.Course.NameEn,
                                                                                                                                  Credit = y.Course.Credit,
                                                                                                                                  Lecture = y.Course.Lecture,
                                                                                                                                  Lab = y.Course.Lab,
                                                                                                                                  Other = y.Course.Other
                                                                                                                              })
                                                                                                                 .ToList()
                                                                  })
                                                     .IgnoreQueryFilters()
                                                     .GetPaged(criteria, page);

            return View(model);
        }

        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            return View(new QuestionnaireCourseGroupViewModel());
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(QuestionnaireCourseGroupViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            try
            {
                var questionnaireCourseGroup = new QuestionnaireCourseGroup
                                               {
                                                   Name = model.Name,
                                                   Description = model.Description
                                               };

                _db.QuestionnaireCourseGroups.Add(questionnaireCourseGroup);
                var questionnaireCourseGroupDetails = new List<QuestionnaireCourseGroupDetail>();
                foreach (var item in model.CourseIds)
                {
                    var questionnaireCourseGroupDetail = new QuestionnaireCourseGroupDetail
                                                         {
                                                             QuestionnaireCourseGroupId = questionnaireCourseGroup.Id,
                                                             CourseId = item
                                                         };

                    questionnaireCourseGroupDetails.Add(questionnaireCourseGroupDetail);
                }
                
                _db.QuestionnaireCourseGroupDetails.AddRange(questionnaireCourseGroupDetails);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }
            catch 
            {
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.UnableToCreate);
                CreateSelectList();
                return View(model);
            }
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            var model = _db.QuestionnaireCourseGroups.AsNoTracking()
                                                     .Where(x => x.Id == id)
                                                     .Select(x => new QuestionnaireCourseGroupViewModel
                                                                  {
                                                                      QuestionnaireCourseGroupId = x.Id,
                                                                      Name = x.Name,
                                                                      Description = x.Description,
                                                                      IsActive = x.IsActive
                                                                  })
                                                     .IgnoreQueryFilters()
                                                     .SingleOrDefault();

            model.CourseIds = _db.QuestionnaireCourseGroupDetails.AsNoTracking()
                                                                 .Where(x => x.QuestionnaireCourseGroupId == model.QuestionnaireCourseGroupId)
                                                                 .Select(x => x.CourseId)
                                                                 .ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(QuestionnaireCourseGroupViewModel model, string returnUrl)
        {
            var courseGroup = _masterProvider.GetQuestionnaireCourseGroup(model.QuestionnaireCourseGroupId);
            try
            {
                courseGroup.Name = model.Name;
                courseGroup.Description = model.Description;
                courseGroup.IsActive = model.IsActive;
                var courseGroupDetails = _db.QuestionnaireCourseGroupDetails.Where(x => x.QuestionnaireCourseGroupId == model.QuestionnaireCourseGroupId)
                                                                            .ToList();

                var courseGroupDetailCourseIds = courseGroupDetails.Select(x => x.CourseId).ToList();
                var removeDetails = courseGroupDetails.Where(x => model.CourseIds == null
                                                                  || !model.CourseIds.Any()
                                                                  || !model.CourseIds.Contains(x.CourseId))
                                                      .ToList();

                var removeCourseIds = removeDetails.Select(x => x.CourseId).ToList();
                var questionnaireCourseGroupDetails = new List<QuestionnaireCourseGroupDetail>();
                if (model.CourseIds != null)
                {
                    foreach (var item in model.CourseIds)
                    {
                        if (!courseGroupDetailCourseIds.Contains(item))
                        {
                            var addDetails = new QuestionnaireCourseGroupDetail
                                             {
                                                 QuestionnaireCourseGroupId = model.QuestionnaireCourseGroupId,
                                                 CourseId = item
                                             };

                            questionnaireCourseGroupDetails.Add(addDetails);
                        }
                    }
                }

                _db.QuestionnaireCourseGroupDetails.RemoveRange(removeDetails);
                _db.QuestionnaireCourseGroupDetails.AddRange(questionnaireCourseGroupDetails);

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return Redirect(returnUrl);
            }
            catch 
            {
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.UnableToEdit);
                CreateSelectList();
                return View(model);
            }
        }

        public ActionResult Delete(long id)
        {
            QuestionnaireCourseGroup model = _masterProvider.GetQuestionnaireCourseGroup(id);
            var courseGroupDetails = _db.QuestionnaireCourseGroupDetails.Where(x => x.QuestionnaireCourseGroupId == id)
                                                                        .IgnoreQueryFilters()
                                                                        .ToList();
            try
            {
                _db.QuestionnaireCourseGroupDetails.RemoveRange(courseGroupDetails);
                _db.QuestionnaireCourseGroups.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private void CreateSelectList()
        {
            ViewBag.Courses = _selectListProvider.GetCourses();
        }
    }
}