using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Curriculums;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class StudyPlanController : BaseController
    {
        protected readonly ICurriculumProvider _curriculumProvider;
        protected readonly IRegistrationProvider _registrationProvider;

        public StudyPlanController(ApplicationDbContext db,
                                   IFlashMessage flashMessage,
                                   IMapper mapper,
                                   ISelectListProvider selectListProvider,
                                   ICurriculumProvider curriculumProvider,
                                   IRegistrationProvider registrationProvider) : base(db,flashMessage, mapper, selectListProvider) 
        {
            _curriculumProvider = curriculumProvider;
            _registrationProvider = registrationProvider;
        }

        public IActionResult Create(long curriculumId, string returnUrl)
        {
            var studyPlan = GetStudyPlanViewModel(curriculumId);
            ViewBag.ReturnUrl = returnUrl;
            return View(studyPlan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([FromBody] StudyPlanDetailViewModel model, string returnUrl)
        {
            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var studyPlan = _mapper.Map<StudyPlanDetailViewModel, StudyPlan>(model);
                    _db.StudyPlans.Add(studyPlan);
                    _db.SaveChanges();

                    List<StudyCourse> studyCourses = SetStudyCourses(model.CoursesPlan, studyPlan.Id);
                    _db.StudyCourses.AddRange(studyCourses);
                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return StatusCode((int)HttpStatusCode.Forbidden, Message.UnableToCreate);
                }
            }

            string url = this.Url.Action(nameof(CurriculumVersionController.Details), 
                                         nameof(CurriculumVersion), 
                                         new { id = model.CurriculumVersionId, tabIndex = "1", returnUrl = returnUrl });
            return Ok(url);
        }

        public IActionResult Edit(long versionId, long studyPlanId, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = GetStudyPlanViewModel(versionId);
            var studyPlan = GetStudyPlan(studyPlanId);
            model.Id = studyPlanId;
            model.Year = studyPlan.Year;
            model.Term = studyPlan.Term;
            model.StudyCourses = studyPlan.StudyCourses;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([FromBody] StudyPlanDetailViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var studyCourses = _curriculumProvider.GetStudyCoursesByPlanId(model.Id);
                    _db.StudyCourses.RemoveRange(studyCourses);

                    var studyPlan = _mapper.Map<StudyPlanDetailViewModel, StudyPlan>(model);
                    var modelToUpdate = _curriculumProvider.GetStudyPlanById(model.Id);
                    modelToUpdate = _mapper.Map<StudyPlan, StudyPlan>(studyPlan, modelToUpdate);
                    
                    _db.SaveChanges();

                    List<StudyCourse> studyCoursesToUpdate = SetStudyCourses(model.CoursesPlan, modelToUpdate.Id);  

                    _db.StudyCourses.AddRange(studyCoursesToUpdate);
                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return StatusCode((int)HttpStatusCode.Forbidden, Message.UnableToCreate);
                }
            }

            string url = this.Url.Action(nameof(CurriculumVersionController.Details), 
                                         nameof(CurriculumVersion), 
                                         new { id = model.CurriculumVersionId, tabIndex = "1", returnUrl = returnUrl });
            return Ok(url);
        }

        public ActionResult Delete(long id, string returnUrl)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                var studyCourses = _curriculumProvider.GetStudyCoursesByPlanId(id);
                var studyPlan = _curriculumProvider.GetStudyPlanById(id);
                try
                {
                    _db.StudyCourses.RemoveRange(studyCourses);
                    _db.StudyPlans.Remove(studyPlan);
                    _db.SaveChanges();

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToDelete);
                }

                return RedirectToAction(nameof(CurriculumVersionController.Details), 
                                        nameof(CurriculumVersion), 
                                        new { id = studyPlan.CurriculumVersionId, tabIndex = "1", returnUrl = returnUrl });
            }
        }

        private StudyPlanViewModel GetStudyPlanViewModel(long versionId)
        {
            var curriculum = _curriculumProvider.GetCurriculumVersion(versionId);
            var courses = GetCoursePlan(versionId);
       
            var studyPlan = new StudyPlanViewModel()
                            {
                                CurriculumVersion = curriculum,
                                CoursesPlan = courses  
                            };

            return studyPlan;
        }

        private StudyPlan GetStudyPlan(long studyPlanId)
        {
            var studyPlan = _curriculumProvider.GetStudyPlanById(studyPlanId);
            return studyPlan;
        }

        private List<CoursePlan> GetCoursePlan(long versionId)
        {
            var courses = _curriculumProvider.GetCurriculumCourse(versionId)
                                             .Select(x => _mapper.Map<Course, CoursePlan>(x))
                                             .ToList();
            
            return courses;
        }

        private List<StudyCourse> SetStudyCourses(List<CoursePlan> coursePlans, long studyPlanId)
        {
            List<StudyCourse> studyCourses = new List<StudyCourse>();
            foreach (var item in coursePlans)
            {
                StudyCourse studyCourse = SetStudyCourse(item);
                studyCourse.StudyPlanId = studyPlanId;
                studyCourses.Add(studyCourse);
            }

            return studyCourses;
        }

        private StudyCourse SetStudyCourse(CoursePlan item)
        {
            StudyCourse studyCourse = new StudyCourse();
            if (item.Id == 0)
            {
                studyCourse = _mapper.Map<CoursePlan, StudyCourse>(item);
            }
            else 
            {
                var course = _registrationProvider.GetCourse(item.Id);
                studyCourse = _mapper.Map<Course, StudyCourse>(course);
            }
            
            return studyCourse;
        }
    }
}