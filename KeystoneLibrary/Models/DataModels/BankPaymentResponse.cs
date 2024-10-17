using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class BankPaymentResponse 
    {
        public long Id { get; set; }

        public long? InvoiceId { get; set; }
        
        [StringLength(50)]
        public string? Number { get; set; }

        public decimal? TotalAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public bool IsPaymentSuccess { get; set; }

        [StringLength(200)]
        public string? Reference1 { get; set; }

        [StringLength(200)]
        public string? Reference2 { get; set; }

        [StringLength(200)]
        public string? Reference3 { get; set; }

        [StringLength(200)]
        public string? TransactionId { get; set; }

        public DateTime TransactionDateTime { get; set; }

        public string? RawResponse { get; set; }

        [StringLength(200)]
        public string? CreatedAt { get; set; }

        [JsonIgnore]
        [ForeignKey("InvoiceId")]
        public virtual Invoice? Invoice { get; set; }
    }
}