using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class AcademicLevel : UserTimeStamp
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

        [StringLength(200)]
        public string? FormalNameEn { get; set; } // 'Bachelor', 'Master', etc.

        [StringLength(200)]
        public string? FormalNameTh { get; set; }
        
        [NotMapped]
        public string Abbreviation => $"{ NameEn.ToUpper().Substring(0,1) }";
    }
}