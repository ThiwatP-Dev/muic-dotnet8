using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.Enums
{
    public enum AdmissionStatusEnum
    {
        [Display(Name = "Request")]
        Request,
        [Display(Name = "Waiting Document or Payment")]
        WaitingDocument,
        [Display(Name = "Payment Success")]
        PaymentSuccess,
        [Display(Name = "Completed")]
        Completed
    }
}
