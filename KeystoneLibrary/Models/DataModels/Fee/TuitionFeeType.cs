using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.DataModels.Fee
{
    public class TuitionFeeType : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
        public virtual List<TuitionFeeRate> TuitionFeeRates { get; set; }
    }
}