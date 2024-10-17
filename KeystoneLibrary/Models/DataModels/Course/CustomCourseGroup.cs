using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Fee;
namespace KeystoneLibrary.Models.DataModels
{
    public class CustomCourseGroup : UserTimeStamp
    {
        public long Id { get; set; }
        
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
        public string? CourseIds { get; set; }    
        public virtual List<TuitionFeeRate> TuitionFeeRates { get; set; } 

        [NotMapped]
        public List<long> Courses { get; set; }

        [NotMapped]
        public string CourseNames { get; set; }

        [NotMapped]
        public List<long> CourseIdsLong { get {
                return string.IsNullOrEmpty(CourseIds) ? new List<long>()
                                                       : JsonConvert.DeserializeObject<List<long>>(CourseIds);
            } }
    }   
}