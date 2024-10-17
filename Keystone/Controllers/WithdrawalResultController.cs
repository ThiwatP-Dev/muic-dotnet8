using Microsoft.AspNetCore.Mvc;

namespace Keystone.Controllers
{
    public class WithdrawalResultController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Edit()
        {
            return View();
        }
    }
}