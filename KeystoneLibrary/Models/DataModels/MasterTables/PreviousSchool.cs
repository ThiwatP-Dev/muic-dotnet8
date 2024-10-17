using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class PreviousSchool : UserTimeStamp
    {
        public long Id { get; set; }

        [StringLength(50)]
        public string? Code { get; set; }

        [StringLength(50)]
        public string? OHECCode { get; set; } // Office of the Higher Education Commission

        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required]
        [StringLength(200)]
        public string NameTh { get; set; }
        public long CountryId { get; set; }
        public long? StateId { get; set; }
        public long? ProvinceId { get; set; }
        public long? SchoolTypeId { get; set; }
        public long? SchoolTerritoryId { get; set; }
        public long? SchoolGroupId { get; set; }

        [StringLength(500)]
        public string? Address1 { get; set; } // Address for send letter

        [StringLength(500)]
        public string? Address2 { get; set; }

        [StringLength(500)]
        public string? Address3 { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; }

        [ForeignKey("StateId")]
        public virtual State? State { get; set; }

        [ForeignKey("ProvinceId")]
        public virtual Province? Province { get; set; }

        [ForeignKey("SchoolTypeId")]
        public virtual SchoolType? SchoolType { get; set; }

        [ForeignKey("SchoolTerritoryId")]
        public virtual SchoolTerritory? SchoolTerritory { get; set; }

        [ForeignKey("SchoolGroupId")]
        public virtual SchoolGroup? SchoolGroup { get; set; }

        [JsonIgnore]
        public virtual List<AdmissionInformation> AdmissionInformations {get; set; }
    }
}