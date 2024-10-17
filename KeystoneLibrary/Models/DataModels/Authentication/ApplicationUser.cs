using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string? FirstnameTH { get; set; }

        [StringLength(100)]
        public string? LastnameTH { get; set; }

        [StringLength(100)]
        public string? FirstnameEN { get; set; }
        
        [StringLength(100)]
        public string? LastnameEN { get; set; }
        public long? InstructorId { get; set; }
        public bool IsActive { get; set; }

        [ForeignKey("InstructorId")]
        public virtual Instructor? Instructor { get; set; }

        [NotMapped]
        public string Role { get; set; }

        [NotMapped]
        public string FullnameTH => $"{FirstnameTH} {LastnameTH}";

        [NotMapped]
        public string FullnameEN => $"{FirstnameEN} {LastnameEN}";

        [NotMapped]
        public string ReportNameEn
        {
            get
            {
                var lastname = String.IsNullOrEmpty(LastnameEN) ? string.Empty : $"{ LastnameEN.Substring(0, 1) }.";
                return $"{ NormalizedUserName } { lastname }";
            }
        }
    }
}