using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class FinanceOtherFeeViewModel
    {
        public long Id { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public string FeeItem { get; set; }
        public string TotalAmount { get; set; }
        public string CreatedAt { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string InvoiceNumber { get; set; }
        public string ReceiptNumber { get; set; }
        public string PrintedBy { get; set; }
        public string TotalAmountTextTh { get; set; }
        public List<FinanceOtherFeeReceiptItem> ReceiptItems { get; set; }
    }

    public class FinanceOtherFeeFormModel
    {
        public string StudentCode { get; set; }
        public long TermId { get; set; }
        public int Year { get; set; }
        public List<FinanceOtherFeeItemModel> FeeItems { get; set; }
        public List<ReceiptPaymentMethod> PaymentMethods { get; set; }
        public decimal Amount { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class FinanceOtherFeeItemModel
    {
        public long FeeItemId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class FinanceOtherFeeReceiptItem
    {
        public string Name { get; set; }
        public string Quantity { get; set; }
        public string AmountPrice { get; set; }
        public string TotalAmount { get; set; }
    }
}