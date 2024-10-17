namespace KeystoneLibrary.Models.Report
{
    public class StudentAdvisingCourseViewModel
    {
        public Criteria Criteria { get; set; }
        public List<StudentAdvisingCourse> Results { get; set; }
        public int RowCount => Results== null ? 0 : Results.Count();
    }

    public class StudentAdvisingCourse
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int Count { get; set; }
        public string CountText => Count.ToString(StringFormat.NumberString);
    }
}