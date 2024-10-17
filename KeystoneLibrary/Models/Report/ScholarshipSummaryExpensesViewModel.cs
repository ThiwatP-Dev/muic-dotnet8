using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models
{
    public class ScholarshipSummaryExpensesViewModel
    {
        public Criteria Criteria { get; set; }
        public Student Student { get; set; }
        public List<ScholarshipSummaryExpenses> Results { get; set; }
    }

    public class ScholarshipSummaryExpenses
    {
        public string TermText { get; set; }
        public string FeeGroupName { get; set; }
        public string FeeItemName { get; set; }
        public decimal Amount { get; set; }
    }
}