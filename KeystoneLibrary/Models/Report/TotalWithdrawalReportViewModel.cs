namespace KeystoneLibrary.Models.Report
{
    public class TotalWithdrawalReportViewModel
    {
        public string CourseCode { get; set; }
        public string CourseCredit { get; set; }
        public string CourseNameEn { get; set; }
        public string CourseNameTh { get; set; }
        public string SectionNumber { get; set; }
        public string Instructor { get; set; }
        public long EnrollmentStudent { get; set; }
        public long WithdrawalStudent { get; set; }
        public long LeftOverStudent 
        {
            get
            {
                return EnrollmentStudent - WithdrawalStudent;
            }
        }
    }

}