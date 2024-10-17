namespace KeystoneLibrary.Models
{
    public class StudentStatisticByProvinceAndSchoolReportViewModel
    {
        public string Territory { get; set; }
        public string Province { get; set; }
        public string SchoolName { get; set; }
        public Criteria Criteria { get; set; }
        public List<StudentStatisticByProvinceAndSchoolDetail> RegistrationStudentCounts { get; set; }
        public List<StudentStatisticByProvinceAndSchoolDetail> IntensiveStudentCounts { get; set; }
        public List<StudentStatisticByProvinceAndSchoolDetail> StudyStudentCounts { get; set; }
    }

    public class  StudentStatisticByProvinceAndSchoolDetail
    {
        public int Batch { get; set; }
        public int StudentCount { get; set; }
    }
}