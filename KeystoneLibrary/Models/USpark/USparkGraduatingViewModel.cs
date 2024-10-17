using System.ComponentModel.DataAnnotations;
using KeystoneLibrary.Models.Report;

namespace KeystoneLibrary.Models.USpark
{
    public class USparkGraduatingViewModel
    {
        [JsonProperty("studentCode")]
        public string StudentCode { get; set; }

        [StringLength(100)]
        [JsonProperty("telephone")]
        public string Telephone { get; set; }

        [StringLength(100)]
        [JsonProperty("email")]
        public string Email { get; set; }

        [StringLength(10)]
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("expectedAcademicTerm")]
        public int ExpectedAcademicTerm { get; set; }

        [JsonProperty("expectedAcademicYear")]
        public int ExpectedAcademicYear { get; set; }

        [JsonProperty("requestedAt")]
        public DateTime? RequestedAt { get; set; }

        [StringLength(1000)]
        [JsonProperty("remark")]
        public string Remark { get; set; }

        [JsonProperty("courses")]
        public List<USparkCoursePredictionViewModel> Courses { get; set; }
    }

    public class USparkCoursePredictionViewModel
    {
        [JsonProperty("academicTerm")]
        public int AcademicTerm { get; set; }

        [JsonProperty("academicYear")]
        public int AcademicYear { get; set; }

        [JsonProperty("courseId")]
        public long CourseId { get; set; }
    }

    public class USparkGraduatingRequestViewModel
    {
        public long Id { get; set; }
        public string Status { get; set; }
        public int? ExpectedAcademicTerm { get; set; }
        public int? ExpectedAcademicYear { get; set; }
        public IEnumerable<USparkGraduatingTerm> Terms { get; set; }
        public IEnumerable<USparkGraduatingTermCourse> TermCourses { get; set; }
    }

    public class USparkGraduatingTerm
    {
        public int Year { get; set; }
        public int Term { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsSummer { get; set; }
    }

    public class USparkGraduatingCourse
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public int Credit { get; set; }
    }

    public class USparkGraduatingTermCourse : USparkGraduatingTerm
    {
        public IEnumerable<USparkGraduatingCourse> Courses { get; set; }
    }

    public class USparkCurriculum
    {
        public long Id { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionTh { get; set; }
        public int TotalCredit { get; set; }
        public int AcademicYear { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<CourseGroupViewModel> CourseGroups { get; set; }
    }
}