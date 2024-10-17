using System.ComponentModel.DataAnnotations.Schema;
namespace KeystoneLibrary.Models.DataModels
{
    public class FilterCourseGroupDetail : UserTimeStamp
    {
        public long Id { get; set; }
        public long FilterCourseGroupId { get; set; }    
        public long CourseId { get; set; }    

        [ForeignKey("FilterCourseGroupId")]
        public virtual FilterCourseGroup FilterCourseGroup { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
    }   
}