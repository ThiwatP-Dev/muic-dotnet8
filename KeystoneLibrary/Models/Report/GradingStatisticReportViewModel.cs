namespace KeystoneLibrary.Models.Report
{
    public class GradingStatisticReportViewModel
    {
        public Criteria Criteria { get; set; }
        public string Faculty { get; set; }
        public int TotalRecord { get; set; }
        public List<string> GradeHeaders { get; set; }
        public List<GradingStatisticByCourseViewModel> GradeByCourses { get; set; }
        public List<GradingStatisticDepartment> GradingStatisticDepartments { get; set; }
        public List<GradingStatisticReportCountViewModel> GradingStatisticReportCounts { get; set; } // Student Count from Course
        public GradingStatisticByCourseViewModel Footer { get; set; }
    }

    public class GradingStatisticByCourseViewModel
    {     
        public string Code { get; set; }
        public string Name { get; set; }
        public string Credit { get; set; }
        public int TotalStudentRegister { get; set; }
        public int TotalStudentPass { get; set; }
        public List<GradingStatisticReportCountViewModel> Grades { get; set; }
    }

    public class GradingStatisticDepartment
    {
        public string Department { get; set; }
        public List<GradingStatisticReportCountViewModel> GradingStatisticReportCounts { get; set; }
    }

    public class GradingStatisticReportCountViewModel
    {
        public string Grade { get; set; }
        public int StudentCount { get; set; }
        public decimal Percentage { get; set; }
        public string PercentageText => Percentage.ToString(StringFormat.TwoDecimal);
    }
}