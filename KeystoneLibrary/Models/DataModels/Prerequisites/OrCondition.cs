using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Prerequisites
{
    public class OrCondition : UserTimeStamp
    {
        public long Id { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstConditionType { get; set; }
        public long FirstConditionId { get; set; }

        [Required]
        [StringLength(50)]
        public string SecondConditionType { get; set; }
        public long SecondConditionId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ExpiredAt { get; set; }
        public long? MUICId { get; set; }

        [NotMapped]
        public string FirstConditionName { get; set; }

        [NotMapped]
        public string SecondConditionName { get; set; }

        [NotMapped]
        public string FirstConditionTypeText
        {
            get
            {
                switch (FirstConditionType)
                {
                    case "and":
                        return "And";
                    case "or":
                        return "Or";
                    case "coursegroup":
                        return "Course Group";
                    case "credit":
                        return "Credit";
                    case "gpa":
                        return "GPA";
                    case "grade":
                        return "Grade";
                    case "term":
                        return "Term";
                    case "totalcoursegroup":
                        return "Total Course Group";
                    case "batch":
                        return "Batch";
                    case "ability":
                        return "Ability";
                    default:
                        return "N/A";
                }
            }
        }

        [NotMapped]
        public string SecondConditionTypeText
        {
            get
            {
                switch (SecondConditionType)
                {
                    case "and":
                        return "And";
                    case "or":
                        return "Or";
                    case "coursegroup":
                        return "Course Group";
                    case "credit":
                        return "Credit";
                    case "gpa":
                        return "GPA";
                    case "grade":
                        return "Grade";
                    case "term":
                        return "Term";
                    case "totalcoursegroup":
                        return "Total Course Group";
                    case "batch":
                        return "Batch";
                    case "ability":
                        return "Ability";
                    default:
                        return "N/A";
                }
            }
        }

        [NotMapped]
        public string ExpiredAtText => ExpiredAt?.ToString(StringFormat.ShortDate) ?? "N/A";

        [NotMapped]
        public string OrConditionName => $"First Type { FirstConditionType } Second Type { SecondConditionType }";
    }
}