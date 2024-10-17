using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class GradeRecordViewModel
    {
        public long BarcodeId { get; set; }
        public string BarcodeNumber { get; set; }
        public long SectionId { get; set; }
        public string Status { get; set; }
        public int PublishedStudent { get; set; }
        public int SectionStudent { get; set; }
        public Barcode BarcodeInfromation { get; set; }
        public List<GradingRange> LetterGradingRanges { get; set; } = new List<GradingRange>();
        public List<GradingRange> PassFailGradingRanges { get; set; } = new List<GradingRange>();
        public List<StudentGradeRecord> StudentRecords { get; set; }
        public List<GradeRecordAllocation> Allocations { get; set; }
    }

    public class StudentGradeRecord
    {
        public long StudentScoreId { get; set; }
        public long RegistrationCourseId { get; set; }
        public string StudentCode { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string SectionNumber { get; set; }
        public string StudentName { get; set; }
        public string Grade { get; set; }
        public decimal? TotalScore { get; set; }
        public int RoundedScore { get; set; }
        public bool IsPaid { get; set; }
        public bool IsPublished { get; set; }
        public string CourseAndSection => $"{ CourseCode }({ SectionNumber })";
        public List<Allocation> Scores { get; set; }
    }

    public class GradeRecordAllocation 
    {
        public string Type { get; set; }
        public string Abbreviation { get; set; }
        public decimal FullScore { get; set; }
    }
}