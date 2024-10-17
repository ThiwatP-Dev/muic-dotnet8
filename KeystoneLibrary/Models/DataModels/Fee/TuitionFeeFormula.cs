using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Fee
{
    public class TuitionFeeFormula : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
        public long? FirstTuitionFeeTypeId { get; set; }
        public long? SecondTuitionFeeTypeId { get; set; }

        [JsonIgnore]
        [ForeignKey("FirstTuitionFeeTypeId")]
        public virtual TuitionFeeType? FirstTuitionFeeType { get; set; }

        [JsonIgnore]
        [ForeignKey("SecondTuitionFeeTypeId")]
        public virtual TuitionFeeType? SecondTuitionFeeType { get; set; }
        public virtual List<TuitionFee> TuitionFees { get; set; }

        [NotMapped]
        public string FirstTuitionFeeTypeText => FirstTuitionFeeType == null ? "N/A" : FirstTuitionFeeType.Name;

        [NotMapped]
        public string SecondTuitionFeeTypeText => SecondTuitionFeeType == null ? "N/A" : SecondTuitionFeeType.Name;
    }
}