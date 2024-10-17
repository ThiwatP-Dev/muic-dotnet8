using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("GraduatedStudentReport", "")]
    public class GraduatedStudentReportController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        public GraduatedStudentReportController(ApplicationDbContext db,
                                                IMapper mapper,
                                                IFlashMessage flashMessage,
                                                ISelectListProvider selectListProvider,
                                                IDateTimeProvider dateTimeProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            var model = new GraduatedStudentReportViewModel();

            var startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            var endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);
            
            model.Criteria = criteria;
            model.GraduatedStudentReportDetails = _db.GraduationInformations.Include(x => x.Student)
                                                                                .ThenInclude(x => x.Title)
                                                                            .Include(x => x.Term)
                                                                            .Include(x => x.AcademicHonor)
                                                                            .Where(x => x.Term.AcademicLevelId == criteria.AcademicLevelId
                                                                                        && (criteria.TermId == 0
                                                                                            || x.TermId == criteria.TermId)
                                                                                        && (criteria.HonorId == 0
                                                                                            || x.HonorId == criteria.HonorId)
                                                                                        && (startedAt == null
                                                                                           || x.GraduatedAt.Value.Date >= startedAt.Value.Date)
                                                                                        && (endedAt == null
                                                                                           || x.GraduatedAt.Value.Date <= endedAt.Value.Date)
                                                                                        && (string.IsNullOrEmpty(criteria.StudentStatus)
                                                                                            || x.Student.StudentStatus == criteria.StudentStatus))
                                                                            .Select(x => _mapper.Map<GraduationInformation, GraduatedStudentReportDetail>(x))
                                                                            .IgnoreQueryFilters()
                                                                            .OrderBy(x => x.GraduatedTerm)
                                                                                .ThenBy(x => x.Code)
                                                                            .ToList();
            return View(model);
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Honors = _selectListProvider.GetAcademicHonors();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
        }
    }
}