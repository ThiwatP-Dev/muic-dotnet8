namespace KeystoneLibrary.Models.Report
{
    public class PublishedGradeReportViewModel
    {
        public string BarcodeNumber { get; set; }
        public string Course { get; set; }
        public string Sections { get; set; }
        public string SectionNumber { get; set; }
        public string SectionType { get; set; }
        public List<long> SectionListId { get; set; }
        public string SectionIds { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string PublishedBy { get; set; }
        public string GeneratedAtText => GeneratedAt.AddHours(7).ToString(StringFormat.ShortDate);
        public string PublishedAtText => PublishedAt?.AddHours(7).ToString(StringFormat.ShortDate);
        public string ApprovedAtText => ApprovedAt?.AddHours(7).ToString(StringFormat.ShortDate);
        public bool IsPublished { get; set; }
    }
}