using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class Coordinator : UserTimeStamp
    {
        public long Id { get; set; }

        public long CourseId { get; set; }

        public long TermId { get; set; }

        public long InstructorId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("InstructorId")]
        public virtual Instructor Instructor { get; set; }
    }
}