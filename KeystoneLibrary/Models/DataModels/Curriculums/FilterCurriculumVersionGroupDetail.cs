using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Curriculums;

namespace KeystoneLibrary.Models.DataModels
{
    public class FilterCurriculumVersionGroupDetail : UserTimeStamp
    {
        public long Id { get; set; }
        public long FilterCurriculumVersionGroupId { get; set; }    
        public long CurriculumVersionId { get; set; }    

        [ForeignKey("FilterCurriculumVersionGroupId")]
        public virtual FilterCurriculumVersionGroup FilterCurriculumVersionGroup { get; set; }

        [ForeignKey("CurriculumVersionId")]
        public virtual CurriculumVersion CurriculumVersion { get; set; }
    }   
}