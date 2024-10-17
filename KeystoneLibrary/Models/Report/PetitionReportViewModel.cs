namespace KeystoneLibrary.Models.Report
{
    public class PetitionReportViewModel
    {
        public Criteria Criteria { get; set; }
        public List<PetitionReportDetail> PetitionReportDetails { get; set; }
    }

    public class PetitionReportDetail
    {
        public string PetitionName { get; set; }
        public long PetitionId { get; set; }
        public int Request { get; set; }
        public int Accept { get; set; }
        public int Reject { get; set; }
        public int Total { get; set; }
    }
}