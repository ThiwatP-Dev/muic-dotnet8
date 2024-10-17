namespace KeystoneLibrary.Models
{
    public class TermFeeSimulateViewModel
    {
        public Criteria Criteria { get; set; }
        public string StudentFeeGroupName { get; set; }
        public string StudentCode { get; set; }
        public string StudentFullName { get; set; }
        public string TermText { get; set; }
        public List<TermFeeSimulateItemViewModel> Results { get; set; }
    }

    public class TermFeeSimulateItemViewModel
    {
        public string TermText { get; set; }
        public string FeeItemName { get; set; }
        public decimal Amount { get; set; }
        public string AmountText => Amount.ToString(StringFormat.Money);
    }
}