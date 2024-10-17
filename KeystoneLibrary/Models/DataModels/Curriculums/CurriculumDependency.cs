using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Prerequisites;

namespace KeystoneLibrary.Models.DataModels.Curriculums
{
    public class CurriculumDependency : UserTimeStamp
    {
        public long Id { get; set; }
        public long CurriculumVersionId { get; set; }

        [StringLength(200)]
        public string? DependencyType { get; set; } // Corequisite, Equivalence
        public long DependencyId { get; set; }
        public long? MUICId { get; set; }

        [ForeignKey("CurriculumVersionId")]
        public virtual CurriculumVersion CurriculumVersion { get; set; }

        [NotMapped]
        public Corequisite Corequisite { get; set; }

        [NotMapped]
        public CourseEquivalent CourseEquivalent { get; set; }
    }
}