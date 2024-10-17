using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class StudentAddress : UserTimeStamp
    {
        private string DefaultString = "";
        private string? _housenumber;
        private string? _moo;
        private string? _soiTh;
        private string? _roadTh;
        private string? _soiEn;
        private string? _roadEn;
        public long Id { get; set; }
        public Guid StudentId { get; set; }

        [StringLength(50)]
        public string? HouseNumber
        { 
            get
            {
                return _housenumber ?? DefaultString;
            }
            set
            {
                _housenumber = value;
            }
        }

        [StringLength(500)]
        public string? AddressTh1 { get; set; }

        [StringLength(500)]
        public string? AddressTh2 { get; set; }

        [StringLength(500)]
        public string? AddressEn1 { get; set; }

        [StringLength(500)]
        public string? AddressEn2 { get; set; }

        [StringLength(50)]
        public string? Moo
        {
            get
            {
                return _moo ?? DefaultString;
            }
            set
            {
                _moo = value;
            }
        }

        [StringLength(200)]
        public string? SoiTh
        {
            get
            {
                return _soiTh ?? DefaultString;
            }
            set
            {
                _soiTh = value;
            }
        }

        [StringLength(200)]
        public string? RoadTh
        {
            get
            {
                return _roadTh ?? DefaultString;
            }
            set
            {
                _roadTh = value;
            }
        }

        [StringLength(200)]
        public string? SoiEn
        {
            get
            {
                return _soiEn ?? DefaultString;
            }
            set
            {
                _soiEn = value;
            }
        }

        [StringLength(200)]
        public string? RoadEn
        {
            get
            {
                return _roadEn ?? DefaultString;
            }
            set
            {
                _roadEn = value;
            }
        }
        
        public long CountryId { get; set; }
        public long? ProvinceId { get; set; }
        public long? DistrictId { get; set; }
        public long? SubdistrictId { get; set; }
        public long? CityId { get; set; }
        public long? StateId { get; set; }

        [Required]
        [StringLength(20)]
        public string ZipCode { get; set; }

        [StringLength(20)]
        public string? TelephoneNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string Type { get; set; } // Permanent, Current

        [JsonIgnore]
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; }

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

        [NotMapped]
        //public string AddressEn => $"{ AddressEn1 } { (string.IsNullOrEmpty(AddressEn2) ? "" : AddressEn2) } { HouseNumber } Soi { SoiEn } { RoadEn }";
        public string AddressEn => $"{AddressEn1} {(string.IsNullOrEmpty(AddressEn2) ? "" : AddressEn2)}";

        [NotMapped]
        //public string AddressTh => $"{ AddressTh1 } { (string.IsNullOrEmpty(AddressTh2) ? "" : AddressTh2) } { HouseNumber } Soi { SoiTh } { RoadTh }";
        public string AddressTh => $"{AddressTh1} {(string.IsNullOrEmpty(AddressTh2) ? "" : AddressTh2)}";
    }
}