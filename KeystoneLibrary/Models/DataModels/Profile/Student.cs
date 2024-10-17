using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Scholarship;
using KeystoneLibrary.Models.DataModels.Admission;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.Graduation;
using KeystoneLibrary.Models.DataModels.Fee;
using KeystoneLibrary.Models.ViewModels;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class Student : UserTimeStamp
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; }
        public long StudentFeeTypeId { get; set; } // normal, inbound exchange, inbound personal, agency
        public long? StudentFeeGroupId { get; set; } // Term fee group
        public long ResidentTypeId { get; set; } // resident, non-resident, visiting

        [Required]
        [StringLength(10)]
        public string StudentStatus { get; set; } // a = admission, s = studying, d = delete, rs = resign, dm = dismiss
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

        [Required]
        public int Gender { get; set; } // 0 = undefined, 1 = male, 2 = female
        public long? RaceId { get; set; }
        public long? NationalityId { get; set; }
        public long? ReligionId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime BirthDate { get; set; } = DateTime.Now;
        public long? BirthProvinceId { get; set; }
        public long? BirthStateId { get; set; }
        public long? BirthCityId { get; set; }
        public long? BirthCountryId { get; set; }

        [StringLength(20)]
        public string? CitizenNumber { get; set; }

        [StringLength(20)]
        public string? Passport { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? PassportIssuedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? PassportExpiredAt { get; set; }
        public long? BankBranchId { get; set; }

        [StringLength(20)]
        public string? BankAccount { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? AccountUpdatedAt { get; set; }

        [StringLength(320)]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? Email2 { get; set; }

        [StringLength(50)]
        public string? PersonalEmail { get; set; }

        [StringLength(50)]
        public string? PersonalEmail2 { get; set; }

        [StringLength(20)]
        public string? TelephoneNumber1 { get; set; }

        [StringLength(20)]
        public string? TelephoneNumber2 { get; set; }

        [StringLength(20)]
        public string? TelephoneNumber3 { get; set; }

        [StringLength(200)]
        public string? Facebook { get; set; }

        [StringLength(200)]
        public string? Line { get; set; }

        [StringLength(200)]
        public string? OtherContact { get; set; }

        [StringLength(200)] 
        public string? NativeLanguage { get; set; } // En, Th
        
        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? IdCardCreatedDate { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? IdCardExpiredDate { get; set; }
        public long RegistrationStatusId { get; set; }

        [StringLength(5)]
        public string? MaritalStatus { get; set; } // m = Married, s = Single, o = Other 
        public long? DeformationId { get; set; }

        [StringLength(50)]
        public string? DisabledBookCode { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DisableBookIssuedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DisableBookExpiredAt { get; set; }

        [StringLength(50)]
        public string? LivingStatus { get; set; } // a = Alive, d = Death, o = Other

        [StringLength(1000)]
        public string? StudentRemark { get; set; }

        [StringLength(1000)]
        public string? Talent { get; set; }

        [StringLength(2100)]
        public string? ProfileImageURL { get; set; }

        [StringLength(2100)]
        public string? Barcode { get; set; }
        
        [StringLength(1000)]
        public string? AdmissionRemark { get; set; }

        [ForeignKey("TitleId")]
        public virtual Title Title { get; set; }

        [ForeignKey("RaceId")]
        public virtual Race? Race { get; set; }

        [ForeignKey("NationalityId")]
        public virtual Nationality? Nationality { get; set; }

        [ForeignKey("ReligionId")]
        public virtual Religion? Religion { get; set; }

        [ForeignKey("BirthProvinceId")]
        public virtual Province? BirthProvince { get; set; }

        [ForeignKey("BirthStateId")]
        public virtual State? BirthState { get; set; }

        [ForeignKey("BirthCityId")]
        public virtual City? BirthCity { get; set; }

        [ForeignKey("BirthCountryId")]
        public virtual Country? BirthCountry { get; set; }

        [ForeignKey("BankBranchId")]
        public virtual BankBranch? BankBranch { get; set; }

        [ForeignKey("RegistrationStatusId")]
        public virtual RegistrationStatus RegistrationStatus { get; set; }

        [ForeignKey("StudentFeeTypeId")]
        public virtual StudentFeeType StudentFeeType { get; set; }

        [ForeignKey("StudentFeeGroupId")]
        public virtual StudentFeeGroup? StudentFeeGroup { get; set; }

        [ForeignKey("ResidentTypeId")]
        public virtual ResidentType ResidentType { get; set; }

        [ForeignKey("DeformationId")]
        public virtual Deformation? Deformation { get; set; }
        public virtual GraduatingRequest GraduatingRequest { get; set; }
        public virtual AcademicInformation AcademicInformation { get; set; }
        public virtual AdmissionInformation AdmissionInformation { get; set; }
       
        public virtual List<GraduationInformation> GraduationInformations { get; set; }
        public virtual List<ParentInformation> ParentInformations { get; set; }
        public virtual List<StudentAddress> StudentAddresses { get; set; }
        public virtual List<CheatingStatus> CheatingStatuses { get; set; }
        public virtual List<CurriculumInformation> CurriculumInformations { get; set; }
        public virtual List<MaintenanceStatus> MaintenanceStatuses { get; set; }
        public virtual List<StudentIncident> StudentIncidents { get; set; }
        public virtual List<RegistrationCourse> RegistrationCourses { get; set; }
        public virtual List<LatePaymentTransaction> LatePayments { get; set; }
        public virtual List<ScholarshipStudent> ScholarshipStudents { get; set; }
        public virtual List<StudentProbation> StudentProbations { get; set; }
        public virtual List<StudentTransferLog> StudentTransferLogs { get; set; }
        public virtual List<StudentStatusLog> StudentStatusLogs { get; set; }
        public virtual List<ExtendedStudent> ExtendedStudents { get; set; }
        public virtual List<StudentExemptedExamScore> StudentExemptedExamScores { get; set; }
        public virtual List<StudentDocument> StudentDocuments { get; set; }
        public virtual List<OnCredit> OnCredits { get; set; }

        [NotMapped]
        public CurriculumInformation CurrentCurriculum { get; set; }

        [NotMapped]
        public GraduationInformation GraduationInformation { get; set; }

        [NotMapped]
        public string FullNameEn => string.IsNullOrEmpty(MidNameEn) ? $"{ Title?.NameEn } { FirstNameEn } { LastNameEn }"
                                                                    : $"{ Title?.NameEn } { FirstNameEn } { MidNameEn } { LastNameEn }";

        [NotMapped]
        public string FullNameEnNoTitle => string.IsNullOrEmpty(MidNameEn) ? $"{ FirstNameEn } { LastNameEn }"
                                                                           : $"{ FirstNameEn } { MidNameEn } { LastNameEn }";

        [NotMapped]
        public string FullNameTh => string.IsNullOrEmpty(MidNameTh) ? $"{ Title?.NameTh } { FirstNameTh } { LastNameTh }"
                                                                    : $"{ Title?.NameTh } { FirstNameTh } { MidNameTh } { LastNameTh }";

        [NotMapped]
        public string BirthDateText => BirthDate.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string BirthDateThText => BirthDate.AddYears(543).ToString(StringFormat.ShortDate);
        
        [NotMapped]
        public string BirthDateFullText {
            get {
                System.Globalization.CultureInfo _cultureENInfo = new System.Globalization.CultureInfo("en-EN");
                return BirthDate.ToString("MMMM d, yyyy", _cultureENInfo);
            }
        }

        [NotMapped]
        public string BirthDateThFullText {
            get {
                System.Globalization.CultureInfo _cultureTHInfo = new System.Globalization.CultureInfo("th-TH");
                return BirthDate.ToString("d MMMM yyyy", _cultureTHInfo);
            }
        }

        [NotMapped]
        public string CodeAndName
        {
            get
            {
                var lastname = String.IsNullOrEmpty(LastNameEn) ? "" : $"{ LastNameEn.Substring(0, 1) }.";
                return $"{ Code } { FirstNameEn } { lastname }";
            }
        }

        [NotMapped]
        public int CodeInt
        {
            get
            {
                int code;
                bool success = Int32.TryParse(Code, out code);
                return success ? code : 0;
            }
        }

        [NotMapped]
        public string StudentStatusText
        {
            get
            {
                switch (StudentStatus)
                {
                    case "a":
                        return "Admission";
                    case "s":
                        return "Studying";
                    case "d":
                        return "Deleted";
                    case "b":
                        return "Blacklist";
                    case "rs":
                        return "Resigned";
                    case "dm":
                        return "Dismiss";
                    case "prc":
                        return "Passed all required course";
                    case "pa":
                        return "Passed away";
                    case "g":
                        return "Graduated";
                    case "g1":
                        return "Graduated with first class honor";
                    case "g2":
                        return "Graduated with second class honor";
                    case "ex":
                        return "Exchange";
                    case "tr":
                        return "Transferred to other university";
                    case "la":
                        return "Leave of absence";
                    case "np":
                        return "No Report";
                    case "re":
                        return "Reenter";
                    case "ra":
                        return "Re-admission";
                    default:
                        return "N/A";
                }
            }
        }

        [NotMapped]
        public string GenderText
        {
            get
            {
                switch (Gender)
                {
                    case 0:
                        return "Undefined";
                    case 1:
                        return "Male";
                    case 2:
                        return "Female";
                    default:
                        return "N/A";
                }
            }
        }

        [NotMapped]
        public string NativeLanguageText
        {
            get
            {
                switch (NativeLanguage)
                {
                    case "En":
                        return "English";
                    case "Th":
                        return "Thai";
                    case "Ch":
                        return "Chinese";
                    default:
                        return "N/A";
                }
            }
        }

        [NotMapped]
        public string MaritalStatusText
        {
            get
            {
                switch (MaritalStatus)
                {
                    case "m":
                        return "Married";
                    case "s":
                        return "Single";
                    default:
                        return "Other";
                }
            }
        }

        [NotMapped]
        public int Age => DateTime.Now.Year - BirthDate.Year;

        [NotMapped]
        public StudentRequiredDocument StudentRequiredDocument { get; set; }

        [NotMapped]
        public string IdCardExpiredDateString => IdCardExpiredDate == null ? ""
                                                                           : $"{IdCardExpiredDate.Value.Month}/{IdCardExpiredDate.Value.Year}";

        [NotMapped]
        public List<StudentRegistrationCourseViewModel> StudentRegistrationCourseViewModels { get; set; } = new List<StudentRegistrationCourseViewModel>();
        
        [NotMapped]
        public StudentRegistrationCoursesViewModel StudentRegistrationCoursesViewModels { get; set; } = new StudentRegistrationCoursesViewModel();

        [NotMapped]
        public bool IsStudentExtended { get; set; }

        [NotMapped]
        public bool IsIdCardExisted => IdCardCreatedDate != null;

        [NotMapped]
        public bool IsProfileImageExisted => !string.IsNullOrEmpty(ProfileImageURL);

        [NotMapped]
        public ScholarshipStudent ScholarshipStudent { get; set; }

        [NotMapped]
        public bool IsCurrentStudentProbation { get; set; }

        [NotMapped]
        public string IdCardCreatedDateText => IdCardCreatedDate == null ? "" : IdCardCreatedDate?.ToShortDateString();

        [NotMapped]
        public string IdCardExpiredDateText => IdCardExpiredDate == null ? "" : IdCardExpiredDate?.ToShortDateString();
        // For Student Status Report

        [NotMapped]
        public long RegistrationTermId { get; set; }

        [NotMapped]
        public string RegistrationTermText { get; set; }

        [NotMapped]
        public string StatusLogTerm { get; set; }

        [NotMapped]
        public string StatusLogDate { get; set; }
        
        [NotMapped]
        public string StatusActivedTerm { get; set; }

        [NotMapped]
        public string StatusActivedDate { get; set; }

        [NotMapped]
        public StudentAddress CurrentAddress { get; set; }
        
        [NotMapped]
        public StudentAddress PermanentAddress { get; set; }

        [NotMapped]
        public ParentInformation FatherInformation { get; set; }

        [NotMapped]
        public ParentInformation MotherInformation { get; set; }

        [NotMapped]
        public ParentInformation MainParentInformation { get; set; }

        [NotMapped]
        public ParentInformation EmergencyInformation { get; set; }
        
        [NotMapped]
        public long ReenterReasonId { get; set; }

        // File
        [NotMapped]
        public IFormFile UploadFile { get; set; }

        [NotMapped]
        public string UploadFileBase64 { get; set; }

        [NotMapped]
        public string StudentCode { get; set; }

        [NotMapped]
        public string ReturnController { get; set; }

        [NotMapped]
        public string ProfileImage64 { get; set; }

        [NotMapped]
        public List<CourseGroup> CourseGroups { get; set; }

        [NotMapped]
        public bool IsRegistrationLock { get; set; }

        [NotMapped]
        public bool IsPaymentLock { get; set; }

        [NotMapped]
        public bool IsSignInLock { get; set; }
        [NotMapped]
        public List<StudentIncidentLog> StudentIncidentLogs { get; set; }
        [NotMapped]
        public List<UserTabPermissionViewModel> TabPermissions { get; set; }

        // Faculty --> Department --> Curriculum
        // A student can get 1 Faculty, 1 Department and 1 Curriculum.
        // If student change faculty or department, curriculum will change too.
        // (Add in OnModelCreate, change curriculum in student, get FacultyId and DepartmentId from curriculumId and change in Student table)
        // Student can get 1-2 minors or 1-2 concentations.
        // Concentation depend on curriculum.
        // Minor is undependence but it should approve from curriculum.
    }
}