using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models
{
    public class InvoiceViewModel
    {
        public long InvoiceId { get; set; }
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public string Type { get; set; }
        public string Term { get; set; }
        public int AcademicYear { get; set; }
        public string InvoiceNumber { get; set; }
        public string StudentCode { get; set; }
        public string FullName { get; set; }
        public string Program { get; set; }
        public string PrintedAt { get; set; }
        public bool? IsAddDrop { get; set; }
        public bool? IsLateRegis { get; set; }
        public string StartedDate { get; set; }
        public string EndedDate { get; set; }
        public string AllDiscountAmountText { get; set; }
        public string TotalAmount { get; set; }        
        public string Base64Barcode { get; set; }
        public string Barcode { get; set; }
        public List<InvoiceItemDetail> InvoiceItems { get; set; }
    }

    public class InvoiceItemDetail
    {
        public long FeeItemId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Amount { get; set; }
    }

    public class BarcodeViewModel
    {
        public int Action { get ; set;} 
        public string Value { get; set; }
        public string Value2 { get; set; }
    }

    public class PaymentRequestBodyViewModel
    {
        public string OrderId { get; set; }
        public string PaymentProvider { get; set; }
        public string PaymentMethod { get; set; }
        public string ConfigurationKey { get; set; }
        public string CustomerId { get; set; }
        public decimal Amount { get; set; }

        [StringLength(20)]
        public string Reference1 { get; set; }

        [StringLength(20)]
        public string Reference2 { get; set; }

        [StringLength(20)]
        public string Reference3 { get; set; }
    }

    public class InvoiceUpdateIsPaidViewModel
    {
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaymentSuccess { get; set; }
        public string TransactionId { get; set; }
        public string Reference1 { get; set; }
        public string Reference2{ get; set; }
        public string Reference3 { get; set; }
        public string TransactionDateTime { get; set; }
        public string RawResponseMessage { get; set; }
    } 
}