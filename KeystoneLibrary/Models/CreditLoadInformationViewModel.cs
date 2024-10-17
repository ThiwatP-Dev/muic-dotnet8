using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models
{
    public class CreditLoadInformationViewModel
    {
        public Guid StudentId { get; set; }
        public int PreviousMinimumCredit { get; set; }
        public int PreviousMaximumCredit { get; set; } 
        public int MinimumCredit { get; set; }
        public int MaximumCredit { get; set; }

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime ApprovedAt { get; set; }
        public long ApprovedBy { get; set; }
        public string Remark { get; set; }
        public string Code { get; set; }
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public string TabIndex { get; set; }
        public string ReturnUrl { get; set; }
    }
}