using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class AcademicHonor : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }
        
        [Required]
        [StringLength(200)]
        public string NameTh { get; set; }

        [Required]
        public long AcademicLevelId { get; set; }
        public decimal Minimum { get; set; }
        public decimal Maximum { get; set; }

        [ForeignKey("AcademicLevelId")]
        public virtual AcademicLevel AcademicLevel { get; set; }

        [NotMapped]
        public string MinimumText => Minimum.ToString(StringFormat.TwoDecimal);

        [NotMapped]
        public string MaximumText => Maximum.ToString(StringFormat.TwoDecimal);
    }
}