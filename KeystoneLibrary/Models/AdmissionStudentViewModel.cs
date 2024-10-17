using KeystoneLibrary.Models.DataModels.Admission;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models
{
    public class AdmissionStudentViewModel
    {
        public Student Student { get; set; } = new Student();
        public AdmissionStudentInformation AdmissionStudentInformation { get; set; } = new AdmissionStudentInformation();
        public StudentRequiredDocument StudentRequiredDocument { get; set; } = new StudentRequiredDocument();
        public bool IsStudentInformationFilled
        {
            get
            {
                var isStudentInformationFilled = Student.Id != Guid.Empty
                                                 && AdmissionStudentInformation.AdmissionInformation.Id != 0
                                                 && Student.AcademicInformation.Id != 0;
                return isStudentInformationFilled;
            }
        }
    }

    public class AdmissionStudentInformation
    {
        public AdmissionInformation AdmissionInformation { get; set; } = new AdmissionInformation();
        public List<StudentExemptedExamScore> StudentExemptedExamScores { get; set; } = new List<StudentExemptedExamScore>();
    }

    public class StudentRequiredDocument
    {
        public Guid StudentId { get; set; }
        public List<StudentDocument> StudentDocuments { get; set; }
    }
}