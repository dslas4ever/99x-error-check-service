using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace ErrorCheckService
{
    class Email
    {
        private string smtpServer;  // "smtp.gmail.com"
        private string smtpUser;    // smtp user
        private string smtpPass;    // smtp pass
        private int smtpPort;       // 587
        private string from;
        private string to;

        Email()
        {
            smtpServer = ConfigurationManager.AppSettings["smtpServer"];
            smtpUser = ConfigurationManager.AppSettings["smtpUser"];
            smtpPass = ConfigurationManager.AppSettings["smtpPass"];
            smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["smtpPort"]);
            from = ConfigurationManager.AppSettings["emailFrom"];
            to = ConfigurationManager.AppSettings["emailTo"];
        }

        public Boolean Send(string subject, string body, string attachmentLocation)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(from);
                mail.To.Add(to);
                mail.Subject = subject;
                mail.Body = body;
                if (!string.IsNullOrEmpty(attachmentLocation))
                {
                    Attachment attachment = new Attachment(attachmentLocation);
                    mail.Attachments.Add(attachment);
                }
                
                using (SmtpClient client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                }) client.Send(mail);
                return true;
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("ErrorCheckService | EmailService", "error occured : " +  e, EventLogEntryType.Error);
                return false;
            }
        }
    }
}
