namespace KeystoneLibrary.Models.Report
{
    public class DemandStudentByCourseViewModel
    {
        public Criteria Criteria { get; set; }
        public List<DemandStudentByCourse> Results { get; set; }
    }

    public class DemandStudentByCourse
    {
        public string CurriculumVersionName { get; set; }
        public int Batch { get; set; }
        public bool IsRegistered { get; set; }
        public bool IsPassed { get; set; }
    }
}