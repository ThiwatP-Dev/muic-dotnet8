using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Scholarship
{
    public class Scholarship : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(500)]
        public string NameEn { get; set; }

        [Required]
        [StringLength(500)]
        public string NameTh { get; set; }
        public long? StartedTermId { get; set; }
        public long? EndedTermId { get; set; }
        public int? TotalYear { get; set; }
        public decimal? MinimumGPA { get; set; }
        public decimal? MaximumGPA { get; set; }
        public bool AllowRepeatedRegistration { get; set; }
        public bool HaveContract { get; set; }
        public bool HaveScholarshipHour { get; set; }
        public decimal? LimitedAmount { get; set; } // For all student 
        public decimal? DefaultAmount { get; set; } // For one student
        public int? TotalStudent { get; set; } // Total no of student
        public long ScholarshipTypeId { get; set; }
        public long? SponsorId { get; set; }
        public bool IsFullAmount { get; set; }
        public bool IsPayBack { get; set; }
        public bool IsExceptFine { get; set; }

        [StringLength(2000)]
        public string? Remark { get; set; }
        public virtual List<ScholarshipFeeItem> ScholarshipFeeItems { get; set; }
        public virtual List<BudgetDetail> BudgetDetails { get; set; }
        
        [NotMapped]
        public string DefaultAmountText => DefaultAmount?.ToString(StringFormat.Money);
        
        [NotMapped]
        public string LimitedAmountText => LimitedAmount?.ToString(StringFormat.Money);
        
        [NotMapped]
        public long? AcademicLevelId { get; set; }
        
        [ForeignKey("StartedTermId")]
        public virtual Term? StartedTerm { get; set; }

        [ForeignKey("EndedTermId")]
        public virtual Term? EndedTerm { get; set; }

        [ForeignKey("ScholarshipTypeId")]
        public virtual ScholarshipType ScholarshipType { get; set; }

        [ForeignKey("SponsorId")]
        public virtual Sponsor? Sponsor { get; set; }
    }
}