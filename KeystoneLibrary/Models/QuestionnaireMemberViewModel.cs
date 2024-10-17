namespace KeystoneLibrary.Models
{
    public class QuestionnaireMemberViewModel
    {
        public long AcademicLevelId { get; set; }
        public long FacultyId { get; set; }
        public long DepartmentId { get; set; }
        public long InstructorTypeId { get; set; }
        public string CodeAndName { get; set; }
        public List<QuestionnaireMemberInstructor> Instructors { get; set; }
        
    }

    public class QuestionnaireMemberInstructor
    {
        public long InstructorId { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string IsChecked { get; set; }
        public string FullName => $"{ Title } { FirstName } { LastName }";
    }
}