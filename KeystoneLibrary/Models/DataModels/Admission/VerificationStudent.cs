using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels.Admission
{
    public class VerificationStudent
    {
        public long VerificationLetterId { get; set; }
        public Guid StudentId { get; set; }

        [JsonIgnore]
        public virtual VerificationLetter VerificationLetter { get; set; }

        [JsonIgnore]
        public virtual Student Student { get; set; }

        [NotMapped]
        public string IsChecked { get; set; }

        [NotMapped]
        public string PreviousSchoolNameEn { get; set; }

        [NotMapped]
        public string PreviousSchoolNameTh { get; set; }
    }
}