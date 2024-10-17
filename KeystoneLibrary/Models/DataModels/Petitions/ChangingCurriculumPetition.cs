using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.Enums;

namespace KeystoneLibrary.Models.DataModels.Petitions
{
    public class ChangingCurriculumPetition : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }

        [Required]
        [StringLength(20)]
        public string StudentCode { get; set; }
        public long TermId { get; set; }
        public long CurrentDepartmentId { get; set; }
        public long NewDepartmentId { get; set; }
        public long CurrentCurriculumVersionId { get; set; }
        public long? NewCurriculumVersionId { get; set; }

        [StringLength(20)]
        public string? Channel { get; set; } // web, app

        [StringLength(5000)]
        public string? StudentRemark { get; set; }

        [StringLength(5000)]
        public string? Remark { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public PetitionStatusEnum Status {get;set;}
        
        [JsonIgnore]
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("CurrentDepartmentId")]
        public virtual Department CurrentDepartment { get; set; }

        [ForeignKey("NewDepartmentId")]
        public virtual Department NewDepartment { get; set; }

        [ForeignKey("CurrentCurriculumVersionId")]
        public virtual CurriculumVersion CurrentCurriculumVersion { get; set; }

        [ForeignKey("NewCurriculumVersionId")]
        public virtual CurriculumVersion? NewCurriculumVersion { get; set; }

        [NotMapped]
        public string ChannelText
        {
            get
            {
                switch (Channel)
                {
                    case "w":
                        return "Web";
                    case "a":
                        return "Application";
                    default:
                        return "N/A";
                }
            }
        }
    }
}
