namespace KeystoneLibrary.Config
{
    public class PaymentConfiguration
	{
		public DateTime StartRegistrationPaymentDate { get; set; }

		public DateTime LastRegistartionDate { get; set; }

		public DateTime LastPaymentDate { get; set; }

		public DateTime RegistrationLatePaymentDate { get; set; }

		public DateTime AddDropLatePaymentDate { get; set; }

		public string UpdateUSPaidRegistrationCourseEndpoint { get; set; }
	}
}
