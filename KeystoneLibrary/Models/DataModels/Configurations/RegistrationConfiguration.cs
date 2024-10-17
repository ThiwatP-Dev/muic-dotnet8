using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Configurations
{
    public class RegistrationConfiguration : UserTimeStamp
    {
        public long Id { get; set; }
        public long FromTermId { get; set; }
        public long? ToTermId { get; set; }
        public int MininumCreditToWithdraw { get; set; }
        public int RegistrationTime { get; set; }
        public bool IsPaymentAllowed { get; set; }
        public bool IsRegistrationAllowed { get; set; }

        [ForeignKey("FromTermId")]
        public virtual Term FromTerm { get; set; }

        [ForeignKey("ToTermId")]
        public virtual Term ToTerm { get; set; }

        [NotMapped]
        public long AcademicLevelId { get; set; }
    
    }
}