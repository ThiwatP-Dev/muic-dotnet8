using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Scholarship
{
    public class ScholarshipBudget : UserTimeStamp
    {
        public long Id { get; set; }
        public long ScholarshipId { get; set; }
        public long? AcademicLevelId { get; set; }
        public long? FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public long? CurriculumId { get; set; }
        public long? CurriculumVersionId { get; set; }
        public decimal Amount { get; set; }
        public int TotalYear { get; set; }

        [NotMapped]
        public string AmountText => Amount.ToString(StringFormat.TwoDecimal);
        
        [ForeignKey("ScholarshipId")]
        public virtual Scholarship Scholarship { get; set; }

        [ForeignKey("AcademicLevelId")]
        public virtual AcademicLevel AcademicLevel { get; set; }

        [ForeignKey("FacultyId")]
        public virtual Faculty Faculty { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        [ForeignKey("CurriculumId")]
        public virtual Curriculum Curriculum { get; set; }

        [ForeignKey("CurriculumVersionId")]
        public virtual CurriculumVersion CurriculumVersion { get; set; }
    }
}