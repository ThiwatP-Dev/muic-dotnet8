namespace KeystoneLibrary.Models
{
    public class UpdateStudentFeeViewModel
    {
        public Criteria Criteria { get; set; } = new Criteria();
        public int MinimumCredit { get; set; }
        public int MaximumCredit { get; set; }
        public List<UpdateStudentFeeDetail> Details { get; set; }
    }

    public class UpdateStudentFeeDetail
    {
        public Guid StudentId { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string MidName { get; set; }
        public string LastName { get; set; }
        public string Major { get; set; }
        public string Nationality { get; set; }
        public string ResidentType { get; set; }
        public string StudentFeeType { get; set; }
        public string StudentFeeGroup { get; set; }
        public string AdmissionType { get; set; }
        public string IsChecked { get; set; }
        public string FullName => string.IsNullOrEmpty(MidName) ? $"{ Title } { FirstName } { LastName }"
                                                                : $"{ Title } { FirstName } { MidName } { LastName }";

    }
}