using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels.Graduation
{
    public class GraduatingRequest : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }

        [StringLength(100)]
        public string? Telephone { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(10)]
        public string? Channel { get; set; } // o = Officer, i = Ios, a = Android, w = Web
        public int? ExpectedAcademicTerm { get; set; }
        public int? ExpectedAcademicYear { get; set; }

        public int? ConfirmAcademicTerm { get; set; }
        public int? ConfirmAcademicYear { get; set; }
        public DateTime? RequestedDate { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }
        public bool IsAcceptTerm { get; set; }
        public bool IsPublished { get; set; }

        [StringLength(5)]
        public string? Status { get; set; } // w = waiting, a = approved, r = reject

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
        
        public virtual List<CoursePrediction> CoursePredictions { get; set; }
        public virtual List<GraduatingRequestLog> GraduatingRequestLogs { get; set; }

        [NotMapped]
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case "w":
                        return "Submitted";
                    case "a":
                        return "Accepted";
                    case "p":
                        return "Checking in progress";
                    case "c":
                        return "Completed";
                    case "r":
                        return "Rejected";
                    case "t":
                        return "Returned";
                    default:
                        return "N/A";
                }
            }
        }

        public string ChannelText
        {
            get 
            {
                switch (Channel)
                {
                    case "o":
                    return "Officer";

                    case "i":
                    return "iOS";

                    case "w":
                    return "Web";

                    default:
                    return "N/A";
                }
            }
        }

        [NotMapped]
        public int Credit { get; set; }

        [NotMapped]
        public string CurriculumVersion { get; set; }

        [NotMapped]
        public string RequestedDateText => RequestedDate.HasValue ? RequestedDate.Value.ToString(StringFormat.ShortDate) : "-";

        [NotMapped]
        public string ExpectedTermText => ExpectedAcademicTerm.HasValue ? ExpectedAcademicTerm + "/" + ExpectedAcademicYear : "-";

        [NotMapped]
        public string ExpectedTermYearText => ExpectedAcademicTerm.HasValue ? ExpectedAcademicTerm + "/" + ExpectedAcademicYear.ToString().Substring(2) + "-" + (ExpectedAcademicYear + 1).ToString().Substring(2): "-";
    }
}