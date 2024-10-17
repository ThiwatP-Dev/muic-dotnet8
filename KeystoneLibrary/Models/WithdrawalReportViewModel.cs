namespace KeystoneLibrary.Models
{
    public class WithdrawalReportViewModel
    {
        public Criteria Criteria { get; set; }
        public string Term { get; set; }
        public List<WithdrawalReportByCourse> WithdrawalReportByCourses { get; set; }
        public List<WithdrawalReportByStudent> WithdrawalReportByStudents { get; set; }
    }

    public class WithdrawalReportByCourse
    {
        public string CourseCode { get; set; }
        public string CourseSpecialChar { get; set; }
        public string CourseName { get; set; }
        public string CourseCodeAndName => $"{ CourseCode }{ CourseSpecialChar} { CourseName }";
        public string InstructorTitle { get; set; }
        public string InstructorFirstName { get; set; }
        public string InstructorLastName { get; set; }
        public string InstructorFullName => $"{ InstructorTitle } { InstructorFirstName } { InstructorLastName }";
        public List<Item> Withdrawals { get; set; }

        public class Item
        {
            public string Type { get; set; }
            public string StudentTitle { get; set; }
            public string StudentCode { get; set; }
            public string StudentFirstName { get; set; }
            public string StudentMidName { get; set; }
            public string StudentLastName { get; set; }
            public string SectionNumber { get; set; }
            public string DepartmentName { get; set; }
            public string StudentFullName => string.IsNullOrEmpty(StudentMidName) ? $"{ StudentTitle } { StudentFirstName } { StudentLastName }"
                                                                                  : $"{ StudentTitle } { StudentFirstName } { StudentMidName } { StudentLastName }";
            public string InstructorCode { get; set; }
            public string InstructorTitle { get; set; }
            public string InstructorFirstName { get; set; }
            public string InstructorLastName { get; set; }
            public string InstructorFullName => $"{ InstructorTitle } { InstructorFirstName } { InstructorLastName }";
            public string InstructorCodeAndName
            {
                get
                {
                    var lastname = string.IsNullOrEmpty(InstructorLastName) ? "" : $"{ InstructorLastName.Substring(0, 1) }.";
                    return $"{ InstructorCode } { InstructorFirstName } { lastname }";
                }
            }
            public string Remark { get; set; }
        }
    }

    public class WithdrawalReportByStudent
    {
        public string StudentCode { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public string StudentMidName { get; set; }
        public string StudentTitle { get; set; }
        public string DepartmentName { get; set; }
        public string StudentCodeAndName => string.IsNullOrEmpty(StudentMidName) ? $"{ StudentCode } { StudentTitle } { StudentFirstName } { StudentLastName }"
                                                                                 : $"{ StudentCode } { StudentTitle } { StudentFirstName } { StudentMidName } { StudentLastName }";
        public string StudentCodeAndNameShort
        {
            get
            {
                var lastname = string.IsNullOrEmpty(StudentLastName) ? "" : $"{ StudentLastName.Substring(0, 1) }.";
                return $"{ StudentCode } { StudentFirstName } { lastname }";
            }
        }

        public List<Item> Withdrawals { get; set; }

        public class Item
        {
            public string Type { get; set; }
            public string CourseCode { get; set; }
            public string CourseName { get; set; }
            public string SectionNumber { get; set; }
            public string InstructorCode { get; set; }
            public string InstructorTitle { get; set; }
            public string InstructorFirstName { get; set; }
            public string InstructorLastName { get; set; }
            public string InstructorFullName => $"{ InstructorTitle } { InstructorFirstName } { InstructorLastName }";
            public string Remark { get; set; }
        }
    }
}