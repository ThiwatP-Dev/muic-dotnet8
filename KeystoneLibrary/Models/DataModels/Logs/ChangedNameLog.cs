using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Logs
{
    public class ChangedNameLog
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long? PetitionId { get; set; }

        [StringLength(100)]
        public string? FirstNameEn { get; set; }

        [StringLength(100)]
        public string? LastNameEn { get; set; }

        [StringLength(100)]
        public string? ChangedFirstNameEn { get; set; }

        [StringLength(100)]
        public string? ChangedLastNameEn { get; set; }

        [StringLength(100)]
        public string? FirstNameTh { get; set; }

        [StringLength(100)]
        public string? LastNameTh { get; set; }

        [StringLength(100)]
        public string? ChangedFirstNameTh { get; set; }

        [StringLength(100)]
        public string? ChangedLastNameTh { get; set; }
        public int RunningNumber { get; set; } // 0001
        public int Year { get; set; } // 2021

        [StringLength(200)]
        public string? ReferenceNumber { get; set; }

        [StringLength(500)]
        public string? DistrictRegistrationOffice { get; set; } // ชื่ออำเภอที่เปลี่ยน
        
        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime DistrictRegistrationAt { get; set; } // วันที่เปลี่ยนในเอกสาร

        [StringLength(500)]
        public string? DocumentUrl { get; set; }

        [StringLength(10)]
        public string? ChangedType { get; set; } // c = change, s = spelling

        [StringLength(10)]
        public string? NameType { get; set; } // f = firstname, l = lastname, b = both

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? RequestedAt { get; set; } // วันที่ request petition

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ChangedAt { get; set; } // วันที่เปลี่ยนในระบบมหาวิทยาลัย

        [StringLength(200)]
        public string? ChangedBy { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }

        [StringLength(5)]
        public string? Status { get; set; } // w = waiting, a = approved, r = reject

        [NotMapped]
        public string StatusText
        {
            get 
            {
                switch (Status)
                {
                    case "w":
                    return "Waiting";

                    case "a":
                    return "Approved";

                    case "r":
                    return "Reject";

                    default:
                    return "N/A";
                }
            }
        }

        [NotMapped]
        public string ChangedAtText => ChangedAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string DistrictRegistrationAtText => DistrictRegistrationAt.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string RequestedAtText => RequestedAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string ChangedTypeText
        {
            get
            {
                switch (ChangedType)
                {
                    case "c":
                        return "Change";
                    case "s":
                        return "Spelling";
                    default:
                        return "N/A";
                }
            }
        }

        [NotMapped]
        public string NameTypeText
        {
            get
            {
                switch (NameType)
                {
                    case "f":
                        return "Firstname";
                    case "l":
                        return "Lastname";
                    case "b":
                        return "Both";
                    default:
                        return "N/A";
                }
            }
        }
    }
}