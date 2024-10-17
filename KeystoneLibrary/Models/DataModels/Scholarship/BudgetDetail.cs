using KeystoneLibrary.Models.DataModels.Fee;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Scholarship
{
    public class BudgetDetail : UserTimeStamp
    {
        public long Id { get; set; }
        public long ScholarshipId { get; set; }
        public long FeeGroupId { get; set; }
        public decimal Amount { get; set; }

        [NotMapped]
        public string AmountText => Amount.ToString(StringFormat.TwoDecimal);
        
        [ForeignKey("ScholarshipId")]
        public virtual Scholarship Scholarship { get; set; }

        [ForeignKey("FeeGroupId")]
        public virtual FeeGroup FeeGroup { get; set; }
    }
}