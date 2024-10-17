using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class EventCategory : UserTimeStamp
    {
        public long Id { get; set; }
        
        [StringLength(200)]
        public string? NameEn { get; set; }

        [StringLength(200)]
        public string? NameTh { get; set; }
    }
}