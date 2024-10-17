using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Configurations
{
    public class AddDropFeeConfiguration : UserTimeStamp
    {
        public long Id { get; set; }
        public long FromTermId { get; set; }
        public long? ToTermId { get; set; }
        public decimal Amount { get; set; }
        public int FreeAddDropCount { get; set; } // Allow student to do Add/Drop for free for how many times. (1 time = 1 payment during Add/Drop period) 

        [ForeignKey("FromTermId")]
        public virtual Term FromTerm { get; set; }

        [ForeignKey("ToTermId")]
        public virtual Term ToTerm { get; set; }
    }
}