using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class StudentTransferLogDetail
    {
        public long Id { get; set; }
        public long StudentTransferLogId { get; set; }
        public long? CourseId { get; set; } // Current Course --> if external course cannot transfer, keep log CourseId = null
        public long? RegistrationCourseId { get; set; } // Registration Course after save Course for student
        public long? PreviousRegistrationCourseId { get; set; } // Change Curriculum --> Previous Registration Course before Transfer
        public long? ExternalCourseId { get; set; } // Transfer University --> Previous External course from another university
        public long? CourseGroupId { get; set; } // case: Not match group
        
        [StringLength(10)]
        public string? PreviousGrade { get; set; }
        public long? GradeId { get; set; }
        
        [ForeignKey("StudentTransferLogId")]
        public virtual StudentTransferLog StudentTransferLog { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        [ForeignKey("ExternalCourseId")]
        public virtual Course? ExternalCourse { get; set; }

        [ForeignKey("RegistrationCourseId")]
        public virtual RegistrationCourse? RegistrationCourse { get; set; }

        [ForeignKey("PreviousRegistrationCourseId")]
        public virtual RegistrationCourse? PreviousRegistrationCourse { get; set; }

        [ForeignKey("CourseGroupId")]
        public virtual CourseGroup? CourseGroup { get; set; }

        [ForeignKey("GradeId")]
        public virtual Grade? Grade { get; set; }
    }
}