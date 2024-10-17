using KeystoneLibrary.Models.DataModels.Scholarship;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models
{
    public class ImportScholarshipStudentViewModel
    {
        public Scholarship Scholarship { get; set; }
        public ScholarshipType ScholarshipType { get; set; }
        public DateTime? ReferenceDate { get; set; }
        public string FileName { get; set; }
        public List<ImportScholarshipStudentSuccessDetail> Success { get; set; }
        public List<ImportScholarshipStudentFailDetail> Fail { get; set; }
        public string ReferenceDateText => ReferenceDate?.ToString(StringFormat.ShortDate);
    }

    public class ImportScholarshipStudentFailDetail
    {
        public string StudentCode { get; set; }
        public string StudentFullName { get; set; }
        public int EffectiveTerm { get; set; }
        public int EffectiveYear { get; set; }
        public int ExpireTerm { get; set; }
        public int ExpireYear { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; }
        public string Comment { get; set; }
        public string AmountText => Amount.ToString(StringFormat.Money);    
    }

    public class ImportScholarshipStudentSuccessDetail
    {
        public string StudentCode { get; set; }
        public string StudentFullName { get; set; }
        public long EffectiveTermId { get; set; }
        public long ExpireTermId { get; set; }
        public string EffectivedTermText { get; set; }
        public string ExpiredTermText { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; }
        public string AmountText => Amount.ToString(StringFormat.Money);       
    }
    
}