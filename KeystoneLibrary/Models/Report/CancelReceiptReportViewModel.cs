using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.Report
{
    public class CanceledReceiptReportViewModel
    {
        public string Code {get; set; }
        public string FullName { get; set; }
        public string ReceiptNumber { get; set; }

        [DisplayFormat(DataFormatString = "N2")]
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtText => CreatedAt.ToString(StringFormat.ShortDate);
        public string CreatedBy { get; set; }
        public DateTime CanceledAt { get; set; }
        public string CanceledAtText => CanceledAt.ToString(StringFormat.ShortDate);
        public string CanceledBy { get; set; }
    }
}