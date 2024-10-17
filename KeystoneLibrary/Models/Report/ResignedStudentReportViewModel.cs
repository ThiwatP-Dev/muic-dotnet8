namespace KeystoneLibrary.Models.Report
{
    public class ResignStatisticReportViewModel
    {
        public Criteria Criteria { get; set; }
        public List<ResignStatisticReport> Results { get; set; }
        public List<int> Terms { get; set; }
    }

    public class ResignStatisticReport
    {
        public string FacultyAbbreviation { get; set; }
        public string DepartmentAbbreviation { get; set; }
        public string DepartmentName { get; set; }
        public int AcademicYear { get; set; }
        public int AcademicTerm { get; set; }
        public int Batch { get; set; }
    }
}