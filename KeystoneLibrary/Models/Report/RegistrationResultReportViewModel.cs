namespace KeystoneLibrary.Models.Report
{
    public class RegistrationResultReportViewModel
    {
        public Criteria Criteria { get; set; }
        public Guid StudentId { get; set; }
        public bool IsFinishedRegistration { get; set; }
        public List<RegistrationResultReport> RegistrationResultReports { get; set; }
        public int Total
        {
            get
            {
                return RegistrationResultReports?.Count() ?? 0;
            }
        }
    }

    public class RegistrationResultReport
    {
        public string StudentCode { get; set; }
        public string StudentTitle { get; set; }
        public string StudentName { get; set; }
        public string StudentStatus { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string AdvisorTitle { get; set; }
        public string AdvisorName { get; set; }
        public List<RegisteredCourse> RegisteredCourses { get; set; }
        public string PaidCourses
        {
            get
            {
                if (RegisteredCourses != null)
                {
                    var paidCourses = string.Join(", ", RegisteredCourses.Where(x => x.IsPaid).Select(x => x.CourseAndSection));
                    return paidCourses;
                }
                return "";
            }
        }
        public string PaidCoursesRegistrationCredit
        {
            get
            {
                if (RegisteredCourses != null)
                {
                    return RegisteredCourses.Where(x => x.IsPaid).Sum(x => x.RegistrationCredit) + "" ;
                }
                return "";
            }
        }
        public string UnPaidCourses
        {
            get
            {
                if (RegisteredCourses != null)
                {
                    var unPaidCourses = string.Join(", ", RegisteredCourses.Where(x => !x.IsPaid).Select(x => x.CourseAndSection));
                    return unPaidCourses;
                }
                return "";
            }
        }
        public string UnPaidCoursesRegistrationCredit
        {
            get
            {
                if (RegisteredCourses != null)
                {
                    return RegisteredCourses.Where(x => !x.IsPaid).Sum(x => x.RegistrationCredit) + "";
                }
                return "";
            }
        }

        public int TotalCredit
        {
            get
            {
                if (RegisteredCourses != null)
                {
                    return RegisteredCourses.Sum(x => x.RegistrationCredit);
                }
                return 0;
            }
        }

        public int TotalAcademicCredit
        {
            get
            {
                if (RegisteredCourses != null)
                {
                    return RegisteredCourses.Sum(x => x.AcademicCredit);
                }
                return 0;
            }
        }
    }

    public class RegisteredCourse
    {
        public string CourseAndSection { get; set; }
        public bool IsPaid { get; set; }

        public int RegistrationCredit { get; set;}
        public int AcademicCredit { get; set; }
    }
}