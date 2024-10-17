using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels
{
    public class BlacklistedStudent : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid? StudentId { get; set; }
        public long TitleId { get; set; }

        [StringLength(100)]
        public string? FirstNameTh { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstNameEn { get; set; }

        [StringLength(100)]
        public string? MidNameTh { get; set; }

        [StringLength(100)]
        public string? MidNameEn { get; set; }

        [StringLength(100)]
        public string? LastNameTh { get; set; }
        
        [Required]
        [StringLength(100)]
        public string LastNameEn { get; set; }
        public int Gender { get; set; } // 0 = undefined, 1 = male, 2 = female

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime BirthDate { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string? CitizenNumber { get; set; }

        [StringLength(20)]
        public string? Passport { get; set; }

        [StringLength(1000)]
        public string? Detail { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student? Student { get; set; }

        [ForeignKey("TitleId")]
        public virtual Title Title { get; set; }

        [NotMapped]
        public string FullNameEn => $"{ FirstNameEn } { LastNameEn }";

        [NotMapped]
        public string FullNameTh => $"{ FirstNameTh } { LastNameTh }"; 

        [NotMapped]
        public string Code { get; set; }
    }
}