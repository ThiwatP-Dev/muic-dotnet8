namespace KeystoneLibrary.Models.Report
{
    public class StudentExpectedGraduateReportViewModel
    {
        public Criteria Criteria { get; set; }
        public List<StudentExpectedGraduateReport> Results { get; set; }
    }

    public class StudentExpectedGraduateReport
    {
        public Guid StudentId { get; set; }
        public string Code { get; set; }
        public string TitleEn { get; set; }
        public string TitleTh { get; set; }
        public string FirstNameEn { get; set; }
        public string FirstNameTh { get; set; }
        public string LastNameEn { get; set; }
        public string LastNameTh { get; set; }
        public string MidNameEn { get; set; }
        public string MidNameTh { get; set; }
        public string FacultyName { get; set; }
        public string DepartmentName { get; set; }
        public string FacultyNameTh { get; set; }
        public string DepartmentNameTh { get; set; }
        public string CurriculumVersionName { get; set; }
        public int Credit { get; set; }
        public string CreditText => Credit.ToString(StringFormat.NumberString);
        public decimal GPA { get; set; }
        public string GPAText => GPA.ToString(StringFormat.TwoDecimal);
        public string StudentStatusText { get; set; }
        public bool IsRequested { get; set; }
        public DateTime? RequestedAt { get; set; }
        public string RequestedAtText => RequestedAt?.AddHours(7).ToString(StringFormat.ShortDate);
    }
}