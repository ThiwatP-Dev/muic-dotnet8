using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class ExaminationType : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required]
        [StringLength(200)]
        public string NameTh { get; set; }

        [StringLength(10)]
        public string? Abbreviation { get; set; }
    }
}