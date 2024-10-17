namespace KeystoneLibrary.Models
{
    public class GradeApprovalViewModel
    {
        public Criteria Criteria { get; set; }
        public List<GradeApprovalDetail> Details { get; set; }
    }

    public class GradeApprovalDetail
    {
        public long BarcodeId { get; set; }
        public long SectionId { get; set; }
        public long CourseId { get; set; }
        public string Term { get; set; }
        public long TermId { get; set; }
        public string CourseCode { get; set; }
        public string SectionNumber { get; set; }
        public string JointSectionIds { get; set; }
        public string JointSection { get; set; }
        public string BarcodeNumber { get; set; }
        public string Status { get; set; }
        public int GradeEnteredStudent { get; set; }
        public int SpecifyGradeStudent { get; set; }
        public int NoScoreStudent { get; set; }
        public int PublishedStudent { get; set; }
        public int ApprovaledStudent { get; set; }
        public int WithdrawnStudent { get; set; }
        public int SectionStudent { get; set; }
        public decimal Mean { get; set; }
        public decimal Max { get; set; }
        public decimal Min { get; set; }
        public decimal SD { get; set; }
        public bool IsPublished { get; set; }
        public bool IsChecked { get; set; }
        public bool IsApproved { get; set; }
        public string MeanText => Mean.ToString(StringFormat.TwoDecimal);
        public string MaxText => Max.ToString(StringFormat.TwoDecimal);
        public string MinText => Min.ToString(StringFormat.TwoDecimal);
        public string SDText => SD.ToString(StringFormat.TwoDecimal);
        public List<long> JointSectionIdList { get; set; } = new List<long>();
        public int NumberValue
        {
            get
            {
                int number;
                bool success = Int32.TryParse(SectionNumber, out number);
                return success ? number : 0;
            }
        }
    }
}