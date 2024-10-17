using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels
{
    public class ExaminationPeriod : UserTimeStamp
    {
        public long Id { get; set; }
        public long TermId { get; set; }
        public int Period { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? MidtermDate { get; set; }
        public TimeSpan? MidtermStart { get; set; }
        public TimeSpan? MidtermEnd { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? FinalDate { get; set; }
        public TimeSpan? FinalStart { get; set; }
        public TimeSpan? FinalEnd { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [NotMapped]
        public string MidtermText => $"{ MidtermDateText } { MidtermTimeText }";

        [NotMapped]
        public string FinalText => $"{ FinalDateText } { FinalTimeText }";

        [NotMapped]
        public string MidtermDateText => MidtermDate?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string MidtermTimeText => $"{ MidtermStart?.ToString(StringFormat.TimeSpan) } - { MidtermEnd?.ToString(StringFormat.TimeSpan) }";

        [NotMapped]
        public string FinalDateText => FinalDate?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string FinalTimeText => $"{ FinalStart?.ToString(StringFormat.TimeSpan) } - { FinalEnd?.ToString(StringFormat.TimeSpan) }";

        [NotMapped]
        public List<ExaminationCoursePeriod> Courses { get; set; }

        [NotMapped]
        public long AcademicLevelId { get; set; }
    }
}