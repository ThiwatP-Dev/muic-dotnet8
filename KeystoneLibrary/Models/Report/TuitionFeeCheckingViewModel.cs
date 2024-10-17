using static KeystoneLibrary.Models.Report.TuitionFeeCheckingReportJobConfiguration;

namespace KeystoneLibrary.Models.Report
{
    public class TuitionFeeCheckingViewModel
    {
        public Criteria Criteria { get; set; }
        public List<TuitionFeeCheckingBatch> Headers { get; set; }
        public List<TuitionFeeChecking> Results { get; set; }
        public List<TuitionFeeCheckingReportJob> TuitionFeeCheckingReportJobs { get; set; }
    }

    public class TuitionFeeChecking
    {
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public DateTime? CreatedAtUtc { get; set; }
        public string CourseName{ get; set; }
        public string CourseCredit { get; set; }
        public int Sequence { get; set; }
        public decimal Value { get; set; }
        public string ValueText => Value.ToString(StringFormat.Money);
        public string CreatedAtTHText => CreatedAtUtc?.AddHours(7).ToString(StringFormat.ShortDateTime) ?? "";
    }

    public class TuitionFeeCheckingBatch
    {
        public int Sequence { get; set; }
        public int StartedBatch { get; set; }
        public int? EndedBatch { get; set; }
        public long StudentFeeTypeId { get; set; }
        public string StudentFeeTypeNameEn { get; set; }
        public long CustomCourseGroupId { get; set; }
        public string CustomCourseGroupName { get; set; }
        public string BatchText => $"{ StartedBatch } { (EndedBatch == null ? string.Empty : $"- { EndedBatch }") }";
    }

    public class TuitionFeeCheckingReportJobConfiguration
    {
        public List<TuitionFeeCheckingReportJob> TuitionFeeCheckingReportJobs { get; set; } = new List<TuitionFeeCheckingReportJob> { };

        public class TuitionFeeCheckingReportJob
        {
            public long AcademicLevelId { get; set; }
            public DateTime CreatedAtUTC { get; set; }
            public DateTime? EndTimeUTC { get; set; }
            public TimeSpan ElapseTime { get; set; }
            public string ResultFilePath { get; set; }
            public string ResultFileUrl { get; set; }
            public string CreatedBy { get; set; }
            public string Remark { get; set; }
        }
    }
}