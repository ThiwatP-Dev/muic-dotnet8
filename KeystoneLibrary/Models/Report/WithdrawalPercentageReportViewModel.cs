namespace KeystoneLibrary.Models.Report
{
    public class WithdrawalPercentageViewModel
    {
        public Criteria Criteria { get; set; }
        public string Term { get; set; }
        public string Type { get ; set; }
        public List<WithdrawalPercentageDetail> WithdrawalPercentageDetails { get; set; } = new List<WithdrawalPercentageDetail>();
        public decimal TotalNoOfStudents => WithdrawalPercentageDetails.Sum(x => x.NoOfStudents);
        public decimal TotalApplicationWithdrawalStudents => WithdrawalPercentageDetails.Sum(x => x.WithdrawalStudents.Where(y => y.Type == "Application")
                                                                                                                      .Sum(y => y.NoOfStudents));
        public decimal TotalApplicationWithdrawalStudentsPercentage => (TotalApplicationWithdrawalStudents * 100) / TotalNoOfStudents;
        public string TotalApplicationWithdrawalStudentsPercentageString => TotalApplicationWithdrawalStudentsPercentage.ToString(StringFormat.TwoDecimal);
        public decimal TotalDebarmentWithdrawalStudents => WithdrawalPercentageDetails.Sum(x => x.WithdrawalStudents.Where(y => y.Type == "Debarment")
                                                                                                                      .Sum(y => y.NoOfStudents));
        public decimal TotalDebarmentWithdrawalStudentsPercentage => (TotalDebarmentWithdrawalStudents * 100) / TotalNoOfStudents;
        public string TotalDebarmentWithdrawalStudentsPercentageString => TotalDebarmentWithdrawalStudentsPercentage.ToString(StringFormat.TwoDecimal);
        public decimal TotalNoOfWithdrawalStudent => WithdrawalPercentageDetails.Sum(x => x.NoOfWithdrawalStudent);
        public decimal TotalNoOfWithdrawalStudentPercentage => (TotalNoOfWithdrawalStudent * 100) / TotalNoOfStudents;
        public string TotalNoOfWithdrawalStudentPercentageString => TotalNoOfWithdrawalStudentPercentage.ToString(StringFormat.TwoDecimal);
    }

    public class WithdrawalPercentageDetail
    {
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public decimal NoOfStudents { get; set; }
        public decimal NoOfWithdrawalStudent { get; set; }
        public decimal WithdrawalStudentPercentage { get; set; }
        public string WithdrawalStudentPercentageText => WithdrawalStudentPercentage.ToString(StringFormat.TwoDecimal);
        public decimal ApplicationWithdrawalStudentPercentage { get; set; }
        public string ApplicationWithdrawalStudentPercentageText => ApplicationWithdrawalStudentPercentage.ToString(StringFormat.TwoDecimal);
        public decimal DebarmentWithdrawalStudentPercentage { get; set; }
        public string DebarmentWithdrawalStudentPercentageText => DebarmentWithdrawalStudentPercentage.ToString(StringFormat.TwoDecimal);
        public List<WithdrawalStudent> WithdrawalStudents { get; set; } = new List<WithdrawalStudent>();
    }

    public class WithdrawalStudent
    {
        public string Type { get; set; }
        public decimal NoOfStudents { get; set; }
    }
}