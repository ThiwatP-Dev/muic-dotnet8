using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels
{
    public class GradingLog : UserTimeStamp
    {
        public long Id { get; set; }
        public long? StudentRawScoreId { get; set; }
        public long RegistrationCourseId { get; set; }

        [Required]
        [StringLength(10)]
        public string PreviousGrade { get; set; }

        [Required]
        [StringLength(10)]
        public string CurrentGrade { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}")]
        public DateTime? ApprovedAt { get; set; }

        [StringLength(200)]
        public string? ApprovedBy { get; set; }

        [StringLength(100)]
        public string? Type { get; set; } // b = edit before approve, m = maintenance, w = withdraw, a = manual add

        [StringLength(1000)]
        public string? Remark { get; set; }

        [ForeignKey("StudentRawScoreId")]
        public virtual StudentRawScore? StudentRawScore { get; set; }

        [ForeignKey("RegistrationCourseId")]
        public virtual RegistrationCourse RegistrationCourse { get; set; }

        [NotMapped]
        public string ApprovedAtText => ApprovedAt?.AddHours(7).ToString(StringFormat.ShortDate) ?? StringFormat.StringEmpty;

        [NotMapped]
        public string TypeText
        {
            get
            {
                switch (Type)
                {
                    case "b":
                        return "Before Approval";
                    case "a":
                        return "After Approval";
                    case "m":
                        return "Maintenance";
                    case "w":
                        return "Withdrawal";
                    default:
                        return "Other";
                }
            }
        }

        [NotMapped]
        public List<Grade> Grades { get; set; }

        // public override void OnBeforeInsert(string userId = null)
        // {
        //     this.UpdatedAt = DateTime.UtcNow;
        //     this.UpdatedBy =  userId ?? "N/A";
        // }

        // public override void OnBeforeUpdate(string userId = null)
        // {
        //     this.UpdatedAt = DateTime.UtcNow;
        //     this.UpdatedBy =  userId ?? "N/A";
        // }

        [StringLength(1000)]
        public string? DocumentUrl { get; set; }

        [NotMapped]
        public string CreatedAtLocalText => CreatedAt.AddHours(7).ToString(StringFormat.ShortDate);

        [NotMapped]
        public string UpdatedAtLocalText => UpdatedAt.AddHours(7).ToString(StringFormat.ShortDateTime);
    
    }
}