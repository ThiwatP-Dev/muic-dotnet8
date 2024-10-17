using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels.Admission
{
    public class StudentDocument : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long? RequiredDocumentId { get; set; }
        public long? DocumentId { get; set; }
        public bool IsRequired { get; set; }
        public int Amount { get; set; } // submitted document
        public int SubmittedAmount { get; set; }

        [StringLength(2000)]
        public string? ImageUrl { get; set; }
        
        [StringLength(2000)]
        public string? Remark { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("RequiredDocumentId")]
        public virtual RequiredDocument? RequiredDocument { get; set; }

        [ForeignKey("DocumentId")]
        public virtual Document? Document { get; set; }

        [NotMapped]
        public string DocumentStatus => SubmittedAmount < Amount ? "w" : "c"; // w = waiting, c = complete
        
        [NotMapped]
        public IFormFile UploadFile { get; set; }
    }   
}