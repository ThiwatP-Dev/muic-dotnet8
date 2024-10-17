namespace KeystoneLibrary.Models
{
    public class LatePaymentViewModel
    {
        public long? Id { get; set; }
        public long? AcademicLevelId { get; set; }
        public long? TermId { get; set; }
        public Guid StudentId { get; set; }
        public string StudentCode { get; set; }
        public string StudentCodeAndName { get; set; }
        public string StudentFullName { get; set; }
        public string FinishedRegistration { get; set; }
        public string Type { get; set; }
        public string Remark { get; set; }
        public string Status { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public List<LatePaymentViewModel> LatePayments { get; set; }
    }
}