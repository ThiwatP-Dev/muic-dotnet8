using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels
{
    public class StudentTerm : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long TermId { get; set; }
        public int MinimumCredit { get; set; }
        public int MaximumCredit { get; set; }
        public int AddDropCount { get; set; }
       
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }
    }
}