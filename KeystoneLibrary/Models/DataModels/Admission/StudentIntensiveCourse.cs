using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels.Admission
{
    public class StudentIntensiveCourse : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long IntensiveCourseId { get; set; }
        public bool IsPaid { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
        
        [ForeignKey("IntensiveCourseId")]
        public virtual IntensiveCourse IntensiveCourse { get; set; }
    }   
}