using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Graduation
{
    public class CourseGroupingLog : UserTimeStamp
    {
        public long Id { get; set; }
        public long GraduatingRequestId { get; set; }

        [StringLength(200)]
        public string? Remark { get; set; }
        public bool IsPublished { get; set; }
        public bool IsApproved { get; set; }

        [ForeignKey("GraduatingRequestId")]
        public virtual GraduatingRequest GraduatingRequest { get; set; }
        public virtual List<CourseGroupingDetail> CourseGroupingDetails { get; set; }
    }
}