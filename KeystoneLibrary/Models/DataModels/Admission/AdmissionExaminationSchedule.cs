using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels.Admission
{
    public class AdmissionExaminationSchedule
    {
        public long Id { get; set; }
        public long AdmissionExaminationId { get; set; }
        public long AdmissionExaminationTypeId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? TestedAt { get; set; } // add in academic calendar and nullable because student will test when they come to abac
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public long? RoomId { get; set; }

        [ForeignKey("AdmissionExaminationId")]
        public virtual AdmissionExamination AdmissionExamination { get; set; }
        
        [ForeignKey("AdmissionExaminationTypeId")]
        public virtual AdmissionExaminationType AdmissionExaminationType { get; set; }

        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }

        [NotMapped]
        public string TestedAtText => TestedAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string StartTimeText => StartTime?.ToString(StringFormat.TimeSpan);

        [NotMapped]
        public string EndTimeText => EndTime?.ToString(StringFormat.TimeSpan);

        [NotMapped]
        public string StartEndTimeText => StartTimeText + "-" + EndTimeText;
    }   
}