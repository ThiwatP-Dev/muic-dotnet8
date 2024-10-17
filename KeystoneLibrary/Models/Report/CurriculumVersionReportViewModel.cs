using KeystoneLibrary.Models.DataModels.Curriculums;

namespace KeystoneLibrary.Models.Report
{
    public class CurriculumVersionReportViewModel
    {
        public long AcademicLevelId { get; set; }
        public long CurriculumId { get; set; }
        public long CurriculumVersionId { get; set; }
        public string CurriculumName { get; set; }
        public string ReferenceCode { get; set; }
        public string AcademicLevel { get; set; }
        public string AbbreviationEn { get; set; }
        public string AbbreviationTh { get; set; }
        public string FacultyName { get; set; }
        public string DepartmentName { get; set; }
        public string TermTypeText { get; set; }
        public string MinimumGPAText { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionTh { get; set; }
        public string CurriculumVersionCode { get; set; }
        public string CurriculumVersionNameEn { get; set; }
        public string CurriculumVersionNameTh { get; set; }
        public string DegreeNameEn { get; set; }
        public string DegreeNameTh { get; set; }
        public string DegreeAbbreviationEn { get; set; }
        public string DegreeAbbreviationTh { get; set; }
        public string ImplementedTerm { get; set; }
        public string OpenedTerm { get; set; }
        public string ClosedTerm { get; set; }
        public int MinimumTerm { get; set; }
        public int MaximumTerm { get; set; }
        public string AcademicProgramName { get; set; }
        public string ApprovedDateText { get; set; }
        public string Remark { get; set; }
        public int TotalCredit { get; set; }
        public List<CourseGroup> CourseGroups { get; set; }
        
    }
}