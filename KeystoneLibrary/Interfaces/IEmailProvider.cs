using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Interfaces
{
    public interface IEmailProvider
    {
        bool SendEmail(string sendToEmail, string sendToName, string senderName, string subject, string messageText);
        bool SendStudentProbationEmails(List<Student> studentInfos);
        bool SendEmailReceivedOnlineAdmission(Student student, AdmissionInformation admissionInformation, string admissionEmail);
    }
}