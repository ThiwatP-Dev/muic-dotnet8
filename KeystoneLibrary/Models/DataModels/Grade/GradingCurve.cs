using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels
{
    public class GradingCurve : UserTimeStamp
    {
        public long Id { get; set; }
        public long CourseId { get; set; }
        public long TermId { get; set; }
        public long? GradeTemplateId { get; set; }

        public long GradeId { get; set; }
        public decimal Minimum { get; set; }
        public decimal Maximum { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("GradeTemplateId")]
        public virtual GradeTemplate GradeTemplate { get; set; }

        [ForeignKey("GradeId")]
        public virtual Grade Grade { get; set; }
    }
}