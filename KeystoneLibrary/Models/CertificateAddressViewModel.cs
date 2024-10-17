namespace KeystoneLibrary.Models
{
    public class CertificateAddressViewModel
    {
        public Criteria Criteria { get; set; }
        public List<CertificateAddress> Results { get; set; }
    }

    public class CertificateAddress
    {
        public string Certificate { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public string CreatedAt { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string TelephoneNumber { get; set; }
    }
}