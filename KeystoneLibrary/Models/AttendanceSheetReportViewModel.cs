using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models.Report
{
    public class AttendanceSheetReportViewModel
    {
        public Criteria Criteria { get; set; }
        public long SectionId { get; set; }
        public string InstructorCode { get; set; }
        public string InstructorFullName { get; set; }
        public string SubjectCodeAndName { get; set; }
        public string SectionNumber { get; set; }
        public string AcademicTerm { get; set; }
        public string AcademicYear { get; set; }
        public string AcademicNextYear { get; set; }
        public int TotalSectionDetail { get; set; } = 12;
        public string GeneratedDate { get; set; } = DateTime.Now.ToShortDateString();
        public string Credit { get; set; }
        public Instructor MainInstructor { get; set; }
        public List<AttendanceStudent> StudentList { get; set; } = new List<AttendanceStudent>();
        public int PageCount => (int)Math.Ceiling((decimal)StudentList.Count / 25);

    }

    public class AttendanceStudent
    {
        public string StudentCode { get; set; }
        public string ProfileImageURL { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MidName { get; set; }
        public string CourseCode { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public bool IsPaid { get; set; }
        public string PaidStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtText => (CreatedAt == new DateTime()) ? "-" : CreatedAt.ToString(StringFormat.ShortDate);
        public string FullNameEn => string.IsNullOrEmpty(MidName) ? $"{ Title } { FirstName } { LastName }"
                                                                  : $"{ Title } { FirstName } { MidName } { LastName }";
    }
}