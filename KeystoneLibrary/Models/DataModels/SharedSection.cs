using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class SharedSection
    {
        public long Id { get; set; }
        public long SectionId { get; set; }

        [Required]
        [StringLength(30)]
        public string Number { get; set; }
        public long CourseId { get; set; }
        public int SeatAvailable { get; set; }
        public int SeatLimit { get; set; }
        public int SeatUsed { get; set; }
        public int ExtraSeat { get; set; }
        public int MinimumSeat { get; set; }

        [StringLength(500)]
        public string? Remark { get; set; }

        [JsonIgnore]
        [ForeignKey("SectionId")]
        public virtual Section Section { get; set; }

        [JsonIgnore]
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [NotMapped]
        public decimal AvailabilityPercentage => (SeatLimit <= 0) ? 0 : (SeatAvailable * 100) / SeatLimit;

        [NotMapped]
        public bool IsSeatOver { get; set; }

        [NotMapped]
        public int NumberValue
        {
            get
            {
                int number;
                bool success = Int32.TryParse(Number, out number);
                return success ? number : 0;
            }
        }
    }
}