namespace KeystoneLibrary.Models
{
    public class SearchInstructorScheduleViewModel
    {
        private string _academicLevel;
        private string _faculty;
        private string _department;
        public long Id { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public string AcademicLevel 
        { 
            get
            {
                if (String.IsNullOrEmpty(_academicLevel))
                {
                    return "N/A";
                }
                else
                {
                    return _academicLevel;
                }   
            }
            set
            {
                _academicLevel = value;
            }
        }
        public string Faculty 
        {
            get
            {
                if (String.IsNullOrEmpty(_faculty))
                {
                    return "N/A";
                }
                else
                {
                    return _faculty;
                }
            }
            set
            {
                _faculty = value;
            }
        }
        public string Department
        {
            get
            {
                if (String.IsNullOrEmpty(_department))
                {
                    return "N/A";
                }
                else
                {
                    return _department;
                }
            }
            set 
            {
                _department = value;
            }
        }
    }
    
    public class InstructorScheduleViewModel
    {
        public long TermId { get; set; } 
        public long FacultyId { get; set; }
        public string Term { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public List<ScheduleViewModel> Schedules { get; set; }
    }
}