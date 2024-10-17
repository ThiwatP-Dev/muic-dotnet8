namespace KeystoneLibrary.Models.USpark
{
    internal class USparkStudent
    {
    }

    public class USparkStudentPermissionResponse
    {
        public string StudentCode { get; set; }

        public bool IsAllowRegistration { get; set; }
        public string IsAllowRegistrationRemark { get; set; }
        public bool IsAllowPaymentVisible { get; set; }
        public string IsAllowPaymentVisibleRemark { get; set; }
        public bool IsAllowPayment { get; set; }
        public string IsAllowPaymentRemark { get; set; }
        public bool IsAllowSignIn { get; set; }
        public string IsAllowSignInRemark { get; set; }
    }
}
