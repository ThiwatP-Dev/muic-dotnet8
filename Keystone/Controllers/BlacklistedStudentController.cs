using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Enumeration;
using KeystoneLibrary.Helpers;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("BlacklistedStudent", "")]
    public class BlacklistedStudentController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly IAcademicProvider _academicProvider;
        protected readonly ICacheProvider _cacheProvider;
        
        public BlacklistedStudentController(ApplicationDbContext db,
                                            IFlashMessage flashMessage,
                                            ISelectListProvider selectListProvider,
                                            IStudentProvider studentProvider,
                                            IAcademicProvider academicProvider,
                                            IMapper mapper,
                                            ICacheProvider cacheProvider) : base(db, flashMessage ,mapper ,selectListProvider) 
        { 
            _studentProvider = studentProvider;
            _academicProvider = academicProvider;
            _cacheProvider = cacheProvider;
        }   

        public ActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            var students = _db.BlacklistedStudents.Include(x => x.Student)
                                                      .ThenInclude(x => x.AcademicInformation)
                                                          .ThenInclude(x => x.AcademicLevel)
                                                  .Include(x => x.Student)
                                                      .ThenInclude(x => x.AcademicInformation)
                                                          .ThenInclude(x => x.Faculty)
                                                  .Include(x => x.Student)
                                                      .ThenInclude(x => x.AcademicInformation)
                                                          .ThenInclude(x => x.Department)
                                                  .IgnoreQueryFilters()
                                                  .Where(x => (String.IsNullOrEmpty(criteria.FirstName)
                                                               || x.FirstNameEn.StartsWith(criteria.FirstName)
                                                               || x.FirstNameTh.StartsWith(criteria.FirstName))
                                                               && (String.IsNullOrEmpty(criteria.LastName)
                                                                   || x.LastNameEn.StartsWith(criteria.LastName)
                                                                   || x.LastNameTh.StartsWith(criteria.LastName))
                                                               && (string.IsNullOrEmpty(criteria.CitizenAndPassport) 
                                                                   || x.CitizenNumber.StartsWith(criteria.CitizenAndPassport)
                                                                   || x.Passport.StartsWith(criteria.CitizenAndPassport))
                                                               && (criteria.AcademicLevelId == 0
                                                                   || x.Student.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId)
                                                               && (criteria.FacultyId == 0
                                                                   || x.Student.AcademicInformation.FacultyId == criteria.FacultyId)
                                                               && (criteria.DepartmentId == 0
                                                                   || x.Student.AcademicInformation.DepartmentId == criteria.DepartmentId))
                                                  .OrderBy(x => x.FirstNameEn)
                                                      .ThenBy(x => x.LastNameEn)
                                                  .GetPaged(criteria, page, true);

            return View(students);
        }

        [PermissionAuthorize("BlacklistedStudent", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            return View(new BlacklistedStudent());
        }

        [PermissionAuthorize("BlacklistedStudent", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BlacklistedStudent model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    UpdateBlacklistedStudentId(model);
                    _db.BlacklistedStudents.Add(model);

                    if (model.IsActive && model.StudentId != Guid.Empty)
                    {
                        var student = _db.Students.Include(x => x.AcademicInformation)
                                                  .SingleOrDefault(x => x.Code == model.Code);

                        student.StudentStatus = "b";
                        student.IsActive = KeystoneLibrary.Providers.StudentProvider.IsActiveFromStudentStatus(student.StudentStatus);
                        var currentTerm = _cacheProvider.GetCurrentTerm(student.AcademicInformation.AcademicLevelId);
                        var success = _studentProvider.SaveStudentStatusLog(student.Id
                                                                            , currentTerm.Id
                                                                            , SaveStatusSouces.BLACKLIST.GetDisplayName()
                                                                            , $"Change student status to { student.StudentStatusText }"
                                                                            , student.StudentStatus);
                        if (!success)
                        {
                            _flashMessage.Danger(Message.UnableToEdit);
                            return View(model);
                        }
                    }
                    
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new Criteria
                                                           {
                                                               CitizenAndPassport = string.IsNullOrEmpty(model.CitizenNumber) 
                                                                                    ? model.Passport : model.CitizenNumber
                                                           });
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    CreateSelectList();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }
            else 
            {
                ViewBag.ReturnUrl = returnUrl;
                CreateSelectList();
                _flashMessage.Danger(Message.UnableToCreate);
                return View(model);
            }
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = Find(id);
            CreateSelectList();
            return View(model);
        }

        [PermissionAuthorize("BlacklistedStudent", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(BlacklistedStudent model, string returnUrl)
        {
            var updatedModel = Find(model.Id);
            if (ModelState.IsValid && await TryUpdateModelAsync<BlacklistedStudent>(updatedModel))
            {
                try 
                {    
                    UpdateBlacklistedStudentId(updatedModel);

                    if (model.StudentId != Guid.Empty)
                    {
                        var student = _db.Students.Include(x => x.AcademicInformation)
                                                  .SingleOrDefault(x => x.Id == updatedModel.StudentId);

                        var success = false;
                        var currentTerm = _cacheProvider.GetCurrentTerm(student.AcademicInformation.AcademicLevelId);
                        if (updatedModel.IsActive)
                        {
                            student.StudentStatus = "b";
                            student.IsActive = KeystoneLibrary.Providers.StudentProvider.IsActiveFromStudentStatus(student.StudentStatus);
                            updatedModel.StudentId = student.Id;
                            success = _studentProvider.SaveStudentStatusLog(student.Id
                                                                            , currentTerm.Id
                                                                            , SaveStatusSouces.BLACKLIST.GetDisplayName()
                                                                            , $"Change student status to { student.StudentStatusText }"
                                                                            , student.StudentStatus);
                        }
                        else
                        {
                            var latestStudentStatus = _studentProvider.GetLastStudentStatus(student.Id);
                            student.StudentStatus = latestStudentStatus;
                            student.IsActive = KeystoneLibrary.Providers.StudentProvider.IsActiveFromStudentStatus(latestStudentStatus);
                            success = _studentProvider.SaveStudentStatusLog(student.Id
                                                                            , currentTerm.Id
                                                                            , SaveStatusSouces.BLACKLIST.GetDisplayName()
                                                                            , $"Change student status to { student.StudentStatusText }"
                                                                            , student.StudentStatus);
                        }

                        if (!success)
                        {
                            _flashMessage.Danger(Message.UnableToEdit);
                            return View(model);
                        }
                    }

                    await _db.SaveChangesAsync();

                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new Criteria
                                                           {
                                                               CitizenAndPassport = string.IsNullOrEmpty(model.CitizenNumber) 
                                                                                    ? model.Passport : model.CitizenNumber
                                                           });
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    CreateSelectList();
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(model);
                }
            }
            else
            {
                ViewBag.ReturnUrl = returnUrl;
                CreateSelectList();
                _flashMessage.Danger(Message.UnableToEdit);
                return View(model);
            }
        }

        [PermissionAuthorize("BlacklistedStudent", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = Find(id);
            try
            {
                if (model.StudentId != Guid.Empty)
                {
                    var latestStudentStatus = _studentProvider.GetLastStudentStatus(model.StudentId ?? Guid.Empty);
                    var student = _db.Students.Include(x => x.AcademicInformation)
                                              .SingleOrDefault(x => x.Id == model.StudentId);
                    student.StudentStatus = latestStudentStatus;
                    student.IsActive = KeystoneLibrary.Providers.StudentProvider.IsActiveFromStudentStatus(student.StudentStatus);

                    var currentTerm = _cacheProvider.GetCurrentTerm(student.AcademicInformation.AcademicLevelId);
                    var success = _studentProvider.SaveStudentStatusLog(student.Id
                                                                        , currentTerm.Id
                                                                        , SaveStatusSouces.BLACKLIST.GetDisplayName()
                                                                        , $"Change student status to { student.StudentStatusText }"
                                                                        , student.StudentStatus);

                    if (!success)
                    {
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }

                _db.BlacklistedStudents.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        public PartialViewResult GetStudentInformation(string studentCode)
        {
            CreateSelectList();
            var student = _mapper.Map<Student, BlacklistedStudent>(_studentProvider.GetStudentByCode(studentCode));
            return PartialView("~/Views/BlacklistedStudent/_FormDetail.cshtml", student);
        }

        private BlacklistedStudent Find(long? id)
        {
            var model = _db.BlacklistedStudents.Include(x => x.Student)
                                               .IgnoreQueryFilters()
                                               .SingleOrDefault(x => x.Id == id);
            model.Code = model.Student.Code;

            return model;
        }

        private void UpdateBlacklistedStudentId(BlacklistedStudent model)
        {
            if (!string.IsNullOrEmpty(model.Code) && (_studentProvider.IsExistStudent(model.Code, "s") || _studentProvider.IsExistStudent(model.Code, "b")))
            {
                model.StudentId = _studentProvider.GetStudentByCode(model.Code).Id;
            }
            else 
            {
                model.StudentId = null;
            }
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Titles = _selectListProvider.GetTitlesEn();
            ViewBag.Genders = _selectListProvider.GetGender();
            if (academicLevelId > 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }

            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartments(facultyId);
            }
        }
    }
}