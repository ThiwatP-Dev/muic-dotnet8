using System.ComponentModel.DataAnnotations;
namespace KeystoneLibrary.Models.DataModels
{
    public class FilterCourseGroup : UserTimeStamp
    {
        public long Id { get; set; }
        
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }
        public virtual List<FilterCourseGroupDetail> FilterCourseGroupDetails { get; set; } 
    }   
}