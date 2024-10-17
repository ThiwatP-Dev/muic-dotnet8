namespace KeystoneLibrary.Models
{
    public class ChangeCurriculumViewModel
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public string Code { get; set; }
        public string ProfileImageURL { get; set; }
        public string FullName { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string Minor { get; set; }
        public string Concentration { get; set; }
        public long CurriculumId { get; set; }
        public long CurriculumVersionId { get; set; }
        public string Curriculum { get; set; }
        public string AdmissionType { get; set; }
        public string Scholarship { get; set; }
        public string GraduatedClass { get; set; }
        public string Program { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string FacultyText 
        {
            get
            {
                return $"{ Faculty } - { Department }";
            }
        }
    }
} 