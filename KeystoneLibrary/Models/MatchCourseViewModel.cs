using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Admission;

namespace KeystoneLibrary.Models
{
    public class MatchCourseViewModel
    {
        public long CurrentTermId { get; set; }
        public long CourseId { get; set; }
        public long StudentFeeGroupId { get; set; }
        public Guid StudentId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string AcademicLevel { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string CurriculumVersion { get; set; }
        public string AdmissionType { get; set; }
        public string AdmissionTerm { get; set; }
        public string EntranceExamResult { get; set; }
        public string StudentFeeGroup { get; set; }
        public List<RegistrationMatchCourse> RegistrationMatchCourses { get; set; } = new List<RegistrationMatchCourse>();
        public List<MatchCourseTermFee> MatchCourseTermFees { get; set; } = new List<MatchCourseTermFee>();
        public List<StudentDocument> StudentDocuments { get; set; } = new List<StudentDocument>();
        public List<StudentExemptedExamScore> StudentExemptedExamScores { get; set; } = new List<StudentExemptedExamScore>();
        public List<ExemptedExaminationScore> ExemptedExaminationScores { get; set; } = new List<ExemptedExaminationScore>();
        public List<Section> Sections { get; set; } = new List<Section>();
        public string EntranceExamResultText
        {
            get
            {
                switch (EntranceExamResult)
                {
                    case "p":
                        return "Pass";
                    case "f":
                        return "Fail";
                    case "n":
                        return "Non test or no result";
                    default:
                        return "N/A";
                }
            }
        }

        public SelectList SectionSelectList
        {
            get
            {
                if (Sections != null && Sections.Any())
                {
                    var sections = Sections.Select(x => new SelectListItem
                                                    {
                                                        Text = x.Number,
                                                        Value = x.Id.ToString()
                                                    });
                    return new SelectList(sections, "Value", "Text");
                }
                return new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }
    }

    public class RegistrationMatchCourse
    {
        public long RegistrationCourseId { get; set; }
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public long? SectionId { get; set; }
        public string SectionNumber { get; set; }
        public bool IsPaid { get; set; }
    }

    public class MatchCourseTermFee
    {
        public long FeeItemId { get; set; }
        public decimal Amount { get; set; }
    }
}