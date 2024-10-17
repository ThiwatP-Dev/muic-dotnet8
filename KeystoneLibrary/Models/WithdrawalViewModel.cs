using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Withdrawals;

namespace KeystoneLibrary.Models
{
    public class WithdrawalViewModel
    {
        public long CourseId { get; set; }
        public long SectionId { get; set; }
        public string StudentCode { get; set; }
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public string Remark { get; set; }
        public Criteria Criteria { get; set; }
        public string ReturnUrl { get; set; }
        public WithdrawByCourseViewModel  WithdrawByCourse { get; set; }
        public WithdrawByStudentViewModel WithdrawByStudent { get; set; }
    }

    public class WithdrawByCourseViewModel
    {
        public string Type { get; set; }
        public long? ApprovedBy { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public long CourseId { get; set; }
        public long SectionId { get; set; }
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public string Remark { get; set; }
        public List<RegistrationCourse> RegistrationCourses { get; set; }
        public List<Withdrawal> StudentWithdrawals { get; set; }
    }

    public class WithdrawByStudentViewModel
    {
        public string Type { get; set; }
        public long? ApprovedBy { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public string StudentCode { get; set; }
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public List<long> CourseIds { get; set; }
        public List<RegistrationCourse> RegistrationCourses { get; set; }

        public string Remark { get; set; }
    }

    public class WithdrawalSearchResultViewModel
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Course { get; set; }
        public string Credit { get; set; }
        public string Section { get; set; }
        public string Instructor { get; set; }
        public string Type { get; set; }
        public string TypeText { get; set; }
        public string Major { get; set; }
        public string Status { get; set; }
        public string StatusText { get; set; }
        public string ApprovedDate { get; set; }
        public string Approver { get; set; }
        public string RequestedDate { get; set; }
        public string Remark { get; set; }

        [NotMapped]
        public string RequestedBy
        {
            get
            {
                switch (Type)
                {
                    case "u":
                        return Name;
                    case "d":
                    case "p":
                        return Instructor;
                    default:
                        return "";
                }
            }
        }
    }
}