namespace KeystoneLibrary.Models
{
    public class UpdateCreditViewModel
    {
        public Criteria Criteria { get; set; } = new Criteria();
        public int MinimumCredit { get; set; }
        public int MaximumCredit { get; set; }
        public List<StudentUpdateCredit> StudentUpdateCredits { get; set; }
    }

    public class StudentUpdateCredit
    {
        public string IsChecked { get; set; }
        public long AcademicInfomationId { get; set; }
        public string StudentCode { get; set; }
        public string TitleNameEn { get; set; }
        public string FirstNameEn { get; set; }
        public string MidNameEn { get; set; }
        public string LastNameEn { get; set; }
        public string Major { get; set; }
        public string CurriculumVersion { get; set; }
        public int? MinimumCredit { get; set; }
        public int? MaximumCredit  { get; set; }
        public string FullNameEn => string.IsNullOrEmpty(MidNameEn) ? $"{ TitleNameEn } { FirstNameEn } { LastNameEn }"
                                                                    : $"{ TitleNameEn } { FirstNameEn } { MidNameEn } { LastNameEn }";

    }
}