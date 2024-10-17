using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class Country : UserTimeStamp
    {
        public long Id { get; set; }

        [StringLength(20)]
        public string? Code { get; set; }

        [StringLength(20)]
        public string? OHECCode { get; set; } // Office of the Higher Education Commission

        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }
        
        [Required]
        [StringLength(200)]
        public string NameTh { get; set; }
    }
}