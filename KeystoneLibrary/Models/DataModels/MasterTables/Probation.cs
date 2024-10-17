using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class Probation : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(300)]
        public string Name { get; set; }
        public decimal MinimumGPA { get; set; }
        public decimal MaximumGPA { get; set; }
        public int Time { get; set; } // how many time that student can take this probation

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime EffectivedAt { get; set; } = DateTime.Now;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ExpiredAt { get; set; }

        [NotMapped]
        public string EffectivedAtText => EffectivedAt.ToShortDateString();

        [NotMapped]
        public string ExpiredAtText => ExpiredAt == null ? "N/A" : ExpiredAt?.ToShortDateString();

        [NotMapped]
        public string ProbationGPA => $"{ Name }({ MinimumGPA } - { MaximumGPA })";
    }
}