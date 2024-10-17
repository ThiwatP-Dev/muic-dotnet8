using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.DataModels
{
    public class InstallmentTransaction : UserTimeStamp
    {
        public long Id { get; set; }
        public long InstallmentId { get; set; }
        public decimal TotalAmount { get; set; }

        [StringLength(10)]
        public string? Channel { get; set; }
        public bool IsPaid { get; set; }
        public bool IsTransfered { get; set; }
        public long? RegistrationCourseId { get; set; }
        public DateTime? PaidAt { get; set; }

        [ForeignKey("InstallmentId")]
        public virtual Installment Installment { get; set; }

        [ForeignKey("RegistrationCourseId")]
        public virtual RegistrationCourse? RegistrationCourse { get; set; }

    }
}