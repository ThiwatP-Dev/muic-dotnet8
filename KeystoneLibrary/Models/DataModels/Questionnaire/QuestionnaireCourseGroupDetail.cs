using System.ComponentModel.DataAnnotations.Schema;
namespace KeystoneLibrary.Models.DataModels
{
    public class QuestionnaireCourseGroupDetail : UserTimeStamp
    {
        public long Id { get; set; }
        public long QuestionnaireCourseGroupId { get; set; }    
        public long CourseId { get; set; }    

        [ForeignKey("QuestionnaireCourseGroupId")]
        public virtual QuestionnaireCourseGroup QuestionnaireCourseGroup { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
    }   
}