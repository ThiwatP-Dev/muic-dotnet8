using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class ResignReason : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(500)]
        public string DescriptionEn { get; set; }
        
        [Required]
        [StringLength(500)]
        public string DescriptionTh { get; set; }
    }
}