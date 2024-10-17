using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels
{
    public class RegistrationCondition : UserTimeStamp
    {
        public long Id { get; set; }

        [StringLength(500)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(4000)]
        public string? StudentCodes { get; set; } // case : open slot for specific student
        public long? AcademicLevelId { get; set; }
        public long? AcademicProgramId { get; set; }
        public long? FacultyId { get; set; }
        public long? DepartmentId { get; set; }

        [StringLength(100)]
        public string? StudentCodeStart { get; set; }

        [StringLength(100)]
        public string? StudentCodeEnd { get; set; }
        public int? BatchCodeStart { get; set; }
        public int? BatchCodeEnd { get; set; }
        public int? LastDigitStart { get; set; }
        public int? LastDigitEnd { get; set; }
        public bool? IsGraduating { get; set; }
        public bool? IsAthlete { get; set; }

        [ForeignKey("AcademicLevelId")]
        public virtual AcademicLevel? AcademicLevel { get; set; }

        [ForeignKey("AcademicProgramId")]
        public virtual AcademicProgram? AcademicProgram { get; set; }

        [ForeignKey("FacultyId")]
        public virtual Faculty? Faculty { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }
        public virtual List<RegistrationSlotCondition> RegistrationSlotConditions { get; set; }

        [NotMapped]
        public string StudentCode { get; set; }

        [NotMapped]
        public List<string> Students { get; set; } = new List<string>();

        [NotMapped]
        public string IsGraduatingText { get; set; }

        [NotMapped]
        public string IsAthleteText { get; set; }

        [NotMapped]
        public string StudentCodeRange
        {
            get
            {
                if (string.IsNullOrEmpty(StudentCodeStart) && string.IsNullOrEmpty(StudentCodeEnd))
                {
                    return "";
                }
                else
                {
                    return $"{ (string.IsNullOrEmpty(StudentCodeStart) ? "xxxxxxx" : StudentCodeStart) } - { (string.IsNullOrEmpty(StudentCodeEnd) ? "xxxxxxx" : StudentCodeEnd) }";
                }
            }
        }

        [NotMapped]
        public string Batch
        {
            get
            {
                if (!BatchCodeStart.HasValue && !BatchCodeEnd.HasValue)
                {
                    return "";
                }
                else
                {
                    return $"{ (!BatchCodeStart.HasValue ? "xx" : BatchCodeStart.ToString()) } - { (!BatchCodeEnd.HasValue ? "xx" : BatchCodeEnd.ToString()) }";
                }
            }
        }

        [NotMapped]
        public string LastDigit
        {
            get
            {
                if (!LastDigitStart.HasValue && !LastDigitEnd.HasValue)
                {
                    return "";
                }
                else
                {
                    return $"{ (!LastDigitStart.HasValue ? "x" : LastDigitStart.ToString()) } - { (!LastDigitEnd.HasValue ? "x" : LastDigitEnd.ToString()) }";
                }
            }
        }
    }
}