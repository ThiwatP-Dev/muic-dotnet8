using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Curriculums
{
    public class CurriculumInstructor : UserTimeStamp
    {
        public long CurriculumVersionId { get; set; }
        public long InstructorId { get; set; }

        [Required]
        [StringLength(5)]
        public string Type { get; set; } // curriculum instructor or thesis instructor or instructor in this curriculum

        [JsonIgnore]
        public virtual CurriculumVersion CurriculumVersion { get; set; }

        [JsonIgnore]
        public virtual Instructor Instructor { get; set; }

        [NotMapped]
        public string TypeText
        {
            get
            {
                switch (Type)
                {
                    case "c":
                        return "Instructor of curriculum";
                    case "t":
                        return "Thesis Instructor";
                    default:
                        return "Instructor";
                }
            }
        }
    }
}