using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class PublishedGradeReportController : BaseController
    {
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IUserProvider _userProvider;

        public PublishedGradeReportController(ApplicationDbContext db,
                                              ISelectListProvider selectListProvider,
                                              IFlashMessage flashMessage,
                                              IMapper mapper,
                                              IUserProvider userProvider,
                                              IRegistrationProvider registrationProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _registrationProvider = registrationProvider;
            _userProvider = userProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId);
            if (criteria.CourseId == 0 && criteria.TermId == 0 && string.IsNullOrEmpty(criteria.Status))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var models = _db.Barcodes.Include(x => x.Course)
                                     .Include(x => x.Section)
                                     .Where(x => (criteria.CourseId == 0
                                                  || x.CourseId == criteria.CourseId)
                                                  && (string.IsNullOrEmpty(criteria.Status)
                                                      || x.IsPublished == Convert.ToBoolean(criteria.Status))
                                                  && (criteria.AcademicLevelId == 0
                                                      || x.Section.Term.AcademicLevelId == criteria.AcademicLevelId)
                                                  && (criteria.TermId == 0
                                                      || x.Section.TermId == criteria.TermId))
                                     .AsNoTracking()
                                     .Select(x => new PublishedGradeReportViewModel
                                                  {
                                                      BarcodeNumber = x.BarcodeNumber,
                                                      Course = x.Course.CourseAndCredit,
                                                      SectionIds = x.SectionIds,
                                                      GeneratedAt = x.GeneratedAt,
                                                      PublishedAt = x.PublishedAt,
                                                      PublishedBy = x.PublishedBy,
                                                      ApprovedAt = x.ApprovedAt,
                                                      ApprovedBy = x.ApprovedBy,
                                                      SectionNumber = x.Section.Number,
                                                      SectionType = x.Section.SectionTypes
                                                  })
                                     .OrderBy(x => x.BarcodeNumber)
                                     .ToList();
            
            var userId = models.SelectMany(x => new[] {x.ApprovedBy, x.PublishedBy}).ToList();
            var users = _userProvider.GetCreatedFullNameByIds(userId);
            foreach (var item in models)
            {
                var sectionIds = item.SectionIds == null ? null: JsonConvert.DeserializeObject<List<long>>(item.SectionIds);
                if (item.SectionIds != null)
                {
                    var sections = _db.Sections.Where(x => sectionIds.Contains(x.Id))
                                               .Select(x => x.Course.Code + "(" + x.Number + ")")
                                               .ToList();
                    
                    item.Sections = string.Join(", ", sections);
                    item.ApprovedBy = users.Where(x => x.CreatedBy == item.ApprovedBy)?.FirstOrDefault()?.CreatedByFullNameEn;
                    item.PublishedBy = users.Where(x => x.CreatedBy == item.PublishedBy)?.FirstOrDefault()?.CreatedByFullNameEn;
                }
            }

            var modelPageResult = models.AsQueryable().GetPaged(criteria, page, true);
            return View(modelPageResult);
        }

        private void CreateSelectList(long academicLevelId, long termId)
        {
            ViewBag.Statuses = _selectListProvider.GetYesNoAnswer();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }

            if (termId != 0)
            {
                ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);
            }
        }
    }
}