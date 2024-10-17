using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class RegistrationTerm : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? StartedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EndedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ExpiredAt { get; set; }

        public long TermId { get; set; }

        [StringLength(10)]
        public string? Type { get; set; }
        // r = registration
        // p = payment
        // a = add/drop
        // ap = add/drop payment
        // w = Withdrawal
        // e = Evaluation
        // g = grade submission period
        // cc = change Curriculum

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [JsonIgnore]
        public virtual List<Slot> Slots { get; set; }

        [NotMapped]
        public string StartedDate => StartedAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string EndedDate => EndedAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string ExpiredAtText => ExpiredAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string NameNullAble { get; set; }

        [NotMapped]
        public long AcademicLevelId { get; set; }

        [NotMapped]
        public string TypeText
        { 
            get 
            {
                if (Type == "r")
                {
                    return "Registration";
                }
                else if (Type == "a")
                {
                    return "Add/Drop";
                }
                else if (Type == "p")
                {
                    return "Payment";
                }
                else if (Type == "ap")
                {
                    return "Add/Drop Payment";
                }
                else if (Type == "w")
                {
                    return "Withdrawal";
                }
                else if (Type == "e")
                {
                    return "Evaluation";
                }
                else if (Type == "g")
                {
                    return "Grade Submission";
                }
                else if (Type == "cc")
                {
                    return "Change Curriculum";
                }
                else
                {
                    return "N/A";
                }
            } 
        }
    }
}