namespace KeystoneLibrary.Models
{
    public class StudentChangeCurriculumViewModel
    {
        public Criteria Criteria { get; set; }
        public List<CurriculumStudent> CurriculumStudents { get; set; }
    }

    public class CurriculumStudent
    {
        public Guid StudentId { get; set; }
        public string Code { get; set; }
        public string FullNameEn { get; set; }
        public decimal GPA { get; set; }
        public int CreditComp { get; set; }
        public string Email { get; set; }
    }
}