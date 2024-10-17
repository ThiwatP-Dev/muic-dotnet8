using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class SpecializationGroupBlackList : UserTimeStamp
    {
        public long Id { get; set; }
        public long SpecializationGroupId { get; set; }
        public long DepartmentId { get; set; }

        [JsonIgnore]
        [ForeignKey("SpecializationGroupId")]
        public virtual SpecializationGroup SpecializationGroup { get; set; }

        [JsonIgnore]
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }
    }
}