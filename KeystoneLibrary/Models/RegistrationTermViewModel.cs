using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models
{
    public class RegistrationTermViewModel
    {
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public List<RegistrationTermDetailViewModel> RegistrationTermDetails { get; set; }
    }

    public class RegistrationTermDetailViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string AcademicLevelNameEn { get; set; }
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public string TermText { get; set; }
        public bool IsActive { get; set; }
        public string Type { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? StartedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EndedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ExpiredAt { get; set; }

        public string StartedAtText => StartedAt?.ToString(StringFormat.ShortDate);
        public string EndedAtText => EndedAt?.ToString(StringFormat.ShortDate);
        public string ExpiredAtText => ExpiredAt?.ToString(StringFormat.ShortDate);

    }
}