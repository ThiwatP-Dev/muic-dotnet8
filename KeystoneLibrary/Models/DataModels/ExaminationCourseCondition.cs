
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace KeystoneLibrary.Models.DataModels
{
    public class ExaminationCourseCondition : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CourseIds { get; set; }

        [Required]
        [StringLength(100)]
        public string Condition { get; set; }

        [NotMapped]
        public string ConditionText
        {
            get
            {
                switch (Condition)
                {
                    case "s":
                        return "Same Day";
                    case "d":
                        return "Different Day";
                    default:
                        return "N/A";
                }
            }
        }

        [NotMapped]
        public List<long> Courses { get; set; }

        [NotMapped]
        public List<Course> CourseList { get; set; }
    }
}