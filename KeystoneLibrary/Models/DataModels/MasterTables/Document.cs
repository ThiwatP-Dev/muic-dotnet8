using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class Document : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }
        
        [StringLength(200)]
        public string? NameTh { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }

        [StringLength(2083)]
        public string? ExampleImageUrl { get; set; }
    }
}