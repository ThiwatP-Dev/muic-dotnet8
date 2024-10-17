namespace KeystoneLibrary.Models.Report
{
    public class StudentGraduationReportViewModel
    {
        public Criteria Criteria { get; set; }
        public List<StudentInformationViewModel> Results { get; set; }
    }
}