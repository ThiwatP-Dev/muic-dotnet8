using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Questionnaire
{
    public class QuestionnairePeriod : UserTimeStamp
    {
        public long Id { get; set; }
        public long TermId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime StartedAt { get; set; } = DateTime.Today;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime EndedAt { get; set; } = DateTime.Today;

        [JsonIgnore]
        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [NotMapped]
        public long AcademicLevelId { get; set; }

        [NotMapped]
        public string StartedAtText => StartedAt.ToString(StringFormat.ShortDateTime);

        [NotMapped]
        public string EndedAtText => EndedAt.ToString(StringFormat.ShortDateTime);
    }
}