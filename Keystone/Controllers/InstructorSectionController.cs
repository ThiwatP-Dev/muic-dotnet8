using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class InstructorSectionController : BaseController
    {
        public InstructorSectionController(ApplicationDbContext db,
                                           IFlashMessage flashMessage,
                                           ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public ActionResult Edit(long Id, Criteria criteria)
        {
            ViewBag.Criteria = criteria;
            var instructors = _db.InstructorSections.Include(x => x.Instructor)
                                                    .Include(x => x.SectionDetail)
                                                        .ThenInclude(x => x.Section)
                                                        .ThenInclude(x => x.Course)
                                                    .Where(x => x.SectionDetail.SectionId == Id)
                                                    .OrderBy(x => x.Instructor.Code)
                                                    .ThenBy(x => x.Instructor.FullNameEn)
                                                    .ToList();
            
            return View("~/Views/CourseToBeOffered/InstructorSection/Edit.cshtml", instructors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(List<InstructorSection> model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    foreach (var item in model)
                    {
                        _db.Entry(item).State = EntityState.Modified;
                    }

                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(CourseToBeOfferedController.Index), "CourseToBeOffered");
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View("~/Views/CourseToBeOffered/InstructorSection/Edit.cshtml", model);
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return View("~/Views/CourseToBeOffered/InstructorSection/Edit.cshtml", model);
        }
    }
}