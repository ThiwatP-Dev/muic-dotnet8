using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Admission
{
    public class VerificationLetter : UserTimeStamp
    {
        public long Id { get; set; }
        public int RunningNumber { get; set; }
        public int Year { get; set; }
        public long? AdmissionTermId { get; set; } // use for filter student
        public long? AdmissionRoundId { get; set; } // use for filter student

        [StringLength(20)]
        public string? StudentCodeFrom { get; set; }

        [StringLength(20)]
        public string? StudentCodeTo { get; set; }
        public int? BatchFrom { get; set; }
        public int? BatchTo { get; set; }

        [Required]
        [StringLength(300)]
        public string Recipient { get; set; } // ผู้อำนวยการสำนักงานเขต รวมถึงคนเซ็นลายเซ็นด้านล่าง

        [Required]
        [StringLength(300)]
        public string SchoolName { get; set; } // สังกัดสำนักงานเขตพื้นที่การศึกษา
        public long? SchoolGroupId { get; set; }
        public long? PreviousSchoolId { get; set; }

        [StringLength(300)]
        public string? SchoolType { get; set; } // ex. โรงเรียน or สังกัด
        public long SignatoryId { get; set; }

        [Required]
        [StringLength(300)]
        public string OfficerName { get; set; }

        [StringLength(300)]
        public string? OfficerPosition { get; set; }

        [StringLength(300)]
        public string? OfficerDivision { get; set; }

        [StringLength(20)]
        public string? OfficerPhone { get; set; }

        [StringLength(320)]
        public string? OfficerEmail { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime SentAt { get; set; }

        [StringLength(20)]
        public string? ReceivedNumber { get; set; }
        public DateTime? ReceivedAt { get; set; }

        [ForeignKey("SignatoryId")]
        public virtual Signatory Signatory { get; set; }

        [ForeignKey("AdmissionTermId")]
        public virtual Term? AdmissionTerm { get; set; }

        [ForeignKey("AdmissionRoundId")]
        public virtual AdmissionRound? AdmissionRound { get; set; }

        [ForeignKey("SchoolGroupId")]
        public virtual SchoolGroup? SchoolGroup { get; set; }

        [ForeignKey("PreviousSchoolId")]
        public virtual PreviousSchool? PreviousSchool { get; set; }

        [JsonIgnore]
        public virtual List<VerificationStudent> VerificationStudents { get; set; } = new List<VerificationStudent>();

        [NotMapped]
        public int StudentCodeFromInt 
        {
            get
            {
                int studentCodeFrom;
                bool success = Int32.TryParse(StudentCodeFrom, out studentCodeFrom);
                return success ? studentCodeFrom : 0;
            }
        }

        [NotMapped]
        public int StudentCodeToInt 
        {
            get
            {
                int studentCodeTo;
                bool success = Int32.TryParse(StudentCodeTo, out studentCodeTo);
                return success ? studentCodeTo : 0;
            }
        }

        [NotMapped]
        public long AcademicLevelId { get; set; }

        [NotMapped]
        public string RunningNumberString => RunningNumber.ToString().PadLeft(4, '0');

        [NotMapped]
        public string RunningNumberYear => $"{ RunningNumberString }/{ Year+543 }";

        [NotMapped]
        public string SentAtText => SentAt.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string ReceivedAtText => ReceivedAt?.ToString(StringFormat.ShortDate);
    }   
}