using System.Net;
using System.Net.Mail;
using EmailHelper.Models;

namespace EmailHelper.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;
        public EmailService(
            EmailOptions emailOptions
        )
        {
            _emailOptions = emailOptions;
        }

        public async Task SendMailAsync(string subject, string body, List<string> toEmails, List<string>? ccEmails = null, List<EmailAttachment>? attachments = null)
        {
            await HandleSendMailAsync(subject, body, toEmails, ccEmails, attachments);
        }

        private async Task HandleSendMailAsync(string subject, string body, List<string> toEmails, List<string>? ccEmails = null, List<EmailAttachment>? attachments = null)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_emailOptions.MailFrom ?? string.Empty, _emailOptions.SenderName)
            };

            //to emails
            foreach (var email in toEmails.Where(email => !string.IsNullOrEmpty(email)))
            {
                message.To.Add(email);
            }

            //cc emails
            if (ccEmails?.Count > 0)
            {
                foreach (var email in ccEmails.Where(email => !toEmails.Contains(email) && !string.IsNullOrEmpty(email)))
                {
                    message.CC.Add(email);
                }
            }

            //attachment
            if (attachments?.Count > 0)
            {
                foreach (var attachment in attachments)
                {
                    message.Attachments.Add(new Attachment(new MemoryStream(attachment.Attachment), attachment.FileName));
                }
            }

            message.IsBodyHtml = true;
            if (_emailOptions.Environment != "Production")
            {
                message.Subject = $"[{_emailOptions.Environment}] " + subject;
            }
            else
            {
                message.Subject = subject;
            }

            message.Body = body;

            using var client = new SmtpClient(_emailOptions.SmtpServer, _emailOptions.SmtpPort);
            client.Credentials = new NetworkCredential(_emailOptions.SmtpUser, _emailOptions.SmtpPassword);
            client.EnableSsl = true;
            await client.SendMailAsync(message);
        }
    }
}