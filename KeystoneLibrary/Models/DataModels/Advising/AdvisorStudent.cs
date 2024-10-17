using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Profile;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels.Advising
{
    public class AdvisorStudent : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long InstructorId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}")]
        public DateTime StartedAt { get; set; }
        
        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}")]
        public DateTime EndedAt { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("InstructorId")]
        public virtual Instructor Instructor { get; set; }
    }
}