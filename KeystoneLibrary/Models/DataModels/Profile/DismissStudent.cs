using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class DismissStudent : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long TermId { get; set; } // current term
        public long ProbationId { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }
        
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("ProbationId")]
        public virtual Probation Probation { get; set; }
    }
}
