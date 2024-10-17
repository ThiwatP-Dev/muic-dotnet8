using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Admission;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("AdmissionCurriculum", "")]
    public class AdmissionCurriculumController : BaseController
    {
        protected readonly IAdmissionProvider _admissionProvider;
        public AdmissionCurriculumController(ApplicationDbContext db,
                                             ISelectListProvider selectListProvider,
                                             IFlashMessage flashMessage,
                                             IAdmissionProvider admissionProvider) : base(db, flashMessage, selectListProvider)
        {
            _admissionProvider = admissionProvider;
        }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0 && criteria.TermId == 0 && criteria.AdmissionRoundId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var models = _db.AdmissionCurriculums.Include(x => x.AcademicLevel)
                                                 .Include(x => x.Faculty)
                                                 .Include(x => x.AdmissionRound)
                                                     .ThenInclude(x => x.AdmissionTerm)
                                                 .Include(x => x.CurriculumVersion)
                                                 .Where(x => (criteria.AcademicLevelId == 0
                                                              || x.AcademicLevelId == criteria.AcademicLevelId)
                                                              && (criteria.AdmissionRoundId == 0
                                                                  || x.AdmissionRoundId == criteria.AdmissionRoundId)
                                                              && (criteria.FacultyId == 0
                                                                  || x.FacultyId == criteria.FacultyId)
                                                              && (criteria.DepartmentId == 0
                                                                  || x.DepartmentId == criteria.DepartmentId))
                                                 .GroupBy(x => new 
                                                               {
                                                                   x.AdmissionRoundId,
                                                                   x.FacultyId
                                                               })
                                                 .Select(x => new AdmissionCurriculumViewModel
                                                              {
                                                                  AdmissionRoundId = x.Key.AdmissionRoundId,
                                                                  AcademicLevelId = x.FirstOrDefault().AcademicLevelId,
                                                                  Term = x.FirstOrDefault().AdmissionRound.AdmissionTerm.TermText,
                                                                  Round = x.FirstOrDefault().AdmissionRound.Round,
                                                                  AcademicLevel = x.FirstOrDefault().AcademicLevel.NameEn,
                                                                  Faculty = x.FirstOrDefault().Faculty.CodeAndShortName,
                                                                  FacultyId = x.FirstOrDefault().FacultyId,
                                                                  CurriculumVersions = x.Select(y => y.CurriculumVersion)
                                                                                        .ToList()
                                                              })
                                                 .ToList();

            var modelPageResult = models.AsQueryable()
                                        .GetPaged(criteria, page);
            return View(modelPageResult);
        }

        [PermissionAuthorize("AdmissionCurriculum", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new AdmissionCurriculum());
        }

        [PermissionAuthorize("AdmissionCurriculum", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdmissionCurriculum model, List<long> curriculumVersionIds, string returnUrl)
        {
            if (curriculumVersionIds == null)
            {
                ViewBag.ReturnUrl = returnUrl;
                CreateSelectList(model.AcademicLevelId, model.AdmissionTermId, model.FacultyId);
                _flashMessage.Danger(Message.RequiredData);
                model.CurriculumVersions = _admissionProvider.GetCurriculumVersionForAdmissionCurriculum(model.AdmissionTermId, model.AdmissionRoundId, model.FacultyId);
                return View(model);
            }

            if (_admissionProvider.IsExistAdmissionCurriculum(model.AdmissionRoundId, model.FacultyId))
            {
                CreateSelectList(model.AcademicLevelId, model.AdmissionTermId, model.FacultyId);
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }

            var version = _db.CurriculumVersions.Include(x => x.Curriculum)
                                                .Where(x => curriculumVersionIds.Contains(x.Id))
                                                .ToList();

            foreach (var curriculumVersionId in curriculumVersionIds)
            {
                var curriculum = version.SingleOrDefault(x => x.Id == curriculumVersionId);
                _db.AdmissionCurriculums.Add(new AdmissionCurriculum
                                             {
                                                 AdmissionRoundId = model.AdmissionRoundId,
                                                 AcademicLevelId = model.AcademicLevelId,
                                                 FacultyId = model.FacultyId,
                                                 DepartmentId = curriculum.Curriculum?.DepartmentId ?? 0,
                                                 CurriculumId = curriculum.CurriculumId,
                                                 CurriculumVersionId = curriculumVersionId
                                             });
            }
            
            try
            {
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index), new
                                                       {
                                                           AcademicLevelId = model.AcademicLevelId,
                                                           TermId = model.AdmissionTermId,
                                                           AdmissionRoundId = model.AdmissionRoundId
                                                       });
            }
            catch
            {
                ViewBag.ReturnUrl = returnUrl;
                CreateSelectList(model.AcademicLevelId, model.AdmissionTermId, model.FacultyId);
                _flashMessage.Danger(Message.UnableToCreate);
                model.CurriculumVersions = _admissionProvider.GetCurriculumVersionForAdmissionCurriculum(model.AdmissionTermId, model.AdmissionRoundId, model.FacultyId);
                return View(model);
            }
        }

        public ActionResult Edit(long academicLevelId, long admissionRoundId, long facultyId, string returnUrl)
        {
            var admissionTerm = _admissionProvider.GetTermByAdmissionRoundId(admissionRoundId);
            var model = new AdmissionCurriculum
                        {
                            AcademicLevelId = academicLevelId,
                            AdmissionTermId = admissionTerm?.Id ?? 0,
                            AdmissionRoundId = admissionRoundId
                        };

            model.CurriculumVersions = _admissionProvider.GetAdmissionCurriculumByRoundAndFaculty(admissionRoundId, facultyId)
                                                         .Select(x => x.CurriculumVersion)
                                                         .ToList();

            CreateSelectList(model.AcademicLevelId, model.AdmissionTermId, model.FacultyId);
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [PermissionAuthorize("AdmissionCurriculum", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AdmissionCurriculum model, List<long> curriculumVersionIds, string returnUrl)
        {
            var currentCurriculums = _admissionProvider.GetAdmissionCurriculumByRoundAndFaculty(model.AdmissionRoundId, model.FacultyId);
            var removeCurriculums = currentCurriculums.Where(x => !curriculumVersionIds.Contains(x.CurriculumVersionId))
                                                      .ToList();
            if (removeCurriculums.Any())
            {
                _db.AdmissionCurriculums.RemoveRange(removeCurriculums);
            }

            var currentCurriculumIds = currentCurriculums.Select(x => x.CurriculumVersionId)
                                                         .ToList();
            var newCurriculumIds = curriculumVersionIds.Where(x => !currentCurriculumIds.Contains(x))
                                                       .ToList();
            var curriculumVersions = _db.CurriculumVersions.Include(x => x.Curriculum)
                                                           .Where(x => newCurriculumIds.Contains(x.Id))
                                                           .ToList();

            foreach (var item in newCurriculumIds)
            {
                var curriculum = curriculumVersions.SingleOrDefault(x => x.Id == item);
                _db.AdmissionCurriculums.Add(new AdmissionCurriculum
                                             {
                                                 AdmissionRoundId = model.AdmissionRoundId,
                                                 AcademicLevelId = model.AcademicLevelId,
                                                 AdmissionTermId = model.AdmissionTermId,
                                                 FacultyId = model.FacultyId,
                                                 DepartmentId = curriculum.Curriculum?.DepartmentId ?? 0,
                                                 CurriculumId = curriculum.CurriculumId,
                                                 CurriculumVersionId = item
                                             });
            }

            try
            {
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index), new
                                                       {
                                                           AcademicLevelId = model.AcademicLevelId,
                                                           TermId = model.AdmissionTermId,
                                                           AdmissionRoundId = model.AdmissionRoundId
                                                       });
            }
            catch
            {
                ViewBag.ReturnUrl = returnUrl;
                CreateSelectList(model.AcademicLevelId, model.AdmissionTermId, model.FacultyId);
                _flashMessage.Danger(Message.UnableToEdit);
                model.CurriculumVersions = _admissionProvider.GetCurriculumVersionForAdmissionCurriculum(model.AdmissionTermId, model.AdmissionRoundId, model.FacultyId);
                return View(model);
            }
        }

        [PermissionAuthorize("AdmissionCurriculum", PolicyGenerator.Write)]
        public ActionResult Delete(long academicLevelId, long admissionRoundId, long facultyId)
        {
            List<AdmissionCurriculum> models = _admissionProvider.GetAdmissionCurriculumByRoundAndFaculty(admissionRoundId, facultyId);
            try
            {
                _db.AdmissionCurriculums.RemoveRange(models);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private void CreateSelectList(long academicLevelId = 0, long termId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            }

            if (academicLevelId > 0)
            {
                ViewBag.AdmissionTerms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.AdmissionRounds = _selectListProvider.GetAdmissionRounds(academicLevelId, termId);
            }
        }
    }
}