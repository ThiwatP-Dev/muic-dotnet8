using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class GradeManagementViewModel
    {
        public long TermId { get; set; }
        public string TermText { get; set; }
        public string CourseText { get; set; }
        public string InstructorFullNameEn { get; set; }
        public long InstructorId { get; set; }
        public long AcademicLevelId { get; set; }
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
        public Guid InstructorGuid { get; set; }
        public List<GradingStatus> GradingStatuses { get; set; } = new List<GradingStatus>();
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
        public IFormFile UploadFile { get; set; }
    }

    public class StudentBySectionViewModelGroupByGradeTemplate
    {
        public long StandardGradingGroupId { get; set; }
        public StandardGradingGroup StandardGradingGroup { get; set; }
        public List<StudentBySectionViewModel> StudentBySectionViewModels {get; set;}
    }

    public class StudentBySectionViewModel
    {
        public bool IsSkipGrading { get; set; }
        public long StudentScoreId { get; set; }
        public long RegistrationCourseId { get; set; }
        public long SectionId { get; set; }
        public long CourseId { get; set; }
        public long? CurriculumVersionId { get; set; }
        public string CourseCode { get; set; }
        public string SectionNumber { get; set; }
        public Guid StudentId { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public List<Allocation> Scores { get; set; } = new List<Allocation>();
        public decimal TotalScore => Scores?.Sum(x => x.FullScore) ?? 0;
        public decimal Percentage { get; set; }
        public decimal PercentageForGPACalculation { get; set; }
        public bool IsWithdrawal { get; set; }
        public bool IsPaid { get; set; }
        public bool IsGradePublished { get; set; }
        public string IsWithdrawalText => IsWithdrawal ? "Yes" : "No";
        public string IsPaidText => IsPaid ? "Yes" : "No";
        public string IsGradePublishedText => IsGradePublished ? "Yes" : "No";
        public string IsEditScoreDisabled => IsGradePublished ? "disabled" : "";
        public string IsSkipGradingChecked => IsSkipGrading ? "checked" : "";
        public bool IsCheating { get; set; }
    }

    public class Allocation
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string Abbreviation { get; set; }
        public decimal? FullScore { get; set; }
    }

    public class GradeSection
    {
        public long SectionId { get; set; }
        public string ChildrenSectionIds { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string SectionNumber { get; set; }
        public int TotalStudent { get; set; }
        public int TotalWithdrawal { get; set; }
        public bool IsSelected { get; set; }
    }

    public class GradingStatus
    {
        public long AllocationId { get; set; }
        public string CourseIds { get; set; }
        public string CourseNames { get; set; }
        public string SectionNumbers { get; set; }
        public bool IsAllocated { get; set; }
        public bool IsScored { get; set; }
        public bool IsGraded { get; set; }
        public string Barcode { get; set; }
        public bool IsBarcodeGenereated { get; set; }
        public bool IsPublished { get; set; }
        public string LastUpdate { get; set; }
    }

    public class GradingReportViewModel
    {
        public string BarcodeNumber { get; set; }
        public bool IsSave { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string Semester { get; set; }
        public string AcademicTerm { get; set; }
        public string AcademicYear { get; set; }
        public string Course { get; set; }
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public long CourseId { get; set; }
        public string CourseCreitText { get; set; }
        public string PrintedAt { get; set; }
        public List<Course> Courses { get; set; }
        // public string CoursesString => string.Join(", ", Courses.Select(x => x.CodeAndName));
        public List<string> Sections { get; set; } = new List<string>();
        public string SectionString => string.Join(", ", Sections);
        public List<Instructor> Instructors { get; set; } = new List<Instructor>();
        public string InstructorsString => string.Join(", ", Instructors.Select(x => x.FullNameEn));
        public ClassStatistics ClassStatistics { get; set; }
        public List<GradingRange> GradingRanges { get; set; } = new List<GradingRange>();
        public GradingRangeTotal GradingRangeTotal
        {
            get
            {
                var gradingRangeTotal = new GradingRangeTotal
                                        {
                                            Frequency = GradingRanges.Sum(x => x.Frequency),
                                            TotalPercentage = GradingRanges.Sum(x => x.TotalPercentage)
                                        };

                return gradingRangeTotal;
            }
        }

        public List<GradeNormalCurve> GradeNormalCurves { get; set; } = new List<GradeNormalCurve>();
        public List<GradingFrequency> GradingFrequencies { get; set; } = new List<GradingFrequency>();
        public List<Allocation> MarkAllocaiton { get; set; } = new List<Allocation>();
        public List<GradingCurve> GradingCurves { get; set; } = new List<GradingCurve>();
        public List<GradingCurve> GradingCurvesNotCalc { get; set; } = new List<GradingCurve>();
        public bool IsCalGradeExist { get; set; }
        public bool IsNotCalGradeExist { get; set; }
        public List<StudentScoreAllocation> StudentScoreAllocations { get; set; } = new List<StudentScoreAllocation>();
        public List<StudentSectionAllocation> SectionAllocations { get; set; } = new List<StudentSectionAllocation>();
        public List<SectionMarkAllocationsViewModel> SectionMarkAllocations { get; set; } = new List<SectionMarkAllocationsViewModel>();
        public List<GradingRange> GradingRangesCalc => GradingRanges.Where(x => x.IsCalculated).ToList();
        public List<GradingRange> GradingRangesNotCalc => GradingRanges.Where(x => !x.IsCalculated).ToList();
        public List<StudentRawScoreViewModel> StudentRawScores { get; set; } = new List<StudentRawScoreViewModel>();
        public decimal TotalAllocationScore => MarkAllocaiton.Sum(x => x.FullScore ?? 0);
        public bool HaveBarcode => BarcodeNumber != "N/A" ? true : false;
        public bool IsGradeMember { get; set; }
    }
    
    public class SectionMarkAllocationsViewModel
    {
        public string InstructorFullNameEns { get; set; }
        public List<Allocation> MarkAllocaiton { get; set; } = new List<Allocation>();
        public List<StudentSectionAllocation> SectionAllocations { get; set; } = new List<StudentSectionAllocation>();
        public List<StudentScoreAllocation> StudentScoreAllocations { get; set; } = new List<StudentScoreAllocation>();
    }

    public class ClassStatistics
    {
        public string GPA { get; set; }
        public decimal Min { get; set; }
        public decimal Mean { get; set; }
        public decimal Median { get; set; }
        public decimal Max { get; set; }
        public decimal SD { get; set; }
    }

    public class GradeNormalCurve
    {
        public decimal CurvePercentage { get; set; }
        public string Grade { get; set; }
    }

    public class GradingRange
    {
        public long? GradeTemplateId { get; set; }
        public decimal? MinRange { get; set; }
        public decimal? MaxRange { get; set; }
        public string Range
        {
            get
            {
                if (MinRange >= 0 && MaxRange > 0)
                {
                    return $"{ MaxRange?.ToString(StringFormat.GeneralDecimal) } - { MinRange?.ToString(StringFormat.GeneralDecimal) }";
                }
                
                return "";
            }
        }
        public string Grade { get; set; }
        public bool IsCalculated { get; set; }
        public decimal Frequency { get; set; }
        public decimal TotalPercentage { get; set; }
        public decimal GradePercentage { get; set; }
        public decimal CumulativePercentage { get; set; }
    }

    public class GradingRangeTotal
    {
        public decimal Frequency { get; set; }
        public decimal TotalPercentage { get; set; }
    }

    public class GradingFrequency
    {
        public decimal Score { get; set; }
        public double Frequency { get; set; }
        public double TotalPercentage { get; set; }
    }

    public class StudentScoreAllocation
    {
        public Guid StudentId { get; set; }
        public string Section { get; set; }
        public long SectionId { get; set; }
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public string StudentCode { get; set; }
        public string StudentTitle { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentMidName { get; set; }
        public string StudentLastName { get; set; }
        public decimal GradeWeight { get; set; }
        public bool IsCalcGrade { get; set; }
        public bool IsGradePublish { get; set; }
        public int CourseCredit { get; set; }
        public long? GradeTemplateId { get; set; }
        public string FullName => string.IsNullOrEmpty(StudentMidName) ? $"{ StudentTitle } { StudentFirstName } { StudentLastName }"
                                                                       : $"{ StudentTitle } { StudentFirstName } { StudentMidName } { StudentLastName }";
        public string Grade { get; set; }
        public long? GradeId { get; set; }
        public decimal Round { get; set; }
        public decimal? TotalScore { get; set; }
        public string Major { get; set; }
        public List<Allocation> Allocations { get; set; } = new List<Allocation>();
        public string AllocationTotal { get; set; }
    }

    public class StudentSectionAllocation
    {
        public string Section { get; set; }
        public long SectionId { get; set; }
        public string Course { get; set; }
        public long CourseId { get; set; }
        public string Credit { get; set; }
        public string SectionType { get; set; }
    }
}