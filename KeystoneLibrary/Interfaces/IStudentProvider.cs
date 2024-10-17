using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.DataModels.Admission;

namespace KeystoneLibrary.Interfaces
{
    public interface IStudentProvider
    {
        bool IsActiveStudentGraduationInformation(GraduationInformation model);
        bool IsExistStudent(string code, string status = "s");
        bool IsExistStudentCodeAndStatus(string code, string status);
        bool IsExistStudentExceptAdmission(Guid id);
        bool IsExistStudentExceptAdmission(string code);
        bool IsExistAllStudent(string code);
        Student GetStudentById(Guid id);
        Student GetStudentByCode(string code);
        Student GetStudentAndAdmissionStudentByCode(string code);
        Student GetStudentInformationById(Guid id);
        Student GetStudentInformationByCode(string code);
        Student GetStudentInformationByCitizenCard();
        string GetStudentCodeById(Guid Id);
        bool SaveStudentDocumentForAdmission(RegistrationApplicationViewModel model);
        StudentCertificate GetStudentCertificate(long Id);
        string GetNationalitiesByIds(List<long> ids);
        List<RegistrationCourse> GetRegistrationCourseByStudentId(Guid id, long termId);
        List<Student> GetStudentForLatedPayment(long termId);
        GraduationInformation GetStudentGraduationInformation(long id);
        int GetStudyYear(int admissionYear, int graduatedYear, int currentAcademicYear);
        string GetTitleById(long id, string language = "en");
        string GetPronoun(int gender, string language = "en");
        string GetPossessive(int gender, string language = "en");
        StudentRequiredDocument GetStudentRequiredDocument(Student student);
        List<StudentDocument> GetWaitingDocumentByStudentId(string code);
        List<StudentDocument> GetStudentDocument(Guid studentId);
        AcademicInformation GetAcademicInformationByStudentId(Guid studentId);
        StudentProbation GetStudentProbationById(long id);
        List<StudentProbation> GetCurrentStudentProbation(Guid studentId, long currentTermId);
        List<StudentExemptedExamScore> GetStudentExemptedExamScore(Guid studentId);
        bool SaveStudentDocument(StudentRequiredDocument model);
        List<Student> GetStudentFromCodeRanges(int fromCode, int toCode, long academicLevelId, long admissionId);
        bool SaveStudentProfileImage(string studentCode, string imageUrl);
        bool SaveStudentProfileImageByApplicationNumber(string applicationNumber, string imageUrl);
        List<StudentRegistrationCourseViewModel> GetStudentRegistrationCourseViewModel(Guid? studentId = null, string code = null);
        List<StudentRegistrationCourseViewModel> GetStudentRegistrationCourseTranferViewModel(Guid? studentId = null, string code = null);
        List<StudentRegistrationCourseViewModel> GetStudentRegistrationCourseTranferWithGradeViewModel(Guid? studentId = null, string code = null);
        bool IsStudentExtended(Guid? studentId = null, string code = null);
        bool UpdateImageUrl(Guid studentId, string imageUrl);
        CurriculumInformation GetCurrentCurriculum(Guid studentId);
        List<StudentIncidentLog> GetStudentIncidentLogsByStudentId(Guid id);
        List<StudentIncident> GetStudentIncidentsByStudentId(Guid id);
        
        // POST
        string SaveStudentContact(Student student);
        StudentProbationViewModel GetStudentProbation(Criteria criteria);
        bool SaveStudentStatusLog(Guid studentId, long currentTermId, string source, string remark, string status, DateTime? effectiveDate = null);
        DismissStudentViewModel GetDismissTermAndGrade(string code);
        DismissStudent FindDismissStudent(long Id);
        bool UpdateDismissStudent(DismissStudentViewModel model, Student student);
        bool SaveDocumentStudentByDocumentGroup(Guid studentId, long documentGroupId);
        bool UpdateStudentExemptedExamScore(List<StudentExemptedExamScore> scores, Guid studentId);
        string GetLastStudentStatus(Guid id, string status = null);
        Student SaveReenterStudent(Student student, Student model, long termId, string newCode, string type, string reason, out string errorMessage); // type = re, ra
        Document GetDocument(long documentId);
        CheatingStatus GetCheatingStatus(long id);
        bool SaveStudentIncident(CheatingStatus status);
        bool UpdateStudentIncident(CheatingStatus previousModel, CheatingStatus model);
        long GetStudentAcademicLevelIdByCode(string code);
        List<TransferUniversity> GetTransferUniversityByStudentCode(string studentCode);
        StudentResultApiViewModel AddStudents(long academicLevelId, long termId, long admissionRoundId, SaveStudentsViewModel model);
        void UpdateGradeComp();

        //GPA
        StudentGPAViewModel GetGPA(Guid studentId, long termId = 0);
        int GetRegistrationCreditbyStudentId (Guid studentId);
        int GetCreditTransferbyStudentId (Guid studentId);
        void UpdateCGPA(Guid studentId);
        void UpdateTermGrade(Guid studentId, long termId);
        List<Student> GetStudentForAssignAdvisee(Criteria criteria);

        //Resign

        List<ResignStudentViewModel> GetResignStudents(Criteria criteria);
    }
}