using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Interfaces;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace KeystoneLibrary.Providers
{
    public class EmailProvider : IEmailProvider
    {
        private string SMTP_SERVER = "email-smtp.us-west-2.amazonaws.com";
        private int PORT = 587;
        private string USERNAME = "AKIAWLPPPAGXGNM7GG4R";
        private string PASSWORD = "BDK5JWBZaeOJ2nH1I9xLvMnQuNDsvBXC7GWVaAKpRBAG";
        private string SENDER_EMAIL = "no-reply@muic.io";

        public bool SendEmail(string sendToEmail, string sendToName, string senderName, string subject, string messageText)
        {
            try 
            {
                using (var client = new SmtpClient()) 
                {
                    client.Connect(SMTP_SERVER, PORT, SecureSocketOptions.StartTls);
                    client.Authenticate(USERNAME, PASSWORD);
                    
                    var emailMessage = new MimeMessage();
                    emailMessage.From.Add(new MailboxAddress(senderName, SENDER_EMAIL));
                    emailMessage.To.Add(new MailboxAddress(sendToName, sendToEmail));
                    emailMessage.Subject = subject;
                    emailMessage.Body = new TextPart("plain") 
                    {
                        Text = messageText
                    };
                    client.Send(emailMessage);
                    client.Disconnect(true);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendStudentProbationEmails(List<Student> studentInfos) 
        {
            try 
            {
                using (var client = new SmtpClient()) 
                {
                    client.Connect(SMTP_SERVER, PORT, SecureSocketOptions.StartTls);
                    client.Authenticate(USERNAME, PASSWORD);

                    foreach(var student in studentInfos) 
                    {
                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress("MUIC", SENDER_EMAIL));
                        message.To.Add(new MailboxAddress(student.FullNameEn, student.Email));
                        message.Subject = "Test email subject to " + student.FullNameEn;
                        message.Body = new TextPart("plain") {
                            Text = $"Test email body to {student.FullNameEn}"
                        };
                        client.Send(message);
                    }
                    
                    client.Disconnect(true);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool SendEmailReceivedOnlineAdmission(Student student, AdmissionInformation admissionInformation, string admissionEmail)
        {
            try 
            {
                using (var client = new SmtpClient()) 
                {
                    client.Connect(SMTP_SERVER, PORT, SecureSocketOptions.StartTls);
                    client.Authenticate(USERNAME, PASSWORD);
                    
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("MUIC", SENDER_EMAIL));
                    message.To.Add(new MailboxAddress(student.FullNameEn, student.PersonalEmail));
                    message.Subject = "Online application submitted.";
                    message.Body = new TextPart("plain") {
                    Text = $"Hello {student.FullNameEn}, \n\n" +
                    $"Your online application has been created. Your login information is as follows:\n" + 
                    $"Application number: {admissionInformation.ApplicationNumber}\nPassword: {admissionInformation.Password}\n" +
                    $"Please use this Application Number and Password to login to the system."
                    };
                    client.Send(message);
                    client.Disconnect(true);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}