using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class GradingScoreSummaryViewModel
    {
        public long TermId { get; set; }
        public long AcademicLevelId { get; set; }
        public string ReturnUrl { get; set; }
        public long InstructorId { get; set; }
        public long GradingAllocationId { get; set; }
        public long StandardGradingGroupId { get; set; }
        public List<long> CourseIds { get; set; } = new List<long>();
        public List<Course> Courses { get; set; } = new List<Course>();
        public decimal MaxScore { get; set; }
        public string JsonAllocations { get; set; }
        public string JsonStudentScores { get; set; }
        public List<Allocation> Allocations { get; set; } = new List<Allocation>();
        public decimal TotalAllocationScore => Allocations?.Sum(x => x.FullScore) ?? 0;
        public List<StudentBySectionViewModel> StudentScores { get; set; } = new List<StudentBySectionViewModel>();
        public List<StudentBySectionViewModelGroupByGradeTemplate> StudentScoresGroupByGradeTemplate { get; set; } = new List<StudentBySectionViewModelGroupByGradeTemplate>();
        public List<StudentBySectionViewModel> StudentScoresResult { get; set; } = new List<StudentBySectionViewModel>();
        public List<GradingScoreSummaryDetailViewModel> GradingScores { get; set; } = new List<GradingScoreSummaryDetailViewModel>();
        public Guid InstructorGuid { get; set; }
        public List<GradingScoreSummaryDetailViewModel> GradingStatuses { get; set; } = new List<GradingScoreSummaryDetailViewModel>();
        public bool IsCoordinator { get; set; }
        public string BuildNumber { get; set; }
        public List<StandardGradingGroup> StandardGradingGroups { get; set; }
        public List<StandardGradingGroup> SelectedStandardGradingGroups { get; set; }
        public List<long> ResetCourseIds { get; set; } = new List<long>();
        public long ResetTermId { get; set; }
        public long ResetAcademicLevelId { get; set; }
        public long ResetGradingAllocationId { get; set; }
        public long ResetStandardGradingGroupId { get; set; }
        public int StudentScoresCount { get; set; }
        public int WithdrawCount { get; set; }
        public int TotalStudent { get; set; }
        public IFormFile UploadFile { get; set; }
    }
    public class GradingScoreSummaryDetailViewModel
    {
        public long TermId { get; set; }
        public long CourseId { get; set; }
        public long InstructorId { get; set; }
        public string CourseNames { get; set; }
        public string SectionNumbers { get; set; }
        public bool IsAllocated { get; set; }
        public bool IsScored { get; set; }
        public bool IsGraded { get; set; }
        public string Barcode { get; set; }
        public bool IsBarcodeGenereated { get; set; }
        public bool IsPublished { get; set; }
        public string LastUpdate { get; set; }
        public string JointSections { get; set; }
        public int SkipGrading { get; set; }
        public int TotalStudent { get; set; }
        public int TotalStudentScoring { get; set; }
        public int Withdrawal { get; set; }
        public int Published { get; set; }
    }

    public class GradeLogsViewModel
    {
        public string Student { get; set; }
        public string Course { get; set; }
        public List<GradingLogsViewModel> GradingLogs { get; set; } = new List<GradingLogsViewModel>();
    }

    public class GradingLogsViewModel 
    {
        public string PreviousGrade { get; set; }
        public string CurrentGrade { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtText { get; set; }
        public string Type { get; set; }
        public string Remark  { get; set; }
    }

    public class GradingStudentRawScoreViewModel
    {
        public string CurrentGrade { get; set; }
        public long TermId { get; set; }
        public long CourseId { get; set; }
        public string Student { get; set; }
        public string Course { get; set; }
        public bool IsGradeMember { get; set; }
        public long? CurrentGradeId { get; set; }
        public List<GradeSelectViewModal> Grades { get; set; }
    }

    public class GradeSelectViewModal
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}