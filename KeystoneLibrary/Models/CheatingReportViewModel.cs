namespace KeystoneLibrary.Models
{
    public class CheatingReportViewModel
    {
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public List<CheatingStatusDetail> CheatingStatusDetails { get; set; }
    }

    public class CheatingStatusDetail
    {
        public string Term { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int SectionNumber { get; set; }
        public string ExaminationType { get; set; }
        public string Incident { get; set; }
        public string FromTerm { get; set; }
        public string ToTerm { get; set; }
        public bool PaidStatus { get; set; }
        public string TermRange 
        {
            get
            {
                string start = string.IsNullOrEmpty(FromTerm) ? "xxx" : FromTerm;
                string end = string.IsNullOrEmpty(ToTerm) ? "xxx" : ToTerm;
                return string.IsNullOrEmpty(FromTerm) && string.IsNullOrEmpty(ToTerm) ? "" : $"{ start } - { end }";
            }
            
        }
    }
}