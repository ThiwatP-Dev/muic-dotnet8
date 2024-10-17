using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("SectionReport", "")]
    public class SectionReportController : BaseController
    {
        protected readonly ISectionProvider _sectionProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly ICacheProvider _cacheProvider;
        public SectionReportController(ApplicationDbContext db,
                                       IFlashMessage flashMessage,
                                       ISelectListProvider selectListProvider,
                                       IDateTimeProvider dateTimeProvider,
                                       ICacheProvider cacheProvider,
                                       ISectionProvider sectionProvider) : base(db, flashMessage, selectListProvider) 
                                       
        {
            _sectionProvider = sectionProvider;
            _dateTimeProvider = dateTimeProvider;
            _cacheProvider = cacheProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.FacultyId, criteria.CourseId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                criteria.AcademicLevelId = _db.AcademicLevels.SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
                criteria.TermId = _cacheProvider.GetCurrentTerm(criteria.AcademicLevelId).Id;
                CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.FacultyId, criteria.CourseId);
                return View(new PagedResult<SectionReportViewModel>()
                            {
                                Criteria = criteria
                            });
            }
            
            DateTime? startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);
            var sections = _db.Sections.Where(x => x.TermId == criteria.TermId
                                                   && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                       || x.Course.Code.Contains(criteria.CodeAndName)
                                                       || x.Course.NameEn.Contains(criteria.CodeAndName)
                                                       || x.Course.NameTh.Contains(criteria.CodeAndName)
                                                       || (criteria.CodeAndName.Contains("*")
                                                           && x.Course.CourseRateId == 2))
                                                   && (criteria.FacultyId == 0
                                                       || x.Course.FacultyId == criteria.FacultyId)
                                                   && (string.IsNullOrEmpty(criteria.IsClosed)
                                                       || x.IsClosed == Convert.ToBoolean(criteria.IsClosed))
                                                   && (string.IsNullOrEmpty(criteria.SeatAvailable)
                                                       || (Convert.ToBoolean(criteria.SeatAvailable) ? x.SeatAvailable > 0
                                                                                                       : x.SeatAvailable == 0))
                                                   && (string.IsNullOrEmpty(criteria.HaveMidterm)
                                                       || x.MidtermDate != null == Convert.ToBoolean(criteria.HaveMidterm))
                                                   && (string.IsNullOrEmpty(criteria.HaveFinal)
                                                       || x.FinalDate != null == Convert.ToBoolean(criteria.HaveFinal))
                                                   && (string.IsNullOrEmpty(criteria.Status)
                                                       || x.Status == criteria.Status)
                                                   && (startedAt == null
                                                       || x.OpenedSectionAt.Value.Date >= startedAt.Value.Date)
                                                   && (endedAt == null
                                                       || x.OpenedSectionAt.Value.Date <= endedAt.Value.Date)
                                                   && (string.IsNullOrEmpty(criteria.CreatedBy)
                                                       || x.CreatedBy == criteria.CreatedBy)
                                                   && (string.IsNullOrEmpty(criteria.SectionType)
                                                       || (criteria.SectionType == "o" ? x.IsOutbound
                                                                                       : criteria.SectionType == "g"
                                                                                       ? x.IsSpecialCase
                                                                                       : criteria.SectionType == "j" 
                                                                                       ? x.ParentSectionId != null
                                                                                       : x.ParentSectionId == null)))
                                       .Select(x => new SectionReportViewModel
                                               {
                                                   Id = x.Id,
                                                   CourseCode = x.Course.Code,
                                                   CourseNameEn = x.Course.NameEn,
                                                   CourseCredit = x.Course.Credit,
                                                   CourseRateId = x.Course.CourseRateId,
                                                   CourseLab = x.Course.Lab,
                                                   CourseLecture = x.Course.Lecture,
                                                   CourseOther = x.Course.Other,
                                                   Title = x.MainInstructor.Title.NameEn,
                                                   FirstNameEn = x.MainInstructor.FirstNameEn,
                                                   LastNameEn = x.MainInstructor.LastNameEn,
                                                   FacultyNameEn = x.Course.Faculty.NameEn,
                                                   Number = x.Number,
                                                   MidtermDate = x.MidtermDate,
                                                   MidtermStart = x.MidtermStart,
                                                   MidtermEnd = x.MidtermEnd,
                                                   FinalDate = x.FinalDate,
                                                   FinalStart = x.FinalStart,
                                                   FinalEnd = x.FinalEnd,
                                                   SeatLimit = x.SeatLimit,
                                                   SeatUsed = x.SeatUsed,
                                                   PlanningSeat = x.PlanningSeat,
                                                   Status = x.Status,
                                                   IsClosed = x.IsClosed,
                                                   ApprovedBy = x.ApprovedBy,
                                                   ApprovedAt = x.ApprovedAt,
                                                   Remark = x.Remark,
                                                   ParentSection = x.ParentSectionId,
                                                   OpenedSectionAt = x.OpenedSectionAt,
                                                   ClosedSectionAt = x.ClosedSectionAt,
                                                   SeatAvailable = x.SeatAvailable,
                                                   ParentSectionId = x.ParentSectionId,
                                                   ParentSectionCourseCode = x.ParentSection.Course.Code,
                                                   ParentSectionNumber = x.ParentSection.Number,
                                                   ParentSeatUsed = x.ParentSection == null ? 0 : x.ParentSection.SeatUsed,
                                                   ParentCourseRateId = x.ParentSection.Course.CourseRateId,
                                                   IsOutbound = x.IsOutbound,
                                                   IsSpecialCase = x.IsSpecialCase,
                                                   CreatedBy = x.CreatedBy
                                               })
                                       .OrderBy(x => x.CourseCode)
                                           .ThenBy(x => x.NumberValue)
                                       .GetPaged(criteria, page);

            sections.Results = sections.Results.Where(x => ((criteria.StudentLessThan ?? 0) == 0 || x.SeatUsed <= criteria.StudentLessThan)
                                            && (string.IsNullOrEmpty(criteria.IsNoStudent) 
                                            || (Convert.ToBoolean(criteria.IsNoStudent) ? x.SeatUsed == 0 : x.SeatUsed > 0)))
                               .ToList();

            var sectionIds = sections.Results.Select(x => x.Id).ToList();
            var sectionIdsNullable = sections.Results.Select(x => (long?)x.Id).ToList();
            var parentSectionIds = sections.Results.Where(x => x.ParentSectionId != null).Select(x => x.ParentSectionId);
            var jointTotalSeatUsed = _db.Sections.Where(x => parentSectionIds.Contains(x.ParentSectionId));
            var jointSections = _db.Sections.Where(x => sectionIdsNullable.Contains(x.ParentSectionId))
                                            .Select(x => new JointSectionReportViewModel
                                                         {
                                                             Id = x.Id,
                                                             ParentSectionId = x.ParentSectionId,
                                                             Number = x.Number,
                                                             CourseCode = x.Course.Code,
                                                             SeatUsed = x.SeatUsed,
                                                             CourseRateId = x.Course.CourseRateId
                                                         })
                                            .ToList();

            var sectionDetails = _db.SectionDetails.Where(x => sectionIds.Contains(x.SectionId)
                                                               && (criteria.InstructorId == 0
                                                                   || x.InstructorSections.Any(y => y.InstructorId == criteria.InstructorId)))
                                                   .Select(x => new SectionDetailReportViewModel
                                                                {
                                                                    SectionDetailId = x.Id,
                                                                    SectionId = x.SectionId,
                                                                    Day = x.Day,
                                                                    StartTime = x.StartTime,
                                                                    EndTime = x.EndTime,
                                                                    RoomNameEn = x.Room.NameEn
                                                                })
                                                   .OrderBy(x => x.Day)
                                                       .ThenBy(x => x.StartTime)
                                                   .ToList();

            var sectionDetailIds = sectionDetails.Select(x => x.SectionDetailId).ToList();
            var instructorSections = _db.InstructorSections.Where(x => sectionDetailIds.Contains(x.SectionDetailId))
                                                           .Select(x => new 
                                                                        {
                                                                            SectionDetailId = x.SectionDetailId,
                                                                            FirstNameEn = x.Instructor.FirstNameEn,
                                                                            LastNameEn = x.Instructor.LastNameEn,
                                                                            Title = x.Instructor.Title.NameEn
                                                                        })
                                                           .ToList();

            foreach (var item in sectionDetails)
            {
                item.InstructorSections = instructorSections.Where(x => x.SectionDetailId == item.SectionDetailId)
                                                            .Select(x => $"{ x.Title } { x.FirstNameEn } { x.LastNameEn }")
                                                            .ToList();
            }

            var users = _db.Users.Where(x => sections.Results.Select(y => y.CreatedBy).Contains(x.Id))
                                 .ToList();

            var instructors = _db.Instructors.Include(x => x.Title)
                                             .Where(x => users.Select(y => y.InstructorId).Contains(x.Id))
                                             .ToList();

            foreach (var item in sections.Results)
            {
                var user = users.SingleOrDefault(x => x.Id == item.CreatedBy);
                var instructor = user != null ? instructors.SingleOrDefault(x => x.Id == user.InstructorId) : null;
                if (instructor != null)
                {
                    item.CreatedByText = $"{ instructor.Title.NameEn } { instructor.FirstNameEn } { instructor.LastNameEn }";
                }
                else if (user != null && !string.IsNullOrEmpty(user.FirstnameEN))
                {
                    item.CreatedByText = $"{ user.FirstnameEN } { user.LastnameEN }"; ;
                }
                else if (user != null)
                {
                    item.CreatedByText = user.UserName;
                }
                else
                {
                    item.CreatedByText = item.CreatedBy;
                }

                item.JointSections = jointSections.Where(x => x.ParentSectionId == item.Id)
                                                  .ToList();

                if (item.ParentSectionId == null)
                {
                    item.TotalSeatUsed = item.SeatUsed + item.JointSections.Sum(x => x.SeatUsed);
                }
                else
                {
                    item.TotalSeatUsed = item.ParentSeatUsed + jointTotalSeatUsed.Where(x => x.ParentSectionId == item.ParentSectionId)
                                                                                 .Sum(x => x.SeatUsed);
                }
                                                
                item.SectionDetails = sectionDetails.Where(x => x.SectionId == item.Id)
                                                    .ToList();
            }
            return View(sections);
        }

        public IActionResult ExportExcel(Criteria criteria, string returnUrl)
        {
            var sections = _sectionProvider.GetSectionReportViewModel(criteria);
            ViewBag.ReturnUrl = returnUrl;
            return View(sections);
        }

        public IActionResult ExportExcels(Criteria criteria, string returnUrl)
        {
            var sections = _sectionProvider.GetSectionReportViewModel(criteria);
            ViewBag.ReturnUrl = returnUrl;
            return View(sections);
        }

        private void CreateSelectList(long academicLevelId = 0, long termId = 0, long facultyId = 0, long courseId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.OpenCloseStatuses = _selectListProvider.GetOpenCloseStatuses();
            ViewBag.Statuses = _selectListProvider.GetSectionStatuses();
            ViewBag.Examinations = _selectListProvider.GetAllYesNoAnswer();
            ViewBag.Instructors = _selectListProvider.GetInstructors();
            ViewBag.SectionTypes = _selectListProvider.GetSectionTypes();
            ViewBag.Users = _selectListProvider.GetUsersFullNameEn();
            ViewBag.SeatAvailableStatuses = _selectListProvider.GetSeatAvailableStatuses();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                if (facultyId > 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }
            
            if (termId > 0)
            {
                ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);
                if (courseId > 0)
                {
                    ViewBag.Sections = _selectListProvider.GetSections(termId, courseId);
                }
            }
        }
    }
}