using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Graduation
{
    public class GraduatingRequestLog : UserTimeStamp
    {
        public long Id { get; set; }
        public long GraduatingRequestId { get; set; }

        [StringLength(5)]
        public string? Status { get; set; } // w = waiting, a = approved, r = reject
        public bool IsPublished { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }

        [ForeignKey("GraduatingRequestId")]
        public virtual GraduatingRequest GraduatingRequest { get; set; }

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
    }
}