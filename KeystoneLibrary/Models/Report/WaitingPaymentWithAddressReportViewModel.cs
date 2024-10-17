namespace KeystoneLibrary.Models.Report
{
    public class WaitingPaymentWithAddressReportViewModel
    {
        public Criteria Criteria { get; set; }

        public List<WaitingPaymentWithAddressReportItemViewModel> ReportItems { get; set; }
    }

    public class WaitingPaymentWithAddressReportItemViewModel
    {
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public string StudentStatus { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public long InvoiceId { get; set; }
        public string Term { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceType { get; set; }
        public bool PaidStatus { get; set; }
        public bool ConfirmStatus { get; set; }
        public decimal InvoiceTotalAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalRelatedCredit { get; set; }

        public string AddressEn => $"{ AddressEn1 } { (string.IsNullOrEmpty(AddressEn2) ? "" : AddressEn2) } { HouseNumber } { (string.IsNullOrEmpty(SoiEn) ? "" : "Soi " + SoiEn) } { RoadEn }";
        public string AddressEn1 { get; set; }
        public string AddressEn2 { get; set; }
        public string HouseNumber { get; set; }
        public string SoiEn { get; set; }
        public string RoadEn { get; set; }
        public string CountryEn { get; set; }
        public string ProvinceEn { get; set; }
        public string DistrictEn { get; set; }
        public string SubdistrictEn { get; set; }
        public string CityEn { get; set; }
        public string StateEn { get; set; }
        public string ZipCode { get; set; }
        public string TelephoneNumber { get; set; }
        public string Email { get; set; }
        public string Email2 { get; set; }
        public string PersonalEmail { get; set; }
        public string PersonalEmail2 { get; set; }
        public string TelephoneNumber1 { get; set; }
        public string TelephoneNumber2 { get; set; }
        public string TelephoneNumber3 { get; set; }
        public string Facebook { get; set; }
        public string Line { get; set; }
        public string OtherContact { get; set; }

    }
}
