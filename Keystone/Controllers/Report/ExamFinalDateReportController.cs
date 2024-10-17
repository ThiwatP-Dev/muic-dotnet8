using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("ExamFinalDateReport", "")]
    public class ExamFinalDateReportController : BaseController
    {
        protected readonly IReservationProvider _reservationProvider;
        protected readonly ISectionProvider _sectionProvider;

        public ExamFinalDateReportController(ApplicationDbContext db,
                                             ISelectListProvider selectListProvider,
                                             IReservationProvider reservationProvider,
                                             ISectionProvider sectionProvider,
                                             IFlashMessage flashMessage) : base(db, flashMessage, selectListProvider)
        {
            _reservationProvider = reservationProvider;
            _sectionProvider = sectionProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var results = _db.Sections.Where(x => (criteria.AcademicLevelId == 0
                                                   || x.Term.AcademicLevelId == criteria.AcademicLevelId)
                                                   && (criteria.TermId == 0
                                                       || x.TermId == criteria.TermId)
                                                   && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                       || x.Course.Code.Contains(criteria.CodeAndName)
                                                       || x.Course.NameEn.Contains(criteria.CodeAndName))
                                                   && ((criteria.SectionFrom ?? 0) == 0
                                                       || x.NumberValue >= criteria.SectionFrom)
                                                   && ((criteria.SectionTo ?? 0) == 0
                                                       || x.NumberValue <= criteria.SectionTo)
                                                   && (string.IsNullOrEmpty(criteria.SectionStatus)
                                                       || x.Status == criteria.SectionStatus)
                                                   && (string.IsNullOrEmpty(criteria.HaveFinal)
                                                       || (Convert.ToBoolean(criteria.HaveFinal) ? x.FinalDate.HasValue && x.FinalDate != new DateTime()
                                                                                                : !x.FinalDate.HasValue || x.FinalDate == new DateTime()))
                                                   && (string.IsNullOrEmpty(criteria.SectionType)
                                                       || (criteria.SectionType == "o" ? x.IsOutbound
                                                                                       : criteria.SectionType == "g"
                                                                                       ? x.IsSpecialCase
                                                                                       : criteria.SectionType == "j" 
                                                                                       ? x.ParentSectionId != null
                                                                                       : x.ParentSectionId == null)))
                                      .Select(x => new UpdateFinalDate
                                                   {
                                                       SectionNumber = x.Number,
                                                       CourseCode = x.Course.Code,
                                                       CourseName = x.Course.NameEn,
                                                       CourseCredit = x.Course.Credit,
                                                       CourseLab = x.Course.Lab,
                                                       CourseLecture = x.Course.Lecture,
                                                       CourseOther = x.Course.Other,
                                                       SeatUsedText = x.SeatUsedText,
                                                       FinalDate = x.FinalDate,
                                                       FinalDateTime = x.FinalDateTime,
                                                       SectionTypes = x.SectionTypes,
                                                       StatusText = x.StatusText,
                                                       InstructorName = x.MainInstructor == null ? null : x.MainInstructor.Title.NameEn + " " + x.MainInstructor.FirstNameEn + " " + x.MainInstructor.LastNameEn
                                                   })
                                      .OrderBy(x => x.CourseCode)
                                         .ThenBy(x => x.SectionNumber)
                                      .ToList();

            UpdateFinalDateViewModel model = new UpdateFinalDateViewModel
                                             {
                                                 Criteria = criteria,
                                                 Results = results
                                             };

            return View(model);
        }

        private void CreateSelectList(long academicLevelId)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.SectionStatuses = _selectListProvider.GetSectionStatuses();
            ViewBag.SectionTypes = _selectListProvider.GetSectionTypes();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}