namespace KeystoneLibrary.Models.Report
{
    public class TotalScholarshipByTermViewModel
    {
        public string Term { get; set; }
        public List<TotalScholarshipByTermDetail> TotalScholarshipByTermDetails { get; set; }
        public int TotalScholarship { get; set; } = 0;
        public int TotalStudent { get; set; } = 0;   
        public decimal TotalScholarshipByTerm { get; set; } = 0;
        public string TotalScholarshipByTermText => TotalScholarshipByTerm.ToString(StringFormat.TwoDecimal);
    }

    public class TotalScholarshipByTermDetail
    {
        public long TermId { get; set; }
        public long ScholarshipTypeId { get; set; }
        public string ScholarshipTypeNameEn { get; set; }
        public string ScholarshipTypeNameTh { get; set; }
        public int TotalScholarship { get; set; }
        public int TotalStudent { get; set; }
        public decimal TotalUsedAmount { get; set; }
        public string TotalUsedAmountText => TotalUsedAmount.ToString(StringFormat.TwoDecimal);
    }

    public class TotalStudentDetailViewModel
    {
        public string Term { get; set; }
        public string ScholarshipTypeNameEn { get; set; }
        public string TotalUsedAmountText { get; set; }
        public List<TotalStudentDetail> TotalStudentDetails { get; set; }
    }

    public class TotalStudentDetail
    {
        public string Code { get; set; }
        public string FullNameEn { get; set; }
        public string FacultyNameEn { get; set; }
        public string DepartmentNameEn { get; set; }
        public string ScholarshipNameEn { get; set; }
        public decimal TotalAmount { get; set; }
        public string TotalAmountText => TotalAmount.ToString(StringFormat.TwoDecimal);
    }
}