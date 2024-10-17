using KeystoneLibrary.Data;
using Microsoft.AspNetCore.Mvc;

namespace Keystone.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(ApplicationDbContext db) : base(db) { }
        
        public IActionResult Index()
        {
            return View();
        }
    }
}
