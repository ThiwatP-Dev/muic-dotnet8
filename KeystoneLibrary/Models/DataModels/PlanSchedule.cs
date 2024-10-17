using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace KeystoneLibrary.Models.DataModels
{
    public class PlanSchedule : UserTimeStamp
    {
        public long Id { get; set; }
        public long PlanId { get; set; }

        [Required]
        [StringLength(100)]
        public string SectionIds { get; set; }
        
        [ForeignKey("PlanId")]
        public virtual Plan Plan { get; set; }
    }
}