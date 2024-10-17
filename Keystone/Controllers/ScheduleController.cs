using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Keystone.Controllers
{
    [PermissionAuthorize("Schedule", "")]
    public class ScheduleController : BaseController
    {
        protected readonly ICacheProvider _cacheProvider;

        public ScheduleController(ApplicationDbContext db,
                                  IMapper mapper,
                                  IMemoryCache memoryCache,
                                  ISelectListProvider selectListProvider,
                                  ICacheProvider cacheProvider) : base(db, mapper, selectListProvider)
        { 
            _cacheProvider = cacheProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            var instructors = _cacheProvider.GetInstructors();

            var sections = (from section in _db.Sections.Include(x => x.Course)
                                                        .Include(x => x.Term)
                                                        .Include(x => x.ParentSection)
                                                            .ThenInclude(x => x.SectionDetails)
                                                            .ThenInclude(x => x.InstructorSections)
                                                            .ThenInclude(x => x.Instructor)
                                                            .ThenInclude(x => x.Title)
                                                        .Include(x => x.ParentSection)
                                                            .ThenInclude(x => x.SectionDetails)
                                                            .ThenInclude(x => x.Room)
                                                        .Include(x => x.SectionDetails)
                                                            .ThenInclude(x => x.InstructorSections)
                                                            .ThenInclude(x => x.Instructor)
                                                            .ThenInclude(x => x.Title)

                            join sectionDetail in _db.SectionDetails.Include(x => x.Room)
                                                                        .ThenInclude(x => x.Building)
                                               on section.Id equals sectionDetail.SectionId
                            where section.TermId == criteria.TermId
                                  && !section.IsParent
                                  && (criteria.AcademicLevelIds == null
                                      || criteria.AcademicLevelIds.Contains(section.Course.AcademicLevelId))
                                  && (criteria.CampusIds == null
                                      || criteria.CampusIds.Contains(sectionDetail.Room.Building.CampusId))
                                  && (criteria.FacultyId == 0
                                      || criteria.FacultyId == section.Course.FacultyId)
                                  && (criteria.DepartmentId == 0
                                      || criteria.DepartmentId == section.Course.DepartmentId)
                                  && (criteria.CourseIds == null
                                      || !criteria.CourseIds.Any()
                                      || criteria.CourseIds.Contains(section.CourseId))
                                  && (criteria.SectionNumbers == null
                                      || !criteria.SectionNumbers.Any()
                                      || criteria.SectionNumbers.Contains(section.Number))
                                  && (criteria.InstructorIds == null
                                      || !criteria.InstructorIds.Any()
                                      || criteria.InstructorIds.Contains(sectionDetail.InstructorId.Value))
                                  && (criteria.IsEvening == "All"
                                      || String.IsNullOrEmpty(criteria.IsEvening)
                                      || section.IsEvening == Convert.ToBoolean(criteria.IsEvening))
                            group section by section.Id into x
                            select new ScheduleViewModel()
                                   {
                                       Id = x.Key,
                                       ParentSectionId = x.First().ParentSectionId,
                                       CourseCode = x.First().Course.Code,
                                       CourseName = x.First().Course.NameEn,
                                       Section = x.First().Number,
                                       MidtermDate = x.First().MidtermString,
                                       MidtermTime = x.First().MidtermTime,
                                       FinalDate = x.First().FinalString,
                                       FinalTime = x.First().FinalTime,
                                       IsClosed = x.First().IsClosed,
                                       StartedAt = x.First().Term.StartedAt == x.First().OpenedAt ? null : x.First().OpenedAt,
                                       EndedAt = x.First().Term.EndedAt == x.First().ClosedAt ? null : x.First().ClosedAt,
                                       ScheduleTimes =  (x.First().ParentSection == null ? x.First().SectionDetails
                                                                                         : x.First().ParentSection.SectionDetails.Union(x.First().SectionDetails))
                                                        .Select(y => _mapper.Map<SectionDetail, ClassScheduleTimeViewModel>(y))
                                                        .OrderBy(y => y.Day)
                                                        .ThenBy(y => y.StartTime)
                                                        .ToList()
                                   }).OrderBy(x => x.CourseCode)
                                     .ThenBy(x => x.CourseName)
                                     .ThenBy(x => x.Section)
                                     .AsNoTracking()
                                     .ToList();

            // foreach (var item in sections)
            // {
            //     item.ScheduleTimes.Select(x => {
            //                                        x.InstructorNameEn = String.Join(", ", instructors.Where(y => x.InstructorIds.Contains(y.Id))
            //                                                                                  .Select(y => y.CodeAndName));
            //                                        x.InstructorNameTh = String.Join(", ", instructors.Where(y => x.InstructorIds.Contains(y.Id))
            //                                                                                  .Select(y => y.CodeAndNameTh));
            //                                        return x;
            //                                    }).ToList();
            // }

            var model = sections.AsQueryable().GetPaged(criteria, page, true);

            return View(model);
        }

        private void CreateSelectList(long academicLevleId, long facultyId) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Programs = _selectListProvider.GetSectionPrograms();
            ViewBag.Campuses = _selectListProvider.GetCampuses();
            ViewBag.Instructors = _selectListProvider.GetInstructors();
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.Sections = _selectListProvider.GetSectionNumbers();

            if (academicLevleId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevleId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevleId);
            }

            if (facultyId != 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevleId, facultyId);
            }
        }
    }
}