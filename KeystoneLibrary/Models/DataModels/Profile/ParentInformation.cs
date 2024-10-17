using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class ParentInformation : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }

        [StringLength(200)]
        public string? FirstNameEn { get; set; }

        [StringLength(200)]
        public string? MidNameEn { get; set; }

        [StringLength(200)]
        public string? LastNameEn { get; set; }

        [StringLength(200)]
        public string? FirstNameTh { get; set; }

        [StringLength(200)]
        public string? MidNameTh { get; set; }

        [StringLength(200)]
        public string? LastNameTh { get; set; }
        public long RelationshipId { get; set; }

        [StringLength(100)]
        public string? CitizenNumber { get; set; }

        [StringLength(100)]
        public string? Passport { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(5)] 
        public string? LivingStatus { get; set; }
        public long? RevenueId { get; set; }
        public long? OccupationId { get; set; }
        public bool MailToParent { get; set; }
        public bool EmailToParent { get; set; }
        public bool SMSToParent { get; set; }
        public bool IsMainParent { get; set; }
        public bool IsEmergencyContact { get; set; }

        [Required]
        [StringLength(20)]
        public string TelephoneNumber1 { get; set; }

        [StringLength(20)]
        public string? TelephoneNumber2 { get; set; }

        [StringLength(500)]
        public string? AddressTh { get; set; }

        [StringLength(500)]
        public string? AddressEn { get; set; }
        public long? CountryId { get; set; }
        public long? ProvinceId { get; set; }
        public long? DistrictId { get; set; }
        public long? SubdistrictId { get; set; }
        public long? CityId { get; set; }
        public long? StateId { get; set; }

        [StringLength(20)]
        public string? ZipCode { get; set; }

        [JsonIgnore]
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("CountryId")]
        public virtual Country? Country { get; set; }

        [ForeignKey("ProvinceId")]
        public virtual Province? Province { get; set; }

        [ForeignKey("DistrictId")]
        public virtual District? District { get; set; }

        [ForeignKey("SubdistrictId")]
        public virtual Subdistrict? Subdistrict { get; set; }

        [ForeignKey("CityId")]
        public virtual City? City { get; set; }

        [ForeignKey("StateId")]
        public virtual State? State { get; set; }

        [ForeignKey("RelationshipId")]
        public virtual Relationship Relationship { get; set; }

        [ForeignKey("RevenueId")]
        public virtual Revenue? Revenue { get; set; }

        [ForeignKey("OccupationId")]
        public virtual Occupation? Occupation { get; set; }

        [NotMapped]
        public string TelephoneText => String.IsNullOrEmpty(TelephoneNumber2)
                                       ? TelephoneNumber1 : $"{ TelephoneNumber1}, { TelephoneNumber2 }";

        [NotMapped]
        public string FullNameEn => string.IsNullOrEmpty(MidNameEn) ? $"{ FirstNameEn } { LastNameEn }"
                                                                    : $"{ FirstNameEn } { MidNameEn } { LastNameEn }";

        [NotMapped]
        public string FullNameTh => string.IsNullOrEmpty(MidNameTh) ? $"{ FirstNameTh } { LastNameTh }"
                                                                    : $"{ FirstNameTh } { MidNameTh } { LastNameTh }";
    }
}