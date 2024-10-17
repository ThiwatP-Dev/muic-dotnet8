using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels.Advising
{
    public class AdvisingLog : UserTimeStamp
    {
        public long Id { get; set; }
        public long TermId { get; set; }
        public Guid StudentId { get; set; }
        public long InstructorId { get; set; }

        [StringLength(1000)] 
        public string? Message { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("InstructorId")]
        public virtual Instructor Instructor { get; set; }
    }
}