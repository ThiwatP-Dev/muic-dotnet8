using System.ComponentModel.DataAnnotations;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Admission;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models
{
    public class StudentIdCardViewModel
    {
        public string CardType { get; set; }
        public long? AcademicLevelId { get; set; }
        public long? AdmissionRoundId { get; set; }
        public string Code { get; set; }
        public string FromCode { get; set; }
        public string ToCode { get; set; }
        public string ExaminationType { get; set; }
        public string ExaminationDate { get; set; }
        public long CourseId { get; set; }
        public string ErrorMessage { get; set; }
        public List<StudentIdCardDetail> StudentIdCardDetails { get; set; }
        public StudentIdCardDetail StudentIdCardDetail { get; set; }
        public List<StudentIdCardFormDetail> FormDetails { get; set; } = new List<StudentIdCardFormDetail>();
        
        public int FromCodeInt 
        {
            get
            {
                int code;
                bool success = Int32.TryParse(FromCode, out code);
                return success ? code : 0;
            }
        }

        public int ToCodeInt 
        {
            get
            {
                int code;
                bool success = Int32.TryParse(ToCode, out code);
                return success ? code : 0;
            }
        }
    }

    public class StudentIdCardDetail 
    {
        public Guid StudentId { get; set; }
        public long TitleId { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ThaiName { get; set; }
        public string ThaiLastName { get; set; }
        public string Barcode { get; set; }
        public AcademicInformation AcademicInformation { get; set; }
        public long AcademicLevelId { get; set; }
        public string AcademicLevel { get; set; }
        public long FacultyId { get; set; }
        public string FacultyName { get; set; }
        public string FacultyAbbreviation { get; set; }
        public long DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentAbbreviation { get; set; }
        public long StudentFeeGroupId { get; set; }
        
        public AdmissionInformation AdmissionInformation { get; set; }
        public long AdmissionTermId { get; set; }
        public long AdmissionRoundId { get; set; }

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ValidSince { get; set; } = DateTime.Now;
        public string ValidSinceText => ValidSince?.ToString(StringFormat.ShortDate) ?? "";

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ValidUntil { get; set; }
        public string ValidUntilText => ValidUntil?.ToString(StringFormat.ShortDate) ?? "";

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? PaymentDueDate { get; set; } = DateTime.Now;
        public string PaymentDueDateText => PaymentDueDate?.ToString(StringFormat.ShortDate) ?? "";

        public string IdCardValidRange => $"{ ValidSince?.ToString(StringFormat.ShortDate) ?? "" } - { ValidUntil?.ToString(StringFormat.ShortDate) ?? "" }";
        public string ProfileImageURL { get; set; }
        public long ExaminationTypeId { get; set; }
        public string Address { get; set; }
        public StudentAddress CurrentAddress { get; set; }
        public StudentExemptedExamScore StudentExamScore { get; set; }
        public string HouseNumber { get; set; }
        public string Moo { get; set; }
        public string Soi { get; set; }
        public string Road { get; set; }
        public long? CountryId { get; set; }
        public long? ProvinceId { get; set; }
        public string ProvinceName { get; set; }
        public long? DistrictId { get; set; }
        public string DistrictName { get; set; }
        public long? SubDistrictId { get; set; }
        public string SubDistrictName { get; set; }
        public long? StateId { get; set; }
        public string StateName { get; set; }
        public long? CityId { get; set; }
        public string CityName { get; set; }
        public string ZipCode { get; set; }
        public string TelephoneNumber { get; set; }
        public string Email { get; set; }
        public string Nationality { get; set; }
        public string Religion { get; set; }
        public string Birthday { get; set; }
        public string BirthCountry { get; set; }
        public string BirthProvince { get; set; }
        public string BirthState { get; set; }
        public bool IsThai { get; set; }
        public int Age { get; set; }
        public string CitizenNumber { get; set; }
        public string PreviousBachelorSchool { get; set; }
        public string PreviousMasterSchool { get; set; }
        public string BachelorGraduatedYear { get; set; }
        public string MasterGraduatedYear { get; set; }
        public List<EmergencyDetail> EmergencyDetails { get; set; }
        public string Campus { get; set; }
        public int AcademicYear { get; set; }
        public string ProgramOfStudy { get; set; }
        public string Program { get; set; }
        public List<TemporaryCardFeeDetail> TemporaryCardFeeDetails { get; set; }
        public List<TemporaryCardDocumentDetail> TemporaryCardDocumentDetails { get; set; }
        public List<TemporarynExaminationDetail> TemporarynExaminationDetails { get; set; }
        public decimal TotalFee { get; set; }
        public string TotalFeeText => TotalFee.ToString(StringFormat.TwoDecimal);
        public string Code { get; set; }
        public string AcademicLevelName { get; set; }
        public string ImagePath { get; set; }
        public DateTime ExpiredDate { get; set; }
        public string ActiveAt { get; set; }
        public string ExpiredAt { get; set; }

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public string MidtermDate { get; set; }
        public string MidtermStart { get; set; }
        public string MidtermEnd { get; set; }
        public string FinalDate { get; set; }
        public string FinalStart { get; set; }
        public string FinalEnd { get; set; }
        public string ExaminationDate { get; set; }
        public string ExaminationTypeName { get; set; }
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string InstructorName { get; set; }
        public long SectionId { get; set; }
        public string SectionNumber { get; set; }
        public List<Section> Sections { get; set; }
        public List<ExamRow> ExamTable { get; set; }
        public DateTime DateIssued { get; set; } = DateTime.Now;
        public string DateIssuedString => $"{ DateIssued.ToString(StringFormat.ShortDate) }";
        public List<string> RequiredDocumentNames { get; set; }
        public string MidtermDateString => string.IsNullOrEmpty(MidtermStart) && string.IsNullOrEmpty(MidtermEnd) ? ""
                                           : !string.IsNullOrEmpty(MidtermStart) && string.IsNullOrEmpty(MidtermEnd) ? MidtermStart
                                           : string.IsNullOrEmpty(MidtermStart) && !string.IsNullOrEmpty(MidtermEnd) ? MidtermEnd
                                           : $"{ MidtermStart } - { MidtermEnd }";
        public string FinalDateString => string.IsNullOrEmpty(FinalStart) && string.IsNullOrEmpty(FinalEnd) ? ""
                                         : !string.IsNullOrEmpty(FinalStart) && string.IsNullOrEmpty(FinalEnd) ? FinalStart
                                         : string.IsNullOrEmpty(FinalStart) && !string.IsNullOrEmpty(FinalEnd) ? FinalEnd
                                         : $"{ FinalStart } - { FinalEnd }";
        public string ExaminationTime => ExaminationTypeName == "Midterm" ? MidtermDateString : FinalDateString;
        public string ExamDateAndTime => $"{ ExaminationDate } {( string.IsNullOrEmpty(ExaminationTime) ? "" : $"({ ExaminationTime })" )}";
        
    }

    public class StudentIdCardFormDetail
    {
        public long AcademicLevelId { get; set; }
        public long FacultyId { get; set; }
        public string Code { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ThaiFirstName { get; set; }
        public string ThaiLastName { get; set; }
        public string AcademicLevel { get; set; }
        public string FacultyAbbreviation { get; set; }
        public string DepartmentAbbreviation { get; set; }
        public string Barcode { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string ExpiredDateString => ExpiredDate?.ToString(StringFormat.ShortDate);
        public string ProfileImageURL { get; set; }
        public string Faculty { get; set; }
    }

    public class ExamRow
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public DateTime ExamDate { get; set; }
        public string Course { get; set; }
        public string Section { get; set; }
    }

    public class TemporaryCardFeeDetail
    {
        public long FeeItemId { get; set; }
        public string FeeItem { get; set; }
        public decimal Amount { get; set; }
        public string AmountText => Amount.ToString(StringFormat.TwoDecimal);
    }

    public class TemporaryCardDocumentDetail
    {
        public string DocumentName { get; set; }
        public int Amount { get; set; }
        public int Submitted { get; set; }
        public string Remark { get; set; }
    }

    public class TemporarynExaminationDetail
    {
        public long AcademicLevelId { get; set; }
        public long FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public long AdmissionRoundId { get; set; }
        public string AdmissionExaminationType { get; set; }
        public string Campus { get; set; }
        public string Building { get; set; }
        public string Room { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Remark { get; set; }
    }

    public class EmergencyDetail
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Relationship { get; set; }
    }
}