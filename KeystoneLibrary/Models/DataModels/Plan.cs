using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels
{
    public class Plan : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CourseIds { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        public long TermId { get; set; }
        public Guid? StudentId { get; set; }
        public long? FacultyId { get; set; }

        [ForeignKey("FacultyId")]
        public virtual Faculty Faculty { get; set; }
        
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }
        public virtual List<PlanSchedule> PlanSchedules { get; set; } = new List<PlanSchedule>();
        public List<long> CourseList => string.IsNullOrEmpty(CourseIds) ? new List<long>()
                                                                        : JsonConvert.DeserializeObject<List<long>>(CourseIds);

        [NotMapped]
        public List<string> CourseName { get; set; } = new List<string>();
        
        [NotMapped]
        public string CourseNameText => String.Join(", ", CourseName);
    }
}