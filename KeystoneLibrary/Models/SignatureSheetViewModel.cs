namespace KeystoneLibrary.Models
{
    public class SignatureSheetViewModel
    {
        public Criteria Criteria { get; set; }
        public List<SignatureSheetDetail> SignatureSheetDetails { get; set; }
    }
    public class SignatureSheetDetail
    {
        public long SectionId { get; set; }
        public string SectionNumber { get; set; }
        public int AcademicTerm { get; set; }
        public int AcademicYear { get; set; }
        public string Credit { get; set; }
        public string SubjectCodeAndName { get; set; }
        public string GeneratedDate { get; set; } = DateTime.Now.ToShortDateString();
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string OpenedDate { get; set; }
        public string IntructorFullNameEn { get; set; }
        public List<SignatureSheetStudentDetail> Students { get; set; } = new List<SignatureSheetStudentDetail>();
        public int PageCount => (int)Math.Ceiling((decimal)Students.Count / 25);
    }

    public class SignatureSheetStudentDetail
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string LastName { get; set; }
        public string MidName { get; set; }
        public string FirstName { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string CourseCode { get; set; }
        public string PaidStatus { get; set; }
        public string WithdrawnStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtText => (CreatedAt == new DateTime()) ? "-" : CreatedAt.ToString(StringFormat.ShortDate);
        public string Name => string.IsNullOrEmpty(MidName) ? $"{ Title } { FirstName } { LastName }"
                                                            : $"{ Title } { FirstName } { MidName } { LastName }";
    }
}