using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("StudentSchedule", "")]
    public class StudentScheduleController : BaseController
    {
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IScheduleProvider _scheduleProvider;

        public StudentScheduleController(ApplicationDbContext db,
                                         IMapper mapper,
                                         IMemoryCache memoryCache,
                                         ISelectListProvider selectListProvider,
                                         IScheduleProvider scheduleProvider,
                                         IFlashMessage flashMessage,
                                         IAcademicProvider academicProvider) : base(db, flashMessage, mapper, selectListProvider) 
        { 
            _academicProvider = academicProvider;
            _scheduleProvider = scheduleProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0 || string.IsNullOrEmpty(criteria.CodeAndName))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            StudentScheduleViewModel model = new StudentScheduleViewModel();
            model = _db.Students.Include(x => x.RegistrationCourses)
                                .Include(x => x.AcademicInformation)
                                    .ThenInclude(x => x.Faculty)
                                .Include(x => x.AcademicInformation)
                                    .ThenInclude(x => x.Department)
                                .Include(x => x.AcademicInformation)
                                    .ThenInclude(x => x.CurriculumVersion)
                                .Include(x => x.AcademicInformation)
                                    .ThenInclude(x => x.Advisor)
                                    .ThenInclude(x => x.Title)
                                .Where(x => string.IsNullOrEmpty(criteria.CodeAndName)
                                            || x.Code.StartsWith(criteria.CodeAndName))
                                .Select(x => _mapper.Map<Student, StudentScheduleViewModel>(x))
                                .SingleOrDefault();

            if (model != null)
            {
                var term = _academicProvider.GetTerm(criteria.TermId);
                model.Term = term == null ? "" : term.TermText;
                var sectionIds = model.RegistrationCourses.Where(x => x.TermId == criteria.TermId
                                                                      && x.Status != "d")
                                                          .Select(x => x.SectionId ?? 0)
                                                          .ToList();

                model.Schedules = _scheduleProvider.GetSchedule(sectionIds);
                ViewBag.CoursesJsonData = JsonConvert.SerializeObject(model.Schedules);
                return View(model);
            }

            _flashMessage.Danger(Message.StudentNotFound);
            return View();
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}