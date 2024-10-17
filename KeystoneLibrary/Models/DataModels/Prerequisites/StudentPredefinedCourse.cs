using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Profile;
using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.DataModels.Prerequisites
{
    public class StudentPredefinedCourse : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long CourseId { get; set; }

        [StringLength(10)]
        public string? Type { get; set; } // F = Force, R = Recommended, W = Whitelist 

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
    }
}