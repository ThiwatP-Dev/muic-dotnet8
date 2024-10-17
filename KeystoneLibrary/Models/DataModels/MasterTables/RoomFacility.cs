using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class RoomFacility
    {
        public long Id { get; set; }

        [Required]
        public long RoomId { get; set; }
        
        [Required]
        public long FacilityId { get; set; }
        public int Amount { get; set; } = 0;
        public bool IsActive { get; set; }

        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }

        [ForeignKey("FacilityId")]
        public virtual Facility Facility { get; set; }
    }
}