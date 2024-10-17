using AutoMapper;
using KeystoneLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Keystone.Controllers
{
    [AllowAnonymous]
    public class USparkSyncController : BaseController
    {
        public USparkSyncController(ApplicationDbContext db, 
                                          IFlashMessage flashMessage, 
                                          IMapper mapper) : base(db, flashMessage, mapper)
        {
            
        }
        
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GenerateStudentPredefinedCourses()
        {
            _db.Database.ExecuteSql($"GenerateStudentPredefinedCourses");
            return RedirectToAction("Index");
        }
    }
}
