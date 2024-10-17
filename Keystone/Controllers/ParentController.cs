using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class ParentController : BaseController
   {
        protected readonly IStudentProvider _studentProvider;

        public ParentController(ApplicationDbContext db,
                               IFlashMessage flashMessage,
                               ISelectListProvider selectListProvider, 
                               IMapper mapper,
                               IStudentProvider studentProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _studentProvider = studentProvider;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Details(long id, string page)
        {
            var model = Find(id);
            ViewData["Page"] = page;
            return PartialView("~/Views/Student/Parent/_Details.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ParentInformation model, string page)
        {
            try
            {
                _db.ParentInformations.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCreate);
            }

            if (page == "a") // a = admission student page
            {
                var studentCode = _studentProvider.GetStudentCodeById(model.StudentId);
                return RedirectToAction(nameof(Details), "AdmissionStudent", new { codeOrNumber = studentCode, tabIndex = "5" });
            }
            
            return RedirectToAction(nameof(Details), "Student", new { id = model.StudentId, tabIndex = "8" }); // student profile page
        }
        
        public ActionResult Edit(long id, string page)
        {
            CreateSelectList();
            var model = Find(id);
            ViewData["Page"] = page;
            return PartialView("~/Views/Student/Parent/_Form.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ParentInformation model, string page)
        {
            try
            {
                _db.Entry(model).State = EntityState.Modified;
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
            }
            
            if (page == "a") // a = admission student page
            {
                var studentCode = _studentProvider.GetStudentCodeById(model.StudentId);
                return RedirectToAction(nameof(Details), "AdmissionStudent", new { codeOrNumber = studentCode, tabIndex = "5" });
            }

            return RedirectToAction(nameof(Details), "Student", new { id = model.StudentId, tabIndex = "8" }); // student profile page
        }

        public ActionResult Delete(long id, string page)
        {
            var model = Find(id);
            _db.ParentInformations.Remove(model);
            _db.SaveChanges();
            _flashMessage.Confirmation(Message.SaveSucceed);
            
            if (page == "a") // a = admission student page
            {
                var studentCode = _studentProvider.GetStudentCodeById(model.StudentId);
                return RedirectToAction(nameof(Details), "AdmissionStudent", new { codeOrNumber = studentCode, tabIndex = "5" });
            }

            return RedirectToAction(nameof(Details), "Student", new { id = model.StudentId, tabIndex = "8" }); // student profile page
        }

        private ParentInformation Find(long? id) 
        {
            var parent = _db.ParentInformations.Include(x => x.Country)
                                               .Include(x => x.Province)
                                               .Include(x => x.District)
                                               .Include(x => x.Subdistrict)
                                               .Include(x => x.Relationship)
                                               .SingleOrDefault(x => x.Id == id);
            return parent;
        }

        private void CreateSelectList() 
        {
            ViewBag.Countries = _selectListProvider.GetCountries();
            ViewBag.Provinces = _selectListProvider.GetProvinces();
            ViewBag.Districts = _selectListProvider.GetDistricts();
            ViewBag.Subdistricts = _selectListProvider.GetSubdistricts();
            ViewBag.Cities = _selectListProvider.GetCities();
            ViewBag.States = _selectListProvider.GetStates();
            ViewBag.Relationships = _selectListProvider.GetRelationships();
        }
   }
}