using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models
{
    public class ExemptedExaminationScoreViewModel
    {
        public Criteria Criteria { get; set; }
        public List<ExemptedExaminationScoreDetail> ExemptedExaminationScoreDetails { get; set; }
    }

    public class ExemptedExaminationScoreDetail
    {
        public long ExemptedExaminationId { get; set; }
        public string ExemptedExaminationName { get; set; }
        public long AcademicLevelId { get; set; }
        public long AdmissionTypeId { get; set; }
        public string AdmissionTypeName { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? IssuedDate { get; set; } = DateTime.Now;
        public string IssuedDateInput { get; set; }
        public string IssuedDateString => IssuedDate?.ToString(StringFormat.ShortDate);

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ExpiredDate { get; set; } = DateTime.Now;
        public string ExpiredDateInput { get; set; }
        public string ExpiredDateString => ExpiredDate?.ToString(StringFormat.ShortDate);
        public string FacultyName { get; set; }
        public string DepartmentName { get; set; }
        public List<PreferredCourse> PreferredCourses { get; set; }
        public List<AffectedFacultyDepartment> AffectedFacultyDepartments { get; set; }
        public bool IsActive { get; set; }
    }

    public class PreferredCourse
    {
        public decimal MinScore { get; set; }
        public decimal MaxScore { get; set; }
        public long CourseId { get; set; }
        public string CourseName { get; set; }
        public string Remark { get; set; }
    }

    public class AffectedFacultyDepartment
    {
        public long FacultyId { get; set; }
        public List<long> DepartmentIds { get; set; }
        public SelectList FacultyDepartments { get; set; }
    }
}