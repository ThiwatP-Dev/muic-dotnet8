namespace KeystoneLibrary.Models
{
    public class CloseSectionStudentList
    {
        public string StudentCode { get; set; }
        public string StudentTitle { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentLastNameName { get; set; }
        public string CourseCode { get; set; }
        public string SectionNumber { get; set; }
        public string SectionType { get; set; }
        public string Division { get; set; }
        public string Major { get; set; }
        public string Email { get; set; }
        public bool PaymentStatus { get; set; }
        public string StudentFullName => $"{ StudentTitle } { StudentFirstName } { StudentLastNameName }";
        public string FullCourse => $"{ CourseCode } ({ SectionNumber }) { SectionType }";
    }
}