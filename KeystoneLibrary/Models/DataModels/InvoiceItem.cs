using KeystoneLibrary.Models.DataModels.Fee;
using KeystoneLibrary.Models.DataModels.Scholarship;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class InvoiceItem
    {
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Type { get; set; } // r = registration, a = add, d = delete, rf = refund
        public long InvoiceId { get; set; } 
        public long? CourseId { get; set; }
        public long? SectionId { get; set; }
        public long? RegistrationCourseId { get; set; }
        public long FeeItemId { get; set; } // Tuition

        [StringLength(100)]
        public string? FeeItemName { get; set; }
        public long? ScholarshipStudentId { get; set; } // scolarship 50 %
        public int TaxRate { get; set; }
        public bool IsVAT { get; set; }
        public bool IsVATIncluded { get; set; }
        public decimal Amount { get; set; } // 5000
        public decimal ScholarshipAmount { get; set; } // 2500
        public decimal DiscountAmount { get; set; } // waive
        [StringLength(200)]
        public string? DiscountRemark { get; set; } // waive
        public decimal TotalVATAmount { get; set; } // 0 : No vat
        public decimal TotalAmount { get; set; } // 2500 <-- Personal Pay Amount as before
        public bool IsPaid { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }

        [JsonIgnore]
        [ForeignKey("InvoiceId")]
        public virtual Invoice Invoice { get; set; }

        [JsonIgnore]
        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }
        
        [NotMapped]
        public string CourseCode { get; set; }

        [NotMapped]
        public string CourseName { get; set; }

        [NotMapped]
        public string CourseAndCredit { get; set; }

        [ForeignKey("SectionId")]
        public virtual Section? Section { get; set; }

        [ForeignKey("RegistrationCourseId")]
        public virtual RegistrationCourse? RegistrationCourse { get; set; }

        [ForeignKey("FeeItemId")]
        public virtual FeeItem FeeItem { get; set; }

        [ForeignKey("ScholarshipStudentId")]
        public virtual ScholarshipStudent? ScholarshipStudent { get; set; }
        public virtual ReceiptItem ReceiptItem { get; set; }

        [NotMapped]
        public string TypeText
        {
            get
            {
                switch (Type)
                {
                    case "r":
                        return "Registration";
                    case "a":
                        return "Add";
                    case "d":
                        return "Delete";
                    case "rf":
                        return "Refund";
                    default:
                        return "N/A";
                }
            }
        }

        [NotMapped]
        public string AmountText => Amount.ToString(StringFormat.Money);

        [NotMapped]
        public string TotalAmountText => TotalAmount.ToString(StringFormat.Money);
        
        [NotMapped]
        public List<ReceiptPaymentMethod> PaymentMethods { get; set; }

        [NotMapped]
        public string ScholarshipAmountText => ScholarshipAmount.ToString(StringFormat.Money);

        [NotMapped]
        public string DiscountAmountText => DiscountAmount.ToString(StringFormat.Money);
        
        [NotMapped]
        public string IsSelected { get; set; }
        
        [NotMapped]
        public string LumpSumAddDrop { get; set; }

        /// <summary>
        /// Used for mark late payment available until
        /// </summary>
        [NotMapped]
        public DateTime? LatePaymentExpiryDate { get; set; }
    }
}