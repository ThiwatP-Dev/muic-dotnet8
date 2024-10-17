using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class StudentGrade : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public long TermId { get; set; }

        [Required]
        public decimal GPA { get; set; }

        [NotMapped]
        public string GPAText => GPA.ToString(StringFormat.TwoDecimal);
        
        [JsonIgnore]
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [JsonIgnore]
        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }
    }
}