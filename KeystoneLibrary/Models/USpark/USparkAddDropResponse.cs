using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.USpark
{
    public class USparkAddDropResponse
    {
        [NotMapped]
        public List<USparkOrder> Result { get; set; }
        public string Message { get; set; }
    }
}