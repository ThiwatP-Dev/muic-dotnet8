namespace KeystoneLibrary.Models.Report
{
    public class LateRegistrationReportViewModel
    {
        public int AcademicYear { get; set; }
        public int AcademicTerm { get; set; }
        public string TermText => $"{ AcademicTerm }/{ AcademicYear }";
        public string Code { get; set; }
        public string TitleNameEn { get; set; }
        public string FirstNameEn { get; set; }
        public string MidNameEn { get; set; }
        public string LastNameEn { get; set; }
        public string FullNameEn => string.IsNullOrEmpty(MidNameEn) ? $"{ TitleNameEn } { FirstNameEn } { LastNameEn }"
                                                                    : $"{ TitleNameEn } { FirstNameEn } { MidNameEn } { LastNameEn }";
        public string StudentStatus { get; set; }
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
        
        public string DepartmentCode { get; set; }
        public string FacultyCode { get; set; }
        public string FacultyNameEn { get; set; }
        public string FacultyCodeAndName => $"{ FacultyCode } - { FacultyNameEn }";
        public string AdvisorTitleNameEn { get; set; }
        public string AdvisorFirstNameEn { get; set; }
        public string AdvisorLastNameEn { get; set; }
        public string AdvisorFullNameEn => $"{ AdvisorTitleNameEn } { AdvisorFirstNameEn } { AdvisorLastNameEn }";
        public int Credit { get; set; }
        public bool IsPaid { get; set; }
    }
}