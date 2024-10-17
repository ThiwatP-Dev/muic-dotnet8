namespace KeystoneLibrary.Models
{
    public class InstructorInfoViewModel
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public long TitleId { get; set; }
        public string TitleEn { get; set; }
        public string TitleTh { get; set; }
        public string FirstNameEn { get; set; }
        public string LastNameEn { get; set; }
        public string FirstNameTh { get; set; }
        public string LastNameTh { get; set; }
        public int Gender { get; set; }
        public long NationalityId { get; set; }
        public long? RaceId { get; set; }
        public long? ReligionId { get; set; }
        public string Address { get; set; }
        public long CountryId { get; set; }
        public long? ProvinceId { get; set; }
        public long? DistrictId { get; set; }
        public long? SubdistrictId { get; set; }
        public long? StateId { get; set; }
        public long? CityId { get; set; }
        public string ZipCode { get; set; }
        public string TelephoneNumber1 { get; set; }
        public string TelephoneNumber2 { get; set; }
        public string Email { get; set; }
        public string PersonalEmail { get; set; }
        public long TypeId { get; set; }
        public string Type { get; set; }
        public string OfficeRoom { get; set; }
        public string AdminPosition { get; set; }
        public string AcademicPosition { get; set; }
        public long RankId { get; set; }
        public long? AcademicLevelId { get; set; }
        public long? FacultyId { get; set; }
        public string FacultyCode { get; set; }
        public string Faculty { get; set; }
        public long? DepartmentId { get; set; }
        public string DepartmentCode { get; set; }
        public string Department { get; set; }
        public string Remark { get; set; }
        public bool IsActive { get; set; } = true;

        public string FullNameEn => $"{ TitleEn } { FirstNameEn } { LastNameEn }";
        public string FullNameTh => $"{ TitleTh }{ FirstNameTh } { LastNameTh }";

        public string ProfileImageURL { get; set; }
    }
}