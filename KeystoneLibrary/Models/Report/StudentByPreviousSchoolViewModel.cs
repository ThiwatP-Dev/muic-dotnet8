using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.Report
{
    public class StudentByPreviousSchoolViewModel
    {
        public long Id { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public long CountryId { get; set; }
        public long? StateId { get; set; }
        public long? ProvinceId { get; set; }
        public long? SchoolTypeId { get; set; }
        public long? SchoolTerritoryId { get; set; }
        public string Country { get; set; }
        public string ProvinceOrState { get; set; }
        public string SchoolType { get; set; }
        public string SchoolTerritory { get; set; }
        public int TotalStudent { get; set; }
        public List<AdmissionInformation> AdmissionInformations { get; set; }
    }
}