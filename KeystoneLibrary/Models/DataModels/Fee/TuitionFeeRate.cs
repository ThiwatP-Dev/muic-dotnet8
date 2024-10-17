using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Fee
{
    public class TuitionFeeRate : UserTimeStamp
    {
        public long Id { get; set; }
        [StringLength(100)]
        public string? Name { get; set; }
        public int? StartedBatch { get; set; }
        public int? EndedBatch { get; set; }
        public decimal Amount { get; set; }
        public long TuitionFeeTypeId { get; set; }
        public long? StudentFeeTypeId { get; set; }
        public long? CustomCourseGroupId { get; set; }
        public string? WhitelistMajorIds { get; set; }

        [JsonIgnore]
        [ForeignKey("TuitionFeeTypeId")]
        public virtual TuitionFeeType TuitionFeeType { get; set; }

        [JsonIgnore]
        [ForeignKey("StudentFeeTypeId")]
        public virtual StudentFeeType? StudentFeeType { get; set; }

        [JsonIgnore]
        [ForeignKey("CustomCourseGroupId")]
        public virtual CustomCourseGroup? CustomCourseGroup { get; set; }

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
        public string AmountText => Amount.ToString(StringFormat.TwoDecimal);

        [NotMapped]
        public string TuitionFeeTypeText => TuitionFeeType == null ? "N/A" : TuitionFeeType.Name;

        [NotMapped]
        public List<long> WhitelistMajors { get; set; }

        [NotMapped]
        public string WhitelistMajorsText { get; set; }
    }
}