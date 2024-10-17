using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.USpark
{
    public class USparkCalculateFeeResponse
    {
        [NotMapped]
        public USparkOrder Result { get; set; }
        public string Message { get; set; }
    }
}