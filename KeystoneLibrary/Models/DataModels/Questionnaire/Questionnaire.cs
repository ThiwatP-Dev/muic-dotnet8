using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Questionnaire
{
    public class Questionnaire : UserTimeStamp
    {
        public long Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required]
        [StringLength(200)]
        public string NameTh { get; set; }
        public long? FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public long? CourseId { get; set; }

        [StringLength(500)]
        public string? DescriptionEn { get; set; }

        [StringLength(500)]
        public string? DescriptionTh { get; set; }

        [Required]
        [StringLength(50)]
        public string ResponseType { get; set; } // Student, Instructor, Officer

        [JsonIgnore]
        [ForeignKey("FacultyId")]
        public virtual Faculty Faculty { get; set; }

        [JsonIgnore]
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        [JsonIgnore]
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
        public virtual List<QuestionGroup> QuestionGroups { get; set; }

        [NotMapped]
        public QuestionGroup QuestionGroup { get; set; }
    }
}