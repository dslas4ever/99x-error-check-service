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

        public Boolean Send(string subject, string body)
        {
            try
            {
                using (SmtpClient client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                }) client.Send(from, to, subject, body);
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
