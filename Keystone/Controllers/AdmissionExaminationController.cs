using AutoMapper;
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
    [PermissionAuthorize("AdmissionExamination", "")]
    public class AdmissionExaminationController : BaseController
    {
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IAdmissionProvider _admissionProvider;

        public AdmissionExaminationController(ApplicationDbContext db, 
                                              IFlashMessage flashMessage,
                                              IMapper mapper,
                                              ISelectListProvider selectListProvider,
                                              IAcademicProvider academicProvider,
                                              IAdmissionProvider admissionProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _academicProvider = academicProvider;
            _admissionProvider = admissionProvider;
        }

        public IActionResult Index( Criteria criteria, int page = 1)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId);
            if (criteria.AcademicLevelId == 0 && criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var models = _db.AdmissionExaminations.Include(x => x.AcademicLevel)
                                                  .Include(x => x.AdmissionRound)
                                                      .ThenInclude(x => x.AdmissionTerm)   
                                                  .Include(x => x.Faculty)
                                                  .Include(x => x.Department)
                                                  .Where(x => x.AcademicLevelId == criteria.AcademicLevelId
                                                              && x.AdmissionRound.AdmissionTermId == criteria.TermId
                                                              && (criteria.AdmissionRoundId == 0 
                                                                  || x.AdmissionRoundId == criteria.AdmissionRoundId)
                                                              && (criteria.FacultyId == 0
                                                                  || x.FacultyId == criteria.FacultyId)
                                                              && (criteria.DepartmentId == 0
                                                                  || x.DepartmentId == criteria.DepartmentId))
                                                  .IgnoreQueryFilters()
                                                  .Select(x => _mapper.Map<AdmissionExamination, AdmissionExaminationViewModel>(x))
                                                  .ToList();
            
            var modelPageResult = models.AsQueryable().GetPaged(criteria, page);
            return View(modelPageResult);
        }

        public PartialViewResult Details(long id)
        {
            var admissionExamination = Find(id);
            var model = _mapper.Map<AdmissionExamination, AdmissionExaminationViewModel>(admissionExamination);
            return PartialView("~/Views/AdmissionExamination/_ModalContent.cshtml", model);
        }

        [PermissionAuthorize("AdmissionExamination", PolicyGenerator.Write)]
        public IActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new AdmissionExaminationViewModel());
        }

        [PermissionAuthorize("AdmissionExamination", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdmissionExaminationViewModel model, string returnUrl)
        {
            var admissionExaminations = new List<AdmissionExamination>();
            var examinationSchedules = new List<AdmissionExaminationSchedule>();
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in model.AdmissionExaminationDetails)
                    {
                        if (_admissionProvider.IsExistAdmissionExamination(model.AdmissionRoundId, item.FacultyId, item.DepartmentIds))
                        {
                            _flashMessage.Danger(Message.FacultyOrDepartmentAlreadyExist);
                            CreateSelectList(model.AcademicLevelId, model.AdmissionTermId);
                            return View(model);
                        }

                        if (item.DepartmentIds != null)
                        {
                            foreach(var departmentId in item.DepartmentIds)
                            {
                                admissionExaminations.Add(SetAdmissionExamination(model, item.FacultyId, departmentId));
                            }
                        }
                        else
                        {
                            admissionExaminations.Add(SetAdmissionExamination(model, item.FacultyId));
                        }
                    }

                    _db.AdmissionExaminations.AddRange(admissionExaminations);
                    _db.SaveChanges();

                    foreach (var examination in admissionExaminations)
                    {
                        foreach (var schedule in model.AdmissionExaminationSchedules)
                        {
                            examinationSchedules.Add(new AdmissionExaminationSchedule
                                                     {
                                                         AdmissionExaminationId = examination.Id,
                                                         AdmissionExaminationTypeId = schedule.AdmissionExaminationTypeId,
                                                         TestedAt = schedule.TestedAt,
                                                         StartTime = schedule.StartTime,
                                                         EndTime = schedule.EndTime,
                                                         RoomId = schedule.RoomId
                                                     });
                        }
                    }
                    
                    _db.AdmissionExaminationSchedules.AddRange(examinationSchedules);
                    _db.SaveChanges();

                    transaction.Commit();
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
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);
                    CreateSelectList(model.AcademicLevelId, model.AdmissionTermId);
                    return View(model);
                }
            }
        }

        public IActionResult Edit(long id, string returnUrl)
        {
            var admissionExamination = Find(id);
            var model = _mapper.Map<AdmissionExamination, AdmissionExaminationViewModel>(admissionExamination);

            CreateSelectList(model.AcademicLevelId, model.AdmissionTermId);
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [PermissionAuthorize("AdmissionExamination", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AdmissionExaminationViewModel model, string returnUrl)
        {
            var modelToUpdate = _db.AdmissionExaminations.Find(model.Id);
            var schedulesToUpdate = _db.AdmissionExaminationSchedules.Where(x => x.AdmissionExaminationId == model.Id)
                                                                     .ToList();

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.AdmissionExaminationSchedules.RemoveRange(schedulesToUpdate);
                    if (model.AdmissionExaminationSchedules.Any(x => x.AdmissionExaminationId == 0))
                    {
                        model.AdmissionExaminationSchedules.Select(x => {
                                                                            x.AdmissionExaminationId = model.Id;
                                                                            return x;
                                                                        })
                                                           .ToList();
                    }

                    _db.AdmissionExaminationSchedules.AddRange(model.AdmissionExaminationSchedules);
                    
                    _mapper.Map<AdmissionExaminationViewModel, AdmissionExamination>(model, modelToUpdate);
                    _db.SaveChanges();

                    transaction.Commit();
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
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToEdit);
                    CreateSelectList(model.AcademicLevelId, model.AdmissionTermId);
                    return View(model);
                }
            }
        }

        [PermissionAuthorize("AdmissionExamination", PolicyGenerator.Write)]
        public IActionResult Delete(long id)
        {
            var model = Find(id);
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.AdmissionExaminationSchedules.RemoveRange(model.AdmissionExaminationSchedules);
                    _db.AdmissionExaminations.Remove(model);

                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToDelete);
                }
            }

            return RedirectToAction(nameof(Index), new
                                                   {
                                                       AcademicLevelId = model.AcademicLevelId,
                                                       TermId = model.AdmissionRound.AdmissionTermId,
                                                       AdmissionRoundId = model.AdmissionRoundId
                                                   });
        }

        public AdmissionExamination Find(long id)
        {
            var model = _db.AdmissionExaminations.Include(x => x.AdmissionRound)
                                                     .ThenInclude(x => x.AdmissionTerm)
                                                 .Include(x => x.Faculty)
                                                 .Include(x => x.Department)
                                                 .Include(x => x.AcademicLevel)
                                                 .IgnoreQueryFilters()
                                                 .SingleOrDefault(x => x.Id == id);

            model.AdmissionExaminationSchedules = _db.AdmissionExaminationSchedules.Include(x => x.Room)
                                                                                   .Include(x => x.AdmissionExaminationType)
                                                                                   .Where(x => x.AdmissionExaminationId == id)
                                                                                   .IgnoreQueryFilters()
                                                                                   .ToList();
            return model;
        }

        private void CreateSelectList(long academicLevelId = 0, long admissionTermId = 0) 
        {
            ViewBag.AdmissionExaminationTypes = _selectListProvider.GetAdmissionExaminationTypes();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Rooms = _selectListProvider.GetRooms();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.Departments = _selectListProvider.GetDepartments();
            }

            if (admissionTermId != 0)
            {
                ViewBag.AdmissionRounds = _selectListProvider.GetAdmissionRoundByTermId(admissionTermId);
            }
        }

        public AdmissionExamination SetAdmissionExamination(AdmissionExaminationViewModel model, long facultyId, long? departmentId = null)
        {
            var admissionExamination = new AdmissionExamination
                                           {
                                               AcademicLevelId = model.AcademicLevelId,
                                               FacultyId = facultyId,
                                               DepartmentId = departmentId,
                                               AdmissionRoundId = model.AdmissionRoundId,
                                               NameEn = model.NameEn,
                                               NameTh = model.NameTh,
                                               Remark = model.Remark
                                           };
            
            return admissionExamination;
        }
    }
}