using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class PrintingLogReportViewModel
    {
        public Criteria Criteria { get; set; }
        public PrintingLogReportStatistics PrintingLogReportStatistics { get; set; }
        public List<PrintingLog> PrintingLogReports { get; set; }
    }

    public class PrintingLogReportStatistics
    {
        public int All { get; set; }
        public int Male { get; set; }
        public int Female { get; set; }
        public int Undefined { get; set; }
        public int Normal { get; set; }
        public int Urgent { get; set; }
        public int English { get; set; }
        public int Thai { get; set; }
        public int Paid { get; set; }
        public int Unpaid { get; set; }
    }

    public class UpdateTrackingNumberViewModel
    {
        public long PrintingLogId { get; set; }
        public string TrackingNumber { get; set; }
    }
}