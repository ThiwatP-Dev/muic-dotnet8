namespace KeystoneLibrary.Models
{
    public class RoomHistoryReportViewModel
    {
        public string Name { get; set; }
        public string Campus { get; set; }
        public string Building { get; set; }
        public int Floor { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string UsingType { get; set; }
        public DateTime Date { get; set; }
        public string ExaminationCourseCode { get; set; }
        public string ExaminationCourseName { get; set; }
        public string ExaminationSectionNumber { get; set; }
        public string ExaminationInstructor { get; set; }
        public string RoomReservationRemark { get; set; }
        public string SectionCourseCode { get; set; }
        public string SectionCourseName { get; set; }
        public string SectionNumber { get; set; }
        public string SectionInstructor { get; set; }
        public bool Cancel { get; set; }
        public bool MakeUp { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedAtText => CreatedAt?.ToString(StringFormat.ShortDate);
        public string CreatedBy { get; set; }
        public string DateText => Date.ToString(StringFormat.ShortDate);
        public string TimeDisplay => $"{ StartTime.ToString(StringFormat.TimeSpan )} - { EndTime.ToString(StringFormat.TimeSpan) }";
        public string Instructor => UsingType == "Studying" ? SectionInstructor 
                                                            : ExaminationInstructor;

        public string CourseSection => string.IsNullOrEmpty(SectionCourseCode) ? "" : $"{ SectionCourseCode } ({ SectionNumber })";
        public string ExaminationCourseSection => string.IsNullOrEmpty(ExaminationCourseCode) ? "" : $"{ ExaminationCourseCode } ({ExaminationSectionNumber})";
        public string Course => UsingType == "Studying" ? CourseSection : ExaminationCourseSection;
        public string CourseAndInstructor => string.IsNullOrEmpty(Course) ? Instructor 
                                                                          : (Course + (string.IsNullOrEmpty(Instructor) ? ""
                                                                                                                       : ", " + Instructor));

        public string Remark => string.IsNullOrEmpty(CourseAndInstructor) ? RoomReservationRemark 
                                                                          : (string.IsNullOrEmpty(RoomReservationRemark) ? "" 
                                                                                                                         : ", " + RoomReservationRemark);
    }
}