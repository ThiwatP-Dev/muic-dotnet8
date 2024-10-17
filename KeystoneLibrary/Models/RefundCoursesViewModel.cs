using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class RefundCoursesViewModel
    {
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public Guid StudentId { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public string ScholarshipName { get; set; }
        public List<ReceiptItem> ReceiptItems { get; set; }
        public List<ReceiptItem> NoRemainningReceiptItems { get; set; }
        public string TotalRefundAmount { get; set; }
    }
}