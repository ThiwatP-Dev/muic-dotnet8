using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class CertificateManagementController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;

        public CertificateManagementController(ApplicationDbContext db,
                                               IFlashMessage flashMessage,
                                               ISelectListProvider selectListProvider,
                                               IStudentProvider studentProvider,
                                               IDateTimeProvider dateTimeProvider) : base(db, flashMessage, selectListProvider) 
        { 
            _studentProvider = studentProvider;
            _dateTimeProvider = dateTimeProvider;
        }

        public ActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (string.IsNullOrEmpty(criteria.CertificationType)
                && string.IsNullOrEmpty(criteria.Code)
                && string.IsNullOrEmpty(criteria.CodeAndName)
                && criteria.StartedAt == null
                && criteria.EndedAt == null
                && criteria.AcademicLevelId == 0
                && criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            DateTime? start = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? end = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);
            var certificate = _db.StudentCertificates.Include(x => x.Term)
                                                     .Include(x => x.Title)
                                                     .Include(x => x.Student)
                                                         .ThenInclude(x => x.AcademicInformation)
                                                         .ThenInclude(x => x.Faculty)
                                                     .Include(x => x.Student)
                                                         .ThenInclude(x => x.AcademicInformation)
                                                         .ThenInclude(x => x.Department)
                                                     .Where(x => (string.IsNullOrEmpty(criteria.CertificationType)
                                                                  ||x.Certificate == criteria.CertificationType)
                                                                 && (criteria.AcademicLevelId == 0
                                                                     || x.Term.AcademicLevelId == criteria.AcademicLevelId)
                                                                 && (criteria.TermId == 0
                                                                     || x.TermId == criteria.TermId)
                                                                 && (string.IsNullOrEmpty(criteria.Code)
                                                                     || x.StudentCode.StartsWith(criteria.Code))
                                                                 && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                                     || x.Student.FirstNameEn.StartsWith(criteria.CodeAndName)
                                                                     || x.Student.FirstNameTh.StartsWith(criteria.CodeAndName)
                                                                     || x.Student.MidNameEn.StartsWith(criteria.CodeAndName)
                                                                     || x.Student.MidNameTh.StartsWith(criteria.CodeAndName)
                                                                     || x.Student.LastNameEn.StartsWith(criteria.CodeAndName)
                                                                     || x.Student.LastNameTh.StartsWith(criteria.CodeAndName))
                                                                 && (string.IsNullOrEmpty(criteria.StartedAt)
                                                                     || x.CreatedAt >= start.Value.Date)
                                                                 && (string.IsNullOrEmpty(criteria.EndedAt)
                                                                     || x.CreatedAt <= end.Value.Date))
                                                     .IgnoreQueryFilters()
                                                     .GetPaged(criteria, page, true);
            return View(certificate);
        }

        public ActionResult Edit(long id)
        {
            CreateSelectList();
            var model = _studentProvider.GetStudentCertificate(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = _studentProvider.GetStudentCertificate(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<StudentCertificate>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new Criteria { CertificationType = model.Certificate });
                }         
                catch
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                    CreateSelectList();
                    return RedirectToAction(nameof(Index), new Criteria { CertificationType = model.Certificate });
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            CreateSelectList();
            return RedirectToAction(nameof(Index), new Criteria { CertificationType = model.Certificate });
        }
        
        public ActionResult Details(long id)
        {
            var model = _studentProvider.GetStudentCertificate(id);
            return PartialView("_Details", model);
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.CertificateTypes = _selectListProvider.GetCertificateTypes();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Statuses = _selectListProvider.GetPetitionStatuses();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}