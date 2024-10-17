using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Profile;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels
{
    public class RegistrationLog : Entity
    {
        public long Id { get; set; }
        public int Sequence { get; set; }

        [StringLength(5)]
        public string? Channel { get; set; } // r = Officer, i = Ios, a = Android, w = Web, s = Student
        public Guid StudentId { get; set; }
        public long TermId { get; set; }
        public long? RegistrationConditionId { get; set; }

        public string? Modifications { get; set; } // Jsonstring of 'New Course':['BG2001(541)'], 
                                                  // 'Retained Course':['CS2001(541)', 'ACT1600(541)'], 'Discard Course':['BG2001(542)']
        public string? RegistrationCourses { get; set; } // Jsonstring type List<string> of Summary courses --> {['BG1101(541)', 'CS2001(541)']}

        [StringLength(5)]
        public string? Type { get; set; } // r = registration period, a = adding period, tr = Transfer University, c = Change Curriculum
        public int Round { get; set; }

        public long? USparkId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime CreatedAt { get; set; }

        [StringLength(500)]
        public string? CreatedBy { get; set; }
        
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("RegistrationConditionId")]
        public virtual RegistrationCondition? RegistrationCondition { get; set; }

        public override void OnBeforeInsert(string userId = null)
        {
            this.CreatedAt = DateTime.UtcNow;
            this.CreatedBy = userId;
        }

        [NotMapped]
        public string NewCourse { get; set; }

        [NotMapped]
        public string CreatedByFullNameEn { get; set; }

        [NotMapped]
        public string RetainedCourse { get; set; }

        [NotMapped]
        public string DiscardedCourse { get; set; }

        [NotMapped]
        public string CreatedAtText => CreatedAt.ToString(StringFormat.ShortDateTime);

        [NotMapped]
        public List<RegistrationModificationDetailViewModel> RegistrationSummary
        {
            get
            {
                var result = new JsonSerializerSettings { Error = (se, ev) => { ev.ErrorContext.Handled = true; } };
                var registrationSummary = JsonConvert.DeserializeObject<List<RegistrationModificationDetailViewModel>>(RegistrationCourses, result);
                return registrationSummary;
            }
        }

        [NotMapped]
        public RegistrationModificationViewModel RegistrationModification
        {
            get
            {
                var result = new JsonSerializerSettings { Error = (se, ev) => { ev.ErrorContext.Handled = true; } };
                var registrationModification = JsonConvert.DeserializeObject<RegistrationModificationViewModel>(Modifications, result);
                return registrationModification;
            }
        }

        [NotMapped]
        public string Summary { get; set; }
        
        [NotMapped]
        public class RegistrationModificationViewModel
        {
            public List<RegistrationModificationDetailViewModel> NewCourse { get; set; }
            public List<RegistrationModificationDetailViewModel> RetainedCourse { get; set; }
            public List<RegistrationModificationDetailViewModel> DiscardedCourse { get; set; }
        }

        [NotMapped]
        public string TypeText
        {
            get
            {
                switch (Type)
                {
                    case "r":
                        return "Registration";
                    case "a":
                        return "Adding";
                    default:
                        return "N/A";
                }
            }
        }

        [NotMapped]
        public string ChannelText
        {
            get
            {
                switch (Channel)
                {
                    case "r":
                    case "R":
                        return "Officer";
                    case "i":
                    case "I":
                        return "iOS";
                    case "a":
                    case "A":
                        return "Android";
                    case "w":
                    case "W":
                        return "Web";
                    default:
                        return "N/A";
                }
            }
        }
    }

    public class RegistrationModificationDetailViewModel
    {
        public string CourseCode { get; set; }
        public string SectionNumber { get; set; }
        public string CourseString => $"{ CourseCode } ({SectionNumber})";
    }
}