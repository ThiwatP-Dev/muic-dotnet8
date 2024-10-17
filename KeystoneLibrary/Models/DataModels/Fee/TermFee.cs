using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Fee
{
    public class TermFee : UserTimeStamp
    {
        public long Id { get; set; }
        public long StudentFeeGroupId { get; set; }
        public int? StartedBatch { get; set; }
        public int? EndedBatch { get; set; }
        public long? FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public long? CurriculumId { get; set; }
        public long? CurriculumVersionId { get; set; }
        public long? TermTypeId { get; set; }
        public int? Term { get; set; }
        public long FeeItemId { get; set; }
        public long? StartedTermId { get; set; }
        public long? EndedTermId { get; set; }
        public bool IsOneTime { get; set; } // One time per each student
        public bool IsPerTerm { get; set; } // One time per semester
        public bool IsPerYear { get; set; } // One time per year
        public decimal Amount { get; set; }

        [JsonIgnore]
        [ForeignKey("FeeItemId")]
        public virtual FeeItem FeeItem { get; set; }

        [JsonIgnore]
        [ForeignKey("StudentFeeGroupId")]
        public virtual StudentFeeGroup StudentFeeGroup { get; set; }
        
        [NotMapped]
        public TermType TermType { get; set; }

        [NotMapped]
        public Term StartedTerm { get; set; }

        [NotMapped]
        public Term EndedTerm { get; set; }

        [NotMapped]
        public long? FeeItemIdAllowNull { get; set; }

        [NotMapped]
        public decimal? AmountAllowNull { get; set; }

        [NotMapped]
        public string CalculateType { get; set; }

        [NotMapped]
        public string AmountText => Amount.ToString(StringFormat.TwoDecimal);
    }   
}