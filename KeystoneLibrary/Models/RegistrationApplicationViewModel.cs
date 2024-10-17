using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Admission;

namespace KeystoneLibrary.Models
{
    public class RegistrationApplicationViewModel
    {
        public string ApplicationNumber { get; set; }
        public int ApplicationStatus { get; set; }
        public bool IsLogin { get; set; }

        // Persnonal Information
        public Guid StudentId { get; set; }
        public long TitleId { get; set; }
        public string FirstNameEn { get; set; }
        public string MidNameEn { get; set; }
        public string LastNameEn { get; set; }
        public string FirstNameTh { get; set; }
        public string MidNameTh { get; set; }
        public string LastNameTh { get; set; }
        public int Gender { get; set; }
        public long NationalityId { get; set; }
        public long RaceId { get; set; }
        public string CitizenNumber { get; set; } = "";
        public string Passport { get; set; } = "";

        public DateTime? BirthDate { get; set; }
        public long ReligionId { get; set; }
        public long BirthCountryId { get; set; }
        public long? BirthProvinceId { get; set; }
        public long? BirthDistrictId { get; set; }
        public long? BirthSubdistrictId { get; set; }
        public long? BirthStateId { get; set; }
        public long? BirthCityId { get; set; }
        public string PersonalEmail { get; set; }
        public string TelephoneNumber1 { get; set; }

        // Educational Information
        public long AcademicLevelId { get; set; }
        public long FacultyId { get; set; }
        public long DepartmentId { get; set; }
        public long QualificationId { get; set; }
        public long SchoolInstituitionId { get; set; }
        public long SchoolInstituitionCountryId { get; set; }
        public bool IsSchoolNotListed { get; set; }
        public string Duration { get; set; }
        public int? GraduatedYear { get; set; }
        public decimal? GPA { get; set; }

        // Address Information
        public bool IsSameAddress { get; set; }
        public string AddressEn { get; set; }
        public string AddressTh { get; set; }
        public string HouseNumber { get; set; }
        public string Moo { get; set; }
        public string Soi { get; set; }
        public string Road { get; set; }
        public long CountryId { get; set; }
        public long ProvinceId { get; set; }
        public long DistrictId { get; set; }
        public long SubdistrictId { get; set; }
        public long StateId { get; set; }
        public long CityId { get; set; }
        public string ZipCode { get; set; }
        public string TelephoneNumber { get; set; }
        public string PermanentAddressEn { get; set; }
        public string PermanentAddressTh { get; set; }
        public string PermanentHouseNumber { get; set; }
        public string PermanentMoo { get; set; }
        public string PermanentSoi { get; set; }
        public string PermanentRoad { get; set; }
        public long PermanentCountryId { get; set; }
        public long PermanentProvinceId { get; set; }
        public long PermanentDistrictId { get; set; }
        public long PermanentSubdistrictId { get; set; }
        public long PermanentStateId { get; set; }
        public long PermanentCityId { get; set; }
        public string PermanentZipCode { get; set; }
        public string PermanentTelephoneNumber { get; set; }

        // ParentInformation
        public string FatherFullName { get; set; }
        public long FatherCountryId { get; set; }
        public long? FatherOccupationId { get; set; }
        public string FatherCitizenNumber { get; set; } = "";
        public string FatherPassportNumber { get; set; } = "";
        public string FatherEmail { get; set; }
        public string FatherPhoneNumber { get; set; }
        public string MotherFullName { get; set; }
        public long MotherCountryId { get; set; }
        public long? MotherOccupationId { get; set; }
        public string MotherCitizenNumber { get; set; } = "";
        public string MotherPassportNumber { get; set; } = "";
        public string MotherEmail { get; set; }
        public string MotherPhoneNumber { get; set; }
        public long GuardianRelationshipId { get; set; }
        public string GuardianFullName { get; set; }
        public long GuardianCountryId { get; set; }
        public long? GuardianOccupationId { get; set; }
        public string GuardianCitizenNumber { get; set; } = "";
        public string GuardianPassportNumber { get; set; } = "";
        public string GuardianEmail { get; set; }
        public string GuardianPhoneNumber { get; set; }

        public bool IsTermAndConditionAccepted { get; set; }

        // Examination
        public List<ExemptedAdmissionExamination> ExemptedExaminations { get; set; }
        public List<StudentDocument> StudentDocuments { get; set; }
        public string ProfileImageURL { get; set; }
        public IFormFile UploadFile { get; set; }
        public string UploadFileBase64 { get; set; }
        public string ReturnController { get; set; }
        public string ApplicationNo { get; set; }
    }
}