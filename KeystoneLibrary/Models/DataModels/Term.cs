using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels
{
    public class Term : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        public long AcademicLevelId { get; set; }

        [Required]
        public int AcademicYear { get; set; }

        [Required]
        public int AcademicTerm { get; set; }

        [Required]
        public long TermTypeId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? StartedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? EndedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? FirstRegistrationEndedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? FirstRegistrationPaymentEndedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? AddDropPaymentEndedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? LastPaymentEndedAt { get; set; }
        public decimal TotalWeeksCount { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsAdvising { get; set; }
        public bool IsRegistration { get; set; }
        public bool IsAdmission { get; set; }
        public bool IsQuestionnaire { get; set; }
        public int MinimumCredit { get; set; }
        public int MaximumCredit { get; set; }

        [ForeignKey("AcademicLevelId")]
        public virtual AcademicLevel AcademicLevel { get; set; }

        [ForeignKey("TermTypeId")]
        public virtual TermType TermType { get; set; }

        [NotMapped]
        public string TermText => $"{ AcademicTerm }/{ AcademicYear }";

        [NotMapped]
        public string TermThText => $"{ AcademicTerm }/{ AcademicYear + 543 }";

        [NotMapped]
        public string TermPeriodText => $"{ AcademicTerm }/{ AcademicYear }-{ AcademicYear + 1 }";

        [NotMapped]
        public string TotalCountText => TotalWeeksCount.ToString(StringFormat.GeneralDecimal);

        [NotMapped]
        public string StartedDate => StartedAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string EndedDate => EndedAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public bool IsScholarshipExpiryTerm { get; set; }

        [NotMapped]
        public string TermCompare => $"{ AcademicYear }/{ AcademicTerm }";

        [NotMapped]
        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? StartedAtLocal { get { return StartedAt?.AddHours(7); } set { StartedAt = value?.AddHours(-7); } }

        [NotMapped]
        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? EndedAtLocal { get { return EndedAt?.AddHours(7); } set { EndedAt = value?.AddHours(-7); } }

        [NotMapped]
        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? FirstRegistrationEndedAtLocal { get { return FirstRegistrationEndedAt?.AddHours(7); } set { FirstRegistrationEndedAt = value?.AddHours(-7); } }

        [NotMapped]
        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? FirstRegistrationPaymentEndedAtLocal { get { return FirstRegistrationPaymentEndedAt?.AddHours(7); } set { FirstRegistrationPaymentEndedAt = value?.AddHours(-7); } }

        [NotMapped]
        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? AddDropPaymentEndedAtLocal { get { return AddDropPaymentEndedAt?.AddHours(7); } set { AddDropPaymentEndedAt = value?.AddHours(-7); } }

        [NotMapped]
        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? LastPaymentEndedAtLocal { get { return LastPaymentEndedAt?.AddHours(7); } set { LastPaymentEndedAt = value?.AddHours(-7); } }
    }
}
