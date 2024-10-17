
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class ExaminationCoursePeriod : UserTimeStamp
    {
        public long Id { get; set; }
        public long CourseId { get; set; }
        public int? Period { get; set; }
        public bool IsEvening { get; set; }
        public bool IsSpeacialCase { get; set; }
        public bool HasMidterm { get; set; }
        public decimal? MidtermHour { get; set; }
        public bool HasFinal { get; set; }
        public decimal? FinalHour { get; set; }

        [StringLength(500)]
        public string? Remark { get; set; } 

        [JsonIgnore]
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
    }
}