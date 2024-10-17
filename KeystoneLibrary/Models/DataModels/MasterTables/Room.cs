using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class Room : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }
        
        [StringLength(200)]
        public string? NameTh { get; set; }
        public int Floor { get; set; }
        public int Capacity { get; set; }
        public int ExaminationCapacity { get; set; }
        public long BuildingId { get; set; }
        public long RoomTypeId { get; set; }
        public bool IsOnline { get; set; }
        public bool IsAllowLecture { get; set; }
        public bool IsAllowSearch { get; set; }
        public bool AllowStudent { get; set; }
        public bool AllowInstructor { get; set; }
        public bool AllowStaff { get; set; }

        [StringLength(500)]
        public string? Remark { get; set; }

        [ForeignKey("BuildingId")]
        public virtual Building Building { get; set; }

        [ForeignKey("RoomTypeId")]
        public virtual RoomType RoomType { get; set; }

        [JsonIgnore]
        public virtual List<RoomFacility> RoomFacilities { get; set; } = new List<RoomFacility>();

        [NotMapped]
        public long CampusId
        {
            get
            {
                return Building == null ? 0 : Building.CampusId;
            }
        }

        public List<RoomSlot> RoomSlots { get; set; }
        
        [NotMapped]
        public string RoomFullNameEn => $"{ NameEn }, FL. { Floor }, { Building?.NameEn }, { Building?.Campus?.NameEn }";
        
        [NotMapped]
        public string RoomFullNameTh => $"{ NameTh }, FL. { Floor }, { Building?.NameTh }, { Building?.Campus?.NameTh }";
    }
}