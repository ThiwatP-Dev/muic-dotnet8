namespace KeystoneLibrary.Models.Report
{
    public class GradingLogGroupReportViewModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public List<GradingLogDetail> Details { get; set; }
    }

    public class GradingLogReportViewModel
    {
        public string StudentCode { get; set; }              
        public string StudentTitle { get; set; }                   
        public string StudentFirstNameEn { get; set; }                   
        public string StudentLastNameEn { get; set; }              
        public string StudentMidNameEn { get; set; }                 
        public string Faculty { get; set; }        
        public string Department { get; set; } 
        public string CourseCode { get; set; }
        public int CourseCredit { get; set; }
        public decimal CourseLecture { get; set; }
        public decimal CourseLab { get; set; }
        public decimal CourseOther { get; set; }
        public string CourseNameEn { get; set; }
        public string Section { get; set; }
        public string PreviousGrade { get; set; }
        public string CurrentGrade { get; set; } 
        public string ApprovedAt { get; set; }
        public string ApprovedBy { get; set; }
        public string StudentFullName => string.IsNullOrEmpty(StudentMidNameEn) ? $"{ StudentTitle } { StudentFirstNameEn } { StudentLastNameEn }"
                                                                                : $"{ StudentTitle } { StudentFirstNameEn } { StudentMidNameEn } { StudentLastNameEn }";

        public string CreditText => $"{ CourseCredit.ToString(StringFormat.GeneralDecimal) } ({ CourseLecture.ToString(StringFormat.GeneralDecimal) }-{ CourseLab.ToString(StringFormat.GeneralDecimal) }-{ CourseOther.ToString(StringFormat.GeneralDecimal) })";
        public long? CourseRateId { get; set; }
        public string Remark { get; set; }
        public string SpecialChar => CourseRateId == 2 ? "**" : "";
        public string CodeAndSpecialChar => $"{ CourseCode }{SpecialChar}";
        public string CourseCodeAndCredit => $"{ CodeAndSpecialChar } { CreditText }";
    }

    public class GradingLogDetail
    {
        public string Course { get; set; }
        public string CourseName { get; set; }
        public string Section { get; set; }
        public string PreviousGrade { get; set; }
        public string CurrentGrade { get; set; }
        public string ApprovedAt { get; set; }
        public string ApprovedBy { get; set; }
        public string Remark { get; set; }
    }

    public class GradingLogViewModel
    {
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public string Course { get; set; }
        public string Section { get; set; }
        public List<GradingLogDetail> Details { get; set; }
    }
}