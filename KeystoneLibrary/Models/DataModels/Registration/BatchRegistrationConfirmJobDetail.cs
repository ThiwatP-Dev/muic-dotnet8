using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class BatchRegistrationConfirmJobDetail : UserTimeStamp
    {
        public BatchRegistrationConfirmJobDetail() { }

        public long Id { get; set; }
        public long BatchRegistrationConfirmJobId { get; set; }

        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public Guid StudentId { get; set; }
        public string? StudentCode { get; set; }

        public DateTime? StartSyncWithUSparkDateTimeUtc { get; set; }
        public DateTime? FinishSyncWithUSparkDateTimeUtc { get; set; }
        public string? SyncWithUSparkRemark { get; set; }

        public DateTime? FinishProcessDateTimeUtc { get; set; }
        public bool IsSuccess { get; set; }
        public string? Result { get; set; }


        [ForeignKey("BatchRegistrationConfirmJobId")]
        public virtual BatchRegistrationConfirmJob BatchRegistrationConfirmJob { get; set; }

        #region Not Mapped lazy coding 
        [NotMapped]
        public bool IsChecked { get; set; }

        [NotMapped]
        public string StudentFullName { get; set; }

        [NotMapped]
        public string DepartmentName { get; set; }

        [NotMapped]
        public string FacultyName { get; set; }

        [NotMapped]
        public string AdmissionTypeName { get; set; }

        [NotMapped]
        public string StartSyncWithUSparkDateTimeThString { get => StartSyncWithUSparkDateTimeUtc?.AddHours(7).ToString(StringFormat.ShortDateTime); }

        [NotMapped]
        public string FinishSyncWithUSparkDateTimeThString { get => FinishSyncWithUSparkDateTimeUtc?.AddHours(7).ToString(StringFormat.ShortDateTime); }

        [NotMapped]
        public string FinishProcessDateTimeThString { get => FinishProcessDateTimeUtc?.AddHours(7).ToString(StringFormat.ShortDateTime); }

        [NotMapped]
        public int TotalRegistrationCredit { get; set; }


        #endregion

    }
}
