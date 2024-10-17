using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels
{
    public class CourseGroupModification : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long? CurriculumCourseId { get; set; }
        public long CourseId { get; set; }
        public long CourseGroupId { get; set; }
        public long? MoveCourseGroupId { get; set; }
        public long? RequiredGradeId { get; set; }
        public bool IsAddManually { get; set; }
        public string? Remark { get; set; }

        public bool IsDisabled { get; set; }

        [ForeignKey("CurriculumCourseId")]
        public virtual CurriculumCourse? CurriculumCourse { get; set; }
        
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
        [ForeignKey("CourseGroupId")]
        public virtual CourseGroup CourseGroup { get; set; }

        [ForeignKey("MoveCourseGroupId")]
        public virtual CourseGroup? MoveCourseGroup { get; set; }

        [ForeignKey("RequiredGradeId")]
        public virtual Grade? RequiredGrade { get; set; }
    }
}