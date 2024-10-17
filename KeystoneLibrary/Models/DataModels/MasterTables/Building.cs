using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class Building : UserTimeStamp
    {
        public long Id { get; set; } 

        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }

        [StringLength(200)] 
        public string? NameTh { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
        public int FloorNumber { get; set; } = 1;
        public TimeSpan? OpenedTime { get; set; }
        public TimeSpan? ClosedTime { get; set; }
        public long CampusId { get; set; }

        [ForeignKey("CampusId")]
        public virtual Campus Campus { get; set; }

        [NotMapped]
        public string OpenedTimeText => OpenedTime?.ToString(StringFormat.TimeSpan);

        [NotMapped]
        public string ClosedTimeText => ClosedTime?.ToString(StringFormat.TimeSpan);
    }
}