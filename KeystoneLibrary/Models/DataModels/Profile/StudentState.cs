using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    //เอาไว้เก็บ state ของ นศ ว่าลงทะเบียนถึงขึ้นตอนไหนแล้ว 
    public class StudentState : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public long TermId { get; set; }

        [Required]
        public string State { get; set; }
        
        [JsonIgnore]
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [JsonIgnore]
        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [NotMapped]
        public string StateText
        {
            get
            {
                switch (State)
                {
                    case "REG": //นศ. สามารถลงทะเบียนได้
                        return "Registration";
                    case "PAY_REG": //นศ. สามารถชำระเงินค่าลงทะเบียนได้
                        return "Payment for Registration";
                    case "ADD": // นศ. สามารถ Add / Drop ได้
                        return "Add/Drop";
                    case "PAY_ADD": // นศ. สามารถชำระเงินค่า Add / Drop ได้
                        return "Payment for Add/Drop";
                    default:
                        return "N/A";
                }
            }
        }
    }
}