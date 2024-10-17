namespace KeystoneLibrary.Models.Report
{
    public class StudentStatusStatisticViewModel
    {
        public Criteria Criteria { get; set; }
        public List<TermDetails> TermHeader { get; set; }
        public List<StudentStatusStatisticDeatils> Students { get; set; }
    }
    public class StudentStatusStatisticDeatils
    {
        public string Faculty { get; set; }
        public int NumberOfStudentInFaculty { get; set; }
        public List<StudentStatusStatisticDepartment> StudentStatusStatisticDepartments { get; set; }
        public List<StudentStatusStatisticReportCount> TotalStudentInTerm { get; set; }
    }

    public class StudentStatusStatisticDepartment
    {
        
        public string Department { get; set; }
        public string Code { get; set; }
        public int NumberOfStudentInDepartment { get; set; }
        public List<StudentStatusStatisticReportCount> StudentStatusStatisticReportCounts { get; set; }
    }

    public class StudentStatusStatisticReportCount
    {
        public int Year { get; set; }
        public int Term { get; set; }
        public int NumberOfStudent { get; set; }
    }

    public class TermDetails
    {
        public int Year { get; set; }
        public List<int> Terms { get; set; }
    }
}