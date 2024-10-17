namespace KeystoneLibrary.Models.Report
{
    public class CourseGroupViewModel
    {
        public int Sequence { get; set; }
        public long CourseGroupId { get; set; }
        public long GraduatingRequestId { get; set; }
        public long CurriculumVersionId { get; set; }
        public string NameEn { get; set; }
        public string FullPathName { get; set; }
        public string DescriptionEn { get; set; }
        public string SpecializationName { get; set; }
        public string SpecializationType { get; set; }
        public int RequiredCreditCompleted { get; set; }
        public int TotalCreditsCompleted { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsNone { get; set; }
        public bool IsAutoAssignGraduationCourse { get; set; }
        public bool IsMinor { get; set; }
        public List<CourseGroupViewModel> Children { get; set; }
        public List<CourseGroupCourseViewModel> Courses { get; set; }
    }

    public class CourseGroupCourseViewModel
    {
        public long Id { get; set; }
        public long CourseId { get; set; }
        public string TermText { get; set; }
        public string CourseCode { get; set; }
        public string CourseNameEn { get; set; }
        public bool IsTransferCourse { get; set; }
        public bool IsStarCourse { get; set; }
        public int Credit { get; set; }
        public string CreditText { get; set; }
        public decimal? RequiredGradeWeight { get; set; }
        public string RequiredGradeName { get; set; }
        public decimal? RegisteredGradeWeight { get; set; }
        public string RegisteredGradeName { get; set; }
        public long? MoveCourseGroupId { get; set; }
        public string MoveCourseGroup { get; set; }
        public bool IsCourseEquivalent { get; set; }
        public string CourseEquivalentName { get; set; }
        public bool IsPassed => RegisteredGradeWeight != null && (RegisteredGradeWeight ?? 0) >= (RequiredGradeWeight ?? 0);
        public bool IsAddManually { get; set; }
        public long GraduatingRequestId { get; set; }
        public long CourseModificationId { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsMoved { get; set; }
        public bool IsPlanned { get; set; }
        public bool IsRequired { get; set; }
        public string Remark { get; set; }
        public List<CourseGroupCourseGradeViewModel> Grades { get; set; }
    }

    public class CourseGroupCourseGradeViewModel
    {
        public long CourseId { get; set; }
        public string TermText { get; set; }
        public bool IsTransferCourse { get; set; }
        public bool IsStarCourse { get; set; }
        public decimal? RequiredGradeWeight { get; set; }
        public string RequiredGradeName { get; set; }
        public decimal? RegisteredGradeWeight { get; set; }
        public string RegisteredGradeName { get; set; }
        public decimal Credit { get; set; }
        public bool IsGradePublished { get; set; }
        public bool IsPassed { get; set; }
    }

    public class CourseGroupingViewModel
    {
        public long GraduatingRequestId { get; set; }
        public long CourseGroupId { get; set; }
        public long CourseId { get; set; }
        public long MoveCourseGroupId { get; set; }
        public long? RequiredGradeId { get; set; }
        public string Remark { get; set; }
    }

    public class ResponseChangeCourseGroup
    {
        public string Status { get; set; }
        public string MoveCourseGroup { get; set; }
        public long MoveCourseGroupId { get; set; }
        public string Remark { get; set; }
    }
}