using KeystoneLibrary.Models.DataModels.Curriculums;

namespace KeystoneLibrary.Models
{
    public class StudyPlanViewModel
    {
        public long Id { get; set; }
        public int Year { get; set; }
        public int Term { get; set; }
        public CurriculumVersion CurriculumVersion { get; set; }
        public List<CoursePlan> CoursesPlan { get; set; }
        public List<StudyCourse> StudyCourses { get; set; }
    }

    public class CoursePlan
    {
        public long Id { get; set; } // Course Id
        public long StudyCourseId { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public int Credit { get; set; }
    }

    public class StudyPlanDetailViewModel
    {
        public long Id { get; set; }
        public long CurriculumVersionId { get; set; }
        public int Year { get; set; }
        public int Term { get; set; }
        public int TotalCredit { get; set; }
        public List<CoursePlan> CoursesPlan { get; set; }
        public string Description
        {
            get
            {
                return $"Year { Year } Term { Term }";
            }
        }
    }

    public class CurriculumStudyPlanViewModel
    {
        public int Year { get; set; }
        public int TotalCredit { get; set; }
        public long CurriculumVersionId { get; set; }
        public List<StudyPlan> StudyPlans { get; set; }

        private readonly string[] yearString = new string[] { "Zero", "First", "Second", "Third", "Fouth",
                                                              "Fifth", "Sixth", "Seventh", "Eighth", "Ninth", "Tenth" };

        public string YearText 
        { 
            get
            {
                string year = Year > yearString.Length ? "" : yearString[Year];
                return $"{ year } Year";
            }
        }

        public string CurriculumVersion { get; set; }
        public string AcademicProgram { get; set; }
        public string SpecializationGroup { get; set; }
    }
}