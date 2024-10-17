using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class Percentage : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        public decimal Value { get; set; }
    }
}