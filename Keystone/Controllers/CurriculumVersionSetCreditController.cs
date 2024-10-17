using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Curriculums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("CurriculumVersionSetCredit", "")]
    public class CurriculumVersionSetCreditController : BaseController
    {
        protected readonly ICurriculumProvider _curriculumProvider;

        public CurriculumVersionSetCreditController(ApplicationDbContext db,
                                                    IFlashMessage flashMessage,
                                                    ISelectListProvider selectListProvider,
                                                    ICurriculumProvider curriculumProvider) : base(db, flashMessage, selectListProvider)
        {
            _curriculumProvider = curriculumProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View("~/Views/Curriculum/SetCredit/Index.cshtml");
            }
            
            var curriculumVersions = _db.CurriculumVersions.Include(x => x.Curriculum)
                                                                .ThenInclude(x => x.Faculty)
                                                            .Include(x => x.Curriculum)
                                                                .ThenInclude(x => x.Department)
                                                            .Include(x => x.Curriculum)
                                                                .ThenInclude(x => x.AcademicLevel)
                                                            .Where(x => x.Curriculum.AcademicLevelId == criteria.AcademicLevelId
                                                                        && (criteria.FacultyId == 0
                                                                            || x.Curriculum.FacultyId == criteria.FacultyId)
                                                                        && (criteria.DepartmentId == 0
                                                                            || x.Curriculum.DepartmentId == criteria.DepartmentId)
                                                                        && (criteria.CurriculumId == 0
                                                                            || x.CurriculumId == criteria.CurriculumId)
                                                                        && (criteria.CurriculumVersionId == 0
                                                                            || x.Id == criteria.CurriculumVersionId))
                                                            .Select(x => x)
                                                            .OrderBy(x => x.NameEn)
                                                                .ThenBy(x => x.Curriculum.NameEn)
                                                            .GetPaged(criteria, page);

            return View("~/Views/Curriculum/SetCredit/Index.cshtml", curriculumVersions);
        }

        [PermissionAuthorize("CurriculumVersionSetCredit", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(PagedResult<CurriculumVersion> model, int page, Criteria criteria)
        {
            try
            {
                _curriculumProvider.SaveCurriculumVersionExpectCredit(model.Results.ToList());
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
            }
            
            return RedirectToAction("Index", "CurriculumVersionSetCredit", criteria);
        }
        
        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long curriculumId = 0) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();

            if (academicLevelId != 0)
            {
                ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                if (curriculumId != 0)
                {
                    ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
                }
            }
            
            if (facultyId != 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            }
        }
    }
}