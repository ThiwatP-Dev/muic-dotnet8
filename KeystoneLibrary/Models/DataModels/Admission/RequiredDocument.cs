using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Admission
{
    public class RequiredDocument
    {
        // Match Admission Group with requried document
        public long Id { get; set; }
        public long AdmissionDocumentGroupId { get; set; }
        public long DocumentId { get; set; }
        public bool IsRequired { get; set; }
        public int Amount { get; set; }

        [ForeignKey("AdmissionDocumentGroupId")]
        public virtual AdmissionDocumentGroup AdmissionDocumentGroup { get; set; }
        
        [ForeignKey("DocumentId")]
        public virtual Document Document { get; set; }
    }   
}