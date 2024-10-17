namespace KeystoneLibrary.Models.Report
{
    public class FinalExaminationSubmissionViewModel
    {
        public Criteria Criteria { get; set; }
        public List<FinalExaminationSubmission> Results { get; set; }
    }

    public class FinalExaminationSubmission
    {
        public long SectionId { get; set; }
        public long CourseId { get; set; }
        public string FacultyNameEn { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string SectionNumber { get; set; }
        public string InstructorTitle { get; set; }
        public string InstructorFirstName { get; set; }
        public string InstructorLastName { get; set; }
        public string JointSections { get; set; }
        public int RegisteredStudent { get; set; }
        public int PublishedStudent { get; set; }
        public int WithdrawStudent { get; set; }
        public int GradeEnteredStudent { get; set; }
        public int CourseCredit { get; set; }
        public decimal CourseLecture { get; set; }
        public decimal CourseOther { get; set; }
        public decimal CourseLab { get; set; }
        public bool IsSpecialCase { get; set; }
        public bool IsOutbound { get; set; }
        public long? CourseRateId { get; set; }
        public string SpecialChar => CourseRateId == 2 ? "**" : "";
        public DateTime? FinalDate { get; set; }
        public string FinalDateText => FinalDate?.ToString(StringFormat.ShortDate);
        public DateTime? SubmissionAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string SubmissionAtText => SubmissionAt?.ToString(StringFormat.ShortDate);
        public string ApprovedAtText => ApprovedAt?.ToString(StringFormat.ShortDate);
        public DateTime? DueAt { get; set; }
        public string DueAtText => DueAt?.ToString(StringFormat.ShortDate);
        public bool IsPending => (PublishedStudent + GradeEnteredStudent) < RegisteredStudent;
        public bool IsOntime => (DueAt == null || SubmissionAt == null) || SubmissionAt.Value.Date <= DueAt.Value.Date;
        public string StatusText => IsPending ? "Pending" : SubmissionAt == null ? string.Empty : IsOntime ? "On Time" : "Late";
        public string InstructorFullNameEn => string.IsNullOrEmpty(InstructorFirstName) ? string.Empty : $"{ InstructorTitle } { InstructorFirstName } { InstructorLastName }";
        public string CourseCodeAndCredit => $"{ CourseCode }{ SpecialChar } { CourseCredit.ToString(StringFormat.GeneralDecimal) } ({ CourseLecture.ToString(StringFormat.GeneralDecimal) }-{ CourseLab.ToString(StringFormat.GeneralDecimal) }-{ CourseOther.ToString(StringFormat.GeneralDecimal) })";
        public string SectionTypes 
        { 
            get 
            {
                string result = "";
                if (IsSpecialCase)
                {
                    result += "Ghost";
                }

                if (IsOutbound)
                {
                    result += string.IsNullOrEmpty(result) ? "" : ", ";
                    result += "Outbound";
                }

                return string.IsNullOrEmpty(result) ? "" : "(" + result + ")";
            }
        }
    }
}