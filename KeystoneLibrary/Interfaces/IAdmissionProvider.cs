using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Admission;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Interfaces
{
    public interface IAdmissionProvider
    {
        List<PreviousSchool> GetPreviousSchoolsByCountryId(long id);
        PreviousSchool GetPreviousSchool(long id);
        List<EducationBackground> GetEducationBackgroundsByCountryId(long id);
        AdmissionType GetAdmissionType(long id);
        List<AdmissionRound> GetAdmissionRoundByAcademicLevelId(long id);
        List<AdmissionRound> GetAdmissionRoundByTermId(long termId);
        List<AdmissionRound> GetAdmissionRoundByAcademicLevelIdAndTermId(long academicLevelId, long termId = 0);
        Term GetTermByAdmissionRoundId(long admissionRoundId);
        List<CurriculumVersion> GetCurriculumVersionForAdmissionCurriculum(long termId,long admissionRoundId, long facultyId, long? departmentId = null);
        Student GetStudentInformationById(Guid id);
        Student GetStudentInformationByCode(string code);
        List<AdmissionExamination> GetAdmissionExaminationByFacultyIdAndTestedAt(long facultyId, DateTime? TestedAt);
        StudentExemptedExamScore GetStudentExemptedExams(string type, Guid studentId);
        List<AdmissionDocumentGroup> GetStudentDocumentGroupsByCountryId(long academicLevelId, long? facultyId, long? departmentId, long? previousSchoolCountryId);
        bool IsExistIntensiveCourse(long courseId, long? facultyId, long? departmentId);
        bool IsDuplicateStudentCodeRange(long id, long academicLevelId, int startedCode, int endedCode);
        bool IsExistRangeInAdmissionRound(long id, long academicLevelId, long admissionRoundId);
        bool IsExistCodeCitizenPassportApplicationNumber(string code);
        bool IsExistStudentCode(string code);
        bool IsExistStudentId(Guid studentId);
        ResponseModel CreateOnlineAdmissionApplication(RegistrationApplicationViewModel viewModel);
        ResponseModel UpdateOnlineAdmissionApplication(RegistrationApplicationViewModel viewModel);
        RegistrationApplicationViewModel GetAdmissionInformation(Guid studentId);
        string GenerateStudentCode(long academicLevelId, long admissionRoundId);
        string GetAdmissionFirstClassDate(long admissionRoundId);
        StudentCodeStatusRange GetStudentCodeRange(long admissionRoundId);
        void SetStudentVerificationLetter(List<Guid> studentIds, string referenceNumber, DateTime? sentDate);
        void SetStudentReplyVerificationLetter(List<Guid> studentIds, string receivedNumber, DateTime? receivedDate);
        decimal GetIELTSScore(Guid studentId);
        List<StudentStatisticByProvinceAndSchoolReportViewModel> GetStudentStatisticByProvinceAndSchoolReport(List<Student> students, Criteria criteria);
        bool IsExistAdmissionExamination(long admissionRoundId, long facultyId, List<long?> departmentIds = null);
        bool IsExistAdmissionRound(long id, long termId, int round);
        AdmissionRound GetAdmissionRound(long id);
        bool IsStudnetBlacklisted(string citizenNumber, string passportNumber, string firstNameEn, string lastNameEn, string firstNameTh, string lastNameTh, DateTime birthDate, int gender);
        List<AdmissionCurriculum> GetAdmissionCurriculumByRoundAndFaculty(long admissionRoundId, long facultyId);
        bool IsExistAdmissionCurriculum(long admissionRoundId, long facultyId);
        ApplicationFormViewModel GetApplicationFormViewModel(Student student);
        bool IsStudentBlacklisted(string citizenNumber, string passportNumber, string firstNameEn, string lastNameEn, string firstNameTh, string lastNameTh, DateTime birthDate, int gender);
        List<ExemptedAdmissionExamination> GetExemptedExaminations();
        RegistrationApplicationViewModel GetStudentRequiredDocuments(Guid studentId);
        RegistrationApplicationViewModel GetApplicationStatus(Guid studentId);
        Guid CheckLogin(string username, string password);
        bool CheckIfSameAddress(StudentAddress firstAddress, StudentAddress secondAddress);
        string GetApplicationNumber(Guid studentId);

        // Verification
        int GetVerificationLetterRunningNumber(int year);
        List<VerificationStudent> GetVerificationStudents(VerificationLetter verificationLetter);
        VerificationLetter GetVerificationLetter(long id);
        List<VerificationStudent> GetverificationLetterByVerificationLetterId(long letterId);
    }
}