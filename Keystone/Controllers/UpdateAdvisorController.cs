using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("UpdateAdvisor", "")]
    public class UpdateAdvisorController : BaseController
    {
        protected readonly IInstructorProvider _instructorProvider;
        protected readonly IStudentProvider _studentProvider;

        public UpdateAdvisorController(ApplicationDbContext db,
                                    IFlashMessage flashMessage,
                                    ISelectListProvider selectListProvider,
                                    IInstructorProvider instructorProvider,
                                    IStudentProvider studentProvider,
                                    IMapper mapper) : base(db, flashMessage, mapper, selectListProvider)
        {
            _instructorProvider = instructorProvider;
            _studentProvider = studentProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateAssignAdviseeSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.CurriculumId);
            if (criteria.StudentCodeFrom == null && criteria.StudentCodeTo == null && string.IsNullOrEmpty(criteria.FirstName) && string.IsNullOrEmpty(criteria.LastName)
            && criteria.AdmissionTypeId == 0 && criteria.AcademicLevelId == 0 && criteria.FacultyId == 0 && criteria.DepartmentId == 0
            && criteria.CurriculumId == 0 && criteria.CurriculumVersionId == 0 && criteria.Gender == null && criteria.NationalityId == 0
            && criteria.CreditFrom == null && criteria.CreditTo == null && string.IsNullOrEmpty(criteria.StudentStatus) && string.IsNullOrEmpty(criteria.Status))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(new PagedResult<Student>
                {
                    Criteria = criteria,
                    Results = new List<Student>(),
                    CurrentPage = 1,
                    RowCount = 0,
                    PageSize = 0,
                });
            }

            var model = _studentProvider.GetStudentForAssignAdvisee(criteria);
            var pageModel = new PagedResult<Student>
            {
                Criteria = criteria,
                Results = model,
                CurrentPage = 1,
                RowCount = model.Count,
                PageSize = model.Count,
            };

            return View(pageModel);
        }

        private void CreateAssignAdviseeSelectList(long academicLevelId, long facultyId, long curriculumId)
        {
            ViewBag.Statuses = _selectListProvider.GetActiveStatuses();
            ViewBag.Nationalities = _selectListProvider.GetNationalities();
            ViewBag.Genders = _selectListProvider.GetGender();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);
            ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
            ViewBag.AdmissionTypes = _selectListProvider.GetAdmissionTypes();
            ViewBag.Instructors = _selectListProvider.GetInstructors();
        }

        [PermissionAuthorize("UpdateAdvisor", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveAssignAdvisee(Criteria criteria, List<Guid> studentListId, long instructorId)
        {
            string errMsg = null;
            if (studentListId == null || studentListId.Count == 0)
            {
                errMsg += "Student";
            }
            if (instructorId == 0)
            {
                errMsg += (errMsg != null ? ", Advisor" : "Advisor");
            }

            if (string.IsNullOrEmpty(errMsg))
            {
                try
                {
                    _instructorProvider.AssignAdvisee(studentListId, instructorId);
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToDelete);
                }
            }
            else
            {
                _flashMessage.Warning(Message.RequiredData + " " + errMsg);
            }
            return RedirectToAction("Index", criteria);
        }
    }
}
