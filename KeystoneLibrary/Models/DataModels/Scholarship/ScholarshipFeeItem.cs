using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Fee;

namespace KeystoneLibrary.Models.DataModels.Scholarship
{
    public class ScholarshipFeeItem : UserTimeStamp
    {
        public long Id { get; set; }
        public long ScholarshipId { get; set; }
        public long FeeItemId { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }

        [NotMapped]
        public string PercentageText => Percentage.ToString(StringFormat.GeneralDecimal);
        
        [ForeignKey("ScholarshipId")]
        public virtual Scholarship Scholarship { get; set; }

        [ForeignKey("FeeItemId")]
        public virtual FeeItem FeeItem { get; set; }
    }
}