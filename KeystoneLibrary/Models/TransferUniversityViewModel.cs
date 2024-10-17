namespace KeystoneLibrary.Models
{
    public class TransferUniversityViewModel
    {
        public long Id { get; set; }
        public string OHECCode { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public long CountryId { get; set; }
        public long AcademicLevelId { get; set; }
        public bool IsActive { get; set; }
        public List<TransferUniversityCourse> Courses { get; set; }
    }

    public class TransferUniversityCourse
    {
        public long AcademicLevelId { get; set; }
        public long? TransferUniversityId { get; set; }
        public long CourseId { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public string ShortNameEn { get; set; }
        public string ShortNameTh { get; set; }
        public string TranscriptNameEn1 { get; set; }
        public string TranscriptNameTh1 { get; set; }
        public string TranscriptNameEn2 { get; set; }
        public string TranscriptNameTh2 { get; set; }
        public string TranscriptNameEn3 { get; set; }
        public string TranscriptNameTh3 { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionTh { get; set; }
        public int? Credit { get; set; }
        public decimal? Lecture { get; set; }
        public decimal? Lab { get; set; }
        public decimal? Other { get; set; }
        public bool IsShowInTranscript { get; set; }
        public bool IsCalculateCredit { get; set; }
    }
}