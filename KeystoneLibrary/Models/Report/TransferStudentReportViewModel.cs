using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.Report
{
    public class TransferStudentReportViewModel
    {
        public Criteria Criteria { get; set; }
        public List<StudentTransferLog> Results { get; set; }
    }
}