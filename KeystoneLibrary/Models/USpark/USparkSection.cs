namespace KeystoneLibrary.Models.USpark
{
    public class USparkSection
    {
       public long Id { get; set; }

       public string SectionNumber { get; set; }

       public string CourseCode { get; set; }

       public string CourseName { get; set; }

       public string ParentSection { get; set; }

       public int SeatLimit { get; set; }

       public int SeatAvailable { get; set; }

       public int SeatUsed { get; set; }

       public int PlanningSeat { get; set; }

       public int ExtraSeat { get; set; }

       public DateTime OpenedSectionDate { get; set; }

       public DateTime ClosedSectionDate { get; set; }

       public string Status { get; set; }

       public DateTime ApprovedDate { get; set; }

       public string ApprovedBy { get; set; }

       public bool IsClosed { get; set; }

       public string Remark { get; set; }
    }
}