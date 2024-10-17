namespace KeystoneLibrary.Models.Report
{
    public class TotalStudentByFeeCodeReportViewModel
    {
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public long FeeItemId { get; set; }
        public int Year { get; set; }
        public int Term { get; set; }
        public string AcademicLevel { get; set; }
        public string FeeCode { get; set; }
        public string FeeNameEn { get; set; }
        public string FeeNameTh { get; set; }
        public int TotalStudent { get; set; }
        public decimal TotalAmount { get; set; }
        public string TotalStudentText => TotalStudent.ToString(StringFormat.NumberString);
        public string TotalAmountText => TotalAmount.ToString(StringFormat.Money);
    }

    public class TotalStudentByFeeCodeDetail
    {
        public string Title { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public string CitizenNumber { get; set; }
        public string FacultyCode { get; set; }
        public string DepartmentCode { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string Gender { get; set; }
        public string Passport { get; set; }
        public string BirthDate { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Nationality { get; set; }
        public string Amount { get; set; }
        public string CreatedDate { get; set; }
    }
}