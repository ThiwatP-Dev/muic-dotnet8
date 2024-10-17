using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.Enums
{
    public enum FinancialTypeEnum
    {
        [Display(Name = "Registration")]
        r,
        [Display(Name = "Add")]
        a,

        [Display(Name = "Delete")]
        d,

        [Display(Name = "Refund")]
        rf
    }
}