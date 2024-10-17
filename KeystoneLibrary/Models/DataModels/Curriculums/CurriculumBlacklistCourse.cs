using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Curriculums;

namespace KeystoneLibrary.Models.DataModels
{
    public class CurriculumBlacklistCourse : UserTimeStamp
    {
        public long Id { get; set; }
        public long CourseId { get; set; }
        public long CurriculumVersionId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [ForeignKey("CurriculumVersionId")]
        public virtual CurriculumVersion CurriculumVersion { get; set; }
    }
}