namespace KeystoneLibrary.Models
{
    public class InstructorViewModel
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string ProfileImageURL { get; set; }
        public string FullNameEn { get; set; }
        public string FullNameTh { get; set; }
        public string FacultyNameEn { get; set; }
        public string FacultyNameTh { get; set; }
        public string DepartmentNameEn { get; set; }
        public string DepartmentNameTh { get; set; }
        public string DepartmentCode { get; set; }
        public string Email { get; set; }
        public string Type { get; set; }
        public bool IsActive { get; set; }
    }
}