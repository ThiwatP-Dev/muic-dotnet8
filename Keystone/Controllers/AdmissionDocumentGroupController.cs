using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Admission;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class AdmissionDocumentGroupController : BaseController
    {
        public AdmissionDocumentGroupController(ApplicationDbContext db, 
                                                IFlashMessage flashMessage, 
                                                ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            var model = _db.AdmissionDocumentGroups.Include(x => x.AcademicLevel)
                                                   .Include(x => x.Faculty)
                                                   .Include(x => x.Department)
                                                   .Include(x => x.GraduatedCountry)
                                                   .Include(x => x.AdmissionType)
                                                   .Where(x => (string.IsNullOrEmpty(criteria.CodeAndName)
                                                                || x.Name.StartsWith(criteria.CodeAndName))
                                                                && (criteria.AcademicLevelId == 0
                                                                    || x.AcademicLevelId == criteria.AcademicLevelId)
                                                                && (criteria.AdmissionTypeId == 0
                                                                    || x.AdmissionTypeId == criteria.AdmissionTypeId)
                                                                && (criteria.FacultyId == 0
                                                                    || x.FacultyId == criteria.FacultyId)
                                                                && (criteria.DepartmentId == 0
                                                                    || x.DepartmentId == criteria.DepartmentId)
                                                                && (criteria.CountryId == 0
                                                                    || x.GraduatedCountryId == criteria.CountryId))
                                                   .IgnoreQueryFilters()
                                                   .GetPaged(criteria, page);
            return View(model);
        }

        public IActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            return View(new AdmissionDocumentGroup());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdmissionDocumentGroup model, string returnUrl)
        {
            try
            {
                _db.AdmissionDocumentGroups.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index), new
                                                       {
                                                           CodeAndName = model.Name
                                                       });
            }
            catch
            {
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.UnableToCreate);
                CreateSelectList();
                return View(model);
            }
        }

        public IActionResult Edit(long id, string returnUrl)
        {
            var model = Find(id);
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.AcademicLevelId, model.FacultyId ?? 0);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AdmissionDocumentGroup model, string returnUrl)
        {
            try
            {
                _db.Entry(model).State = EntityState.Modified;
                _db.Entry(model).Property(x => x.CreatedAt).IsModified = false;
                _db.Entry(model).Property(x => x.CreatedBy).IsModified = false;

                foreach (var item in model.RequiredDocuments)
                {
                    if (item.Id == 0)
                    {
                        _db.RequiredDocuments.Add(item);
                    }
                    else
                    {
                        _db.Entry(item).State = EntityState.Modified;
                    }
                }

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index), new
                                                       {
                                                           CodeAndName = model.Name
                                                       });
            }
            catch
            {
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.UnableToEdit);
                CreateSelectList();
                return View(model);
            }
        }

        public IActionResult Delete(long id)
        {
            var model = Find(id);

            try
            {
                _db.AdmissionDocumentGroups.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private AdmissionDocumentGroup Find(long? id)
        {
            var admissionDocumentGroup = _db.AdmissionDocumentGroups.Include(x => x.AcademicLevel)
                                                                    .Include(x => x.Faculty)
                                                                    .Include(x => x.Department)
                                                                    .Include(x => x.GraduatedCountry)
                                                                    .IgnoreQueryFilters()
                                                                    .SingleOrDefault(x => x.Id == id);

            admissionDocumentGroup.RequiredDocuments = _db.RequiredDocuments.Where(x => x.AdmissionDocumentGroupId == admissionDocumentGroup.Id)
                                                                            .ToList();
            return admissionDocumentGroup;
        }

        public PartialViewResult GetDocumentList(long admissionDocumentGroupId)
        {
            var documents = _db.RequiredDocuments.Include(x => x.Document)
                                                 .Where(x => x.AdmissionDocumentGroupId == admissionDocumentGroupId)
                                                 .ToList();
            return PartialView("~/Views/AdmissionDocumentGroup/_ModalContent.cshtml", documents);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }
            
            if (facultyId != 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            }

            ViewBag.Countries = _selectListProvider.GetCountries();
            ViewBag.Documents = _selectListProvider.GetDocuments();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            ViewBag.AdmissionTypes = _selectListProvider.GetAdmissionTypes();
        }
    }
}