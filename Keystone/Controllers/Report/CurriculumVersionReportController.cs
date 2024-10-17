using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("CurriculumVersionReport", "")]
    public class CurriculumVersionReportController : BaseController
    {
        protected readonly IReportProvider _reportProvider;
        protected readonly ICurriculumProvider _curriculumProvider;

        public CurriculumVersionReportController(ApplicationDbContext db,
                                                 IFlashMessage flashMessage,
                                                 IMapper mapper,
                                                 ISelectListProvider selectListProvider,
                                                 IReportProvider reportProvider,
                                                 ICurriculumProvider curriculumProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _reportProvider = reportProvider;
            _curriculumProvider = curriculumProvider;
        }

        public IActionResult Index(long academicLevelId, long curriculumId, long curriculumVersionId)
        {
            CreateSelectList(academicLevelId, curriculumId);
            if (curriculumVersionId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var model = _reportProvider.GetCurriculumVersionReport(curriculumVersionId);
            return View(model);
        }

        public IActionResult ExportExcel(long curriculumVersionId)
        {
            var model = _reportProvider.GetCurriculumVersionReport(curriculumVersionId);
            return View(model);
        }

        public IActionResult Structure(long academicLevelId, long curriculumId, long curriculumVersionId)
        {
            CreateSelectList(academicLevelId, curriculumId);
            if (curriculumVersionId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            var curriculumVersions = _curriculumProvider.GetCurriculumVersionStructure(curriculumVersionId);
            return View(curriculumVersions);
        }

        public IActionResult StructureByStudent(string studentId)
        {
            ViewBag.Students = _selectListProvider.GetStudents();
            if (string.IsNullOrEmpty(studentId))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var curriculumVersions = _curriculumProvider.GetCurriculumVersionStructureByStudent(studentId);
            return View(curriculumVersions);
        }

        public void CreateSelectList(long academicLevelId = 0, long curriculumId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Curriculums = _selectListProvider.GetCurriculum();
            ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
        }
    }
}