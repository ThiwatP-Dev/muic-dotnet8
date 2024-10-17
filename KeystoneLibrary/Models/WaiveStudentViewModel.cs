using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class WaiveStudentViewModel
    {
        public Criteria Criteria { get; set; }
        public decimal WaiveAmount { get; set; }
        public List<InvoiceItem> Results { get; set; }
    }
}