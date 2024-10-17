namespace KeystoneLibrary.Models
{
    public class FacultyViewModel
    {
        public long FacultyId { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public string Abbreviation { get; set; }
        public List<FacultyDepartment> Departments { get; set; }
        public List<FacultyCurriculum> Curriculums { get; set; }
        public List<FacultyDirector> Directors { get; set; }
        public List<FacultyDirector> ChairMen { get; set; }
    }

    public class FacultyDepartment
    {
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public string Abbreviation { get; set; }
    }

    public class FacultyCurriculum
    {
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
    }

    public class FacultyDirector
    {
        public long FacultyMemberId { get; set; }
        public long FacultyId { get; set; }
        public long InstructorId { get; set; }
        public long? FilterCourseGroupId { get; set; }
        public long? FilterCurriculumVersionGroupId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string FilterCourseGroup { get; set; }
        public string FilterCurriculumVersionGroup { get; set; }
        public string Type { get; set; }
    }
}