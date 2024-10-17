using KeystoneLibrary.Models.DataModels.Logs;

namespace KeystoneLibrary.Models
{
    public class ChangedNameLogViewModel
    {
        public string StudentCode { get; set; }
        public ChangedNameLog ChangedNameDetail { get; set; }
    }

    public class ChangedNameViewModel
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public string OldFirstNameEn { get; set; }
        public string OldLastNameEn { get; set; }
        public string OldFirstNameTh { get; set; }
        public string OldLastNameTh { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string CurriculumVersion { get; set; }
        public int GradClass { get; set; }
        public string GradClassText => GradClass == 0 ? "" : GradClass.ToString();
        public string GraduatedDate { get; set; }
        public List<ChangedNameLog> StudentChangedNameLogs { get; set; }
        public int RunningNumber { get; set; }
        public int Year { get; set; }
        public string ChangedAtText { get; set; }
        public string ReferenceNumber { get; set; }
        public string DistrictRegistrationOffice { get; set; }
        public string DistrictRegistrationAtText { get; set; }
        public string Remark { get; set; }
        public string NewFirstNameEn { get; set; }
        public string NewLastNameEn { get; set; }
        public string NewFirstNameTh { get; set; }
        public string NewLastNameTh { get; set; }
        public string ChangedType { get; set; }
        public string ChangedFlag { get; set; }
        public string DocumentUrl { get; set; }
        public IFormFile UploadFile { get; set; }
        public string RequestedAt { get; set; }
        public string ChangedBy { get; set; }
    }
}