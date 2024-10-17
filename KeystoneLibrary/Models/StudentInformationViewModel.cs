using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models
{
    public class StudentInformationViewModel
    {
        public Guid StudentId { get; set; }
        public string StudentCode { get; set; }
        public string StudentStatus { get; set; } // a = admission, s = studying, d = delete, rs = resign, dm = dismiss
        public string TitleEn { get; set; }
        public string TitleTh { get; set; }
        public string FirstNameTh { get; set; }
        public string FirstNameEn { get; set; }
        public string MidNameTh { get; set; }
        public string MidNameEn { get; set; }
        public string LastNameTh { get; set; }
        public string LastNameEn { get; set; }
        public string Email { get; set; }
        public string TelephoneNumber { get; set; }
        public string MobileNumber { get; set; }
        public string FacultyCode { get; set; }
        public string FacultyNameEn { get; set; }
        public string FacultyNameTh { get; set; }
        public long FacultyId { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentNameEn { get; set; }
        public string DepartmentNameTh { get; set; }
        public long DepartmentId { get; set; }
        public string CurriculumVersionCode { get; set; }
        public string CurriculumVersionNameEn { get; set; }
        public string CurriculumVersionNameTh { get; set; }
        public long CurriculumVersionId { get; set; }
        public string CitizenNumber { get; set; }
        public string Passport { get; set; }
        public string NationalityNameEn { get; set; }
        public string NationalityNameTh { get; set; }
        public string ProfileImageURL { get; set; }
        public int TotalCredit { get; set; } // credit from curriculum
        public int CreditComp { get; set; }
        public int? CreditEarn { get; set; } // Credit Registration
        public decimal GPA { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public string AdvisorTitleEn { get; set; }
        public string AdvisorTitleTh { get; set; }
        public string AdvisorFirstNameTh { get; set; }
        public string AdvisorFirstNameEn { get; set; }
        public string AdvisorLastNameTh { get; set; }
        public string AdvisorLastNameEn { get; set; }
        public string CurrentScholarshipNameEn { get; set; }
        public bool IsCurrentStudentProbation { get; set; }
        public bool IsStudentExtended { get; set; }
        public string StudentFeeTypeEn { get; set; }
        public string ResidentTypeEn { get; set; }
        public DateTime? GraduatedAt { get; set; }
        public string AdmissionTermText { get; set; }
        public string GraduatedAtText => GraduatedAt?.ToString(StringFormat.ShortDate);
        public List<StudentAddress> StudentAddresses { get; set; } = new List<StudentAddress>();
        public string FullNameEn => string.IsNullOrEmpty(MidNameEn) ? $"{ TitleEn } { FirstNameEn } { LastNameEn }"
                                                                    : $"{ TitleEn } { FirstNameEn } { MidNameEn } { LastNameEn }";


        public string FullNameTh => string.IsNullOrEmpty(MidNameTh) ? $"{ TitleTh } { FirstNameTh } { LastNameTh }"
                                                                    : $"{ TitleTh } { FirstNameTh } { MidNameTh } { LastNameTh }";

        public string AdvisorFullNameEn => $"{ AdvisorTitleEn } { AdvisorFirstNameEn } { AdvisorLastNameEn }";


        public string AdvisorFullNameTh => $"{ AdvisorTitleTh } { AdvisorFirstNameTh } { AdvisorLastNameTh }";

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

        public string DepartmentCodeAndName => $"{ DepartmentCode } - { DepartmentNameEn }";
        public string DepartmentCodeAndNameTh => $"{ DepartmentCode } - { DepartmentNameTh }";
        public string FacultyCodeAndName => $"{ FacultyCode } - { FacultyNameEn }";
        public string FacultyCodeAndNameTh => $"{ FacultyCode } - { FacultyNameTh }";
        public string CurriculumVersionCodeAndName => $"{ CurriculumVersionCode } - { CurriculumVersionNameEn }";
        public string CurriculumVersionCodeAndNameTh => $"{ CurriculumVersionCode } - { CurriculumVersionNameTh }";
        public string GPAString => GPA.ToString(StringFormat.TwoDecimal);
    }
}