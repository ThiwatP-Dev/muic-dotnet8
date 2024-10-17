using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Withdrawals;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Profile;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels
{
    public class RegistrationCourse : UserTimeStamp 
    {
        public RegistrationCourse()
        {

        }

        public RegistrationCourse(RegistrationCourse source)
        {
            StudentId = source.StudentId;
            TermId = source.TermId;
            CourseId = source.CourseId;
            SectionId = source.SectionId;
            IsPaid = source.IsPaid;
            IsLock = source.IsLock;
            GradeName = source.GradeName;
            IsSurveyed = source.IsSurveyed;
            Status = source.Status;
            IsActive = source.IsActive;
            GradePublishedAt = source.GradePublishedAt;
            IsGradePublished = source.IsGradePublished;
            EstimatedGradeId = source.EstimatedGradeId;
            GradeId = source.GradeId;
            IsInstallment = source.IsInstallment;
            IsOnCredit = source.IsOnCredit;
            IsTransferCourse = source.IsTransferCourse;
            IsStarCourse = source.IsStarCourse;
            TransferCourseCode = source.TransferCourseCode;
            TransferCourseName = source.TransferCourseName;
            TransferGradeName = source.TransferGradeName;
            TransferUniversityId = source.TransferUniversityId;
            Channel = source.Channel;
            USPreregistrationId = source.USPreregistrationId;
        }

        public long Id { get; set; }

        public Guid StudentId { get; set; }

        public long TermId { get; set; }

        public long CourseId { get; set; }

        public long? SectionId { get; set; }

        public bool IsPaid { get; set; }

        public bool IsLock { get; set; }

        public bool IsOnCredit { get; set; }

        public bool IsInstallment { get; set; }

        public bool IsTransferCourse { get; set; }

        public bool IsStarCourse { get; set; }

        [StringLength(5)]
        public string? GradeName { get; set; }

        public long? GradeId { get; set; }

        public bool IsGradePublished { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? GradePublishedAt { get; set; }

        public long? EstimatedGradeId { get; set; }

        public bool IsSurveyed { get; set; }

        [StringLength(5)]
        public string? Status { get; set; } //  r = registration, a = add, d = delete
        
        [StringLength(5)]
        public string? Channel { get; set; } //  r = registrar, a = android, i = ios, w = web

        [StringLength(20)]
        public string? TransferCourseCode { get; set; }

        [StringLength(200)]
        public string? TransferCourseName { get; set; }

        [StringLength(5)]
        public string? TransferGradeName { get; set; }

        public long? TransferUniversityId { get; set; }

        public Guid? USPreregistrationId { get; set; }

        [JsonIgnore]
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("SectionId")]
        public virtual Section? Section { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [ForeignKey("GradeId")]
        public virtual Grade Grade { get; set; }

        [ForeignKey("EstimatedGradeId")]
        public virtual Grade? EstimatedGrade { get; set; }

        [ForeignKey("TransferUniversityId")]
        public virtual TransferUniversity? TransferUniversity { get; set; }

        public virtual List<Withdrawal> Withdrawals { get; set; }
        public virtual List<InstallmentTransaction> InstallmentTransactions { get; set; }

        [NotMapped]
        public string WithdrawalTypes 
        { 
            get
            {
                if (Withdrawals == null || !Withdrawals.Any())
                {
                    return "";
                }
                else
                {
                    var types = Withdrawals.Select(x => x.TypeText);
                    return String.Join(", ", types);
                }
            }
        }

        [NotMapped]
        public string CourseCode => Course != null ? Course.Code : "";

        [NotMapped]
        public string SectionNumber => Section != null ? Section.Number : "";

        [NotMapped]
        public string CourseAndNumber => Course != null && Section != null ? $"{ Course.Code }({ Section.Number })" : "";

        [NotMapped]
        public string CourseNameAndNumber => Course != null && Section != null ? $"{ Course.CodeAndName }({ Section.Number })" : "";

        [NotMapped]
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case "r":
                        return "Registration";
                    case "a":
                        return "Add";
                    case "d":
                        return "Delete";
                    default:
                        return "N/A";
                }
            }
        }

        [NotMapped]
        public string GradeStatusText
        {
            get
            {
                switch (IsGradePublished)
                {
                    case true:
                        return "Finished";
                    case false:
                        return "In Progress";
                    default:
                        return "N/A";
                }
            }
        }

        [NotMapped]
        public bool IsTransfered { get; set; }

        [NotMapped]
        public List<RefundDetail> Refunds { get; set; }

        [NotMapped]
        public string Remark { get; set; }

        [NotMapped]
        public string GradePublishedAtText => GradePublishedAt == null ? "" : GradePublishedAt.Value.ToString(StringFormat.ShortDate);
        
        [NotMapped]
        public long? EquivalentCourseId { get; set; }

        [NotMapped]
        public bool IsAssignToCourseGroup { get; set; }

        [NotMapped]
        public bool IsWithDrawn => Grade?.Name?.ToUpper() == "W" ? true : false ;
    }
}