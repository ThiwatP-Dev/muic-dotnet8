using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Schedules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class PlanController : BaseController
    {
        private IScheduleProvider _scheduleProvider;
        private ISectionProvider _sectionProvider;
        private ICacheProvider _cacheProvider;

        public PlanController(ApplicationDbContext db,
                              IFlashMessage flashMessage,
                              IMapper mapper,
                              ISelectListProvider selectListProvider,
                              IScheduleProvider scheduleProvider,
                              ISectionProvider sectionProvider,
                              ICacheProvider cacheProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _scheduleProvider = scheduleProvider;
            _sectionProvider = sectionProvider;
            _cacheProvider = cacheProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            var models = _db.Plans.Include(x => x.Term)
                                      .ThenInclude(x => x.AcademicLevel)
                                  .Include(x => x.Faculty)
                                  .Include(x => x.Student)
                                  .Include(x => x.PlanSchedules)
                                  .Where(x => (criteria.AcademicLevelId == 0
                                               || x.Term.AcademicLevelId == criteria.AcademicLevelId)
                                               && (criteria.TermId == 0
                                                   || x.TermId == criteria.TermId)
                                               && (string.IsNullOrEmpty(criteria.FirstName)
                                                   || x.Name.StartsWith(criteria.FirstName)))
                                  .ToList();
            
            var courses = _cacheProvider.GetCourses();

            foreach(var item in models)
            {
                item.CourseName = courses.Where(x => item.CourseList.Contains(x.Id))
                                         .Select(x => x.Code)
                                         .ToList();
            }

            var modelPageResult = models.AsQueryable()
                                        .GetPaged(criteria, page);

            return View(modelPageResult);
        }

        public ActionResult Manage(long id, Criteria criteria)
        {
            CreateSelectList(criteria);
            return View();
        }

        public ActionResult Delete()
        {
            return View();
        }

        public ActionResult ChooseSections()
        {
            return PartialView("_SelectSectionsModal");
        }

        public ActionResult ScheduleList()
        {
            var courseSections = new List<CourseSection>();
            
            // Mockup for input example
            //
            // courseSections.Add(new CourseSection { CourseId = 1, SectionIds = new List<long> { 9, 15, 20, 21, 22 } });
            // courseSections.Add(new CourseSection { CourseId = 2, SectionIds = new List<long> { 16, 17, 18, 23 } });
            // courseSections.Add(new CourseSection { CourseId = 13, SectionIds = new List<long> { 4, 8, 14 } });

            var registrationTermId = _db.Terms.SingleOrDefault(x => x.IsCurrent).Id;

            var model = new GenerateSchedule
            {
                SemesterId = registrationTermId,
                CourseSections = courseSections
            };

            var generatedSchedule = _scheduleProvider.GenerateSchedules(model);

            SchedulePlanViewModel schedulePlan = new SchedulePlanViewModel { Courses = generatedSchedule.GeneratedScheduleViewModel.Courses };

            if (generatedSchedule.IsSucceed)
            {
                foreach(var item in generatedSchedule.GeneratedScheduleViewModel.Schedules)
                {
                    ShowScheduleViewModel showSections = new ShowScheduleViewModel();
                    showSections.HasExamConflicted = item.HasExamConflicted;

                    foreach(var sectionId in item.SectionIds)
                    {
                        var section = _sectionProvider.GetSectionBySemester(sectionId, registrationTermId);
                        showSections.Sections.Add(section);
                    }

                    schedulePlan.Schedules.Add(showSections);
                }

                return View(schedulePlan);
            }
            
            //Fail to generate schedule cause class time confilct
            //
            return Ok(generatedSchedule.ErrorMessage);
        }

        public IActionResult GetCourseDetials(long id)
        {
            var courseDetails = _db.Courses.Include(x => x.Sections)
                                           .Where(x => x.Id == id)
                                           .SingleOrDefault();

            return PartialView("_CourseList", courseDetails);
        }

        private void CreateSelectList(Criteria criteria) 
        {
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.Sections = _selectListProvider.GetSections(criteria.CourseId);
        }

        public void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}