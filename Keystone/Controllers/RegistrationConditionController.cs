using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("RegistrationCondition", "")]
    public class RegistrationConditionController : BaseController
    {
        public RegistrationConditionController(ApplicationDbContext db,
                                               IFlashMessage flashMessage,
                                               ISelectListProvider selectListProvider,
                                               IMapper mapper) : base(db, flashMessage, mapper, selectListProvider) { }
        
        public ActionResult Index(Criteria criteria, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);

            var result = new RegistrationConditionViewModel
                         {
                             Criteria = criteria
                         };

            result.RegistrationConditions = _db.RegistrationConditions.Include(x => x.AcademicLevel)
                                                                      .Include(x => x.AcademicProgram)
                                                                      .Include(x => x.Faculty)
                                                                      .Include(x => x.Department)
                                                                      .Where(x => (x.Name.Contains(criteria.Name) 
                                                                                        || string.IsNullOrEmpty(criteria.Name))
                                                                                        && (x.AcademicProgramId == criteria.AcademicProgramId 
                                                                                           || criteria.AcademicProgramId == 0)
                                                                                        && (x.FacultyId == criteria.FacultyId 
                                                                                           || criteria.FacultyId == 0)
                                                                                        && (x.DepartmentId == criteria.DepartmentId 
                                                                                           || criteria.DepartmentId == 0))
                                                                      .ToList();
            return View(result);
        }

        [PermissionAuthorize("RegistrationCondition", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();

            return View(new RegistrationCondition());
        }

        [PermissionAuthorize("RegistrationCondition", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RegistrationCondition model)
        {
            if(string.IsNullOrEmpty(model.Name))
            {
                CreateSelectList();
                _flashMessage.Warning(Message.RequiredData);

                return View(model);
            }

            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    model.IsAthlete = Convert.ToBoolean(model.IsAthleteText);
                    model.IsGraduating = Convert.ToBoolean(model.IsGraduatingText);
                    _db.RegistrationConditions.Add(model);
                    _db.SaveChanges();
                    transaction.Commit();


                    _flashMessage.Confirmation(Message.SaveSucceed);

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    transaction.Rollback();
                    CreateSelectList();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View (model);
                }
            }
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            var model = Find(id);

            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0);

            return View(model);
        }

        [PermissionAuthorize("RegistrationCondition", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RegistrationCondition model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            
            if(string.IsNullOrEmpty(model.Name))
            {
                CreateSelectList();
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }


            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var condition = _db.RegistrationConditions.SingleOrDefault(x => x.Id == model.Id);
                    condition.IsActive = model.IsActive;
                    condition.Name = model.Name;
                    condition.Description = model.Description;
                    condition.AcademicLevelId = model.AcademicLevelId;
                    condition.AcademicProgramId = model.AcademicProgramId;
                    condition.FacultyId = model.FacultyId;
                    condition.DepartmentId = model.DepartmentId;
                    condition.BatchCodeEnd = model.BatchCodeEnd;
                    condition.BatchCodeStart = model.BatchCodeStart;
                    condition.LastDigitEnd = model.LastDigitEnd;
                    condition.LastDigitStart = model.LastDigitStart;
                    condition.IsAthlete = model.IsAthlete;
                    condition.IsGraduating = model.IsGraduating;
                    condition.StudentCodes = model.StudentCodes;
                    condition.StudentCodeStart = model.StudentCodeStart;
                    condition.StudentCodeEnd = model.StudentCodeEnd;
                    
                    _db.SaveChanges();
                    transaction.Commit();
                    
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    CreateSelectList();

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToEdit);
                    CreateSelectList();
                    
                    return View(model);
                }
            }            
        }

        [PermissionAuthorize("RegistrationCondition", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = _db.RegistrationConditions.SingleOrDefault(x => x.Id == id);

            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.RegistrationConditions.Remove(model);
                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToDelete);
                    transaction.Rollback();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public PartialViewResult GetRegistrationConditionDetail(long id)
        {
            CreateSelectList();
            var model = Find(id);
            
            return PartialView("~/Views/RegistrationCondition/_Detail.cshtml", model);
        }

        private RegistrationCondition Find(long id)
        {
            var model = _db.RegistrationConditions.Include(x => x.AcademicProgram)
                                                  .Include(x => x.Faculty)
                                                  .Include(x => x.Department)
                                                  .Where(x => x.Id == id)
                                                  .SingleOrDefault();
            return model;
        }

        private void CreateSelectList(long academicLevelId = 0 ,long facultyId = 0) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.AcademicPrograms = _selectListProvider.GetAcademicPrograms();
            ViewBag.Departments = _selectListProvider.GetDepartments();
            ViewBag.Faculties = _selectListProvider.GetFaculties();

            if (academicLevelId != 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.AcademicPrograms = _selectListProvider.GetAcademicProgramsByAcademicLevelId(academicLevelId);

                if (facultyId != 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }
        }
    }
}