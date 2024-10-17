using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class StudentByPreviousSchoolController : BaseController
    {
        public StudentByPreviousSchoolController(ApplicationDbContext db,
                                                 ISelectListProvider selectListProvider,
                                                 IFlashMessage flashMessage,
                                                 IMapper mapper) : base(db, flashMessage, mapper, selectListProvider) { }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.CountryId);
            if (string.IsNullOrEmpty(criteria.CodeAndName) && criteria.CountryId == 0 && criteria.ProvinceId == 0 
                && criteria.StateId == 0 && criteria.SchoolTypeId == 0 && criteria.SchoolTerritoryId == 0)
            {
                _flashMessage.Danger(Message.RequiredData);
                return View();
            }

            var schools = _db.PreviousSchools.Include(x => x.State)
                                             .Include(x => x.Province)
                                             .Include(x => x.Country)
                                             .Include(x => x.AdmissionInformations)
                                             .Include(x => x.SchoolType)
                                             .Include(x => x.SchoolTerritory)   
                                             .Where(x => (string.IsNullOrEmpty(criteria.CodeAndName)
                                                          || x.NameEn.Contains(criteria.CodeAndName, StringComparison.OrdinalIgnoreCase)
                                                          || x.NameTh.Contains(criteria.CodeAndName, StringComparison.OrdinalIgnoreCase))
                                                          && (criteria.CountryId == 0
                                                              || criteria.CountryId == x.CountryId)
                                                          && (criteria.ProvinceId == 0
                                                              || criteria.ProvinceId == x.ProvinceId)
                                                          && (criteria.StateId == 0
                                                              || criteria.StateId == x.StateId)
                                                          && (criteria.SchoolTypeId == 0
                                                              || criteria.SchoolTypeId == x.SchoolTypeId)
                                                          && (criteria.SchoolTerritoryId == 0
                                                              || criteria.SchoolTerritoryId == x.SchoolTerritoryId))
                                             .Select(x => _mapper.Map<PreviousSchool, StudentByPreviousSchoolViewModel>(x))
                                             .GetPaged(criteria, page);

            return View(schools);
        }

        public IActionResult Details(long Id)
        {
            var students = _db.PreviousSchools.Include(x => x.State)
                                              .Include(x => x.Province)
                                              .Include(x => x.Country)
                                              .Include(x => x.AdmissionInformations)
                                                  .ThenInclude(x => x.Student)
                                              .Include(x => x.AdmissionInformations)
                                                  .ThenInclude(x => x.AdmissionTerm)
                                              .Include(x => x.AdmissionInformations)
                                                  .ThenInclude(x => x.AdmissionType)
                                              .Where(x => x.Id == Id)
                                              .Select(x => _mapper.Map<PreviousSchool, StudentByPreviousSchoolViewModel>(x))
                                              .SingleOrDefault();

            return View(students);
        }

        public void CreateSelectList(long countryId = 0)
        {
            ViewBag.Countries = _selectListProvider.GetCountries();
            ViewBag.SchoolTypes = _selectListProvider.GetSchoolTypes();
            ViewBag.SchoolTerritories = _selectListProvider.GetSchoolTerritories();
            if (countryId != 0)
            {
                ViewBag.States = _selectListProvider.GetStates(countryId);
                ViewBag.Provinces = _selectListProvider.GetProvinces(countryId);
            }
        }
    }
}