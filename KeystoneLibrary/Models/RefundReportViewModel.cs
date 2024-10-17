namespace KeystoneLibrary.Models
{
    public class RefundReportViewModel
    {
        public Criteria Criteria { get; set; }
        public List<RefundReportDetail> Details { get; set; }
    }

    public class RefundReportDetail
    {        
        public long InvoiceId { get; set; }
        public string Term { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceType { get; set; }
        public string StudentCode { get; set; }
        public string StudentTitle { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentMidName { get; set; }
        public string StudentLastName { get; set; }
        public string Major { get; set; }
        public bool PaidStatus { get; set; }
        public string PaidDate { get; set; }
        public string TotalAmount { get; set; }
        public string CreateDate { get; set; }
        public string StudentFullName => string.IsNullOrEmpty(StudentMidName) ? $"{ StudentTitle } { StudentFirstName } { StudentLastName }"
                                                                              : $"{ StudentTitle } { StudentFirstName } { StudentMidName } { StudentLastName }";
    }
}