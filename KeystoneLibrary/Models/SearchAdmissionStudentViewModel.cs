namespace KeystoneLibrary.Models
{
    public class SearchAdmissionStudentViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string FullNameEn { get; set; }
        public string FullNameTh { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string CitizenNumber { get; set; }
        public string Passport { get; set; }
        public string Nationality { get; set; }
        public string Status { get; set; }
        public string ProfileImageURL { get; set; }
        public bool IsActive { get; set; }
        public string AcademicLevel { get; set; }
        public int Round { get; set; }
        public string Term { get; set; }
        public string Batch { get; set; }
        public string AdmissionStatus { get; set; }
    }
}