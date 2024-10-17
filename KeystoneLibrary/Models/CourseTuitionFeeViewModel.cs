using KeystoneLibrary.Models.DataModels.Fee;

namespace KeystoneLibrary.Models
{
    public class CourseTuitionFeeViewModel
    {
        public long AcademicLevelId { get; set; }
        public string AcademicLevel { get; set; }
        public long CourseId { get; set; }
        public string Course { get; set; }
        public string Division { get; set; }
        public string Major { get; set; }
        public string ReturnUrl { get; set; }
        public TuitionFee TuitionFee { get; set; }
        public List<TuitionFee> TuitionFees { get; set; }

        public CourseTuitionFeeViewModel()
        {
            TuitionFee = new TuitionFee();
            TuitionFees = new List<TuitionFee>();
        }
    }

}