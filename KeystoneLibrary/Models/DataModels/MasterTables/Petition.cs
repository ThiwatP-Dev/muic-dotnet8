using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class Petition : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required]
        [StringLength(200)]
        public string NameTh { get; set; }
        public bool IsRequiredPayment { get; set; }
        public decimal? DefaultPrice { get; set; }

        [NotMapped]
        public string DefaultPriceText => DefaultPrice?.ToString(StringFormat.TwoDecimal) ?? "";
    }
}