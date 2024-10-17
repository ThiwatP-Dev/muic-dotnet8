using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class AddressController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;

        public AddressController(ApplicationDbContext db,
                                 IFlashMessage flashMessage,
                                 ISelectListProvider selectListProvider,
                                 IStudentProvider studentProvider) : base(db, flashMessage, selectListProvider)
        {
            _studentProvider = studentProvider;
        }

        public ActionResult Details(long id)
        {
            var model = Find(id);
            return PartialView("~/Views/Student/_AddressInfo.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentAddress model, string page)
        {
            try
            {
                _db.StudentAddresses.Add(model);
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
                return RedirectToAction(nameof(Details), "AdmissionStudent", new { codeOrNumber = studentCode, tabIndex = "4" });
            }

            return RedirectToAction(nameof(Details), "Student", new { id = model.StudentId, tabIndex = "6" }); // student profile page
        }

        public ActionResult Edit(long id)
        {
            var model = Find(id);
            CreateSelectList(model.CountryId, model.ProvinceId ?? 0, model.DistrictId ?? 0, model.StateId ?? 0);
            return PartialView("~/Views/Student/Address/_Form.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StudentAddress model, string page)
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
                return RedirectToAction(nameof(Details), "AdmissionStudent", new { codeOrNumber = studentCode, tabIndex = "4" });
            }

            return RedirectToAction(nameof(Details), "Student", new { id = model.StudentId, tabIndex = "6" }); // student profile page
        }

        public ActionResult Delete(long id, string page)
        {
            var model = Find(id);
            _db.StudentAddresses.Remove(model);
            _db.SaveChanges();
            _flashMessage.Confirmation(Message.SaveSucceed);

            if (page == "a") // a = admission student page
            {
                var studentCode = _studentProvider.GetStudentCodeById(model.StudentId);
                return RedirectToAction(nameof(Details), "AdmissionStudent", new { codeOrNumber = studentCode, tabIndex = "4" });
            }

            return RedirectToAction(nameof(Details), "Student", new { id = model.StudentId, tabIndex = "6" }); // student profile page
        }

        private StudentAddress Find(long? Id)
        {
            var address = _db.StudentAddresses.Include(x => x.Country)
                                              .Include(x => x.Province)
                                              .Include(x => x.District)
                                              .Include(x => x.Subdistrict)
                                              .Include(x => x.City)
                                              .Include(x => x.State)
                                              .SingleOrDefault(x => x.Id == Id);
            return address;
        }

        private void CreateSelectList(long countryId, long provinceId = 0, long districtId = 0, long stateId = 0) 
        {
            ViewBag.Countries = _selectListProvider.GetCountries();
            if (countryId != 0)
            {
                ViewBag.Provinces = _selectListProvider.GetProvinces(countryId);
                ViewBag.States = _selectListProvider.GetStates(countryId);
                if (stateId != 0)
                {
                    ViewBag.Cities = _selectListProvider.GetCities(countryId, stateId);
                }
                else
                {
                    ViewBag.Cities = _selectListProvider.GetCities(countryId);
                }
            }
            
            if (provinceId != 0)
            {
                ViewBag.Districts =  _selectListProvider.GetDistricts(provinceId);
            }

            if (districtId != 0)
            {
                ViewBag.Subdistricts = _selectListProvider.GetSubdistricts(districtId);
            }
        }
    }
}