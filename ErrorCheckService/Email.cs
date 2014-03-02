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
        private string smtpServer;
        private string from;
        private string[] to;

        public Email()
        {
            smtpServer = ConfigurationManager.AppSettings["smtpServer"];
            from = ConfigurationManager.AppSettings["emailFrom"];
            to = ConfigurationManager.AppSettings["emailTo"].Split(';');
        }

        public Boolean Send(string subject, string body, string attachmentLocation)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(from);
                foreach (var email in to)
                {
                    mail.To.Add(email);
                }
                mail.Subject = subject;
                mail.Body = body;
                if (!string.IsNullOrEmpty(attachmentLocation))
                {
                    Attachment attachment = new Attachment(attachmentLocation);
                    mail.Attachments.Add(attachment);
                }
                using (SmtpClient client = new SmtpClient(smtpServer)) client.Send(mail);
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
