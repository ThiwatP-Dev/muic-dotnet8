namespace KeystoneLibrary.Models
{
    public class ConfirmRefundViewModel
    {
        public bool IsChecked { get; set; }
        public long InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public decimal Amount { get; set; }
        public string AmountText => Amount.ToString(StringFormat.Money);
    }
}