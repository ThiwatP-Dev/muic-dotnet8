using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("PetitionReport", "")]
    public class PetitionReportController : BaseController
    {
        protected readonly IRegistrationProvider _registrationProvider;

        public PetitionReportController(ApplicationDbContext db,
                                        ISelectListProvider selectListProvider,
                                        IFlashMessage flashMessage) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            var model = new PetitionReportViewModel
                        {
                            Criteria = criteria
                        };

            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Danger(Message.RequiredData);
                return View();
            }

            model.PetitionReportDetails = _db.StudentPetitions.Include(x => x.Petition)
                                                              .Include(x => x.Student)
                                                              .Where(x => x.Term.AcademicLevelId == criteria.AcademicLevelId
                                                                          && x.TermId == criteria.TermId
                                                                          && (criteria.PetitionId == 0
                                                                              || x.PetitionId == criteria.PetitionId)
                                                                          && (criteria.FacultyId == 0
                                                                              || x.Student.AcademicInformation.FacultyId == criteria.FacultyId)
                                                                          && (criteria.DepartmentId == 0
                                                                              || x.Student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                                          && (string.IsNullOrEmpty(criteria.Channel)
                                                                              || x.Channel == criteria.Channel)
                                                                          && (string.IsNullOrEmpty(criteria.Code)
                                                                              || x.Student.Code == criteria.Code)
                                                                          && (string.IsNullOrEmpty(criteria.Status)
                                                                              || x.Status == criteria.Status))
                                                              .GroupBy(x => x.Petition)
                                                              .Select(x => new PetitionReportDetail
                                                                           {
                                                                               PetitionName = x.FirstOrDefault().Petition.NameEn,
                                                                               PetitionId = x.FirstOrDefault().PetitionId,
                                                                               Request = x.Where(y => y.Status == "r")
                                                                                          .Count(),
                                                                               Accept = x.Where(y => y.Status == "a")
                                                                                         .Count(),
                                                                               Reject = x.Where(y => y.Status == "j")
                                                                                         .Count(),
                                                                               Total = x.Count()
                                                                           })
                                                              .ToList();
            return View(model);
        }

        private void CreateSelectList(long academicLevelId, long facultyId)
        {
            ViewBag.Petitions = _selectListProvider.GetPetitions();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            ViewBag.Channels = _selectListProvider.GetPetitionChannels();
            ViewBag.Statuses = _selectListProvider.GetPetitionStatuses();
        }
    }
}