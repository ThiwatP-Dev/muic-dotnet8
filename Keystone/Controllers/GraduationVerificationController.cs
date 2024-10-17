using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class GraduationVerificationController : BaseController
    {
        public GraduationVerificationController(ApplicationDbContext db, 
                                                IFlashMessage flashMessage, 
                                                IMapper mapper, 
                                                ISelectListProvider selectListProvider) : base(db, flashMessage, mapper, selectListProvider) {}

        public IActionResult Index()
        {
            var model = new StudentTransferViewModel();
            model.StudentCourseCategorizations = new List<StudentCourseCategorizationViewModel>();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CourseCategorization(StudentTransferViewModel model)
        {
            return RedirectToAction(nameof(Index));
        }
    }
}