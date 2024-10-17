using Microsoft.AspNetCore.Mvc;

namespace Keystone.Controllers
{
    public class EthicController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}