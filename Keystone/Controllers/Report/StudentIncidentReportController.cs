using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class StudentIncidentReportController : BaseController
    {
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IUserProvider _userProvider;

        public StudentIncidentReportController(ApplicationDbContext db,
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
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var models = _db.StudentIncidents.AsNoTracking()
                                             .IgnoreQueryFilters()
                                             .Where(x => (criteria.IncidentId == 0
                                                          || x.IncidentId == criteria.IncidentId)
                                                          && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                              || x.Student.Code.Contains(criteria.CodeAndName)
                                                              || x.Student.FirstNameEn.Contains(criteria.CodeAndName)
                                                              || x.Student.LastNameEn.Contains(criteria.CodeAndName))
                                                          && (criteria.AcademicLevelId == 0
                                                              || x.Term.AcademicLevelId == criteria.AcademicLevelId)
                                                          && (criteria.TermId == 0
                                                              || x.TermId == criteria.TermId))
                                             .Select(x => new StudentIncidentReportViewModel
                                                          { 
                                                            StudentCode = x.Student.Code,
                                                            TitleEn = x.Student.Title.NameEn,
                                                            FirstNameEn = x.Student.FirstNameEn,
                                                            MidNameEn = x.Student.MidNameEn,
                                                            LastNameEn = x.Student.LastNameEn,
                                                            Incident = x.Incident.NameEn,
                                                            AcademicTerm = x.Term.AcademicTerm,
                                                            AcademicYear = x.Term.AcademicYear,
                                                            LockedDocument = x.LockedDocument,
                                                            LockedRegistration  = x.LockedRegistration,
                                                            LockedPayment  = x.LockedPayment,
                                                            LockedVisa = x.LockedVisa,
                                                            LockedGraduation = x.LockedGraduation,
                                                            LockedChangeFaculty = x.LockedChangeFaculty,
                                                            LockedSignIn = x.LockedSignIn,
                                                            CreatedBy = x.CreatedBy,
                                                            UpdatedBy = x.UpdatedBy,
                                                            ApprovedBy = x.ApprovedBy,
                                                            ApprovedAt = x.ApprovedAt,
                                                            UpdatedAt = x.UpdatedAt,
                                                            CreatedAt = x.CreatedAt,
                                                            IsActive = x.IsActive
                                                          })
                                             .OrderBy(x => x.StudentCode)
                                                .ThenBy(x => x.CreatedAt)
                                             .GetPaged(criteria, page, true);
            
            var userIds = models.Results.SelectMany(x => new[] {x.CreatedBy, x.UpdatedBy}).ToList();
            var users = _userProvider.GetCreatedFullNameByIds(userIds);
            foreach (var item in models.Results)
            {
                item.UpdatedBy = users.Where(x => x.CreatedBy == item.UpdatedBy)?.FirstOrDefault()?.CreatedByFullNameEn;
                item.CreatedBy = users.Where(x => x.CreatedBy == item.CreatedBy)?.FirstOrDefault()?.CreatedByFullNameEn;
            }

            return View(models);
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Incidents = _selectListProvider.GetIncidents();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}