using static KeystoneLibrary.Models.Report.RegistrationResultWithAmountAndCreditReportItemViewModel;

namespace KeystoneLibrary.Models.Report
{
    public class RegistrationResultWithAmountAndCreditReportViewModel
    {
        public Criteria Criteria { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal OtherTotalAmount { get; set; }
        public int StudentCount { get; set; }
        public List<Item> Fees { get; set; } = new List<Item>();
        public List<KeyValuePair<string, List<long>>> FeeColumns { get; set; } = new List<KeyValuePair<string, List<long>>>()
        {
            //From FeeItem
            new KeyValuePair<string, List<long>> ("Tuition" , new List<long>{1}),
            new KeyValuePair<string, List<long>> ("Education Fee" , new List<long>{13}),
            new KeyValuePair<string, List<long>> ("Insurance Fee" , new List<long>{15}),
            new KeyValuePair<string, List<long>> ("Trimester Lump Sum Tution Fees" , new List<long>{7,9,10,11,33}),
            new KeyValuePair<string, List<long>> ("Add/Drop Fee" , new List<long>{21}),
            new KeyValuePair<string, List<long>> ("Late Registration Fee" , new List<long>{24}),
            new KeyValuePair<string, List<long>> ("Late Payment Fee" , new List<long>{17}),
            //new KeyValuePair<string, List<long>> ("Scholarship" , new List<long>{ ReceiptProvider.SPECIAL_FEE_TYPE_ID_SCHOLARSHIP})
        };
        public List<RegistrationResultWithAmountAndCreditReportBatch> Batches { get; set; }
        public List<RegistrationResultWithAmountAndCreditReportItemViewModel> ReportItems { get; set; }
        public List<RegistrationResultWithAmountAndCreditReportBatchDetail> Faculties { get; set; }
    }

    public class RegistrationResultWithAmountAndCreditReportBatch
    {
        public int StudentBatch { get; set; }
        public int StudentCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal OtherTotalAmount { get; set; }
        public List<Item> Fees { get; set; }
        public List<RegistrationResultWithAmountAndCreditReportBatchDetail> Faculties { get; set; }
        public List<RegistrationResultWithAmountAndCreditReportItemViewModel> ReportItems { get; set; }

    }
    public class RegistrationResultWithAmountAndCreditReportBatchDetail
    {
        public string Faculty { get; set; }
        public int StudentCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal OtherTotalAmount { get; set; }
        public List<Item> Fees { get; set; }
        public List<RegistrationResultWithAmountAndCreditReportItemViewModel> ReportItems { get; set; }
    }

    public class RegistrationResultWithAmountAndCreditReportItemViewModel
    {
        public Guid StudentId { get; set; }
        public int StudentBatch { get; set; }
        public string StudentCode { get; set; }
        public string StudentCitizenNumber { get; set; }
        public string StudentTitleEn { get; set; }
        public string StudentFirstNameEn { get; set; }
        public string StudentLastNameEn { get; set; }
        public string StudentTitleTh { get; set; }
        public string StudentFirstNameTh { get; set; }
        public string StudentLastNameTh { get; set; }
        public string StudentNationality { get; set; }
        public string StudentFeeType { get; set; }
        public string StudentTelephoneNumber { get; set; }
        public string AdvisorTitle { get; set; }
        public string AdvisorFirstName { get; set; }
        public string AdvisorLastName { get; set; }
        public string StudentStatus { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public long InvoiceId { get; set; }
        public string Term { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceType { get; set; }
        public string InvoicePaymentMethod { get; set; }
        public DateTime? InvoiceDateTime { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime? ReceiptDateTime { get; set; }
        public bool IsPaid { get; set; }
        public bool ConfirmStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalRelatedCredit { get; set; }
        public int TotalRelatedCourse { get; set; }
        public DateTime? FirstRegistrationDate { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public decimal OtherTotalAmount { get; set; }
        public int StudentCount { get; set; }
        public List<Item> Items { get; set; }
        public string InvoiceTypeText => string.IsNullOrEmpty(InvoiceType)? DataModels.Invoice.TYPE_UNCONFIRM : TotalAmount >= 0 ? InvoiceType : DataModels.Invoice.REFUND_TYPE_REFUND;
        public string FirstRegistrationDateText => FirstRegistrationDate?.ToString(StringFormat.ShortDate);

        public string LastPaymentDateText => LastPaymentDate?.ToString(StringFormat.ShortDate);

        public string InvoiceDateText => InvoiceDateTime?.ToString(StringFormat.ShortDate);

        public string ReceiptDateText => ReceiptDateTime?.ToString(StringFormat.ShortDate);

        public class Item
        {
            public long FeeItemId { get; set; }
            public string FeeName { get; set; }
            public decimal Amount { get; set; }
            public string AmountText { get; set; }
            public decimal DiscountAmount { get; set; }
            public decimal TotalAmount { get; set; }
            public List<long> FeeValues { get; set; }
        }
    }
}
