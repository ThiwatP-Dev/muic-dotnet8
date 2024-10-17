using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class RegistrationReceiptDairySummaryViewModel
    {
        public string GrandTotal => Terms.Sum(x => x.GrandTotal).ToString(StringFormat.TwoDecimal);
        public Criteria Criteria { get; set; }
        public List<RegistrationReceiptDairyTerm> Terms { get; set; }
    }

    public class RegistrationReceiptDairyTerm
    {
        public List<Term> TermHeader { get; set; }
        public string TotalByDate { get; set; }
        public decimal GrandTotal => Students.Sum(x => x.TotalAmount);
        public List<RegistrationReceiptDairyStudent> Students { get; set; }
        
    }

    public class RegistrationReceiptDairyStudent
    {
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public decimal TotalAmount => Amounts.Sum(x => x.Amount);
        public string TotalAmountText => Amounts.Sum(x => x.Amount).ToString(StringFormat.TwoDecimal);
        public List<RegistrationReceiptDairyAmount> Amounts { get; set; }
    }

    public class RegistrationReceiptDairyAmount
    {
        public string Term { get; set; }
        public decimal Amount { get; set; }
        public string AmountText { get; set; }
    }
}