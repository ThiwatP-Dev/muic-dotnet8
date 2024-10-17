using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Curriculums;

namespace KeystoneLibrary.Models.DataModels
{
    public class CurriculumSpecializationGroup : UserTimeStamp
    {
        public long Id { get; set; }
        public long SpecializationGroupId { get; set; }
        public long CurriculumVersionId { get; set; }

        [ForeignKey("SpecializationGroupId")]
        public virtual SpecializationGroup SpecializationGroup { get; set; }

        [ForeignKey("CurriculumVersionId")]
        public virtual CurriculumVersion CurriculumVersion { get; set; }
    }
}