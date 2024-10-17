using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Scholarship;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models
{
    public class ResignStudentViewModel : StudentInformationViewModel
    {
        public Guid StudentId { get; set; }
        public long TermId { get; set; }
        public long ResignReasonId { get; set; }
        public string ResignReason { get; set; }
        public int CreditEarned { get; set; }
        public string AcademicLevel { get; set; }
        public string Curriculum { get; set; }
        public string CurriculumVersion { get; set; }
        public string AdmissionTerm { get; set; }
        public string Remark { get; set; }
        public string EffectiveTerm { get; set; }
        public string ApprovedAtText { get; set; }
        public long EffectiveTermId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime ApprovedAt { get; set; } = DateTime.Now;
        public List<ScholarshipStudent> ScholarshipStudents { get; set; }
        public List<RegistrationCourse> RegistrationCourses { get; set; }
    }
}