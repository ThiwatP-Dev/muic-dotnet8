using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Admission
{
    public class Agency : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Address1 { get; set; }

        [StringLength(500)]
        public string? Address2 { get; set; }

        [StringLength(500)]
        public string? Address3 { get; set; }

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        [StringLength(200)]
        public string? OfficerName { get; set; }

        [StringLength(59)]
        public string? MobileNumber { get; set; }

        [StringLength(320)]
        public string? Email { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }
        public long? CountryId { get; set; }

        [ForeignKey("CountryId")]
        public virtual Country? Country { get; set; }

        public virtual List<AgencyContract> AgencyContracts { get; set; }
    }
}