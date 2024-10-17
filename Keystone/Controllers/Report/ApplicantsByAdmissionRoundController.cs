using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    public class ApplicantsByAdmissionRoundController : BaseController
    {
        protected readonly IReportProvider _reportProvider;

        public ApplicantsByAdmissionRoundController(ApplicationDbContext db,
                                                    IFlashMessage flashMessage,
                                                    IReportProvider reportProvider,
                                                    ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) 
        {
            _reportProvider = reportProvider;
        }
        
        public ActionResult Index(int page, Criteria criteria) 
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.DepartmentId);
            if (criteria.AcademicLevelId == 0) 
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var result = _reportProvider.GetApplicantsByAdmissionRounds(criteria);
            return View(result);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long department = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);

                if (facultyId != 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }
        }
    }
}