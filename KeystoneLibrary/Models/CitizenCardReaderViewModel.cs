using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models
{
    public class CitizenCardReaderViewModel
    {
        [Required]
        [StringLength(20)]
        public string Code { get; set; }

        [Required]
        public long TitleId { get; set; }
        
        [StringLength(100)]
        public string FirstNameTh { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstNameEn { get; set; }

        [StringLength(100)]
        public string MidNameTh { get; set; }

        [StringLength(100)]
        public string MidNameEn { get; set; }

        [StringLength(100)]
        public string LastNameTh { get; set; }

        [Required]
        [StringLength(100)]
        public string LastNameEn { get; set; }

        [Required]
        public int Gender { get; set; }
        public long? RaceId { get; set; }
        public long NationalityId { get; set; }
        public long ReligionId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime BirthDate { get; set; } = DateTime.Now;
        public long? BirthProvinceId { get; set; }
        public long? BirthStateId { get; set; }
        public long? BirthCityId { get; set; }
        public long BirthCountryId { get; set; }

        [StringLength(20)]
        public string CitizenNumber { get; set; }

        [StringLength(20)]
        public string Passport { get; set; }

        [StringLength(320)]
        public string Email { get; set; }

        [StringLength(50)]
        public string PersonalEmail { get; set; }

        [StringLength(20)]
        public string TelephoneNumber1 { get; set; }

        [StringLength(20)]
        public string TelephoneNumber2 { get; set; }

        [StringLength(200)]
        public string Facebook { get; set; }

        [StringLength(200)]
        public string Line { get; set; }

        [StringLength(200)]
        public string OtherContact { get; set; }

        [StringLength(5)]
        public string MaritalStatus { get; set; } // m = Married, s = Single, o = Other 

        [StringLength(50)]
        public string LivingStatus { get; set; } // a = Alive, d = Death, o = Other

        [StringLength(1000)]
        public string Talent { get; set; }

        public long? DeformationId { get; set; }

        [StringLength(50)]
        public string DisabledBookCode { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DisableBookIssuedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DisableBookExpiredAt { get; set; }

        [StringLength(10)]
        public string StudentStatus { get; set; }

        public string ProfileImage64 { get; set; }

        [StringLength(50)]
        public string HouseNumber { get; set; }

        [StringLength(500)]
        public string AddressTh1 { get; set; }

        [StringLength(500)]
        public string AddressTh2 { get; set; }

        [StringLength(500)]
        public string AddressEn1 { get; set; }

        [StringLength(500)]
        public string AddressEn2 { get; set; }

        [StringLength(50)]
        public string Moo { get; set; }

        [StringLength(200)]
        public string SoiTh { get; set; }

        [StringLength(200)]
        public string RoadTh { get; set; }

        [StringLength(200)]
        public string SoiEn { get; set; }

        [StringLength(200)]
        public string RoadEn { get; set; }
        
        public long CountryId { get; set; }
        public long? ProvinceId { get; set; }
        public long? DistrictId { get; set; }
        public long? SubdistrictId { get; set; }
        public long? CityId { get; set; }
        public long? StateId { get; set; }

        [Required]
        [StringLength(20)]
        public string ZipCode { get; set; }

        [StringLength(20)]
        public string TelephoneNumber { get; set; }

        public long AcademicLevelId { get; set; }

        [Required]
        public long AdmissionTermId { get; set; }

        public long? AdmissionRoundId { get; set; }

        [Required]
        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime AdmissionDate { get; set; } = DateTime.Now;

        [Required]
        public long AdmissionTypeId { get; set; }

        public long? FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public long? CurriculumVersionId { get; set; }
        public long? CurriculumId { get; set; }
        public long? PreviousBachelorSchoolId { get; set; }
        public long? PreviousMasterSchoolId { get; set; }
        public long? AgencyId { get; set; }

        [StringLength(200)] 
        public string OfficerName { get; set; }

        [StringLength(200)] 
        public string OfficerPhone { get; set; }
        public long? AdmissionPlaceId { get; set; }
        public long? ChannelId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? AppliedAt { get; set; } // วันที่ขึ้นทะเบียนนักศึกษาใหม่

        [StringLength(2)] 
        public string EntranceExamResult { get; set; } // p = pass, f = fail, n = non test or no result

        public long? SchoolCountryId { get; set; }
        public long? AdmissionDocumentGroupId { get; set; }
        public long? EducationBackgroundId { get; set; }
        public long? PreviousSchoolId { get; set; }
        public int? PreviousGraduatedYear { get; set; }
        public decimal? PreviousSchoolGPA { get; set; }
        public int Batch { get; set; }
        public long? StudyPlanId { get; set; }
        public long AcademicProgramId { get; set; } // Day, Night, Fast track

        [Required]
        public long StudentGroupId { get; set; }
        
        public long? StudentFeeGroupId { get; set; }
        public long? StudentFeeTypeId { get; set; }
    }
}