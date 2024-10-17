using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class Signatory : UserTimeStamp
    {
        public long Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string FirstNameEn { get; set; }

        [Required]
        [StringLength(200)]
        public string FirstNameTh { get; set; }

        [StringLength(200)]
        public string? MiddleNameEn { get; set; }

        [StringLength(200)]
        public string? MiddleNameTh { get; set; }

        [Required]
        [StringLength(200)]
        public string LastNameEn { get; set; }

        [Required]
        [StringLength(200)]
        public string LastNameTh { get; set; }

        [Required]
        [StringLength(200)]
        public string PositionEn { get; set; }

        [Required]
        [StringLength(200)]
        public string PositionTh { get; set; }

        [StringLength(2048)]
        public string? SignImageURL { get; set; }

        [StringLength(2000)]
        public string? Remark { get; set; }

        [NotMapped]
        public string FullNameEn => $"{ FirstNameEn } { MiddleNameEn } { LastNameEn }";

        [NotMapped]
        public string FullNameTh => $"{ FirstNameTh } { MiddleNameTh } { LastNameTh }";
        
        [NotMapped]
        public IFormFile UploadFile { get; set; }
    }
}