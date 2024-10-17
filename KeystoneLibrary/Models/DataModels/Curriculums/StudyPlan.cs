using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Curriculums
{
    public class StudyPlan : UserTimeStamp
    {
        public long Id { get; set; }
        public long CurriculumVersionId { get; set; }
        public long? AcademicProgramId { get; set; }
        public long? SpecializationGroupId { get; set; }

        [StringLength(300)]
        public string? Language { get; set; }

        // Student Group ex. Transfer student

        [StringLength(500)]
        public string? DescriptionEn { get; set; }

        [StringLength(500)]
        public string? DescriptionTh { get; set; }
        public int Year { get; set; }
        public int Term { get; set; }
        public int TotalCredit { get; set; }

        [ForeignKey("CurriculumVersionId")]
        public virtual CurriculumVersion CurriculumVersion { get; set; }

        [ForeignKey("AcademicProgramId")]
        public virtual AcademicProgram? AcademicProgram { get; set; }

        [ForeignKey("SpecializationGroupId")]
        public virtual SpecializationGroup? SpecializationGroup { get; set; }
        public virtual List<StudyCourse> StudyCourses { get; set; }

        [NotMapped]
        private readonly static string[] yearString = new string[] { "Zero", "First", "Second", "Third", "Fouth",
                                                                     "Fifth", "Sixth", "Seventh", "Eighth" };

        [NotMapped]
        public string YearText 
        { 
            get
            {
                return Year > yearString.Length ? "" : yearString[Year];
            }
        }
    }
}