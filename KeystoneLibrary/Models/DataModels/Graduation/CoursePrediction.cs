using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Graduation
{
    public class CoursePrediction : UserTimeStamp
    {
        public long Id { get; set; }
        public long GraduatingRequestId { get; set; }
        public int AcademicTerm { get; set; }
        public int AcademicYear { get; set; }
        public long CourseId { get; set; } 

        [ForeignKey("GraduatingRequestId")]
        public virtual GraduatingRequest GraduatingRequest { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [NotMapped]
        public string Semester => (AcademicTerm > 0 ? AcademicTerm.ToString() + "/" + AcademicYear.ToString() : "n/a");
        
        [NotMapped]
        public List<long> CourseIdList { get; set; }

        [NotMapped]
        public List<Course> Courses { get; set; }

        [NotMapped]
        public int TotalCredit { get; set; }
    }
}