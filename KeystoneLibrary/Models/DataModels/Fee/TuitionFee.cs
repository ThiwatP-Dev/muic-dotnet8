using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Fee
{
    public class TuitionFee : UserTimeStamp
    {
        public long Id { get; set; }

        [StringLength(50)]
        public string? Code { get; set; }
        public int? StartedBatch { get; set; }
        public int? EndedBatch { get; set; }
        public long? FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public long? CurriculumId { get; set; }
        public long? CurriculumVersionId { get; set; }
        public long? StudentFeeTypeId { get; set; } // thai student, foreign

        [StringLength(500)]
        public string? StudentGroupIds { get; set; } // Student Group Id List ex. {1, 2, 3}
        public long? CourseId { get; set; }

        [StringLength(50)]
        public string? SectionNumber { get; set; }
        public long FeeItemId { get; set; }
        public long? CourseRateId { get; set; }
        public long? TuitionFeeFormulaId { get; set; } // Fix = null
        public long? StartedTermId { get; set; }
        public long? EndedTermId { get; set; }
        public decimal Amount { get; set; }

        [NotMapped]
        public bool Active { get; set; }

        [NotMapped]
        public long AcademicLevelId { get; set; }

        [NotMapped]
        public string Faculty { get; set; }

        [NotMapped]
        public string Department { get; set; }

        [NotMapped]
        public string Curriculum { get; set; }

        [NotMapped]
        public string CurriculumVersion { get; set; }

        [NotMapped]
        public string StudentFeeType { get; set; }

        [NotMapped]
        public Course Course { get; set; }

        [NotMapped]
        public CourseRate CourseRate { get; set; }

        [NotMapped]
        public string TermPeriod
        {
            get
            {
                if (StartedTerm == null && EndedTerm == null)
                {
                    return "N/A";
                }
                else
                {
                    string start = StartedTerm?.TermText ?? "xxx";
                    string end = EndedTerm?.TermText ?? "xxx";
                    return $"{ start } - { end }";
                }
            }
        }

        [NotMapped]
        public string AmountText => Amount.ToString(StringFormat.Money);

        [NotMapped]
        public string BatchRange
        {
            get
            {
                string start = StartedBatch == 0 || StartedBatch == null ? "xxx" : $"{ StartedBatch }";
                string end = EndedBatch == 0 || EndedBatch == null ? "xxx" : $"{ EndedBatch }";
                return StartedBatch == 0 && EndedBatch == 0 ? "N/A" : $"{ start } - { end }";
            }
        }

        [NotMapped]
        public FeeItem FeeItem { get; set; }

        [NotMapped]
        public Term StartedTerm { get; set; }

        [NotMapped]
        public Term EndedTerm { get; set; }

        [NotMapped]
        public string TuitionFeeFormula { get; set; }

        public TuitionFee(){
            Active = true;
        }
    }
}