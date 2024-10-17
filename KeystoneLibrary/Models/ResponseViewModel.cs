namespace KeystoneLibrary.Models
{
    public class ResponseViewModel
    {
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public int QuestionGroupCount { get; set; }
        public int QuestionCount { get; set; }
        public decimal Response { get; set; }
        public decimal StudentCount { get; set; }
        public decimal Percentage => StudentCount == 0 ? 0 : (Response / StudentCount) * 100;
        public string PercentageText => Percentage.ToString(StringFormat.TwoDecimal);
    }
}