using System.ComponentModel.DataAnnotations;
using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models.Report
{
    public class CertificationViewModel
    {
        public long StudentCertificateId { get; set; }
        public string ReferenceNumber { get; set; }
        public string Language { get; set; }
        public string CertificationType { get; set; }
        public string Purpose { get; set; }
        public Guid StudentId { get; set; }
        public string StudentCode { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public bool IsUrgent { get; set; }

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime BirthDate { get; set; }
        public long VerifyCode { get; set; }
        public long AcademicLevelId { get; set; }
        public string AcademicLevelName { get; set; } = "N/A";
        public int Year { get; set; }
        public long ApprovedBy { get; set; }
        public string ApprovedByName { get; set; }
        public string Position { get; set; }
        public List<long> SignIds { get; set; }
        public List<String> Signs { get; set; }
        public List<String> Positions { get; set; }
        public int StudyYear { get; set; } = 1;
        public long FacultyId { get; set; }
        public string FacultyName { get; set; } = "N/A";
        public long DepartmentId { get; set; }
        public string DepartmentName { get; set; } = "N/A";
        public int? AdmissionYear { get; set; }
        public long TitleId { get; set; }
        public string Title { get; set; }
        public decimal GPA { get; set; } = new decimal(0);
        public string AcademicHonor { get; set; }
        public int? GraduatedYear { get; set; }
        public string Pronoun { get; set; }
        public string Possessive { get; set; }
        public int CreditComp { get; set; }
        public string ErrorMessage { get; set; }
        public string CreatedAtString { get; set; } = DateTime.Now.ToString(StringFormat.ShortDate);

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime AdmissionDate { get; set; }

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? GraduatedAt { get; set; }
        public int CreditEarned { get; set; }
        public int TotalCredit { get; set; }
        public List<long> CourseIds { get; set; }
        public List<string> CourseCodeAndNames { get; set; }
        public string DegreeName { get; set; }

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime CeremonyAt { get; set; } = DateTime.Now;
        public int RegistringCredit { get; set; }
        public int Amount { get; set; }
        public decimal DocumentFee { get; set; }
        public bool IsRequiredPayment { get; set; }
        public bool IsPaid { get; set; }
        public int Gender { get; set; }
        public int RunningNumber { get; set; }
        public int DocumentYear { get; set; }
        public Receipt Receipt { get; set; }

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime PaidAt { get; set; }
        public long ReceiptId { get; set; }
        public string ReceiptNumber { get; set; }
        public long TermId { get; set; }
        public string TermText { get; set; }
        public int AcademicTerm { get; set; }
        public int AcademicYear { get; set; }
        public decimal IELTSScore { get; set; }
        public long AbroadCountryId { get; set; }
        public string AbroadCountry { get; set; }

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime AbroadFromDate { get; set; } = DateTime.Now;

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime AbroadToDate { get; set; } = DateTime.Now;
        public int AllMajorRank { get; set; }
        public int AllFacultyRank { get; set; }
        public int AllEnrolleeRank { get; set; }
        public int MajorRank { get; set; }
        public int FacultyRank { get; set; }
        public int EnrolleeRank { get; set; }
        public string ChangedName { get; set; }
        public string ChangedSurname { get; set; }
        public bool IsAdmissionStudent { get; set; }
        public string ChangeNameType { get; set; }
        public string ChangeNameTypeText { get; set; }
        public string StudentYear 
        {
            get
            {
                switch (StudyYear)
                {
                    case 1:
                        return "freshman";

                    case 2:
                        return "sophomore";

                    case 3:
                        return "junior";

                    case 4:
                        return "senior";

                    default:
                        return "";
                }
            }
        }
    }
}