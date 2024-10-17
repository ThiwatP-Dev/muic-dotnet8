using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels
{
    public class RegistrationChangeCourseLog : UserTimeStamp 
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long RegistrationCourseId { get; set; }
        public long FromCourseId { get; set; }
        public long? FromSectionId { get; set; }
        public long ToCourseId { get; set; }
        public bool FromStarCourse { get; set; }
        public bool ToStarCourse { get; set; }
        public bool FromTransferCourse { get; set; }
        public bool ToTransferCourse { get; set; }

        [StringLength(5)]
        public string? FromGradeName { get; set; }
        public long? FromGradeId { get; set; }

        [StringLength(5)]
        public string? ToGradeName { get; set; }
        public long? ToGradeId { get; set; }


        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("RegistrationCourseId")]
        public virtual RegistrationCourse RegistrationCourse { get; set; }

        [ForeignKey("FromCourseId")]
        public virtual Course FromCourse { get; set; }

        [ForeignKey("ToCourseId")]
        public virtual Course ToCourse { get; set; }

        [ForeignKey("FromGradeId")]
        public virtual Grade? FromGrade { get; set; }

        [ForeignKey("ToGradeId")]
        public virtual Grade? ToGrade { get; set; }
    }
}