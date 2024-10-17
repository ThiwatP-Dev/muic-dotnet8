using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class StudentRawScoreDetail : UserTimeStamp
    {
        public long Id { get; set; }
        public long StudentRawScoreId { get; set; }
        public long MarkAllocationId { get; set; }
        public decimal? Score { get; set; }

        [ForeignKey("StudentRawScoreId")]
        public virtual StudentRawScore StudentRawScore { get; set; }

        [ForeignKey("MarkAllocationId")]
        public virtual MarkAllocation MarkAllocation { get; set; }
    }
}