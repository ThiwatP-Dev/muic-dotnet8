using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels
{
    public class FacultyMember : UserTimeStamp
    {
        public long Id { get; set; }
        public long FacultyId { get; set; }
        public long InstructorId { get; set; }
        public long? FilterCourseGroupId { get; set; }
        public long? FilterCurriculumVersionGroupId { get; set; }

        [StringLength(5)]
        public string? Type { get; set; }
        // pd = Program Director
        // pc = Program Coordinator
        // c = Chairman

        [ForeignKey("FacultyId")]
        public virtual Faculty Faculty { get; set; }

        [ForeignKey("InstructorId")]
        public virtual Instructor Instructor { get; set; }

        [ForeignKey("FilterCourseGroupId")]
        public virtual FilterCourseGroup? FilterCourseGroup { get; set; }

        [ForeignKey("FilterCurriculumVersionGroupId")]
        public virtual FilterCurriculumVersionGroup? FilterCurriculumVersionGroup { get; set; }


        [NotMapped]
        public string TypeText
        {
            get
            {
                switch (Type)
                {
                    case "pd":
                        return "Program Director";
                    case "pc":
                        return "Program Coordinator";
                    case "c":
                        return "Chairman";
                    default:
                        return "Instructor";
                }
            }
        }
    }
}