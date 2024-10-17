namespace KeystoneLibrary.Models.Report
{
    public class StudentFeeGroupReportViewModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Remark { get; set; }
        public string AcademicLevel { get; set; }
        public string FeeItem { get; set; }
        public string StartedTerm { get; set; }
        public string EndedTerm { get; set; }
        public int StartedTermAcademicYear { get; set; }
        public int StartedTermAcademicTerm { get; set; }
        public int EndedTermAcademicYear { get; set; }
        public int EndedTermAcademicTerm { get; set; }
        public int? StartedBatch { get; set; }
        public int? EndedBatch { get; set; }
        public bool? IsThai { get; set; }
        public bool IsLumpsumPayment { get; set; }
        public bool IsOneTime { get; set; }
        public bool IsPerYear { get; set; }
        public bool IsPerTerm { get; set; }
        public string TermType { get; set; }
        public int? Term { get; set; }
        public decimal Amount { get; set; }
        public string StudentFeeType { get; set; }
        public string BatchRange
        {
            get
            {
                string start = StartedBatch == 0 || StartedBatch == null ? "xxx" : $"{ StartedBatch }";
                string end = EndedBatch == 0 || EndedBatch == null ? "xxx" : $"{ EndedBatch }";
                return StartedBatch == 0 && EndedBatch == 0 ? "" : $"{ start } - { end }";
            }
        }
        public string AmountText => Amount.ToString(StringFormat.Money);
    }
}