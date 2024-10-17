using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class StandardGradingScore
    {
        public long Id { get; set; }
        public long StandardGradingGroupId { get; set; }
        public decimal Minimum { get; set; }
        public decimal Maximum { get; set; }
        public long GradeId { get; set; }

        [ForeignKey("StandardGradingGroupId")]
        public virtual StandardGradingGroup StandardGradingGroup { get; set; }

        [ForeignKey("GradeId")]
        public virtual Grade Grade { get; set; }
    }
}