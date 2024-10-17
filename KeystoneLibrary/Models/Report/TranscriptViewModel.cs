using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.Report
{
    public class TranscriptViewModel
    {
        public Criteria Criteria { get; set; }
        public List<TranscriptInformation> Transcripts { get; set; }
    }

    public class TranscriptInformation
    {
        public DateTime? CreatedAt { get; set; }
        public string CreatedAtText => CreatedAt?.ToString(StringFormat.ShortDate);
        public string Time => CreatedAt?.ToString(StringFormat.TimeSec);
        public string Language { get; set; }
        public string Purpose { get; set; }
        public bool IsUrgent { get; set; }
        public Guid StudentId { get; set; }
        public string StudentCode { get; set; }
        public string FirstName { get; set; }
        public string MidName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Address1 { get; set; } // house number, road
        public string Address2 { get; set; } // subdistrict, district
        public string Address3 { get; set; } // province, post number
        public string PhoneNumber { get; set; }

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? BirthDateRender { get; set; }
        public string BirthPlace { get; set; }
        public string Nationality { get; set; }
        public string Religion { get; set; }

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? AdmissionAtRender { get; set; }
        public string AdmissionAt { get; set; }
        public string AcademicLevel { get; set; }
        public string EducationBackground { get; set; }
        public string PreviousSchool { get; set; }
        public string PreviousSchoolCountry { get; set; }
        public string PreviousGraduatedYear { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public long CurriculumId { get; set; }
        public long CurriculumVersionId { get; set; }
        public string Curriculum { get; set; }
        public string CurriculumVersion { get; set; }
        public string Minor { get; set; } // if null or empty not show
        public string SecondMinor { get; set; } // if null or empty not show
        public string Concentration { get; set; } // if null or empty not show
        public string SecondConcentration { get; set; } // if null or empty not show
        public bool IsGraduated { get; set; }
        public string StudentStatus { get; set; }

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? GraduatedAtRender { get; set; }
        public string GraduatedAt { get; set; }
        
        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? StatusAt { get; set; }
        public string GraduateClass { get; set; }
        public string GraduateTerm { get; set; }
        public string Degree { get; set; }
        public string Award { get; set; }
        public int TotalCreditCompleted { get; set; } // 3 digits
        public int TotalCreditAudited { get; set; }
        public int TotalCreditExempted { get; set; }
        public int TotalCreditTransferred { get; set; } // 3 digits
        public int TotalFoundationCourseCredit { get; set; }
        public int TotalIndipendentStudyCompleted { get; set; }
        public int TotalPhDDissertationCompleted { get; set; }
        public int TotalThesisCompleted { get; set; }
        public int TotalCreditEarnd { get; set; } // 3 digits
        public string ApprovedBy { get; set; }
        public string ProjectType { get; set; }
        public List<string> ProjectNames { get; set; }
        public string ProfileImageURL { get; set; }
        public decimal TransferGPTS { get; set; }
        public string GPTSString => TransferGPTS.ToString(StringFormat.TwoDecimal);
        public int CumulativeCredit { get; set; }
        public decimal CumulativeGPTS { get; set; }
        public string CumulativeGPTSString => CumulativeGPTS.ToString(StringFormat.TwoDecimal);
        public decimal CumulativeGPA { get; set; }
        public string CumulativeGPAString => CumulativeGPA.ToString(StringFormat.TwoDecimal);
        public decimal CreditCompleted { get; set; }
        public List<TransferUniversity> TransferedUniversity { get; set; }
        public List<TranscriptTerm> TranscriptTerms { get; set; } = new List<TranscriptTerm>();
        public List<TranscriptTerm> Transfer { get; set; } = new List<TranscriptTerm>();
        public List<TranscriptTerm> TransferWithCourse { get; set; } = new List<TranscriptTerm>();
        public List<TranscriptTerm> TranscriptCoursesTransfer { get; set; } = new List<TranscriptTerm>();
        public string BirthDate 
        { 
            get
            {
                if(Language == "th")
                {
                    System.Globalization.CultureInfo _cultureTHInfo = new System.Globalization.CultureInfo("th-TH");
                    return BirthDateRender?.ToString("d MMMM yyyy", _cultureTHInfo);
                }else
                {
                    System.Globalization.CultureInfo _cultureENInfo = new System.Globalization.CultureInfo("en-EN");
                    return BirthDateRender?.ToString("MMMM d, yyyy", _cultureENInfo);
                }
            }
        }
        public string StatusAtText
        {
            get
            {
                System.Globalization.CultureInfo _cultureENInfo = new System.Globalization.CultureInfo("en-EN");
                return StatusAt?.ToString("MMMM d, yyyy", _cultureENInfo);
            }
        }
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
    }

    public class TranscriptTerm
    {
        public long TermId { get; set; }
        public int AcademicYear { get; set; }
        public int AcademicTerm { get; set; }
        public int TermCredit { get; set; }
        public int TermCreditRegis { get; set; }
        public bool IsSummer { get; set; }
        public bool IsTransfer { get; set; }
        public decimal GPTS { get; set; } // ค่าระดับ
        public string GPTSString => GPTS.ToString(StringFormat.TwoDecimal);
        public decimal TermGPATranscript { get; set; }
        public decimal TermGPA
        {
            get
            {
                if (SumGrade != 0 && TermCredit != 0)
                {
                    return Decimal.Round(SumGrade/TermCredit, 2);
                }
                return 0;
            }
        }
        public string TermGPAString => TermGPA.ToString(StringFormat.TwoDecimal);
        public string TermGPATranscriptString => TermGPATranscript.ToString(StringFormat.TwoDecimal);
        public int CumulativeCredit { get; set; }
        public decimal CumulativeGPTS { get; set; }
        public string CumulativeGPTSString => CumulativeGPTS.ToString(StringFormat.TwoDecimal);
        public decimal TotalCredit { get; set; }
        public string TotalCreditString => TotalCredit.ToString(StringFormat.NumberString);
        public decimal TotalRegistrationCredit { get; set; }
        public decimal TotalRegistrationCreditFromCredit { get; set; }
        public string TotalRegistrationCreditString => TotalRegistrationCredit.ToString(StringFormat.NumberString);
        public string TotalRegistrationCreditFromCreditString => TotalRegistrationCreditFromCredit.ToString(StringFormat.NumberString);
        public decimal CumulativeCreditRegis { get; set; }
        public decimal CumulativeCreditRegisFromCredit { get; set; }
        public string CumulativeCreditRegisString => CumulativeCreditRegis.ToString(StringFormat.NumberString);
        public string CumulativeCreditRegisFromCreditString => CumulativeCreditRegisFromCredit.ToString(StringFormat.NumberString);
        public decimal CumulativeCreditComp { get; set; }
        public string CumulativeCreditCompString => CumulativeCreditComp.ToString(StringFormat.NumberString);
        public decimal CumulativeGPA { get; set; }
        public string CumulativeGPAString => CumulativeGPA.ToString(StringFormat.TwoDecimal);
        public decimal CreditCompleted { get; set; }
        public List<TranscriptCourse> TranscriptCourses { get; set; } = new List<TranscriptCourse>();
        public int TranscriptCoursesCount => TranscriptCourses.Any() ? TranscriptCourses.Count() : 0 ;
        public decimal SumGrade { get; set; }
    }

    public class TranscriptCourse
    {
        public string CourseCode { get; set; }
        public string CourseName1 { get; set; }
        public string CourseName2 { get; set; } // if null or empty not show
        public string CourseName3 { get; set; } // if null or empty not show
        public string CreditText { get; set; }
        public List<string> CourseNames
        {
            get
            {
                var names = new List<string>();
                names.Add($"{ CourseName1 } { CourseName2 } { CourseName3 }");
                var output = names.Where(x => !string.IsNullOrEmpty(x))
                                  .ToList();
                return output;
            }
        }

        public int CourseNameCount => CourseNames == null ? 0 : CourseNames.Count();
        public int Credit { get; set; }
        public int RegistrationCredit { get; set; }
        public string Grade { get; set; }
        public decimal? GradeWeight { get; set; }
        public string Section { get; set; }
        public long? TrasferUniversityId { get; set; }
    }
}