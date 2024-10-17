using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Fee
{
    public class FeeGroup : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }
        
        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required]
        [StringLength(200)]
        public string NameTh { get; set; }
        public bool ShownDetails  { get; set; }

        [NotMapped]
        public string CodeAndName
        { 
            get 
            {
                return $"{ Code } - { NameEn }"; 
            }
        }
    }   
}