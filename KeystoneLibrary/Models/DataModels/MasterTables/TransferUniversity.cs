using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class TransferUniversity : UserTimeStamp
    {
        public long Id { get; set; }

        [StringLength(50)]
        public string? OHECCode { get; set; } // Office of the Higher Education Commission

        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required]
        [StringLength(200)]
        public string NameTh { get; set; }
        public long CountryId { get; set; }

        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; }

        [JsonIgnore]
        public virtual List<Course> Courses {get; set; }

        [NotMapped]
        public long AcademicLevelId { get; set; }
    }
}