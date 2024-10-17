using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Fee
{
    public class StudentFeeGroup : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }
        public int? StartedBatch { get; set; }
        public int? EndedBatch { get; set; }

        [StringLength(500)]
        public string? StudentGroupIds { get; set; } // Student Group Id List ex. {1, 2, 3}
        public long? AcademicLevelId { get; set; }
        public long? FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public long? CurriculumId { get; set; }
        public long? CurriculumVersionId { get; set; }
        public long? NationalityId { get; set; }
        public bool? IsThai { get; set; } // true = Thai, false = non-Thai, null = all
        public bool IsForIntensive { get; set; } // fee for intensive student
        public long? StartedTermId { get; set; }
        public long? EndedTermId { get; set; }
        public bool IsLumpsumPayment { get; set; }

        public long? StudentFeeTypeId { get; set; }
        public virtual List<TermFee> TermFees { get; set; }

        [NotMapped]
        public string AcademicLevel { get; set; }

        [NotMapped]
        public string StartedTerm { get; set; }

        [NotMapped]
        public string EndedTerm { get; set; }

        [NotMapped]
        public int StartedTermAcademicYear { get; set; }

        [NotMapped]
        public int StartedTermAcademicTerm { get; set; }

        [NotMapped]
        public int EndedTermAcademicYear { get; set; }

        [NotMapped]
        public int EndedTermAcademicTerm { get; set; }

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
        public List<long> StudentGroupIdsLong => string.IsNullOrEmpty(StudentGroupIds)
                                                 ? new List<long>() : JsonConvert.DeserializeObject<List<long>>(StudentGroupIds);

        [NotMapped]
        public string StudentFeeType { get; set; }
    }
}