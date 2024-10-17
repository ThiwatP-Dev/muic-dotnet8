using KeystoneLibrary.Models.DataModels;
namespace KeystoneLibrary.Models
{
    public class RegistrationConditionViewModel
    {
        public Criteria Criteria { get; set; }

        public List<RegistrationCondition> RegistrationConditions { get; set; }
    }

    public class RegistrationConditionDetailViewModel
    { 
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Students { get; set; }
        public string StudentCodes { get; set; }
        public string StudentCodesText { get; set; }
        public long? AcademicLevelId { get; set; }
        public long? AcademicProgramId { get; set; }
        public string AcademicProgramName { get; set; }
        public long? FacultyId { get; set; }
        public string FacultyName { get; set; }
        public long? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int BatchCodeStart { get; set; }
        public int BatchCodeEnd { get; set; }
        public int LastDigitStart { get; set; }
        public int LastDigitEnd { get; set; }
        public bool IsGraduating { get; set; }
        public bool IsAthlete { get; set; }
        public bool IsActive { get; set; }
    }
}