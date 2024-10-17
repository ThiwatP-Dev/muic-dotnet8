using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Admission
{
    public class IntensiveCourse : UserTimeStamp
    {
        public long Id { get; set; }
        public long? AcademicLevelId { get; set; }
        public long? FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public long CourseId { get; set; }

        [ForeignKey("AcademicLevelId")]
        public virtual AcademicLevel AcademicLevel { get; set; }
        
        [ForeignKey("FacultyId")]
        public virtual Faculty Faculty { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
    }   
}