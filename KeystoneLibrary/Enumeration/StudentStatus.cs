using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Enumeration
{
    public enum StudentStatus
    {
        [Display(Name = "Admission")]
        A,
        
        [Display(Name = "Studying")]
        S,

        [Display(Name = "Deleted")]
        D,

        [Display(Name = "Blacklist")]
        B,

        [Display(Name = "Resigned")]
        RS,

        [Display(Name = "Dismiss")]
        DM,

        [Display(Name = "Passed all required course")]
        PRC,

        [Display(Name = "Passed away")]
        PA,

        [Display(Name = "Graduated")]
        G,

        [Display(Name = "Graduated with first class honor")]
        G1,

        [Display(Name = "Graduated with second class honor")]
        G2,

        [Display(Name = "Exchange")]
        EX,

        [Display(Name = "Transferred to other university")]
        TR,

        [Display(Name = "Leave of absence")]
        LA,

        [Display(Name = "No Report")]
        NP,

        [Display(Name = "Reenter")]
        RE,

        [Display(Name = "Re-admission")]
        RA
    }
}