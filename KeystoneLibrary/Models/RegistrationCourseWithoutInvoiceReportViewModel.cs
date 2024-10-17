namespace KeystoneLibrary.Models
{
    public class RegistrationCourseWithoutInvoiceReportViewModel
    {
        public Criteria Criteria { get; set; }
        public List<RegistrationCourseWithoutInvoiceReportDetail> Details { get; set; }
    }

    public class RegistrationCourseWithoutInvoiceReportDetail
    {
        public Guid StudentId { get; set; }
        public long RegistrationCourseId { get; set; }
        public long CourseId { get; set; }
        public string StudentCode { get; set; }
        public string StudentTitle { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentMidName { get; set; }
        public string StudentLastName { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool HaveInvoice { get; set; }
        public string CourseCode { get; set; }
        public decimal Credit { get; set; }
        public decimal Lecture { get; set; }
        public decimal Lab { get; set; }
        public decimal Other { get; set; }
        public string SectionNumber { get; set; }
        public string StudentFullName => string.IsNullOrEmpty(StudentMidName) ? $"{ StudentTitle } { StudentFirstName } { StudentLastName }"
                                                                              : $"{ StudentTitle } { StudentFirstName } { StudentMidName } { StudentLastName }";
        public string CourseCredit => $"{ Credit.ToString(StringFormat.GeneralDecimal) }({ Lecture.ToString(StringFormat.GeneralDecimal) }-{ Lab.ToString(StringFormat.GeneralDecimal) }-{ Other.ToString(StringFormat.GeneralDecimal) })";
        public string CourseAndSection => $"{ CourseCode } { CourseCredit } ({ SectionNumber })";
        public string CourseAndSectionText { get; set; }
    }
}