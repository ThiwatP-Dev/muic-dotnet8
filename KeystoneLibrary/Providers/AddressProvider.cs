using KeystoneLibrary.Data;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Interfaces;

namespace KeystoneLibrary.Providers
{
    public class AddressProvider : IAddressProvider
    {
        protected readonly ApplicationDbContext _db;
        
        public AddressProvider(ApplicationDbContext db) 
        {
            _db = db;
        }

        public List<Province> GetProvinceByCountryId(long id)
        {
            var provinces = _db.Provinces.Where(x => x.CountryId == id)
                                         .ToList();
            return provinces;
        }

        public List<District> GetDistrictByProvinceId(long id)
        {
            var districts = _db.Districts.Where(x => x.ProvinceId == id)
                                         .ToList();
            return districts;
        }

        public List<Subdistrict> GetSubdistrictByDistrictId(long id)
        {
            var subdistricts = _db.Subdistricts.Where(x => x.DistrictId == id)
                                               .ToList();
            return subdistricts;
        }

        public List<State> GetStatesByCountryId(long id)
        {
            var states = _db.States.Where(x => x.CountryId == id)
                                   .ToList();
            return states;
        }
    }
}