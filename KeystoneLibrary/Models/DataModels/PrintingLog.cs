using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels
{
    public class PrintingLog
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }

        [Required]
        [StringLength(20)]
        public string StudentCode { get; set; }

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }
        public int Gender { get; set; } // 1 = male, 2 = female
        public long FacultyId { get; set; }

        [StringLength(200)]
        public string? FacultyName { get; set; }
        public long DepartmentId { get; set; }

        [StringLength(200)]
        public string? DepartmentName { get; set; }

        [Required]
        public int RunningNumber { get; set; }

        [Required]
        public int Year { get; set; }

        [StringLength(200)]
        public string? ReferenceNumber { get; set; }

        [StringLength(500)]
        public string? Document { get; set; }
        public bool IsUrgent { get; set; }
        public int Amount { get; set; }
        
        [StringLength(200)]
        public string? Purpose { get; set; } // s = for student, o = for officer, i = for instructor

        [StringLength(200)]
        public string? TrackingNumber { get; set; }

        [StringLength(10)]
        public string? Language { get; set; } // THAI, ENG

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? RequestedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? PrintedAt { get; set; }

        [StringLength(200)]
        public string? PrintedBy { get; set; }
        public bool IsPaid { get; set; }
        public decimal DocumentFee { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }

        [NotMapped]
        public string PrintedAtText => PrintedAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string FullName => $"{ FirstName } { LastName }";

        [NotMapped]
        public string GenderText
        {
            get
            {
                switch(Gender)
                {
                    case 0:
                        return "Undefined";
                    case 1:
                        return "Male";
                    case 2:
                        return "Female";
                }
                return "N/A";
            }
        }
    }
}