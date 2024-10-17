using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using AutoMapper;
using KeystoneLibrary.Enumeration;
using KeystoneLibrary.Helpers;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("DismissStudent", "")]
    public class DismissStudentController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly ICacheProvider _cacheProvider;

        public DismissStudentController(ApplicationDbContext db,
                                        IFlashMessage flashMessage,
                                        ICacheProvider cacheProvider,
                                        IStudentProvider studentProvider,
                                        ISelectListProvider selectListProvider,
                                        IMapper mapper) : base(db, flashMessage, mapper, selectListProvider) 
        { 
            _cacheProvider = cacheProvider;
            _studentProvider = studentProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);

            if (criteria.AcademicLevelId == 0)
            {
                 _flashMessage.Warning(Message.RequiredData);
                 return View();
            }

            var models = _db.DismissStudents.Include(x => x.Student)
                                                .ThenInclude(x => x.AcademicInformation) 
                                                .ThenInclude(x => x.Department)
                                                .ThenInclude(x => x.Faculty)
                                            .Include(x => x.Term) 
                                            .Include(x => x.Probation)
                                            .Where(x => criteria.AcademicLevelId == x.Student.AcademicInformation.AcademicLevelId
                                                        && (criteria.TermId == 0
                                                            || criteria.TermId == x.TermId)
                                                        && (string.IsNullOrEmpty(criteria.Code)
                                                            ||criteria.Code == x.Student.Code)
                                                        && (string.IsNullOrEmpty(criteria.FirstName)
                                                            || x.Student.FirstNameEn.Contains(criteria.FirstName)
                                                            || x.Student.LastNameEn.Contains(criteria.FirstName)
                                                            || x.Student.FirstNameTh.Contains(criteria.FirstName)
                                                            || x.Student.LastNameTh.Contains(criteria.FirstName))
                                                        && (criteria.FacultyId == 0
                                                            || criteria.FacultyId == x.Student.AcademicInformation.FacultyId)
                                                        && (criteria.DepartmentId == 0
                                                            || criteria.DepartmentId == x.Student.AcademicInformation.DepartmentId)
                                                        && (criteria.ProbationId == 0 
                                                            || criteria.ProbationId == x.ProbationId)
                                                        && (criteria.StartStudentBatch == null
                                                            || criteria.StartStudentBatch == 0
                                                            || criteria.StartStudentBatch <= x.Student.AcademicInformation.Batch)
                                                        && (criteria.EndStudentBatch == null
                                                            || criteria.EndStudentBatch == 0
                                                            || criteria.EndStudentBatch >= x.Student.AcademicInformation.Batch))
                                            .Select(x => _mapper.Map<DismissStudent, DismissStudentViewModel>(x))
                                            .GetPaged(criteria, page, true);
            return View(models);
        }

        [PermissionAuthorize("DismissStudent", PolicyGenerator.Write)]
        public IActionResult Create(string code, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            var dismissStudent = new DismissStudentViewModel();
            if (_studentProvider.IsExistStudent(code, "dm"))
            {
                _flashMessage.Danger(Message.DataAlreadyExist);
            }
            else
            {
                if (!string.IsNullOrEmpty(code) && _studentProvider.IsExistAllStudent(code))
                {
                    var student = _studentProvider.GetStudentInformationByCode(code);
                    var academicLevelId = student.AcademicInformation.AcademicLevelId;
                    var currentTerm = _cacheProvider.GetCurrentTerm(academicLevelId);
                    dismissStudent = _studentProvider.GetDismissTermAndGrade(code);
                    dismissStudent.StudentId = student.Id;
                    dismissStudent.TermId = currentTerm.Id;
                    dismissStudent.Code = student.Code;
                    dismissStudent.FullName = student.FullNameEn;
                    dismissStudent.Faculty = student.AcademicInformation.Faculty.NameEn;
                    dismissStudent.Department = student.AcademicInformation.Department.NameEn;
                    dismissStudent.GPA = student.AcademicInformation.GPA;
                    dismissStudent.CreditEarned = student.AcademicInformation.CreditEarned ?? 0;
                    dismissStudent.Advisor = student.AcademicInformation.Advisor.FullNameEn;
                    dismissStudent.AcademicLevel = student.AcademicInformation.AcademicLevel.NameEn;
                    dismissStudent.Curriculum = student.AcademicInformation.CurriculumVersion.Curriculum.NameEn;
                    dismissStudent.CurriculumVersion = student.AcademicInformation.CurriculumVersion.NameEn;
                    dismissStudent.Term = currentTerm.TermText;
                    
                }
                else
                {
                    _flashMessage.Danger(Message.StudentNotFound);
                }
            }

            return View(dismissStudent);
        }

        [PermissionAuthorize("DismissStudent", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DismissStudentViewModel model)
        {
            if (_studentProvider.IsExistStudent(model.Code, "dm"))
            {
                _flashMessage.Danger(Message.DataAlreadyExist);
                return View(model);
            }

            var student = _studentProvider.GetStudentById(model.StudentId);
            var success = _studentProvider.UpdateDismissStudent(model, student);
            if (!success)
            {
                _flashMessage.Danger(Message.UnableToCreate);
                return View(model);
            }

            _db.SaveChanges();
            _flashMessage.Confirmation(Message.SaveSucceed);
            return RedirectToAction(nameof(Index), new { AcademicLevelId = student.AcademicInformation.AcademicLevelId, Code = model.Code });
        }

        [PermissionAuthorize("DismissStudent", PolicyGenerator.Write)]
        public ActionResult Revert(long id)
        {
            var model = _studentProvider.FindDismissStudent(id);
            var lastStudentStatus = _studentProvider.GetLastStudentStatus(model.StudentId);
            var changeStatus = _studentProvider.GetStudentById(model.StudentId);
            try
            {
                model.IsActive = KeystoneLibrary.Providers.StudentProvider.IsActiveFromStudentStatus(lastStudentStatus);
                changeStatus.StudentStatus = lastStudentStatus;
                var success = _studentProvider.SaveStudentStatusLog(model.StudentId
                                                                    , model.TermId
                                                                    , SaveStatusSouces.DISMISSSTUDENT.GetDisplayName()
                                                                    , "Revert from Dismiss Student"
                                                                    , lastStudentStatus);
                if (success)
                {
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                else
                {
                     _flashMessage.Danger(Message.UnableToSaveStudentLog);
                }
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToRevert);
            }

            return RedirectToAction(nameof(Index), new { AcademicLevelId = changeStatus.AcademicInformation.AcademicLevelId,
                                                         Code = changeStatus.Code });
        }

        public void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Probations = _selectListProvider.GetProbations();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId (academicLevelId);

                if (facultyId > 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }
        }
    }
}