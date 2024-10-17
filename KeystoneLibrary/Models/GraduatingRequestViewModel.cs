using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Graduation;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.Report;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models
{
    public class GraduatingRequestViewModel
    {
        public long GraduatingRequestId { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string CurriculumVersion { get; set; }
        public long CourseGroupingLogId { get; set; }
        public long CurriculumVersionId { get; set; }
        public Student Student { get; set; }
        public List<SpecializationGroup> SpecializationGroups { get; set; }
        public GraduatingRequest GraduatingRequest { get; set; }
        public GraduationInformation GraduationInformation { get; set; } = new GraduationInformation();
        public string CurrentTerm { get; set; }
        public List<RegistrationCourse> RegistrationCourses { get; set; } = new List<RegistrationCourse>();
        public List<GraduatingRequestLog> GraduatingRequestLogs { get; set; } = new List<GraduatingRequestLog>();
        public List<CoursePrediction> CoursePredictions { get; set; } = new List<CoursePrediction>();
        public List<CourseGroupingLog> CourseGroupingLogs { get; set; } = new List<CourseGroupingLog>();
        public CourseGroupingLog CourseGroupingLog { get; set; } = new CourseGroupingLog();
        public List<CourseGroupingCreateViewModel> CourseGroupingCreates { get; set; }
        public List<CourseGroupingDetailViewModel> CourseGroups { get; set; } = new List<CourseGroupingDetailViewModel>();
        public bool IsPublish { get; set; }
        public bool IsPrint { get; set; }
        public List<CourseGroupViewModel> CurriculumCourseGroups { get; set; }
        public List<CourseGroupViewModel> CurriculumCourseGrouping { get; set; }
        public CourseGroupViewModel OtherCourseGroups { get; set; }
        public List<CourseGroupViewModel> GroupingLogRegistrations { get; set; }
        public int TotalCourseGroup { get; set; }
        public int TotalCurriculumVersionCredit { get; set; }

        public string Status { get; set; }
    }

    public class CourseGroupingDetailViewModel
    {
        public long CourseGroupId { get; set; }
        public string CourseGroupName { get; set; }
        public List<CourseGroupingCreateViewModel> Courses { get; set; }
    }

    public class GraduationHonor
    {
        public string IsChecked { get; set; }
        public string Code { get; set; }
        public string Remark { get; set; }
        public bool IsPassed { get; set; }
    }

    public class GraduatingRequestExcelViewModel : StudentInformationViewModel
    {
        public long? GraduatingRequestId { get; set; }
        public string RequestStatus { get; set; }
        public DateTime? RequestedDate { get; set; }
        public DateTime? GraduatedAt { get; set; }
        public string GradeatedTerm { get; set; }
        public int? ExpectedAcademicTerm { get; set; }
        public int? ExpectedAcademicYear { get; set; }

        public string RequestDateText => $"{ RequestedDate?.ToString(StringFormat.ShortDate) }";
        public string GraduatedAtText => $"{ GraduatedAt?.ToString(StringFormat.ShortDate) }";
        public string ExpectedTermText => ExpectedAcademicTerm.HasValue ? ExpectedAcademicTerm + "/" + ExpectedAcademicYear.ToString().Substring(2) + "-" + (ExpectedAcademicYear + 1).ToString().Substring(2): "-";
        public string RequestStatusText
        {
            get
            {
                switch (RequestStatus)
                {
                   case "w":
                        return "Submitted";
                    case "a":
                        return "Accepted";
                    case "p":
                        return "Checking in progress";
                    case "c":
                        return "Completed";
                    case "r":
                        return "Rejected";
                    case "t":
                        return "Returned";
                    default:
                        return "N/A";
                }
            }
        }
    }
}