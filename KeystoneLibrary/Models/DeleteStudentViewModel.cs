namespace KeystoneLibrary.Models
{
    public class DeleteStudentViewModel
    {
        public List<SearchDeleteStudentViewModel> SearchDeleteStudents { get; set; }
        public Criteria Criteria { get; set; }
    }

    public class SearchDeleteStudentViewModel
    {
        public Guid StudentId { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string MidName { get; set; }
        public string LastName { get; set; }
        public string AcademicLevel { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public long AcademicLevelId { get; set; }
        public long FacultyId { get; set; }
        public long DepartmentId { get; set; }
        public string StudentStatus { get; set; }
        public string IsChecked { get; set; }
        public string FullName => string.IsNullOrEmpty(MidName) ? $"{ Title } { FirstName } { LastName }"
                                                                : $"{ Title } { FirstName } { MidName } { LastName }";
        public string StudentStatusText
        {
            get
            {
                switch (StudentStatus)
                {
                    case "a":
                        return "Admission";
                    case "s":
                        return "Studying";
                    case "d":
                        return "Deleted";
                    case "b":
                        return "Blacklist";
                    case "rs":
                        return "Resigned";
                    case "dm":
                        return "Dismiss";
                    case "prc":
                        return "Passed all required course";
                    case "pa":
                        return "Passed away";
                    case "g":
                        return "Graduated";
                    case "g1":
                        return "Graduated with first class honor";
                    case "g2":
                        return "Graduated with second class honor";
                    case "ex":
                        return "Exchange";
                    case "tr":
                        return "Transferred to other university";
                    case "la":
                        return "Leave of absence";
                    case "np":
                        return "No Report";
                    case "re":
                        return "Reenter";
                    case "ra":
                        return "Re-admission";
                    default:
                        return "N/A";
                }
            }
        }
    }
}