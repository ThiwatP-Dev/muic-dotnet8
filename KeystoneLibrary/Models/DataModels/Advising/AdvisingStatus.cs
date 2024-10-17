using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels.Advising
{
    public class AdvisingStatus : UserTimeStamp
    {
        public long Id { get; set; }
        public long TermId { get; set; }
        public Guid StudentId { get; set; }
        public long InstructorId { get; set; }
        public bool IsAdvised { get; set; }
        public bool IsPaymentAllowed { get; set; }
        public bool IsRegistrationAllowed { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("InstructorId")]
        public virtual Instructor Instructor { get; set; }
    }
}