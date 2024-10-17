using KeystoneLibrary.Models.Report;

namespace KeystoneLibrary.Models
{
    public class StudentTransferViewModel
    {
        public StudentTransferViewModel() { }
        public StudentTransferViewModel(long _transferUniversityId)
        {
            TransferUniversityId = _transferUniversityId;
        }

        public string StudentCode { get; set; }
        public Guid StudentId { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public string AcademicLevel { get; set; }
        public string FacultyName { get; set; }
        public string DepartmentName { get; set; }
        public int Credit { get; set; }
        public decimal GPA { get; set; }
        public long CurriculumId { get; set; }
        public string CurriculumName { get; set; }
        public long CurriculumVersionId { get; set; }
        public string CurriculumVersionName { get; set; }
        public string NewCurriculumVersionName { get; set; }
        public int ChangedMajorCount { get; set; }
        public bool CountChangedTime { get; set; }
        public List<StudentTransferCourseViewModel> StudentCourses { get; set; } = new List<StudentTransferCourseViewModel>();
        public List<StudentCourseEquivalentViewModel> StudentCourseEquivalents { get; set; } = new List<StudentCourseEquivalentViewModel>();
        public List<StudentCourseCategorizationViewModel> StudentCourseCategorizations { get; set; } = new List<StudentCourseCategorizationViewModel>();
        public List<StudentCourseCategorizationViewModel> SummaryOldCurriculumCourseGroups { get; set; } = new List<StudentCourseCategorizationViewModel>();
        public List<StudentCourseCategorizationViewModel> SummaryNewCurriculumCourseGroups { get; set; } = new List<StudentCourseCategorizationViewModel>();
        public List<CourseGroupViewModel> CurrentCurriculumCourseGroupViewModels { get; set; } = new List<CourseGroupViewModel>();
        public List<CourseGroupViewModel> NewCurriculumCourseGroupViewModels { get; set; } = new List<CourseGroupViewModel>();
        public bool HasCoursesInNoCourseGroup { get; set; }
        public long? TransferUniversityId { get; set; }
    }

    public class StudentTransferCourseViewModel
    {
        public long RegistrationCourseId { get; set; }
        public long TermId { get; set; }
        public string TermText { get; set; }
        public long CourseId { get; set; }
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public long SectionId { get; set; }
        public string SectionNumber { get; set; }
        public long? GradeId { get; set; }
        public string GradeName { get; set; }
        public long CourseGroupId { get; set; }
        public string CourseGroupName { get; set; }
        public bool IsNewCourse { get; set; }
    }

    public class StudentCourseEquivalentViewModel
    {
        public long RegistrationCourseId { get; set; }
        public long? CurrentCourseId { get; set; }
        public string CurrentCourseName { get; set; }
        public string CurrentCourseGrade { get; set; }
        public long TermId { get; set; }
        public string TermText { get; set; }
        public long SectionId { get; set; }
        public long NewCourseId { get; set; }
        public string NewCourseCode { get; set; }
        public string NewCourseName { get; set; }
        public long? GradeId { get; set; }
        public long? NewGradeId { get; set; }
        public string GradeName { get; set; }
        public string PreviousGrade { get; set; }
        public long CurriculumCourseGroupId { get; set; }
        public string CourseGroupName { get; set; }
        public bool IsStarCourse { get; set; }
        public bool InCurriculum { get; set; }
        public string IsChecked { get; set; }
        public SelectList CourseSelectList { get; set; }
        public SelectList CourseGroupSelectList { get; set; }
    }

    public class StudentCourseCategorizationViewModel
    {
        public long CourseGroupId { get; set; }
        public string CourseGroupName { get; set; }
        public List<StudentCourseCategorizationDetail> courseList { get; set; } = new List<StudentCourseCategorizationDetail>();

    }

    public class StudentCourseCategorizationDetail
    {
        public long? ExternalCourseId { get; set; }
        public long CourseId { get; set; }
        public long CourseGroupId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string CourseNameAndCredit { get; set; }
        public string CourseFullName { get; set; }
        public long? GradeId { get; set; }
        public string GradeName { get; set; }
        public long TermId { get; set; }
        public long? RegistrationCourseId { get; set; }
        public string PreviousGrade { get; set; }
        public bool IsStarCourse { get; set; }
    }
}