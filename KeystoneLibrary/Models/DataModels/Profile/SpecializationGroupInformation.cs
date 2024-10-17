using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class SpecializationGroupInformation : UserTimeStamp
    {
        public long Id { get; set; }
        public long CurriculumInformationId { get; set; }
        public long SpecializationGroupId { get; set; } // ability

        [JsonIgnore]
        [ForeignKey("CurriculumInformationId")]
        public virtual CurriculumInformation CurriculumInformation { get; set; }

        [JsonIgnore]
        [ForeignKey("SpecializationGroupId")]
        public virtual SpecializationGroup SpecializationGroup { get; set; }

    }
}