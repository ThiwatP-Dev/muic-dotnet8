using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Keystone.Controllers
{
    public class TeachingLoadController : BaseController
    {
        public TeachingLoadController(ApplicationDbContext db,
                                      ISelectListProvider selectListProvider) : base(db, selectListProvider) { }

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            CreateSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(List<TeachingLoad> model)
        {
            return View();
        }

        private void CreateSelectList() 
        {
            ViewBag.Faculties = _selectListProvider.GetFaculties();
            ViewBag.TeachingTypes = _selectListProvider.GetTeachingTypes();
        }

        private List<InstructorSection> GetInstructorSections(long id) 
        {
            var sections = _db.InstructorSections.Include(x => x.SectionDetail)
                                                     .ThenInclude(x => x.Section)
                                                     .ThenInclude(x => x.Course)
                                                 .Where(x => x.InstructorId == id)
                                                 .ToList();
            return sections;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetInstructorCourse(long id) 
        {
            var courses = GetInstructorSections(id).Select(x => x.SectionDetail.Section.CourseId).Distinct();
            var selectItem = _db.Courses.Where(x => courses.Contains(x.Id))
                                        .Select(x => new SelectListItem
                                                     {
                                                         Text = x.CodeAndName,
                                                         Value = x.Id.ToString()
                                                     })
                                        .OrderBy(x => x.Text);
            return Json(new SelectList(selectItem, "Value", "Text"));
        }

        private int GetTotalSection(long id) 
        {
            var sections = GetInstructorSections(id);
            return sections.Select(x => x.SectionDetail.Section).Count();
        }
    }
}