using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class MarkAllocation : UserTimeStamp
    {
        public long Id { get; set; }
        public long TermId { get; set; }
        public long CourseId { get; set; }
        public int Sequence { get; set; }

        [StringLength(100)]
        public string? Name { get; set; } 

        [StringLength(100)]
        public string? Abbreviation { get; set; } 

        public decimal Score { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
        
        [JsonIgnore]
        public virtual List<StudentRawScoreDetail> StudentRawScoreDetails { get; set; }
    }
}