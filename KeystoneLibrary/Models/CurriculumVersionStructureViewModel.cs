namespace KeystoneLibrary.Models
{
    public class CurriculumVersionStructureViewModel
    {
        public long StudentId { get; set; }
        public long AcademicLevelId { get; set; }
        public long CurriculumId { get; set; }
        public long CurriculumVersionId { get; set; }
        public string AcademicLevel { get; set; }
        public string Department { get; set; }
        public string Faculty { get; set; }
        public string Curriculum { get; set; }
        public string CurriculumVersion { get; set; }
        public string ImplementedTerm { get; set; }    
        public string OpenedTerm { get; set; }   
        public string ClosedTerm { get; set; }  
        public int MinimumTerm { get; set; }  
        public int MaximumTerm { get; set; }   
        public int TotalCredit { get; set; }
        public List<CourseGroupStructureViewModel> CourseGroups { get; set; }
        public CurriculumVersionStructureViewModel()
        {
            CourseGroups = new List<CourseGroupStructureViewModel>();
        }
    }

    public class CourseGroupStructureViewModel
    {
        public long CourseGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TotalCredit { get; set; }
        public List<CourseGroupStructureViewModel> CourseGroups { get; set; }
        public List<CourseStructureViewModel> Courses { get; set; }

        public CourseGroupStructureViewModel()
        {
            CourseGroups = new List<CourseGroupStructureViewModel>();
            Courses = new List<CourseStructureViewModel>();
        }
    }

    public class CourseStructureViewModel
    {
        public long CourseId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Credit { get; set; }
        public bool IsRequiredGrade { get; set; }
        public string Grade { get; set; }
        public List<string> Prerequisites { get; set; }
    }
}