using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{

    public class MatchStudentFeeGroupController : BaseController 
    {
        public MatchStudentFeeGroupController(ApplicationDbContext db,
                                              IFlashMessage flashMessage,
                                              ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index()
        {
            return View();
        }
    }
}