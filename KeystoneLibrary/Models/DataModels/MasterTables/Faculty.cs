using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class Faculty : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; }

        [Required]
        [StringLength(500)]
        public string NameTh { get; set; }

        [Required]
        [StringLength(500)]
        public string NameEn { get; set; }

        [StringLength(200)]
        public string? ShortNameTh { get; set; }

        [StringLength(200)]
        public string? ShortNameEn { get; set; }

        [StringLength(50)]
        public string? Abbreviation { get; set; }

        [StringLength(200)]
        public string? FormalNameEn { get; set; } // ex. Bachelor of 'Arts'

        [StringLength(200)]
        public string? FormalNameTh { get; set; }

        [StringLength(2100)]
        public string? LogoURL { get; set; }

        public virtual ICollection<Department> Departments { get; set; }

        [NotMapped]
        public string CodeAndName => $"{ Code } - { NameEn }";

        [NotMapped]
        public string CodeAndShortName => $"{ Code } - { ShortNameEn }";

        [NotMapped]
        public string CodeAndNameTh => $"{ Code } - { NameTh }";
    }
}