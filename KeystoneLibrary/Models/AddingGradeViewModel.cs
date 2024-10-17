using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models
{
    public class AddingGradeViewModel
    {
        public Criteria Criteria { get; set; } = new Criteria();
        public Guid StudentId { get; set; }
        public Student Student { get; set; }
        public string CurriculumName { get; set; }
        public List<RegistrationCourse> RegistrationCourses { get; set; }
        public List<RegistrationCourse> AddingCourses { get; set; }
        
        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime ApproveDate { get; set; } = DateTime.Now;
        public long ApprovedById { get; set; }
    }
}