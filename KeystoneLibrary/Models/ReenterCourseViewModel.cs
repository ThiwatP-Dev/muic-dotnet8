namespace KeystoneLibrary.Models
{
    public class ReenterCourseViewModel
    {
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public string CurriculumVersion { get; set; }
        public int TransferedCredit { get; set; }
        public long TransferedTermId { get; set; }
        public string TransferedGrade { get; set; }
        public Criteria Criteria { get; set; }
        public List<TransferCourseViewModel> TransferCourses { get; set; }
    }

    public class TransferCourseViewModel
    {
        public long RegistrationCourseId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Term { get; set; }
        public int AcademicYear { get; set; }
        public int AcademicTerm { get; set; }
        public int Credit { get; set; }
        public string Grade { get; set; }
    }
}