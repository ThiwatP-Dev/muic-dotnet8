using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class RegistrationSlotCondition
    {
        public long Id { get; set; }
        public long SlotId { get; set; }
        public long RegistrationConditionId { get; set; }
        
        [ForeignKey("SlotId")]
        public virtual Slot Slot { get; set; }

        [ForeignKey("RegistrationConditionId")]
        public virtual RegistrationCondition RegistrationCondition { get; set; }
    }
}