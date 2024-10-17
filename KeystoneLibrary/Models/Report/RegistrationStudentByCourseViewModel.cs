namespace KeystoneLibrary.Models.Report
{
    public class RegistrationStudentByCourseViewModel
    {
        public Criteria Criteria { get; set; }
        public List<RegistrationStudentByCourse> Results { get; set; }
    }

    public class RegistrationStudentByCourse
    {
        public string StudentCode { get; set; }
        public string StudentFullNameEn { get; set; }
        public string DepartmentAbbreviation { get; set; }
        public string ResidentTypeNameEn { get; set; }
        public string CourseAndCredit { get; set; }
        public string SectionNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public string TotalAmountText => TotalAmount.ToString(StringFormat.Money);
        public bool IsPaid { get; set; }
        public bool IsConfirmInvoice { get; set; }

        public long SectionId { get; set; }
        public Guid StudentId { get; set; }
    }
}