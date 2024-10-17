using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class Revenue : UserTimeStamp 
    {
        public long Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; }

        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required]
        [StringLength(200)]
        public string NameTh { get; set; }
    }
}