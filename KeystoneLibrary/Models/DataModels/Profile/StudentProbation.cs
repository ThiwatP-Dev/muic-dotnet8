using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class StudentProbation : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long TermId { get; set; }
        public long? ProbationId { get; set; }
        public long? RetireId { get; set; }
        public decimal StudentGPA { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }
        public bool SendEmail { get; set; }
        
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("ProbationId")]
        public virtual Probation? Probation { get; set; }

        [ForeignKey("RetireId")]
        public virtual Retire? Retire { get; set; }

        [NotMapped]
        public string StudentGPAText => StudentGPA.ToString(StringFormat.TwoDecimal);
    }
}
