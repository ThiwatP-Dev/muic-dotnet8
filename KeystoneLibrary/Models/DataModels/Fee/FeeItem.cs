using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Fee
{
    public class FeeItem : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }
        
        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required]
        [StringLength(200)]
        public string NameTh { get; set; }
        
        [Required]
        [StringLength(200)]
        public string FormalNameEn { get; set; } // use for receipt

        [Required]
        [StringLength(200)]
        public string FormalNameTh { get; set; } // use for receipt

        [StringLength(200)]
        public string? AccountCode { get; set; } // use for receipt

        public decimal DefaultPrice { get; set; }

        [NotMapped]
        public string DefaultPriceText => DefaultPrice.ToString(StringFormat.TwoDecimal);

        public long FeeGroupId { get; set; }
        public bool IsEditable { get; set; }
        public bool IsRefundable { get; set; }
        public bool IsLumpsum { get; set; }
        public decimal RefundPercentage  { get; set; }

        [StringLength(500)]
        public string? Remark { get; set; }
        
        [NotMapped]
        public string CodeAndName => $"{ Code } - { NameEn }";

        [ForeignKey("FeeGroupId")]
        public virtual FeeGroup FeeGroup { get; set; }
    }   
}