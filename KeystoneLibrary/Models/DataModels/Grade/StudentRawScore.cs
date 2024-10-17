using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels
{
    public class StudentRawScore : UserTimeStamp
    {
        public long Id { get; set; }
        public long RegistrationCourseId { get; set; }
        public Guid StudentId { get; set; }
        public long CourseId { get; set; }
        public long SectionId { get; set; }
        public decimal? TotalScore { get; set; }
        public long? GradeId { get; set; }
        public long? GradeTemplateId { get; set; }
        public decimal? Percentage { get; set; } // calculate from total score after setting curve
        public long? BarcodeId { get; set; }
        public bool IsSkipGrading { get; set; }

        [ForeignKey("RegistrationCourseId")]
        public virtual RegistrationCourse RegistrationCourse { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [ForeignKey("SectionId")]
        public virtual Section Section { get; set; }

        [ForeignKey("GradeId")]
        public virtual Grade Grade { get; set; }

        [ForeignKey("GradeTemplateId")]
        public virtual GradeTemplate GradeTemplate { get; set; }

        [ForeignKey("BarcodeId")]
        public virtual Barcode Barcode { get; set; }
        public virtual List<StudentRawScoreDetail> StudentRawScoreDetails { get; set; }
        public virtual List<GradingLog> GradingLogs { get; set; }
    }
}