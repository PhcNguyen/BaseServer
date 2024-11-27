using System;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;

using NServer.Infrastructure.Logging;

namespace NServer.Infrastructure.Services
{
    internal class EmailService(string smtpServer, int smtpPort, string fromAddress, string fromPassword, bool useSsl = true)
    {
        private readonly string _smtpServer = smtpServer;
        private readonly int _smtpPort = smtpPort;
        private readonly string _fromAddress = fromAddress;
        private readonly string _fromPassword = fromPassword;
        private readonly bool _useSsl = useSsl;

        public void SendTextEmail(string toAddress, string subject, string body)
        {
            using var mailMessage = new MailMessage(_fromAddress, toAddress, subject, body);
            SendEmail(mailMessage);
        }

        public void SendHtmlEmail(string toAddress, string subject, string htmlBody)
        {
            using var mailMessage = new MailMessage(_fromAddress, toAddress, subject, htmlBody)
            {
                IsBodyHtml = true
            };
            SendEmail(mailMessage);
        }

        public void SendEmailWithAttachment(string toAddress, string subject, string body, List<string> attachments)
        {
            using var mailMessage = new MailMessage(_fromAddress, toAddress, subject, body);

            foreach (var attachmentPath in attachments)
            {
                var attachment = new Attachment(attachmentPath);
                mailMessage.Attachments.Add(attachment);
            }

            SendEmail(mailMessage);
        }

        private void SendEmail(MailMessage mailMessage)
        {
            try
            {
                using var smtpClient = new SmtpClient(_smtpServer, _smtpPort)
                {
                    Credentials = new NetworkCredential(_fromAddress, _fromPassword),
                    EnableSsl = _useSsl
                };

                smtpClient.Send(mailMessage);
                NLog.Instance.Info("Email sent successfully!");
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Error sending email: {ex.Message}");
            }
        }
    }
}
