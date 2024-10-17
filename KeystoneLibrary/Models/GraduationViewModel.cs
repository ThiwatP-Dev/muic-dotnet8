namespace KeystoneLibrary.Models
{
    public class GraduationViewModel
    {
        public Guid StudentId { get; set; }
        public string StudentCode { get; set; }
        public string Title { get; set; }
        public string TitleTh { get; set; }
        public string StudentFullName { get; set; }
        public string Th { get; set; }
        public string StudentFullNameTh { get; set; }
        public string FacultyName { get; set; }
        public string FacultyNameTh { get; set; }
        public string DepartmentNameTh { get; set; }
        public string StudentStatus { get; set; }
        public string DepartmentName { get; set; }
        public int Credit { get; set; }
        public int CreditRegistration { get; set; }
        public string GraduatingDateText { get; set; }
        public string CreditText => Credit.ToString(StringFormat.NumberString);
        public string CreditRegisText => CreditRegistration.ToString(StringFormat.NumberString);
        public decimal GPA { get; set; }
        public string GPAText => GPA.ToString(StringFormat.TwoDecimal);
        public string Remark { get; set; }
    }
}