namespace KeystoneLibrary.Models
{
    public class ImportFeeFromBankViewModel
    {
        public List<ImportFeeFromBankSuccessDetail> Success { get; set; }
        public List<ImportFeeFromBankFailDetail> Fail { get; set; }
    }

    public class ImportFeeFromBankFailDetail
    {
        public string StudentCode { get; set; }
        public string StudentFullName { get; set; }
        public decimal Amount { get; set; }
        public string AmountText => Amount.ToString(StringFormat.Money);
        public DateTime PaidDate { get; set; }
        public string PaidDateText => PaidDate.ToString(StringFormat.ShortDate);
    }

    public class ImportFeeFromBankSuccessDetail
    {
        public string StudentCode { get; set; }
        public string InvoiceNumber { get; set; }
        public string ReceiptNumber { get; set; }
    }
}