using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Admission
{
    public class AdmissionDocumentGroup : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        public long AcademicLevelId { get; set; }
        public long? FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public bool IsThai { get; set; } // Graduation country 
        public long? GraduatedCountryId { get; set; } // Previous School Country = High school Graduated country
        public long? AdmissionTypeId { get; set; }

        [ForeignKey("AcademicLevelId")]
        public virtual AcademicLevel AcademicLevel { get; set; }
        
        [ForeignKey("FacultyId")]
        public virtual Faculty Faculty { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        [ForeignKey("GraduatedCountryId")]
        public virtual Country GraduatedCountry { get; set; }

        [ForeignKey("AdmissionTypeId")]
        public virtual AdmissionType AdmissionType { get; set; }
        public virtual List<RequiredDocument> RequiredDocuments { get; set; }
    }   
}