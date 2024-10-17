using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Fee;

namespace KeystoneLibrary.Models
{
    public class TermFeeViewModel
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public FeeItem FeeItem { get; set; }
        public int Term { get; set; }
        public string AcademicLevelName { get; set; }
        public string TermTypeName { get; set; }
        public bool IsOneTime { get; set; }
        public bool IsActive { get; set; }
        public string Amount { get; set; }
        public string StartedBatch { get; set; }
        public string EndedBatch { get; set; }
        public string BatchRange
        {
            get
            {
                string start = String.IsNullOrEmpty(StartedBatch) ? "xxx" : StartedBatch;
                string end = String.IsNullOrEmpty(EndedBatch) ? "xxx" : EndedBatch;
                return String.IsNullOrEmpty(StartedBatch) && String.IsNullOrEmpty(EndedBatch) ? "N/A" : $"{ start } - { end }";
            }
        }
        public Term StartedTerm { get; set; }
        public Term EndedTerm { get; set; }
        public string TermPeriod
        {
            get
            {
                if (StartedTerm == null && EndedTerm == null)
                {
                    return "N/A";
                }
                else
                {
                    string start = StartedTerm?.TermText ?? "xxx";
                    string end = EndedTerm?.TermText ?? "xxx";
                    return $"{ start } - { end }";
                }
            }
        }
    }

    public class TermFeeModalViewModel
    {
        public string FeeItemName { get; set; }
        public int StartedBatch { get; set; }
        public int EndedBatch { get; set; }
        public string StartedTerm { get; set; }
        public string EndedTerm { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string Curriculum { get; set; }
        public string CurriculumVersion { get; set; }
        public string Nationality { get; set; }
        public bool? IsThai { get; set; }
        public string ThaiStatus
        {
            get
            {
                if (IsThai == null)
                {
                    return "All";
                }
                else if (IsThai.HasValue && IsThai.Value)
                {
                    return "Thai";
                }
                else
                {
                    return "Non-Thai";
                }
            }
        }
    }
}