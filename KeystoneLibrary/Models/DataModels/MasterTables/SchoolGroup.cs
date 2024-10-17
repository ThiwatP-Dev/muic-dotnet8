using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class SchoolGroup : UserTimeStamp
    {
        public long Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string NameTh { get; set; }

        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }

        [StringLength(500)]
        public string? Address1 { get; set; } // Address for send letter

        [StringLength(500)]
        public string? Address2 { get; set; }

        [StringLength(500)]
        public string? Address3 { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }
    }
}