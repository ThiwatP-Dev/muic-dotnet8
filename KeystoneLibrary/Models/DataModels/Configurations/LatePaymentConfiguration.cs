using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Configurations
{
    public class LatePaymentConfiguration : UserTimeStamp
    {
        public long Id { get; set; }
        public long FromTermId { get; set; }
        public long? ToTermId { get; set; }
        public decimal AmountPerDay { get; set; }
        public int MaximumDays { get; set; }

        [ForeignKey("FromTermId")]
        public virtual Term FromTerm { get; set; }

        [ForeignKey("ToTermId")]
        public virtual Term ToTerm { get; set; }
    }
}