using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class TransferViewModel
    {
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseNameEn { get; set; }
        public string CourseNameTh { get; set; }
        public long PreviousSectionId { get; set; }
        public string SectionNumber { get; set; }
        public long TermId { get; set; }
        public long AcademicLevelId { get; set; }
        public string PreviousSectionTime { get; set; }
        public long NewSectionId { get; set; }
        public List<RegistrationCourse> Students { get; set; }
    }
}