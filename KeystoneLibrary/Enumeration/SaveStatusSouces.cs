using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Enumeration
{
    public enum SaveStatusSouces
    {
        [Display(Name = "Admission")]
        ADMISSION,

        [Display(Name = "Blacklist")]
        BLACKLIST,

        [Display(Name = "Change Student Status")]
        CHANGESTUDENTSTATUS,

        [Display(Name = "Dismiss Student")]
        DISMISSSTUDENT,

        [Display(Name = "General")]
        GENERAL,

        [Display(Name = "Graduation")]
        GRADUATION,

        [Display(Name = "Maintain")]
        MAINTAIN,

        [Display(Name = "Re-Admission")]
        READMISSION,

        [Display(Name = "Re-Enter")]
        REENTER,

        [Display(Name = "Student API")]
        STUDENTAPI
    }
}