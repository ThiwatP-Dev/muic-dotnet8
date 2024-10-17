using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models
{
    public class ReceiveDairySummaryViewModel
    {
        public List<PaymentMethod> PaymentMethods { get; set; }
        public Criteria Criteria { get; set; }
        public List<ReceiveReceiptItemViewModel> ReceiptItems { get; set; }
    }

    public class ReceiveReceiptItemViewModel
    {
        public long ReceiptId { get; set; }
        public string Name { get; set; }
        public int TotalStudent { get; set; }
        public string StudentPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string TotalPriceText => TotalPrice.ToString(StringFormat.Money);
        public string TotalPaymentMethod => PaymentAmounts.Sum(x => x.PaymentAmount).ToString(StringFormat.TwoDecimal);
        public List<PaymentMethod> PaymentMethods { get; set; }
        public List<ReceiptPaymentMethod> ReceiptPaymentMethods { get; set; }
        public List<PaymentMethodAmountViewModel> PaymentAmounts { get; set; }
    }

    public class PaymentMethodAmountViewModel
    {
        public string PaymentName { get; set; }
        public decimal  PaymentAmount { get; set; }
        public string  PaymentAmountText => PaymentAmount.ToString(StringFormat.Money);
    }
}