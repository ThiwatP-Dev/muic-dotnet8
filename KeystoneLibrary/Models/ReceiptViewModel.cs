using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Fee;

namespace KeystoneLibrary.Models
{
    public class ReceiptViewModel
    {
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public List<Receipt> Receipts { get; set; } = new List<Receipt>();
    }

    public class CourseRegistration
    {
        public Guid StudentId { get; set; }
        public int RegistrationRound { get; set; }
        public long RegistrationTermId { get; set; }
        public List<RegisteringCourse> RegisteringCourses { get; set; }
        public bool IsRegistered { get; set; }
    }

    public class RegisteringCourse
    {
        public long RegistrationCourseId { get; set; }
        public long CourseId { get; set; }
        public decimal RefundPercentage { get; set; }
        public bool IsPaid { get; set; }
        public long SectionId { get; set; }
        public string SectionNumber { get; set; }
    }

    public class TuitionRegistration
    {
        public TuitionRegistration()
        {
            TuitionFees = new List<TuitionFee>();
        }

        public long TermId { get; set; }
        public Section Section { get; set ; }
        public List<TuitionFee> TuitionFees { get; set; }
    }

    public class TuitionItem
    {
        public long TermId { get; set; }
        public Section Section { get; set; }
        public List<TuitionFee> TuitionFees { get; set; }
    }

    public class ReceiptModalViewModel
    {
        public List<ReceiptDetailViewModel> ReceiptDetailViewModels { get; set; }
        public string TotalText => ReceiptDetailViewModels.Sum(x => x.Total).ToString(StringFormat.TwoDecimal);
        public string TotalAmountText => ReceiptDetailViewModels.Sum(x => x.TotalAmount).ToString(StringFormat.TwoDecimal);
        public string ScholarshipTotalText => ReceiptDetailViewModels.Sum(x => x.ScholarshipAmount).ToString(StringFormat.TwoDecimal);
    }

    public class ReceiptModalItem
    {
        public long FeeItemId { get; set; }
        public string FeeItemName { get; set; }
        public long CourseId { get; set; }
        public string CourseName { get; set; }
        public string CourseAndCredit { get; set; }
        public int PaymentCredit { get; set; }
        public string PaymentType { get; set; }
        public decimal Amount { get; set; }
        public string AmountText => Amount.ToString(StringFormat.TwoDecimal);
        public decimal TotalAmount { get; set; }
        public string TotalAmountText => TotalAmount.ToString(StringFormat.TwoDecimal);
        public decimal ScholarshipAmount { get; set; }
        public string ScholarshipAmountText => ScholarshipAmount.ToString(StringFormat.TwoDecimal);
        public bool IsTermFee { get; set; }
        public string ItemJsonString { get; set; }
    }

    public class ReceiptDetailViewModel
    {
        public long ItemId { get; set; }
        public string ItemTitle { get; set; }
        public List<ReceiptModalItem> ReceiptModalItems { get; set; } = new List<ReceiptModalItem>();
        public decimal Total => ReceiptModalItems.Sum(x => x.Amount);
        public string TotalText => Total.ToString(StringFormat.TwoDecimal);
        public decimal ScholarshipAmount => ReceiptModalItems.Sum(x => x.ScholarshipAmount);
        public string ScholarshipAmountText => ScholarshipAmount.ToString(StringFormat.TwoDecimal);
        public decimal TotalAmount => ReceiptModalItems.Sum(x => x.TotalAmount);
        public string TotalAmountText => TotalAmount.ToString(StringFormat.TwoDecimal);
        public bool IsTermFee { get; set; }
    }

    public class FeeItemViewModel
    {
        public string FeeItemName { get; set; }
        public long FeeItemId { get; set; }
    }

    public class ReceiptPreviewViewModel
    {
        public long ReceiptId { get; set; }
        public long TermId { get; set; }
        public long AcademicLevelId { get; set; }
        public string Term { get; set; }
        public string ReceiptNumber { get; set; }
        public string PrintedAt { get; set; }
        public string PaidAt { get; set; }
        public string StudentCode { get; set; }
        public string FullName { get; set; }
        public string Program { get; set; }
        public string InvoiceNumber { get; set; }
        public string PrintedBy { get; set; }
        public string TotalAmount { get; set; }
        public string TotalAmountTextTh { get; set; }
        public string TotalAmountTextEn { get; set; }
        public string InvoiceType { get; set; }
        public bool IsPrinted { get; set; }
        public List<ReceiptItemDetail> ReceiptItemDetails { get; set; }
    }

    public class ReceiptItemDetail
    {
        public long FeeItemId { get; set; }
        public string NameTh { get; set; }
        public string NameEn { get; set; }
        public string Amount { get; set; }
    }
}