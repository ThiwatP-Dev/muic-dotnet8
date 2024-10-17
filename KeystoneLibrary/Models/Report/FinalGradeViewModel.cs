namespace KeystoneLibrary.Models
{
    public class FinalGradeViewModel
    {
        public Criteria Criteria { get; set; }
        public List<FinalGradeDetail> FinalGradeDetails { get; set; }
    }
    public class FinalGradeDetail
    {
        public string Term {get; set; }
        public string CourseCode { get; set; }
        public string SectionNumber {get; set;}
        public List<FinalGradeStudentDetail> Students {get; set;}
        public string BarcodeNumber { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
    }

    public class FinalGradeStudentDetail
    {
        public string Code { get; set; }
        public string Grade { get; set; }
        public string FullName { get; set; }
    }

    public class CourseCodeRangeViewModel
    {
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public string SubStringCode { get; set; }
        public int SubStringCodeInt 
        {
            get
            {
                int code;
                bool success = Int32.TryParse(SubStringCode, out code);
                return success ? code : 0;
            }
        }
    }
}