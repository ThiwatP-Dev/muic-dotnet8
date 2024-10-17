using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class StudentCertificate : UserTimeStamp
    {
       public long Id { get; set; }
        public Guid? StudentId { get; set; }

        [Required]
        [StringLength(20)]
        public string StudentCode { get; set; } // support alumni request
        public long TitleId { get; set; }

        [StringLength(200)]
        public string? FirstName { get; set; }

        [StringLength(200)]
        public string? LastName { get; set; }

        [StringLength(200)]
        public string? Certificate { get; set; } // from certificate model
        public long TermId { get; set; }

        [StringLength(10)]
        public string? Channel { get; set; } // web, app

        [StringLength(10)]
        public string? Language { get; set; } // en, th, both
        public int Amount { get; set; }
        public long DistributionMethodId { get; set; }

        [StringLength(320)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? TelephoneNumber { get; set; }

        [StringLength(1000)]
        public string? Address { get; set; }

        [StringLength(5000)]
        public string? Request { get; set; }

        [StringLength(5000)]
        public string? Remark { get; set; }
        public bool IsPaid { get; set; }

        [Required]
        [StringLength(10)]
        public string Status { get; set; } // r = request, a = accept, j = reject
        
        [JsonIgnore]
        [ForeignKey("StudentId")]
        public virtual Student? Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("TitleId")]
        public virtual Title Title { get; set; }

        [ForeignKey("DistributionMethodId")]
        public virtual DistributionMethod DistributionMethod { get; set; }

        [NotMapped]
        public string Code => Student?.Code ?? "";

        [NotMapped]
        public string TitleText => Language == "en" ? Title?.NameEn ?? "" : Title?.NameTh ?? "";

        [NotMapped]
        public string FullName => $"{ TitleText } { FirstName } { LastName }";

        [NotMapped]
        public string LanguageText => Language == "en" ? "English" : "Thai";

        [NotMapped]
        public string CertificateText => string.IsNullOrEmpty(Certificate)
                                         ? "" : ((CertificateEnum)Enum.Parse(typeof(CertificateEnum), Certificate)).GetDisplayName();

        [NotMapped]
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case "r":
                        return "Request";
                    case "a":
                        return "Accept";
                    case "j":
                        return "Reject";
                    default:
                        return "N/A";
                }
            }
        }

        [NotMapped]
        public string ChannelText
        {
            get
            {
                switch (Channel)
                {
                    case "w":
                        return "Web";
                    case "a":
                        return "Application";
                    default:
                        return "N/A";
                }
            }
        }
    }
}