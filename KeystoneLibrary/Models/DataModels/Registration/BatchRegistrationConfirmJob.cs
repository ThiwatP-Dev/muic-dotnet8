using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class BatchRegistrationConfirmJob : UserTimeStamp
    {
        public BatchRegistrationConfirmJob() { }
        public long Id { get; set; }

        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }

        public string? CriteriaJson { get; set; }

        public DateTime? StartProcessDateTimeUtc { get; set; }
        public DateTime? FinishProcessDateTimeUtc { get; set; }

        public bool IsRunning { get; set; }
        public bool IsCompleted { get; set; }
        public string? RunRemark { get; set; }

        public virtual List<BatchRegistrationConfirmJobDetail> BatchRegistrationConfirmJobDetails { get; set; }

        #region Not Mapped lazy coding 

        [NotMapped]
        public string Term { get; set; }

        [NotMapped]
        public string StartProcessDateTimeThString { get => StartProcessDateTimeUtc?.AddHours(7).ToString(StringFormat.ShortDateTime); }

        [NotMapped]
        public string FinishProcessDateTimeThString { get => FinishProcessDateTimeUtc?.AddHours(7).ToString(StringFormat.ShortDateTime); }

        [NotMapped]
        public long TotalCase { get => BatchRegistrationConfirmJobDetails?.Count ?? 0; }

        [NotMapped]
        public long SuccessCase { get => BatchRegistrationConfirmJobDetails?.Count(x => x.IsSuccess) ?? 0; }

        [NotMapped]
        public long ErrorCase { get => BatchRegistrationConfirmJobDetails?.Count(x => !x.IsSuccess && x.FinishProcessDateTimeUtc.HasValue) ?? 0; }

        #endregion
    }
}
