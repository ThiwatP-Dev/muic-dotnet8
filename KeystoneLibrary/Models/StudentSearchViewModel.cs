namespace KeystoneLibrary.Models
{
    public class StudentSearchViewModel
    {
        public string Code { get; set; }
        public string FullNameEn { get; set; }
        public string FullNameTh { get; set; }
        public string FacultyNameEn { get; set; }
        public string FacultyNameTh { get; set; }
        public long FacultyId { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentNameEn { get; set; }
        public string DepartmentNameTh { get; set; }
        public long DepartmentId { get; set; }
        public string CitizenNumber { get; set; }
        public string Passport { get; set; }
        public string NationalityNameEn { get; set; }
        public string NationalityNameTh { get; set; }
        public string ProfileImageURL { get; set; }
        public int Credit { get; set; }
        public decimal GPA { get; set; }
        public bool IsActive { get; set; }
        public string Advisor { get; set; }
    }
}