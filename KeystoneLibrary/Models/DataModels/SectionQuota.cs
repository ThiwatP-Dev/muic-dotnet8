using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class SectionQuota : UserTimeStamp
    {
        public long Id { get; set; }
        public long TermId { get; set; }
        public long FacultyId { get; set; }
        public int Quota { get; set; }
        
        [NotMapped]
        public long AcademicLevelId { get; set; }
        
        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("FacultyId")]
        public virtual Faculty Faculty { get; set; }
    }
}