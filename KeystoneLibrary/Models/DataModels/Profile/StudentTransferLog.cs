using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class StudentTransferLog : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long TermId { get; set; }
        public long? TransferUniversityId { get; set; }

        [Required]
        [StringLength(100)]
        public string Source { get; set; } // Transfer, Change Curriculum. Graduating Check

        [StringLength(1000)]
        public string Remark { get; set; } // Transfer, Change Curriculum. Graduating Check
        
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("TransferUniversityId")]
        public virtual TransferUniversity TransferUniversity { get; set; }
        public virtual List<StudentTransferLogDetail> StudentTransferLogDetails { get; set; }
    }
}