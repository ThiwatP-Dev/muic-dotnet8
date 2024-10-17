using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Interfaces
{
    public interface IAddressProvider
    {
        List<Province> GetProvinceByCountryId(long id);
        List<District> GetDistrictByProvinceId(long id);
        List<Subdistrict> GetSubdistrictByDistrictId(long id);
        List<State> GetStatesByCountryId(long id);
    }
}