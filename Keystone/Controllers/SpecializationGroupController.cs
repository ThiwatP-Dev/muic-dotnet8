using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Curriculums;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class SpecializationGroupController : BaseController
    {
        protected readonly ICurriculumProvider _curriculumProvider;

        public SpecializationGroupController(ApplicationDbContext db,
                                             IFlashMessage flashMessage,
                                             ISelectListProvider selectListProvider,
                                             ICurriculumProvider curriculumProvider) : base(db, flashMessage, selectListProvider) 
        {
            _curriculumProvider = curriculumProvider;
        }

        public IActionResult Details(long? minorId, long? concentrationId, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = new CurriculumVersionViewModel();
            model.MinorId = minorId ?? 0;
            model.ConcentrationId = concentrationId ?? 0;
            model.CurriculumVersion = new CurriculumVersion();
            model.CurriculumVersion.CourseGroups = _curriculumProvider.GetCourseGroupsBySpecializationGroupId(minorId ?? concentrationId ?? 0);

            return View(model);
        }
    }
}