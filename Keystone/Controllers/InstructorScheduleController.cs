using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace Keystone.Controllers
{
    [PermissionAuthorize("InstructorSchedule", "")]
    public class InstructorScheduleController : BaseController
    {
        protected readonly IAcademicProvider _academicProvider;
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IScheduleProvider _scheduleProvider;

        public InstructorScheduleController(ApplicationDbContext db,
                                            IMapper mapper,
                                            IMemoryCache memoryCache,
                                            ISelectListProvider selectListProvider,
                                            IAcademicProvider academicProvider,
                                            ICacheProvider cacheProvider,
                                            IScheduleProvider scheduleProvider) : base(db, mapper, selectListProvider) 
        { 
            _academicProvider = academicProvider;
            _cacheProvider = cacheProvider;
            _scheduleProvider = scheduleProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.FacultyId, criteria.AcademicLevelId);
            var instructorIds = _db.InstructorSections.Include(x => x.SectionDetail)
                                                          .ThenInclude(x => x.Section)
                                                      .Where(x => x.SectionDetail.Section.TermId == criteria.TermId)
                                                      .Select(x => x.InstructorId)
                                                      .Distinct()
                                                      .ToList();

            var instructors = _db.Instructors.Include(x => x.InstructorWorkStatus)
                                                 .ThenInclude(x => x.AcademicLevel)
                                             .Include(x => x.InstructorWorkStatus)
                                                 .ThenInclude(x => x.Faculty)
                                             .Include(x => x.InstructorWorkStatus)
                                                 .ThenInclude(x => x.Department)
                                             .Where(x => instructorIds.Contains(x.Id)
                                                         && (criteria.FacultyId == 0
                                                             || x.InstructorWorkStatus.FacultyId == criteria.FacultyId)
                                                         && (criteria.DepartmentId == 0
                                                             || x.InstructorWorkStatus.DepartmentId == criteria.DepartmentId)
                                                         && (criteria.InstructorIds == null
                                                             || criteria.InstructorIds.Contains(x.Id)))
                                             .Select(x => _mapper.Map<Instructor, SearchInstructorScheduleViewModel>(x))
                                             .OrderBy(x => x.Code)
                                             .GetPaged(criteria, page);

            return View(instructors);
        }

        public ActionResult Details(long termId, long instructorId, long facultyId)
        {
            InstructorScheduleViewModel model = new InstructorScheduleViewModel();
            model = _cacheProvider.GetInstructors()
                                  .Where(x => x.Id == instructorId)
                                  .Select(x => _mapper.Map<Instructor, InstructorScheduleViewModel>(x))
                                  .SingleOrDefault();

            model.TermId = termId;
            model.FacultyId = facultyId;
            var term = _academicProvider.GetTerm(termId);
            model.Term = term == null ? "" : term.TermText;
            model.Schedules = _scheduleProvider.GetInstructorSchedule(termId, instructorId);
            ViewBag.CoursesJsonData = JsonConvert.SerializeObject(model.Schedules);
            return View(model);
        }

        private void CreateSelectList(long facultyId, long academicLevelId)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            ViewBag.Instructors = facultyId == 0 ? _selectListProvider.GetInstructors()
                                                 : _selectListProvider.GetInstructorsByFacultyId(facultyId);
        }
    }
}