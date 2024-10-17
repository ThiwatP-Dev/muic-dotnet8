using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class Facility : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }
        
        [StringLength(200)]
        public string? NameTh { get; set; }

        [JsonIgnore]
        public virtual List<RoomFacility> RoomFacilities { get; set; }
    }
}