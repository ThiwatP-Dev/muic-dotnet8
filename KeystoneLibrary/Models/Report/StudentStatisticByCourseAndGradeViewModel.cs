namespace KeystoneLibrary.Models.Report
{
    public class StudentStatisticByCourseAndGradeViewModel
    {
        public Criteria Criteria { get; set; }
        public List<string> GradeHeader { get; set; }
        public int GrandTotal => Courses.Sum(x => x.GrandTotal);
        public List<StudentStatisticByCourse> Courses { get; set; }
        public List<StudentStatisticByGrade> Grades { get; set; }
    }
    
    public class StudentStatisticByCourse
    {
        public string CourseName { get; set; }
        public int GrandTotal => Grades.Sum(x => x.StudentCount);
        public List<StudentStatisticByGrade> Grades { get; set; }
    }

    public class StudentStatisticByGrade
    {
        public string GradeName { get; set; }
        public int StudentCount { get; set; }
    }
}