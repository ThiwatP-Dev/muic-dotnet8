using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.Report;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Interfaces
{
    public interface IReportProvider
    {
        List<PrintingLog> GetPrintingLogs(int year);
        int GetNewRunningNumber(int year);
        TranscriptInformation GetTranscript(Student student, string language, bool isPreview);
        TranscriptInformation MapTranscriptPreview(TranscriptInformation transcript, TranscriptViewModel model);
        Student GetStudentInformationForTranscript(string studentCode, Criteria criteria);
        string GetNewReferenceNumber(int year, string language = "en");
        int GetYear(int year, string language = "en");
        string GetSignatoryPositionById(long id, string language = "en");
        string GetSignatoryNameById(long id, string language = "en");
        List<string> GetSignatoriesByIds(List<long> ids, string language);
        List<string> GetPositionsBySignatoryIds(List<long> ids, string language);
        string GetCountryById(long id, string language = "en");
        string GetChangeNameType(string type, string language = "en");
        
        // Student
        ApplicantsByAdmissionRoundViewModel GetApplicantsByAdmissionRounds(Criteria criteria);
        CurriculumVersionReportViewModel GetCurriculumVersionReport(long curriculumVersionId);
    }
}