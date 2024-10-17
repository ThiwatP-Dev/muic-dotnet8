using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class StudentPetition : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long PetitionId { get; set; }
        public long TermId { get; set; }

        [StringLength(10)]
        public string? Channel { get; set; } // web, app

        [StringLength(320)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? TelephoneNumber { get; set; }

        [StringLength(5000)]
        public string? Request { get; set; }

        [StringLength(5000)]
        public string? Remark { get; set; }

        [Required]
        [StringLength(10)]
        public string Status { get; set; } // r = request, a = accept, j = reject
        
        [JsonIgnore]
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("PetitionId")]
        public virtual Petition Petition { get; set; }
        public virtual List<PetitionLog> PetitionLogs { get; set; }

        [NotMapped]
        public string Code => Student?.Code ?? "";

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
