using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Curriculums;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class CurriculumInformation : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }

        [Required]
        public long FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public long? CurriculumVersionId { get; set; }
        public long? StudyPlanId { get; set; }

        [JsonIgnore]
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("CurriculumVersionId")]
        public virtual CurriculumVersion CurriculumVersion { get; set; }

        [ForeignKey("StudyPlanId")]
        public virtual StudyPlan StudyPlan { get; set; }

        [JsonIgnore]
        [ForeignKey("FacultyId")]
        public virtual Faculty Faculty { get; set; }

        [JsonIgnore]
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }
        public virtual List<SpecializationGroupInformation> SpecializationGroupInformations { get; set; }

        [NotMapped]
        public long AcademiclevelId { get; set; }

        [NotMapped]
        public string StudentName { get; set; }

        [NotMapped]
        public string StudentCode { get; set; }

        [NotMapped]
        public string Term { get; set; }

        [NotMapped]
        public string Division { get; set; }

        [NotMapped]
        public string Major { get; set; }

        [NotMapped]
        public long? CurriculumId { get; set; }

    }
}