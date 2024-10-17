using KeystoneLibrary.Enumeration;

namespace KeystoneLibrary.Models
{
    public class StudentStatusSummaryViewModel
    {
        public Criteria Criteria { get; set; }
        public List<StudentStatusSummaryResult> Results { get; set; } = new List<StudentStatusSummaryResult>();
        public List<StudentStatus> Stautses 
        {
            get 
            {
                if (Criteria == null 
                    || Criteria.StudentStatuses == null 
                    || !Criteria.StudentStatuses.Any())
                {
                    return Enum.GetValues(typeof(StudentStatus))
                               .Cast<StudentStatus>()
                               .ToList();
                }
                else
                {
                    return Enum.GetValues(typeof(StudentStatus))
                               .Cast<StudentStatus>()
                               .Where(x => Criteria.StudentStatuses.Select(y => y.ToUpper())
                                                                   .Contains(x.ToString()))
                               .ToList();
                }
            }
        }

        public List<string> Headers => Stautses.Select(x => x.GetDisplayName()).ToList();
        public List<string> Columns => Stautses.Select(x => x.ToString()).ToList();
    }

    public class StudentStatusSummaryDetail
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string FullName { get; set; }
        public string FacultyName { get; set; }
        public string DepartmentName { get; set; }
        public string PersonalEmail { get; set; }
        public string StudentStatusText { get; set; }
        public string Term { get; set; }
        public DateTime? Date { get; set; }
        public string DateText => Date?.ToString(StringFormat.ShortDate);
        public string AdvisorName { get; set; }
    }

    public class StudentStatusSummaryResult
    {
        public int Batch { get; set; }
        public long? DepartmentId { get; set; }
        public string DepartmentAbbreviation { get; set; }
        public long A { get; set; }
        public long S { get; set; }
        public long D { get; set; }
        public long B { get; set; }
        public long RS { get; set; }
        public long DM { get; set; }
        public long PRC { get; set; }
        public long PA { get; set; }
        public long G { get; set; }
        public long G1 { get; set; }
        public long G2 { get; set; }
        public long EX { get; set; }
        public long TR { get; set; }
        public long LA { get; set; }
        public long NP { get; set; }
        public long RE { get; set; }
        public long RA { get; set; }
    }
}