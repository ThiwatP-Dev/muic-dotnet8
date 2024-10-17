namespace KeystoneLibrary.Models
{
    public class ReenterViewModel
    {
        public Criteria Criteria { get; set; }
        public List<StudentViewModel> StudentDetails { get; set; }
    }

    public class StudentViewModel
    {
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string Type { get; set; }
        public string CurrentStudentCode { get; set; }
    }
}