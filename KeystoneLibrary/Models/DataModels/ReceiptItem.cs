using KeystoneLibrary.Models.DataModels.Fee;
using KeystoneLibrary.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class ReceiptItem
    {
        public long Id { get; set; }

        [Required]
        public long ReceiptId { get; set; }
        public long InvoiceId { get; set; }
        public long InvoiceItemId { get; set; }

        [Required]
        [StringLength(100)]
        public string Type { get; set; } // a = add, d = delete, r = refund
        public long? FeeItemId { get; set; }

        [StringLength(100)]
        public string? FeeItemName { get; set; }

        [NotMapped]
        public long CourseId { get; set; }

        [NotMapped]
        public string CourseCode { get; set; }

        [NotMapped]
        public string CouseName { get; set; }

        public int TaxRate { get; set; }
        public bool IsVAT { get; set; }
        public bool IsVATIncluded { get; set; }
        public decimal Amount { get; set; } // 5000
        public decimal ScholarshipAmount { get; set; } // 2500

        // [Obsolete]
        // public decimal PersonalPayAmount { get; set; } // 2500
        public decimal DiscountAmount { get; set; } // waive
        public decimal TotalVATAmount { get; set; } // 0 : No vat
        public decimal TotalAmount { get; set; } // 5000
        public decimal RemainingAmount { get; set; } // remaining for refund

        [JsonIgnore]
        [ForeignKey("ReceiptId")]
        public virtual Receipt Receipt { get; set; }

        [ForeignKey("InvoiceId")]
        public virtual Invoice Invoice { get; set; }

        [ForeignKey("InvoiceItemId")]
        public virtual InvoiceItem InvoiceItem { get; set; }

        [ForeignKey("FeeItemId")]
        public virtual FeeItem FeeItem { get; set; }

        [NotMapped]
        public bool IsRefund { get; set; }

        [NotMapped]
        public string AmountText
        {
            get
            {
                return Amount.ToString(StringFormat.TwoDecimal);
            }
        }

        [NotMapped]
        public string TotalVATAmountText
        {
            get
            {
                return TotalVATAmount.ToString(StringFormat.TwoDecimal);
            }
        }

        [NotMapped]
        public string TotalAmountText
        {
            get
            {
                return TotalAmount.ToString(StringFormat.TwoDecimal);
            }
        }

        [NotMapped]
        public string ScholarshipAmountText
        {
            get
            {
                return ScholarshipAmount.ToString(StringFormat.TwoDecimal);
            }
        }

        [NotMapped]
        public string RemainingAmountText
        {
            get
            {
                return RemainingAmount.ToString(StringFormat.TwoDecimal);
            }
        }

        [NotMapped]
        public string DiscountAmountText => DiscountAmount.ToString(StringFormat.TwoDecimal);

        [NotMapped]
        public string TypeText => string.IsNullOrEmpty(Type)
                                         ? "N/A" : (Enum.TryParse(Type, out FinancialTypeEnum type)? type.GetDisplayName() : "N/A");

        // [NotMapped]
        // public string TypeText
        // {
        //     get
        //     {
        //         switch (Type)
        //         {
        //             case "r":
        //                 return "Registration";
        //             case "a":
        //                 return "Add";
        //             case "d":
        //                 return "Delete";
        //             case "rf":
        //                 return "Refund";
        //             default:
        //                 return "N/A";
        //         }
        //     }
        // }

        [NotMapped]
        public long NonNullableFeeItemId => FeeItemId == null ? 0 : FeeItemId ?? 0;

        [NotMapped]
        public string ReceiptItemQuantity => Amount == 0 ? "1" : (TotalAmount / Amount).ToString();

        [NotMapped]
        public decimal RemainingScholarshipAmount { get; set; }

        [NotMapped]
        public decimal RemainingPersonalPayAmount { get; set; }

        [NotMapped]
        public decimal? RefundPercentage { get; set; }

        [NotMapped]
        public decimal? RefundAmount { get; set; }

        [NotMapped]
        public decimal RefundPersonalPayAmount { get; set; }

        [NotMapped]
        public decimal RefundScholarshipAmount { get; set; }

        [NotMapped]
        public DateTime? RefundedAt { get; set; }

        [NotMapped]
        public bool IsReturnBudget { get; set; }

        
    }
}