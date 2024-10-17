using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Graduation
{
    public class CourseGroupingDetail : UserTimeStamp
    {
        public long Id { get; set; }
        public long CourseGroupingLogId { get; set; }
        public long? CourseGroupId { get; set; }
        public long? ParentCourseGroupId { get; set; }
        public long? MovedCourseGroupId { get; set; }

        [StringLength(500)]
        public string? CourseGroupName { get; set; } // Star, Free-Elective

        [StringLength(500)]
        public string? ParentCourseGroupName { get; set; } // Star, Free-Elective
        public long CourseId { get; set; }
        public long? RegistrationCourseId { get; set; }
        public long? EquivalentCourseId { get; set; }
        public bool IsAddManually { get; set; }

        [ForeignKey("CourseGroupingLogId")]
        public virtual CourseGroupingLog CourseGroupingLog { get; set; }

        [ForeignKey("RegistrationCourseId")]
        public virtual RegistrationCourse? RegistrationCourse { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [ForeignKey("EquivalentCourseId")]
        public virtual Course? EquivalentCourse { get; set; }
    }
}